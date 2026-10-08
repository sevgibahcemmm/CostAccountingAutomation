using Cost.Accounting.Automation.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.IO;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Veritabanı dosyalarının (<c>.mdf</c> / <c>.ldf</c>) konacağı <c>Data</c>
/// klasörünü belirler ve gerekiyorsa oluşturur.
/// </summary>
/// <remarks>
/// <para>
/// Çözümleme sırası:
/// </para>
/// <list type="number">
/// <item><description>
/// <c>DatabaseFiles:DataDirectory</c> (kurulum sihirbazı yazar; ortam
/// değişkeni <c>DatabaseFiles__DataDirectory</c> ile ezilebilir).
/// </description></item>
/// <item><description>
/// Geliştirme: <c>AppContext.BaseDirectory</c> üstünde çözüm dosyası
/// (<c>*.slnx</c> / <c>*.sln</c>) aranır; bulunursa proje kökündeki
/// <c>Data</c> klasörü kullanılır. Böylece veritabanları kaynak kodla
/// aynı kök klasörde durur.
/// </description></item>
/// <item><description>
/// Kurulu sürüm: çözüm dosyası bulunamazsa uygulama klasöründeki
/// <c>Data</c> (örn. <c>C:\Program Files\Cost Accounting Automation\Data</c>).
/// </description></item>
/// </list>
/// <para>
/// Klasör oluşturulamazsa (örn. Program Files'a yetkisiz yazma denemesi)
/// açıklayıcı bir hata fırlatılır; hata hazırlama adımlarında kullanıcıya
/// gösterilir.
/// </para>
/// </remarks>
public sealed class DatabaseFilePathResolver(IOptions<DatabaseFilesOptions> options)
{
    /// <summary>Yukarı doğru dizi tarama üst sınırı (bin/Debug/net10.0 vb.).</summary>
    private const int MaxWalkUpLevels = 12;

    private readonly DatabaseFilesOptions _options = options.Value;

    /// <summary>Dosya yolu verilerek mi oluşturulacak (bkz. <see cref="DatabaseFilesOptions.UseExplicitFileNames"/>).</summary>
    public bool UseExplicitFileNames => _options.UseExplicitFileNames;

    /// <summary>Klasör kullanıcı tarafından açıkça verilmiş mi?</summary>
    public bool IsDirectoryConfigured =>
        !string.IsNullOrWhiteSpace(_options.DataDirectory);

    /// <summary>
    /// Veritabanı dosyalarının konacağı klasörü döndürür; klasör yoksa oluşturur.
    /// </summary>
    /// <exception cref="InvalidOperationException">Klasör oluşturulamadığında.</exception>
    public string GetDataDirectory()
    {
        string directory = IsDirectoryConfigured
            ? Environment.ExpandEnvironmentVariables(_options.DataDirectory!.Trim())
            : ResolveDefaultDirectory();

        try
        {
            Directory.CreateDirectory(directory);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Veritabanı klasörü oluşturulamadı: {directory}{Environment.NewLine}{Environment.NewLine}" +
                "Klasörün var olduğundan ve yazma yetkisi olduğundan emin olun. " +
                "Kurulum (setup) sırasında veritabanı klasörünü başka bir diske " +
                "(örn. D:\\Veritabani) taşıyabilirsiniz.",
                ex);
        }

        return Path.GetFullPath(directory);
    }

    /// <summary>
    /// Açık yol verilmemişse çözüm kökü ya da uygulama klasörü yanındaki
    /// <c>Data</c> klasörünü bulur.
    /// </summary>
    private static string ResolveDefaultDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        for (int level = 0; directory is not null && level < MaxWalkUpLevels; level++)
        {
            if (HasSolutionFile(directory))
            {
                return Path.Combine(directory.FullName, "Data");
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "Data");
    }

    private static bool HasSolutionFile(DirectoryInfo directory)
    {
        try
        {
            return directory.GetFiles("*.slnx").Length > 0
                || directory.GetFiles("*.sln").Length > 0;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
