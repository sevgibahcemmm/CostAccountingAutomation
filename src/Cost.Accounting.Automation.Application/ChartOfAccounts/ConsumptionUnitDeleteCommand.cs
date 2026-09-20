using Cost.Accounting.Automation.Application;
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

        AccountDeletionCheck check = await chartOfAccountRepository.GetDeletionCheckAsync([request.Id], cancellationToken);
        if (check.MovementAccountIds.Count > 0)
        {
            return Result<string>.Failure(
                $"'{unit.Name.Value}' tüketim birimi işlem/hareket gördüğü için silinemez.");
        }

        chartOfAccountRepository.SoftDelete(unit);

        if (check.RelatedAccountIds.Count == 0)
        {
            return $"'{unit.Name.Value}' tüketim birimi silindi.";
        }

        return DeleteWarnings.Compose(
            $"'{unit.Name.Value}' tüketim birimi silindi. NOT: ilişkili kayıtlarda kullanılıyor; " +
            $"hareket görmediği için silme gerçekleştirildi.");
    }
}
