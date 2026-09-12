using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Users;
[Permission("user:delete")]
public sealed record UserDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class UserDeleteCommandHandler(
    IUserRepository userRepository) : IRequestHandler<UserDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UserDeleteCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (user is null)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı");
        }

        if (user.UserName.Value == "admin")
        {
            return Result<string>.Failure("Admin kullanıcısı silinemez");
        }

        user.Delete();
        userRepository.Update(user);

        return "Kullanıcı başarıyla silindi";
    }
}