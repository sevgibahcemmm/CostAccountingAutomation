using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.CostSlips;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:view")]
public sealed record ExpenseDistributionReportQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    CostSlipType? CostSlipType = null)
    : IRequest<ExpenseDistributionReportResult>;

public sealed class ExpenseDistributionReportResult
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public CostSlipType? CostSlipType { get; init; }
    public List<ExpenseDistributionRow> Rows { get; init; } = [];
    public List<string> ColumnHeaders { get; init; } = [];
}

public sealed class ExpenseDistributionRow
{
    public Guid WorkshopId { get; init; }
    public string WorkshopName { get; init; } = string.Empty;
    public Dictionary<ExpenseAccountType, decimal> Values { get; init; } = [];
    public decimal Total => Values.Values.Sum();
}