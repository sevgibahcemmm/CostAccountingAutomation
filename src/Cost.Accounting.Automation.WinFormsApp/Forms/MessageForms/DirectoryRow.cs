using Cost.Accounting.Automation.Application.Messages;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms;

/// <summary>
/// Kullanıcı listesinde gösterilen satır.
/// </summary>
/// <remarks>
/// Sorgu katmanı <c>System.Drawing</c> bilmez ve uygulama katmanı platformdan
/// bağımsızdır. Ekrana özgü alanlar (durum noktasının çizileceği boş sütun gibi)
/// bu sarmalayıcıda tutulur; böylece iş kuralları ile görselleştirme birbirine
/// karışmaz.
/// </remarks>
internal sealed class DirectoryRow
{
    public DirectoryRow(MessageDirectoryDto user) => User = user;

    /// <summary>
    /// Satırın arkasındaki veri. Yoklama sonuçları eski nesneler yerine bu
    /// referans tazelenir; böylece grid yeniden bağlanmadan değerler güncellenir.
    /// </summary>
    public MessageDirectoryDto User { get; private set; }

    /// <summary>
    /// Satırın verisini yenisiyle değiştirir. Aynı kimlik ve durum için
    /// çağrılır; kaydırma konumu korunurken "Çevrimiçi/Pasif", okunmamış sayı
    /// ve son görülme bilgisi tazelensin diye.
    /// </summary>
    public void Update(MessageDirectoryDto user) => User = user;

    public Guid UserId => User.UserId;

    /// <summary>Ad ve soyad.</summary>
    public string FullName => User.FullName;

    /// <summary>
    /// Durum noktasının çizildiği sütun. Metin taşımaz; nokta
    /// <c>ViewUsers_CustomDrawCell</c> içinde doğrudan çizilir.
    /// </summary>
    /// <remarks>
    /// Neden görsel düzenleyici değil: DevExpress'in görsel düzenleyicisi
    /// bağlı sütunda "görsel yok" yer tutucusu çiziyordu. Hücreyi doğrudan
    /// boyamak hem her temada doğru sonucu verir hem de rengi skin ile aynı
    /// yerden (<c>SkinTheme</c>) alır.
    /// </remarks>
    public string StatusMark => string.Empty;

    /// <summary>"Çevrimiçi" / "Pasif" metni.</summary>
    public string Status => User.PresenceCaption;

    /// <summary>
    /// Son görülme bilgisi. Çevrimiçi kullanıcıda oturum başlangıcı, pasif
    /// kullanıcıda en son göründüğü zaman gösterilir.
    /// </summary>
    public string LastSeen => User.IsOnline
        ? FormatRelative(User.SessionStartedAt ?? User.LastSeenAt ?? DateTimeOffset.Now)
        : User.LastSeenAt is { } lastSeen
            ? FormatRelative(lastSeen)
            : "Hiç girmedi";

    public string UserName => User.UserName;

    public string? RegistryNumber => User.RegistryNumber;

    /// <summary>Profil fotoğrafının göreli yolu; yoksa baş harf avatarı çizilir.</summary>
    public string? AvatarPath => User.AvatarPath;

    public string? CompanyName => User.CompanyName;

    public string RoleName => User.RoleName;

    public int UnreadCount => User.UnreadCount;

    public bool IsOnline => User.IsOnline;

    /// <summary>
    /// "5 dk önce" gibi göreli zaman metni. Uzun süredir görülmeyen
    /// kullanıcılarda tarih gösterilir; "180 gün önce" anlamlı değildir.
    /// </summary>
    private static string FormatRelative(DateTimeOffset moment)
    {
        TimeSpan elapsed = DateTimeOffset.Now - moment;

        if (elapsed.TotalMinutes < 1)
        {
            return "az önce";
        }

        if (elapsed.TotalHours < 1)
        {
            return $"{(int)elapsed.TotalMinutes} dk önce";
        }

        if (elapsed.TotalDays < 1)
        {
            return $"{(int)elapsed.TotalHours} saat önce";
        }

        if (elapsed.TotalDays < 30)
        {
            return $"{(int)elapsed.TotalDays} gün önce";
        }

        return moment.ToString("dd.MM.yyyy");
    }
}