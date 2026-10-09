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
        private const string YearPlaceholder = "Mali yıl seçin";

        /// <summary>
        /// Kullanıcı adı girilmeden önce seçim kutularında görünen metin.
        /// </summary>
        /// <remarks>
        /// Önceden yalnızca "Kullanıcı adı girin" yazıyordu; bu, özellikle Release
        /// derlemesinde <i>veritabanında kurum yok</i> izlenimi veriyordu. Metin
        /// artık eylemi ve <b>neden</b> gerektiğini birlikte söyler.
        /// </remarks>
        private const string ScopePendingText = "Önce kullanıcı adınızı yazın";
        private const string NoOpenYearText = "Açık mali yıl yok";

        /// <summary>Aktif mali yıl: içinde bulunulan takvim yılı.</summary>
        private static int ActiveYear => DateTime.Now.Year;

        private readonly System.Windows.Forms.Timer _fadeTimer = new() { Interval = 15 };
        private readonly System.Windows.Forms.Timer _userNameTimer = new() { Interval = 400 };
        private Bitmap? _leftBackground;
        private Task? _initTask;
        private bool _passwordVisible;

        /// <summary>
        /// Çözülen oturum kapsamının <c>sys_admin</c> olup olmadığı.
        /// </summary>
        private bool _isSysAdminScope;
        private Guid _captchaChallengeId;
        private List<LoginScopeCompanyDto> _companies = [];

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
            _userNameTimer.Tick += UserNameTimer_Tick;

            btnLogin.Appearance.Options.UseBackColor = true;

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

            // Kurum ve yıl listeleri kullanıcı adına bağlıdır; kullanıcı adı
            // değiştikçe kapsam yeniden çözülür. Aşağıdaki DEBUG bloğu bu
            // bağlantıdan sonra çalışır ve ön dolgu için de tetikler.
            txtUserName.TextChanged += (_, _) => QueueScopeResolve();
            txtUserName.KeyDown += TxtUserName_KeyDown;
            txtPassword.KeyDown += TxtPassword_KeyDown;

            Load += (_, _) => _initTask = InitAsync();
            FormClosed += XtraLoginForm_FormClosed;

#if DEBUG
            // Yalnızca geliştirme kolaylığı; Release derlemesinde alanlar boş gelir.
           // txtUserName.EditValue = "sevgibahcemm";
          //  txtPassword.EditValue = "61785";
