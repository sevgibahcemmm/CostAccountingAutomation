using Cost.Accounting.Automation.Application.AccountingYears;
using Cost.Accounting.Automation.Application.Auth;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.Utils;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Drawing2D;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TS.MediatR;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class XtraLoginForm : DevExpress.XtraEditors.XtraForm
    {
        private static readonly Color[] LeftPanelGradient = new[]
        {
            Color.FromArgb(15, 23, 42),
            Color.FromArgb(30, 27, 75),
            Color.FromArgb(30, 58, 138)
        };

        private const string CompanyPlaceholder = "Kurum seçin";
        private const string CompanyLoadingText = "Kurumlar yükleniyor...";
        private const string YearPlaceholder = "Mali yıl seçin";

        private readonly System.Windows.Forms.Timer _fadeTimer = new() { Interval = 15 };
        private Bitmap? _leftBackground;
        private Task? _initTask;
        private bool _passwordVisible;
        private Guid _captchaChallengeId;
        private List<LoginScopeDto> _scopes = [];

        public XtraLoginForm()
        {
            InitializeComponent();

            // Form, içindeki tüm kontroller çizilmeden ekranda görünmesin diye şeffaf açılır;
            // OnShown içinde her şey çizildikten sonra yumuşakça belirir.
            Opacity = 0;
            DoubleBuffered = true;
            EnableDoubleBuffering(
                pnlLeft, pnlLeftBadge, pnlRight, pnlUserBadge,
                pnlCompanyBox, pnlYearBox, pnlUserNameBox, pnlPasswordBox, pnlCaptchaResult);
            _fadeTimer.Tick += FadeTimer_Tick;

#if DEBUG
            // Yalnızca geliştirme kolaylığı; Release derlemesinde alanlar boş gelir.
            txtUserName.EditValue = "Admin";
            txtPassword.EditValue = "1";
#endif

            btnLogin.Appearance.Options.UseBackColor = false;
            btnLogin.Appearance.BackColor = Color.Transparent;

            lnkForgot.Click += LnkForgot_Click;
            pnlUserNameBox.Paint += AuthFormStyles.RoundedField_Paint;
            txtUserName.Enter += AuthFormStyles.Field_Enter;
            txtUserName.Leave += AuthFormStyles.Field_Leave;
            pnlPasswordBox.Paint += AuthFormStyles.RoundedField_Paint;
            txtPassword.Enter += AuthFormStyles.Field_Enter;
            txtPassword.Leave += AuthFormStyles.Field_Leave;
            pnlCaptchaResult.Paint += AuthFormStyles.RoundedField_Paint;
            txtCaptchaResult.Enter += AuthFormStyles.Field_Enter;
            txtCaptchaResult.Leave += AuthFormStyles.Field_Leave;
            pnlCompanyBox.Paint += AuthFormStyles.RoundedField_Paint;
            lookUpCompany.Enter += AuthFormStyles.Field_Enter;
            lookUpCompany.Leave += AuthFormStyles.Field_Leave;
            pnlYearBox.Paint += AuthFormStyles.RoundedField_Paint;
            lookUpYear.Enter += AuthFormStyles.Field_Enter;
            lookUpYear.Leave += AuthFormStyles.Field_Leave;

            lookUpCompany.EditValueChanged += (_, _) => LoadYearsForSelectedCompany();
            Load += (_, _) => _initTask = InitAsync();
            FormClosed += XtraLoginForm_FormClosed;
        }

        private void XtraLoginForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _fadeTimer.Stop();
            _fadeTimer.Dispose();
            _leftBackground?.Dispose();
            System.Windows.Forms.Application.Exit();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // WS_EX_COMPOSITED: alt kontrollerin tamamı tek seferde, çift tamponlu çizilir.
                // Ekranda hâlâ bozulma olursa bu satırı kaldırın.
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Kurum/yıl listeleri ve captcha yüklenene kadar (en fazla 2 sn) form şeffaf kalır;
            // böylece açıldığında hiçbir şey sonradan değişmez, titreme olmaz.
            if (_initTask is not null)
            {
                await Task.WhenAny(_initTask, Task.Delay(2000));
            }

            if (IsDisposed)
            {
                return;
            }

            Refresh();
            _fadeTimer.Start();
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            Opacity = Math.Min(1d, Opacity + 0.12d);
            if (Opacity >= 1d)
            {
                _fadeTimer.Stop();
            }
        }

        private static void EnableDoubleBuffering(params Control[] controls)
        {
            System.Reflection.PropertyInfo? property = typeof(Control).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            foreach (Control control in controls)
            {
                property?.SetValue(control, true);
            }
        }

        private async Task InitAsync()
        {
            btnLogin.Enabled = false;
            lookUpCompany.Properties.NullText = CompanyLoadingText;
            try
            {
                await LoadScopeAsync();
                await RecreateCaptchaAsync();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Veritabanı başlatılırken bir hata oluştu: " + ex.Message, ToastType.Error);
            }
            finally
            {
                if (lookUpCompany.Properties.NullText == CompanyLoadingText)
                {
                    lookUpCompany.Properties.NullText = CompanyPlaceholder;
                }

                btnLogin.Enabled = true;

                ActiveControl = lookUpCompany.EditValue is null
                    ? lookUpCompany
                    : string.IsNullOrEmpty(txtUserName.Text) ? txtUserName : txtPassword;
            }
        }

        /// <summary>
        /// Kurum ve mali yıl listelerini master veritabanından doldurur.
        /// Listeler yalnızca açık yılları içerir; kapalı yıl seçilemez.
        /// </summary>
        private async Task LoadScopeAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await mediator.Send(new LoginScopeGetQuery(), CancellationToken.None);

            if (!result.IsSuccessful || result.Data is null)
            {
                ToastHelper.Show(
                    AuthFormStyles.GetErrorText(result.ErrorMessages),
                    ToastType.Error);
                lookUpCompany.Properties.NullText = "Kurum listesi alınamadı";
                return;
            }

            _scopes = result.Data;

            lookUpCompany.Properties.DataSource = _scopes;
            lookUpCompany.Properties.ValueMember = nameof(LoginScopeDto.CompanyId);
            lookUpCompany.Properties.DisplayMember = nameof(LoginScopeDto.CompanyName);
            lookUpCompany.Properties.BestFitMode = BestFitMode.BestFit;
            lookUpCompanyView.OptionsBehavior.AutoPopulateColumns = false;
            lookUpCompanyView.Columns.Clear();
            GridColumn companyColumn = lookUpCompanyView.Columns.AddField(nameof(LoginScopeDto.CompanyName));
            companyColumn.Caption = "Kurum";
            companyColumn.VisibleIndex = 0;
            lookUpCompanyView.BestFitColumns();

            lookUpYear.Properties.ValueMember = nameof(LoginScopeYearDto.CompanyYearId);
            lookUpYear.Properties.DisplayMember = nameof(LoginScopeYearDto.Year);
            lookUpYear.Properties.BestFitMode = BestFitMode.BestFit;
            lookUpYearView.OptionsBehavior.AutoPopulateColumns = false;
            lookUpYearView.Columns.Clear();
            GridColumn yearColumn = lookUpYearView.Columns.AddField(nameof(LoginScopeYearDto.Year));
            yearColumn.Caption = "Mali Yıl";
            yearColumn.VisibleIndex = 0;
            lookUpYearView.Columns.AddField(nameof(LoginScopeYearDto.DatabaseName)).Caption = "Veritabanı";
            lookUpYearView.BestFitColumns();

            if (_scopes.Count == 1)
            {
                lookUpCompany.EditValue = _scopes[0].CompanyId;
            }
        }

        private void LoadYearsForSelectedCompany()
        {
            if (lookUpCompany.EditValue is not Guid companyId)
            {
                lookUpYear.Properties.DataSource = null;
                return;
            }

            LoginScopeDto? scope = _scopes.FirstOrDefault(s => s.CompanyId == companyId);
            List<LoginScopeYearDto> openYears = scope?.Years
                .Where(y => !y.IsClosed)
                .OrderByDescending(y => y.Year)
                .ToList() ?? [];

            lookUpYear.Properties.DataSource = openYears;
            lookUpYear.EditValue = openYears.Count > 0 ? openYears[0].CompanyYearId : null;

            lookUpYear.Properties.NullText = openYears.Count == 0 && scope is not null
                ? "Açık mali yıl yok"
                : YearPlaceholder;
        }

        private async Task RecreateCaptchaAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await mediator.Send(new CreateCaptchaCommand(), CancellationToken.None);

            if (result.IsSuccessful && result.Data is not null)
            {
                _captchaChallengeId = result.Data.ChallengeId;
                lblCaptchaQuestion.Text = result.Data.Question;
                txtCaptchaResult.Text = AutoSolveCaptcha(result.Data.Question);
            }
        }

        private static string AutoSolveCaptcha(string question)
        {
            try
            {
                string[] nums = System.Text.RegularExpressions.Regex.Matches(question, @"\d+")
                    .Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => m.Value)
                    .ToArray();

                if (nums.Length >= 3
                    && int.TryParse(nums[0], out int a)
                    && int.TryParse(nums[1], out int b))
                {
                    return $"{a + b} {nums[^1]}";
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        private void lblCaptchaQuestion_Click(object? sender, EventArgs e)
        {
            _ = RecreateCaptchaAsync();
        }

        // ----------------------------------------------------------------
        // Login flow
        // ----------------------------------------------------------------

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string userOrEmail = txtUserName.Text.Trim();

            if (lookUpCompany.EditValue is not Guid companyId)
            {
                ToastHelper.Show("Kurum seçmelisiniz.", ToastType.Error);
                lookUpCompany.Focus();
                return;
            }

            if (lookUpYear.EditValue is not Guid companyYearId)
            {
                ToastHelper.Show("Mali yıl seçmelisiniz.", ToastType.Error);
                lookUpYear.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(userOrEmail) || string.IsNullOrEmpty(txtPassword.Text))
            {
                ToastHelper.Show("Kullanıcı adı ve şifre boş olamaz.", ToastType.Error);
                return;
            }

            btnLogin.Enabled = false;
            try
            {
                string? token = null;

                // Bekleme penceresi yalnızca doğrulama/giriş isteğini kapsar;
                // ana sayfa bekleme kapandıktan sonra gösterilir.
                await LoadingHelper.RunAsync(
                    async () =>
                    {
                        using var scope = Program.Services.CreateScope();
                        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                        var captchaResult = await mediator.Send(
                            new ValidateCaptchaCommand(_captchaChallengeId, txtCaptchaResult.Text),
                            CancellationToken.None);

                        if (!captchaResult.IsSuccessful || captchaResult.Data != true)
                        {
                            string msg = captchaResult.IsSuccessful
                                ? "Matematik sorusu veya doğrulama kodu hatalı. Yeniden deneyin."
                                : AuthFormStyles.GetErrorText(captchaResult.ErrorMessages);

                            ToastHelper.Show(msg, ToastType.Error);
                            await RecreateCaptchaAsync();
                            return;
                        }

                        var loginResult = await mediator.Send(
                            new LoginCommand(userOrEmail, txtPassword.Text, companyId),
                            CancellationToken.None);

                        if (!loginResult.IsSuccessful || loginResult.Data is null)
                        {
                            ToastHelper.Show(AuthFormStyles.GetErrorText(loginResult.ErrorMessages), ToastType.Error);
                            return;
                        }

                        token = loginResult.Data.Token;
                    },
                    caption: "Giriş yapılıyor...",
                    description: "Lütfen bekleyin...");

                if (string.IsNullOrEmpty(token))
                {
                    ToastHelper.Show("Giriş işlemi tamamlanamadı.", ToastType.Error);
                    return;
                }

                // Yıl veritabanını aç ve iş bağlamını yıla yönlendir. Bu adım
                // başarısız olursa ana pencere hiç açılmaz.
                AccountingYearSelectResult? year = null;

                await LoadingHelper.RunAsync(
                    async () =>
                    {
                        using var scope = Program.Services.CreateScope();
                        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                        var yearResult = await mediator.Send(
                            new AccountingYearSelectCommand(companyYearId),
                            CancellationToken.None);

                        if (!yearResult.IsSuccessful || yearResult.Data is null)
                        {
                            ToastHelper.Show(
                                AuthFormStyles.GetErrorText(yearResult.ErrorMessages),
                                ToastType.Error);
                            return;
                        }

                        year = yearResult.Data;
                    },
                    caption: "Mali yıl hazırlanıyor...",
                    description: "Veritabanı açılıyor, lütfen bekleyin...");

                if (year is null)
                {
                    await RecreateCaptchaAsync();
                    return;
                }

                OpenMainPage(token, year);
            }
            catch (ValidationException ex)
            {
                ToastHelper.Show(AuthFormStyles.GetValidationText(ex), ToastType.Error);
                await RecreateCaptchaAsync();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Giriş sırasında bir hata oluştu: " + ex.Message, ToastType.Error);
                await RecreateCaptchaAsync();
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private void OpenMainPage(string token, AccountingYearSelectResult year)
        {
            var (userId, companyId, roleName, userFullName) = DecodeToken(token);

            SessionClaimContext session = Program.Services.GetRequiredService<SessionClaimContext>();
            session.SetCurrentUser(userId, companyId, roleName, userFullName, token);

            RibbonMainForm mainForm = Program.Services.GetRequiredService<RibbonMainForm>();
            Hide();
            mainForm.Show();

            string message = year.DatabaseCreated
                ? $"{year.CompanyName} / {year.Year} mali yılı açıldı."
                : $"{year.CompanyName} / {year.Year} mali yılı hazırlandı.";

            ToastHelper.Show("Giriş başarılı. " + message, ToastType.Success);
        }

        private static (Guid userId, Guid companyId, string roleName, string userFullName) DecodeToken(string token)
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            Guid userId = Guid.Parse(jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Guid companyId = Guid.Parse(jwt.Claims.First(c => c.Type == "companyId").Value);
            string roleName = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? string.Empty;
            string userFullName = jwt.Claims.FirstOrDefault(c => c.Type == "fullName")?.Value ?? string.Empty;

            return (userId, companyId, roleName, userFullName);
        }

        private void LnkForgot_Click(object? sender, EventArgs e)
        {
            using var scope = Program.Services.CreateScope();
            var forgotForm = scope.ServiceProvider.GetRequiredService<ForgotPasswordForm>();
            forgotForm.ShowDialog(this);
        }

        private void lblTogglePassword_Click(object? sender, EventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            txtPassword.Properties.PasswordChar = _passwordVisible ? '\0' : '\u2022';
            txtPassword.Properties.UseSystemPasswordChar = false;
            lblTogglePassword.ImageOptions.SvgImage = _passwordVisible ? DxIcon.EyeOff : DxIcon.Eye;
            txtPassword.Refresh();
        }

        // ----------------------------------------------------------------
        // Visual paint handlers
        // ----------------------------------------------------------------

        private void pnlLeft_Paint(object sender, PaintEventArgs e)
        {
            Size size = pnlLeft.ClientSize;
            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            // Arka plan (gradyan, noktalar, yaylar, özellik listesi) yalnızca boyut
            // değiştiğinde yeniden üretilir; her paint'te sadece bitmap kopyalanır.
            if (_leftBackground is null || _leftBackground.Size != size)
            {
                _leftBackground?.Dispose();
                _leftBackground = BuildLeftBackground(size, pnlLeft.DeviceDpi / 96f);
            }

            e.Graphics.DrawImageUnscaled(_leftBackground, 0, 0);
        }

        private static Bitmap BuildLeftBackground(Size size, float scale)
        {
            Bitmap bitmap = new(size.Width, size.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            Rectangle rect = new(Point.Empty, size);

            using (var brush = new LinearGradientBrush(rect, LeftPanelGradient[0], LeftPanelGradient[2], 35f))
            {
                var blend = new ColorBlend { Positions = new[] { 0f, 0.55f, 1f } };
                blend.Colors = LeftPanelGradient;
                brush.InterpolationColors = blend;
                g.FillRectangle(brush, rect);
            }

            DrawGlowCircle(g, rect.Width - 40, -90, 300, 96, 165, 250);
            DrawGlowCircle(g, -120, rect.Height - 200, 260, 139, 92, 246);
            DrawGlowCircle(g, rect.Width - 200, rect.Height - 140, 180, 56, 189, 248);

            using (var dotBrush = new SolidBrush(Color.FromArgb(14, 255, 255, 255)))
            {
                int spacing = 40;
                for (int x = 16; x < rect.Width; x += spacing)
                {
                    for (int y = 16; y < rect.Height; y += spacing)
                    {
                        g.FillEllipse(dotBrush, x, y, 2, 2);
                    }
                }
            }

            using (var thinGlowPen = new Pen(Color.FromArgb(70, Color.FromArgb(129, 140, 248)), 1.4f))
            {
                thinGlowPen.StartCap = LineCap.Round;
                thinGlowPen.EndCap = LineCap.Round;
                g.DrawArc(thinGlowPen, rect.Width - 240, -20, 300, 240, 200, 140);
                g.DrawArc(thinGlowPen, -150, rect.Height - 200, 300, 240, 20, 130);
            }

            // Başlık bloğunun altındaki ince süsleme çizgisi.
            using (var accentLinePen = new Pen(Color.FromArgb(120, Color.FromArgb(167, 139, 250)), 2.2f))
            {
                accentLinePen.StartCap = LineCap.Round;
                accentLinePen.EndCap = LineCap.Round;
                float lineY = 288 * scale;
                g.DrawLine(accentLinePen, rect.Width / 2f - 42 * scale, lineY, rect.Width / 2f + 42 * scale, lineY);
            }

            return bitmap;
        }

        private void pnlLeftBadge_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            var control = (Control)sender;
            int size = Math.Min(control.Width, control.Height);

            for (int i = 3; i >= 1; i--)
            {
                int ring = size - i * 7;
                if (ring <= 0) continue;
                using var ringPen = new Pen(Color.FromArgb(22, Color.FromArgb(129, 140, 248)), 2.5f);
                g.DrawEllipse(ringPen, (size - ring) / 2f, (size - ring) / 2f, ring, ring);
            }

            using var circlePath = new GraphicsPath();
            circlePath.AddEllipse(1, 1, size - 2, size - 2);

            using var haloBrush = new SolidBrush(Color.FromArgb(45, 139, 92, 246));
            g.FillEllipse(haloBrush, -4, -4, size + 8, size + 8);

            using var bgBrush = new LinearGradientBrush(
                new Rectangle(0, 0, size, size),
                Color.FromArgb(99, 102, 241),
                Color.FromArgb(168, 85, 247),
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(bgBrush, circlePath);

            using var innerGlow = new GraphicsPath();
            innerGlow.AddEllipse(10, 10, size - 20, size - 20);
            using var innerBrush = new SolidBrush(Color.FromArgb(35, 255, 255, 255));
            g.FillPath(innerBrush, innerGlow);

            using var borderPen = new Pen(Color.FromArgb(70, 255, 255, 255), 1.6f);
            g.DrawPath(borderPen, circlePath);
        }

        private void pnlUserBadge_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            var control = (Control)sender;
            int size = Math.Min(control.Width, control.Height);

            using var haloBrush = new SolidBrush(Color.FromArgb(40, 99, 102, 241));
            g.FillEllipse(haloBrush, -5, -5, size + 10, size + 10);

            using var bgBrush = new LinearGradientBrush(
                new Rectangle(0, 0, size, size),
                Color.FromArgb(238, 242, 255),
                Color.FromArgb(224, 231, 255),
                LinearGradientMode.ForwardDiagonal);

            using var ellipsePath = new GraphicsPath();
            ellipsePath.AddEllipse(1, 1, size - 2, size - 2);
            g.FillPath(bgBrush, ellipsePath);

            using var borderPen = new Pen(Color.FromArgb(199, 210, 254), 2f);
            g.DrawPath(borderPen, ellipsePath);
        }

        private static void DrawGlowCircle(Graphics g, float x, float y, float diameter, int r, int gr, int b)
        {
            using var brush = new SolidBrush(Color.FromArgb(24, r, gr, b));
            g.FillEllipse(brush, x, y, diameter, diameter);

            using var innerBrush = new SolidBrush(Color.FromArgb(16, r, gr, b));
            g.FillEllipse(innerBrush, x + diameter * 0.15f, y + diameter * 0.15f, diameter * 0.7f, diameter * 0.7f);
        }
    }
}