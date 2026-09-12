using FluentValidation;
using GenericRepository;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Shared;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Roles;
[Permission("role:create")]
public sealed record RoleCreateCommand(
    string Name,
    bool IsActive,
    List<string>? Permissions = null) : IRequest<Result<string>>;


public sealed class RoleCreateCommandValidator : AbstractValidator<RoleCreateCommand>
{
    public RoleCreateCommandValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Geçerli bir rol adı girin");
        RuleFor(p => p.Name)
            .Must(n => !string.Equals(n?.Trim(), "sys_admin", StringComparison.OrdinalIgnoreCase))
            .WithMessage("'sys_admin' adı sistem tarafından ayrılmıştır");
    }
}

internal sealed class RoleCreateCommandHandler(
    IRoleRepository roleRepository) : IRequestHandler<RoleCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RoleCreateCommand request, CancellationToken cancellationToken)
    {
        if (string.Equals(request.Name.Trim(), "sys_admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Failure("'sys_admin' adı sistem tarafından ayrılmıştır");
        }

        var nameExists = await roleRepository.AnyAsync(p => p.Name.Value == request.Name, cancellationToken);

        if (nameExists)
        {
            return Result<string>.Failure("Rol adı daha önce tanımlanmış");
        }

        Name name = new(request.Name);
        Role role = new(name, request.IsActive);

        if (request.Permissions is { Count: > 0 })
        {
            role.SetPermissions(request.Permissions.Select(p => new Permission(p)));
        }

        roleRepository.Add(role);

        return "Role başarıyla kaydedildi";
    }
}