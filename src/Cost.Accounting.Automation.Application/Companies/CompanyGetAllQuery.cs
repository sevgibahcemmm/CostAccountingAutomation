using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Companies;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Companies;
[Permission("company:view")]
public sealed record CompanyGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<CompanyDto>>
{
    public CompanyGetAllQuery() : this(false) { }
}

internal sealed class CompanyGetAllQueryHandler(
    ICompanyRepository CompanyRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<CompanyGetAllQuery, IQueryable<CompanyDto>>
{
    public async Task<IQueryable<CompanyDto>> Handle(CompanyGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Company> source = request.OnlyDeleted
            ? CompanyRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : CompanyRepository.GetAllActive();

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
