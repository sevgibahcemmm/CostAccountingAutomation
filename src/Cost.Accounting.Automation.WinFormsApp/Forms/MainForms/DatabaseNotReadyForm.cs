using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    /// <summary>
    /// Veritabanı kullanıma hazır olmadığında giriş ekranı yerine gösterilen
    /// bilgilendirme penceresi.
    ///
    /// <para>
    /// Bu pencere girişe izin vermez ve kendisini kapatan tek bir düğme içerir
    /// ("Yeniden Dene" dışında). Kullanıcının yapabileceği doğru hamle bellidir:
    /// beklemek, yöneticiye başvurmak ya da bağlantıyı denetlemek. Bu yüzden
    /// ekranda kurulum sihirbazı gösterilmez — merkezi sunucuda şemayı değiştirmek
    /// kullanıcının yetkisindedir.
    /// </para>
    /// </summary>
    public sealed class DatabaseNotReadyForm : XtraForm
    {
        private readonly DatabaseSchemaCheckResult _result;
        private readonly Label _titleLabel;
        private readonly Label _bodyLabel;
        private readonly Label _detailLabel;
        private readonly TextBox _commandBox;
        private readonly Panel _listHost;
        private readonly SimpleButton _retryButton;

        /// <summary>
        /// <paramref name="result"/> durumuna göre pencereyi hazırlar.
        /// </summary>
        public DatabaseNotReadyForm(DatabaseSchemaCheckResult result)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));

            Text = "Veritabanı Hazır Değil";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(620, 470);
            SuspendLayout();

            var surface = SkinTheme.HighContrastSurface;
            BackColor = surface;

            _titleLabel = CreateLabel(surface, 22, style: FontStyle.Bold, size: 12f);
            _bodyLabel = CreateLabel(surface, 22, top: 74, size: 9.5f);
            _detailLabel = CreateLabel(surface, 22, top: 186, size: 9f);
            _detailLabel.ForeColor = SkinTheme.MutedText(surface);

            _commandBox = CreateCommandBox(surface);
            _listHost = new Panel { BackColor = surface, Location = new Point(22, 300) };

            _retryButton = new SimpleButton
            {
                Text = "Yeniden Dene",
                Size = new Size(130, 32),
                Location = new Point(ClientSize.Width - 152, ClientSize.Height - 46),
                Cursor = Cursors.Hand
            };
            _retryButton.Click += BtnRetry_Click;

            var closeButton = new SimpleButton
            {
                Text = "Kapat",
                Size = new Size(110, 32),
                Location = new Point(ClientSize.Width - 292, ClientSize.Height - 46),
                Cursor = Cursors.Hand
            };
            closeButton.Click += (_, _) => Close();

            Controls.Add(_titleLabel);
            Controls.Add(_bodyLabel);
            Controls.Add(_detailLabel);
            Controls.Add(_commandBox);
            Controls.Add(_listHost);
            Controls.Add(_retryButton);
            Controls.Add(closeButton);

            AcceptButton = _retryButton;
            CancelButton = closeButton;

            ApplyState();
            ResumeLayout(true);
        }

        private void ApplyState()
        {
            switch (_result.State)
            {
                case DatabaseSchemaState.SchemaOutdated:
                    _titleLabel.ForeColor = SkinTheme.EnsureReadable(SkinTheme.Warning, BackColor);
                    _titleLabel.Text = "Veritabanı güncel değil";
                    _bodyLabel.Text =
                        "Sunucudaki veritabanı şeması, bu program sürümünün ihtiyaç duyduğundan eski. "
                        + "Güvenlik nedeniyle program kendi şemasını güncellemez.\n\n"
                        + "Programı kullanmaya başlamak için sistem yöneticiniz veritabanını güncellemelidir.";

                    ShowPendingMigrations();
                    _commandBox.Visible = true;
                    _commandBox.Text = ProvisioningCommand;

                    _retryButton.Text = "Yeniden Dene";
                    _retryButton.Left = ClientSize.Width - 152;

                    break;

                case DatabaseSchemaState.DatabaseMissing:
                    _titleLabel.ForeColor = SkinTheme.EnsureReadable(SkinTheme.Danger, BackColor);
                    _titleLabel.Text = "Veritabanı bulunamadı";
                    _bodyLabel.Text =
                        "Bağlantı dizesinde tanımlı veritabanı sunucuda mevcut değil. "
                        + "Program veritabanını kendisi oluşturmaz.\n\n"
                        + "Lütfen sistem yöneticinizle iletişime geçin.";
                    _detailLabel.Visible = false;
                    _listHost.Visible = false;
                    _commandBox.Visible = true;
                    _commandBox.Text = ProvisioningCommand;

                    _retryButton.Text = "Yeniden Dene";
                    _retryButton.Left = ClientSize.Width - 152;

                    break;

                default:
                    _titleLabel.ForeColor = SkinTheme.EnsureReadable(SkinTheme.Danger, BackColor);
                    _titleLabel.Text = "Sunucuya ulaşılamıyor";
                    _bodyLabel.Text =
                        "Veritabanı sunucusuyla bağlantı kurulamadı. Program veritabanını kontrol "
                        + "edemeden açılamaz.\n\n"
                        + "Ağ bağlantınızı ve sunucunun erişilebilirliğini denetleyin, "
                        + "ardından yeniden deneyin.";
                    _detailLabel.Visible = false;
                    _listHost.Visible = false;
                    _commandBox.Visible = false;

                    _retryButton.Text = "Yeniden Dene";
                    _retryButton.Left = ClientSize.Width - 152;

                    break;
            }

            if (!string.IsNullOrWhiteSpace(_result.Detail))
            {
                _detailLabel.Text = _result.Detail;
            }
        }

        /// <summary>Bekleyen migration listesini gösterir.</summary>
        /// <remarks>
        /// Liste kısaltılır: kullanıcı on beşinci bekleyen migration'ı okuyarak
        /// ne yapacağını anlamaz. Üçten fazlası varsa "... ve N adet daha" ile
        /// özetlenir.
        /// </remarks>
        private void ShowPendingMigrations()
        {
            const int MaxShown = 3;

            var lines = _result.PendingMigrations
                .Take(MaxShown)
                .Select(m => "- " + m)
                .ToList();

            int remaining = _result.PendingMigrationCount - lines.Count;

            if (remaining > 0)
            {
                lines.Add($"... ve {remaining} adet daha");
            }

            _listHost.Height = Math.Min(120, lines.Count * 18 + 8);

            var list = new ListBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = SkinTheme.SurfaceReadOnly(BackColor),
                ForeColor = SkinTheme.HighContrastText,
                Location = new Point(0, 0),
                Size = new Size(_listHost.Width, _listHost.Height),
                IntegralHeight = false,
                Font = new Font("Consolas", 8.5f)
            };

            list.Items.AddRange(lines.Cast<object>().ToArray());
            _listHost.Controls.Add(list);

            _detailLabel.Text = "Bekleyen değişiklikler:";
            _detailLabel.ForeColor = SkinTheme.HighContrastText;
        }

        private static string ProvisioningCommand => "caa-provision provision";

        private void BtnRetry_Click(object? sender, EventArgs e)
        {
            // Pencere kapanır; Program yeniden kontrol edip giriş ekranını açar.
            DialogResult = DialogResult.Retry;
            Close();
        }

        private static Label CreateLabel(Color surface, int left, int top = 30, float size = 10f, FontStyle style = FontStyle.Regular)
        {
            return new Label
            {
                AutoSize = false,
                Location = new Point(left, top),
                Size = new Size(576, 40),
                BackColor = surface,
                ForeColor = SkinTheme.HighContrastText,
                Font = new Font(SystemFonts.MessageBoxFont?.FontFamily ?? SystemFonts.DefaultFont.FontFamily, size, style),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static TextBox CreateCommandBox(Color surface)
        {
            return new TextBox
            {
                Location = new Point(22, 356),
                Size = new Size(576, 26),
                ReadOnly = true,
                Multiline = false,
                BackColor = SkinTheme.SurfaceReadOnly(surface),
                ForeColor = SkinTheme.HighContrastText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9f)
            };
        }
    }
}
