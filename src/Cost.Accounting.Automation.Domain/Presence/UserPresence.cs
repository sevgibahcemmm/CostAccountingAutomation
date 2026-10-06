using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Presence;

/// <summary>
/// Bir kullanıcının canlı oturumunun izi: "şu an kim uygulamada?".
/// </summary>
/// <remarks>
/// <para>
/// Uygulamanın sunucu katmanı yoktur; istemciler doğrudan master veritabanına
/// bağlanır. Bu yüzden çevrimiçi bilgisi bir <b>kalp atışı</b> sütunu olarak
/// veritabanında tutulur: her istemci kendi satırını periyodik olarak tazeler,
/// diğer istemciler de aynı satırları okur. Gerçek bir anlık iletim kanalı
/// (SignalR, WebSocket) gerektirmez.
/// </para>
/// <para>
/// "Çevrimiçi" bir bayrak değil, <b>türetilmiş</b> bir durumdur: son kalp atışından
/// <see cref="OnlineWindow"/> kadar süre geçmemiş olması gerekir. Böylece istemci
/// çökerse ya da kapanırsa satır veritabanında "açık" kalmaz; pencere aşılınca
/// kullanıcı kendiliğinden <b>Pasif</b> görünür. Bir istemci kapanırken ayrıca
/// <see cref="SignOut"/> çağırır; o zaman diğer istemciler kapanışı beklemeden
/// görür.
/// </para>
/// <para>
/// Aynı kullanıcı birden çok makinede çalışabilir. Satır kullanıcı başına tek
/// olduğu için bu durum <see cref="SessionId"/> ile ayrılır: kalp atışı yazan
/// oturum kimliğini yazar, çıkış yalnızca <b>kendi oturumu</b> için geçerlidir.
/// Bir pencereden çıkmak diğer pencerenin oturumunu düşürmez.
/// </para>
/// </remarks>
public sealed class UserPresence
{
    /// <summary>
    /// Son kalp atışından bu kadar süre geçmemişse kullanıcı çevrimiçi sayılır.
    /// </summary>
    /// <remarks>
    /// Değer kalp atışı aralığının üç katıdır. İki aralık boyunca hiç kalp atışı
    /// gelmezse ağ ya da veritabanı bağlantısında sorun vardır; üçüncüsünde de
    /// gelmezse kullanıcı gerçekten çıkmış sayılır. Tek bir kaybolan kalp atışı
    /// (kısa bir ağ kesintisi) yanlışlıkla "Pasif" göstermemelidir.
    /// </remarks>
    public static readonly TimeSpan OnlineWindow = TimeSpan.FromSeconds(75);

    /// <summary>
    /// Kapanmış oturum kayıtlarının ne kadar süre tutulacağı. Bundan eski kayıtlar
    /// "ne zaman çıkmıştı" bilgisini vermeye devam eder ama tabloyu sınırsız
    /// büyütmez.
    /// </summary>
    public static readonly TimeSpan ClosedSessionRetention = TimeSpan.FromDays(7);

    private UserPresence()
    {
    }

    /// <summary>Kullanıcı için yeni bir oturum satırı açar.</summary>
    public UserPresence(
        IdentityId userId,
        IdentityId companyId,
        Guid sessionId,
        DateTimeOffset now,
        string? machineName)
    {
        UserId = userId;
        CompanyId = companyId;
        SessionId = sessionId;
        SessionStartedAt = now;
        LastHeartbeatAt = now;
        MachineName = Normalize(machineName);
    }

    /// <summary>Kullanıcı kimliği; başlık olarak kullanılır.</summary>
    public IdentityId UserId { get; private set; } = default!;

    /// <summary>Oturumun açıldığı kurum.</summary>
    public IdentityId CompanyId { get; private set; } = default!;

    /// <summary>
    /// Satırı şu an tazeleyen oturumun kimliği. Her girişte yeni bir değer üretilir.
    /// </summary>
    public Guid SessionId { get; private set; }

