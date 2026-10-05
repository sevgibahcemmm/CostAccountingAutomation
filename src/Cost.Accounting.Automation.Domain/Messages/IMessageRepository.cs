using Cost.Accounting.Automation.Domain.Abstractions;
using GenericRepository;

namespace Cost.Accounting.Automation.Domain.Messages;

/// <summary>
/// Mesajlaşma sorguları.
/// </summary>
/// <remarks>
/// Mesajlar master (merkezi) veritabanında tutulur ve kullanıcıyla birlikte
/// yaşar. Bu yüzden repository doğrudan <c>MasterDbContext</c> üzerine kuruludur.
/// </remarks>
public interface IMessageRepository : IRepository<UserMessage>
{
    /// <summary>
    /// Kullanıcının gelen kutusu: kendisine gelen, silinmemiş mesajlar.
    /// </summary>
    /// <param name="recipientId">Alıcı kullanıcı.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task<List<UserMessage>> GetInboxAsync(
        IdentityId recipientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// İki kullanıcı arasındaki tüm mesajların geçmişi (her iki yön).
    /// </summary>
    /// <remarks>
    /// Gönderilen ve alınan mesajlar birlikte, tarih sırasına göre döner.
    /// <paramref name="announcementScope"/> <c>true</c> ise karşılıklı tüm mesajlar
    /// yerine yalnızca karşı tarafın gönderdiği duyurular döner; böylece bir
    /// kullanıcının kendi duyuruları kendi konuşma penceresinde tek bir başlık
    /// altında toplanır.
    /// </remarks>
    Task<List<UserMessage>> GetConversationAsync(
        IdentityId currentUserId,
        IdentityId counterpartId,
        bool announcementScope = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının gönderdiği mesajlar.
    /// </summary>
    Task<List<UserMessage>> GetSentAsync(
        IdentityId senderId,
        CancellationToken cancellationToken = default);

    /// <summary>Kullanıcının okunmamış mesaj sayısı.</summary>
    Task<int> CountUnreadAsync(
        IdentityId recipientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mesajı okundu olarak işaretler.
    /// </summary>
    /// <returns>
    /// İşaretlenen mesaj yoksa <c>false</c>. Mesajın alıcısı başka bir kullanıcıysa
    /// <c>false</c> döner; yani bir kullanıcı başkasının mesajını okundu yapamaz.
    /// </returns>
    Task<bool> MarkAsReadAsync(
        Guid messageId,
        IdentityId recipientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mesajı yumuşak siler.
    /// </summary>
    /// <returns>
    /// Silinen mesaj yoksa <c>false</c>. Mesajın göndereni başka bir kullanıcıysa
    /// <c>false</c> döner.
    /// </returns>
    Task<bool> SoftDeleteAsync(
        Guid messageId,
        IdentityId senderId,
        CancellationToken cancellationToken = default);

    /// <summary>Bir kullanıcının mesaj gönderme yetkisi var mı diye bakmadan önce hedefi doğrular.</summary>
    Task<bool> AnyConversationWithAsync(
        IdentityId currentUserId,
        IdentityId counterpartId,
        CancellationToken cancellationToken = default);
}
