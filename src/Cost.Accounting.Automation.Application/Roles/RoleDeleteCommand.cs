using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Roles;
[Permission("role:delete")]
public sealed record RoleDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class RoleDeleteCommandHandler(
    IRoleRepository roleRepository,
    IUserRepository userRepository) : IRequestHandler<RoleDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RoleDeleteCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (role is null)
        {
            return Result<string>.Failure("Rol bulunamadı");
        }

        bool hasUser = await userRepository.AnyAsync(u => u.RoleId == request.Id, cancellationToken);

        role.Delete();
        roleRepository.Update(role);

        if (hasUser)
        {
            return DeleteWarnings.Compose(
                $"'{role.Name.Value}' rolü silindi, ancak rol kullanıcılara tanımlı olduğu için " +
                $"ilgili kullanıcıların rolünün gözden geçirilmesi gerekir.");
        }

        return "Rol başarıyla silindi";
    }
}