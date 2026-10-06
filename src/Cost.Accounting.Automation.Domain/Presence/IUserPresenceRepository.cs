using Cost.Accounting.Automation.Domain.Abstractions;
using GenericRepository;

namespace Cost.Accounting.Automation.Domain.Presence;

/// <summary>
/// Çevrimiçi kullanıcı bilgisinin okunduğu ve yazıldığı depo.
/// </summary>
/// <remarks>
/// <para>
/// Kullanıcılarla aynı veritabanında yaşar (master) ve kullanıcı başına tek
/// satır tutar. Bu yüzden tablo küçüktür ve her istemcinin periyodik olarak
/// okuması güvenlidir.
/// </para>
/// </remarks>
public interface IUserPresenceRepository : IRepository<UserPresence>
{
    /// <summary>Kullanıcının mevcut satırını bulur; yoksa <c>null</c>.</summary>
    Task<UserPresence?> FindByUserIdAsync(
        IdentityId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yakın zamanda güncellenmiş tüm satırları döndürür. "Yakın zamanda"
    /// eşiği <see cref="UserPresence.OnlineWindow"/> ve
    /// <see cref="UserPresence.ClosedSessionRetention"/> değerlerinden büyük
    /// olmalıdır; aksi hâlde uzun süredir görünmeyen kullanıcıların son
    /// görülme bilgisi kaybolur.
    /// </summary>
    Task<List<UserPresence>> ListSinceAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Çevrimiçi kullanıcı sayısını döndürür. Kullanıcı listesinin tamamı
    /// çekilmeden rozet güncellenmek istendiğinde kullanılır.
    /// </summary>
    Task<int> CountOnlineAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saklama süresini aşmış, kapalı oturum satırlarını siler.
    /// </summary>
    /// <returns>Silinen satır sayısı.</returns>
    Task<int> PurgeClosedSessionsAsync(
        DateTimeOffset olderThan,
        CancellationToken cancellationToken = default);
}