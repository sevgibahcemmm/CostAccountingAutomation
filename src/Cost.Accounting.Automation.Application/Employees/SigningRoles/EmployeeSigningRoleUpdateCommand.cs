using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Employees.SigningRoles;

[Permission("employee:update")]
public sealed record EmployeeSigningRoleUpdateCommand(
    Guid Id,
    string Name,
    string Description,
    bool RequiresWorkshop,
    int SortOrder,
    bool IsActive) : IRequest<Result<string>>;

public sealed class EmployeeSigningRoleUpdateCommandValidator : AbstractValidator<EmployeeSigningRoleUpdateCommand>
{
    public EmployeeSigningRoleUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir görev seçin");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Görev adı giriniz")
            .MaximumLength(120).WithMessage("Görev adı en fazla 120 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir");

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 9999).WithMessage("Sıra 0 ile 9999 arasında olmalıdır");
    }
}

internal sealed class EmployeeSigningRoleUpdateCommandHandler(
    IEmployeeSigningRoleRepository repository,
    IDuplicateCheckService duplicateCheckService)
    : IRequestHandler<EmployeeSigningRoleUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        EmployeeSigningRoleUpdateCommand request,
        CancellationToken cancellationToken)
    {
        EmployeeSigningRole? role = await repository.GetByExpressionWithTrackingAsync(
            r => r.Id == new IdentityId(request.Id),
            cancellationToken);

        if (role is null)
        {
            return Result<string>.Failure("Yetkili görev bulunamadı");
        }

        string name = request.Name.Trim();

        string? duplicateKey = EmployeeSigningRole.BuildDuplicateKey(name);

        EmployeeSigningRole? duplicate = await duplicateCheckService.FindDuplicateAsync<EmployeeSigningRole>(
            duplicateKey,
            excludeId: request.Id,
            cancellationToken: cancellationToken);

        if (duplicate is not null)
        {
            return Result<string>.Failure($"'{name}' adında başka bir yetkili görev zaten tanımlı");
        }

        role.SetName(new Name(name));
        role.SetDescription(request.Description.Trim());
        role.SetRequiresWorkshop(request.RequiresWorkshop);
        role.SetSortOrder(request.SortOrder);
        role.SetStatus(request.IsActive);

        repository.Update(role);

        return $"'{name}' yetkili görevi başarıyla güncellendi";
    }
}
