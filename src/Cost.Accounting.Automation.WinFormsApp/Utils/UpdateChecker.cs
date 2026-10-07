using Cost.Accounting.Automation.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Yayın sunucusundaki sürüm manifestosunu okuyup uygulamanın sürümüyle
    /// karşılaştırır.
    ///
    /// <para>
    /// Manifesto, <c>Update:ManifestUrl</c> adresinde duran küçük bir JSON
    /// dosyasıdır (<c>version</c>, <c>url</c>, <c>notes</c>, <c>mandatory</c>).
    /// Kontrol açılışta bir kez ve kısa zaman aşımıyla yapılır; ağ yoksa ya da
    /// adres bozuksa sessizce atlanır ve uygulama normal açılır.
    /// </para>
    /// </summary>
    public sealed class UpdateChecker
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly UpdateOptions _options;

        public UpdateChecker(IOptions<UpdateOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Manifestoyu indirir ve yeni bir sürüm varsa döndürür; aksi hâlde
        /// <c>null</c>. Ağ hataları çağırana bırakılır; çağıran sessizce yutar.
        /// </summary>
        public async Task<UpdateManifest?> CheckAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ManifestUrl))
            {
                return null;
            }

            UpdateManifest? manifest;

            using (var http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds))
            })
            {
                string json = await http
                    .GetStringAsync(_options.ManifestUrl, cancellationToken)
                    .ConfigureAwait(false);

                manifest = JsonSerializer.Deserialize<UpdateManifest>(json, JsonOptions);
            }

            if (manifest is null
                || string.IsNullOrWhiteSpace(manifest.Version)
                || !Version.TryParse(manifest.Version, out Version? remote)
                || remote is null)
            {
                return null;
            }

            if (Compare(remote, CurrentVersion()) <= 0)
            {
                return null;
            }

            return IsSkipped(manifest.Version) ? null : manifest;
        }

        /// <summary>Çalışan uygulamanın sürümü.</summary>
        public static Version CurrentVersion()
        {
            return Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0);
        }

        /// <summary>İki sürümü Yalnızca Major.Minor.Build üzerinden karşılaştırır.</summary>
        /// <remarks>
        /// Revision bilinçli olarak yok sayılır: manifesto "1.0.0" yazarken
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

    /// <summary>Yayın sunucusundaki <c>update.json</c> modeli.</summary>
    public sealed class UpdateManifest
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>Yeni sürümün indirme bağlantısı (setup.exe).</summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>Kullanıcıya gösterilecek kısa sürüm notları.</summary>
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        /// <summary>Zorunlu güncelleme ise "atla"/"daha sonra" gizlenir.</summary>
        [JsonPropertyName("mandatory")]
        public bool Mandatory { get; set; }
    }
}