namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Veritabanı hazırlığında yürütülen adımlar. İlk kurulum sihirbazı bu
/// adımları sırayla ekrana basar ve her adımı tik işaretiyle tamamlar.
/// </summary>
/// <remarks>
/// Sıra numarası değil <b>anlam</b> önemlidir: adım <c>Running</c> olduktan
/// sonra aynı adımla <c>Completed</c> bildirilebilir (örneğin yıl veritabanı
/// adımının detayı her kurum için güncellenir). Bu yüzden arayüz adımları
/// numaralandırmak yerine <see cref="DatabaseProvisionStep"/> değeriyle eşler.
/// </remarks>
public enum DatabaseProvisionStep
{
    /// <summary>Veritabanı sunucusuna bağlanılıyor.</summary>
    ConnectServer,

    /// <summary>Ana (master) veritabanı oluşturuluyor.</summary>
    CreateMasterDatabase,

    /// <summary>Ana veritabanının tabloları ve altyapısı kuruluyor.</summary>
    ApplyMasterSchema,

    /// <summary>Kurum kayıtları oluşturuluyor.</summary>
    SeedCompanies,

    /// <summary>Rol ve kullanıcı kayıtları oluşturuluyor.</summary>
    SeedRolesAndUsers,

    /// <summary>sys_admin rolüne yetki tanımları ekleniyor.</summary>
    SeedPermissions,

    /// <summary>Şirketler için mali yıl iş veritabanları hazırlanıyor.</summary>
    ProvisionYearDatabases,

    /// <summary>Standart UFRS hesap planı yükleniyor.</summary>
    SeedChartOfAccounts,

    /// <summary>Birim cinsleri ve KDV oranları yükleniyor.</summary>
    SeedUnitsAndTaxRates,

    /// <summary>Sanal (örnek) müşteri ve tedarikçi kayıtları oluşturuluyor.</summary>
    SeedSampleRecords
}

/// <summary>Bir adımın anlık durumu.</summary>
public enum DatabaseProvisionStepState
{
    /// <summary>Henüz başlamadı.</summary>
    Pending,

    /// <summary>Devam ediyor.</summary>
    Running,

    /// <summary>Başarıyla bitti.</summary>
    Completed,

    /// <summary>Başarısız oldu; süreç burada durdurulur.</summary>
    Failed,

    /// <summary>
    /// Adım çalıştırılmadan atlandı; kayıtlar zaten mevcuttu.
    /// </summary>
    Skipped
}

/// <summary>
/// Adım durumu bildirimi. <paramref name="Detail"/> ekranda adımın sağında
/// gösterilen kısa bilgidir (adet, veritabanı adı gibi); sayısal olmak zorunda
/// değildir. <paramref name="Error"/> yalnızca <see cref="DatabaseProvisionStepState.Failed"/>
/// durumunda anlamlıdır.
/// </summary>
public sealed record DatabaseProvisionProgress(
    DatabaseProvisionStep Step,
    DatabaseProvisionStepState State,
    string? Detail = null,
    string? Error = null);

/// <summary>Ana veritabanının varlığına göre açılış davranışı.</summary>
public enum DatabaseFirstRunState
{
    /// <summary>Ana veritabanı yok; ilk kurulum sihirbazı gösterilecek.</summary>
    Missing,

    /// <summary>Ana veritabanı var; arka planda sessizce güncellenecek.</summary>
    Exists,

    /// <summary>
    /// Sunucuya ulaşılamadı. Sihirbaz yine de gösterilir; böylece kullanıcı
    /// ham bir özel durum hatası yerine anlaşılır bir mesaj ve alttaki hatayı görür.
    /// </summary>
    Unreachable
}
