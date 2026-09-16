using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:delete")]
public sealed record ConsumptionUnitDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class ConsumptionUnitDeleteCommandHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ConsumptionUnitDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ConsumptionUnitDeleteCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        ChartOfAccount? unit = await chartOfAccountRepository.GetByExpressionWithTrackingAsync(
            x => x.Id == id,
            cancellationToken);

        if (unit is null || unit.Type != ChartOfAccountType.ConsumptionUnit)
        {
            return Result<string>.Failure("Tüketim birimi bulunamadı.");
        }

        chartOfAccountRepository.SoftDelete(unit);

        return $"'{unit.Name.Value}' tüketim birimi silindi.";
    }
}
