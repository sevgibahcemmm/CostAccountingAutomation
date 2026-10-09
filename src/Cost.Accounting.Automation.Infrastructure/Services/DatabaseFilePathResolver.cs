using Cost.Accounting.Automation.Infrastructure.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Veritabanı dosyalarının (<c>.mdf</c> / <c>.ldf</c>) konacağı klasörü
/// belirler ve gerekiyorsa oluşturur.
/// </summary>
/// <remarks>
/// <para>
/// Veritabanı her ortamda <b>tek bir yerde</b> durur: <c>DatabaseFiles:DataDirectory</c>
/// açıkça verilmişse o klasör, verilmemişse <see cref="DatabaseFilesOptions.WellKnownDataDirectory"/>
/// kullanılır. Geliştirici IDE'den çalıştırsa da kurulu sürümü başlatsa da aynı
/// veritabanını görür; proje kökü ya da uygulama klasörü dikkate alınmaz.
/// </para>
/// <para>
/// Klasör dosyaları <b>SQL Server hizmeti</b> oluşturur ve yazar, uygulama değil.
/// Klasör ilk kez oluşturulurken hizmet hesabına (<c>NT SERVICE\MSSQL$INSTANCE</c>)
/// o klasöre tam denetim icacls ile verilmeye çalışılır; yetki verilemezse (ör.
/// yönetici olmayan oturum) hazırlık adımı hatayı kullanıcıya gösterir.
/// </para>
/// </remarks>
public sealed class DatabaseFilePathResolver(
    IOptions<DatabaseFilesOptions> options,
    IConfiguration configuration)
{
    private readonly DatabaseFilesOptions _options = options.Value;

    /// <summary>Dosya yolu verilerek mi oluşturulacak (bkz. <see cref="DatabaseFilesOptions.UseExplicitFileNames"/>).</summary>
    public bool UseExplicitFileNames => _options.UseExplicitFileNames;

    /// <summary>Klasör kullanıcı tarafından açıkça verilmiş mi?</summary>
    public bool IsDirectoryConfigured =>
        !string.IsNullOrWhiteSpace(_options.DataDirectory);

    /// <summary>
    /// Veritabanı dosyalarının konacağı klasörü döndürür; klasör yoksa oluşturur
    /// ve SQL Server hizmet hesabına erişim vermeyi dener.
    /// </summary>
    /// <exception cref="InvalidOperationException">Klasör oluşturulamadığında.</exception>
    public string GetDataDirectory()
    {
        string directory = IsDirectoryConfigured
            ? Environment.ExpandEnvironmentVariables(_options.DataDirectory!.Trim())
            : DatabaseFilesOptions.WellKnownDataDirectory;

        string fullPath = Path.GetFullPath(directory);

        try
        {
            bool existed = Directory.Exists(fullPath);

            Directory.CreateDirectory(fullPath);

            if (!existed)
            {
                GrantSqlServiceAccess(fullPath);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Veritabanı klasörü oluşturulamadı: {fullPath}{Environment.NewLine}{Environment.NewLine}" +
                "Klasörün var olduğundan ve yazma yetkisi olduğundan emin olun. " +
                "Kurulum (setup) sırasında veritabanı klasörünü başka bir diske " +
                "(örn. D:\\Veritabani) taşıyabilirsiniz.",
                ex);
        }

        return fullPath;
    }

    /// <summary>
    /// Klasörü SQL Server hizmet hesabına açmaya çalışır. Klasör dosyalarını
    /// uygulama değil SQL Server hizmeti oluşturur; bu yüzden hizmetin yazma
    /// yetkisi olmadan veritabanı açılamaz. Başarısızlık sessizce geçilir —
    /// hazırlık adımı (veya kurulum sihirbazı) eksik yetkiyi zaten bildirir.
    /// </summary>
    private void GrantSqlServiceAccess(string directory)
    {
        string? account = SqlServiceAccount();

        if (account is null)
        {
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "icacls.exe",
                Arguments = $"\"{directory}\" /grant \"{account}:(OI)(CI)F\" /T /C /Q",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using Process? process = Process.Start(startInfo);

            process?.WaitForExit(15_000);
        }
        catch
        {
            // Hak verilemezse (ör. yönetici olmayan oturum) hazırlık adımı
            // hatayı gösterir; burada sessizce geçilir.
        }
    }

    /// <summary>
    /// Bağlantı dizesindeki sunucu adından SQL Server hizmet hesabını türetir
    /// (<c>NT SERVICE\MSSQLSERVER</c> ya da <c>NT SERVICE\MSSQL$INSTANCE</c>).
    /// </summary>
    private string? SqlServiceAccount()
    {
        try
        {
            string? masterConnectionString = configuration.GetConnectionString("Master");

            if (string.IsNullOrWhiteSpace(masterConnectionString))
            {
                return null;
            }

            string dataSource = new SqlConnectionStringBuilder(masterConnectionString).DataSource;

            int slash = dataSource.IndexOf('\\');
            string instance = slash >= 0 ? dataSource[(slash + 1)..] : string.Empty;

            return string.IsNullOrWhiteSpace(instance)
                || instance.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
                ? @"NT SERVICE\MSSQLSERVER"
                : @"NT SERVICE\MSSQL$" + instance;
        }
        catch
        {
            return null;
        }
    }
}