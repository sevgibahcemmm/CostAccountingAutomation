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
            AuthFormStyles.ApplyButtonAppearance(btnGenerate);
            AuthFormStyles.ApplyButtonAppearance(btnContinue);
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

                // Sifirlama kodu burada uretilmez ve ekrana basilmaz. Kullanici
                // once talep eder, kodu sistem yoneticisinden alir, sonra "Yeni
                // Sifre Belirle" adimina gecer.
                //
                // Daha once komut kodu yanitta donduruyor ve bu forma otomatik
                // yaziliyordu; e-posta adresini bilen herkes hesabi ele gecebiliyordu.
                pnlCodeBox.Visible = false;
                btnContinue.Visible = true;
                btnContinue.Enabled = true;
                lblTitle.Text = "Talebiniz alındı";

                // Kullaniciya bundan sonra ne yapacagini soyle. E-posta gonderilmez;
                // kod sistem yoneticisi tarafindan uretilip sozluel olarak iletilir.
                ShowGuidance(
                    "Kod e-postanıza gönderilmez. Sistem yöneticiniz kodu üretip size "
                    + "telefonla ya da yüz yüze iletecek. Kodu aldıktan sonra aşağıdaki "
                    + "düğmeyle yeni şifrenizi belirleyebilirsiniz.");

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
        /// Kullanıcıya sürecin devamını anlatan yönlendirme metnini gösterir.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Etiket, eskiden kullanıcının kendi oluşturduğu kodu gösterdiği alana
        /// yerleştirilir; o alan artık kullanılmadığı için boştur ve düğmeyle
        /// çakışmaz. Yükseklik metin satırına göre verilir çünkü tasarımcıdaki
        /// 36 piksel iki satıra yeter ama bu metin üç satıra taşar.
        /// </para>
        /// <para>
        /// Metin bilinçli olarak olumlu bir kırmızı değil nötr bir renkte gösterilir:
        /// burada bir hata değil, yönlendirme anlatılır.
        /// </para>
        /// </remarks>
        private void ShowGuidance(string message)
        {
            lblMessage.Location = new Point(40, 240);
            lblMessage.Size = new Size(400, 60);
            lblMessage.Appearance.ForeColor = SkinTheme.EnsureReadable(
                Color.FromArgb(71, 85, 105),
                SkinTheme.SurfaceOf(this));
            lblMessage.Appearance.Options.UseForeColor = true;
            lblMessage.Text = message;
            lblMessage.Visible = true;
        }

        private void BtnContinue_Click(object? sender, EventArgs e)
        {
            // Form yalnizca "yeni sifre belirle" adimina gecis yapar; kodu
            // kullanici yoneticiden alip bu ekrana elle girer.
            using var scope = Program.Services.CreateScope();
            var resetForm = scope.ServiceProvider.GetRequiredService<ResetPasswordForm>();
            resetForm.FocusResetCodeInput();

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