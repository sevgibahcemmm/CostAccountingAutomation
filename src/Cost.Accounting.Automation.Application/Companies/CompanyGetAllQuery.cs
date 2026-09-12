using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
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
    ICompanyRepository CompanyRepository) : IRequestHandler<CompanyGetAllQuery, IQueryable<CompanyDto>>
{
    public Task<IQueryable<CompanyDto>> Handle(CompanyGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<Company>> source = request.OnlyDeleted
            ? CompanyRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : CompanyRepository.GetAllWithAudit();

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}
