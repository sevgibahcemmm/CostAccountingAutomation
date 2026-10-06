using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Presence;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Domain.Presence;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>
/// Mesajlaşma ekranının sol sütununu besleyen "kullanıcı listesi".
/// </summary>
/// <remarks>
/// <para>
/// Gereken şey "konuştuğum kişiler" değil, <b>şu anda ulaşabildiğim herkes</b>dir.
/// Bu yüzden listede daha önce mesaj alışverişi olmamış kullanıcılar da yer alır;
/// onlarla konuşma listeden tek tıkla başlatılır.
/// </para>
/// <para>
/// <b>Oturum açan kullanıcının kendisi listede yoktur.</b> Kendine mesaj
/// gönderilemediği için (bkz. <c>MessageSendCommand</c>) listede görünmesi
/// yalnızca yanıltıcıdır: "Çevrimiçi" yazan bir satır tıklanıp gönder
/// düğmesine basıldığında hata vermemelidir. Filtre <b>depoda değil, burada</b>
/// uygulanır; böylece hiçbir ekran unutursa kendi satırını göstermeye çalışmaz.
/// </para>
/// <para>
/// Sorgu tek bir ekranda gösterilecek bütün alanları bir arada getirir: kim, hangi
/// kurumda, şu an çevrimiçi mi, kaç okunmamış mesajı var. Alanları ayrı ayrı
/// sorgulamak yerine tek listede birleştirmek, liste 10 saniyede bir tazelendiği
/// için veritabanına gereksiz tur atmaz.
/// </para>
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageDirectoryQuery : IRequest<Result<List<MessageDirectoryDto>>>;

/// <summary>Listede gösterilen bir kullanıcı satırı.</summary>
public sealed class MessageDirectoryDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    /// <summary>Sicil numarası; personel kaydıyla eşleştirmek için.</summary>
    public string? RegistryNumber { get; set; }

    public string? TcNo { get; set; }

    public string? CompanyName { get; set; }

    public string RoleName { get; set; } = string.Empty;

    /// <summary>Şu an uygulamada mı?</summary>
    public bool IsOnline { get; set; }

    /// <summary>Son görülme zamanı; yalnızca pasifken anlamlıdır.</summary>
    public DateTimeOffset? LastSeenAt { get; set; }

    /// <summary>Mevcut oturumun açıldığı an.</summary>
    public DateTimeOffset? SessionStartedAt { get; set; }

    /// <summary>Bu kullanıcıdan gelen okunmamış mesaj sayısı.</summary>
    public int UnreadCount { get; set; }

    /// <summary>Bu kullanıcıyla daha önce konuşuldu mu?</summary>
    public bool HasConversation { get; set; }

    /// <summary>
    /// Durumun okunabilir karşılığı. Ekranda "True/False" yerine bu metin
    /// gösterilir; yeşil ikon ayrıca çizilir.
    /// </summary>
    public string PresenceCaption => IsOnline ? "Çevrimiçi" : "Pasif";
}

