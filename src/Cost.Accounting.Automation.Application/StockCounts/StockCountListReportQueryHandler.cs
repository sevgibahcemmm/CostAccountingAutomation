using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.StockCounts;

internal sealed class StockCountListReportQueryHandler(
    IProductRepository productRepository,
    IProductMovementRepository productMovementRepository,
    IStockIssueRepository stockIssueRepository,
    ICostSlipRepository costSlipRepository,
    IChartOfAccountRepository chartOfAccountRepository)
    : IRequestHandler<StockCountListReportQuery, List<StockCountReportRowDto>>
{
    public async Task<List<StockCountReportRowDto>> Handle(
        StockCountListReportQuery request,
        CancellationToken cancellationToken)
    {
        return request.GroupMode == StockCountGroupMode.Warehouse
            ? await BuildByWarehouseAsync(request, cancellationToken)
            : await BuildByWorkshopAsync(request, cancellationToken);
    }

    private async Task<List<StockCountReportRowDto>> BuildByWarehouseAsync(
        StockCountListReportQuery request,
        CancellationToken cancellationToken)
    {
        List<Product> products = await productRepository.GetAll()
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive)
            .Include(p => p.Warehouse)
            .Include(p => p.ProductUnitType)
            .ToListAsync(cancellationToken);

        if (!request.AllGroups && request.GroupIds is { Count: > 0 })
        {
            HashSet<Guid> selected = request.GroupIds.ToHashSet();
            products = products
                .Where(p => selected.Contains(p.WarehouseId))
                .ToList();
        }

        Dictionary<IdentityId, decimal> balanceMap = await LoadBalancesAsync(
            products.Select(p => p.Id).ToHashSet(),
            request.AsOfDate,
            cancellationToken);

        List<StockCountReportRowDto> rows = products
            .Select(p => new StockCountReportRowDto
            {
                ProductId = p.Id,
                ProductCode = p.ProductCode.Value,
                ProductName = p.Name.Value,
                UnitTypeName = p.ProductUnitType?.Name.Value ?? string.Empty,
                SystemQuantity = balanceMap.GetValueOrDefault(p.Id),
                GroupId = p.WarehouseId,
                GroupName = p.Warehouse?.Name.Value ?? "Belirtilmemiş Depo"
            })
            .Where(r => r.SystemQuantity > 0)
            .OrderBy(r => r.GroupName)
            .ThenBy(r => r.ProductCode)
            .ThenBy(r => r.ProductName)
            .ToList();

        rows = AssignRowNumbers(rows);
        return rows;
    }

    private async Task<List<StockCountReportRowDto>> BuildByWorkshopAsync(
        StockCountListReportQuery request,
        CancellationToken cancellationToken)
    {
        List<StockIssue> transfers = await stockIssueRepository.GetAll()
            .AsNoTracking()
            .Where(i => i.IssueType == StockIssueType.AtelierTransfer && !i.IsDeleted && i.Date <= request.AsOfDate)
            .Include(i => i.Lines)
            .Include(i => i.TargetAccount)
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.DocumentNumber)
            .ToListAsync(cancellationToken);

        Dictionary<(IdentityId ProductId, IdentityId TargetId), decimal> balances = [];
        foreach (StockIssue issue in transfers)
        {
            IdentityId targetId = issue.TargetAccountId;
            foreach (StockIssueLine line in issue.Lines)
            {
                balances.TryGetValue((line.ProductId, targetId), out decimal quantity);
                balances[(line.ProductId, targetId)] = quantity + line.Quantity;
            }
        }

        await AddAtelierProductionsAsync(balances, request.AsOfDate, cancellationToken);
        await SubtractAtelierConsumptionsAsync(balances, request.AsOfDate, cancellationToken);

        if (!request.AllGroups && request.GroupIds is { Count: > 0 })
        {
            HashSet<Guid> selected = request.GroupIds.ToHashSet();
            balances = balances
                .Where(kv => selected.Contains(kv.Key.TargetId.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        (balances, Dictionary<IdentityId, string> accountNames) =
            await KeepWorkshopAccountsAsync(balances, cancellationToken);

        if (balances.Count == 0)
        {
            return [];
        }

        HashSet<IdentityId> productIds = balances.Keys
            .Select(k => k.ProductId)
            .ToHashSet();

        List<Product> products = await productRepository.GetAll()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .Include(p => p.ProductUnitType)
            .Include(p => p.Warehouse)
            .ToListAsync(cancellationToken);

        // Atölye raporu atölyede fiziksel olarak bulunan ürünleri gösterir. Mamuller
        // (152) üretim sonrası depoya çıkar; üretim pusulası onayı bakiyeye eklediği
        // için 152'li satırlar atölye listelerine sızıyordu. Yarı mamul (151) ve
        // malzeme depoları (150 / 150.98) atölyede kaldığı için listede tutulur.
        products = products
            .Where(p => p.Warehouse is null
                || !p.Warehouse.Code.Value.StartsWith(FinishedGoodsWarehouseRoot, StringComparison.Ordinal))
            .ToList();

        Dictionary<IdentityId, Product> productMap = products.ToDictionary(p => p.Id);

        // Grup adı önce hesap planından gelir; yalnızca atölye üretim/tüketiminden gelen
        // (transferi olmayan) atölyelerde listede görünen ad yedek olarak kullanılır.
        Dictionary<IdentityId, string> targetNameMap = new(accountNames);
        foreach (IGrouping<IdentityId, StockIssue> group in transfers
                     .Where(t => t.TargetAccount is not null)
                     .GroupBy(t => t.TargetAccountId))
        {
            targetNameMap.TryAdd(group.Key, group.First().TargetAccount!.Name.Value);
        }

        List<StockCountReportRowDto> rows = balances
            .Where(kv => productMap.ContainsKey(kv.Key.ProductId))
            .Select(kv =>
            {
                Product product = productMap[kv.Key.ProductId];
                return new StockCountReportRowDto
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode.Value,
                    ProductName = product.Name.Value,
                    UnitTypeName = product.ProductUnitType?.Name.Value ?? string.Empty,
                    SystemQuantity = kv.Value,
                    GroupId = kv.Key.TargetId.Value,
                    GroupName = targetNameMap.GetValueOrDefault(kv.Key.TargetId, "Belirtilmemiş Atölye")
                };
            })
            .Where(r => r.SystemQuantity > 0)
            .OrderBy(r => r.GroupName)
            .ThenBy(r => r.ProductCode)
            .ThenBy(r => r.ProductName)
            .ToList();

        rows = AssignRowNumbers(rows);
        return rows;
    }

    /// <summary>
    /// Atölye bazında raporda gruplama yalnızca 150.55 kökü altındaki atölye hesapları
    /// (yapraklar) üzerinden yapılır. Depo kökleri (150 / 150.98 / 151 / 152) ve tüketim
    /// birimi hesapları atölye bakiyesi taşımadığı için gruba dönüşmez; aksi halde "tüm
    /// atölyeler" seçiliyken 152 satırları da listeye sızıyordu.
    /// </summary>
    private const string WorkshopCodeRoot = "150.55";

    /// <summary>Mamuller (bitmiş ürün) deposu kökü; atölye raporunda hariç tutulur.</summary>
    private const string FinishedGoodsWarehouseRoot = "152";

    private async Task<(
        Dictionary<(IdentityId ProductId, IdentityId TargetId), decimal> Balances,
        Dictionary<IdentityId, string> Names)> KeepWorkshopAccountsAsync(
        Dictionary<(IdentityId ProductId, IdentityId TargetId), decimal> balances,
        CancellationToken cancellationToken)
    {
        // IdentityId üzerinden karşılaştırma; Id.Value ile LINQ sorgusu SQL'e çevrilemiyor.
        HashSet<IdentityId> groupIds = balances.Keys.Select(k => k.TargetId).ToHashSet();
        if (groupIds.Count == 0)
        {
            return (balances, []);
        }

        List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAll()
            .AsNoTracking()
            .Where(a => groupIds.Contains(a.Id) && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        List<ChartOfAccount> workshops = accounts
            .Where(a => a.Code.Value.StartsWith(WorkshopCodeRoot, StringComparison.Ordinal))
            .ToList();

        if (workshops.Count == 0)
        {
            return ([], []);
        }

        Dictionary<IdentityId, string> names = workshops.ToDictionary(a => a.Id, a => a.Name.Value);
        HashSet<IdentityId> workshopIds = names.Keys.ToHashSet();

        return (balances
            .Where(kv => workshopIds.Contains(kv.Key.TargetId))
            .ToDictionary(kv => kv.Key, kv => kv.Value), names);
    }

    private async Task<Dictionary<IdentityId, decimal>> LoadBalancesAsync(
        HashSet<IdentityId> productIds,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return [];
        }

        // Ürün bazlı bakiye tüm hareketlerin netiyle hesaplanır:
        // bakiye = tüm girişler (+) - tüm çıkışlar (-). Atölye transferi
        // giriş/çıkış ikilisi net sıfır etki yapar, MKP tüketimi ise gerçek
        // çıkıştır; böylece üretilip tüketilen yarımamülün bakiyesi sıfır olur.
        List<ProductMovement> movements = await productMovementRepository.GetAll()
            .AsNoTracking()
            .WhereCountsAsProductStock()
            .Where(m => productIds.Contains(m.ProductId) && !m.IsDeleted && m.Date <= asOfDate)
            .ToListAsync(cancellationToken);

        return movements
            .GroupBy(m => m.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(m => m.MovementType == ProductMovementType.Input ? m.Quantity : -m.Quantity));
    }

    private async Task AddAtelierProductionsAsync(
        Dictionary<(IdentityId ProductId, IdentityId TargetId), decimal> balances,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        // Maliyet pusulası onayı üretilen mamul/yarı mamül için
        // "Maliyet Pusulası Girişi" (+) hareketi üretir. Üretilen ürün fiziksel
        // olarak atölyede bulunduğundan atölye bakiyesine eklenmelidir; aksi
        // halde üretilen yarımamülün bakiyesi tüketim kadar negatif görünür.
        List<CostSlip> approvedSlips = await costSlipRepository.GetAll()
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Status == CostSlipStatus.Approved && s.CostDate <= asOfDate)
            .Where(s => s.ProducedProductId != null)
            .ToListAsync(cancellationToken);

        foreach (CostSlip slip in approvedSlips)
        {
            if (slip.ProducedProductId is not { } producedProductId)
            {
                continue;
            }

            IdentityId workshopId = slip.WorkshopId;

            balances.TryGetValue((new IdentityId(producedProductId.Value), workshopId), out decimal quantity);
            balances[(new IdentityId(producedProductId.Value), workshopId)] = quantity + slip.Quantity;
        }
    }

    private async Task SubtractAtelierConsumptionsAsync(
        Dictionary<(IdentityId ProductId, IdentityId TargetId), decimal> balances,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        if (balances.Count == 0)
        {
            return;
        }

        // Maliyet pusulası onayı atölyedeki stoktan MKP tüketimi (-) çıkışı
        // üretir. Atölye bakiyesi = transfer girişleri + üretim girişleri -
        // atölye tüketimleri olmalıdır. Tüketim burada düşülür.
        List<CostSlip> approvedSlips = await costSlipRepository.GetAll()
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Status == CostSlipStatus.Approved && s.CostDate <= asOfDate)
            .Include(s => s.CostSlipItems)
            .ToListAsync(cancellationToken);

        foreach (CostSlip slip in approvedSlips)
        {
            IdentityId workshopId = slip.WorkshopId;

            foreach (CostSlipItem item in slip.CostSlipItems)
            {
                if (item.ProductId is null)
                {
                    continue;
                }

                balances.TryGetValue((new IdentityId(item.ProductId.Value), workshopId), out decimal quantity);
                balances[(new IdentityId(item.ProductId.Value), workshopId)] = quantity - item.Quantity;
            }
        }
    }

    private static List<StockCountReportRowDto> AssignRowNumbers(List<StockCountReportRowDto> rows)
    {
        Dictionary<Guid, decimal> groupTotals = rows
            .GroupBy(r => r.GroupId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.SystemQuantity));

        decimal grandTotal = rows.Sum(r => r.SystemQuantity);

        int index = 0;
        foreach (IGrouping<Guid, StockCountReportRowDto> group in rows.GroupBy(r => r.GroupId))
        {
            foreach (StockCountReportRowDto row in group)
            {
                row.RowNumber = ++index;
                row.GroupTotalQuantity = groupTotals.GetValueOrDefault(row.GroupId);
                row.GrandTotalQuantity = grandTotal;
            }
        }

        return rows;
    }
}