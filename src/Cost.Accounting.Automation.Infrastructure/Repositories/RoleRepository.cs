using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class RoleRepository : MasterAuditableRepository<Role>, IRoleRepository
{
    public RoleRepository(MasterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// <c>Role</c> ve <c>User</c> MASTER veritabanındadır; bu yüzden denetim
    /// <see cref="MasterDbContext"/> üzerinde yapılır.
    /// </summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid> keys = [.. roleIds];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        List<Guid> userIds = await this.Context.Set<User>()
            .Where(u => keys.Contains(u.RoleId))
            .Select(u => u.RoleId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new DeletionCheck([], userIds);
    }
}
