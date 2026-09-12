using GenericRepository;
using Cost.Accounting.Automation.Domain.LoginTokens;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;
internal sealed class LoginTokenRepository : Repository<LoginToken, ApplicationDbContext>, ILoginTokenRepository
{
    public LoginTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}
