using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:delete")]
public sealed record ConsumptionUnitDeleteCommand(Guid Id) : IRequest<Result<string>>;

/// <summary>
/// Tüketim birimi bir hesap planı kaydı olduğu için hesap korumasıyla
/// denetlenir: işlem gördüğü için silinemez, ilişkili kayıtlarda kullanılıyorsa
/// silme sonrası not üretir.
/// </summary>
internal sealed class ConsumptionUnitDeleteCommandHandler(
    IChartOfAccountRepository chartOfAccountRepository)
    : IRequestHandler<ConsumptionUnitDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        ConsumptionUnitDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<ChartOfAccount>(
            chartOfAccountRepository,
            (ids, token) => chartOfAccountRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "tüketim birimi",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "tüketim birimi");
    }
}