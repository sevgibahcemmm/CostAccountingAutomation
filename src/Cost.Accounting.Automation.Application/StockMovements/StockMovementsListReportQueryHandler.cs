using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.StockMovements;

internal sealed class StockMovementsListReportQueryHandler(
    IProductMovementRepository productMovementRepository)
    : IRequestHandler<StockMovementsListReportQuery, List<StockMovementReportRowDto>>
{
    public async Task<List<StockMovementReportRowDto>> Handle(
        StockMovementsListReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = productMovementRepository.GetAll()
            .Where(m => m.Date >= request.StartDate && m.Date <= request.EndDate)
            .Where(m => !m.IsDeleted);

        if (request.ProductId.HasValue)
        {
            query = query.Where(m => m.ProductId == request.ProductId.Value);
        }

        var movements = await query
            .Include(m => m.Product)
                .ThenInclude(p => p!.ProductUnitType)
            .ToListAsync(cancellationToken);

        var grouped = movements
            .GroupBy(m => new
            {
                m.ProductId,
                ProductName = m.Product!.Name.Value,
                ProductCode = m.Product.ProductCode.Value,
                UnitTypeName = m.Product.ProductUnitType!.Name.Value
            })
            .Select(g => new StockMovementReportRowDto
            {
                ProductName = g.Key.ProductName,
                ProductCode = g.Key.ProductCode,
                UnitTypeName = g.Key.UnitTypeName,
                TotalInQuantity = g.Where(m => m.MovementType == ProductMovementType.Input).Sum(m => m.Quantity),
                TotalOutQuantity = g.Where(m => m.MovementType == ProductMovementType.Output).Sum(m => m.Quantity),
                BalanceQuantity = g.Where(m => m.MovementType == ProductMovementType.Input).Sum(m => m.Quantity)
                                - g.Where(m => m.MovementType == ProductMovementType.Output).Sum(m => m.Quantity),
                UnitCost = g.Where(m => m.MovementType == ProductMovementType.Input && m.UnitPrice != null)
                                .Select(m => (decimal?)m.UnitPrice!.Value)
                                .Average() ?? 0m,
                TotalInAmount = g.Where(m => m.MovementType == ProductMovementType.Input && m.UnitPrice != null)
                                .Sum(m => m.Quantity * m.UnitPrice!.Value),
                TotalOutAmount = g.Where(m => m.MovementType == ProductMovementType.Output && m.UnitPrice != null)
                                .Sum(m => m.Quantity * m.UnitPrice!.Value),
                BalanceAmount = (g.Where(m => m.MovementType == ProductMovementType.Input && m.UnitPrice != null)
                                .Sum(m => m.Quantity * m.UnitPrice!.Value))
                                - (g.Where(m => m.MovementType == ProductMovementType.Output && m.UnitPrice != null)
                                .Sum(m => m.Quantity * m.UnitPrice!.Value)),
                SalesQuantity = 0m, // Would need invoice/sale data
                SalesAmount = 0m    // Would need invoice/sale data
            })
            .OrderBy(r => r.ProductName)
            .ToList();

        return grouped;
    }
}