using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class CreateUpdateForm
    {
        private void InitializeComponent()
        {
            _appName = new Label();
            _title = new Label();
            _hint = new Label();
            _repositoryCaption = new Label();
            _repositoryText = new TextBox();
            _browseButton = new Button();
            _versionLabel = new Label();
            _notesCaption = new Label();
            _notesText = new TextBox();
            _logCaption = new Label();
            _logText = new TextBox();
            _progressBar = new System.Windows.Forms.ProgressBar();
            _publishButton = new Button();
            _closeButton = new Button();
            _accentStrip = new Panel();
            SuspendLayout();
            // 
            // _appName
            // 
            _appName.AutoSize = true;
            _appName.Font = new Font(Font.FontFamily, 8.5f);
            _appName.Location = new Point(24, 18);
            _appName.Name = "_appName";
            _appName.Text = "Maliyet Muhasebesi Otomasyonu";
            // 
            // _title
            // 
            _title.AutoSize = true;
            _title.Font = new Font(Font.FontFamily, 13f, FontStyle.Bold);
            _title.Location = new Point(24, 36);
            _title.Name = "_title";
            _title.Text = "Güncelleme Oluştur";
            // 
            // _hint
            // 
            _hint.AutoSize = true;
            _hint.Font = new Font(Font.FontFamily, 8.5f);
            _hint.Location = new Point(24, 66);
            _hint.Name = "_hint";
            _hint.Text = "Kaynak kod klasörünü seçin. Kurulum paketi bu klasördeki dosyalardan üretilir.";
            // 
            // _repositoryCaption
            // 
            _repositoryCaption.AutoSize = true;
            _repositoryCaption.Location = new Point(24, 100);
            _repositoryCaption.Name = "_repositoryCaption";
            _repositoryCaption.Text = "Kaynak kod klasörü";
            // 
            // _repositoryText
            // 
            _repositoryText.Location = new Point(24, 120);
            _repositoryText.Name = "_repositoryText";
            _repositoryText.ReadOnly = true;
            _repositoryText.Size = new Size(560, 23);
            // 
            // _browseButton
            // 
            _browseButton.Location = new Point(592, 119);
            _browseButton.Name = "_browseButton";
            _browseButton.Size = new Size(96, 25);
            _browseButton.TabIndex = 0;
            _browseButton.Text = "Gözat...";
            _browseButton.Click += BrowseButton_Click;
            // 
            // _versionLabel
            // 
            _versionLabel.AutoSize = true;
            _versionLabel.Font = new Font(Font.FontFamily, 9.5f);
            _versionLabel.Location = new Point(24, 156);
            _versionLabel.Name = "_versionLabel";
            _versionLabel.Text = "Yayınlanacak sürüm: hesaplanıyor...";
            // 
            // _notesCaption
            // 
            _notesCaption.AutoSize = true;
            _notesCaption.Location = new Point(24, 186);
            _notesCaption.Name = "_notesCaption";
            _notesCaption.Text = "Sürüm notları (kullanıcılara gösterilir)";
            // 
            // _notesText
            // 
            _notesText.Location = new Point(24, 206);
            _notesText.Multiline = true;
            _notesText.Name = "_notesText";
            _notesText.ScrollBars = ScrollBars.Vertical;
            _notesText.Size = new Size(664, 56);
            _notesText.TabIndex = 1;
            // 
            // _logCaption
            // 
            _logCaption.AutoSize = true;
            _logCaption.Location = new Point(24, 274);
            _logCaption.Name = "_logCaption";
            _logCaption.Text = "Kurulum çıktısı";
            // 
            // _logText
            // 
            _logText.Location = new Point(24, 294);
            _logText.Multiline = true;
            _logText.Name = "_logText";
            _logText.ReadOnly = true;
            _logText.ScrollBars = ScrollBars.Both;
            _logText.Size = new Size(664, MinLogHeight);
            _logText.TabIndex = 2;
            _logText.WordWrap = false;
            // 
            // _progressBar
            // 
            _progressBar.Location = new Point(24, 476);
            _progressBar.MarqueeAnimationSpeed = 30;
            _progressBar.Name = "_progressBar";
            _progressBar.Size = new Size(664, 14);
            _progressBar.Style = ProgressBarStyle.Marquee;
            _progressBar.TabIndex = 3;
            _progressBar.Visible = false;
            // 
            // _publishButton
            // 
            _publishButton.Enabled = false;
            _publishButton.Location = new Point(480, 504);
            _publishButton.Name = "_publishButton";
            _publishButton.Size = new Size(100, 30);
            _publishButton.TabIndex = 4;
            _publishButton.Text = "Yayınla";
            _publishButton.Click += PublishButton_Click;
            // 
            // _closeButton
            // 
            _closeButton.Location = new Point(588, 504);
            _closeButton.Name = "_closeButton";
            _closeButton.Size = new Size(100, 30);
            _closeButton.TabIndex = 5;
            _closeButton.Text = "Kapat";
            _closeButton.Click += CloseButton_Click;
            // 
            // _accentStrip
            // 
            _accentStrip.Dock = DockStyle.Top;
            _accentStrip.Height = 4;
            _accentStrip.Name = "_accentStrip";
            _accentStrip.TabIndex = 6;
            // 
            // CreateUpdateForm
            // 
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(720, 560);
            Controls.AddRange(new Control[] { _appName, _title, _hint, _repositoryCaption, _repositoryText, _browseButton, _versionLabel, _notesCaption, _notesText, _logCaption, _logText, _progressBar, _publishButton, _closeButton, _accentStrip });
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CreateUpdateForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Güncelleme Oluştur";
            ResumeLayout(false);
        }

        private Label _appName = null!;
        private Label _title = null!;
        private Label _hint = null!;
        private Label _repositoryCaption = null!;
        private TextBox _repositoryText = null!;
        private Button _browseButton = null!;
        private Label _versionLabel = null!;
        private Label _notesCaption = null!;
        private TextBox _notesText = null!;
        private Label _logCaption = null!;
        private TextBox _logText = null!;
        private System.Windows.Forms.ProgressBar _progressBar = null!;
        private Button _publishButton = null!;
        private Button _closeButton = null!;
        private Panel _accentStrip = null!;
    }
}