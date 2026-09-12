using Cost.Accounting.Automation.Application.Auth;
using Cost.Accounting.Automation.Infrastructure;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
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
        private static readonly Color LeftPanelColor = Color.FromArgb(15, 23, 42);

        private bool _passwordVisible;
        private Guid _captchaChallengeId;

        public XtraLoginForm()
        {
            InitializeComponent();
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
            btnLogin.Paint += AuthFormStyles.Button_Paint;
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
            txtPassword.Properties.PasswordChar = _passwordVisible ? '\0' : '\u2022';
            txtPassword.Properties.UseSystemPasswordChar = false;
            lblTogglePassword.Text = _passwordVisible ? "🙈" : "👁";
            txtPassword.Refresh();
        }

        // ----------------------------------------------------------------
        // Visual paint handlers
        // ----------------------------------------------------------------

        private void pnlLeft_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ((Control)sender).ClientRectangle;

            using var brush = new LinearGradientBrush(
                rect,
                LeftPanelColor,
                Color.FromArgb(30, 58, 108),
                LinearGradientMode.Vertical);
            g.FillRectangle(brush, rect);

            using var circle1 = new SolidBrush(Color.FromArgb(22, 96, 165, 250));
            g.FillEllipse(circle1, rect.Width - 150, -70, 220, 220);

            using var circle2 = new SolidBrush(Color.FromArgb(14, 96, 165, 250));
            g.FillEllipse(circle2, -60, rect.Height - 150, 200, 200);

            using var dotBrush = new SolidBrush(Color.FromArgb(10, 255, 255, 255));
            int spacing = 38;
            for (int x = 18; x < rect.Width; x += spacing)
            {
                for (int y = 18; y < rect.Height; y += spacing)
                {
                    g.FillEllipse(dotBrush, x, y, 2, 2);
                }
            }

            using var linePen = new Pen(Color.FromArgb(60, 96, 165, 250), 2f);
            linePen.StartCap = LineCap.Round;
            linePen.EndCap = LineCap.Round;
            g.DrawLine(linePen, rect.Width / 2 - 40, 272, rect.Width / 2, 272);
        }

        private void pnlLeftBadge_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var control = (Control)sender;
            int size = Math.Min(control.Width, control.Height);

            using var circlePath = new GraphicsPath();
            circlePath.AddEllipse(0, 0, size - 1, size - 1);

            using var bgBrush = new LinearGradientBrush(
                new Rectangle(0, 0, size, size),
                Color.FromArgb(37, 99, 235),
                Color.FromArgb(59, 130, 246),
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(bgBrush, circlePath);

            using var borderPen = new Pen(Color.FromArgb(40, 96, 165, 250), 2f);
            g.DrawPath(borderPen, circlePath);
        }

        private void pnlUserBadge_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var control = (Control)sender;
            int size = Math.Min(control.Width, control.Height);

            using var bgBrush = new LinearGradientBrush(
                new Rectangle(0, 0, size, size),
                Color.FromArgb(239, 246, 255),
                Color.FromArgb(219, 234, 254),
                LinearGradientMode.ForwardDiagonal);

            using var ellipsePath = new GraphicsPath();
            ellipsePath.AddEllipse(1, 1, size - 2, size - 2);
            g.FillPath(bgBrush, ellipsePath);

            using var borderPen = new Pen(Color.FromArgb(191, 219, 254), 2f);
            g.DrawPath(borderPen, ellipsePath);
        }
    }
}