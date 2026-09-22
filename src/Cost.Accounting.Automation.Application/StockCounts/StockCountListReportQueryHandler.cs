using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.StockCounts;

internal sealed class StockCountListReportQueryHandler(
    IProductRepository productRepository,
    IProductMovementRepository productMovementRepository,
    IStockIssueRepository stockIssueRepository)
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

        // Atölye transferi, depodan çıkışın yanı sıra "Atölye Transferi Girişi"
        // adında bir giriş hareketi de üretir. Bu giriş hareketi atölye stokunu
        // temsil eder ve depo bakiyesine girmemelidir; aksi halde transfer edilen
        // miktar depodan hiç düşmemiş gibi görünür.
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