using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Infrastructure;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    /// <summary>
    /// Açılış penceresi ve ilk kurulum sihirbazı.
    ///
    /// <para>
    /// Bu pencere <b>her açılışta</b>, giriş ekranından önce gösterilir. Böylece
    /// veritabanı yoklaması ve şema güncellemesi arka planda çalışırken
    /// uygulama "donmuş" gibi görünmez. Önceden yalnızca veritabanı yoksa
    /// açılıyordu; sunucuya ulaşılamadığında 30 saniyelik bağlantı zaman aşımı
    /// hiçbir pencere gösterilmeden geçtiği için uygulama kilitleniyordu.
    /// </para>
    ///
    /// <para>
    /// Davranış akışı: veritabanı kontrolü (kısa zaman aşımı) → veritabanı
    /// varsa arka planda sessizce güncellenip pencere kendini kapatır, yoksa
    /// adım listesi tik işaretleriyle gösterilir, sunucuya ulaşılamazsa
    /// gerçek hata mesajı "Tekrar Dene" düğmesiyle birlikte sunulur.
    /// </para>
    ///
    /// <para>
    /// Adım listesi <see cref="DatabaseInitializer.Steps"/>'ten gelir; ekranda
    /// gösterilen adımlar veritabanı tarafında gerçekten yürütülen adımlarla
    /// aynı kaynaktan beslenir.
    /// </para>
    /// </summary>
    public partial class DatabaseSetupForm : DevExpress.XtraEditors.XtraForm
    {
        private static readonly Dictionary<DatabaseProvisionStep, string> StepTitles = new()
        {
            [DatabaseProvisionStep.ConnectServer] = "Veritabanı sunucusuna bağlanılıyor",
            [DatabaseProvisionStep.CreateMasterDatabase] = "Ana veritabanı oluşturuluyor",
            [DatabaseProvisionStep.ApplyMasterSchema] = "Tablo ve altyapı kuruluyor",
            [DatabaseProvisionStep.SeedCompanies] = "Kurum kayıtları oluşturuluyor",
            [DatabaseProvisionStep.SeedRolesAndUsers] = "Rol ve kullanıcı kayıtları oluşturuluyor",
            [DatabaseProvisionStep.SeedPermissions] = "Yetki tanımları hazırlanıyor",
            [DatabaseProvisionStep.ProvisionYearDatabases] = "Mali yıl veritabanları hazırlanıyor",
            [DatabaseProvisionStep.SeedChartOfAccounts] = "Standart hesap planı yükleniyor",
            [DatabaseProvisionStep.SeedUnitsAndTaxRates] = "Birim cinsleri ve KDV oranları yükleniyor",
            [DatabaseProvisionStep.SeedSampleRecords] = "Sanal veri kayıtları oluşturuluyor"
        };

        private readonly Dictionary<DatabaseProvisionStep, StepRow> _rows = [];

        /// <summary>
        /// Adım özetlerinin tamamını göstermek için ortak ipucu. Satır başına
        /// ayrı <see cref="ToolTip"/> oluşturmak yerine tek bir örnek kullanılır.
        /// </summary>
        private readonly ToolTip _stepToolTip = new()
        {
            InitialDelay = 250,
            ReshowDelay = 100
        };

        private readonly Color _surface;
        private readonly Color _surfaceMuted;
        private readonly Color _text;
        private readonly Color _mutedText;
        private readonly Color _border;

        private CancellationTokenSource? _cts;
        private DatabaseProvisionStep? _runningStep;
        private bool _shouldContinueToLogin;

        /// <summary>
        /// Kurulum tamamlandı (ya da veritabanı zaten günceldi). <c>Program</c>
        /// yalnızca bu değer <c>true</c> ise giriş formunu açar.
        /// </summary>
        public bool ShouldContinueToLogin => _shouldContinueToLogin;

        public DatabaseSetupForm()
        {
            InitializeComponent();

            // Kurulum ekranı giriş öncesi olduğu için skin bu noktada zaten
            // ayarlanmıştır; renkler buna göre çözülür ve pencere koyu temada da
            // açık temada da okunabilir kalır.
            _surface = SkinTheme.HighContrastSurface;
            _surfaceMuted = SkinTheme.SurfaceMuted(SkinTheme.HighContrastSurface);
            _text = SkinTheme.HighContrastText;
            _mutedText = SkinTheme.MutedText(_surface);
            _border = SkinTheme.BorderMuted(_surface);

            btnOk.Click += BtnOk_Click;
            btnRetry.Click += BtnRetry_Click;
            btnElevated.Click += BtnElevated_Click;

            BuildStepRows();
            ApplyTheme();

            // Kontrol aşamasındayken adım listesi gereksiz yere "boş" görünmesin.
            pnlSteps.Visible = false;
            SetBanner(BannerState.Checking);
        }

        /// <summary>
        /// Adım satırlarını kurulum sırasına göre önceden basar. Böylece
        /// kullanıcı neyin sırada beklediğini de görür.
        /// </summary>
        private void BuildStepRows()
        {
            int top = pnlSteps.Padding.Top;

            int width = pnlSteps.ClientSize.Width - pnlSteps.Padding.Horizontal;

            foreach (DatabaseProvisionStep step in DatabaseInitializer.Steps)
            {
                StepRow row = new(
                    StepTitles[step],
                    top,
                    width,
                    _mutedText,
                    _mutedText,
                    _surface,
                    _stepToolTip);

                _rows[step] = row;

                pnlSteps.Controls.Add(row.Container);

                top += StepRow.Height;
            }
        }

        private void ApplyTheme()
        {
            pnlSurface.BackColor = _surface;
            pnlHeader.BackColor = _surfaceMuted;
            pnlFooter.BackColor = _surfaceMuted;

            pnlHeaderAccent.BackColor = SkinTheme.Primary;
            lblHeaderTitle.ForeColor = _text;
            lblHeaderSubtitle.ForeColor = _mutedText;

            pnlProgressTrack.BackColor = _border;
            pnlProgressFill.BackColor = SkinTheme.Primary;
            lblProgress.ForeColor = _mutedText;

            foreach (StepRow row in _rows.Values)
            {
                row.ApplyPending(_mutedText);
            }
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            await DetectAndPrepareAsync();
        }

        /// <summary>
        /// Veritabanının varlığını kontrol eder ve gerekiyorsa hazırlar.
        /// </summary>
        private async Task DetectAndPrepareAsync()
        {
            SetBusy(true, "Veritabanı kontrol ediliyor...");

            DatabaseFirstRunState state =
                await Task.Run(() => DatabaseInitializer.GetFirstRunStateAsync(Program.Services));

            CrashLog.Write("DatabaseSetup", $"Database state: {state}");

            switch (state)
            {
                case DatabaseFirstRunState.Exists:
                    // Veritabanı var: kullanıcıyı bekletmeden güncelle ve pencereyi
                    // kapat. Pencerenin tek işi "bekle" bilgisini vermekti.
                    await Task.Run(() => DatabaseInitializer.InitializeAsync(
                        Program.Services,
                        appVersion: UpdateChecker.CurrentVersionString()));

                    _shouldContinueToLogin = true;
                    Close();

                    break;

                case DatabaseFirstRunState.Missing:
                    await CreateDatabaseAsync();
                    break;

                default:
                    Fail(
                        "Veritabanı sunucusuna ulaşılamıyor. "
                        + "Yerel SQL Server / LocalDB çalışıyor mu ve bağlantı "
                        + "adresi doğru mu? Ayrıntı için logs\\crash.log dosyasına bakınız.");
                    break;
            }
        }

        private async Task CreateDatabaseAsync()
        {
            pnlSteps.Visible = true;

            SetBanner(BannerState.Working);

            var progress = new Progress<DatabaseProvisionProgress>(Apply);

            _cts = new CancellationTokenSource();

            try
            {
                await Task.Run(
                    () => DatabaseInitializer.InitializeAsync(
                        Program.Services,
                        progress,
                        _cts.Token,
                        UpdateChecker.CurrentVersionString()));

                Complete();
            }
            catch (OperationCanceledException)
            {
                Fail("İşlem durduruldu.");
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("DatabaseSetup.CreateDatabase", ex);
                Fail(ex.Message);
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }

        private void Apply(DatabaseProvisionProgress report)
        {
            if (SetStep(report.Step, report.State, report.Detail, report.Error))
            {
                UpdateProgress();
            }
        }

        /// <summary>Bir adımın durumunu günceller; durum değiştiyse true döner.</summary>
        private bool SetStep(
            DatabaseProvisionStep step,
            DatabaseProvisionStepState state,
            string? detail,
            string? error)
        {
            if (!_rows.TryGetValue(step, out StepRow? row))
            {
                return false;
            }

            switch (state)
            {
                case DatabaseProvisionStepState.Running:
                    _runningStep = step;
                    row.ApplyRunning(_text, SkinTheme.Question, detail);
                    return true;

                case DatabaseProvisionStepState.Completed:
                    _runningStep = null;
                    row.ApplyDone(_text, SkinTheme.Success, detail);
                    return true;

                case DatabaseProvisionStepState.Failed:
                    _runningStep = null;
                    row.ApplyFailed(_text, SkinTheme.Danger, error ?? detail);
                    return true;

                case DatabaseProvisionStepState.Skipped:
                    // Atlanan adım da tamamlanmış sayılır; yalnızca simge ve
                    // metin nötr kalır ki "atlandı" ile "yapıldı" ayrışsın.
                    _runningStep = null;
                    row.ApplySkipped(_text, SkinTheme.SecondaryText, detail);
                    return true;

                default:
                    return false;
            }
        }

        private void UpdateProgress()
        {
            int total = _rows.Count;

            int done = _rows.Values.Count(
                r => r.State is DatabaseProvisionStepState.Completed
                    or DatabaseProvisionStepState.Skipped);

            int width = pnlProgressTrack.ClientSize.Width;

            pnlProgressFill.Width =
                total == 0
                    ? 0
                    : (int)Math.Round(width * (double)done / total);

            lblProgress.Text = $"{done} / {total} adım tamamlandı";
        }

        private void SetBusy(bool busy, string status)
        {
            btnOk.Enabled = !busy;
            btnOk.Visible = !busy;
            btnRetry.Enabled = !busy;
            btnRetry.Visible = !busy;

            if (busy)
            {
                btnElevated.Enabled = false;
                btnElevated.Visible = false;
            }

            lblProgress.Text = status;

            pnlProgressFill.Width = 0;
        }

        private void Complete()
        {
            // Bildirimi hiç gönderilmemiş adımlar (örneğin kurum yoksa yıl
            // veritabanı açılmaz) "gerekmiyordu" olarak kapanır; aksi hâlde
            // sonsuza dek bekliyor gibi görünürlerdi.
            foreach (StepRow row in _rows.Values)
            {
                if (row.State == DatabaseProvisionStepState.Pending)
                {
                    row.ApplySkipped(_text, SkinTheme.SecondaryText, "gerekmiyordu");
                }
            }

            UpdateProgress();

            _shouldContinueToLogin = true;

            SetBanner(BannerState.Success);

            btnOk.Text = "Tamam";
            btnOk.Visible = true;
            btnOk.Enabled = true;
            btnOk.Focus();
            btnRetry.Visible = false;

            CrashLog.Write("DatabaseSetup", "Kurulum tamamlandi.");
        }

        private void Fail(string message)
        {
            // İstisna, adımın ortasında fırlatılmış olabilir; o adım "devam
            // ediyor" simgesiyle sonsuza dek kalmasın diye burada kapatılır.
            if (_runningStep is DatabaseProvisionStep failedStep
                && _rows.TryGetValue(failedStep, out StepRow? row))
            {
                row.ApplyFailed(_text, SkinTheme.Danger, "başarısız");
                _runningStep = null;

                UpdateProgress();
            }

            _shouldContinueToLogin = false;

            SetBanner(BannerState.Error, message);

            btnOk.Visible = false;

            btnRetry.Visible = true;
            btnRetry.Enabled = true;
            btnRetry.Focus();

            btnElevated.Visible = true;
            btnElevated.Enabled = true;

            CrashLog.Write("DatabaseSetup", "Kurulum basarisiz: " + message);
        }

        private void SetBanner(BannerState state, string? detail = null)
        {
            switch (state)
            {
                case BannerState.Checking:
                    pnlBannerAccent.BackColor = SkinTheme.Question;
                    lblBannerTitle.Text = "Uygulama hazırlanıyor";
                    lblBannerTitle.ForeColor = SkinTheme.Question;
                    lblBannerText.ForeColor = _text;
                    lblBannerText.Text =
                        detail ?? "Veritabanı kontrol ediliyor, lütfen bekleyin...";
                    break;

                case BannerState.Success:
                    pnlBannerAccent.BackColor = SkinTheme.Success;
                    lblBannerTitle.Text = "Kurulum tamamlandı";
                    lblBannerTitle.ForeColor = SkinTheme.Success;
                    lblBannerText.ForeColor = _text;
                    lblBannerText.Text =
                        detail
                        ?? "Veritabanı oluşturuldu ve sanal kayıtlar yüklendi. Artık giriş yapabilirsiniz.";
                    break;

                case BannerState.Error:
                    pnlBannerAccent.BackColor = SkinTheme.Danger;
                    lblBannerTitle.Text = "Kurulum tamamlanamadı";
                    lblBannerTitle.ForeColor = SkinTheme.Danger;
                    lblBannerText.ForeColor = _text;
                    lblBannerText.Text =
                        detail ?? "Beklenmeyen bir hata oluştu.";
                    break;

                default:
                    pnlBannerAccent.BackColor = SkinTheme.Warning;
                    lblBannerTitle.Text = "Veritabanı bulunamadı";
                    lblBannerTitle.ForeColor = SkinTheme.Warning;
                    lblBannerText.ForeColor = _text;
                    lblBannerText.Text =
                        detail ?? "Lütfen bekleyin, oluşturuluyor...";
                    break;
            }

            pnlBanner.BackColor =
                SkinTheme.Blend(_surface, pnlBannerAccent.BackColor, 0.10F);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Kurulum sürerken kapatılırsa yarım kalmış bir veritabanı
            // bırakılabilir; bu yüzden kapatma engellenir.
            if (_cts is not null && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                MsgBox.Notice(
                    this,
                    "Veritabanı hazırlığı sürüyor. Lütfen işlem tamamlanana kadar bekleyin.",
                    "İşlem Devam Ediyor");

                return;
            }

            base.OnFormClosing(e);
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            // Kurulum sürerken düğmeye basılması yalnızca onay bekler.
            if (_cts is not null)
            {
                return;
            }

            Close();
        }

        private async void BtnRetry_Click(object? sender, EventArgs e)
        {
            if (_cts is not null)
            {
                return;
            }

            // Yeniden denemede adım listesi ve ilerleme sıfırlanır.
            foreach (StepRow row in _rows.Values)
            {
                row.ApplyPending(_mutedText);
            }

            pnlSteps.Visible = false;

            await DetectAndPrepareAsync();
        }

        /// <summary>
        /// Yönetici haklarıyla <c>caa-provision provision --version &lt;kurulu&gt;</c>
        /// çalıştırır; başarılıysa kontrolü yeniden yürütür.
        ///
        /// <para>
        /// Kurulum modunda veritabanını normalde uygulama kendisi hazırlar;
        /// yönetici düğmesi "kullanıcı olarak başarılamadı" durumları için bir
        /// kaçış yoludur (ör. klasör/SQL hizmet yetkisi gerekiyorsa). Araç
        /// kurulumla <c>{app}\tools</c> klasörüne konur.
        /// </para>
        /// </summary>
        private async void BtnElevated_Click(object? sender, EventArgs e)
        {
            if (_cts is not null)
            {
                return;
            }

            btnElevated.Enabled = false;
            SetBusy(true, "Yönetici ile veritabanı hazırlanıyor...");

            try
            {
                var result = await Task.Run(ProvisioningProcess.RunProvision);

                switch (result.State)
                {
                    case ProvisioningRunState.Succeeded:
                        // Veritabanı hazır; kontrolü yeniden yürüt, girişe devam et.
                        foreach (StepRow row in _rows.Values)
                        {
                            row.ApplyPending(_mutedText);
                        }

                        pnlSteps.Visible = false;

                        await DetectAndPrepareAsync();
                        break;

                    case ProvisioningRunState.NotFound:
                        MsgBox.Notice(
                            this,
                            "caa-provision.exe bulunamadı. Program kurulum klasörüne "
                            + "\"tools\" altında kurulur; lütfen kurulumun eksiksiz "
                            + "olduğunu denetleyin.",
                            "Araç Bulunamadı");
                        break;

                    case ProvisioningRunState.Cancelled:
                        // Kullanıcı UAC istemini iptal etti; hiçbir işlem yapılmadı.
                        break;

                    default:
                        MsgBox.Notice(
                            this,
                            "Veritabanı hazırlığı başarısız oldu (çıkış kodu: "
                            + result.ExitCode + "). 'caa-provision provision' "
                            + "komutunu yönetici konsolunda elle deneyin.",
                            "Hazırlık Başarısız");
                        break;
                }
            }
            finally
            {
                btnElevated.Enabled = true;
            }
        }

        private enum BannerState
        {
            Checking,
            Working,
            Success,
            Error
        }

        /// <summary>
        /// Tek bir adım satırı: durum simgesi, başlık ve sağda özet rozet.
        ///
        /// <para>
        /// Satır konumla yerleştirilir; içindeki denetimler <c>Left</c> /
        /// <c>Fill</c> / <c>Right</c> olarak dock edilir. Etiketlerde
        /// <c>AutoSize</c> KAPALI olmalıdır: açık bırakıldığında denetim kendi
        /// metin genişliğine göre boyutlanır, <c>Dock</c> uygulanmaz ve sağdaki
        /// özet metni (adet, veritabanı adı) görünmez olur.
        /// </para>
        ///
        /// <para>
        /// Özet, sabit genişlikte bir <see cref="Label"/> olarak tasarlanmıştı.
        /// Veritabanı adları ("CAA_2026_DEMİRCİ AÇIK CEZA İNFAZ KURUMU
        /// MÜDÜRLÜĞÜ") bu genişliğe sığmadığı için kırpılıyordu. Artık özet,
        /// metnine göre ölçülüp büyüyen bir <b>rozet</b> (Panel + etiket)
        /// olarak çizilir: tek satırda kalır, kalan genişliğe göre kırpılır ve
        /// tam metin ipucunda gösterilir.
        /// </para>
        /// </summary>
        private sealed class StepRow
        {
            public const int Height = 34;

            private const int GlyphWidth = 30;

            /// <summary>Rosetin iç boşluğu (sol + sağ).</summary>
            private const int ChipPadding = 11;

            /// <summary>Rozetin satır yüksekliği.</summary>
            private const int ChipHeight = 22;

            /// <summary>
            /// Başlık için ayrılan en az genişlik. Rozet büyürken başlık
            /// ancak bu kadarın altına düşerse kendi metnini kırpma yoluna
            /// gider; özet okunabilir kalmaya devam eder.
            /// </summary>
            private const int TitleReserveWidth = 190;

            private static readonly Font ChipFont = new("Segoe UI", 8.5F);

            private readonly string _caption;
            private readonly Color _surface;
            private readonly ToolTip _toolTip;

            private string _detail = string.Empty;

            public StepRow(
                string caption,
                int top,
                int containerWidth,
                Color textColor,
                Color detailColor,
                Color surface,
                ToolTip toolTip)
            {
                _caption = caption;
                _surface = surface;
                _toolTip = toolTip;

                Container = new Panel
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackColor = Color.Transparent,
                    Location = new Point(0, top),
                    Size = new Size(containerWidth, Height)
                };

                Glyph = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Left,
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                    ForeColor = detailColor,
                    Size = new Size(GlyphWidth, Height),
                    Text = "○",
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // Rozet, metne göre büyür; bu yüzden kendi içinde "Fill" olan
                // bir etiket ve dışında tam yükseklikte bir panel var.
                Detail = new Label
                {
                    AutoSize = false,
                    AutoEllipsis = true,
                    Dock = DockStyle.Fill,
                    Font = ChipFont,
                    ForeColor = detailColor,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                DetailChip = new Panel
                {
                    Dock = DockStyle.Right,
                    Height = ChipHeight,
                    Padding = new Padding(ChipPadding, 0, ChipPadding, 0)
                };

                DetailChip.Controls.Add(Detail);

                Title = new Label
                {
                    AutoSize = false,
                    AutoEllipsis = true,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = textColor,
                    Text = caption,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Dock sırası ters çalışır: en son eklenen ilk yerleşir.
                Container.Controls.Add(Title);
                Container.Controls.Add(DetailChip);
                Container.Controls.Add(Glyph);

                // Pencere genişlediğinde rozet yeniden sığdırılır.
                Container.Resize += (_, _) => ResizeChip();
            }

            public Panel Container { get; }

            public Label Glyph { get; }

            public Label Title { get; }

            public Panel DetailChip { get; }

            public Label Detail { get; }

            public DatabaseProvisionStepState State { get; private set; }
                = DatabaseProvisionStepState.Pending;

            public void ApplyPending(Color mutedText)
            {
                State = DatabaseProvisionStepState.Pending;

                Glyph.Text = "○";
                Glyph.ForeColor = mutedText;

                Title.Text = _caption;
                Title.ForeColor = mutedText;

                SetDetail(string.Empty, mutedText);
            }

            public void ApplyRunning(Color text, Color accent, string? detail)
            {
                State = DatabaseProvisionStepState.Running;

                Glyph.Text = "◐";
                Glyph.ForeColor = accent;

                Title.ForeColor = text;

                SetDetail(detail ?? "yürüyor...", accent);
            }

            public void ApplyDone(Color text, Color accent, string? detail)
            {
                State = DatabaseProvisionStepState.Completed;

                Glyph.Text = "✔";
                Glyph.ForeColor = accent;

                Title.ForeColor = text;

                SetDetail(detail ?? "tamamlandı", accent);
            }

            public void ApplySkipped(Color text, Color accent, string? detail)
            {
                State = DatabaseProvisionStepState.Skipped;

                Glyph.Text = "✔";
                Glyph.ForeColor = accent;

                Title.ForeColor = text;

                SetDetail(detail ?? "atlandı", accent);
            }

            public void ApplyFailed(Color text, Color accent, string? detail)
            {
                State = DatabaseProvisionStepState.Failed;

                Glyph.Text = "✕";
                Glyph.ForeColor = accent;

                Title.ForeColor = text;

                SetDetail(detail ?? "başarısız", accent);
            }

            /// <summary>
            /// Sağdaki özet rozetini günceller. Metin boşsa rozet gizlenir;
            /// böylece henüz başlanmamış adımların satırı tertemiz görünür.
            /// </summary>
            private void SetDetail(string? text, Color accent)
            {
                _detail = text ?? string.Empty;

                if (_detail.Length == 0)
                {
                    DetailChip.Visible = false;
                    _toolTip.SetToolTip(Detail, string.Empty);

                    return;
                }

                DetailChip.Visible = true;

                // Rozetin zemini durum renginin çok seyrek karışımı; metin
                // rengi ise ölçülerek okunabilirliğe getirilmiş durum rengi.
                // Ham vurgu rengi, seyreltilmiş zeminde WCAG AA'nın altına
                // düşüp soluk görünebiliyordu.
                Color background = SkinTheme.Blend(
                    _surface,
                    accent,
                    SkinTheme.IsDarkSkin ? 0.14F : 0.07F);

                DetailChip.BackColor = background;
                Detail.ForeColor = SkinTheme.EnsureReadable(accent, background);
                Detail.Text = _detail;

                ResizeChip();

                // Kırpıldıysa tam metin ipucunda gösterilir.
                _toolTip.SetToolTip(Detail, _detail);
            }

            /// <summary>
            /// Rozet genişliğini metne göre hesaplar. Taşarsa kırpılır; kırpma
            /// etiketin kendi <c>AutoEllipsis</c> davranışıyla görünür olur.
            /// </summary>
            private void ResizeChip()
            {
                if (_detail.Length == 0)
                {
                    return;
                }

                int textWidth = TextRenderer.MeasureText(_detail, ChipFont).Width;

                // Rozet, başlığın hakkını bırakacak şekilde büyür: ne kadar
                // uzunsa o kadar geniş olur, ama başlık için asgari yer kalır.
                int ceiling = Container.ClientSize.Width - GlyphWidth - TitleReserveWidth;

                int width = Math.Min(textWidth + ChipPadding * 2, Math.Max(ceiling, ChipHeight));

                DetailChip.Width = Math.Max(width, ChipHeight);
                DetailChip.Height = ChipHeight;
            }
        }
    }
}