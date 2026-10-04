using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Companies;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Companies;

/// <summary>Seçili şirketleri tek transaction'da siler.</summary>
[Permission("company:delete")]
public sealed record BulkDeleteCompaniesCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteCompaniesCommandHandler(
    ICompanyRepository companyRepository)
    : IRequestHandler<BulkDeleteCompaniesCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteCompaniesCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Company>(
            companyRepository,
            (ids, token) => companyRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "şirket",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "şirket");
    }
}