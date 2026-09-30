using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.AccountingYears;

/// <summary>
/// Yeni mali yıl açar: master'a yıl kaydını ekler ve adı verilen iş
/// veritabanını oluşturup şemasını günceller.
/// </summary>
[Permission("company:edit")]
public sealed record AccountingYearCreateCommand(
    Guid CompanyId,
    int Year,
    string DatabaseName,
    DateTimeOffset? OpeningDate = null) : IRequest<Result<AccountingYearCreateResult>>;

public sealed record AccountingYearCreateResult
{
    public required Guid CompanyYearId { get; init; }
    public required string DatabaseName { get; init; }
    public required int Year { get; init; }
    public required bool DatabaseCreated { get; init; }
    public required int AppliedMigrationCount { get; init; }
}

public sealed class AccountingYearCreateCommandValidator : AbstractValidator<AccountingYearCreateCommand>
{
    public AccountingYearCreateCommandValidator()
    {
        RuleFor(p => p.CompanyId).NotEmpty().WithMessage("Şirket seçilmelidir.");
        RuleFor(p => p.Year).InclusiveBetween(Year.MinSupported, Year.MaxSupported)
            .WithMessage($"Mali yıl {Year.MinSupported}-{Year.MaxSupported} aralığında olmalıdır.");
        RuleFor(p => p.DatabaseName).NotEmpty().WithMessage("Veritabanı adı boş olamaz.");
        RuleFor(p => p.DatabaseName)
            .Must(BeValidDatabaseName)
            .WithMessage(
                "Veritabanı adı yalnızca harf, rakam, boşluk, tire ve alt çizgi içerebilir; " +
                "harf veya rakamla başlamalı ve ' - ' dışında boşluk içermemelidir.");
    }

    private static bool BeValidDatabaseName(string candidate)
        => DatabaseName.TryCreate(candidate, out _, out _);
}

internal sealed class AccountingYearCreateCommandHandler(
    ICompanyYearRepository companyYearRepository,
    IMasterCompanyNameLookup companyNameLookup,
    IAccountingYearProvisioner provisioner,
    IAccountingDbSelector dbSelector,
    IMasterUnitOfWork unitOfWork) : IRequestHandler<AccountingYearCreateCommand, Result<AccountingYearCreateResult>>
{
    public async Task<Result<AccountingYearCreateResult>> Handle(
        AccountingYearCreateCommand request,
        CancellationToken cancellationToken)
    {
        var companyId = new IdentityId(request.CompanyId);

        if (await companyYearRepository.GetByCompanyAndYearAsync(companyId, request.Year, cancellationToken) is not null)
        {
            return Result<AccountingYearCreateResult>.Failure(
                $"{request.Year} mali yılı bu şirket için zaten açılmış.");
        }

        if (!DatabaseName.TryCreate(request.DatabaseName, out var databaseName, out var nameError)
            || databaseName is null)
        {
            return Result<AccountingYearCreateResult>.Failure(nameError ?? "Veritabanı adı geçersiz.");
        }

        if (await companyYearRepository.DatabaseNameExistsAsync(databaseName.Value, cancellationToken))
        {
            return Result<AccountingYearCreateResult>.Failure(
                $"'{databaseName.Value}' adı başka bir mali yıl kaydında kullanılıyor.");
        }

        if (string.Equals(dbSelector.MasterDatabaseName, databaseName.Value, StringComparison.OrdinalIgnoreCase))
        {
            return Result<AccountingYearCreateResult>.Failure(
                $"'{databaseName.Value}' adı master veritabanında kullanılıyor. Farklı bir ad girin.");
        }

        Dictionary<Guid, string> companyNames = await companyNameLookup.GetNamesAsync(
            [request.CompanyId],
            cancellationToken);

        if (!companyNames.ContainsKey(request.CompanyId))
        {
            return Result<AccountingYearCreateResult>.Failure("Şirket bulunamadı.");
        }

        AccountingYearProvisionResult provision = await provisioner.EnsureDatabaseAsync(
            companyId,
            request.Year,
            databaseName.Value,
            cancellationToken);

        var companyYear = new CompanyYear(companyId, new Year(request.Year), databaseName);
        companyYear.SetOpeningDate(
            request.OpeningDate
            ?? new DateTimeOffset(request.Year, 1, 1, 0, 0, 0, TimeSpan.Zero));

        await companyYearRepository.AddAsync(companyYear, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AccountingYearCreateResult>.Succeed(new AccountingYearCreateResult
        {
            CompanyYearId = companyYear.Id.Value,
            DatabaseName = databaseName.Value,
            Year = request.Year,
            DatabaseCreated = provision.Created,
            AppliedMigrationCount = provision.AppliedMigrationCount
        });
    }
}
