using Cost.Accounting.Automation.Application.Auth;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.XtraEditors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
public partial class ResetPasswordForm : DevExpress.XtraEditors.XtraForm
    {
        private const string SuccessMessage = "Şifreniz başarıyla sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz";

        public bool ResetCompleted { get; private set; }

        /// <summary>
        /// "Giriş ekranına dön" bağlantısına basıldığında <see langword="true"/>
        /// olur. Açan form (şifremi unuttum) da kapanarak kullanıcıyı doğrudan
        /// giriş ekranına döndürür.
        /// </summary>
        public bool GoBackToLogin { get; private set; }

        public ResetPasswordForm()
        {
            InitializeComponent();
            pnlCodeBox.Paint += AuthFormStyles.RoundedField_Paint;
            txtResetCode.Enter += AuthFormStyles.Field_Enter;
            txtResetCode.Leave += AuthFormStyles.Field_Leave;
            pnlNewPasswordBox.Paint += AuthFormStyles.RoundedField_Paint;
            txtNewPassword.Enter += AuthFormStyles.Field_Enter;
            txtNewPassword.Leave += AuthFormStyles.Field_Leave;
pnlConfirmBox.Paint += AuthFormStyles.RoundedField_Paint;
            txtConfirmPassword.Enter += AuthFormStyles.Field_Enter;
            txtConfirmPassword.Leave += AuthFormStyles.Field_Leave;
            AuthFormStyles.ApplyButtonAppearance(btnReset);
        }

/// <summary>
        /// Formu doldurmaz; yalnızca odak başlangıcını ayarlar.
        /// </summary>
        /// <remarks>
        /// Kod <b>kullanıcı tarafından yazılır</b>. Önceden doldurulması, kodun
        /// talep eden kişinin kendi makinesinde otomatik bilinmesi anlamına
        /// gelirdi; o zaman sıfırlama güvenliği tümüyle ortadan kalkardı. Kullanıcı
        /// kodu sistem yöneticisinden alır ve buraya elle girer.
        /// </remarks>
        public void FocusResetCodeInput()
        {
            txtResetCode.Focus();
            txtResetCode.SelectAll();
        }

        private async void BtnReset_Click(object? sender, EventArgs e)
        {
            string resetCode = txtResetCode.Text.Trim();

            if (Guid.TryParse(resetCode, out _) == false)
            {
                ToastHelper.Show("Geçerli bir sıfırlama kodu girin.", ToastType.Error);
                return;
            }


            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ToastHelper.Show("Yeni şifrenizi girin.", ToastType.Error);
                return;
            }

            if (newPassword.Length < 8)
            {
                ToastHelper.Show("Şifreniz en az 8 karakter olmalıdır.", ToastType.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ToastHelper.Show("Şifreler birbiriyle uyuşmuyor.", ToastType.Error);
                return;
            }

            btnReset.Enabled = false;

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var codeCheck = await mediator.Send(
                    new CheckForgotPasswordCodeCommand(resetCode),
                    CancellationToken.None);

                if (!codeCheck.IsSuccessful)
                {
                    ToastHelper.Show(AuthFormStyles.GetErrorText(codeCheck.ErrorMessages), ToastType.Error);
                    return;
                }

                var result = await mediator.Send(
                    new ResetPasswordCommand(
                        resetCode,
                        newPassword,
                        chkLogoutAll.IsOn),
                    CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error);
                    return;
                }

                ResetCompleted = true;

                txtResetCode.Enabled = false;
                txtNewPassword.Enabled = false;
                txtConfirmPassword.Enabled = false;
                chkLogoutAll.Enabled = false;
                btnReset.Visible = false;
                lnkBack.Text = "✓  Giriş ekranına dön";

                ToastHelper.Show(SuccessMessage, ToastType.Success);
            }
            catch (ValidationException ex)
            {
                ToastHelper.Show(AuthFormStyles.GetValidationText(ex), ToastType.Error);
            }
            catch (Exception ex)
            {
                ToastHelper.Show("İşlem sırasında bir hata oluştu: " + ex.Message, ToastType.Error);
            }
            finally
            {
                btnReset.Enabled = true;
            }
        }

private void LnkBack_Click(object? sender, EventArgs e)
        {
            // Şifremi unuttum zincirinden geliyorsa bu formun üstündeki form da
            // kapanmalıdır; böylece kullanıcı doğrudan giriş ekranına döner.
            GoBackToLogin = true;
            Close();
        }
    }
}