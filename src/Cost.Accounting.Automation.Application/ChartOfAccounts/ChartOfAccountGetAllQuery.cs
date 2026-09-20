using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:view")]
public sealed record ChartOfAccountGetAllQuery() : IRequest<IQueryable<ChartOfAccountDto>>;

internal sealed class ChartOfAccountGetAllQueryHandler(
    IChartOfAccountRepository chartOfAccountRepository,
    IChartOfAccountLedgerRepository ledgerRepository)
    : IRequestHandler<ChartOfAccountGetAllQuery, IQueryable<ChartOfAccountDto>>
{
    public async Task<IQueryable<ChartOfAccountDto>> Handle(ChartOfAccountGetAllQuery request, CancellationToken cancellationToken)
    {
        List<ChartOfAccountDto> items = await chartOfAccountRepository
            .GetAllWithAudit()
            .MapTo()
            .ToListAsync(cancellationToken);

        Dictionary<Guid, (decimal Debit, decimal Credit)> totals =
            await ledgerRepository.GetTotalsByAccountAsync(cancellationToken);

        foreach (ChartOfAccountDto item in items)
        {
            if (totals.TryGetValue(item.Id, out (decimal Debit, decimal Credit) t))
            {
                item.DebitAmount = t.Debit;
                item.CreditAmount = t.Credit;
            }
        }

        ChartOfAccountBalanceCalculator.RollUp(items);

        return items.AsQueryable();
    }
}