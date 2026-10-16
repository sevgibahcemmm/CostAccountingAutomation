using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;
[Permission("company:view")]
public sealed record CompanyGetQuery(
    Guid Id) : IRequest<Result<CompanyDto>>;

internal sealed class CompanyGetQueryHandler(
    ICompanyRepository CompanyRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<CompanyGetQuery, Result<CompanyDto>>
{
    public async Task<Result<CompanyDto>> Handle(CompanyGetQuery request, CancellationToken cancellationToken)
    {
        // Bu sorgu her rapor yazdırıldığında çağrılır. Önceden liste sorgusu
        // üzerinden filtreleniyordu; liste belleğe alındığı için "tek şirket"
        // için tüm şirket tablosu okunuyordu.
        CompanyDto? company = await CompanyRepository
            .GetAllActive()
            .Where(i => i.Id == new IdentityId(request.Id))
            .MapTo()
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null)
        {
            return Result<CompanyDto>.Failure("Şube bulunamadı");
        }

        await auditUserNames.FillAsync([company], cancellationToken);

        return company;
    }
}
