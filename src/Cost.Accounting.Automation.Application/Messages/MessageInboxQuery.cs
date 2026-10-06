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

/// <summary>Kullanıcının konuşma listesi (gelen + giden).</summary>
/// <remarks>
/// <para>
/// Konuşma listesi <b>her iki yönü</b> kapsar. Yalnızca gelen kutusu okunursa
/// kullanıcı birine ilk mesajı gönderdiğinde listede hiçbir şey görünmez ve
/// karşı taraf cevap verene kadar da görünmez. Sohbet uygulamalarında yazdığın
/// kişi <b>anında</b> listede belirir; son mesajın önizlemesi de her iki yönden
/// gelen en güncel satırdır.
/// </para>
/// <para>
/// Okunmamış sayısı yalnızca <b>alınan</b> mesajları sayar. Gönderdiğiniz
/// mesajlar okunmamış sayısına girmez.
/// </para>
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageInboxQuery : IRequest<Result<List<ConversationDto>>>;

/// <summary>Bir karşı tarafla sürdürülen konuşmanın özeti.</summary>
public sealed class ConversationDto
{
    /// <summary>Karşı tarafın kullanıcı kimliği.</summary>
    public Guid CounterpartId { get; set; }

    public string CounterpartFullName { get; set; } = string.Empty;

    /// <summary>Sicil numarası; eşleştirmek için kullanıcıya gösterilir.</summary>
    public string? CounterpartRegistryNumber { get; set; }

    public string? CounterpartTcNo { get; set; }

    public string? CounterpartUserName { get; set; }

    /// <summary>Karşı tarafın kurum adı.</summary>
    public string? CounterpartCompanyName { get; set; }

    /// <summary>Son mesajın başlığı.</summary>
    public string? LastSubject { get; set; }

    /// <summary>Son mesajın kısaltılmış önizlemesi.</summary>
    public string LastPreview { get; set; } = string.Empty;

    public Guid LastMessageId { get; set; }

    public DateTimeOffset LastMessageAt { get; set; }

    /// <summary>Bu konuşmada bana gelen okunmamış mesaj sayısı.</summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// Son mesajı <b>ben</b> mi gönderdim? Önizlemede "siz:" ön eki ve
    /// teslim/okunma tiki için gereklidir.
    /// </summary>
    public bool LastMessageIsMine { get; set; }

    /// <summary>
    /// Son mesaj teslim edildi mi? Yalnızca benim gönderdiğim mesajlar için
    /// anlamlıdır.
    /// </summary>
    public bool LastMessageDelivered { get; set; }

    /// <summary>Son mesaj okundu mu? (yalnızca benim gönderdiğim mesaj için)</summary>
    public bool LastMessageRead { get; set; }

    /// <summary>Karşı tarafın duyuruları mı? Duyurular ayrı başlıkta toplanır.</summary>
    public bool IsAnnouncementChannel { get; set; }

    /// <summary>
    /// Kanalın okunabilir adı ("Duyuru Kanalı" / "Kişisel Konuşmalar").
    /// </summary>
    /// <remarks>
    /// Liste ekranı bu metni gruplama anahtarı olarak kullanır. Boolean bir
    /// alanla gruplama yapılsaydı grup başlığı ham "True/False" olarak görünürdü.
    /// </remarks>
    public string ChannelCaption => IsAnnouncementChannel
        ? "Duyuru Kanalı"
        : "Kişisel Konuşmalar";
}

