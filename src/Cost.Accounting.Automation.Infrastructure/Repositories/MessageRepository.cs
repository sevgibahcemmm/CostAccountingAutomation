using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

/// <summary>
/// Mesaj sorguları; master (merkezi) veritabanı üzerinde çalışır.
/// </summary>
/// <remarks>
/// <para>
/// Taban sınıf <see cref="MasterAuditableRepository{TEntity}"/>'dir: mesajlar master
/// veritabanında yaşar, kullanıcılar da orada olduğu için denetim birleştirmesi
/// ayrı bir sorguya ihtiyaç duymaz. Bu taban ayrıca korumalı
/// <c>Context</c> özelliğini sağlar; ham <c>Set&lt;T&gt;()</c> erişimi gerekiyor.
/// </para>
/// <para>
/// Tüm "kim görebilir" kararları burada verilir: <c>GetInboxAsync</c> yalnızca
/// bana gelenleri, <c>GetConversationAsync</c> yalnızca benimle karşı taraf
/// arasındakileri döndürür. Uygulama katmanı bir kimlik uydurursa başka birinin
/// mesajını okuyabilir; bu yüzden filtre <b>depo tarafında</b> uygulanır ve
/// çağıranın verdiği kimlik sorguya parametre olarak geçer.
/// </para>
/// <para>
/// Global sorgu filtresi (<c>IsDeleted = false</c>) zaten uygulandığı için silinmiş
/// mesajlar hiçbir listede görünmez.
/// </para>
/// </remarks>
internal sealed class MessageRepository : MasterAuditableRepository<UserMessage>, IMessageRepository
{
    public MessageRepository(MasterDbContext context) : base(context)
    {
    }

