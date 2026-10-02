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
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            pnlEmailBox = new DevExpress.XtraEditors.PanelControl();
            txtEmail = new DevExpress.XtraEditors.TextEdit();
            btnGenerate = new DevExpress.XtraEditors.SimpleButton();
            lblCodeCaption = new DevExpress.XtraEditors.LabelControl();
            pnlCodeBox = new DevExpress.XtraEditors.PanelControl();
            txtResetCode = new DevExpress.XtraEditors.TextEdit();
            lblMessage = new DevExpress.XtraEditors.LabelControl();
            btnContinue = new DevExpress.XtraEditors.SimpleButton();
            lnkBack = new DevExpress.XtraEditors.HyperlinkLabelControl();
            ((System.ComponentModel.ISupportInitialize)pnlEmailBox).BeginInit();
            pnlEmailBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlCodeBox).BeginInit();
            pnlCodeBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtResetCode.Properties).BeginInit();
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
            lblTitle.Location = new Point(40, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(400, 34);
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
            lblSubtitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSubtitle.Location = new Point(40, 72);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(400, 24);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "E-posta adresinizi girin, sıfırlama kodunuz oluşturulsun";
            //
            // pnlEmailBox
            //
            pnlEmailBox.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlEmailBox.Appearance.Options.UseBackColor = true;
            pnlEmailBox.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlEmailBox.Controls.Add(txtEmail);
            pnlEmailBox.Location = new Point(40, 110);
            pnlEmailBox.Name = "pnlEmailBox";
            pnlEmailBox.Size = new Size(400, 52);
            pnlEmailBox.TabIndex = 2;
            //
            // txtEmail
            //
            txtEmail.Location = new Point(15, 10);
            txtEmail.Name = "txtEmail";
            txtEmail.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            txtEmail.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtEmail.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            txtEmail.Properties.Appearance.Options.UseBackColor = true;
            txtEmail.Properties.Appearance.Options.UseFont = true;
            txtEmail.Properties.Appearance.Options.UseForeColor = true;
            txtEmail.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtEmail.Properties.NullText = "E-posta adresiniz";
            txtEmail.Size = new Size(370, 28);
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
            btnGenerate.Location = new Point(40, 178);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(400, 48);
            btnGenerate.TabIndex = 3;
            btnGenerate.Text = "Sıfırlama Kodu Oluştur";
            btnGenerate.ImageOptions.SvgImage = DxIcon.Key;
            btnGenerate.ImageOptions.SvgImageSize = new Size(18, 18);
            btnGenerate.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnGenerate.Click += BtnGenerate_Click;
            //
            // lblCodeCaption
            //
            lblCodeCaption.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCodeCaption.Appearance.ForeColor = Color.FromArgb(71, 85, 105);
            lblCodeCaption.Appearance.Options.UseFont = true;
            lblCodeCaption.Appearance.Options.UseForeColor = true;
            lblCodeCaption.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblCodeCaption.Location = new Point(42, 244);
            lblCodeCaption.Name = "lblCodeCaption";
            lblCodeCaption.Size = new Size(396, 18);
            lblCodeCaption.TabIndex = 4;
            lblCodeCaption.Text = "Oluşturulan Sıfırlama Kodu";
            //
            // pnlCodeBox
            //
            pnlCodeBox.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlCodeBox.Appearance.Options.UseBackColor = true;
            pnlCodeBox.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlCodeBox.Controls.Add(txtResetCode);
            pnlCodeBox.Location = new Point(40, 266);
            pnlCodeBox.Name = "pnlCodeBox";
            pnlCodeBox.Size = new Size(400, 52);
            pnlCodeBox.TabIndex = 5;
            //
            // txtResetCode
            //
            txtResetCode.Location = new Point(15, 10);
            txtResetCode.Name = "txtResetCode";
            txtResetCode.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            txtResetCode.Properties.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            txtResetCode.Properties.Appearance.ForeColor = Color.FromArgb(37, 99, 235);
            txtResetCode.Properties.Appearance.Options.UseBackColor = true;
            txtResetCode.Properties.Appearance.Options.UseFont = true;
            txtResetCode.Properties.Appearance.Options.UseForeColor = true;
            txtResetCode.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtResetCode.Properties.NullText = "Kod henüz oluşturulmadı";
            txtResetCode.Properties.ReadOnly = true;
            txtResetCode.Size = new Size(370, 28);
            txtResetCode.TabIndex = 0;
            txtResetCode.TabStop = false;
            //
            // lblMessage
            //
            lblMessage.Appearance.Font = new Font("Segoe UI", 9F);
            lblMessage.Appearance.ForeColor = Color.FromArgb(239, 68, 68);
            lblMessage.Appearance.Options.UseFont = true;
            lblMessage.Appearance.Options.UseForeColor = true;
            lblMessage.Appearance.Options.UseTextOptions = true;
            lblMessage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblMessage.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblMessage.Location = new Point(40, 326);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(400, 36);
            lblMessage.TabIndex = 6;
            lblMessage.Visible = false;
            //
            // btnContinue
            //
            btnContinue.Appearance.BackColor = Color.FromArgb(37, 99, 235);
            btnContinue.Appearance.BorderColor = Color.FromArgb(37, 99, 235);
            btnContinue.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnContinue.Appearance.ForeColor = Color.White;
            btnContinue.Appearance.Options.UseBackColor = true;
            btnContinue.Appearance.Options.UseBorderColor = true;
            btnContinue.Appearance.Options.UseFont = true;
            btnContinue.Appearance.Options.UseForeColor = true;
            btnContinue.AppearanceHovered.BackColor = Color.FromArgb(29, 78, 216);
            btnContinue.AppearanceHovered.Options.UseBackColor = true;
            btnContinue.AppearancePressed.BackColor = Color.FromArgb(30, 64, 175);
            btnContinue.AppearancePressed.Options.UseBackColor = true;
            btnContinue.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnContinue.Cursor = Cursors.Hand;
            btnContinue.Location = new Point(40, 372);
            btnContinue.Name = "btnContinue";
            btnContinue.Size = new Size(400, 48);
            btnContinue.TabIndex = 7;
            btnContinue.Text = "Şifreyi Sıfırlamaya Devam Et →";
            btnContinue.Visible = false;
            btnContinue.ImageOptions.SvgImage = DxIcon.Next;
            btnContinue.ImageOptions.SvgImageSize = new Size(18, 18);
            btnContinue.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnContinue.Click += BtnContinue_Click;
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
            lnkBack.Location = new Point(40, 432);
            lnkBack.Name = "lnkBack";
            lnkBack.Size = new Size(400, 22);
            lnkBack.TabIndex = 8;
            lnkBack.Text = "←  Giriş ekranına dön";
            lnkBack.Click += LnkBack_Click;
            //
            // ForgotPasswordForm
            //
            Appearance.BackColor = Color.White;
            Appearance.Options.UseBackColor = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 470);
            Controls.Add(lnkBack);
            Controls.Add(btnContinue);
            Controls.Add(lblMessage);
            Controls.Add(pnlCodeBox);
            Controls.Add(lblCodeCaption);
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
            pnlEmailBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlCodeBox).EndInit();
            pnlCodeBox.ResumeLayout(false);
            pnlCodeBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtResetCode.Properties).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlEmailBox;
        private DevExpress.XtraEditors.TextEdit txtEmail;
        private DevExpress.XtraEditors.SimpleButton btnGenerate;
        private DevExpress.XtraEditors.LabelControl lblCodeCaption;
        private DevExpress.XtraEditors.PanelControl pnlCodeBox;
        private DevExpress.XtraEditors.TextEdit txtResetCode;
        private DevExpress.XtraEditors.LabelControl lblMessage;
        private DevExpress.XtraEditors.SimpleButton btnContinue;
        private DevExpress.XtraEditors.HyperlinkLabelControl lnkBack;
    }
}
