using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Employees.SigningRoles;

/// <summary>
/// Yetkili görev tanımını siler. Görev bir personele atanmışsa silinmez;
/// çünkü o kaydın imza satırı bu göreve bağlıdır. Böyle bir durumda kayıt
/// pasife alınır ve kullanıcı uyarılır.
/// </summary>
[Permission("employee:delete")]
public sealed record EmployeeSigningRoleDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class EmployeeSigningRoleDeleteCommandHandler(
    IEmployeeSigningRoleRepository repository,
    IEmployeeDutyRepository dutyRepository) : IRequestHandler<EmployeeSigningRoleDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        EmployeeSigningRoleDeleteCommand request,
        CancellationToken cancellationToken)
    {
        EmployeeSigningRole? role = await repository.GetByExpressionWithTrackingAsync(
            r => r.Id == new IdentityId(request.Id),
            cancellationToken);

        if (role is null)
        {
            return Result<string>.Failure("Yetkili görev bulunamadı");
        }

        bool assigned = await dutyRepository.AnyAsync(
            d => d.SigningRoleId == role.Id && !d.IsDeleted,
            cancellationToken);

        if (assigned)
        {
            role.SetStatus(false);
            repository.Update(role);

            return Result<string>.Failure(
                $"'{role.Name.Value}' görevi personele atanmış olduğu için silinemedi. "
                + "Görev 'Pasif' yapıldı; personellerden bu görev kaldırıldıktan sonra silebilirsiniz.");
        }

        role.Delete();
        repository.Update(role);

        return $"'{role.Name.Value}' yetkili görevi silindi";
    }
}

[Permission("employee:delete")]
public sealed record EmployeeSigningRoleRestoreCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class EmployeeSigningRoleRestoreCommandHandler(
    IEmployeeSigningRoleRepository repository) : IRequestHandler<EmployeeSigningRoleRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        EmployeeSigningRoleRestoreCommand request,
        CancellationToken cancellationToken)
    {
        EmployeeSigningRole? role = await repository.GetByIdIncludingDeletedAsync(
            new IdentityId(request.Id),
            cancellationToken);

        if (role is null)
        {
            return Result<string>.Failure("Yetkili görev bulunamadı");
        }

        if (!role.IsDeleted)
        {
            return Result<string>.Failure("Yetkili görev zaten silinmiş durumda değil");
        }

        role.Restore();
        repository.Update(role);

        return $"'{role.Name.Value}' yetkili görevi geri yüklendi";
    }
}
