using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.ChartOfAccounts;

public interface IChartOfAccountRepository : IAuditableRepository<ChartOfAccount>
{
    Task<List<ChartOfAccount>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);

    Task<AccountDeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Hareket (işlem) görmüş hesaplar engellenir; yalnızca yapısal/ilişkili referansı olan
/// (hareket görmemiş) hesaplar "uyar ama sil" kapsamında kalır.
/// </summary>
public sealed record AccountDeletionCheck(
    IReadOnlyCollection<Guid> MovementAccountIds,
    IReadOnlyCollection<Guid> RelatedAccountIds);