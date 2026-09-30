using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class UserRepository : MasterAuditableRepository<User>, IUserRepository
{
    public UserRepository(MasterDbContext context) : base(context)
    {
    }
}