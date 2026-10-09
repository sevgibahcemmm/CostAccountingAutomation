using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    /// <summary>
    /// Yeni bir sürüm bulunduğunda gösterilen küçük ilerleme penceresidir.
    ///
    /// <para>
    /// Kullanıcıdan onay istenmez: kurulum dosyası veritabanından indirilir
    /// (ilerleme ortadaki durum kartında gösterilir), Inno Setup
    /// <c>/VERYSILENT</c> bayrağıyla arka planda sessizce çalıştırılır ve
    /// program kapatılır. Setup kurulumu tamamlayıp yeni sürümü kendisi
    /// başlatır; böylece kullanıcı doğrudan giriş ekranına döner.
    /// Sihirbaz/onay penceresi gösterilmez.
    /// </para>
    ///
    /// <para>
    /// Görünüm (kontrol ağacı) <c>UpdateAvailableForm.Designer.cs</c>'te
    /// kurulur; temaya bağlı renkler <see cref="ApplyTheme"/> ile uygulanır.
    /// Tasarım görünümünde SkinTheme üretim dışı bağlamda kararsız olabileceği
    /// için temalar hataya dayanıklı (try/catch) uygulanır.
    /// </para>
    /// </summary>
    public sealed partial class UpdateAvailableForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly UpdateManifest _manifest = null!;
        private readonly UpdateChecker _checker = null!;
        private bool _installerStarted;

        private static string DownloadDirectory => Path.Combine(
            Path.GetTempPath(),
            "CostAccountingAutomation",
            "downloads");

        /// <summary>
        /// Güncelleme başlatılmadıysa (indirme hatası vb.) giriş ekranına devam
        /// edilir. Kurulum başlatıldıysa program kapanır; setup yeni sürümü
        /// kendisi açar.
        /// </summary>
        public bool ShouldContinueToLogin => !_installerStarted;

        /// <summary>WinForms tasarım görünümü için gereken parametresiz kurucu.</summary>
        public UpdateAvailableForm()
        {
            InitializeComponent();
            ApplyTheme();
        }

        public UpdateAvailableForm(UpdateManifest manifest, UpdateChecker checker)
            : this()
        {
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            _checker = checker ?? throw new ArgumentNullException(nameof(checker));
        }

        /// <summary>
        /// Aktif skin'den çözümlenen renkleri uygular. Tasarım görünümünde
        /// (skin bağlamı giriş ekranından önce hazır olmayabilir) uygulama
        /// sessizce atlanır; gerçek kullanımda OnShown öncesi uygulanır.
        /// </summary>
        private void ApplyTheme()
        {
            try
            {
                Color surface = SkinTheme.HighContrastSurface;

                BackColor = surface;
                ForeColor = SkinTheme.HighContrastText;

                accentStrip.BackColor = SkinTheme.Primary;
                appName.ForeColor = SkinTheme.SecondaryText;
                title.ForeColor = SkinTheme.HighContrastText;
                version.ForeColor = SkinTheme.EnsureReadable(SkinTheme.Primary, surface);
                hint.ForeColor = SkinTheme.MutedText(surface);

                card.BackColor = SkinTheme.SurfaceMuted(surface);
                _statusLabel.ForeColor = SkinTheme.HighContrastText;
                subLabel.ForeColor = SkinTheme.MutedText(card.BackColor);
                _percentLabel.ForeColor = SkinTheme.HighContrastText;
            }
            catch
            {
            }
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            await InstallAsync();
        }

        private async Task InstallAsync()
        {
            try
            {
                var progress = new Progress<int>(percent =>
                {
                    _progressBar.Value = percent;
                    _percentLabel.Text = percent + "%";
                });

                string setupPath = await _checker.DownloadToFileAsync(
                    _manifest,
                    DownloadDirectory,
                    progress);

                _progressBar.Value = 100;
                _percentLabel.Text = "100%";
                _statusLabel.Text = "Kurulum yükleniyor...";

                Update();

                // Sihirbaz gösterilmez; sessiz kurulum başlatılır. Kurulum bitince
                // setup, yeni sürümü açacak olan adımı kendisi çalıştırır.
                var startInfo = new ProcessStartInfo(setupPath)
                {
                    UseShellExecute = true,
                    Arguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-"
                };

                Process.Start(startInfo);

                _installerStarted = true;

                // Form, Program.CheckForUpdates içindeki ShowDialog ile alındığı
                // için Application.Exit() modal döngüyü her zaman bitirmeyebilir.
                // Önce Close() ile modal döngü sonlandırılır; Main, false dönen
                // ShouldContinueToLogin ile geri döner ve süreç kapanır. Böylece
                // eski (çalışan) exe açık kalmaz; setup yeni sürümü rahatça yazar.
                Close();
                System.Windows.Forms.Application.Exit();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Update.Install", ex);

                // Güncelleme başarısız oldu; uygulama normal şekilde açılır.
                Close();
            }
        }
    }
}