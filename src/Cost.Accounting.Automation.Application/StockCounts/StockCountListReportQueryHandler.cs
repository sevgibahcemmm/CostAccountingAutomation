using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
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
    ICostSlipRepository costSlipRepository)
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
            .ToListAsync(cancellationToken);

        Dictionary<IdentityId, Product> productMap = products.ToDictionary(p => p.Id);

        Dictionary<IdentityId, string> targetNameMap = transfers
            .Where(t => t.TargetAccount is not null)
            .GroupBy(t => t.TargetAccountId)
            .ToDictionary(g => g.Key, g => g.First().TargetAccount!.Name.Value);

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