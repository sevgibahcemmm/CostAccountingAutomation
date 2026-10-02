using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CostSlips;

internal sealed class ExpenseDistributionReportQueryHandler(
    ICostSlipRepository costSlipRepository,
    IChartOfAccountRepository chartOfAccountRepository)
    : IRequestHandler<ExpenseDistributionReportQuery, ExpenseDistributionReportResult>
{
    public async Task<ExpenseDistributionReportResult> Handle(ExpenseDistributionReportQuery request, CancellationToken cancellationToken)
    {
        var expenseTypes = Enum.GetValues<ExpenseAccountType>().OrderBy(e => (int)e).ToList();

        var slipQuery = costSlipRepository.GetAll()
            .Where(s => s.CostDate >= request.StartDate && s.CostDate <= request.EndDate)
            .Where(s => !s.IsDeleted);

        if (request.CostSlipType.HasValue)
        {
            slipQuery = slipQuery.Where(s => s.CostSlipType == request.CostSlipType.Value);
        }

        var slips = await slipQuery
            .Include(s => s.CostSlipItems)
            .Include(s => s.Workshop)
            .ToListAsync(cancellationToken);

        var workshopAccounts = await chartOfAccountRepository.GetAll()
            .Where(a => a.Type == ChartOfAccountType.Workshop && a.IsActive)
            .OrderBy(a => a.Name.Value)
            .Select(a => new { a.Id, a.Name })
            .ToListAsync(cancellationToken);

        var rows = workshopAccounts.Select(w => new ExpenseDistributionRow
        {
            WorkshopId = w.Id,
            WorkshopName = w.Name.Value,
            Values = expenseTypes.ToDictionary(e => e, _ => 0m)
        }).ToList();

        foreach (var slip in slips)
        {
            var row = rows.FirstOrDefault(r => r.WorkshopId == slip.WorkshopId);
            if (row == null) continue;

            foreach (var item in slip.CostSlipItems)
            {
                if (row.Values.ContainsKey(item.ExpenseAccountType))
                {
                    row.Values[item.ExpenseAccountType] += item.TotalAmount;
                }
            }
        }

        var columnHeaders = expenseTypes.Select(e => CostSlipDto.GetDisplayName(e)).ToList();

        return new ExpenseDistributionReportResult
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CostSlipType = request.CostSlipType,
            Rows = rows,
            ColumnHeaders = columnHeaders
        };
    }
}