#endif
        }

        private void QueueScopeResolve()
        {
            _userNameTimer.Stop();
            _userNameTimer.Start();
        }

        /// <summary>Kullanıcı adı kutusunda Enter'a basılınca kapsam hemen çözülür.</summary>
        private async void TxtUserName_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            _userNameTimer.Stop();
            await ResolveScopeAsync();
            txtPassword.Focus();
        }

        /// <summary>Şifre kutusunda Enter'a basılınsa doğrudan giriş denenir.</summary>
        private void TxtPassword_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnLogin.Enabled)
            {
                e.SuppressKeyPress = true;
                btnLogin.PerformClick();
            }
        }

        private void XtraLoginForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _fadeTimer.Stop();
            _fadeTimer.Dispose();
            _userNameTimer.Stop();
            _userNameTimer.Dispose();
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
            ResetScope();
            try
            {
                await RecreateCaptchaAsync();

                // Alan ön doluysa (ör. geliştirme modunda) TextChanged tetiklenmez;
                // kapsam burada bir kez daha çözülür.
                await ResolveScopeAsync();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Veritabanı başlatılırken bir hata oluştu: " + ex.Message, ToastType.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                ActiveControl = txtUserName;
            }
        }

        private void UserNameTimer_Tick(object? sender, EventArgs e)
        {
            _userNameTimer.Stop();
            _ = ResolveScopeAsync();
        }

        /// <summary>
        /// Kullanıcı adına karşılık gelen kurumu ve açık mali yılları çözer.
        /// Kurum bilgisi her zaman görünür; yalnızca sys_admin listeden
        /// değiştirebilir, diğer kullanıcılar kendi kurumuna kilitlidir.
        /// </summary>
        private async Task ResolveScopeAsync()
        {
            string userName = txtUserName.EditValue?.ToString()?.Trim() ?? string.Empty;

            if (userName.Length == 0)
            {
                ResetScope();
                return;
            }

            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await mediator.Send(new LoginScopeGetQuery(userName), CancellationToken.None);

            if (!result.IsSuccessful)
            {
                ResetScope();
                lookUpYear.Properties.NullText = ScopePendingText;
                return;
            }

            // Kullanıcı bulunamadıysa şifre doğrulanana kadar hiçbir bilgi verilmez.
            if (result.Data is not { } userScope)
            {
                ResetScope();
                return;
            }

            ApplyScope(userScope);
        }

        /// <summary>
        /// Kullanıcı adı henüz girilmediği duruma döner: kurum ve mali yıl
        /// listeleri <b>pasif</b> (açılamaz) yapılır.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Neden listeler boşaltılıp pasifleştiriliyor? Kullanıcı adı yazılmadan
        /// hangi kurumların sistemde olduğu, kaç kullanıcı bulunduğu ya da
        /// kullanıcının hangi kuruma bağlı olduğu <b>şifre doğrulanmadan</b>
        /// söylenmemelidir; aksi halde biri rastgele adlar deneyerek sistemi
        /// haritalayabilir.
        /// </para>
        /// <para>
        /// Daha önce listeler boş bırakılıyordu. Bu, kullanıcıya "veritabanında
        /// kurum yok" izlenimi veriyordu; oysa ekran açılışında hiç sorgu
        /// yapılmamıştır. Pasifleştirmek hem bu yanılgıyı kaldırır hem de
        /// güvenlik kararını korur: kullanıcı adı yazılınca liste açılır.
        /// </para>
        /// </remarks>
        private void ResetScope()
        {
            _companies = [];

            lookUpCompany.Properties.DataSource = null;
            lookUpCompany.EditValue = null;
            lookUpCompany.Properties.NullText = ScopePendingText;

            lookUpYear.Properties.DataSource = null;
            lookUpYear.EditValue = null;
            lookUpYear.Properties.NullText = ScopePendingText;

            SetScopeEnabled(false);
        }

        /// <summary>
        /// Kurum ve mali yıl seçim kutularını açar veya kapatır.
        /// </summary>
        /// <remarks>
        /// Normal kullanıcı kurumu listeden değiştiremez; yalnızca
        /// <c>sys_admin</c> seçebilir. Bu yüzden asıl karar yetkiye aittir ve
        /// <see cref="ApplyScope"/> içinde verilir; metot yalnızca
        /// "kullanıcı adı girilene kadar hiç açılmasın" kuralını uygular.
        /// </remarks>
        private void SetScopeEnabled(bool enabled)
        {
            lookUpCompany.Enabled = enabled;
            lookUpYear.Enabled = enabled;
            lookUpCompany.ReadOnly = enabled == false || !IsSysAdminScope();
        }

        private void ApplyScope(LoginScopeDto userScope)
        {
            _companies = userScope.Companies;
            _isSysAdminScope = userScope.IsSysAdmin;

            lookUpCompany.Properties.DataSource = _companies;
            lookUpCompany.Properties.ValueMember = nameof(LoginScopeCompanyDto.CompanyId);
            lookUpCompany.Properties.DisplayMember = nameof(LoginScopeCompanyDto.CompanyName);
            lookUpCompany.Properties.BestFitMode = BestFitMode.BestFit;
            lookUpCompany.Properties.NullText = CompanyPlaceholder;
            lookUpCompanyView.OptionsBehavior.AutoPopulateColumns = false;
            lookUpCompanyView.Columns.Clear();
            GridColumn companyColumn = lookUpCompanyView.Columns.AddField(nameof(LoginScopeCompanyDto.CompanyName));
            companyColumn.Caption = "Kurum";
            companyColumn.VisibleIndex = 0;
            lookUpCompanyView.BestFitColumns();

            lookUpYear.Properties.ValueMember = nameof(LoginScopeYearDto.CompanyYearId);
            lookUpYear.Properties.DisplayMember = nameof(LoginScopeYearDto.Year);
            lookUpYear.Properties.BestFitMode = BestFitMode.BestFit;
            lookUpYear.Properties.NullText = YearPlaceholder;
            lookUpYearView.OptionsBehavior.AutoPopulateColumns = false;
            lookUpYearView.Columns.Clear();
            GridColumn yearColumn = lookUpYearView.Columns.AddField(nameof(LoginScopeYearDto.Year));
            yearColumn.Caption = "Mali Yıl";
            yearColumn.VisibleIndex = 0;
            lookUpYearView.Columns.AddField(nameof(LoginScopeYearDto.DatabaseName)).Caption = "Veritabanı";
            lookUpYearView.BestFitColumns();

            // Normal kullanıcı listeden kurum seçemez; sys_admin seçebilir.
            // Seçim kutuları yalnızca kapsam çözüldükten sonra açılır.
            SetScopeEnabled(true);

            // Normal kullanıcının kurumu tekildir; sys_admin'de de kendi kurumu varsayılan gelir.
            lookUpCompany.EditValue = userScope.CompanyId;
            LoadYearsForSelectedCompany();
        }

        /// <summary>
        /// Çözülen kapsamın <c>sys_admin</c> olup olmadığı.
        /// </summary>
        /// <remarks>
        /// <c>ApplyScope</c> çalışmadan önce <see langword="false"/> döner; böylece
        /// kapsam yokken seçim kutusu yanlışlıkla açılmaz.
        /// </remarks>
        private bool IsSysAdminScope() => _isSysAdminScope;

        private void LoadYearsForSelectedCompany()
        {
            if (lookUpCompany.EditValue is not Guid companyId)
            {
                lookUpYear.Properties.DataSource = null;
                lookUpYear.EditValue = null;
                lookUpYear.Properties.NullText = _companies.Count == 0 ? ScopePendingText : NoOpenYearText;
                return;
            }

            LoginScopeCompanyDto? company = _companies.FirstOrDefault(c => c.CompanyId == companyId);
            List<LoginScopeYearDto> openYears = company?.Years
                .Where(y => !y.IsClosed)
                .OrderByDescending(y => y.Year == ActiveYear)
                .ThenByDescending(y => y.Year)
                .ToList() ?? [];

            lookUpYear.Properties.DataSource = openYears;

            // Aktif mali yıl içinde bulunulan yıldır; tanımlıysa o seçilir.
            LoginScopeYearDto? activeYear = openYears.FirstOrDefault(y => y.Year == ActiveYear)
                ?? openYears.FirstOrDefault();
            lookUpYear.EditValue = activeYear?.CompanyYearId;

            lookUpYear.Properties.NullText = company is null
                ? ScopePendingText
                : openYears.Count == 0
                    ? NoOpenYearText
                    : openYears.Any(y => y.Year == ActiveYear)
                        ? YearPlaceholder
                        : $"{ActiveYear} yılı tanımlı değil";
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
                txtCaptchaResult.Text = string.Empty;
            }
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

            if (string.IsNullOrWhiteSpace(userOrEmail) || string.IsNullOrEmpty(txtPassword.Text))
            {
                ToastHelper.Show("Kullanıcı adı ve şifre boş olamaz.", ToastType.Error);
                return;
            }

            // Kurum, kullanıcının kendi kaydından gelir; yalnızca sys_admin seçebilir.
            if (_companies.Count == 0)
            {
                ToastHelper.Show("Kullanıcı bilgileri doğrulanamadı. Kullanıcı adını kontrol edin.", ToastType.Error);
                txtUserName.Focus();
                return;
            }

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
            var (userId, _, roleName, userFullName) = DecodeToken(token);

            // Token kullanıcının kendi kurumunu taşır; sys_admin başka bir kurum
            // seçtiyse oturum bağlamı seçilen kuruma yönlendirilir.
            SessionClaimContext session = Program.Services.GetRequiredService<SessionClaimContext>();
            session.SetCurrentUser(userId, year.CompanyId, roleName, userFullName, token);

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