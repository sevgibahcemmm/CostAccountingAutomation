using System.Drawing;
using System.Text.RegularExpressions;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.UserForms
{
    /// <summary>
    /// Kullanıcının kendi (mevcut) şifresini vererek şifresini değiştirdiği
    /// diyalog. Sıfırlama koduna gerek yoktur; şifre iki kez girilir ve mevcut
    /// şifre doğrulanmadan kayıt yapılmaz.
    /// </summary>
    /// <remarks>
    /// Sunucu tarafında
    /// <see cref="Cost.Accounting.Automation.Application.Auth.ChangeMyPasswordCommand"/>
    /// kimliği oturum bilgisinden okur; bu yüzden başka birinin hesabı için bu
    /// diyalogla değişiklik yapılamaz.
    /// </remarks>
    public sealed class ChangeMyPasswordForm : XtraForm
    {
        private readonly TextEdit _txtOldPassword;
        private readonly TextEdit _txtNewPassword;
        private readonly TextEdit _txtNewPasswordConfirm;

        /// <summary>Kullanıcının girdiği mevcut şifre.</summary>
        public string OldPassword => _txtOldPassword.Text.Trim();

        /// <summary>Kullanıcının onaylanmış yeni şifresi.</summary>
        public string NewPassword => _txtNewPassword.Text.Trim();

        public ChangeMyPasswordForm()
        {
            Text = "Şifremi Değiştir";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(500, 336);
            SuspendLayout();

            Color surface = SkinTheme.HighContrastSurface;
            BackColor = surface;

            var titleLabel = new Label
            {
                AutoSize = false,
                Location = new Point(24, 20),
                Size = new Size(452, 26),
                BackColor = surface,
                ForeColor = SkinTheme.HighContrastText,
                Text = "Kendi şifrenizi değiştirin",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            var instructionLabel = new Label
            {
                AutoSize = false,
                Location = new Point(24, 52),
                Size = new Size(452, 40),
                BackColor = surface,
                ForeColor = SkinTheme.MutedText(surface),
                Text =
                    "Mevcut şifrenizi girin, ardından yeni şifrenizi belirleyin.\n"
                    + "Yeni şifre en az 8 karakter olmalı, bir harf ve bir rakam içermelidir.",
                Font = new Font("Segoe UI", 9F)
            };

            _txtOldPassword = CreatePasswordField("Mevcut şifre", new Point(24, 104));
            _txtNewPassword = CreatePasswordField("Yeni şifre", new Point(24, 172));
            _txtNewPasswordConfirm = CreatePasswordField("Yeni şifre (tekrar)", new Point(24, 240));

            _txtOldPassword.Enter += AuthFormStyles.Field_Enter;
            _txtOldPassword.Leave += AuthFormStyles.Field_Leave;
            _txtNewPassword.Enter += AuthFormStyles.Field_Enter;
            _txtNewPassword.Leave += AuthFormStyles.Field_Leave;
            _txtNewPasswordConfirm.Enter += AuthFormStyles.Field_Enter;
            _txtNewPasswordConfirm.Leave += AuthFormStyles.Field_Leave;

            var saveButton = new SimpleButton
            {
                Text = "Şifreyi Güncelle",
                Size = new Size(150, 32),
                Location = new Point(ClientSize.Width - 174, ClientSize.Height - 48),
                Cursor = Cursors.Hand
            };
            AuthFormStyles.ApplyButtonAppearance(saveButton);
            saveButton.Click += BtnSave_Click;

            var cancelButton = new SimpleButton
            {
                Text = "Vazgeç",
                Size = new Size(110, 32),
                Location = new Point(24, ClientSize.Height - 48),
                Cursor = Cursors.Hand
            };
            cancelButton.Click += (_, _) => Close();

            Controls.Add(titleLabel);
            Controls.Add(instructionLabel);
            Controls.Add(_txtOldPassword);
            Controls.Add(_txtNewPassword);
            Controls.Add(_txtNewPasswordConfirm);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;

            ResumeLayout(true);
        }

        private TextEdit CreatePasswordField(string caption, Point location)
        {
            var label = new Label
            {
                AutoSize = false,
                Location = new Point(location.X, location.Y - 22),
                Size = new Size(452, 18),
                BackColor = SkinTheme.HighContrastSurface,
                ForeColor = SkinTheme.MutedText(SkinTheme.HighContrastSurface),
                Text = caption,
                Font = new Font("Segoe UI", 9F)
            };

            var edit = new TextEdit
            {
                Location = location,
                Size = new Size(452, 30)
            };
            edit.Properties.PasswordChar = '\u25CF';
            edit.Properties.NullText = "Şifre girin...";

            Controls.Add(label);

            return edit;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string oldPassword = OldPassword;
            string password = NewPassword;
            string confirm = _txtNewPasswordConfirm.Text.Trim();

            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                ToastHelper.Show("Mevcut şifrenizi girin.", ToastType.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ToastHelper.Show("Yeni şifrenizi girin.", ToastType.Error);
                return;
            }

            if (password.Length < 8)
            {
                ToastHelper.Show("Yeni şifre en az 8 karakter olmalıdır.", ToastType.Error);
                return;
            }

            if (Regex.IsMatch(password, "[A-Za-zÇĞİÖŞÜçğıöşü]") == false)
            {
                ToastHelper.Show("Yeni şifre en az bir harf içermelidir.", ToastType.Error);
                return;
            }

            if (Regex.IsMatch(password, "[0-9]") == false)
            {
                ToastHelper.Show("Yeni şifre en az bir rakam içermelidir.", ToastType.Error);
                return;
            }

            if (password != confirm)
            {
                ToastHelper.Show("Şifreler birbiriyle uyuşmuyor.", ToastType.Error);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}