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

        // Yalnizca CreatedAt ile siralanir. Id (Guid v7) uzerinden bir esitlik
        // kirici eklenmez: SQL Server uniqueidentifier degerlerini kronolojik
        // degil, ic bayt siralamasiyla karsilastirir; Guid v7 olsa bile bu
        // siralama zaman sirasi vermez ve konusma gecmisiyanlis gosterilir.
        // CreatedAt datetimeoffset(7) cozunurlugu 100 nanosaniye oldugu icin
        // iki mesajin ayni anda kaydedilmesi pratikte olusmaz.
        return await query
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
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
}
