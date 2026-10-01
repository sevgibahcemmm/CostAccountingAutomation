using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

/// <summary>
/// Atölye Stok Raporu: hareket görmüş her atölye için özet üst satır ve o atölyeye
/// transfer edilen tüm ürünlerin (giren / tüketilen / bakiye) detayını döndürür.
/// </summary>
[Permission("stock_issue:view")]
public sealed record AtelierTransferStockQuery : IRequest<Result<List<AtelierTransferWorkshopDto>>>;

public sealed class AtelierTransferWorkshopDto
{
    public Guid WorkshopId { get; set; }
    public string WorkshopCode { get; set; } = default!;
    public string WorkshopName { get; set; } = default!;
    public int ProductCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateOnly LastTransferDate { get; set; }
    public List<AtelierTransferWorkshopProductDto> Products { get; set; } = [];
}

public sealed class AtelierTransferWorkshopProductDto
{
    public string WorkshopCode { get; set; } = default!;
    public string WorkshopName { get; set; } = default!;
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string UnitTypeName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal TransferredQuantity { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public decimal DraftQuantity { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal AvailableQuantity => Math.Max(0m, TransferredQuantity - ConsumedQuantity);
}

internal sealed class AtelierTransferStockQueryHandler(
    IStockIssueRepository stockIssueRepository,
    ICostSlipRepository costSlipRepository) : IRequestHandler<AtelierTransferStockQuery, Result<List<AtelierTransferWorkshopDto>>>
{
    public async Task<Result<List<AtelierTransferWorkshopDto>>> Handle(AtelierTransferStockQuery request, CancellationToken cancellationToken)
    {
        List<StockIssue> transfers = await stockIssueRepository.GetAll()
            .Where(i => i.IssueType == StockIssueType.AtelierTransfer && !i.IsDeleted)
            .Where(i => i.TargetAccount != null)
            .Include(i => i.Lines).ThenInclude(l => l.Product!).ThenInclude(p => p.ProductUnitType)
            .Include(i => i.TargetAccount)
            .OrderBy(i => i.Date)
            .ThenBy(i => i.DocumentNumber)
            .ToListAsync(cancellationToken);

        List<IdentityId> productIds = transfers
            .SelectMany(t => t.Lines)
            .Where(l => l.Product != null)
            .Select(l => l.ProductId)
            .Distinct()
            .ToList();

        List<IdentityId> workshopIds = transfers
            .Select(t => t.TargetAccountId)
            .Distinct()
            .ToList();

        List<GroupedSlipItem> slipItems = productIds.Count == 0 || workshopIds.Count == 0
            ? []
            : await GetSlipItemsAsync(workshopIds, productIds, cancellationToken);

        Dictionary<(Guid WorkshopId, Guid ProductId), decimal> consumedMap = slipItems
            .Where(x => x.Status == CostSlipStatus.Approved)
            .GroupBy(x => (x.WorkshopId, x.ProductId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        Dictionary<(Guid WorkshopId, Guid ProductId), decimal> draftMap = slipItems
            .Where(x => x.Status == CostSlipStatus.Draft)
            .GroupBy(x => (x.WorkshopId, x.ProductId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        // Bekleyen miktarın ikinci bileşeni: ONAYLANMAMIŞ transferler. Taslak
        // transfer stok hareketi üretmediği için TransferredQuantity'ye
        // sayılmaz; yalnızca "Taslak (Bekleyen)" olarak görünür.
        foreach (StockIssue transfer in transfers.Where(t => t.Status == StockIssueStatus.Draft))
        {
            foreach (StockIssueLine line in transfer.Lines)
            {
                (Guid, Guid) key = (transfer.TargetAccountId.Value, line.ProductId.Value);
                draftMap.TryGetValue(key, out decimal current);
                draftMap[key] = current + line.Quantity;
            }
        }

        // Gerçek stok yalnızca ONAYLI transferlerden gelir.
        List<StockIssue> approvedTransfers = transfers
            .Where(t => t.Status == StockIssueStatus.Approved)
            .ToList();

        List<AtelierTransferWorkshopDto> result = [];

        foreach (IGrouping<IdentityId, StockIssue> group in approvedTransfers.GroupBy(t => t.TargetAccountId))
        {
            ChartOfAccount target = group.First().TargetAccount!;
            Guid workshopId = group.Key.Value;

            List<AtelierTransferWorkshopProductDto> products = group
                .SelectMany(t => t.Lines)
                .Where(l => l.Product != null)
                .GroupBy(l => l.ProductId)
                .Select(lines =>
                {
                    Product product = lines.First().Product!;
                    decimal transferred = lines.Sum(l => l.Quantity);

                    (Guid, Guid) key = (workshopId, product.Id.Value);

                    return new AtelierTransferWorkshopProductDto
                    {
                        ProductId = product.Id,
                        ProductCode = product.ProductCode.Value,
                        ProductName = product.Name.Value,
                        UnitTypeName = product.ProductUnitType?.Name.Value ?? string.Empty,
                        UnitPrice = lines.Last().UnitCost.Value,
                        TransferredQuantity = transferred,
                        ConsumedQuantity = consumedMap.TryGetValue(key, out decimal consumed) ? consumed : 0m,
                        DraftQuantity = draftMap.TryGetValue(key, out decimal drafted) ? drafted : 0m,
                        TotalAmount = lines.Sum(l => l.Quantity * l.UnitCost.Value)
                    };
                })
                .OrderBy(p => p.ProductCode)
                .ThenBy(p => p.ProductName)
                .ToList();

            result.Add(new AtelierTransferWorkshopDto
            {
                WorkshopId = workshopId,
                WorkshopCode = target.Code.Value,
                WorkshopName = target.Name.Value,
                ProductCount = products.Count,
                TotalQuantity = group.SelectMany(t => t.Lines).Where(l => l.Product != null).Sum(l => l.Quantity),
                TotalAmount = group.SelectMany(t => t.Lines).Where(l => l.Product != null).Sum(l => l.Quantity * l.UnitCost.Value),
                LastTransferDate = group.Max(t => t.Date),
                Products = products
            });
        }

        return result
            .OrderBy(w => w.WorkshopCode)
            .ThenBy(w => w.WorkshopName)
            .ToList();
    }

    private async Task<List<GroupedSlipItem>> GetSlipItemsAsync(
        List<IdentityId> workshopIds,
        List<IdentityId> productIds,
        CancellationToken cancellationToken)
    {
        List<IdentityId?> workshopKeySet = workshopIds.Select(w => (IdentityId?)w).ToList();
        List<IdentityId?> productKeySet = productIds.Select(p => (IdentityId?)p).ToList();

        List<CostSlip> slips = await costSlipRepository.GetAll()
            .Where(s => !s.IsDeleted
                && s.WorkshopId != null
                && workshopKeySet.Contains(s.WorkshopId)
                && s.CostSlipItems.Any(i => i.ProductId != null && productKeySet.Contains(i.ProductId)))
            .Include(s => s.CostSlipItems)
            .ToListAsync(cancellationToken);

        return slips
            .SelectMany(s => s.CostSlipItems
                .Where(i => i.ProductId != null)
                .Select(i => new GroupedSlipItem
                {
                    WorkshopId = s.WorkshopId!.Value,
                    ProductId = i.ProductId!.Value,
                    Quantity = i.Quantity,
                    Status = s.Status
                }))
            .ToList();
    }

    private sealed class GroupedSlipItem
    {
        public Guid WorkshopId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public CostSlipStatus Status { get; set; }
    }
}