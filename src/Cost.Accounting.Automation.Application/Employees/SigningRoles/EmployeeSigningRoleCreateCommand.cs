using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Employees;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Employees.SigningRoles;

/// <summary>
/// Yeni bir yetkili görev tanımı ekler. Böylece kurum, imza bloklarında
/// ihtiyaç duyduğu görevleri kod değiştirmeden tanımlayabilir.
/// </summary>
[Permission("employee:create")]
public sealed record EmployeeSigningRoleCreateCommand(
    string Name,
    string Description,
    bool RequiresWorkshop,
    int SortOrder,
    bool IsActive) : IRequest<Result<string>>;

public sealed class EmployeeSigningRoleCreateCommandValidator : AbstractValidator<EmployeeSigningRoleCreateCommand>
{
    public EmployeeSigningRoleCreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Görev adı giriniz")
            .MaximumLength(120).WithMessage("Görev adı en fazla 120 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir");

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 9999).WithMessage("Sıra 0 ile 9999 arasında olmalıdır");
    }
}

internal sealed class EmployeeSigningRoleCreateCommandHandler(
    IEmployeeSigningRoleRepository repository,
    IDuplicateCheckService duplicateCheckService)
    : IRequestHandler<EmployeeSigningRoleCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        EmployeeSigningRoleCreateCommand request,
        CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();

        string? duplicateKey = EmployeeSigningRole.BuildDuplicateKey(name);

        EmployeeSigningRole? duplicate = await duplicateCheckService.FindDuplicateAsync<EmployeeSigningRole>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (duplicate is not null)
        {
            return Result<string>.Failure($"'{name}' adında bir yetkili görev zaten tanımlı");
        }

        EmployeeSigningRole role = new(
            new Name(name),
            request.Description.Trim(),
            request.RequiresWorkshop,
            request.SortOrder);

        role.SetStatus(request.IsActive);

        await repository.AddAsync(role, cancellationToken);

        return $"'{name}' yetkili görevi başarıyla kaydedildi";
    }
}
