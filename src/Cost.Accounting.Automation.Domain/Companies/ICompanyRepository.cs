using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Companies;

public interface ICompanyRepository : IAuditableRepository<Company>
{
    /// <summary>Şirkete bağlı kullanıcı varsa not üretir; engelleyici hareket yoktur.</summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> companyIds,
        CancellationToken cancellationToken = default);
}
