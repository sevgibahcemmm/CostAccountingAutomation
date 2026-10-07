using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>Gönderenin kendi mesajındaki hatayı düzeltmesi.</summary>
/// <remarks>
/// <para>
/// Yalnızca <b>gönderen</b> düzenleyebilir ve yalnızca normal (birebir)
/// mesajlar düzenlenebilir; duyuru mesajları N alıcıya N satır olarak
/// yazıldığı için tek satır düzenlemek kopyalar arasında tutarsızlık
/// yaratırdı.
/// </para>
/// <para>
/// Mesajın sırası değişmez: <c>CreatedAt</c> korunur, yalnızca gövde ve
/// düzenleme damgası (<c>EditedAt</c>) güncellenir. Alıcı düzenlemeyi
/// baloncukta "düzenlendi" notuyla görür. Silme ve "Sohbeti Temizle"den
/// bağımsızdır.
/// </para>
/// </remarks>
[Permission(MessagePermissions.Send)]
public sealed record MessageEditCommand(
    Guid MessageId,
    string Body) : IRequest<Result<DateTimeOffset?>>;

public sealed class MessageEditCommandValidator : AbstractValidator<MessageEditCommand>
{
    public MessageEditCommandValidator()
    {
        RuleFor(p => p.MessageId)
            .NotEqual(Guid.Empty).WithMessage("Düzenlenecek mesaj belirlenemedi");

        RuleFor(p => p.Body)
            .NotEmpty().WithMessage("Mesaj metni boş olamaz")
            .MaximumLength(MessageBody.MaxLength)
            .WithMessage($"Mesaj en fazla {MessageBody.MaxLength} karakter olabilir");
    }
}

internal sealed class MessageEditCommandHandler(
    IMessageRepository messageRepository,
    IClaimContext claimContext) : IRequestHandler<MessageEditCommand, Result<DateTimeOffset?>>
{
    public async Task<Result<DateTimeOffset?>> Handle(
        MessageEditCommand request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());

        UserMessage? message = await messageRepository.FirstOrDefaultAsync(
            m => m.Id == new IdentityId(request.MessageId), cancellationToken);

        if (message is null)
        {
            return Result<DateTimeOffset?>.Failure("Mesaj bulunamadı");
        }

        if (message.SenderId.Value != me.Value)
        {
            return Result<DateTimeOffset?>.Failure("Yalnızca kendi mesajınızı düzenleyebilirsiniz");
        }

        if (message.IsAnnouncement)
        {
            return Result<DateTimeOffset?>.Failure("Duyuru mesajları düzenlenemez");
        }

        DateTimeOffset? editedAt = await messageRepository.EditBodyAsync(
            request.MessageId, me, request.Body, cancellationToken);

        if (editedAt is null)
        {
            return Result<DateTimeOffset?>.Failure("Mesaj düzenlenemedi");
        }

        return editedAt;
    }
}
