using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

[Permission("stock_issue:view")]
public sealed record AtelierTransferProductsQuery(Guid TargetAccountId) : IRequest<Result<List<AtelierTransferProductDto>>>;

public sealed class AtelierTransferProductDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string UnitTypeName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal TransferredQuantity { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public decimal DraftQuantity { get; set; }

    public decimal AvailableQuantity => Math.Max(0m, TransferredQuantity - ConsumedQuantity);
}

internal sealed class AtelierTransferProductsQueryHandler(
    IStockIssueRepository stockIssueRepository,
    ICostSlipRepository costSlipRepository) : IRequestHandler<AtelierTransferProductsQuery, Result<List<AtelierTransferProductDto>>>
{
    public async Task<Result<List<AtelierTransferProductDto>>> Handle(
        AtelierTransferProductsQuery request,
        CancellationToken cancellationToken)
    {
        List<StockIssue> transfers = await stockIssueRepository.GetAll()
            .Where(i => i.IssueType == StockIssueType.AtelierTransfer && !i.IsDeleted)
            .Where(i => i.TargetAccountId == new IdentityId(request.TargetAccountId))
            .Include(i => i.Lines).ThenInclude(l => l.Product!).ThenInclude(p => p.ProductUnitType)
            .OrderBy(i => i.Date)
            .ThenBy(i => i.DocumentNumber)
            .ToListAsync(cancellationToken);

        HashSet<Guid> productIds = transfers
            .SelectMany(t => t.Lines)
            .Where(l => l.Product is not null)
            .Select(l => l.ProductId.Value)
            .ToHashSet();

        Dictionary<Guid, decimal> consumedMap = [];

        List<CostSlip> slips = productIds.Count > 0
            ? await costSlipRepository.GetAll()
                .Where(s => s.WorkshopId == new IdentityId(request.TargetAccountId)
                    && !s.IsDeleted)
                .Include(s => s.CostSlipItems)
                .ToListAsync(cancellationToken)
            : [];

        Dictionary<Guid, decimal> draftMap = [];
        if (slips.Count > 0)
        {
            var groupedSlips = slips.GroupBy(s => s.Status);
            List<CostSlip> approvedSlips = groupedSlips
                .FirstOrDefault(g => g.Key == CostSlipStatus.Approved)?
                .ToList() ?? [];

            consumedMap = approvedSlips
                .SelectMany(s => s.CostSlipItems)
                .Where(i => i.ProductId is not null)
                .GroupBy(i => i.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            List<CostSlip> draftSlips = groupedSlips
                .FirstOrDefault(g => g.Key == CostSlipStatus.Draft)?
                .ToList() ?? [];

            draftMap = draftSlips
                .SelectMany(s => s.CostSlipItems)
                .Where(i => i.ProductId is not null)
                .GroupBy(i => i.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
        }

        var result = transfers
            .SelectMany(t => t.Lines)
            .Where(l => l.Product is not null)
            .GroupBy(l => l.ProductId)
            .Select(g =>
            {
                Product product = g.First().Product!;
                StockIssueLine latest = g.Last();

                return new AtelierTransferProductDto
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode.Value,
                    ProductName = product.Name.Value,
                    UnitTypeName = product.ProductUnitType?.Name.Value ?? string.Empty,
                    UnitPrice = latest.UnitCost.Value,
                    TransferredQuantity = g.Sum(l => l.Quantity),
                    ConsumedQuantity = consumedMap.TryGetValue(product.Id.Value, out decimal consumed) ? consumed : 0m,
                    DraftQuantity = draftMap.TryGetValue(product.Id.Value, out decimal drafted) ? drafted : 0m
                };
            })
            .OrderBy(p => p.ProductCode)
            .ThenBy(p => p.ProductName)
            .ToList();

        return result;
    }
}