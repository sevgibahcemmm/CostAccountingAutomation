using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;
[Permission("company:view")]
public sealed record CompanyGetQuery(
    Guid Id) : IRequest<Result<CompanyDto>>;

internal sealed class CompanyGetQueryHandler(
    ICompanyRepository CompanyRepository) : IRequestHandler<CompanyGetQuery, Result<CompanyDto>>
{
    public async Task<Result<CompanyDto>> Handle(CompanyGetQuery request, CancellationToken cancellationToken)
    {
        var Company = await CompanyRepository
            .GetAllWithAudit()
            .MapTo()
            .Where(i => i.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (Company is null)
        {
            return Result<CompanyDto>.Failure("Şube bulunamadı");
        }

        return Company;
    }
}