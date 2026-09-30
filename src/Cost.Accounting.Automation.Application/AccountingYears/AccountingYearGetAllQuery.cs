using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.AccountingYears;

/// <summary>Mali yıl listesi satırı.</summary>
[Permission("company:view")]
public sealed record AccountingYearDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public int Year { get; init; }
    public string DatabaseName { get; init; } = string.Empty;
    public bool IsClosed { get; init; }
    public DateTimeOffset OpeningDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>Belirli bir şirketin açık yıl kayıtları.</summary>
[Permission("company:view")]
public sealed record AccountingYearGetAllQuery(Guid? CompanyId = null)
    : IRequest<List<AccountingYearDto>>;

internal sealed class AccountingYearGetAllQueryHandler(
    ICompanyYearRepository companyYearRepository,
    IMasterCompanyNameLookup companyNameLookup) : IRequestHandler<AccountingYearGetAllQuery, List<AccountingYearDto>>
{
    public async Task<List<AccountingYearDto>> Handle(AccountingYearGetAllQuery request, CancellationToken cancellationToken)
    {
        List<CompanyYear> years = request.CompanyId is { } companyId
            ? await companyYearRepository.GetAllByCompanyAsync(new IdentityId(companyId), cancellationToken)
            : await companyYearRepository.GetAllAsync(cancellationToken);

        Dictionary<Guid, string> companyNames = await companyNameLookup.GetNamesAsync(
            years.Select(cy => cy.CompanyId.Value).Distinct().ToList(),
            cancellationToken);

        return years
            .Select(cy => new AccountingYearDto
            {
                Id = cy.Id.Value,
                CompanyId = cy.CompanyId.Value,
                CompanyName = companyNames.GetValueOrDefault(cy.CompanyId.Value, "-"),
                Year = cy.Year.Value,
                DatabaseName = cy.DatabaseName.Value,
                IsClosed = cy.IsClosed,
                OpeningDate = cy.OpeningDate,
                CreatedAt = cy.CreatedAt
            })
            .OrderByDescending(dto => dto.Year)
            .ThenBy(dto => dto.CompanyName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
