using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;

[Permission("company:delete")]
public sealed record CompanyDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class CompanyDeleteCommandHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository) : IRequestHandler<CompanyDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CompanyDeleteCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (company is null)
        {
            return Result<string>.Failure("Şirket bulunamadı");
        }

        bool hasUser = await userRepository.AnyAsync(u => u.CompanyId == request.Id, cancellationToken);

        company.Delete();
        companyRepository.Update(company);

        if (hasUser)
        {
            return DeleteWarnings.Compose(
                $"'{company.Name.Value}' şirketi silindi, ancak şirkete bağlı kullanıcılar olduğu için " +
                $"ilgili kullanıcıların gözden geçirilmesi gerekir.");
        }

        return "Şirket başarıyla silindi";
    }
}