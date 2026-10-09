using Cost.Accounting.Automation.Infrastructure.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IO;
using System.Reflection;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Sürüm kontrolünü merkezi <c>AppReleases</c> tablosundan (Master
    /// veritabanı) okur ve uygulamanın sürümüyle karşılaştırır.
    ///
    /// <para>
    /// Tabloda <see cref="UpdateManifest"/> için gereken alanlar bulunur:
    /// <c>Version</c>, <c>SetupPath</c> (setup.exe'nin UNC paylaşım adresi),
    /// <c>Notes</c>, <c>Mandatory</c>. Kaydı sunucudaki <c>sync-updates.ps1</c>
    /// betiği ya da <c>caa-provision set-version</c> komutu yazar.
    /// Kontrol açılışta bir kez ve kısa zaman aşımıyla yapılır; veritabanına
    /// ulaşılamazsa sessizce atlanır ve uygulama normal açılır.
    /// </para>
    /// </summary>
    public sealed class UpdateChecker
    {
        private readonly UpdateOptions _options;
        private readonly string? _masterConnection;

        public UpdateChecker(IOptions<UpdateOptions> options, IConfiguration configuration)
        {
            _options = options.Value;
            _masterConnection = configuration.GetConnectionString("Master");
        }

        /// <summary>
        /// Master veritabanından en güncel aktif sürümü okur ve yeni bir sürüm
        /// varsa döndürür; aksi hâlde <c>null</c>. Veritabanına ulaşılamaz, kayıt
        /// yoksa ya da veri bozuksa da <c>null</c> döner (sessizce atlanır).
        /// </summary>
        public async Task<UpdateManifest?> CheckAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_masterConnection))
            {
                return null;
            }

            int timeoutSeconds = Math.Max(1, _options.TimeoutSeconds);
            var timeout = TimeSpan.FromSeconds(timeoutSeconds);

            try
            {
                var builder = new SqlConnectionStringBuilder(_masterConnection)
                {
                    ConnectTimeout = timeoutSeconds
                };

                await using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    using var openCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    openCts.CancelAfter(timeout);

                    await connection.OpenAsync(openCts.Token).ConfigureAwait(false);

                    const string sql = """
                        SELECT TOP (1)
                            [Version], SetupPath, ISNULL(Notes, N''), Mandatory
                        FROM dbo.AppReleases
                        WHERE IsActive = 1
                        ORDER BY PublishedAt DESC, Id DESC;
                        """;

                    await using var command = connection.CreateCommand();
                    command.CommandText = sql;
                    command.CommandTimeout = timeoutSeconds;

                    using var readCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    readCts.CancelAfter(timeout);

                    string version;
                    string setupPath;
                    string notes;
                    bool mandatory;

                    await using (var reader = await command.ExecuteReaderAsync(readCts.Token).ConfigureAwait(false))
                    {
                        if (!await reader.ReadAsync(readCts.Token).ConfigureAwait(false))
                        {
                            return null; // henüz yayınlanmış sürüm yok
                        }

                        version = reader.GetString(0);
                        setupPath = reader.GetString(1);
                        notes = reader.GetString(2);
                        mandatory = reader.GetBoolean(3);
                    }

                    if (string.IsNullOrWhiteSpace(version)
                        || string.IsNullOrWhiteSpace(setupPath)
                        || !Version.TryParse(version, out Version? remote)
                        || remote is null)
                    {
                        return null;
                    }

                    if (Compare(remote, CurrentVersion()) <= 0)
                    {
                        return null;
                    }

                    if (IsSkipped(version))
                    {
                        return null;
                    }

                    return new UpdateManifest
                    {
                        Version = version,
                        Url = setupPath,
                        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                        Mandatory = mandatory
                    };
                }
            }
            catch (Exception ex) when (
                ex is SqlException
                or InvalidOperationException)
            {
                return null; // veritabanina erisilemiyor: sessizce yok say
            }
            catch (OperationCanceledException)
            {
                return null; // zaman asimi ya da iptal: sessizce yok say
            }
        }

        /// <summary>Çalışan uygulamanın sürümü.</summary>
        public static Version CurrentVersion()
        {
            return Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0);
        }

        /// <summary>
        /// Arayüzlerde gösterilecek sürüm etiketi ("1.0.1"). Derleme sürümündeki
        /// dördüncü basamak ("0001") görüntülenmez; Compare ile aynı kuralla
        /// Major.Minor.Build kullanılır.
        /// </summary>
        public static string DisplayVersion()
        {
            Version v = CurrentVersion();
            return $"{v.Major}.{v.Minor}.{Build(v)}";
        }

        /// <summary>
        /// Adres bir Windows paylaşımı (UNC) mu? Örn.
        /// <c>\\192.168.1.5\Paylasim\CostAccountingAutomation-Setup-1.0.1.exe</c>.
        /// </summary>
        public static bool IsUncPath(string path)
        {
            return path.StartsWith(@"\\", StringComparison.Ordinal)
                || path.StartsWith("//", StringComparison.Ordinal);
        }

        /// <summary>İki sürümü Yalnızca Major.Minor.Build üzerinden karşılaştırır.</summary>
        /// <remarks>
        /// Revision bilinçli olarak yok sayılır: tabloda "1.0.0" yazarken
        /// derleme sürümü "1.0.0.0" olduğundan ham karşılaştırma yanlış "yeni
        /// sürüm var" sonucu üretirdi.
        /// </remarks>
        public static int Compare(Version a, Version b)
        {
            return (a.Major, a.Minor, Build(a)).CompareTo((b.Major, b.Minor, Build(b)));
        }

        private static int Build(Version version) => version.Build < 0 ? 0 : version.Build;

        private static string SkipFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CostAccountingAutomation",
            "update.skip");

        /// <summary>Kullanıcı bu sürümü daha önce atladı mı?</summary>
        public static bool IsSkipped(string version)
        {
            try
            {
                return File.Exists(SkipFilePath)
                    && File.ReadAllLines(SkipFilePath)
                        .Any(line => string.Equals(line.Trim(), version.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Kullanıcı "bu sürümü atla" dediğinde sürümü kalıcı olarak atlar.</summary>
        public static void SkipVersion(string version)
        {
            try
            {
                string path = SkipFilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.AppendAllLines(path, new[] { version });
            }
            catch
            {
                // Yazılamazsa kullanıcıya bir sonraki açılışta yeniden sorulur; zararsız.
            }
        }
    }

    /// <summary>Uygulamanın güncelleme bildirimi modeli (AppReleases kaydı).</summary>
    public sealed class UpdateManifest
    {
        public string Version { get; set; } = string.Empty;

        /// <summary>Yeni sürümün kurulum dosyası adresi (UNC paylaşım yolu).</summary>
        public string? Url { get; set; }

        /// <summary>Kullanıcıya gösterilecek kısa sürüm notları.</summary>
        public string? Notes { get; set; }

        /// <summary>Zorunlu güncelleme ise "atla"/"daha sonra" gizlenir.</summary>
        public bool Mandatory { get; set; }
    }
}