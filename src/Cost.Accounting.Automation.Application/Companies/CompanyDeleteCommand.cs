using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Companies;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;

[Permission("company:delete")]
public sealed record CompanyDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class CompanyDeleteCommandHandler(
    ICompanyRepository companyRepository) : IRequestHandler<CompanyDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CompanyDeleteCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (company is null)
        {
            return Result<string>.Failure("Şirket bulunamadı");
        }

        company.Delete();
        companyRepository.Update(company);

        return "Şirket başarıyla silindi";
    }
}