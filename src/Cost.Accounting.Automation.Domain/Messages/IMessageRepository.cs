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
    /// İki kullanıcı arasındaki mesaj geçmişi (her iki yön).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Gönderilen ve alınan mesajlar birlikte, tarih sırasına göre döner.
    /// <paramref name="announcementScope"/> <c>true</c> ise karşılıklı tüm mesajlar
    /// yerine yalnızca karşı tarafın gönderdiği duyurular döner; böylece bir
    /// kullanıcının kendi duyuruları kendi konuşma penceresinde tek bir başlık
    /// altında toplanır.
    /// </para>
    /// <para>
    /// <b>Sayfalama:</b> <paramref name="limit"/>&#160;&gt;&#160;0 ise yalnızca en
    /// yeni o kadar satır döner (eskiler değil) ve sonuç eskiden yeniye
    /// sıralanır. <paramref name="before"/> verilirse yalnızca o andan önceki
    /// mesajlar döner; böylece "daha eskilerini yükle" aynı pencereden devam
    /// eder. <paramref name="limit"/>&#160;=&#160;0 (varsayılan) tüm geçmişi döner.
    /// </para>
    /// <para>
    /// <paramref name="visibleAfter"/> verilirse yalnızca o andan sonraki
    /// mesajlar döner; bu, kullanıcının kendi konuşma görünümünü temizleme
    /// (bkz. <see cref="ConversationClear"/>) filtresidir.
    /// </para>
    /// </remarks>
    Task<List<UserMessage>> GetConversationAsync(
        IdentityId currentUserId,
        IdentityId counterpartId,
        bool announcementScope = false,
        int limit = 0,
        DateTimeOffset? before = null,
        DateTimeOffset? visibleAfter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının bu konuşma için temizleme (görünüm gizleme) anını döndürür.
    /// </summary>
    /// <returns>Temizleme yoksa <c>null</c>.</returns>
    Task<DateTimeOffset?> GetClearedAtAsync(
        IdentityId userId,
        IdentityId counterpartId,
        bool announcementScope = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının bu konuşma için temizleme anını yazar ya da kaldırır.
    /// </summary>
    /// <remarks>
    /// Yalnızca <paramref name="userId"/>ye ait görünüm tercihini değiştirir;
    /// mesajlara ve karşı tarafa dokunmaz. <paramref name="clearedAt"/>
    /// <c>null</c> ise satır fiziksel olarak silinir (geçmiş yeniden görünür).
    /// </remarks>
    Task SetClearedAsync(
        IdentityId userId,
        IdentityId counterpartId,
        bool announcementScope,
        DateTimeOffset? clearedAt,
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

    /// <summary>
    /// Mesaj gövdesini yalnızca sahibinin değiştirmesine izin verir.
    /// </summary>
    /// <remarks>
    /// Duyuru mesajları kapsam dışındadır: bir duyuru N alıcıya N satır olarak
    /// yazıldığı için tek satırın düzenlenmesi yalnızca o alıcının kopyasını
    /// değiştirirdi.
    /// </remarks>
    /// <returns>
    /// Yeni düzenleme damgası; mesaj yoksa, sahibi değilse ya da duyuru ise
    /// <c>null</c>.
    /// </returns>
    Task<DateTimeOffset?> EditBodyAsync(
        Guid messageId,
        IdentityId senderId,
        string newBody,
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
