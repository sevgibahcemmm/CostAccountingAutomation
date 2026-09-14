using Cost.Accounting.Automation.Application.Auth;
using Cost.Accounting.Automation.Infrastructure;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors.Controls;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Drawing2D;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TS.MediatR;

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

        private bool _passwordVisible;
        private Guid _captchaChallengeId;

        public XtraLoginForm()
        {
            InitializeComponent();
            ConfigureIcons();

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
            Load += async (s, e) => await InitAsync();
            FormClosed += XtraLoginForm_FormClosed;
        }

        private void XtraLoginForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private async Task InitAsync()
        {
            btnLogin.Enabled = false;
            try
            {
                var captchaTask = RecreateCaptchaAsync();
                var dbInitTask = DatabaseInitializer.InitializeAsync(Program.Services);
                await Task.WhenAll(captchaTask, dbInitTask);
            }
            catch
            {
                ToastHelper.Show("Veritabanı başlatılırken bir hata oluştu. Lütfen uygulamayı yeniden başlatın.", ToastType.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private void ConfigureIcons()
        {
            pnlLeftLockGlyph.Text = string.Empty;
            pnlLeftLockGlyph.ImageOptions.SvgImage = SvgIcons.LockWhiteIcon;
            pnlLeftLockGlyph.ImageOptions.SvgImageSize = new Size(48, 48);

            _lblLogoIcon.Text = string.Empty;
            _lblLogoIcon.ImageOptions.SvgImage = SvgIcons.ShieldIcon;
            _lblLogoIcon.ImageOptions.SvgImageSize = new Size(54, 54);

            lblUserIcon.Text = string.Empty;
            lblUserIcon.ImageOptions.SvgImage = SvgIcons.UserIcon;
            lblUserIcon.ImageOptions.SvgImageSize = new Size(22, 22);

            lblPassIcon.Text = string.Empty;
            lblPassIcon.ImageOptions.SvgImage = SvgIcons.KeyIcon;
            lblPassIcon.ImageOptions.SvgImageSize = new Size(22, 22);

            lblTogglePassword.Text = string.Empty;
            lblTogglePassword.ImageOptions.SvgImage = SvgIcons.EyeIcon;
            lblTogglePassword.ImageOptions.SvgImageSize = new Size(22, 22);

            btnLogin.ImageOptions.SvgImage = SvgIcons.NextIcon;
            btnLogin.ImageOptions.SvgImageSize = new Size(22, 22);
            btnLogin.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.RightCenter;
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
            if (string.IsNullOrWhiteSpace(userOrEmail) || string.IsNullOrEmpty(txtPassword.Text))
            {
                ToastHelper.Show("Kullanıcı adı ve şifre boş olamaz.", ToastType.Error);
                return;
            }

            btnLogin.Enabled = false;
            WaitForm waitForm = CreateWaitForm();
            try
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
                    new LoginCommand(userOrEmail, txtPassword.Text),
                    CancellationToken.None);

                if (!loginResult.IsSuccessful || loginResult.Data is null)
                {
                    ToastHelper.Show(AuthFormStyles.GetErrorText(loginResult.ErrorMessages), ToastType.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(loginResult.Data.Token))
                {
                    OpenMainPage(loginResult.Data.Token);
                }
                else
                {
                    ToastHelper.Show("Giriş işlemi tamamlanamadı.", ToastType.Error);
                }
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
                waitForm.Close();
                waitForm.Dispose();
                btnLogin.Enabled = true;
            }
        }

        private WaitForm CreateWaitForm()
        {
            return WaitFormHelper.Show<WaitForm>("Giriş yapılıyor...", "Lütfen bekleyin...");
        }

        private void OpenMainPage(string token)
        {
            var (userId, companyId, roleName) = DecodeToken(token);

            SessionClaimContext session = Program.Services.GetRequiredService<SessionClaimContext>();
            session.SetCurrentUser(userId, companyId, roleName, token);

            RibbonMainForm mainForm = Program.Services.GetRequiredService<RibbonMainForm>();
            Hide();
            mainForm.Show();
            ToastHelper.Show("Giriş başarılı.", ToastType.Success);
        }

        private static (Guid userId, Guid companyId, string roleName) DecodeToken(string token)
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            Guid userId = Guid.Parse(jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Guid companyId = Guid.Parse(jwt.Claims.First(c => c.Type == "companyId").Value);
            string roleName = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? string.Empty;

            return (userId, companyId, roleName);
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
            txtPassword.Properties.PasswordChar = _passwordVisible ? '\0' : '•';
            txtPassword.Properties.UseSystemPasswordChar = false;
            lblTogglePassword.ImageOptions.SvgImage = _passwordVisible ? SvgIcons.EyeOffIcon : SvgIcons.EyeIcon;
            txtPassword.Refresh();
        }

        // ----------------------------------------------------------------
        // Visual paint handlers
        // ----------------------------------------------------------------

        private void pnlLeft_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;

            var rect = ((Control)sender).ClientRectangle;

            using var brush = new LinearGradientBrush(
                rect,
                LeftPanelGradient[0],
                LeftPanelGradient[2],
                35f);
            var blend = new ColorBlend { Positions = new[] { 0f, 0.55f, 1f } };
            blend.Colors = LeftPanelGradient;
            brush.InterpolationColors = blend;
            g.FillRectangle(brush, rect);

            DrawGlowCircle(g, rect.Width - 40, -90, 300, 96, 165, 250);
            DrawGlowCircle(g, -120, rect.Height - 200, 260, 139, 92, 246);
            DrawGlowCircle(g, rect.Width - 200, rect.Height - 140, 180, 56, 189, 248);

            using var dotBrush = new SolidBrush(Color.FromArgb(14, 255, 255, 255));
            int spacing = 40;
            for (int x = 16; x < rect.Width; x += spacing)
            {
                for (int y = 16; y < rect.Height; y += spacing)
                {
                    g.FillEllipse(dotBrush, x, y, 2, 2);
                }
            }

            using var thinGlowPen = new Pen(Color.FromArgb(70, Color.FromArgb(129, 140, 248)), 1.4f);
            thinGlowPen.StartCap = LineCap.Round;
            thinGlowPen.EndCap = LineCap.Round;
            g.DrawArc(thinGlowPen, rect.Width - 240, -20, 300, 240, 200, 140);
            g.DrawArc(thinGlowPen, -150, rect.Height - 200, 300, 240, 20, 130);

            using var accentLinePen = new Pen(Color.FromArgb(90, Color.FromArgb(167, 139, 250)), 2.2f);
            accentLinePen.StartCap = LineCap.Round;
            accentLinePen.EndCap = LineCap.Round;
            g.DrawLine(accentLinePen, rect.Width / 2 - 42, 302, rect.Width / 2 + 42, 302);
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