    /// <summary>Bu oturumun açıldığı an.</summary>
    public DateTimeOffset SessionStartedAt { get; private set; }

    /// <summary>Son kalp atışının gönderildiği an.</summary>
    public DateTimeOffset LastHeartbeatAt { get; private set; }

    /// <summary>
    /// Oturum kapatıldığında yazılır. <c>null</c> iken oturum kapanmamıştır ya da
    /// <see cref="SessionId"/> değiştirilmiş ve yeni oturum henüz kapanmamıştır.
    /// </summary>
    public DateTimeOffset? LoggedOutAt { get; private set; }

    /// <summary>
    /// İstemcinin çalıştığı makine adı. "Şu an kim, hangi bilgisayardan çalışıyor?"
    /// sorusunu yanıtlar; güvenlik amacıyla oturum kimliğinin yerine geçmez.
    /// </summary>
    public string? MachineName { get; private set; }

    /// <summary>Bu oturumun kullanıcının ilk girişinden bu yana geçen süre.</summary>
    public TimeSpan SessionDurationAt(DateTimeOffset now) => now - SessionStartedAt;

    /// <summary>
    /// Başka bir oturum bu satırı ele geçirdiyse eski oturumun kapanmış sayılması
    /// gerekir; aksi hâlde <see cref="LoggedOutAt"/> eskisinden kalır ve yeni oturum
    /// hiç çevrimiçi görünmez.
    /// </summary>
    /// <param name="sessionId">Yeni oturumun kimliği.</param>
    /// <param name="now">Şu an.</param>
    /// <param name="machineName">Yeni oturumun makine adı.</param>
    public void BeginSession(Guid sessionId, DateTimeOffset now, string? machineName)
    {
        SessionId = sessionId;
        SessionStartedAt = now;
        LastHeartbeatAt = now;
        LoggedOutAt = null;
        MachineName = Normalize(machineName);
    }

    /// <summary>Kalp atışını tazeler; her istemci periyodik olarak çağırır.</summary>
    public void Touch(DateTimeOffset now) => LastHeartbeatAt = now;

    /// <summary>Makine adını günceller (istemci makine adını sonradan bildirebilir).</summary>
    public void SetMachineName(string? machineName) => MachineName = Normalize(machineName);

    /// <summary>
    /// Oturumu kapatır. <paramref name="sessionId"/> satırın sahibi değilse
    /// <b>hiçbir şey yapılmaz</b>: aynı kullanıcının başka bir penceresinden
    /// çıkıldığında buradaki oturum hâlâ açıktır.
    /// </summary>
    /// <returns>Oturumun bu çağrıyla kapatılıp kapatılmadığı.</returns>
    public bool SignOut(Guid sessionId, DateTimeOffset now)
    {
        if (SessionId != sessionId)
        {
            return false;
        }

        // Kalp atışı zaten susmuşsa "pasif" sayılıyor; ayrıca çıkış zamanı yazmak
        // yalnızca son görülmeyi bozar.
        if (LoggedOutAt is null && now - LastHeartbeatAt > OnlineWindow)
        {
            return false;
        }

        LoggedOutAt = now;
        return true;
    }

    /// <summary>Verilen ana göre çevrimiçi olup olmadığını belirler.</summary>
    public bool IsOnlineAt(DateTimeOffset now)
        => LoggedOutAt is null && now - LastHeartbeatAt <= OnlineWindow;

    /// <summary>
    /// Verilen ana göre çevrimdışı olup olmadığını belirler. "Son görülme" bilgisi
    /// yalnızca çevrimdışı olanlar için anlamlıdır.
    /// </summary>
    public bool IsOfflineAt(DateTimeOffset now) => !IsOnlineAt(now);

    private static string? Normalize(string? machineName)
        => string.IsNullOrWhiteSpace(machineName) ? null : machineName.Trim();
}