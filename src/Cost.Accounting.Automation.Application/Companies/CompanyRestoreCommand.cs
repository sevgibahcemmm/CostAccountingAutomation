using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;
[Permission("company:delete")]
public sealed record CompanyRestoreCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class CompanyRestoreCommandHandler(
    ICompanyRepository companyRepository) : IRequestHandler<CompanyRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CompanyRestoreCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdIncludingDeletedAsync(new IdentityId(request.Id), cancellationToken);
        if (company is null)
        {
            return Result<string>.Failure("Şirket bulunamadı");
        }

        if (!company.IsDeleted)
        {
            return Result<string>.Failure("Şirket zaten silinmiş durumda değil");
        }

        company.Restore();
        companyRepository.Update(company);

        return "Şirket başarıyla geri yüklendi";
    }
}