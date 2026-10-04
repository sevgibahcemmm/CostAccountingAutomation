using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

/// <summary>
/// Seçili tüketim birimlerini tek transaction'da siler.
///
/// <para>
/// Tüketim birimleri hesap planında <see cref="ChartOfAccount"/> kaydıdır; bu
/// yüzden hareket denetimi hesap planı korumasıyla aynıdır.
/// </para>
/// </summary>
[Permission("chartofaccount:delete")]
public sealed record BulkDeleteConsumptionUnitsCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteConsumptionUnitsCommandHandler(
    IChartOfAccountRepository chartOfAccountRepository)
    : IRequestHandler<BulkDeleteConsumptionUnitsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteConsumptionUnitsCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<ChartOfAccount>(
            chartOfAccountRepository,
            (ids, token) => chartOfAccountRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "tüketim birimi",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "tüketim birimi");
    }
}