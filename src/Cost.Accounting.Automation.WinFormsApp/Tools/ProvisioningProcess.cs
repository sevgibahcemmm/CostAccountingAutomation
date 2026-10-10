using Cost.Accounting.Automation.WinFormsApp.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Cost.Accounting.Automation.WinFormsApp.Tools
{
    /// <summary>
    /// Veritabanı hazırlama aracını (<c>caa-provision.exe</c>) yönetici
    /// haklarıyla çalıştırır.
    ///
    /// <para>
    /// Uygulama şemaya kullanıcı bağlamında dokunmaz (VerifyOnly) ya da
    /// dokunamaz (yetersiz yetki); bu yüzden "Veritabanını kur / güncelle"
    /// eylemi, kurulumla <c>{app}\tools</c> klasörüne konan aracın
    /// <c>provision --version &lt;kurulu sürüm&gt;</c> komutu yönetici olarak
    /// çalıştırılarak yapılır. Böylece veritabanı klasörü oluşturma, SQL
    /// hizmet hesabına hak verme ve şema güncelleme tek işlemle tamamlanır.
    /// </para>
    /// </summary>
    public static class ProvisioningProcess
    {
        /// <summary>Çalıştırma sonucunun özeti.</summary>
        public readonly record struct Result(ProvisioningRunState State, int ExitCode)
        {
            public bool Succeeded => State == ProvisioningRunState.Succeeded;
        }

        /// <summary>
        /// Aracı bulur ve <c>provision --version &lt;kurulu sürüm&gt;</c>
        /// komutunu UAC istemiyle yönetici olarak çalıştırıp sonucunu döndürür.
        /// </summary>
        public static Result RunProvision()
        {
            string? exe = LocateProvisionExe();

            if (exe is null)
            {
                return new Result(ProvisioningRunState.NotFound, -1);
            }

            try
            {
                var startInfo = new ProcessStartInfo(exe)
                {
                    Arguments =
                        $"provision --version \"{UpdateChecker.CurrentVersionString()}\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = Path.GetDirectoryName(exe) ?? string.Empty
                };

                Process? process = Process.Start(startInfo);

                if (process is null)
                {
                    return new Result(ProvisioningRunState.Failed, -1);
                }

                using (process)
                {
                    process.WaitForExit();

                    return new Result(
                        process.ExitCode == 0
                            ? ProvisioningRunState.Succeeded
                            : ProvisioningRunState.Failed,
                        process.ExitCode);
                }
            }
            catch (Win32Exception)
            {
                // Kullanıcı UAC istemini iptal etti; hiçbir işlem yapılmadı.
                return new Result(ProvisioningRunState.Cancelled, -1);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Provision.Run", ex);

                return new Result(ProvisioningRunState.Failed, -1);
            }
        }

        /// <summary>
        /// Aracı öncelik sırasıyla arar: uygulamanın <c>tools</c> klasörü,
        /// ardından kurulu sürümün <c>tools</c> klasörü, son olarak uygulamanın
        /// kök klasörü. Geleneksel oturum hakkı aranmaz; yönetici çalıştırması
        /// <see cref="RunProvision"/> içinde yapılır.
        /// </summary>
        private static string? LocateProvisionExe()
        {
            string[] candidates =
            {
                Path.Combine(AppContext.BaseDirectory, "tools", "caa-provision.exe"),
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs",
                    "Maliyet Muhasebesi Otomasyonu",
                    "tools",
                    "caa-provision.exe"),
                Path.Combine(AppContext.BaseDirectory, "caa-provision.exe")
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// <see cref="ProvisioningProcess.RunProvision"/> çalıştırmasının durumu.
    /// </summary>
    public enum ProvisioningRunState
    {
        /// <summary>caa-provision.exe hiçbir bilinen konumda bulunamadı.</summary>
        NotFound,

        /// <summary>Kullanıcı yönetici onay istemini iptal etti.</summary>
        Cancelled,

        /// <summary>Araç başlatıldı ve sıfır dışı bir dönüş koduyla bitti.</summary>
        Failed,

        /// <summary>Araç başarıyla (dönüş kodu 0) tamamlandı.</summary>
        Succeeded
    }
}