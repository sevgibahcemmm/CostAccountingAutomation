using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    /// <summary>
    /// "Güncelleme Oluştur" ekranı. Kaynak kod klasörü seçilir, sıradaki sürüm
    /// hesaplanır ve release betiği çalıştırılarak kurulum paketi üretilip
    /// veritabanına yayınlanır.
    /// </summary>
    public sealed class CreateUpdateForm : XtraForm
    {
        private const int MinLogHeight = 170;

        private string _targetVersion = string.Empty;
        private readonly UpdatePublisher _publisher;
        private CancellationTokenSource? _running;
        private bool _runningState;

        private Label _appName = null!;
        private Label _title = null!;
        private Label _hint = null!;
        private Label _repositoryCaption = null!;
        private TextBox _repositoryText = null!;
        private Button _browseButton = null!;
        private Label _versionLabel = null!;
        private Label _notesCaption = null!;
        private TextBox _notesText = null!;
        private Label _logCaption = null!;
        private TextBox _logText = null!;
        private System.Windows.Forms.ProgressBar _progressBar = null!;
        private Button _publishButton = null!;
        private Button _closeButton = null!;
        private Panel _accentStrip = null!;

        public bool Published { get; private set; }

        private bool IsRunning
        {
            get => _runningState;
            set
            {
                _runningState = value;
                _repositoryText.Enabled = !value;
                _browseButton.Enabled = !value;
                _notesText.Enabled = !value;
                _publishButton.Enabled = !value;
                _closeButton.Text = value ? "İptal" : "Kapat";
                _progressBar.Visible = value;
            }
        }

        public CreateUpdateForm(UpdatePublisher publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

            BuildLayout();
            ApplyTheme();
        }

        private void BuildLayout()
        {
            Text = "Güncelleme Oluştur";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(720, 560);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;

            _accentStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4
            };

            _appName = new Label
            {
                AutoSize = true,
                Font = new Font(Font.FontFamily, 8.5f),
                Location = new Point(24, 18),
                Text = "Maliyet Muhasebesi Otomasyonu"
            };

            _title = new Label
            {
                AutoSize = true,
                Font = new Font(Font.FontFamily, 13f, FontStyle.Bold),
                Location = new Point(24, 36),
                Text = "Güncelleme Oluştur"
            };

            _hint = new Label
            {
                AutoSize = true,
                Font = new Font(Font.FontFamily, 8.5f),
                Location = new Point(24, 66),
                Text = "Kaynak kod klasörünü seçin. Kurulum paketi bu klasördeki dosyalardan üretilir."
            };

            _repositoryCaption = new Label
            {
                AutoSize = true,
                Location = new Point(24, 100),
                Text = "Kaynak kod klasörü"
            };

            _repositoryText = new TextBox
            {
                Location = new Point(24, 120),
                ReadOnly = true,
                Size = new Size(560, 23)
            };

            _browseButton = new Button
            {
                Location = new Point(592, 119),
                Size = new Size(96, 25),
                Text = "Gözat..."
            };
            _browseButton.Click += BrowseButton_Click;

            _versionLabel = new Label
            {
                AutoSize = true,
                Font = new Font(Font.FontFamily, 9.5f),
                Location = new Point(24, 156),
                Text = "Yayınlanacak sürüm: hesaplanıyor..."
            };

            _notesCaption = new Label
            {
                AutoSize = true,
                Location = new Point(24, 186),
                Text = "Sürüm notları (kullanıcılara gösterilir)"
            };

            _notesText = new TextBox
            {
                Location = new Point(24, 206),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(664, 56)
            };

            _logCaption = new Label
            {
                AutoSize = true,
                Location = new Point(24, 274),
                Text = "Kurulum çıktısı"
            };

            _logText = new TextBox
            {
                Location = new Point(24, 294),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Size = new Size(664, MinLogHeight),
                WordWrap = false
            };

            _progressBar = new System.Windows.Forms.ProgressBar
            {
                Location = new Point(24, 476),
                MarqueeAnimationSpeed = 30,
                Size = new Size(664, 14),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            _publishButton = new Button
            {
                Enabled = false,
                Location = new Point(480, 504),
                Size = new Size(100, 30),
                Text = "Yayınla"
            };
            _publishButton.Click += PublishButton_Click;

            _closeButton = new Button
            {
                Location = new Point(588, 504),
                Size = new Size(100, 30),
                Text = "Kapat"
            };
            _closeButton.Click += CloseButton_Click;

            Controls.AddRange(
                _appName,
                _title,
                _hint,
                _repositoryCaption,
                _repositoryText,
                _browseButton,
                _versionLabel,
                _notesCaption,
                _notesText,
                _logCaption,
                _logText,
                _progressBar,
                _publishButton,
                _closeButton,
                _accentStrip);
        }

        private void ApplyTheme()
        {
            try
            {
                Color color = BackColor = SkinTheme.HighContrastSurface;

                ForeColor = SkinTheme.HighContrastText;
                _accentStrip.BackColor = SkinTheme.Primary;
                _appName.ForeColor = SkinTheme.SecondaryText;
                _title.ForeColor = SkinTheme.HighContrastText;
                _hint.ForeColor = SkinTheme.MutedText(color);
                _versionLabel.ForeColor = SkinTheme.EnsureReadable(SkinTheme.Primary, color);
                _logText.BackColor = SkinTheme.SurfaceMuted(color);
                _logText.ForeColor = SkinTheme.HighContrastText;
            }
            catch
            {
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _versionLabel.Text = "Kurulu sürüm: " + UpdateChecker.CurrentVersionString()
                + "   ·   Yayınlanacak sürüm: kaynak kod klasörünü seçin";
        }

        private async Task RefreshVersionFromProject()
        {
            string project = UpdatePublisher.ReadProjectVersion(_repositoryText.Text);

            try
            {
                _targetVersion = await _publisher.GetNextVersionAsync(_repositoryText.Text);

                _versionLabel.Text = $"Kurulu sürüm: {UpdateChecker.CurrentVersionString()}"
                    + $"   ·   Yayınlanacak sürüm: {_targetVersion}"
                    + (string.IsNullOrWhiteSpace(project) ? string.Empty : $" (csproj: {project})");

                _publishButton.Enabled = true;

                if (string.IsNullOrWhiteSpace(_notesText.Text))
                {
                    _notesText.Text = "Sürüm " + _targetVersion;
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CreateUpdateForm.RefreshVersion", ex);

                _targetVersion = string.Empty;
                _versionLabel.Text = "Yayınlanacak sürüm okunamadı (veritabanına ulaşılamadı)";
                _publishButton.Enabled = false;
            }
        }

        private async void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Kaynak kodun bulunduğu klasörü seçin (içinde src ve installer klasörleri olmalı).",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (!string.IsNullOrWhiteSpace(_repositoryText.Text))
            {
                dialog.SelectedPath = _repositoryText.Text;
            }

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (!UpdatePublisher.TryResolveReleaseScript(dialog.SelectedPath, out string _, out string error))
            {
                ToastHelper.Show(error, ToastType.Warning, 6000);
                return;
            }

            _repositoryText.Text = dialog.SelectedPath;

            await RefreshVersionFromProject();
        }

        private async void PublishButton_Click(object? sender, EventArgs e)
        {
            if (!UpdatePublisher.TryResolveReleaseScript(_repositoryText.Text, out string scriptPath, out string error))
            {
                ToastHelper.Show(error, ToastType.Warning, 6000);
                return;
            }

            string version = _targetVersion;

            if (string.IsNullOrWhiteSpace(version))
            {
                ToastHelper.Show("Yayınlanacak sürüm okunamadı.", ToastType.Error);
                return;
            }

            IsRunning = true;
            _logText.Clear();

            var output = new Progress<string>(AppendLogLine);
            using var cancellation = new CancellationTokenSource();
            _running = cancellation;

            try
            {
                AppendLogLine("Sürüm " + version + " yayınlanıyor. Bu işlem 15-25 dakika sürebilir...");

                int exitCode = await UpdatePublisher.PublishAsync(
                    scriptPath, version, _notesText.Text, output, cancellation.Token);

                if (exitCode != 0)
                {
                    AppendLogLine(string.Empty);
                    AppendLogLine($"Yayın başarısız oldu (çıkış kodu {exitCode}).");
                    ToastHelper.Show("Yayın başarısız oldu. Ayrıntı için çıktıyı inceleyin.", ToastType.Error, 8000);
                    return;
                }

                AppendLogLine(string.Empty);
                AppendLogLine("Sürüm " + version + " yayınlandı.");

                Published = true;

                if (UpdatePublisher.TryWriteProjectVersion(_repositoryText.Text, version))
                {
                    AppendLogLine("Proje dosyasındaki <Version> değeri " + version + " olarak güncellendi.");
                }

                _publishButton.Enabled = false;
                _publishButton.Text = "Yayınlandı";
                _versionLabel.Text = "Yayınlanan sürüm: " + version;
                _hint.Text = "Bu oturumda başka bir sürüm yayınlanamaz. Yeni bir sürüm için uygulamayı yeniden başlatın.";
                ToastHelper.Show(
                    "Sürüm " + version + " yayınlandı. Kurulu istemciler bir sonraki açılışta güncellenecek.",
                    ToastType.Success,
                    8000);
            }
            catch (OperationCanceledException)
            {
                AppendLogLine(string.Empty);
                AppendLogLine("Yayın iptal edildi.");
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CreateUpdateForm.Publish", ex);

                AppendLogLine(string.Empty);
                AppendLogLine("Yayın sırasında hata oluştu: " + ex.Message);
                ToastHelper.Show("Yayın sırasında hata oluştu: " + ex.Message, ToastType.Error, 8000);
            }
            finally
            {
                _running = null;
                IsRunning = false;
                _publishButton.Enabled = !string.IsNullOrWhiteSpace(_repositoryText.Text);
            }
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            if (IsRunning)
            {
                _running?.Cancel();
            }
            else
            {
                Close();
            }
        }

        private void AppendLogLine(string line)
        {
            if (!IsDisposed)
            {
                _logText.AppendText(line + Environment.NewLine);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (IsRunning)
            {
                e.Cancel = true;
                _running?.Cancel();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _running?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
