using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    /// <summary>
    /// Yeni bir sürüm bulunduğunda giriş ekranından önce gösterilen bildirim
    /// penceresi.
    ///
    /// <para>
    /// Program kendisini kendiliğinden güncellemez. Kullanıcı "İndir" ile
    /// indirme bağlantısını tarayıcıda açar; "Daha Sonra" yalnızca bu açılışı
    /// atlar, "Bu Sürümü Atla" ise sürümü kalıcı olarak atlar (zorunlu
    /// güncellemelerde bu iki seçenek gizlenir).
    /// </para>
    /// </summary>
    public sealed class UpdateAvailableForm : XtraForm
    {
        private readonly UpdateManifest _manifest;

        public UpdateAvailableForm(UpdateManifest manifest)
        {
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));

            Text = "Güncelleme Mevcut";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(560, 340);
            SuspendLayout();

            Color surface = SkinTheme.HighContrastSurface;
            BackColor = surface;

            var title = new Label
            {
                Text = "Programın yeni bir sürümü hazır",
                Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
                ForeColor = SkinTheme.EnsureReadable(SkinTheme.Warning, surface),
                Location = new Point(22, 20),
                AutoSize = true
            };

            var body = new Label
            {
                Text = $"Kurulu sürüm: {UpdateChecker.DisplayVersion()}    •    Yeni sürüm: {_manifest.Version}",
                Font = new Font(Font.FontFamily, 9.5f),
                ForeColor = surface,
                Location = new Point(22, 58),
                AutoSize = true
            };

            var notes = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SkinTheme.HighContrastSurface,
                ForeColor = SkinTheme.HighContrastText,
                Font = new Font(Font.FontFamily, 9.5f),
                Location = new Point(24, 90),
                Size = new Size(ClientSize.Width - 48, 180),
                Text = string.IsNullOrWhiteSpace(_manifest.Notes)
                    ? "Sürüm notu belirtilmemiş."
                    : _manifest.Notes
            };

            var downloadButton = new SimpleButton
            {
                Text = "İndir",
                Size = new Size(120, 32),
                Location = new Point(ClientSize.Width - 142, ClientSize.Height - 46),
                Cursor = Cursors.Hand,
                Enabled = !string.IsNullOrWhiteSpace(_manifest.Url)
            };
            downloadButton.Click += DownloadButton_Click;

            var laterButton = new SimpleButton
            {
                Text = "Daha Sonra",
                Size = new Size(120, 32),
                Location = new Point(ClientSize.Width - 272, ClientSize.Height - 46),
                Cursor = Cursors.Hand,
                Visible = !_manifest.Mandatory
            };
            laterButton.Click += (_, _) => Close();

            var skipButton = new SimpleButton
            {
                Text = "Bu Sürümü Atla",
                Size = new Size(150, 32),
                Location = new Point(24, ClientSize.Height - 46),
                Cursor = Cursors.Hand,
                Visible = !_manifest.Mandatory
            };
            skipButton.Click += SkipButton_Click;

            Controls.Add(title);
            Controls.Add(body);
            Controls.Add(notes);
            Controls.Add(downloadButton);
            Controls.Add(laterButton);
            Controls.Add(skipButton);

            AcceptButton = downloadButton;
            CancelButton = laterButton.Visible ? laterButton : skipButton;

            ResumeLayout(true);
        }

        private void DownloadButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_manifest.Url))
            {
                return;
            }

            try
            {
                // Yerel Alan Agi paylasimi (UNC): setup, gecici bir yerel klasore
                // kopyalanir ve kurulum baslatilir. Calisan exe kilitli oldugundan
                // uygulama kapanir; kurulum eski surumun uzerine yazar ve mevcut
                // appsettings.Local.json'u korur (bkz. CostAccountingAutomation.iss).
                if (UpdateChecker.IsUncPath(_manifest.Url))
                {
                    string updaterDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "CostAccountingAutomation",
                        "Updates");
                    Directory.CreateDirectory(updaterDir);
                    string localSetup = Path.Combine(updaterDir, Path.GetFileName(_manifest.Url));
                    File.Copy(_manifest.Url, localSetup, overwrite: true);

                    Close();
                    Process.Start(new ProcessStartInfo(localSetup) { UseShellExecute = true });
                    Environment.Exit(0);
                    return;
                }

                // Internet adresi (https/...): indirme baglantisini tarayicida ac.
                Process.Start(new ProcessStartInfo(_manifest.Url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    "İndirme bağlantısı açılamadı:\n" + ex.Message,
                    "Güncelleme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            Close();
        }

        private void SkipButton_Click(object? sender, EventArgs e)
        {
            UpdateChecker.SkipVersion(_manifest.Version);
            Close();
        }
    }
}