using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.AccountingYears;

/// <summary>
/// Giriş ekranında seçilen mali yılı açar: yıl veritabanını hazırlar ve
/// <see cref="IAccountingDbSelector"/> üzerinden iş bağlamını yıla yönlendirir.
/// Giriş sonrası, uygulamanın ana penceresine geçmeden hemen önce çağrılır.
/// </summary>
public sealed record AccountingYearSelectCommand(
    Guid CompanyYearId) : IRequest<Result<AccountingYearSelectResult>>;

public sealed record AccountingYearSelectResult
{
    public required Guid CompanyId { get; init; }
    public required string CompanyName { get; init; }
    public required int Year { get; init; }
    public required string DatabaseName { get; init; }
    public required bool DatabaseCreated { get; init; }
}

internal sealed class AccountingYearSelectCommandHandler(
    ICompanyYearRepository companyYearRepository,
    IMasterCompanyNameLookup companyNameLookup,
    IAccountingYearProvisioner provisioner,
    IAccountingDbSelector dbSelector) : IRequestHandler<AccountingYearSelectCommand, Result<AccountingYearSelectResult>>
{
    public async Task<Result<AccountingYearSelectResult>> Handle(
        AccountingYearSelectCommand request,
        CancellationToken cancellationToken)
    {
        CompanyYear? companyYear = await companyYearRepository.GetByIdAsync(
            new IdentityId(request.CompanyYearId),
            cancellationToken);

        if (companyYear is null)
        {
            return Result<AccountingYearSelectResult>.Failure("Seçilen mali yıl bulunamadı.");
        }

        if (companyYear.IsClosed)
        {
            return Result<AccountingYearSelectResult>.Failure(
                $"{companyYear.Year.Value} mali yılı kapatılmış. Açık bir yıl seçin.");
        }

        Dictionary<Guid, string> companyNames = await companyNameLookup.GetNamesAsync(
            [companyYear.CompanyId.Value],
            cancellationToken);

        if (!companyNames.TryGetValue(companyYear.CompanyId.Value, out string? companyName))
        {
            return Result<AccountingYearSelectResult>.Failure("Şirket bulunamadı.");
        }

        AccountingYearProvisionResult provision = await provisioner.EnsureDatabaseAsync(
            companyYear.CompanyId,
            companyYear.Year.Value,
            companyYear.DatabaseName.Value,
            cancellationToken);

        dbSelector.Select(companyYear.CompanyId, companyYear.Year, companyYear.DatabaseName.Value);

        return Result<AccountingYearSelectResult>.Succeed(new AccountingYearSelectResult
        {
            CompanyId = companyYear.CompanyId.Value,
            CompanyName = companyName,
            Year = companyYear.Year.Value,
            DatabaseName = companyYear.DatabaseName.Value,
            DatabaseCreated = provision.Created
        });
    }
}
