using Cost.Accounting.Automation.Application.Updates;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Kaynak koddan yeni bir sürüm üretip veritabanına yayınlar. Release
    /// betiğini (<c>installer\release.ps1</c>) çalıştırarak kurulum paketini
    /// üretir ve <c>AppReleaseService</c> üzerinden yayına ekler.
    /// </summary>
    public sealed class UpdatePublisher
    {
        private const string ReleaseScriptRelativePath = "installer\\release.ps1";
        private const string ProjectRelativePath = "src\\Cost.Accounting.Automation.WinFormsApp\\Cost.Accounting.Automation.WinFormsApp.csproj";
        private const string InstallerFolderName = "installer";
        private const string SourceFolderName = "src";
        private const string VersionPattern = "<Version>(?<value>[^<]+)</Version>";

        private readonly IServiceScopeFactory _scopeFactory;

        public UpdatePublisher(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Yayınlanacak sıradaki sürümü belirler: veritabanındaki en yüksek
        /// sürüm ile proje dosyasındaki <c>&lt;Version&gt;</c> değerinden
        /// büyük olanını bir artırır.
        /// </summary>
        public async Task<string> GetNextVersionAsync(string? repositoryRoot, CancellationToken cancellationToken = default)
        {
            Version published = AppVersion.Parse(
                await ReadLatestPublishedVersionAsync(cancellationToken).ConfigureAwait(false));
            Version current = AppVersion.Parse(ReadProjectVersion(repositoryRoot));

            return Increment(published > current ? published : current);
        }

        private static string Increment(Version version)
        {
            int major = version.Major;
            int minor = version.Minor;
            int build = Math.Max(version.Build, 0);
            int revision = Math.Max(version.Revision, 0);

            if (revision >= 9)
            {
                build++;
                revision = 0;
            }
            else
            {
                revision++;
            }

            return $"{major}.{minor}.{build}.{revision}";
        }

        private async Task<string?> ReadLatestPublishedVersionAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            return await scope.ServiceProvider
                .GetRequiredService<IAppReleaseService>()
                .GetLatestVersionAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Proje dosyasındaki <c>&lt;Version&gt;</c> değerini verilen sürümle
        /// değiştirir. Dosya bulunamazsa veya değer yoksa <c>false</c> döner.
        /// </summary>
        public static bool TryWriteProjectVersion(string? repositoryRoot, string version)
        {
            if (string.IsNullOrWhiteSpace(repositoryRoot))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(
                Path.Combine(repositoryRoot.Trim().Trim('"'), ProjectRelativePath));

            if (!File.Exists(fullPath))
            {
                return false;
            }

            byte[] bytes = File.ReadAllBytes(fullPath);
            bool hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            string content = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                .GetString(hasBom ? bytes[3..] : bytes);

            if (!Regex.IsMatch(content, VersionPattern, RegexOptions.IgnoreCase))
            {
                return false;
            }

            string updated = Regex.Replace(
                content,
                VersionPattern,
                m => m.Groups[0].Value.Split('>')[0] + ">" + version + "</Version>",
                RegexOptions.IgnoreCase);

            File.WriteAllBytes(fullPath, new UTF8Encoding(hasBom).GetBytes(updated));

            return true;
        }

        /// <summary>Proje dosyasındaki sürümü normalize edilmiş hâliyle okur.</summary>
        public static string ReadProjectVersion(string? repositoryRoot)
        {
            if (string.IsNullOrWhiteSpace(repositoryRoot))
            {
                return string.Empty;
            }

            try
            {
                string fullPath = Path.GetFullPath(
                    Path.Combine(repositoryRoot.Trim().Trim('"'), ProjectRelativePath));

                if (!File.Exists(fullPath))
                {
                    return string.Empty;
                }

                Match match = Regex.Match(File.ReadAllText(fullPath), VersionPattern, RegexOptions.IgnoreCase);

                return match.Success ? AppVersion.Normalize(match.Groups["value"].Value) : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Seçilen klasörün depo kökü olup olmadığını doğrular ve release
        /// betiğinin tam yolunu döndürür.
        /// </summary>
        public static bool TryResolveReleaseScript(string? repositoryRoot, out string scriptPath, out string error)
        {
            scriptPath = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(repositoryRoot))
            {
                error = "Önce kaynak kod klasörünü seçin.";
                return false;
            }

            string root = repositoryRoot.Trim().Trim('"');
            string fullPath = Path.GetFullPath(Path.Combine(root, ReleaseScriptRelativePath));

            if (!File.Exists(fullPath))
            {
                error = "Seçilen klasör depo kökü değil. İçinde 'installer\\release.ps1' bulunmalı.";
                return false;
            }

            if (!Directory.Exists(Path.Combine(root, SourceFolderName)))
            {
                error = "Seçilen klasörde 'src' klasörü yok.";
                return false;
            }

            scriptPath = fullPath;
            return true;
        }

        /// <summary>
        /// Release betiğini çalıştırarak sürümü yayınlar ve çıkış kodunu döndürür.
        /// Betiğin standart çıktısı ve hataları <paramref name="output"/>'a akar.
        /// </summary>
        public static async Task<int> PublishAsync(
            string scriptPath,
            string version,
            string? notes,
            IProgress<string>? output = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(scriptPath, nameof(scriptPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = BuildArguments(scriptPath, version, notes),
                WorkingDirectory = Path.GetDirectoryName(scriptPath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process { StartInfo = startInfo };

            try
            {
                process.OutputDataReceived += (_, e) => Report(e.Data, output);
                process.ErrorDataReceived += (_, e) => Report(e.Data, output);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using (cancellationToken.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(entireProcessTree: true);
                        }
                    }
                    catch
                    {
                    }
                }))
                {
                    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                    process.WaitForExit();

                    return process.ExitCode;
                }
            }
            finally
            {
                process.Dispose();
            }
        }

        private static void Report(string? line, IProgress<string>? output)
        {
            if (line is not null)
            {
                output?.Report(line);
            }
        }

        private static string BuildArguments(string scriptPath, string version, string? notes)
        {
            string arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + scriptPath + "\" -Version " + version;

            if (!string.IsNullOrWhiteSpace(notes))
            {
                arguments += " -Notes \"" + notes.Replace("\"", "\"\"") + "\"";
            }

            return arguments;
        }
    }
}
