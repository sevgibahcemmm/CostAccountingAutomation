using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class DatabaseSetupForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlSurface;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlHeaderAccent;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Label lblHeaderSubtitle;
        private System.Windows.Forms.Panel pnlBanner;
        private System.Windows.Forms.Panel pnlBannerAccent;
        private System.Windows.Forms.Label lblBannerTitle;
        private System.Windows.Forms.Label lblBannerText;
        private System.Windows.Forms.Panel pnlSteps;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Panel pnlProgressTrack;
        private System.Windows.Forms.Panel pnlProgressFill;
        private System.Windows.Forms.Label lblProgress;
        private DevExpress.XtraEditors.SimpleButton btnOk;
        private DevExpress.XtraEditors.SimpleButton btnRetry;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseSetupForm));
            pnlSurface = new Panel();
            pnlSteps = new Panel();
            pnlFooter = new Panel();
            btnRetry = new DevExpress.XtraEditors.SimpleButton();
            btnOk = new DevExpress.XtraEditors.SimpleButton();
            pnlProgressTrack = new Panel();
            pnlProgressFill = new Panel();
            lblProgress = new Label();
            pnlBanner = new Panel();
            lblBannerText = new Label();
            lblBannerTitle = new Label();
            pnlBannerAccent = new Panel();
            pnlHeader = new Panel();
            lblHeaderSubtitle = new Label();
            lblHeaderTitle = new Label();
            pnlHeaderAccent = new Panel();
            pnlSurface.SuspendLayout();
            pnlFooter.SuspendLayout();
            pnlProgressTrack.SuspendLayout();
            pnlBanner.SuspendLayout();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSurface
            // 
            pnlSurface.BackColor = Color.White;
            pnlSurface.Controls.Add(pnlSteps);
            pnlSurface.Controls.Add(pnlFooter);
            pnlSurface.Controls.Add(pnlBanner);
            pnlSurface.Controls.Add(pnlHeader);
            pnlSurface.Dock = DockStyle.Fill;
            pnlSurface.Location = new Point(0, 0);
            pnlSurface.Margin = new Padding(3, 2, 3, 2);
            pnlSurface.Name = "pnlSurface";
            pnlSurface.Padding = new Padding(1);
            pnlSurface.Size = new Size(644, 460);
            pnlSurface.TabIndex = 0;
            // 
            // pnlSteps
            // 
            pnlSteps.AutoScroll = true;
            pnlSteps.Dock = DockStyle.Fill;
            pnlSteps.Location = new Point(1, 117);
            pnlSteps.Margin = new Padding(3, 2, 3, 2);
            pnlSteps.Name = "pnlSteps";
            pnlSteps.Padding = new Padding(17, 10, 17, 10);
            pnlSteps.Size = new Size(642, 292);
            pnlSteps.TabIndex = 2;
            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(btnRetry);
            pnlFooter.Controls.Add(btnOk);
            pnlFooter.Controls.Add(pnlProgressTrack);
            pnlFooter.Controls.Add(lblProgress);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(1, 409);
            pnlFooter.Margin = new Padding(3, 2, 3, 2);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(642, 50);
            pnlFooter.TabIndex = 3;
            // 
            // btnRetry
            // 
            btnRetry.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRetry.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            btnRetry.Appearance.Options.UseFont = true;
            btnRetry.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnRetry.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnRetry.ImageOptions.SvgImage");
            btnRetry.ImageOptions.SvgImageSize = new Size(16, 16);
            btnRetry.Location = new Point(541, 12);
            btnRetry.Margin = new Padding(3, 2, 3, 2);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new Size(98, 28);
            btnRetry.TabIndex = 3;
            btnRetry.Text = "Tekrar Dene";
            btnRetry.Visible = false;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOk.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            btnOk.Appearance.Options.UseFont = true;
            btnOk.Enabled = false;
            btnOk.Location = new Point(562, 12);
            btnOk.Margin = new Padding(3, 2, 3, 2);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(77, 28);
            btnOk.TabIndex = 2;
            btnOk.Text = "Tamam";
            // 
            // pnlProgressTrack
            // 
            pnlProgressTrack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlProgressTrack.Controls.Add(pnlProgressFill);
            pnlProgressTrack.Location = new Point(11, 23);
            pnlProgressTrack.Margin = new Padding(3, 2, 3, 2);
            pnlProgressTrack.Name = "pnlProgressTrack";
            pnlProgressTrack.Size = new Size(524, 17);
            pnlProgressTrack.TabIndex = 0;
            // 
            // pnlProgressFill
            // 
            pnlProgressFill.Dock = DockStyle.Left;
            pnlProgressFill.Location = new Point(0, 0);
            pnlProgressFill.Margin = new Padding(3, 2, 3, 2);
            pnlProgressFill.Name = "pnlProgressFill";
            pnlProgressFill.Size = new Size(0, 17);
            pnlProgressFill.TabIndex = 0;
            // 
            // lblProgress
            // 
            lblProgress.Font = new Font("Segoe UI", 8.5F);
            lblProgress.Location = new Point(11, 6);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(413, 15);
            lblProgress.TabIndex = 1;
            lblProgress.Text = "Hazırlanıyor...";
            lblProgress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlBanner
            // 
            pnlBanner.Controls.Add(lblBannerText);
            pnlBanner.Controls.Add(lblBannerTitle);
            pnlBanner.Controls.Add(pnlBannerAccent);
            pnlBanner.Dock = DockStyle.Top;
            pnlBanner.Location = new Point(1, 63);
            pnlBanner.Margin = new Padding(3, 2, 3, 2);
            pnlBanner.Name = "pnlBanner";
            pnlBanner.Size = new Size(642, 54);
            pnlBanner.TabIndex = 1;
            // 
            // lblBannerText
            // 
            lblBannerText.Font = new Font("Segoe UI", 9F);
            lblBannerText.Location = new Point(22, 28);
            lblBannerText.Name = "lblBannerText";
            lblBannerText.Size = new Size(480, 16);
            lblBannerText.TabIndex = 1;
            lblBannerText.Text = "Lütfen bekleyin, oluşturuluyor...";
            lblBannerText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBannerTitle
            // 
            lblBannerTitle.Font = new Font("Segoe UI Semibold", 10.5F);
            lblBannerTitle.Location = new Point(22, 10);
            lblBannerTitle.Name = "lblBannerTitle";
            lblBannerTitle.Size = new Size(480, 18);
            lblBannerTitle.TabIndex = 0;
            lblBannerTitle.Text = "Veritabanı bulunamadı";
            lblBannerTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlBannerAccent
            // 
            pnlBannerAccent.Dock = DockStyle.Left;
            pnlBannerAccent.Location = new Point(0, 0);
            pnlBannerAccent.Margin = new Padding(3, 2, 3, 2);
            pnlBannerAccent.Name = "pnlBannerAccent";
            pnlBannerAccent.Size = new Size(3, 54);
            pnlBannerAccent.TabIndex = 0;
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblHeaderSubtitle);
            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(pnlHeaderAccent);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(1, 1);
            pnlHeader.Margin = new Padding(3, 2, 3, 2);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(642, 62);
            pnlHeader.TabIndex = 0;
            // 
            // lblHeaderSubtitle
            // 
            lblHeaderSubtitle.Font = new Font("Segoe UI", 9F);
            lblHeaderSubtitle.Location = new Point(22, 36);
            lblHeaderSubtitle.Name = "lblHeaderSubtitle";
            lblHeaderSubtitle.Size = new Size(480, 15);
            lblHeaderSubtitle.TabIndex = 1;
            lblHeaderSubtitle.Text = "İlk kurulum";
            lblHeaderSubtitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.Font = new Font("Segoe UI Semibold", 12.5F);
            lblHeaderTitle.Location = new Point(22, 15);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(480, 20);
            lblHeaderTitle.TabIndex = 0;
            lblHeaderTitle.Text = "Maliyet Muhasebesi Otomasyonu";
            lblHeaderTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlHeaderAccent
            // 
            pnlHeaderAccent.Dock = DockStyle.Left;
            pnlHeaderAccent.Location = new Point(0, 0);
            pnlHeaderAccent.Margin = new Padding(3, 2, 3, 2);
            pnlHeaderAccent.Name = "pnlHeaderAccent";
            pnlHeaderAccent.Size = new Size(3, 62);
            pnlHeaderAccent.TabIndex = 0;
            // 
            // DatabaseSetupForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(644, 460);
            Controls.Add(pnlSurface);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DatabaseSetupForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "İlk Kurulum";
            pnlSurface.ResumeLayout(false);
            pnlFooter.ResumeLayout(false);
            pnlProgressTrack.ResumeLayout(false);
            pnlBanner.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
