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
            AuthFormStyles.ApplyButtonAppearance(btnGenerate);
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

                // Talebin nereye iletileceği yanıt mesajıyla (toast) bildirilir;
                // e-posta kanalı açıksa kod adrese gönderilir, kapalıysa üretimi
                // yönetici yapar. Kod hiçbir zaman ekrana/yanıta yazdırılmaz; talep
                // eden, kodu kendisine iletildikten sonra "Yöneticiden Kodum Var →"
                // seçeneğiyle kod + yeni şifre ekranına geçer.
                lblTitle.Text = "Talebiniz alındı";
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

/// <summary>
        /// Kod + yeni şifre ekranını açar. Hem e-posta ile kod alan hem de
        /// yöneticiden kod almış kullanıcı "Yöneticiden Kodum Var →" seçeneğiyle
        /// buraya gelir; kod elle yazılır.
        /// </summary>
        private void OpenResetPasswordForm()
        {
            using var scope = Program.Services.CreateScope();
            var resetForm = scope.ServiceProvider.GetRequiredService<ResetPasswordForm>();
            resetForm.FocusResetCodeInput();

            resetForm.ShowDialog(this);

            // Sıfırlama tamamlandıysa ya da "Giriş ekranına dön" seçildiyse bu
            // form da kapanır; kullanıcı doğrudan giriş ekranına döner.
            if (resetForm.ResetCompleted || resetForm.GoBackToLogin)
            {
                Close();
            }
        }

        private void BtnHaveCode_Click(object? sender, EventArgs e)
        {
            OpenResetPasswordForm();
        }

        private void LnkBack_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}