using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

[Permission("stock_issue:view")]
public sealed record AtelierTransferStockQuery : IRequest<Result<List<AtelierTransferStockMasterDto>>>;

public sealed class AtelierTransferStockDetailDto
{
    public DateOnly Date { get; set; }
    public string DocumentNumber { get; set; } = default!;
    public string TargetAccountCode { get; set; } = default!;
    public string TargetAccountName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class AtelierTransferStockMasterDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string UnitTypeName { get; set; } = default!;
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CurrentStock { get; set; }
    public List<AtelierTransferStockDetailDto> Transfers { get; set; } = [];
}

internal sealed class AtelierTransferStockQueryHandler(
    IStockIssueRepository stockIssueRepository,
    IProductMovementRepository productMovementRepository) : IRequestHandler<AtelierTransferStockQuery, Result<List<AtelierTransferStockMasterDto>>>
{
    public async Task<Result<List<AtelierTransferStockMasterDto>>> Handle(AtelierTransferStockQuery request, CancellationToken cancellationToken)
    {
        List<StockIssue> transfers = await stockIssueRepository.GetAll()
            .Where(i => i.IssueType == StockIssueType.AtelierTransfer && !i.IsDeleted)
            .Include(i => i.Lines).ThenInclude(l => l.Product!).ThenInclude(p => p.ProductUnitType)
            .Include(i => i.TargetAccount)
            .OrderBy(i => i.Date)
            .ThenBy(i => i.DocumentNumber)
            .ToListAsync(cancellationToken);

        HashSet<IdentityId> productIds = transfers
            .SelectMany(t => t.Lines)
            .Select(l => l.ProductId)
            .Distinct()
            .ToHashSet();

        List<ProductMovement> movements = productIds.Count == 0
            ? []
            : await productMovementRepository.GetAll()
                .Where(m => productIds.Contains(m.ProductId) && !m.IsDeleted)
                .ToListAsync(cancellationToken);

        Dictionary<IdentityId, decimal> currentStockMap = movements
            .GroupBy(m => m.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(m => m.MovementType == ProductMovementType.Input ? m.Quantity : -m.Quantity));

        List<AtelierTransferStockMasterDto> result = [];

        foreach (IGrouping<IdentityId, StockIssueLine> group in transfers
            .SelectMany(t => t.Lines)
            .Where(l => l.Product is not null)
            .GroupBy(l => l.ProductId))
        {
            Product product = group.First().Product!;

            List<AtelierTransferStockDetailDto> details = transfers
                .Where(t => t.Lines.Any(l => l.ProductId == product.Id))
                .SelectMany(t => t.Lines
                    .Where(l => l.ProductId == product.Id)
                    .Select(l => new AtelierTransferStockDetailDto
                    {
                        Date = t.Date,
                        DocumentNumber = t.DocumentNumber,
                        TargetAccountCode = t.TargetAccount?.Code.Value ?? string.Empty,
                        TargetAccountName = t.TargetAccount?.Name.Value ?? string.Empty,
                        Quantity = l.Quantity,
                        UnitCost = l.UnitCost.Value,
                        TotalAmount = l.Quantity * l.UnitCost.Value
                    }))
                .OrderBy(d => d.Date)
                .ToList();

            result.Add(new AtelierTransferStockMasterDto
            {
                ProductId = product.Id,
                ProductCode = product.ProductCode.Value,
                ProductName = product.Name.Value,
                UnitTypeName = product.ProductUnitType?.Name.Value ?? string.Empty,
                TotalQuantity = group.Sum(l => l.Quantity),
                TotalAmount = group.Sum(l => l.Quantity * l.UnitCost.Value),
                CurrentStock = currentStockMap.TryGetValue(product.Id, out decimal stock) ? stock : 0m,
                Transfers = details
            });
        }

        return result
            .OrderBy(m => m.ProductCode)
            .ThenBy(m => m.ProductName)
            .ToList();
    }
}