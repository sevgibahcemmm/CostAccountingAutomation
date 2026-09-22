using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.StockMovements;

[Permission("stockmovement:view")]
public sealed record StockMovementsListReportQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ProductId = null,
    Guid? WarehouseId = null)
    : IRequest<List<StockMovementReportRowDto>>;

public sealed class StockMovementReportRowDto
{
    public ChartOfAccountType AccountType { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public string SubGroupCode { get; init; } = string.Empty;
    public string SubGroupName { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string ProductCode { get; init; } = string.Empty;
    public string UnitTypeName { get; init; } = string.Empty;
    public decimal TotalInQuantity { get; init; }
    public decimal TotalOutQuantity { get; init; }
    public decimal BalanceQuantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal TotalInAmount { get; init; }
    public decimal TotalOutAmount { get; init; }
    public decimal BalanceAmount { get; init; }
    public decimal SalesQuantity { get; init; }
    public decimal SalesAmount { get; init; }
}
