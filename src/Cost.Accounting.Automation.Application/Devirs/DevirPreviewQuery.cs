using Cost.Accounting.Automation.Application.Behaviors;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Devirs;

/// <summary>
/// Seçili mali yıl için devir ön izlemesini hazırlar: kaynak yıl, aktarılacak
/// kayıt sayıları ve devrin neden çalışmayacağına dair uyarı.
/// Hiçbir değişiklik yazmaz.
/// </summary>
[Permission("devir:view")]
public sealed record DevirPreviewQuery : IRequest<Result<DevirPreviewResult>>;

internal sealed class DevirPreviewQueryHandler(IDevirTransferService devirService)
    : IRequestHandler<DevirPreviewQuery, Result<DevirPreviewResult>>
{
    public async Task<Result<DevirPreviewResult>> Handle(
        DevirPreviewQuery request,
        CancellationToken cancellationToken)
    {
        DevirPreviewResult preview = await devirService.BuildPreviewAsync(cancellationToken);

        return Result<DevirPreviewResult>.Succeed(preview);
    }
}
