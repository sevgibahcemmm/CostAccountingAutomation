namespace Cost.Accounting.Automation.Infrastructure.Options;

/// <summary>
/// Veritabanı fiziksel dosyalarının (<c>.mdf</c> / <c>.ldf</c>) nereye
/// konulacağını belirler.
/// </summary>
/// <remarks>
/// <para>
/// SQL Server, <c>CREATE DATABASE</c> komutunda dosya yolu verilmezse
/// veritabanını kendi varsayılan veri dizinine açar (örneğin
/// <c>C:\Program Files\Microsoft SQL Server\MSSQL.16\MSSQL\DATA</c>).
/// Bu bölüm, dosyaların <b>tek bir sabit klasörde</b> tutulmasını sağlar.
/// </para>
/// <para>
/// Veritabanı tüm ortamlarda (geliştirme ve kurulu sürüm) aynı yerde durur:
/// varsayılan <see cref="WellKnownDataDirectory"/>'dir. Böylece IDE'den
/// çalıştırılan uygulama ile kurulu sürüm aynı veritabanını kullanır; iki
/// ayrı klasörde iki ayrı veritabanı oluşmaz.
/// </para>
/// <para>
/// Öncelik sırası (<c>DatabaseFiles__DataDirectory</c> ortam değişkeni de
/// bu değerin üzerine yazabilir):
/// </para>
/// <list type="number">
/// <item><description><c>DataDirectory</c>: açıkça verilmiş yol (kurulum sihirbazı makineye
/// özel farklı bir klasör seçtirdiğinde bunu yazar).</description></item>
/// <item><description><see cref="WellKnownDataDirectory"/>: her ortam için tek varsayılan.</description></item>
/// </list>
/// </remarks>
public sealed class DatabaseFilesOptions
{
    public const string SectionName = "DatabaseFiles";

    /// <summary>
    /// Veritabanı dosyalarının (<c>.mdf</c> / <c>.ldf</c>) konacağı tek varsayılan
    /// klasör. Geliştirme ve kurulu sürüm burayı kullanır; her otomatik dağıtım
    /// ortamı aynı veritabanını görür. Kurulum sihirbazı da bu klasörü önerir.
    /// </summary>
    public const string WellKnownDataDirectory = @"C:\CostAccountingAutomation\Database";

    /// <summary>
    /// Veritabanı dosyalarının konacağı klasör. Boş bırakılırsa
    /// <see cref="WellKnownDataDirectory"/> kullanılır. Kurulum sihirbazı bu
    /// alanı makineye özel seçim yapıldığında doldurur.
    /// </summary>
    public string? DataDirectory { get; set; }

    /// <summary>
    /// <see langword="true"/> ise <c>CREATE DATABASE</c> komutuna dosya
    /// yolu verilir ve dosyalar <see cref="DataDirectory"/> klasörüne açılır.
    /// </summary>
    /// <remarks>
    /// <see langword="false"/> yapılırsa dosya yolu verilmez; veritabanı
    /// SQL Sunucusunun kendi varsayılan veri dizinine açılır. Veritabanı
    /// sunucusu uygulamanın çalıştığı makineden uzaktaysa bu değer
    /// <see langword="false"/> yapılmalıdır: dosya yolları sunucunun dosya
    /// sisteminde geçerlidir, istemcinin değil.
    /// </remarks>
    public bool UseExplicitFileNames { get; set; } = true;
}
