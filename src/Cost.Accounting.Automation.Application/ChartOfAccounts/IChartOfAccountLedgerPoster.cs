using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;


public interface IChartOfAccountLedgerPoster
{
    Task PostAsync(
        IdentityId chartOfAccountId,
        decimal debit,
        decimal credit,
        string sourceType,
        IdentityId? sourceId,
        CancellationToken cancellationToken);
}

internal sealed class ChartOfAccountLedgerPoster : IChartOfAccountLedgerPoster
{
    private readonly IChartOfAccountLedgerRepository _ledgerRepository;

    public ChartOfAccountLedgerPoster(IChartOfAccountLedgerRepository ledgerRepository)
    {
        _ledgerRepository = ledgerRepository;
    }

    public Task PostAsync(
        IdentityId chartOfAccountId,
        decimal debit,
        decimal credit,
        string sourceType,
        IdentityId? sourceId,
        CancellationToken cancellationToken)
    {
        ChartOfAccountLedger entry = new(chartOfAccountId, debit, credit, sourceType, sourceId);
        return _ledgerRepository.AddAsync(entry, cancellationToken);
    }
}