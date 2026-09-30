using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

/// <summary>
/// Giriş ekranındaki kurum ve mali yıl seçim kutularını doldurur. Yalnızca
/// master veritabanını okur; oturum açılmadan önce çalışabilir.
/// </summary>
public sealed record LoginScopeGetQuery : IRequest<Result<List<LoginScopeDto>>>;

public sealed record LoginScopeDto
{
    public required Guid CompanyId { get; init; }
    public required string CompanyName { get; init; }
    public required List<LoginScopeYearDto> Years { get; init; }
}

public sealed record LoginScopeYearDto
{
    public required Guid CompanyYearId { get; init; }
    public required int Year { get; init; }
    public required string DatabaseName { get; init; }
    public required bool IsClosed { get; init; }
}

internal sealed class LoginScopeGetQueryHandler(
    IMasterCompanyNameLookup companyNameLookup,
    ICompanyYearRepository companyYearRepository) : IRequestHandler<LoginScopeGetQuery, Result<List<LoginScopeDto>>>
{
    public async Task<Result<List<LoginScopeDto>>> Handle(
        LoginScopeGetQuery request,
        CancellationToken cancellationToken)
    {
        List<MasterCompanyInfo> companies = await companyNameLookup.GetAllAsync(cancellationToken);

        if (companies.Count == 0)
        {
            return Result<List<LoginScopeDto>>.Failure("Tanımlı kurum bulunamadı.");
        }

        List<CompanyYear> years = await companyYearRepository.GetAllAsync(cancellationToken);

        Dictionary<Guid, List<LoginScopeYearDto>> yearsByCompany = years
            .GroupBy(cy => cy.CompanyId.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(cy => cy.Year.Value)
                    .Select(cy => new LoginScopeYearDto
                    {
                        CompanyYearId = cy.Id.Value,
                        Year = cy.Year.Value,
                        DatabaseName = cy.DatabaseName.Value,
                        IsClosed = cy.IsClosed
                    })
                    .ToList());

        List<LoginScopeDto> scopes = companies
            .OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(company => new LoginScopeDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                Years = yearsByCompany.GetValueOrDefault(company.Id) ?? []
            })
            .ToList();

        return Result<List<LoginScopeDto>>.Succeed(scopes);
    }
}
