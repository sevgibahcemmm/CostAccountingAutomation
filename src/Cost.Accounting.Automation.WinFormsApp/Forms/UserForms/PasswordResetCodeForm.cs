using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.UserForms
{
    /// <summary>
    /// Üretilen şifre sıfırlama kodunu <b>yalnızca yöneticiye</b> gösterir.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bu kodun kullanıcıya ulaşma yolu <b>yoktur</b>. Bilinçli olarak e-posta,
    /// SMS ya da otomatik bir kanal yoktur; yönetici kodu kullanıcıya telefonla
    /// ya da yüz yüze iletir.
    /// </para>
    /// <para>
    /// Böylece iki ayrı risk ortadan kalkar:
    /// </para>
    /// <list type="bullet">
    /// <item>Kod, sıfırlama talebinde bulunan kişinin ekranına düşmez.</item>
    /// <item>
    /// Yönetici parolayı öğrenmez; iletilen şey kısa ömürlü ve tek kullanımlık
    /// bir bilgidir.
    /// </item>
    /// </list>
    /// <para>
    /// Pencere kodu kopyalanabilir yapar çünkü yönetici bunu okuyup yazmak
    /// zorunda kalmamalıdır; ancak pencere kapatıldığında kod ekranda kalmaz.
    /// </para>
    /// </remarks>
    public sealed class PasswordResetCodeForm : XtraForm
    {
        private readonly string _code;

        public PasswordResetCodeForm(
            string userFullName,
            string code,
            DateTimeOffset expiresAt)
        {
            _code = code;

            Text = "Şifre Sıfırlama Kodu";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(560, 340);
            SuspendLayout();

            var surface = SkinTheme.HighContrastSurface;
            BackColor = surface;

            var titleLabel = new Label
            {
                AutoSize = false,
                Location = new Point(22, 20),
                Size = new Size(516, 26),
                BackColor = surface,
                ForeColor = SkinTheme.HighContrastText,
                Text = $"{userFullName} için sıfırlama kodu",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            var instructionLabel = new Label
            {
                AutoSize = false,
                Location = new Point(22, 54),
                Size = new Size(516, 60),
                BackColor = surface,
                ForeColor = SkinTheme.HighContrastText,
                Text =
                    "Bu kodu kullanıcıya telefonla ya da yüz yüze iletin.\n"
                    + "Kodu yazılı olarak bırakmayın, ekran görüntüsü almayın ve "
                    + "kendi şifreniz gibi saklamayın.",
                Font = new Font("Segoe UI", 9.5F)
            };

            var codeBox = new TextBox
            {
                Location = new Point(22, 122),
                Size = new Size(516, 34),
                ReadOnly = true,
                Multiline = false,
                BackColor = SkinTheme.SurfaceReadOnly(surface),
                ForeColor = SkinTheme.EnsureReadable(SkinTheme.Primary, surface),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 14F, FontStyle.Bold),
                Text = code
            };

            var expiryLabel = new Label
            {
                AutoSize = false,
                Location = new Point(22, 168),
                Size = new Size(516, 22),
                BackColor = surface,
                ForeColor = SkinTheme.MutedText(surface),
                Text = $"Kod {expiresAt.ToLocalTime():HH:mm} saatine kadar geçerli. "
                       + "Kod yalnızca bir kez kullanılabilir.",
                Font = new Font("Segoe UI", 9F)
            };

            var auditLabel = new Label
            {
                AutoSize = false,
                Location = new Point(22, 198),
                Size = new Size(516, 40),
                BackColor = surface,
                ForeColor = SkinTheme.MutedText(surface),
                Text = "Kodu ürettiğiniz kayıtta kim tarafından üretildiği denetim "
                       + "kayıtlarına yazıldı.",
                Font = new Font("Segoe UI", 8.5F)
            };

            var copyButton = new SimpleButton
            {
                Text = "Kodu Kopyala",
                Size = new Size(140, 32),
                Location = new Point(22, ClientSize.Height - 48),
                Cursor = Cursors.Hand
            };
            copyButton.Click += BtnCopy_Click;

            var closeButton = new SimpleButton
            {
                Text = "Kapat",
                Size = new Size(110, 32),
                Location = new Point(ClientSize.Width - 132, ClientSize.Height - 48),
                Cursor = Cursors.Hand
            };
            closeButton.Click += (_, _) => Close();

            Controls.Add(titleLabel);
            Controls.Add(instructionLabel);
            Controls.Add(codeBox);
            Controls.Add(expiryLabel);
            Controls.Add(auditLabel);
            Controls.Add(copyButton);
            Controls.Add(closeButton);

            AcceptButton = closeButton;
            CancelButton = closeButton;

            ResumeLayout(true);
        }

        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(_code);
                ToastHelper.Show("Kod panoya kopyalandı.", ToastType.Success);
            }
            catch (Exception ex)
            {
                // Pano erişimi nadiren de olsa kilitli olabilir (başka bir uygulama
                // açtığında). Kullanıcı kodu ekrandan elle yazabilir.
                System.Diagnostics.Debug.WriteLine($"[PasswordResetCode] Pano hatası: {ex.Message}");
                ToastHelper.Show("Kod panoya kopyalanamadı; ekrandan elle yazabilirsiniz.", ToastType.Error);
            }
        }
    }
}
