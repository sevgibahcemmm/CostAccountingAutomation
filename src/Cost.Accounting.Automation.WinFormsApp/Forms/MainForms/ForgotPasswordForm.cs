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
    public partial class ForgotPasswordForm : DevExpress.XtraEditors.XtraForm
    {
        public ForgotPasswordForm()
        {
            InitializeComponent();
            pnlEmailBox.Paint += AuthFormStyles.RoundedField_Paint;
            txtEmail.Enter += AuthFormStyles.Field_Enter;
            txtEmail.Leave += AuthFormStyles.Field_Leave;
            pnlCodeBox.Paint += AuthFormStyles.RoundedField_Paint;
btnGenerate.Paint += AuthFormStyles.Button_Paint;
            btnContinue.Paint += AuthFormStyles.Button_Paint;
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                ToastHelper.Show("E-posta adresinizi girin.", ToastType.Error);
                return;
            }

            btnGenerate.Enabled = false;

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(
                    new ForgotPasswordCommand(email),
                    CancellationToken.None);

                if (!result.IsSuccessful || result.Data is null)
                {
                    ToastHelper.Show(AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error);
                    return;
                }

                txtResetCode.Text = result.Data.ResetCode.ToString("D");
                btnContinue.Visible = true;
                ToastHelper.Show(result.Data.Message, ToastType.Success);
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
                btnGenerate.Enabled = true;
            }
        }

        private void BtnContinue_Click(object? sender, EventArgs e)
        {
            if (!Guid.TryParse(txtResetCode.Text.Trim(), out Guid resetCode))
            {
                ToastHelper.Show("Geçerli bir sıfırlama kodu bulunamadı.", ToastType.Error);
                return;
            }

            using var scope = Program.Services.CreateScope();
            var resetForm = scope.ServiceProvider.GetRequiredService<ResetPasswordForm>();
            resetForm.SetResetCode(resetCode);

            resetForm.ShowDialog(this);

            if (resetForm.ResetCompleted)
            {
                Close();
            }
        }

        private void LnkBack_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}