internal sealed class MessageDirectoryQueryHandler(
    IUserRepository userRepository,
    ICompanyRepository companyRepository,
    IRoleRepository roleRepository,
    IMessageRepository messageRepository,
    IUserPresenceRepository presenceRepository,
    IClaimContext claimContext) : IRequestHandler<MessageDirectoryQuery, Result<List<MessageDirectoryDto>>>
{
    /// <summary>
    /// Listeye çevrilmeden önce kullanıcının okunacak alanları.
    /// </summary>
    /// <remarks>
    /// <c>IdentityId</c> alanları <see cref="Guid"/> olarak düzleştirilir:
    /// Contains filtresi model tipini kullanmalıdır, <c>Guid</c> listesi verilirse
    /// EF değer dönüştürücüsünü atlar ve ifade SQL'e çevrilemez.
    /// </remarks>
    private sealed record ContactProjection(
        Guid Id,
        string FullName,
        string UserName,
        string? RegistryNumber,
        string? TcNo,
        Guid CompanyId,
        Guid RoleId);

    public async Task<Result<List<MessageDirectoryDto>>> Handle(
        MessageDirectoryQuery request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());

        DateTimeOffset now = DateTimeOffset.Now;

        // Yalnızca hesabı açık olanlar listelenir. "Pasif" burada oturum
        // durumunu ifade eder; kapalı hesap hiç görünmez.
        //
        // Kullanıcılar projeksiyonla okunur: liste on saniyede bir yenilendiği
        // için tam varlık yüklemek (parola hash'i dâhil) her turda gereksiz
        // bellek ve veritabanı trafiği demektir.
        List<ContactProjection> users = await userRepository
            .Where(u => u.IsActive)

            // Kendimiz listede yokuz. Sorgunun en başına konur: kullanıcı
            // adına göre filtre uygulamak, sonra listeden çıkarmaktan daha
            // açıktır ve ileride "kendimizi unutma" hatasına yol açmaz.
            .Where(u => u.Id != me)
            .Select(u => new ContactProjection(
                u.Id.Value,
                u.FirstName.Value + " " + u.LastName.Value,
                u.UserName.Value,
                u.RegistryNumber,
                u.TRIdentityNumber!.Value,
                u.CompanyId.Value,
                u.RoleId.Value))
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            return new List<MessageDirectoryDto>();
        }

        Dictionary<Guid, UserPresenceStateDto> states = await LoadPresenceAsync(cancellationToken);

        // Gelen kutusu tek kez okunur; hem okunmamış sayısı hem "daha önce
        // konuştuk" bilgisi buradan çıkarılır. İki ayrı sorgu aynı satırları
        // defalarca taşırdı.
        List<UserMessage> inbox = await messageRepository.GetInboxAsync(me, cancellationToken);

        Dictionary<Guid, int> unread = CountUnreadBySender(inbox);
        Dictionary<Guid, DateTimeOffset> lastConversations = LastConversationTimesByCounterpart(inbox);

        Dictionary<Guid, string> companyNames = await LoadCompanyNamesAsync(users, cancellationToken);
        Dictionary<Guid, string> roleNames = await LoadRoleNamesAsync(users, cancellationToken);

        List<MessageDirectoryDto> rows = users.Select(u =>
        {
            states.TryGetValue(u.Id, out UserPresenceStateDto? state);
            lastConversations.TryGetValue(u.Id, out DateTimeOffset lastMessageAt);
            unread.TryGetValue(u.Id, out int unreadCount);

            bool isOnline = state is not null && state.IsOnline;

            return new MessageDirectoryDto
            {
                UserId = u.Id,
                FullName = u.FullName,
                UserName = u.UserName,
                RegistryNumber = string.IsNullOrWhiteSpace(u.RegistryNumber) ? null : u.RegistryNumber,
                TcNo = string.IsNullOrWhiteSpace(u.TcNo) ? null : u.TcNo,
                CompanyName = companyNames.GetValueOrDefault(u.CompanyId),
                RoleName = roleNames.GetValueOrDefault(u.RoleId, string.Empty),
                IsOnline = isOnline,
                LastSeenAt = state?.LastSeenAt,
                SessionStartedAt = isOnline ? state?.SessionStartedAt : null,
                UnreadCount = unreadCount,
                HasConversation = lastMessageAt != default
            };
        }).ToList();

        // Çevrimiçiler önce: kullanıcının aradığı kişi büyük olasılıkla
        // ulaşmak istediği kişidir.
        return rows
            .OrderByDescending(r => r.IsOnline)
            .ThenByDescending(r => r.UnreadCount > 0)
            .ThenBy(r => r.FullName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private async Task<Dictionary<Guid, UserPresenceStateDto>> LoadPresenceAsync(
        CancellationToken cancellationToken)
    {
        List<UserPresence> rows = await presenceRepository.ListSinceAsync(
            DateTimeOffset.Now - UserPresence.ClosedSessionRetention, cancellationToken);

        return PresenceSnapshot.ToStatesByUserId(rows, DateTimeOffset.Now);
    }

    /// <summary>Gelen kutusundaki okunmamış mesajları gönderene göre sayar.</summary>
    private static Dictionary<Guid, int> CountUnreadBySender(List<UserMessage> inbox)
        => inbox
            .Where(m => m.ReadState.Value == false)
            .GroupBy(m => m.SenderId.Value)
            .ToDictionary(g => g.Key, g => g.Count());

    /// <summary>
    /// Her karşı taraf için son konuşma zamanını verir. Bu değer "daha önce
    /// konuştuk" işareti olarak kullanılır; gelen kutusu yalnızca bana gelen
    /// mesajları içerdiği için kendi gönderdiğim mesajlar burada görünmez,
    /// yani işaret "karşı taraf bana bir şey yazmış" anlamına gelir.
    /// </summary>
    private static Dictionary<Guid, DateTimeOffset> LastConversationTimesByCounterpart(
        List<UserMessage> inbox)
    {
        Dictionary<Guid, DateTimeOffset> last = new();

        foreach (UserMessage message in inbox)
        {
            Guid counterpart = message.SenderId.Value;

            if (!last.TryGetValue(counterpart, out DateTimeOffset existing)
                || message.CreatedAt > existing)
            {
                last[counterpart] = message.CreatedAt;
            }
        }

        return last;
    }

    private async Task<Dictionary<Guid, string>> LoadCompanyNamesAsync(
        List<ContactProjection> users,
        CancellationToken cancellationToken)
    {
        var ids = users.Select(u => new IdentityId(u.CompanyId)).Distinct().ToArray();

        var companies = await companyRepository
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { Id = c.Id.Value, Name = c.Name.Value })
            .ToListAsync(cancellationToken);

        return companies.ToDictionary(c => c.Id, c => c.Name);
    }

    private async Task<Dictionary<Guid, string>> LoadRoleNamesAsync(
        List<ContactProjection> users,
        CancellationToken cancellationToken)
    {
        var ids = users.Select(u => new IdentityId(u.RoleId)).Distinct().ToArray();

        var roles = await roleRepository
            .Where(r => ids.Contains(r.Id))
            .Select(r => new { Id = r.Id.Value, Name = r.Name.Value })
            .ToListAsync(cancellationToken);

        return roles.ToDictionary(r => r.Id, r => r.Name);
    }
}