    public async Task<List<UserMessage>> GetInboxAsync(
        IdentityId recipientId,
        CancellationToken cancellationToken = default)
        => await this.Context.Set<UserMessage>()
            .AsNoTracking()
            .Where(m => m.RecipientId == recipientId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<UserMessage>> GetConversationAsync(
        IdentityId currentUserId,
        IdentityId counterpartId,
        bool announcementScope = false,
        int limit = 0,
        DateTimeOffset? before = null,
        DateTimeOffset? visibleAfter = null,
        CancellationToken cancellationToken = default)
    {
        var messages = this.Context.Set<UserMessage>().AsNoTracking();

        // Duyuru kanalında yalnızca karşı tarafın duyuruları listelenir. Kendi
        // duyurunuzdan sonra gelen bir özel mesaj aynı başlık altına karışmasın.
        var query = announcementScope
            ? messages.Where(m => m.SenderId == counterpartId && m.RecipientId == currentUserId)
            : messages.Where(m =>
                (m.SenderId == currentUserId && m.RecipientId == counterpartId)
                || (m.SenderId == counterpartId && m.RecipientId == currentUserId));

        // Kullanıcının kendi temizlediği görünüm: temizleme anından önceki
        // mesajlar bu kullanıcıya gösterilmez (bkz. ConversationClear).
        if (visibleAfter is { } visibleFrom)
        {
            query = query.Where(m => m.CreatedAt > visibleFrom);
        }

        // "Daha eskilerini yükle": istenen andan önceki mesajlar.
        if (before is { } cutoff)
        {
            query = query.Where(m => m.CreatedAt < cutoff);
        }

        if (limit <= 0)
        {
            // Yalnizca CreatedAt ile siralanir. Id (Guid v7) uzerinden bir esitlik
            // kirici eklenmez: SQL Server uniqueidentifier degerlerini kronolojik
            // degil, ic bayt siralamasiyla karsilastirir; siralama yanlis olur.
            // CreatedAt datetimeoffset(7) cozunurlugu 100 nanosaniye oldugu icin
            // iki mesajin ayni anda kaydedilmesi pratikte olusmaz.
            return await query
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // En yeni N satır alınır, sonra ters çevrilerek eskiden yeniye verilir;
        // böylece hem son mesajlar yüklenir hem de baloncuk sırası korunur.
        List<UserMessage> page = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        page.Reverse();

        return page;
    }

    public async Task<DateTimeOffset?> GetClearedAtAsync(
        IdentityId userId,
        IdentityId counterpartId,
        bool announcementScope = false,
        CancellationToken cancellationToken = default)
    {
        ConversationClear? row = await this.Context.Set<ConversationClear>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.UserId == userId
                     && c.CounterpartId == counterpartId
                     && c.IsAnnouncementChannel == announcementScope,
                cancellationToken);

        return row?.ClearedAt;
    }

    public async Task SetClearedAsync(
        IdentityId userId,
        IdentityId counterpartId,
        bool announcementScope,
        DateTimeOffset? clearedAt,
        CancellationToken cancellationToken = default)
    {
        var set = this.Context.Set<ConversationClear>();

        ConversationClear? existing = await set.FirstOrDefaultAsync(
            c => c.UserId == userId
                 && c.CounterpartId == counterpartId
                 && c.IsAnnouncementChannel == announcementScope,
            cancellationToken);

        if (clearedAt is null)
        {
            // Geçmiş yeniden gösterilecek: satır fiziksel olarak kalkar. Bu satırlar
            // yumuşak silinemez — özindeks (kullanıcı + karşı taraf + kanal) eski
            // satırla çakışırdı ve yeni temizleme kaydı engellenirdi.
            if (existing is not null)
            {
                set.Remove(existing);
            }
        }
        else if (existing is null)
        {
            await set.AddAsync(
                new ConversationClear(userId, counterpartId, announcementScope, clearedAt.Value),
                cancellationToken);
        }
        else
        {
            existing.SetClearedAt(clearedAt.Value);
        }

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserMessage>> GetSentAsync(
        IdentityId senderId,
        CancellationToken cancellationToken = default)
        => await this.Context.Set<UserMessage>()
            .AsNoTracking()
            .Where(m => m.SenderId == senderId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountUnreadAsync(
        IdentityId recipientId,
        CancellationToken cancellationToken = default)
        => this.Context.Set<UserMessage>()
            .AsNoTracking()
            .CountAsync(m => m.RecipientId == recipientId && m.ReadState.Value == false, cancellationToken);

    public Task<List<UserMessage>> GetUnreadAsync(
        IdentityId recipientId,
        CancellationToken cancellationToken = default)
        => this.Context.Set<UserMessage>()
            .AsNoTracking()
            .Where(m => m.RecipientId == recipientId && m.ReadState.Value == false)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<List<UserMessage>> GetConversationsAsync(
        IdentityId userId,
        CancellationToken cancellationToken = default)
        => this.Context.Set<UserMessage>()
            .AsNoTracking()
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> MarkAsReadAsync(
        Guid messageId,
        IdentityId recipientId,
        CancellationToken cancellationToken = default)
    {
        var message = await this.Context.Set<UserMessage>()
            .FirstOrDefaultAsync(m => m.Id == new IdentityId(messageId), cancellationToken);

        // Alıcı denetimi burada: başkasının mesajını okundu yapılamaz.
        if (message is null || message.RecipientId.Value != recipientId.Value)
        {
            return false;
        }

        message.MarkAsRead(DateTimeOffset.Now);
        await Context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SoftDeleteAsync(
        Guid messageId,
        IdentityId senderId,
        CancellationToken cancellationToken = default)
    {
        var message = await this.Context.Set<UserMessage>()
            .FirstOrDefaultAsync(m => m.Id == new IdentityId(messageId), cancellationToken);

        // Gönderen denetimi: yalnızca mesajı gönderen kişi silebilir.
        if (message is null || message.SenderId.Value != senderId.Value)
        {
            return false;
        }

        message.MarkAsDeleted();
        await Context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<DateTimeOffset?> EditBodyAsync(
        Guid messageId,
        IdentityId senderId,
        string newBody,
        CancellationToken cancellationToken = default)
    {
        var message = await this.Context.Set<UserMessage>()
            .FirstOrDefaultAsync(m => m.Id == new IdentityId(messageId), cancellationToken);

        // Gönderen denetimi burada da uygulanır: arayüzden gelen istek
        // başkasının mesajını değiştiremez. Duyurular (N satır) kapalıdır.
        if (message is null
            || message.SenderId.Value != senderId.Value
            || message.IsAnnouncement)
        {
            return null;
        }

        DateTimeOffset editedAt = DateTimeOffset.Now;
        message.EditBody(new MessageBody(newBody), editedAt);
        await Context.SaveChangesAsync(cancellationToken);

        return editedAt;
    }

    public Task<bool> AnyConversationWithAsync(
        IdentityId currentUserId,
        IdentityId counterpartId,
        CancellationToken cancellationToken = default)
        => this.Context.Set<UserMessage>()
            .AsNoTracking()
            .AnyAsync(
                m => (m.SenderId == currentUserId && m.RecipientId == counterpartId)
                     || (m.SenderId == counterpartId && m.RecipientId == currentUserId),
                cancellationToken);

    /// <summary>
    /// Karşı tarafın bana gönderdiği okunmamış mesajları okundu sayar ve
    /// işaretlenen satır sayısını döndürür.
    /// </summary>
    /// <remarks>
    /// Yalnızca <b>bana gelen</b> mesajlar işaretlenir; gönderdiğim mesajların
    /// okunma durumunu yalnızca alıcının istemcisi yazabilir. İki yönü birden
    /// işaretlemek, kendi gönderdiğim mesajı anında "okundu" göstermenin
    /// ötesinde şu hataya da yol açar: gönderdiğim satır aynı zamanda alıcının
    /// okunmamış kutusundaki satırdır; onu okundu yapınca alıcı tarafında hiç
    /// bildirim tetiklenmez.
    /// </remarks>
    public async Task<int> MarkConversationAsReadAsync(
        IdentityId recipientId,
        IdentityId counterpartId,
        bool announcementScope = false,
        CancellationToken cancellationToken = default)
    {
        // Kanal ayrımı burada gerekmez: duyuru da karşı taraftan bana gelen bir
        // satırdır ve okunma durumu aynı biçimde belirlenir.
        List<UserMessage> unread = await this.Context.Set<UserMessage>()
            .Where(m => m.SenderId == counterpartId
                && m.RecipientId == recipientId
                && m.ReadState.Value == false)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return 0;
        }

        DateTimeOffset now = DateTimeOffset.Now;

        foreach (UserMessage message in unread)
        {
            message.MarkAsRead(now);
        }

        await Context.SaveChangesAsync(cancellationToken);

        return unread.Count;
    }

    public async Task<int> MarkConversationAsDeliveredAsync(
        IdentityId recipientId,
        IdentityId counterpartId,
        bool announcementScope = false,
        CancellationToken cancellationToken = default)
    {
        // Yalnızca bana gelenler işaretlenir; gönderdiğim mesajların teslim
        // durumunu alıcının istemcisi belirler, benim tarafım yazamaz.
        //
        // Kanal ayrımı burada gerekmez: duyuru da karşı taraftan bana gelen bir
        // satırdır ve teslim durumu aynı biçimde belirlenir. Her iki kanal için
        // aynı sorgu doğrudur.
        List<UserMessage> pending = await this.Context.Set<UserMessage>()
            .Where(m => m.SenderId == counterpartId
                && m.RecipientId == recipientId
                && m.DeliveredAt == null
                && m.ReadState.Value == false)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return 0;
        }

        DateTimeOffset now = DateTimeOffset.Now;

        foreach (UserMessage message in pending)
        {
            message.MarkDelivered(now);
        }

        await Context.SaveChangesAsync(cancellationToken);

        return pending.Count;
    }
}
