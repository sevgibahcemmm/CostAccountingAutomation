using Cost.Accounting.Automation.Application.Behaviors;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CarryForwards;

/// <summary>
/// Seçili mali yıl için devir ön izlemesini hazırlar: kaynak yıl, aktarılacak
/// kayıt sayıları ve devrin neden çalışmayacağına dair uyarı.
/// Hiçbir değişiklik yazmaz.
/// </summary>
[Permission("devir:view")]
public sealed record CarryForwardPreviewQuery : IRequest<Result<CarryForwardPreviewResult>>;

internal sealed class CarryForwardPreviewQueryHandler(ICarryForwardTransferService devirService)
    : IRequestHandler<CarryForwardPreviewQuery, Result<CarryForwardPreviewResult>>
{
    public async Task<Result<CarryForwardPreviewResult>> Handle(
        CarryForwardPreviewQuery request,
        CancellationToken cancellationToken)
    {
        CarryForwardPreviewResult preview = await devirService.BuildPreviewAsync(cancellationToken);

        return Result<CarryForwardPreviewResult>.Succeed(preview);
    }
}
