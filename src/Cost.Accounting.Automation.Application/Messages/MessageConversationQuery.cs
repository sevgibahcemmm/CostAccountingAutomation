using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>
/// İki kullanıcı arasındaki konuşma geçmişi.
/// </summary>
/// <remarks>
/// <paramref name="Limit"/> 0 (varsayılan) ise tüm geçmiş döner; 0'dan büyükse
/// yalnızca en yeni o kadar mesaj (bkz. IMessageRepository.GetConversationAsync).
/// <paramref name="Before"/> verilirse yalnızca o andan önceki mesajlar döner;
/// bu, "daha eskilerini yükle" sayfalamasının devamıdır.
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageConversationQuery(
    Guid CounterpartId,
    bool AnnouncementChannel = false,
    int Limit = 0,
    DateTimeOffset? Before = null) : IRequest<Result<List<MessageDto>>>;

/// <summary>Tek bir mesajın görüntüleme bilgisi.</summary>
public sealed class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }

    public string SenderFullName { get; set; } = string.Empty;

    public bool SentByCurrentUser { get; set; }

    public string? Subject { get; set; }

    public string Body { get; set; } = string.Empty;

    public bool IsAnnouncement { get; set; }

    /// <summary>Mesaj alıcı tarafından okunmuş mu.</summary>
    public bool IsRead { get; set; }

    /// <summary>Mesaj alıcının ekranında gösterilmiş mi (teslim edildi).</summary>
    public bool IsDelivered { get; set; }

    public DateTimeOffset SentAt { get; set; }

    /// <summary>Düzenleme anı; hiç düzenlendiyse <c>null</c>.</summary>
    public DateTimeOffset? EditedAt { get; set; }

    /// <summary>Ekli dosyanın depodaki göreli yolu; ek yoksa <c>null</c>.</summary>
    public string? AttachmentPath { get; set; }

    /// <summary>Gönderenin verdiği dosya adı (ekranada görünen ad).</summary>
    public string? AttachmentFileName { get; set; }

    /// <summary>Ek dosyanın boyutu (bayt).</summary>
    public long? AttachmentSize { get; set; }

    /// <summary>
    /// Ek dosyanın tam disk yolu; açma ve önizleme bu yolla yapılır.
    /// </summary>
    public string? AttachmentFullPath { get; set; }

    /// <summary>Ekli dosya resimse önizlemesi baloncuk içinde çizilir.</summary>
    public bool AttachmentIsImage =>
        AttachmentPath is not null
        && (Path.GetExtension(AttachmentPath) is { } ext
            && (ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".gif", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".webp", StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Mesajın durum tiki. Karşı taraftan gelen mesajlarda boştur — tik
    /// yalnızca gönderdiğimiz mesajlar için anlamlıdır.
    /// </summary>
    /// <remarks>
    /// WhatsApp'taki gösterimle aynı: tek tik gönderildi, gri çift tik teslim
    /// edildi, mavi çift tik okundu.
    /// </remarks>
    public string DeliveryMark => !SentByCurrentUser
        ? string.Empty
        : IsRead ? "✓✓" : IsDelivered ? "✓✓" : "✓";

    /// <summary>Tik rengi: okunduğunda mavi, teslim edildiğinde gri.</summary>
    public bool DeliveryIsRead => SentByCurrentUser && IsRead;
}

internal sealed class MessageConversationQueryHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    IClaimContext claimContext,
    IFileStorageService fileStorage) : IRequestHandler<MessageConversationQuery, Result<List<MessageDto>>>
{
    public async Task<Result<List<MessageDto>>> Handle(
        MessageConversationQuery request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());
        var counterpart = new IdentityId(request.CounterpartId);

        var counterpartUser = await userRepository.FirstOrDefaultAsync(
            p => p.Id == counterpart, cancellationToken);

        if (counterpartUser is null)
        {
            return Result<List<MessageDto>>.Failure("Kullanıcı bulunamadı");
        }

        // Kullanıcının bu konuşma için temizleme (görünüm gizleme) anı; eski
        // mesajlar bu andan önceyse bu kullanıcıya gösterilmez.
        DateTimeOffset? clearedAt = await messageRepository.GetClearedAtAsync(
            me, counterpart, request.AnnouncementChannel, cancellationToken);

        var messages = await messageRepository.GetConversationAsync(
            me,
            counterpart,
            request.AnnouncementChannel,
            request.Limit,
            request.Before,
            clearedAt,
            cancellationToken);

        var counterpartFullName = $"{counterpartUser.FirstName.Value} {counterpartUser.LastName.Value}";

        var myFullName = claimContext.GetUserFullNameOrDefault() ?? "Ben";

        return messages
            .Select(m => new MessageDto
            {
                Id = m.Id.Value,
                SenderId = m.SenderId.Value,

                // Kendi mesajlarımda karşı tarafın değil, kendi adım yazmalı.
                // Aksi hâlde iki taraflı konuşmada bütün satırlar aynı isimle görünür.
                SenderFullName = m.SenderId.Value == me.Value ? myFullName : counterpartFullName,
                SentByCurrentUser = m.SenderId.Value == me.Value,
                Subject = m.Subject?.Value,
                Body = m.Body.Value,
                IsAnnouncement = m.IsAnnouncement,
                IsRead = m.ReadState.Value,
                IsDelivered = m.DeliveredAt is not null,
                SentAt = m.CreatedAt,
                EditedAt = m.EditedAt,
                AttachmentPath = m.AttachmentPath,
                AttachmentFileName = m.AttachmentFileName,
                AttachmentSize = m.AttachmentSize,
                AttachmentFullPath = m.AttachmentPath is null
                    ? null
                    : fileStorage.GetFullPath(m.AttachmentPath)
            })
            .ToList();
    }
}
