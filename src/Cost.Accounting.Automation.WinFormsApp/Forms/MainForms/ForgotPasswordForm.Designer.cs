using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
partial class ForgotPasswordForm
{
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForgotPasswordForm));
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            pnlEmailBox = new DevExpress.XtraEditors.PanelControl();
            txtEmail = new DevExpress.XtraEditors.TextEdit();
            btnGenerate = new Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm.GradientButton();
            lblOr = new DevExpress.XtraEditors.LabelControl();
            btnHaveCode = new DevExpress.XtraEditors.SimpleButton();
            lnkBack = new DevExpress.XtraEditors.HyperlinkLabelControl();
            ((System.ComponentModel.ISupportInitialize)pnlEmailBox).BeginInit();
            pnlEmailBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Appearance.Options.UseTextOptions = true;
            lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblTitle.Location = new Point(34, 26);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(343, 29);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Şifremi Unuttum";
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 10F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Appearance.Options.UseTextOptions = true;
            lblSubtitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblSubtitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            lblSubtitle.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblSubtitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSubtitle.Location = new Point(34, 54);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(343, 40);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Yöneticiden aldığınız kodla devam edin ya da e-posta ile yeni kod isteyin";
            // 
            // pnlEmailBox
            // 
            pnlEmailBox.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlEmailBox.Appearance.Options.UseBackColor = true;
            pnlEmailBox.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlEmailBox.Controls.Add(txtEmail);
            pnlEmailBox.Location = new Point(34, 102);
            pnlEmailBox.Name = "pnlEmailBox";
            pnlEmailBox.Size = new Size(343, 45);
            pnlEmailBox.TabIndex = 2;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(13, 9);
            txtEmail.Name = "txtEmail";
            txtEmail.Properties.Appearance.BackColor = Color.Transparent;
            txtEmail.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtEmail.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            txtEmail.Properties.Appearance.Options.UseBackColor = true;
            txtEmail.Properties.Appearance.Options.UseFont = true;
            txtEmail.Properties.Appearance.Options.UseForeColor = true;
            txtEmail.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtEmail.Properties.NullText = "E-posta adresiniz";
            txtEmail.Size = new Size(317, 24);
            txtEmail.TabIndex = 0;
            // 
            // btnGenerate
            // 
            btnGenerate.Appearance.BackColor = Color.FromArgb(37, 99, 235);
            btnGenerate.Appearance.BorderColor = Color.FromArgb(37, 99, 235);
            btnGenerate.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGenerate.Appearance.ForeColor = Color.White;
            btnGenerate.Appearance.Options.UseBackColor = true;
            btnGenerate.Appearance.Options.UseBorderColor = true;
            btnGenerate.Appearance.Options.UseFont = true;
            btnGenerate.Appearance.Options.UseForeColor = true;
            btnGenerate.AppearanceHovered.BackColor = Color.FromArgb(29, 78, 216);
            btnGenerate.AppearanceHovered.Options.UseBackColor = true;
            btnGenerate.AppearancePressed.BackColor = Color.FromArgb(30, 64, 175);
            btnGenerate.AppearancePressed.Options.UseBackColor = true;
            btnGenerate.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnGenerate.Cursor = Cursors.Hand;
            btnGenerate.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnGenerate.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnGenerate.ImageOptions.SvgImage");
            btnGenerate.ImageOptions.SvgImageSize = new Size(18, 18);
            btnGenerate.Location = new Point(34, 161);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(343, 42);
            btnGenerate.TabIndex = 3;
            btnGenerate.Text = "Sıfırlama Kodumu E-postayla Al";
            btnGenerate.Click += BtnGenerate_Click;
            // 
            // lblOr
            // 
            lblOr.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblOr.Appearance.ForeColor = Color.FromArgb(148, 163, 184);
            lblOr.Appearance.Options.UseFont = true;
            lblOr.Appearance.Options.UseForeColor = true;
            lblOr.Appearance.Options.UseTextOptions = true;
            lblOr.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblOr.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblOr.Location = new Point(34, 213);
            lblOr.Name = "lblOr";
            lblOr.Size = new Size(343, 14);
            lblOr.TabIndex = 10;
            lblOr.Text = "VEYA";
            // 
            // btnHaveCode
            // 
            btnHaveCode.Appearance.BackColor = Color.White;
            btnHaveCode.Appearance.BorderColor = Color.FromArgb(37, 99, 235);
            btnHaveCode.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnHaveCode.Appearance.ForeColor = Color.FromArgb(37, 99, 235);
            btnHaveCode.Appearance.Options.UseBackColor = true;
            btnHaveCode.Appearance.Options.UseBorderColor = true;
            btnHaveCode.Appearance.Options.UseFont = true;
            btnHaveCode.Appearance.Options.UseForeColor = true;
            btnHaveCode.AppearanceHovered.BackColor = Color.FromArgb(239, 246, 255);
            btnHaveCode.AppearanceHovered.Options.UseBackColor = true;
            btnHaveCode.AppearancePressed.BackColor = Color.FromArgb(219, 234, 254);
            btnHaveCode.AppearancePressed.Options.UseBackColor = true;
            btnHaveCode.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            btnHaveCode.Cursor = Cursors.Hand;
            btnHaveCode.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnHaveCode.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnHaveCode.ImageOptions.SvgImage");
            btnHaveCode.ImageOptions.SvgImageSize = new Size(18, 18);
            btnHaveCode.Location = new Point(34, 236);
            btnHaveCode.Name = "btnHaveCode";
            btnHaveCode.Size = new Size(343, 40);
            btnHaveCode.TabIndex = 9;
            btnHaveCode.Text = "Yöneticiden Kodum Var →";
            btnHaveCode.Click += BtnHaveCode_Click;
            // 
            // lnkBack
            // 
            lnkBack.Appearance.Font = new Font("Segoe UI", 9F);
            lnkBack.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            lnkBack.Appearance.Options.UseFont = true;
            lnkBack.Appearance.Options.UseForeColor = true;
            lnkBack.Appearance.Options.UseTextOptions = true;
            lnkBack.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lnkBack.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lnkBack.AppearanceHovered.ForeColor = Color.FromArgb(37, 99, 235);
            lnkBack.AppearanceHovered.Options.UseForeColor = true;
            lnkBack.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lnkBack.Cursor = Cursors.Hand;
            lnkBack.Location = new Point(34, 301);
            lnkBack.Name = "lnkBack";
            lnkBack.Size = new Size(343, 19);
            lnkBack.TabIndex = 8;
            lnkBack.Text = "←  Giriş ekranına dön";
            lnkBack.Click += LnkBack_Click;
            // 
            // ForgotPasswordForm
            // 
            Appearance.BackColor = Color.White;
            Appearance.Options.UseBackColor = true;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(411, 338);
            Controls.Add(btnHaveCode);
            Controls.Add(lblOr);
            Controls.Add(lnkBack);
            Controls.Add(btnGenerate);
            Controls.Add(pnlEmailBox);
            Controls.Add(lblSubtitle);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ForgotPasswordForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Maliyet Muhasebesi Otomasyonu - Şifremi Unuttum";
            ((System.ComponentModel.ISupportInitialize)pnlEmailBox).EndInit();
            pnlEmailBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlEmailBox;
        private DevExpress.XtraEditors.TextEdit txtEmail;
        private Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm.GradientButton btnGenerate;
        private DevExpress.XtraEditors.LabelControl lblOr;
        private DevExpress.XtraEditors.SimpleButton btnHaveCode;
        private DevExpress.XtraEditors.HyperlinkLabelControl lnkBack;
    }
}
