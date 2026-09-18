using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:view")]
public sealed record ChartOfAccountGetNextCodeQuery(Guid? ParentId) : IRequest<Result<string?>>
{
    public ChartOfAccountGetNextCodeQuery() : this((Guid?)null) { }
}

internal sealed class ChartOfAccountGetNextCodeQueryHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ChartOfAccountGetNextCodeQuery, Result<string?>>
{
    public async Task<Result<string?>> Handle(ChartOfAccountGetNextCodeQuery request, CancellationToken cancellationToken)
    {
        List<ChartOfAccount> all = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);

        if (request.ParentId is not Guid parentId)
        {
            return string.Empty;
        }

        ChartOfAccount? parent = all.FirstOrDefault(a => a.Id == parentId);

        if (parent is null)
        {
            return Result<string?>.Failure("Üst hesap bulunamadı.");
        }

        string next = ChartOfAccountCodeHelper.BuildNextChildCode(all, parent.Code.Value);

        return next;
    }
}