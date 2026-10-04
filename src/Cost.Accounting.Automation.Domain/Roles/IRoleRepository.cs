using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Roles;

public interface IRoleRepository : IAuditableRepository<Role>
{
    /// <summary>Role atanmış kullanıcı varsa not üretir; engelleyici hareket yoktur.</summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken = default);
}
