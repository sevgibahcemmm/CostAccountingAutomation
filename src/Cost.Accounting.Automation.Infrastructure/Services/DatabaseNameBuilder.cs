using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Yıl veritabanı adını <c>{Prefix}_{Yıl}_{Şirket}</c> biçiminde üretir.
/// Yılın başta yer alması, Object Explorer'da veritabanlarının yıla göre
/// gruplanmış görünmesini sağlar.
/// </summary>
internal sealed class DatabaseNameBuilder(
    IOptions<DatabaseNamingOptions> options,
    ICompanyYearRepository companyYearRepository,
    IAccountingDbSelector dbSelector) : IDatabaseNameBuilder
{
    private const int MaxOccurrence = 100;

    private readonly DatabaseNamingOptions _options = options.Value;

    public string Build(string companyName, int year, int occurrence = 1)
        => CompanyYear.BuildDatabaseName(
            _options.Prefix,
            new Name(companyName),
            new Year(year),
            occurrence).Value;

    public bool IsValidName(string? candidate, out string? error)
        => DatabaseName.TryCreate(candidate, out _, out error);

    public async Task<string> SuggestAvailableAsync(
        string companyName,
        int year,
        CancellationToken cancellationToken = default)
    {
        List<CompanyYear> existing = await companyYearRepository.GetAllAsync(cancellationToken);

        HashSet<string> taken = new(
            existing.Select(cy => cy.DatabaseName.Value),
            StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(dbSelector.MasterDatabaseName))
        {
            taken.Add(dbSelector.MasterDatabaseName);
        }

        for (int occurrence = 1; occurrence <= MaxOccurrence; occurrence++)
        {
            string candidate = Build(companyName, year, occurrence);

            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }

        return Build(companyName, year, 1);
    }
}
