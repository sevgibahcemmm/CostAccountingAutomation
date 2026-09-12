using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;
internal sealed class UserRepository : AuditableRepository<User, ApplicationDbContext>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
}
