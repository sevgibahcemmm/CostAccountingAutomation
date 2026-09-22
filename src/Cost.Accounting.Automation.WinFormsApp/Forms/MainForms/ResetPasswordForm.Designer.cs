using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
partial class ResetPasswordForm
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
            pnlCodeBox = new DevExpress.XtraEditors.PanelControl();
            txtResetCode = new DevExpress.XtraEditors.TextEdit();
            pnlNewPasswordBox = new DevExpress.XtraEditors.PanelControl();
            txtNewPassword = new DevExpress.XtraEditors.TextEdit();
            pnlConfirmBox = new DevExpress.XtraEditors.PanelControl();
            txtConfirmPassword = new DevExpress.XtraEditors.TextEdit();
            chkLogoutAll = new DevExpress.XtraEditors.CheckEdit();
            lblMessage = new DevExpress.XtraEditors.LabelControl();
            btnReset = new DevExpress.XtraEditors.SimpleButton();
            lnkBack = new DevExpress.XtraEditors.HyperlinkLabelControl();
            ((System.ComponentModel.ISupportInitialize)pnlCodeBox).BeginInit();
            pnlCodeBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtResetCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlNewPasswordBox).BeginInit();
            pnlNewPasswordBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtNewPassword.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlConfirmBox).BeginInit();
            pnlConfirmBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtConfirmPassword.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkLogoutAll.Properties).BeginInit();
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
            lblTitle.Text = "Yeni Şifre Belirleyin";
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
            lblSubtitle.Text = "Sıfırlama kodunuzu ve yeni şifrenizi girin";
            //
            // pnlCodeBox
            //
            pnlCodeBox.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlCodeBox.Appearance.Options.UseBackColor = true;
            pnlCodeBox.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlCodeBox.Controls.Add(txtResetCode);
            pnlCodeBox.Location = new Point(40, 110);
            pnlCodeBox.Name = "pnlCodeBox";
            pnlCodeBox.Size = new Size(400, 52);
            pnlCodeBox.TabIndex = 2;
            //
            // txtResetCode
            //
            txtResetCode.Location = new Point(15, 10);
            txtResetCode.Name = "txtResetCode";
            txtResetCode.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            txtResetCode.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtResetCode.Properties.Appearance.ForeColor = Color.FromArgb(37, 99, 235);
            txtResetCode.Properties.Appearance.Options.UseBackColor = true;
            txtResetCode.Properties.Appearance.Options.UseFont = true;
            txtResetCode.Properties.Appearance.Options.UseForeColor = true;
            txtResetCode.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtResetCode.Properties.NullText = "Sıfırlama kodu";
            txtResetCode.Size = new Size(370, 28);
            txtResetCode.TabIndex = 0;
            //
            // pnlNewPasswordBox
            //
            pnlNewPasswordBox.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlNewPasswordBox.Appearance.Options.UseBackColor = true;
            pnlNewPasswordBox.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlNewPasswordBox.Controls.Add(txtNewPassword);
            pnlNewPasswordBox.Location = new Point(40, 172);
            pnlNewPasswordBox.Name = "pnlNewPasswordBox";
            pnlNewPasswordBox.Size = new Size(400, 52);
            pnlNewPasswordBox.TabIndex = 3;
            //
            // txtNewPassword
            //
            txtNewPassword.Location = new Point(15, 10);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            txtNewPassword.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtNewPassword.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            txtNewPassword.Properties.Appearance.Options.UseBackColor = true;
            txtNewPassword.Properties.Appearance.Options.UseFont = true;
            txtNewPassword.Properties.Appearance.Options.UseForeColor = true;
            txtNewPassword.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtNewPassword.Properties.NullText = "Yeni şifre";
            txtNewPassword.Properties.PasswordChar = '\u2022';
            txtNewPassword.Size = new Size(370, 28);
            txtNewPassword.TabIndex = 0;
            //
            // pnlConfirmBox
            //
            pnlConfirmBox.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlConfirmBox.Appearance.Options.UseBackColor = true;
            pnlConfirmBox.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlConfirmBox.Controls.Add(txtConfirmPassword);
            pnlConfirmBox.Location = new Point(40, 234);
            pnlConfirmBox.Name = "pnlConfirmBox";
            pnlConfirmBox.Size = new Size(400, 52);
            pnlConfirmBox.TabIndex = 4;
            //
            // txtConfirmPassword
            //
            txtConfirmPassword.Location = new Point(15, 10);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            txtConfirmPassword.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtConfirmPassword.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            txtConfirmPassword.Properties.Appearance.Options.UseBackColor = true;
            txtConfirmPassword.Properties.Appearance.Options.UseFont = true;
            txtConfirmPassword.Properties.Appearance.Options.UseForeColor = true;
            txtConfirmPassword.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtConfirmPassword.Properties.NullText = "Yeni şifre (tekrar)";
            txtConfirmPassword.Properties.PasswordChar = '\u2022';
            txtConfirmPassword.Size = new Size(370, 28);
            txtConfirmPassword.TabIndex = 0;
            //
            // chkLogoutAll
            //
            chkLogoutAll.Location = new Point(42, 298);
            chkLogoutAll.Name = "chkLogoutAll";
            chkLogoutAll.Properties.Appearance.Font = new Font("Segoe UI", 9F);
            chkLogoutAll.Properties.Appearance.ForeColor = Color.FromArgb(71, 85, 105);
            chkLogoutAll.Properties.Appearance.Options.UseFont = true;
            chkLogoutAll.Properties.Appearance.Options.UseForeColor = true;
            chkLogoutAll.Properties.Caption = "Tüm cihazlarda oturumu kapat";
            chkLogoutAll.Properties.GlyphAlignment = DevExpress.Utils.HorzAlignment.Near;
            chkLogoutAll.Size = new Size(396, 26);
            chkLogoutAll.TabIndex = 5;
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
            lblMessage.Location = new Point(40, 332);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(400, 40);
            lblMessage.TabIndex = 6;
            lblMessage.Visible = false;
            //
            // btnReset
            //
            btnReset.Appearance.BackColor = Color.FromArgb(37, 99, 235);
            btnReset.Appearance.BorderColor = Color.FromArgb(37, 99, 235);
            btnReset.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnReset.Appearance.ForeColor = Color.White;
            btnReset.Appearance.Options.UseBackColor = true;
            btnReset.Appearance.Options.UseBorderColor = true;
            btnReset.Appearance.Options.UseFont = true;
            btnReset.Appearance.Options.UseForeColor = true;
            btnReset.AppearanceHovered.BackColor = Color.FromArgb(29, 78, 216);
            btnReset.AppearanceHovered.Options.UseBackColor = true;
            btnReset.AppearancePressed.BackColor = Color.FromArgb(30, 64, 175);
            btnReset.AppearancePressed.Options.UseBackColor = true;
            btnReset.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnReset.Cursor = Cursors.Hand;
            btnReset.Location = new Point(40, 382);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(400, 48);
            btnReset.TabIndex = 7;
            btnReset.Text = "Şifreyi Sıfırla";
            btnReset.ImageOptions.SvgImage = DxIcon.Refresh;
            btnReset.ImageOptions.SvgImageSize = new Size(18, 18);
            btnReset.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnReset.Click += BtnReset_Click;
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
            lnkBack.Location = new Point(40, 444);
            lnkBack.Name = "lnkBack";
            lnkBack.Size = new Size(400, 22);
            lnkBack.TabIndex = 8;
            lnkBack.Text = "←  Geri dön";
            lnkBack.Click += LnkBack_Click;
            //
            // ResetPasswordForm
            //
            AcceptButton = btnReset;
            Appearance.BackColor = Color.White;
            Appearance.Options.UseBackColor = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 482);
            Controls.Add(lnkBack);
            Controls.Add(btnReset);
            Controls.Add(lblMessage);
            Controls.Add(chkLogoutAll);
            Controls.Add(pnlConfirmBox);
            Controls.Add(pnlNewPasswordBox);
            Controls.Add(pnlCodeBox);
            Controls.Add(lblSubtitle);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ResetPasswordForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Maliyet Muhasebesi Otomasyonu - Şifre Sıfırlama";
            ((System.ComponentModel.ISupportInitialize)pnlCodeBox).EndInit();
            pnlCodeBox.ResumeLayout(false);
            pnlCodeBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtResetCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlNewPasswordBox).EndInit();
            pnlNewPasswordBox.ResumeLayout(false);
            pnlNewPasswordBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtNewPassword.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlConfirmBox).EndInit();
            pnlConfirmBox.ResumeLayout(false);
            pnlConfirmBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtConfirmPassword.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkLogoutAll.Properties).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlCodeBox;
        private DevExpress.XtraEditors.TextEdit txtResetCode;
        private DevExpress.XtraEditors.PanelControl pnlNewPasswordBox;
        private DevExpress.XtraEditors.TextEdit txtNewPassword;
        private DevExpress.XtraEditors.PanelControl pnlConfirmBox;
        private DevExpress.XtraEditors.TextEdit txtConfirmPassword;
        private DevExpress.XtraEditors.CheckEdit chkLogoutAll;
        private DevExpress.XtraEditors.LabelControl lblMessage;
        private DevExpress.XtraEditors.SimpleButton btnReset;
        private DevExpress.XtraEditors.HyperlinkLabelControl lnkBack;
    }
}