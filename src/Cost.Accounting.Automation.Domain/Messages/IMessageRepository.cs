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
    /// Kullanıcının <b>hem gönderdiği hem aldığı</b> mesajları.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Konuşma listesi bu yöntemle kurulur. Yalnızca gelen kutusu okunursa,
    /// kullanıcı birine ilk mesajı gönderdiğinde listede hiçbir şey görünmez;
    /// karşı taraf cevap verene kadar da görünmez. Oysa sohbet uygulamalarında
    /// yazdığın kişi listede <b>anında</b> belirir.
    /// </para>
    /// <para>
    /// Alıcı denetimi burada yapılır: dönen satırların tamamı ya kullanıcının
    /// gönderdiği ya da kullanıcının aldığı mesajlardır.
    /// </para>
    /// </remarks>
    Task<List<UserMessage>> GetConversationsAsync(
        IdentityId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının okunmamış mesajlarını gönderene göre döndürür.
    /// </summary>
    /// <remarks>
    /// Bildirim üretmek için gereken bilgidir: "kime kaç mesaj geldi?" sorusu
    /// sayıdan değil, gönderenin kimliğinden çıkar. Tüm gelen kutusu değil
    /// yalnızca okunmamış satırlar okunur; kullanıcının yıllarca eski mesajı
    /// bu sorguyu ağırlaştırmamalıdır.
    /// </remarks>
    Task<List<UserMessage>> GetUnreadAsync(
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

    /// <summary>
    /// Karşı tarafın bana gönderdiği, henüz okunmamış mesajların tamamını okundu
    /// işaretler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tek tek mesaj işaretlemek yerine konuşmanın tamamı işaretlenir: kullanıcı
    /// konuşma penceresini açtığında geçmişin tamamını görüyor olmalıdır.
    /// </para>
    /// <para>
    /// Alıcı denetimi burada yapılır; yalnızca <paramref name="recipientId"/>
    /// alanına sahip mesajlar değişir. Başkasının okunmamış mesajı işaretlenemez.
    /// </para>
    /// </remarks>
    /// <returns>Okundu işaretlenen mesaj sayısı.</returns>
    Task<int> MarkConversationAsReadAsync(
        IdentityId recipientId,
        IdentityId counterpartId,
        bool announcementScope = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Karşı tarafın bana gönderdiği, henüz teslim edilmemiş mesajları teslim
    /// edilmiş olarak işaretler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Teslim" ile "okundu" farklıdır: teslim, mesajın alıcının ekranında
    /// göründüğü andır; okundu ise konuşmanın açıldığı andır. Gönderen
    /// kullanıcının gönderdiği mesajlar <b>bu yöntemle işaretlenmez</b>, çünkü
    /// onların teslim durumunu yalnızca alıcının istemcisi belirleyebilir.
    /// </para>
    /// <para>
    /// Zaten okunmuş mesajlar atlanır: okunma teslimin üstündedir.
    /// </para>
    /// </remarks>
    /// <returns>Teslim edilmiş işaretlenen mesaj sayısı.</returns>
    Task<int> MarkConversationAsDeliveredAsync(
        IdentityId recipientId,
        IdentityId counterpartId,
        bool announcementScope = false,
        CancellationToken cancellationToken = default);
}
