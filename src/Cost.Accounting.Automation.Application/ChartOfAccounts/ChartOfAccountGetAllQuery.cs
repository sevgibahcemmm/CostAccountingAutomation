using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:view")]
public sealed record ChartOfAccountGetAllQuery() : IRequest<IQueryable<ChartOfAccountDto>>;

internal sealed class ChartOfAccountGetAllQueryHandler(
    IChartOfAccountRepository chartOfAccountRepository,
    IChartOfAccountLedgerRepository ledgerRepository,
    IAuditUserNameResolver auditUserNames)
    : IRequestHandler<ChartOfAccountGetAllQuery, IQueryable<ChartOfAccountDto>>
{
    public async Task<IQueryable<ChartOfAccountDto>> Handle(ChartOfAccountGetAllQuery request, CancellationToken cancellationToken)
    {
        // Hesap planı binlerce satırdır: liste doğrudan veritabanında projekte
        // edilir. Daha önce tüm tablo belleğe alınıp sonra DTO'ya çevriliyordu.
        List<ChartOfAccountDto> items = await chartOfAccountRepository
            .GetAllActive()
            .MapTo()
            .ToListAsync(cancellationToken);

        await auditUserNames.FillAsync(items, cancellationToken);

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

        // Yukarıdan aşağıya toplulaştırma ağacın tamamını gezmek zorunda; bu
        // veritabanında değil bellekte, O(n) maliyetle yapılır.
        ChartOfAccountBalanceCalculator.RollUp(items);

        return items.AsQueryable();
    }
}
