using Cost.Accounting.Automation.Application.Updates;
using Cost.Accounting.Automation.Infrastructure.Context;
using Cost.Accounting.Automation.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;
using System.IO;
using System.Reflection;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Merkezi master veritabanındaki <c>AppReleases</c> tablosunu okuyarak
    /// uygulamanın sürümünü karşılaştırır ve yeni kurulum dosyasını indirir.
    ///
    /// <para>
    /// Harici bir yayın sunucusu/URL kullanılmaz; hem sürüm bilgisi hem kurulum
    /// dosyası veritabanında saklanır. Kontrol açılışta bir kez yapılır;
    /// veritabanına ulaşılamazsa sessizce atlanır ve uygulama normal açılır.
    /// </para>
    /// </summary>
    public sealed class UpdateChecker
    {
        private readonly UpdateOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdateChecker(IOptions<UpdateOptions> options, IServiceScopeFactory scopeFactory)
        {
            _options = options.Value;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Veritabanındaki en güncel yayın sürümünü kontrol eder; kurulu sürümden
        /// yeni bir sürüm varsa onu döndürür, aksi hâlde <c>null</c>. Veritabanı
        /// hataları çağırana bırakılır; çağıran sessizce yutar.
        /// </summary>
        public async Task<UpdateManifest?> CheckAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return null;
            }

            using var scope = _scopeFactory.CreateScope();
            var appReleaseService = scope.ServiceProvider.GetRequiredService<IAppReleaseService>();

            AppReleaseInfo? latest = await appReleaseService
                .GetLatestPublishedAsync(cancellationToken)
                .ConfigureAwait(false);

            if (latest is null)
            {
                return null;
            }

            if (AppVersion.Compare(latest.Version, CurrentVersionString()) <= 0)
            {
                return null;
            }

            if (IsSkipped(latest.Version))
            {
                return null;
            }

            return new UpdateManifest
            {
                Version = latest.Version,
                Notes = latest.Notes,
                Mandatory = latest.IsMandatory,
                FileSizeBytes = latest.FileSizeBytes
            };
        }

        /// <summary>
        /// Yayınlanmış sürümün kurulum dosyasını veritabanından disk üzerindeki
        /// geçici bir klasöre akış hâlinde indirir ve dosya yolunu döndürür.
        /// </summary>
        public async Task<string> DownloadToFileAsync(
            UpdateManifest manifest,
            string targetDirectory,
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

            await using var connection = context.Database.GetDbConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT FileContent FROM AppReleases WHERE VersionSort = @sortKey AND FileContent IS NOT NULL;";

            System.Data.Common.DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@sortKey";
            parameter.Value = AppVersion.SortKey(manifest.Version);
            command.Parameters.Add(parameter);

            await using var reader = await command
                .ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    $"'{manifest.Version}' sürümü için kurulum dosyası veritabanında bulunamadı.");
            }

            Directory.CreateDirectory(targetDirectory);

            string setupPath = Path.Combine(
                targetDirectory,
                $"CostAccountingAutomation-Setup-{manifest.Version}.exe");

            const int bufferSize = 80 * 1024;
            byte[] buffer = new byte[bufferSize];
            long total = Math.Max(1, manifest.FileSizeBytes);
            long written = 0;
            int lastPercent = -1;

            await using (var file = new FileStream(
                setupPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                useAsync: true))
            using (Stream stream = reader.GetStream(0))
            {
                int read;

                while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await file.WriteAsync(buffer.AsMemory(0, read), cancellationToken);

                    written += read;

                    int percent = (int)(written * 100 / total);

                    if (percent != lastPercent)
                    {
                        lastPercent = percent;
                        progress?.Report(percent);
                    }
                }
            }

            progress?.Report(100);

            return setupPath;
        }

        /// <summary>Çalışan uygulamanın sürümü (dört parçalı).</summary>
        public static Version CurrentVersion()
            => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0, 0);

        /// <summary>Çalışan uygulamanın sürümü (ör. "1.0.0.4").</summary>
        public static string CurrentVersionString()
            => AppVersion.Normalize(Assembly.GetEntryAssembly()?.GetName().Version?.ToString());

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

    /// <summary>
    /// Veritabanındaki yayın sürümünün özeti. Kurulum dosyası bu nesnede
    /// taşınmaz; <see cref="UpdateChecker.DownloadToFileAsync"/> ile indirilir.
    /// </summary>
    public sealed class UpdateManifest
    {
        public string Version { get; set; } = string.Empty;

        /// <summary>Kullanıcıya gösterilecek kısa sürüm notları.</summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Zorunlu güncelleme ise "atla"/"daha sonra" gizlenir ve kullanıcı
        /// güncellemeden uygulamayı kullanamaz.
        /// </summary>
        public bool Mandatory { get; set; }

        public long FileSizeBytes { get; set; }
    }
}