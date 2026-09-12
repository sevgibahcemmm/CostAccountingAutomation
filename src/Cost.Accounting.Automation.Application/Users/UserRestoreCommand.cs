using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Users;
[Permission("user:delete")]
public sealed record UserRestoreCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class UserRestoreCommandHandler(
    IUserRepository userRepository) : IRequestHandler<UserRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UserRestoreCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdIncludingDeletedAsync(new IdentityId(request.Id), cancellationToken);
        if (user is null)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı");
        }

        if (!user.IsDeleted)
        {
            return Result<string>.Failure("Kullanıcı zaten silinmiş durumda değil");
        }

        user.Restore();
        userRepository.Update(user);

        return "Kullanıcı başarıyla geri yüklendi";
    }
}