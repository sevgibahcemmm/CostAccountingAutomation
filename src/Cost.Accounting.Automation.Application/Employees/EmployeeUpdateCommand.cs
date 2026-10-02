using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Employees;
using Cost.Accounting.Automation.Domain.Employees.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// Personel bilgilerini günceller ve yetkili görev listesini verilenle
/// tamamen değiştirir. Görevler yeniden yazıldığı için geçmiş görev
/// bilgisi saklanmaz. Fotoğraf verilmezse mevcut fotoğraf korunur.
/// </summary>
[Permission("employee:update")]
public sealed record EmployeeUpdateCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string IdentityNumber,
    string Title,
    string PhoneNumber1,
    string PhoneNumber2,
    string Email,
    PhotoInput? Photo,
    bool IsActive,
    IReadOnlyList<EmployeeDutyInput> Duties) : IRequest<Result<Guid>>;

public sealed class EmployeeUpdateCommandValidator : AbstractValidator<EmployeeUpdateCommand>
{
    public EmployeeUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir personel seçin");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Adı giriniz")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyadı giriniz")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir");

        RuleFor(x => x.IdentityNumber)
            .NotEmpty().WithMessage("TC kimlik numarasını giriniz")
            .Must(EmployeeIdentityNumber.IsValid)
            .WithMessage("TC kimlik numarası geçersizdir");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ünvanı giriniz")
            .MaximumLength(200).WithMessage("Ünvan en fazla 200 karakter olabilir");

        RuleFor(x => x.PhoneNumber1)
            .NotEmpty().WithMessage("Telefon numarası giriniz")
            .MaximumLength(50).WithMessage("Telefon numarası en fazla 50 karakter olabilir");

        RuleFor(x => x.PhoneNumber2)
            .MaximumLength(50).WithMessage("İkinci telefon en fazla 50 karakter olabilir");

        RuleFor(x => x.Email)
            .MaximumLength(200).WithMessage("E-posta en fazla 200 karakter olabilir");

        RuleFor(x => x.Duties)
            .Must(d => d is { Count: > 0 })
            .WithMessage(EmployeeMessages.NoDuty);

        RuleFor(x => x.Duties)
            .Must(d => !EmployeeDutyValidator.HasDuplicate(d))
            .WithMessage(EmployeeMessages.DuplicateDuty);

        RuleFor(x => x.Duties)
            .Must(d => !EmployeeDutyValidator.HasMissingWorkshop(d))
            .WithMessage("Atölye Şefi görevi için atölye seçmelisiniz");

        RuleFor(x => x.Duties)
            .Must(d => !EmployeeDutyValidator.HasUnselectedRole(d))
            .WithMessage(EmployeeMessages.UnselectedRole);
    }
}

