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

/// <summary>Kullanıcının konuşma listesi (gelen kutusu).</summary>
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

    /// <summary>Bu konuşmada okunmamış mesaj sayısı.</summary>
    public int UnreadCount { get; set; }

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

        var inbox = await messageRepository.GetInboxAsync(me, cancellationToken);

        if (inbox.Count == 0)
        {
            return new List<ConversationDto>();
        }

        // Karşı tarafların kimliği tek sorguda çözülür.
        var counterpartIds = inbox.Select(m => m.SenderId).Distinct().ToArray();

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

        // Konuşma başına gruplama: aynı gönderenden gelen mesajlar tek listede
        // birleşir. Duyurular ayrı kanal olarak tutulur, çünkü gönderenle
        // birebir yazışma ile herkese duyuru farklı bir konuşmadır.
        var conversations = new List<ConversationDto>();

        foreach (var group in inbox.GroupBy(m => m.SenderId))
        {
            List<UserMessage> ordered = group
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id.Value)
                .ToList();

            UserMessage last = ordered[0];

            bool isAnnouncement = ordered.All(m => m.IsAnnouncement);

            if (!users.TryGetValue(group.Key.Value, out var counterpart))
            {
                continue;
            }

            conversations.Add(new ConversationDto
            {
                CounterpartId = counterpart.Id.Value,
                CounterpartFullName = counterpart.FullName,
                CounterpartRegistryNumber = counterpart.RegistryNumber,
                CounterpartTcNo = string.IsNullOrWhiteSpace(counterpart.TcNo) ? null : counterpart.TcNo,
                CounterpartUserName = counterpart.UserName,
                CounterpartCompanyName = companyNames.GetValueOrDefault(counterpart.CompanyId.Value),
                LastSubject = last.Subject?.Value,
                LastPreview = Preview(last.Body.Value),
                LastMessageId = last.Id.Value,
                LastMessageAt = last.CreatedAt,
                UnreadCount = ordered.Count(m => m.ReadState.Value == false),
                IsAnnouncementChannel = isAnnouncement
            });
        }

        return conversations
            .OrderByDescending(c => c.LastMessageAt)
            .ThenByDescending(c => c.LastMessageId)
            .ToList();
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
