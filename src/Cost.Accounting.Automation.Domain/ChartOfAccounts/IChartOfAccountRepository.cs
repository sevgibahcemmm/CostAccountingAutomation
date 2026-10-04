using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.ChartOfAccounts;

public interface IChartOfAccountRepository : IAuditableRepository<ChartOfAccount>
{
    Task<List<ChartOfAccount>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seçilen hesapların tamamını tek sorguda denetler: hareket gören hesaplar
    /// engellenir, yalnızca ilişkili referansı olan hesaplar not üretir.
    /// </summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default);
}