internal sealed class MessageInboxQueryHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    ICompanyRepository companyRepository,
    IClaimContext claimContext) : IRequestHandler<MessageInboxQuery, Result<List<ConversationDto>>>
{
    /// <summary>Son mesaj önizlemesinde gösterilecek en fazla karakter.</summary>
    private const int PreviewLength = 120;

    public async Task<Result<List<ConversationDto>>> Handle(
        MessageInboxQuery request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());

        // Gelen + giden birlikte. Gelen kutusu tek başına kullanılırsa kullanıcı
        // ilk mesajı gönderdiği kişiyi listede göremez.
        var conversation = await messageRepository.GetConversationsAsync(me, cancellationToken);

        if (conversation.Count == 0)
        {
            return new List<ConversationDto>();
        }

        // Karşı tarafların kimliği tek sorguda çözülür.
        var counterpartIds = conversation
            .Select(m => m.SenderId.Value == me.Value ? m.RecipientId : m.SenderId)
            .Distinct()
            .ToArray();

        var counterpartNames = await userRepository
            .Where(u => counterpartIds.Contains(u.Id))
            .Select(u => new
            {
                Id = u.Id,
                FullName = u.FirstName.Value + " " + u.LastName.Value,
                RegistryNumber = u.RegistryNumber,
                TcNo = u.TRIdentityNumber!.Value,
                UserName = u.UserName.Value,
                CompanyId = u.CompanyId
            })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, string> companyNames = await LoadCompanyNamesAsync(
            counterpartNames.Select(c => c.CompanyId.Value),
            cancellationToken);

        var users = counterpartNames.ToDictionary(u => u.Id.Value);

        List<ConversationDto> conversations = new();

        // Konuşma başına gruplama: aynı karşı tarafla paylaşılan mesajlar tek
        // satırda birleşir. Duyurular ayrı kanal olarak tutulur, çünkü yönetici
        // duyuruları ile birebir yazışma farklı bir konuşmadır.
        foreach (var group in conversation.GroupBy(m => m.SenderId.Value == me.Value
            ? m.RecipientId.Value
            : m.SenderId.Value))
        {
            if (!users.TryGetValue(group.Key, out var counterpart))
            {
                continue;
            }

            List<UserMessage> ordered = group
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id.Value)
                .ToList();

            UserMessage last = ordered[0];

            // Aynı kişiye hem duyuru hem özel mesaj gitmiş olabilir. İkisi tek
            // başlık altında birleşirse duyurular kişisel konuşma kanalında
            // kaybolur; bu yüzden kanal başına ayrı satır üretilir.
            foreach (var channel in SplitByChannel(ordered))
            {
                UserMessage channelLast = channel[0];

                conversations.Add(new ConversationDto
                {
                    CounterpartId = counterpart.Id.Value,
                    CounterpartFullName = counterpart.FullName,
                    CounterpartRegistryNumber = counterpart.RegistryNumber,
                    CounterpartTcNo = string.IsNullOrWhiteSpace(counterpart.TcNo) ? null : counterpart.TcNo,
                    CounterpartUserName = counterpart.UserName,
                    CounterpartCompanyName = companyNames.GetValueOrDefault(counterpart.CompanyId.Value),
                    LastSubject = channelLast.Subject?.Value,
                    LastPreview = Preview(channelLast.Body.Value),
                    LastMessageId = channelLast.Id.Value,
                    LastMessageAt = channelLast.CreatedAt,

                    // Okunmamış yalnızca bana gelenlerdir; kendi gönderdiğim
                    // mesajlar sayılmaz.
                    UnreadCount = channel.Count(m =>
                        m.RecipientId.Value == me.Value && m.ReadState.Value == false),

                    LastMessageIsMine = channelLast.SenderId.Value == me.Value,
                    LastMessageDelivered = channelLast.DeliveredAt is not null,
                    LastMessageRead = channelLast.ReadState.Value,
                    IsAnnouncementChannel = channel.All(m => m.IsAnnouncement)
                });
            }
        }

        return conversations
            .OrderByDescending(c => c.LastMessageAt)
            .ThenByDescending(c => c.LastMessageId)
            .ToList();
    }

    /// <summary>
    /// Aynı karşı tarafın mesajlarını kanala göre böler: duyuru ve kişisel
    /// mesajlar ayrı listeler hâlinde döner. Her liste yeniden eskiye sıralıdır.
    /// </summary>
    private static List<List<UserMessage>> SplitByChannel(List<UserMessage> orderedMessages)
    {
        List<UserMessage> announcements = orderedMessages.Where(m => m.IsAnnouncement).ToList();
        List<UserMessage> personal = orderedMessages.Where(m => !m.IsAnnouncement).ToList();

        List<List<UserMessage>> channels = [];

        if (personal.Count > 0)
        {
            channels.Add(personal);
        }

        if (announcements.Count > 0)
        {
            channels.Add(announcements);
        }

        return channels;
    }

    private async Task<Dictionary<Guid, string>> LoadCompanyNamesAsync(
        IEnumerable<Guid> companyIds,
        CancellationToken cancellationToken)
    {
        var ids = companyIds.Distinct().Select(id => new IdentityId(id)).ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        var companies = await companyRepository
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, Name = c.Name.Value })
            .ToListAsync(cancellationToken);

        return companies.ToDictionary(c => c.Id.Value, c => c.Name);
    }

    /// <summary>Gövdeyi tek satıra indirip kısaltır.</summary>
    private static string Preview(string body)
    {
        string single = body.ReplaceLineEndings(" ").Trim();

        return single.Length <= PreviewLength
            ? single
            : single[..PreviewLength] + "...";
    }
}
