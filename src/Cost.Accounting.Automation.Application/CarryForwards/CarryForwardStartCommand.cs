using Cost.Accounting.Automation.Application.Behaviors;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Devirs;

/// <summary>
/// Seçili mali yılın veritabanına, aynı şirketin bir önceki mali yılından
/// devir (açılış bakiyesi) yapar. Yalnızca boş yıl veritabanına çalışır.
/// </summary>
[Permission("devir:manage")]
public sealed record DevirStartCommand(DevirOptions Options) : IRequest<Result<DevirTransferResult>>;

public sealed class DevirStartCommandValidator : AbstractValidator<DevirStartCommand>
{
    public DevirStartCommandValidator()
    {
        RuleFor(p => p.Options)
            .NotNull()
            .WithMessage("Devir seçenekleri belirlenmelidir.");

        RuleFor(p => p.Options)
            .Must(o => o is not null && o.HasAnything)
            .WithMessage("En az bir veri grubu seçilmelidir.");
    }
}

internal sealed class DevirStartCommandHandler(IDevirTransferService devirService)
    : IRequestHandler<DevirStartCommand, Result<DevirTransferResult>>
{
    public async Task<Result<DevirTransferResult>> Handle(
        DevirStartCommand request,
        CancellationToken cancellationToken)
    {
        DevirPreviewResult preview = await devirService.BuildPreviewAsync(cancellationToken);

        if (!preview.HasSource)
        {
            return Result<DevirTransferResult>.Failure(
                "Devir alınacak bir önceki mali yıl bulunamadı. " +
                "Bu şirket için hedef yıldan eski bir mali yıl açılmalı.");
        }

        if (preview.BlockingReason is not null)
        {
            return Result<DevirTransferResult>.Failure(preview.BlockingReason);
        }

        if (preview.PreviousDevir is { } done)
        {
            return Result<DevirTransferResult>.Failure(
                $"{done.TargetYear} mali yılı zaten {done.SourceYear} mali yılından devredilmiş "
                + $"({done.CreatedAt:d.MM.yyyy HH:mm}). Mükerrer devir yapılamaz.");
        }

        // Önizleme ile yazma arasında hedef yıla hareket girmiş olabilir veya
        // devir tamamlanmış olabilir. Servisin bu iş kuralı hataları
        // InvalidOperationException ile bildirir; kullanıcıya stack trace yerine
        // okunabilir mesaj dönmek için burada yakalanır.
        try
        {
            DevirTransferResult result = await devirService.TransferAsync(request.Options, cancellationToken);

            return Result<DevirTransferResult>.Succeed(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result<DevirTransferResult>.Failure(ex.Message);
        }
    }
}
