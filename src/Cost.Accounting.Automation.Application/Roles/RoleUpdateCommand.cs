using FluentValidation;
using GenericRepository;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Shared;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Roles;
[Permission("role:edit")]
public sealed record RoleUpdateCommand(
    Guid Id,
    string Name,
    bool IsActive,
    List<string>? Permissions = null) : IRequest<Result<string>>;

public sealed class RoleUpdateCommandValidator : AbstractValidator<RoleUpdateCommand>
{
    public RoleUpdateCommandValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Geçerli bir rol adı girin");
        RuleFor(p => p.Name)
            .Must(n => !string.Equals(n?.Trim(), "sys_admin", StringComparison.OrdinalIgnoreCase))
            .WithMessage("'sys_admin' adı sistem tarafından ayrılmıştır");
    }
}

internal sealed class RoleUpdateCommandHandler(
    IRoleRepository roleRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<RoleUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RoleUpdateCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (role is null)
        {
            return Result<string>.Failure("Rol bulunamadı");
        }

        if (!string.Equals(role.Name.Value, "sys_admin", StringComparison.OrdinalIgnoreCase)
            && string.Equals(request.Name.Trim(), "sys_admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Failure("'sys_admin' adı sistem tarafından ayrılmıştır");
        }

        string? duplicateKey = Role.BuildDuplicateKey(request.Name);

        Role? nameDuplicate = await duplicateCheckService.FindMasterDuplicateAsync<Role>(
            duplicateKey,
            excludeId: request.Id,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Rol adı başka bir rol tarafından kullanılıyor");
        }

        Name name = new(request.Name);
        role.SetName(name);
        role.SetStatus(request.IsActive);

        if (request.Permissions is not null)
        {
            role.SetPermissions(request.Permissions.Select(p => new Permission(p)));
        }

        roleRepository.Update(role);

        return "Rol başarıyla güncellendi";
    }
}