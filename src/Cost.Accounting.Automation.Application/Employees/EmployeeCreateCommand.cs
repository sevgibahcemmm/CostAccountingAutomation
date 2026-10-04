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

/// <summary>Personel fotoğraflarının yazılacağı klasör (dosya depolama kökü altında).</summary>
public static class EmployeePhotos
{
    public const string Folder = "EmployeeImages";
}

/// <summary>Komut kayıtlarındaki görev satırı.</summary>
/// <param name="SigningRoleId">
/// Yetkili görev tanımının kimliği. Görevler veritabanında tanımlı olduğu için
/// burada enum değil, <c>EmployeeSigningRoles</c> tablosunun birincil anahtarı
/// taşınır; seçim kutusu bu listeyi doldurur.
/// </param>
public sealed record EmployeeDutyInput(
    Guid? SigningRoleId,
    Guid? WorkshopId,
    bool IsActive = true);

/// <summary>
/// Yeni personel kaydı oluşturur ve yetkili görevlerini birlikte yazar.
/// Görevler kayıtla aynı işlemde saklanır; görev geçmişi tutulmaz, çünkü
/// raporlar yalnızca güncel yetkiliyi gösterir.
///
/// <para>
/// Fotoğrafın byte'ları komutla birlikte taşınır ve dosya sistemine
/// <c>IFileStorageService</c> ile işleyicide yazılır; arayüz katmanı disk
/// erişimiyle uğraşmaz.
/// </para>
/// </summary>
[Permission("employee:create")]
public sealed record EmployeeCreateCommand(
    string FirstName,
    string LastName,
    string IdentityNumber,
    string Title,
    string PhoneNumber1,
    string PhoneNumber2,
    string Email,
    string? RegistryNumber,
    PhotoInput? Photo,
    bool IsActive,
    IReadOnlyList<EmployeeDutyInput> Duties) : IRequest<Result<Guid>>;

public sealed class EmployeeCreateCommandValidator : AbstractValidator<EmployeeCreateCommand>
{
    public EmployeeCreateCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Adı giriniz")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyadı giriniz")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir");

        // Alan ekranda gruplu gösterildiği için doğrulama biçimi yok sayar.
        // Yalnızca 11 hanelik rakam kuralı uygulanır; kontrol hanesi
        // doğrulanmaz (bkz. EmployeeIdentityNumber).
        RuleFor(x => x.IdentityNumber)
            .Must(v => EmployeeIdentityNumber.Validate(v) is null)
            .WithMessage(cmd => EmployeeIdentityNumber.Validate(cmd.IdentityNumber)!);

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

        // Sicil numarası isteğe bağlıdır; yalnızca uzunluk sınırlanır.
        // Biçim kısıtı konmaz, sicil numaraları kuruma göre farklı yazılabilir.
        RuleFor(x => x.RegistryNumber)
            .MaximumLength(50).WithMessage(EmployeeMessages.RegistryNumberTooLong);

        RuleFor(x => x.Duties)
            .Must(d => d is { Count: > 0 })
            .WithMessage(EmployeeMessages.NoDuty);

        RuleFor(x => x.Duties)
            .Must(d => !EmployeeDutyValidator.HasDuplicate(d))
            .WithMessage(EmployeeMessages.DuplicateDuty);

        RuleFor(x => x.Duties)
            .Must(d => !EmployeeDutyValidator.HasUnselectedRole(d))
            .WithMessage(EmployeeMessages.UnselectedRole);
    }
}

internal sealed class EmployeeCreateCommandHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeSigningRoleRepository signingRoleRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IDuplicateCheckService duplicateCheckService,
    IFileStorageService fileStorage) : IRequestHandler<EmployeeCreateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(EmployeeCreateCommand request, CancellationToken cancellationToken)
    {
        // Doğrulama katmanı atlanmış olabilir (toplu yükleme vb.); kurallar
        // burada da tekrarlanır çünkü görev benzersizliği veritabanında
        // benzersiz indeksle korunamıyor ve atölye zorunluluğu görev
        // tanımından okunuyor.
        if (request.Duties.Count == 0)
        {
            return Result<Guid>.Failure(EmployeeMessages.NoDuty);
        }

        if (EmployeeDutyValidator.HasDuplicate(request.Duties))
        {
            return Result<Guid>.Failure(EmployeeMessages.DuplicateDuty);
        }

        if (EmployeeDutyValidator.HasUnselectedRole(request.Duties))
        {
            return Result<Guid>.Failure(EmployeeMessages.UnselectedRole);
        }

        string? roleProblem = await EmployeeDutyValidator.FindProblemAsync(
            request.Duties, signingRoleRepository, cancellationToken);

        if (roleProblem is not null)
        {
            return Result<Guid>.Failure(roleProblem);
        }

        if (!await EmployeeDutyValidator.WorkshopsExistAsync(
                request.Duties, chartOfAccountRepository, cancellationToken))
        {
            return Result<Guid>.Failure(EmployeeMessages.InvalidWorkshop);
        }

        string identityNumber = request.IdentityNumber.Trim();

        Employee? duplicate = await duplicateCheckService.FindDuplicateAsync<Employee>(
            Employee.BuildDuplicateKey(identityNumber),
            cancellationToken: cancellationToken);

        if (duplicate is not null)
        {
            return Result<Guid>.Failure(EmployeeMessages.DuplicateIdentityNumber);
        }

        // Sicil numarası doluysa benzersiz olmalıdır. Veritabanındaki filtreli
        // benzersiz indeks son savunmadır; kullanıcıya anlaşılır bir mesaj
        // dönmek için burada önceden denetlenir.
        string? registryNumber = EmployeeRegistryNumber.Normalize(request.RegistryNumber);

        if (registryNumber is not null && await employeeRepository.RegistryNumberExistsAsync(
                registryNumber, excludeId: null, cancellationToken))
        {
            return Result<Guid>.Failure(EmployeeMessages.DuplicateRegistryNumber);
        }

        // Fotoğraf önce diske yazılır: kayıt, dosya yazımı başarısız olursa
        // yarım (fotoğrafsız) bir personel bırakılmasın.
        string? photoPath = null;

        if (request.Photo is PhotoInput photo && photo.Data.Length > 0)
        {
            photoPath = await fileStorage.SaveAsync(
                photo.Data,
                photo.FileName,
                EmployeePhotos.Folder,
                cancellationToken);
        }

        Employee employee = new(
            new FirstName(request.FirstName.Trim()),
            new LastName(request.LastName.Trim()),
            new TRIdentityNumber(identityNumber),
            new EmployeeTitle(request.Title.Trim()),
            request.PhoneNumber1.Trim(),
            request.PhoneNumber2.Trim(),
            request.Email.Trim(),
            photoPath,
            request.IsActive);

        employee.SetRegistryNumber(registryNumber);

        employee.ReplaceDuties(
            request.Duties.Select(d => new EmployeeDuty(
                employee.Id,
                new IdentityId(d.SigningRoleId!.Value),
                d.WorkshopId is null ? null : new IdentityId(d.WorkshopId.Value),
                d.IsActive)));

        await employeeRepository.AddAsync(employee, cancellationToken);

        return Result<Guid>.Succeed(employee.Id.Value);
    }
}