using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Companies;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;

[Permission("company:delete")]
public sealed record CompanyDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

/// <summary>Şirkete bağlı kullanıcı varsa silme sonrası not üretir.</summary>
internal sealed class CompanyDeleteCommandHandler(
    ICompanyRepository companyRepository) : IRequestHandler<CompanyDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CompanyDeleteCommand request, CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Company>(
            companyRepository,
            (ids, token) => companyRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "şirket",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "şirket");
    }
}