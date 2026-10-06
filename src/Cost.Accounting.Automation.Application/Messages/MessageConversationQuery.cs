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

/// <summary>İki kullanıcı arasındaki konuşma geçmişi.</summary>
[Permission(MessagePermissions.View)]
public sealed record MessageConversationQuery(
    Guid CounterpartId,
    bool AnnouncementChannel = false) : IRequest<Result<List<MessageDto>>>;

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
    IClaimContext claimContext) : IRequestHandler<MessageConversationQuery, Result<List<MessageDto>>>
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

        var messages = await messageRepository.GetConversationAsync(
            me, counterpart, request.AnnouncementChannel, cancellationToken);

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
                SentAt = m.CreatedAt
            })
            .ToList();
    }
}
