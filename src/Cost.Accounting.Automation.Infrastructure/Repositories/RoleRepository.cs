using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;
internal sealed class RoleRepository : AuditableRepository<Role, ApplicationDbContext>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }
}