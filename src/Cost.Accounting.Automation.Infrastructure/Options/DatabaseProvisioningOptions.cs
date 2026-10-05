namespace Cost.Accounting.Automation.Infrastructure.Options;

/// <summary>
/// Veritabanı hazırlığını (migration, tohumlama, veritabanı oluşturma) kimin
/// yapacağını belirler.
/// </summary>
public enum DatabaseProvisioningMode
{
    /// <summary>
    /// İstemci hiçbir şema veya veri değişikliği yapmaz. Yalnızca şemanın
    /// güncel olduğunu <b>salt okunur</b> doğrular ve gerekirse yöneticinin
    /// çalıştıracağı komutu ekranda gösterir.
    /// </summary>
    /// <remarks>
    /// Merkezi bir SQL Server'a bağlanan her istemci için zorunlu seçenektir.
    /// N istemci aynı anda açılırsa veritabanı hazırlama işlemi tek bir
    /// yönetici tarafından yapılır; böylece migration geçmişi tablosuna yazma
    /// yarışı, çift tohum kaydı ve eşzamanlı <c>CREATE DATABASE</c> hataları
    /// oluşmaz.
    /// </remarks>
    VerifyOnly = 0,

    /// <summary>
    /// İstemci kendisi veritabanını oluşturur, migration'ları uygular ve
    /// tohumlar. Yalnızca tek geliştirici makinelerinde (LocalDB) kullanılmalıdır.
    /// </summary>
    /// <remarks>
    /// Bu modda işlem yine de <c>sp_getapplock</c> ile kilitlenir; ancak iki
    /// geliştirici aynı anda uygulamayı açarsa ikincisi bekler ve hiçbir şey
    /// yapması gerekmediğini görür.
    /// </remarks>
    Automatic = 1
}

/// <summary>
/// <c>DatabaseProvisioning</c> bölümüne bağlanan ayarlar.
/// </summary>
public sealed class DatabaseProvisioningOptions
{
    public const string SectionName = "DatabaseProvisioning";

    /// <summary>
    /// Hazırlığı yapan taraf. Belirtilmezse <see cref="DatabaseProvisioningMode.VerifyOnly"/>
    /// kullanılır: güvenli olan seçenek budur, çünkü merkezi sunucuya bağlanan
    /// bir istemcinin şemayı değiştirmemesi gerekir.
    /// </summary>
    public DatabaseProvisioningMode Mode { get; set; } = DatabaseProvisioningMode.VerifyOnly;

    /// <summary>
    /// Şema hazırlığı kilidini bekleme süresi (saniye). Aynı anda birden fazla
    /// istemci hazırlık denerse yalnızca biri yapar, diğerleri bu süre kadar
    /// bekler.
    /// </summary>
    public int LockTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Giriş ekranı açılmadan önce bekleyen migration kontrolünün zaman aşımı
    /// (saniye). Bu kontrol yalnızca okuma yapar; kısa tutulmalıdır.
    /// </summary>
    public int SchemaCheckTimeoutSeconds { get; set; } = 20;
}
