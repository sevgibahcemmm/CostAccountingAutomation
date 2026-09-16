using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:view")]
public sealed record ConsumptionUnitGetNextCodeQuery : IRequest<Result<string>>;

internal sealed class ConsumptionUnitGetNextCodeQueryHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ConsumptionUnitGetNextCodeQuery, Result<string>>
{
    public async Task<Result<string>> Handle(ConsumptionUnitGetNextCodeQuery request, CancellationToken cancellationToken)
    {
        List<ChartOfAccount> all = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);

        return Result<string>.Succeed(ConsumptionUnitHelper.BuildNextCode(all));
    }
}
