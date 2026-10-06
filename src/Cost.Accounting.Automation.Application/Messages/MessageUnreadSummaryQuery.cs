using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>
/// Beni bekleyen okunmamış mesajları gönderene göre özetler.
/// </summary>
/// <remarks>
/// <para>
/// Bildirim metni ("Ahmet Yılmaz size 2 mesaj gönderdi") için yalnızca toplam
/// sayı yetmez; kimin yazdığı bilinmelidir. Bu sorgu ikisini birlikte verir.
/// </para>
/// <para>
/// Konuşma penceresi açıkken işaretlenen mesajlar listeden düştüğü için
/// bildirim kendiliğinden susar: kullanıcı zaten okuduğu için uyarılmaz.
/// </para>
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageUnreadSummaryQuery : IRequest<Result<List<UnreadMessageGroupDto>>>;

/// <summary>Bir gönderenden bekleyen mesajlar.</summary>
public sealed class UnreadMessageGroupDto
{
    public Guid SenderId { get; set; }

    public string SenderFullName { get; set; } = string.Empty;

    public int UnreadCount { get; set; }

    /// <summary>Son mesajın önizlemesi; bildirimde gösterilir.</summary>
    public string Preview { get; set; } = string.Empty;

    public DateTimeOffset LastSentAt { get; set; }
}

internal sealed class MessageUnreadSummaryQueryHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    IClaimContext claimContext) : IRequestHandler<MessageUnreadSummaryQuery, Result<List<UnreadMessageGroupDto>>>
{
    private const int PreviewLength = 80;

    public async Task<Result<List<UnreadMessageGroupDto>>> Handle(
        MessageUnreadSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());

        List<UserMessage> unread = await messageRepository.GetUnreadAsync(me, cancellationToken);

        if (unread.Count == 0)
        {
            return new List<UnreadMessageGroupDto>();
        }

        // Gelen kutusu tarihe göre yeniden eskiye sıralıdır; grup içindeki ilk
        // satır en yeni mesajdır.
        var groups = unread
            .GroupBy(m => m.SenderId.Value)
            .ToList();

        var senderIds = groups.Select(g => new IdentityId(g.Key)).ToArray();

        var names = await userRepository
            .Where(u => senderIds.Contains(u.Id))
            .Select(u => new
            {
                Id = u.Id.Value,
                FullName = u.FirstName.Value + " " + u.LastName.Value
            })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, string> nameById = names.ToDictionary(u => u.Id, u => u.FullName);

        List<UnreadMessageGroupDto> result = new(groups.Count);

        foreach (var group in groups)
        {
            if (!nameById.TryGetValue(group.Key, out string? senderName))
            {
                // Gönderen kullanıcı sonradan silinmiş olabilir. Mesajı yine de
                // göstermek gerekir; adı "Silinmiş kullanıcı" olarak yazılır.
                senderName = "Silinmiş kullanıcı";
            }

            UserMessage newest = group.First();

            result.Add(new UnreadMessageGroupDto
            {
                SenderId = group.Key,
                SenderFullName = senderName,
                UnreadCount = group.Count(),
                Preview = Preview(newest.Body.Value),
                LastSentAt = newest.CreatedAt
            });
        }

        return result;
    }

    private static string Preview(string body)
    {
        string single = body.ReplaceLineEndings(" ").Trim();

        return single.Length <= PreviewLength
            ? single
            : single[..PreviewLength] + "...";
    }
}