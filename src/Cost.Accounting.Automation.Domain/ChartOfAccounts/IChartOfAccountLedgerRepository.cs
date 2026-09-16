
namespace Cost.Accounting.Automation.Domain.ChartOfAccounts;

public interface IChartOfAccountLedgerRepository
{
    Task AddAsync(ChartOfAccountLedger entry, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<ChartOfAccountLedger> entries, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, (decimal Debit, decimal Credit)>> GetTotalsByAccountAsync(CancellationToken cancellationToken = default);

    Task<List<ChartOfAccountLedger>> GetBySourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default);

    Task<HashSet<string>> GetExistingSourceKeysAsync(CancellationToken cancellationToken = default);

    void SoftDeleteRange(IEnumerable<ChartOfAccountLedger> entries);

    void RestoreRange(IEnumerable<ChartOfAccountLedger> entries);
}