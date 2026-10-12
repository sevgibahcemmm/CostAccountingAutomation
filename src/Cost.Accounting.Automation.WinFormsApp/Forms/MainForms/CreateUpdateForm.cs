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
    public sealed partial class CreateUpdateForm : XtraForm
    {
        private const int MinLogHeight = 170;

        private string _targetVersion = string.Empty;
        private readonly UpdatePublisher _publisher = null!;
        private CancellationTokenSource? _running;
        private bool _runningState;

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

        /// <summary>
        /// Tasarım zamanı için. Üretimde kullanılmaz; yayıncı parametresiz
        /// kurucuyla sağlanamaz.
        /// </summary>
        public CreateUpdateForm()
        {
            InitializeComponent();
        }

        public CreateUpdateForm(UpdatePublisher publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

            InitializeComponent();
            ApplyTheme();
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
