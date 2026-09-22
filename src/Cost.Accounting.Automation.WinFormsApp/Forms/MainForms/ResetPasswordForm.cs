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
btnReset.Paint += AuthFormStyles.Button_Paint;
        }

        public void SetResetCode(Guid resetCode)
        {
            txtResetCode.Text = resetCode.ToString("D");
            txtNewPassword.Focus();
        }

        private async void BtnReset_Click(object? sender, EventArgs e)
        {
            if (!Guid.TryParse(txtResetCode.Text.Trim(), out Guid resetCode))
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
                        chkLogoutAll.Checked),
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
            Close();
        }
    }
}