using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Application.Services;
public interface IJwtProvider
{
    Task<string> CreateTokenAsync(User user, CancellationToken cancellationToken = default);
}
