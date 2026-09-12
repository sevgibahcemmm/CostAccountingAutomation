using GenericRepository;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Roles;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Roles;
[Permission("role:delete")]
public sealed record RoleDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class RoleDeleteCommandHandler(
    IRoleRepository roleRepository) : IRequestHandler<RoleDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RoleDeleteCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (role is null)
        {
            return Result<string>.Failure("Rol bulunamadı");
        }

        role.Delete();
        roleRepository.Update(role);

        return "Rol başarıyla silindi";
    }
}