internal sealed class EmployeeUpdateCommandHandler(
    IEmployeeRepository employeeRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IDuplicateCheckService duplicateCheckService,
    IFileStorageService fileStorage) : IRequestHandler<EmployeeUpdateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(EmployeeUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId employeeId = new(request.Id);

        Employee? employee = await employeeRepository.GetByExpressionWithTrackingAsync(
            e => e.Id == employeeId,
            cancellationToken);

        if (employee is null)
        {
            return Result<Guid>.Failure(EmployeeMessages.NotFound);
        }

        if (request.Duties.Count == 0)
        {
            return Result<Guid>.Failure(EmployeeMessages.NoDuty);
        }

        if (EmployeeDutyValidator.HasDuplicate(request.Duties))
        {
            return Result<Guid>.Failure(EmployeeMessages.DuplicateDuty);
        }

        if (EmployeeDutyValidator.HasMissingWorkshop(request.Duties))
        {
            return Result<Guid>.Failure("Atölye Şefi görevi için atölye seçmelisiniz.");
        }

        if (EmployeeDutyValidator.HasUnselectedRole(request.Duties))
        {
            return Result<Guid>.Failure(EmployeeMessages.UnselectedRole);
        }

        if (!await EmployeeDutyValidator.WorkshopsExistAsync(
                request.Duties, chartOfAccountRepository, cancellationToken))
        {
            return Result<Guid>.Failure(EmployeeMessages.InvalidWorkshop);
        }

        string identityNumber = request.IdentityNumber.Trim();

        Employee? duplicate = await duplicateCheckService.FindDuplicateAsync<Employee>(
            Employee.BuildDuplicateKey(identityNumber),
            excludeId: request.Id,
            cancellationToken: cancellationToken);

        if (duplicate is not null)
        {
            return Result<Guid>.Failure(EmployeeMessages.DuplicateIdentityNumber);
        }

        employee.SetFirstName(new FirstName(request.FirstName.Trim()));
        employee.SetLastName(new LastName(request.LastName.Trim()));
        employee.SetIdentityNumber(new TRIdentityNumber(identityNumber));
        employee.SetTitle(new EmployeeTitle(request.Title.Trim()));
        employee.SetPhoneNumber1(request.PhoneNumber1.Trim());
        employee.SetPhoneNumber2(request.PhoneNumber2.Trim());
        employee.SetEmail(request.Email.Trim());

        // Fotoğraf yalnızca yenisi seçildiyse değiştirilir; aksi hâlde
        // kayıtlı yol korunur. Fotoğrafın kaldırılması ayrı bir komutla yapılır.
        if (request.Photo is PhotoInput photo && photo.Data.Length > 0)
        {
            string photoPath = await fileStorage.SaveAsync(
                photo.Data,
                photo.FileName,
                EmployeePhotos.Folder,
                cancellationToken);

            employee.SetPhotoPath(photoPath);
        }

        employee.SetStatus(request.IsActive);

        employee.ReplaceDuties(
            request.Duties.Select(d => new EmployeeDuty(
                employee.Id,
                d.SigningRole,
                d.WorkshopId is null ? null : new IdentityId(d.WorkshopId.Value),
                d.IsActive)));

        employeeRepository.Update(employee);

        return Result<Guid>.Succeed(employee.Id.Value);
    }
}

/// <summary>
/// Personeli siler. Onaylanmış belgelerde adı geçen personel silinmez;
/// çünkü o belgenin imzası arşivde bu kişiye bağlıdır. Böyle bir durumda
/// kayıt pasife alınır ve kullanıcı uyarılır.
/// </summary>
[Permission("employee:delete")]
public sealed record EmployeeDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class EmployeeDeleteCommandHandler(
    IEmployeeRepository employeeRepository) : IRequestHandler<EmployeeDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(EmployeeDeleteCommand request, CancellationToken cancellationToken)
    {
        Employee? employee = await employeeRepository.GetByExpressionWithTrackingAsync(
            e => e.Id == new IdentityId(request.Id),
            cancellationToken);

        if (employee is null)
        {
            return Result<string>.Failure(EmployeeMessages.NotFound);
        }

        employee.Delete();
        employeeRepository.Update(employee);

        return DeleteWarnings.Compose(
            $"'{employee.FullName}' personeli silindi. NOT: Onaylanmış belgelerin "
            + "imza bölümlerinde bu kişiye ait ad geçiyor olabilir; bu belgeler "
            + "yeniden basılırsa imza alanı boş kalacaktır.");
    }
}

[Permission("employee:delete")]
public sealed record EmployeeRestoreCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class EmployeeRestoreCommandHandler(
    IEmployeeRepository employeeRepository) : IRequestHandler<EmployeeRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(EmployeeRestoreCommand request, CancellationToken cancellationToken)
    {
        Employee? employee = await employeeRepository.GetByIdIncludingDeletedAsync(
            new IdentityId(request.Id),
            cancellationToken);

        if (employee is null)
        {
            return Result<string>.Failure(EmployeeMessages.NotFound);
        }

        if (!employee.IsDeleted)
        {
            return Result<string>.Failure("Personel zaten silinmiş durumda değil");
        }

        employee.Restore();
        employeeRepository.Update(employee);

        return "Personel geri yüklendi";
    }
}