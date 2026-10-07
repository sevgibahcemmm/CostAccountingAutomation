using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>Gönderenin kendi mesajını tamamen silmesi (yumuşak silme).</summary>
/// <remarks>
/// <para>
/// Silinen mesaj <b>iki tarafın da</b> görünümünden düşer: global sorgu
/// filtresi silinmiş satırları konuşmada, gelen kutusunda ve okunmamış
/// sayımlarında herkes için eler.
/// </para>
/// <para>
/// Bu, "Sohbeti Temizle"den ayrıdır: temizleme yalnızca çağıran kişinin
/// konuşma görünümünü gizler ve geri alınabilir; silme mesaj satırını iki
/// taraf için de yok eder, geri alınamaz. Duyuru mesajları silinemez
/// (N alıcıya N satır).
/// </para>
/// <para>
/// Mesajın eki varsa satır silindikten (depoda kaydedildikten) sonra dosya
/// da diskten kaldırılır.
/// </para>
/// </remarks>
[Permission(MessagePermissions.Send)]
public sealed record MessageDeleteCommand(Guid MessageId) : IRequest<Result<string>>;

internal sealed class MessageDeleteCommandHandler(
    IMessageRepository messageRepository,
    IClaimContext claimContext,
    IFileStorageService fileStorage) : IRequestHandler<MessageDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        MessageDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());

        UserMessage? message = await messageRepository.FirstOrDefaultAsync(
            m => m.Id == new IdentityId(request.MessageId), cancellationToken);

        if (message is null)
        {
            return Result<string>.Failure("Mesaj bulunamadı");
        }

        if (message.SenderId.Value != me.Value)
        {
            return Result<string>.Failure("Yalnızca kendi mesajınızı silebilirsiniz");
        }

        if (message.IsAnnouncement)
        {
            return Result<string>.Failure("Duyuru mesajları silinemez");
        }

        string? attachmentPath = message.AttachmentPath;

        // Silme SoftDeleteAsync içinde açıkça kaydedilir; ancak ek dosya ancak
        // o andan sonra diskten kaldırılır — kayıt düşerse satır dosyaya bağlı
        // kalır.
        if (!await messageRepository.SoftDeleteAsync(request.MessageId, me, cancellationToken))
        {
            return Result<string>.Failure("Mesaj silinemedi");
        }

        if (attachmentPath is not null)
        {
            await fileStorage.DeleteAsync(attachmentPath, cancellationToken);
        }

        return "Mesaj silindi";
    }
}
