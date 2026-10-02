using Cost.Accounting.Automation.Application.Behaviors;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CarryForwards;

/// <summary>
/// Seçili mali yılın veritabanına, aynı şirketin bir önceki mali yılından
/// devir (açılış bakiyesi) yapar. Yalnızca boş yıl veritabanına çalışır.
/// </summary>
[Permission("devir:manage")]
public sealed record CarryForwardStartCommand(CarryForwardOptions Options) : IRequest<Result<CarryForwardTransferResult>>;

public sealed class CarryForwardStartCommandValidator : AbstractValidator<CarryForwardStartCommand>
{
    public CarryForwardStartCommandValidator()
    {
        RuleFor(p => p.Options)
            .NotNull()
            .WithMessage("Devir seçenekleri belirlenmelidir.");

        RuleFor(p => p.Options)
            .Must(o => o is not null && o.HasAnything)
            .WithMessage("En az bir veri grubu seçilmelidir.");
    }
}

internal sealed class CarryForwardStartCommandHandler(ICarryForwardTransferService devirService)
    : IRequestHandler<CarryForwardStartCommand, Result<CarryForwardTransferResult>>
{
    public async Task<Result<CarryForwardTransferResult>> Handle(
        CarryForwardStartCommand request,
        CancellationToken cancellationToken)
    {
        CarryForwardPreviewResult preview = await devirService.BuildPreviewAsync(cancellationToken);

        if (!preview.HasSource)
        {
            return Result<CarryForwardTransferResult>.Failure(
                "Devir alınacak bir önceki mali yıl bulunamadı. " +
                "Bu şirket için hedef yıldan eski bir mali yıl açılmalı.");
        }

        if (preview.BlockingReason is not null)
        {
            return Result<CarryForwardTransferResult>.Failure(preview.BlockingReason);
        }

        if (preview.PreviousCarryForward is { } done)
        {
            return Result<CarryForwardTransferResult>.Failure(
                $"{done.TargetYear} mali yılı zaten {done.SourceYear} mali yılından devredilmiş "
                + $"({done.CreatedAt:d.MM.yyyy HH:mm}). Mükerrer devir yapılamaz.");
        }

        // Önizleme ile yazma arasında hedef yıla hareket girmiş olabilir veya
        // devir tamamlanmış olabilir. Servisin bu iş kuralı hataları
        // InvalidOperationException ile bildirir; kullanıcıya stack trace yerine
        // okunabilir mesaj dönmek için burada yakalanır.
        try
        {
            CarryForwardTransferResult result = await devirService.TransferAsync(request.Options, cancellationToken);

            return Result<CarryForwardTransferResult>.Succeed(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result<CarryForwardTransferResult>.Failure(ex.Message);
        }
    }
}
