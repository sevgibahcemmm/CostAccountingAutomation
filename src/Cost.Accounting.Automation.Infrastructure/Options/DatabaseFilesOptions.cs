namespace Cost.Accounting.Automation.Infrastructure.Options;

/// <summary>
/// Veritabanı fiziksel dosyalarının (<c>.mdf</c> / <c>.ldf</c>) nereye
/// konulacağını belirler.
/// </summary>
/// <remarks>
/// <para>
/// SQL Server, <c>CREATE DATABASE</c> komutunda dosya yolu verilmezse
/// veritabanını kendi varsayılan veri dizinine açar (örneğin
/// <c>C:\Program Files\Microsoft SQL Server\MSSQL.16\MSSQL\DATA</c> veya
/// LocalDB'de kullanıcı profili). Bu bölüm, dosyaların uygulamanın
/// <c>Data</c> klasöründe tutulmasını sağlar.
/// </para>
/// <para>
/// Öncelik sırası (<c>DatabaseFiles__DataDirectory</c> ortam değişkeni de
/// bu değerin üzerine yazabilir):
/// </para>
/// <list type="number">
/// <item><description><c>DataDirectory</c>: kurulum sihirbazının yazdığı açık yol.</description></item>
/// <item><description>Geliştirme: proje kökündeki <c>Data</c> klasörü (slnx/sln yanındaki).</description></item>
/// <item><description>Kurulu sürüm: uygulama klasöründeki <c>Data</c>.</description></item>
/// </list>
/// </remarks>
public sealed class DatabaseFilesOptions
{
    public const string SectionName = "DatabaseFiles";

    /// <summary>
    /// Veritabanı dosyalarının konacağı klasör. Boş bırakılırsa klasör
    /// otomatik çözümlenir (proje kökü <c>Data</c> ya da uygulama klasörü
    /// <c>Data</c>). Kurulum sihirbazı bu alanı kendi seçtiği klasörle doldurur.
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
