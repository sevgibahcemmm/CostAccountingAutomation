using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class CompanyRepository : MasterAuditableRepository<Company>, ICompanyRepository
{
    public CompanyRepository(MasterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// <c>Company</c> ve <c>User</c> MASTER veritabanındadır; bu yüzden denetim
    /// <see cref="MasterDbContext"/> üzerinde yapılır.
    /// </summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> companyIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid> keys = [.. companyIds];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        List<Guid> userIds = await this.Context.Set<User>()
            .Where(u => keys.Contains(u.CompanyId))
            .Select(u => u.CompanyId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new DeletionCheck([], userIds);
    }
}
