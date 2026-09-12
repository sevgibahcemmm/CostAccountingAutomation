using GenericRepository;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Roles;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Roles;
[Permission("role:update_permissions")]
public sealed record RoleUpdatePermissionsCommand(
    Guid RoleId,
    List<string> Permissions) : IRequest<Result<string>>;

internal sealed class RoleUpdatePermissionsCommandHandler(
    IRoleRepository roleRepository) : IRequestHandler<RoleUpdatePermissionsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RoleUpdatePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.FirstOrDefaultAsync(p => p.Id == request.RoleId, cancellationToken);

        if (role is null)
        {
            return Result<string>.Failure("Rol bulunamadı");
        }

        List<Permission> permissions = request.Permissions.Select(s => new Permission(s)).ToList();
        role.SetPermissions(permissions);
        roleRepository.Update(role);

        return "İşlem başarıyla tamamlandı";
    }
}
