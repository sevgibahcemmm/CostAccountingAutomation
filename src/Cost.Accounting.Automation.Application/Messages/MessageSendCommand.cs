using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Domain.Users;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>Mesaja eklenen dosya (gönderenin verdiği ad + içerik).</summary>
public sealed record MessageAttachmentInput(string FileName, byte[] Data);

/// <summary>Bir kullanıcıya tek bir mesaj gönderir.</summary>
[Permission(MessagePermissions.Send)]
public sealed record MessageSendCommand(
    Guid RecipientId,
    string Body,
    string? Subject,
    MessageAttachmentInput? Attachment = null) : IRequest<Result<string>>;

public sealed class MessageSendCommandValidator : AbstractValidator<MessageSendCommand>
{
    /// <summary>Ek için izin verilen en büyük boyut: 25 MB.</summary>
    public const long MaxAttachmentBytes = 25L * 1024 * 1024;

    public MessageSendCommandValidator()
    {
        RuleFor(p => p.RecipientId)
            .NotEqual(Guid.Empty).WithMessage("Geçerli bir alıcı seçin");

        // Metin ya da ek (ya da ikisi birden) zorunludur: yalnızca dosyadan
        // oluşan mesaj geçerlidir.
        RuleFor(p => p.Body)
            .Must((command, body) => !string.IsNullOrWhiteSpace(body) || command.Attachment is not null)
            .WithMessage("Mesaj metni boş olamaz")
            .MaximumLength(MessageBody.MaxLength)
            .WithMessage($"Mesaj en fazla {MessageBody.MaxLength} karakter olabilir");

        RuleFor(p => p.Subject)
            .MaximumLength(200)
            .WithMessage("Konu en fazla 200 karakter olabilir");

        RuleFor(p => p.Attachment)
            .Must(attachment => attachment is null || !string.IsNullOrWhiteSpace(attachment.FileName))
            .WithMessage("Dosya adı boş olamaz")
            .Must(attachment => attachment is null || (attachment.Data is not null && attachment.Data.Length > 0))
            .WithMessage("Dosya içeriği boş olamaz")
            .Must(attachment => attachment is null || attachment.Data.LongLength <= MaxAttachmentBytes)
            .WithMessage("Dosya en fazla 25 MB olabilir");
    }
}

/// <summary>Tek bir alıcıya mesaj gönderir.</summary>
internal sealed class MessageSendCommandHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    IClaimContext claimContext,
    IFileStorageService fileStorage) : IRequestHandler<MessageSendCommand, Result<string>>
{
    private const string AttachmentFolder = "MessageFiles";

    public async Task<Result<string>> Handle(MessageSendCommand request, CancellationToken cancellationToken)
    {
        Guid senderId = claimContext.GetUserId();
        var recipientId = new IdentityId(request.RecipientId);

        if (recipientId.Value == senderId)
        {
            return Result<string>.Failure("Kendinize mesaj gönderemezsiniz");
        }

        var recipient = await userRepository.FirstOrDefaultAsync(
            p => p.Id == recipientId, cancellationToken);

        if (recipient is null)
        {
            return Result<string>.Failure("Alıcı bulunamadı");
        }

        if (recipient.IsActive == false)
        {
            return Result<string>.Failure("Alıcı pasif durumda, mesaj gönderilemiyor");
        }

        // Ek önce depoya yazılır: kayıt (ve dolayısıyla mesaj satırı) ancak
        // dosya gerçekten diskteyse oluşur. Depolama hatası istemciye
        // exception olarak döner ve gönderim yapılmaz.
        string? attachmentPath = null;
        string? attachmentFileName = null;
        long? attachmentSize = null;

        if (request.Attachment is { } attachment)
        {
            attachmentPath = await fileStorage.SaveAsync(
                attachment.Data, attachment.FileName, AttachmentFolder,
                allowAnyExtension: true, cancellationToken);
            attachmentFileName = attachment.FileName;
            attachmentSize = attachment.Data.LongLength;
        }

        var message = new UserMessage(
            new IdentityId(senderId),
            recipientId,
            new MessageBody(request.Body),
            string.IsNullOrWhiteSpace(request.Subject) ? null : new MessageSubject(request.Subject),
            isAnnouncement: false,
            attachmentPath,
            attachmentFileName,
            attachmentSize);

        await messageRepository.AddAsync(message, cancellationToken);

        return "Mesaj gönderildi";
    }
}
