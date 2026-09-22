using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class XtraLoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XtraLoginForm));
            pnlLeft = new DevExpress.XtraEditors.PanelControl();
            pnlLeftBadge = new DevExpress.XtraEditors.PanelControl();
            pnlLeftLockGlyph = new DevExpress.XtraEditors.LabelControl();
            lblLeftTitle = new DevExpress.XtraEditors.LabelControl();
            lblLeftCompany = new DevExpress.XtraEditors.LabelControl();
            lblLeftFeatures = new DevExpress.XtraEditors.LabelControl();
            lblLeftFooter = new DevExpress.XtraEditors.LabelControl();
            pnlRight = new DevExpress.XtraEditors.PanelControl();
            lblMessage = new DevExpress.XtraEditors.LabelControl();
            lnkForgot = new DevExpress.XtraEditors.HyperlinkLabelControl();
            btnLogin = new GradientButton();
            pnlCaptchaResult = new DevExpress.XtraEditors.PanelControl();
            txtCaptchaResult = new DevExpress.XtraEditors.TextEdit();
            lblCaptchaQuestion = new DevExpress.XtraEditors.LabelControl();
            pnlPasswordBox = new DevExpress.XtraEditors.PanelControl();
            lblPassIcon = new DevExpress.XtraEditors.LabelControl();
            txtPassword = new DevExpress.XtraEditors.TextEdit();
            lblTogglePassword = new DevExpress.XtraEditors.LabelControl();
            pnlUserNameBox = new DevExpress.XtraEditors.PanelControl();
            lblUserIcon = new DevExpress.XtraEditors.LabelControl();
            txtUserName = new DevExpress.XtraEditors.TextEdit();
            lblFormSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblFormTitle = new DevExpress.XtraEditors.LabelControl();
            pnlUserBadge = new DevExpress.XtraEditors.PanelControl();
            _lblLogoIcon = new DevExpress.XtraEditors.LabelControl();
            lblFooter = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)pnlLeft).BeginInit();
            pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlLeftBadge).BeginInit();
            pnlLeftBadge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlRight).BeginInit();
            pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlCaptchaResult).BeginInit();
            pnlCaptchaResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtCaptchaResult.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlPasswordBox).BeginInit();
            pnlPasswordBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlUserNameBox).BeginInit();
            pnlUserNameBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtUserName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlUserBadge).BeginInit();
            pnlUserBadge.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLeft
            // 
            pnlLeft.Appearance.BackColor = Color.FromArgb(15, 23, 42);
            pnlLeft.Appearance.Options.UseBackColor = true;
            pnlLeft.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlLeft.Controls.Add(pnlLeftBadge);
            pnlLeft.Controls.Add(lblLeftTitle);
            pnlLeft.Controls.Add(lblLeftCompany);
            pnlLeft.Controls.Add(lblLeftFeatures);
            pnlLeft.Controls.Add(lblLeftFooter);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(343, 555);
            pnlLeft.TabIndex = 0;
            pnlLeft.Paint += pnlLeft_Paint;
            // 
            // pnlLeftBadge
            // 
            pnlLeftBadge.Appearance.BackColor = Color.FromArgb(15, 23, 42);
            pnlLeftBadge.Appearance.Options.UseBackColor = true;
            pnlLeftBadge.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlLeftBadge.Controls.Add(pnlLeftLockGlyph);
            pnlLeftBadge.Location = new Point(126, 50);
            pnlLeftBadge.Name = "pnlLeftBadge";
            pnlLeftBadge.Size = new Size(92, 92);
            pnlLeftBadge.TabIndex = 0;
            pnlLeftBadge.Paint += pnlLeftBadge_Paint;
            // 
            // pnlLeftLockGlyph
            // 
            pnlLeftLockGlyph.Appearance.BackColor = Color.Transparent;
            pnlLeftLockGlyph.Appearance.Font = new Font("Segoe UI Emoji", 22F);
            pnlLeftLockGlyph.Appearance.Options.UseBackColor = true;
            pnlLeftLockGlyph.Appearance.Options.UseFont = true;
            pnlLeftLockGlyph.Appearance.Options.UseTextOptions = true;
            pnlLeftLockGlyph.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            pnlLeftLockGlyph.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            pnlLeftLockGlyph.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            pnlLeftLockGlyph.Location = new Point(23, 23);
            pnlLeftLockGlyph.Name = "pnlLeftLockGlyph";
            pnlLeftLockGlyph.Size = new Size(46, 46);
            pnlLeftLockGlyph.TabIndex = 0;
            pnlLeftLockGlyph.Text = "";
            pnlLeftLockGlyph.ImageOptions.SvgImage = DxIcon.Lock;
            pnlLeftLockGlyph.ImageOptions.SvgImageSize = new Size(48, 48);
            // 
            // lblLeftTitle
            // 
            lblLeftTitle.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblLeftTitle.Appearance.ForeColor = Color.FromArgb(226, 232, 240);
            lblLeftTitle.Appearance.Options.UseFont = true;
            lblLeftTitle.Appearance.Options.UseForeColor = true;
            lblLeftTitle.Appearance.Options.UseTextOptions = true;
            lblLeftTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblLeftTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblLeftTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblLeftTitle.Location = new Point(26, 165);
            lblLeftTitle.Name = "lblLeftTitle";
            lblLeftTitle.Size = new Size(291, 38);
            lblLeftTitle.TabIndex = 1;
            lblLeftTitle.Text = "Maliyet Muhasebesi";
            // 
            // lblLeftCompany
            // 
            lblLeftCompany.Appearance.Font = new Font("Segoe UI Semibold", 10.5F);
            lblLeftCompany.Appearance.ForeColor = Color.FromArgb(129, 140, 248);
            lblLeftCompany.Appearance.Options.UseFont = true;
            lblLeftCompany.Appearance.Options.UseForeColor = true;
            lblLeftCompany.Appearance.Options.UseTextOptions = true;
            lblLeftCompany.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblLeftCompany.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblLeftCompany.Location = new Point(26, 207);
            lblLeftCompany.Name = "lblLeftCompany";
            lblLeftCompany.Size = new Size(291, 21);
            lblLeftCompany.TabIndex = 2;
            lblLeftCompany.Text = "Otomasyon Sistemi";
            // 
            // lblLeftFeatures
            // 
            lblLeftFeatures.Appearance.Font = new Font("Segoe UI", 10F);
            lblLeftFeatures.Appearance.ForeColor = Color.FromArgb(203, 213, 225);
            lblLeftFeatures.Appearance.Options.UseFont = true;
            lblLeftFeatures.Appearance.Options.UseForeColor = true;
            lblLeftFeatures.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblLeftFeatures.Location = new Point(57, 282);
            lblLeftFeatures.Name = "lblLeftFeatures";
            lblLeftFeatures.Size = new Size(242, 121);
            lblLeftFeatures.TabIndex = 3;
            lblLeftFeatures.Text = "✓  Gerçek zamanlı maliyet takibi\r\n\r\n✓  Kolay raporlama ve analiz\r\n\r\n✓  Güvenli kullanıcı yönetimi\r\n\r\n✓  Çoklu firma desteği";
            // 
            // lblLeftFooter
            // 
            lblLeftFooter.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblLeftFooter.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            lblLeftFooter.Appearance.Options.UseFont = true;
            lblLeftFooter.Appearance.Options.UseForeColor = true;
            lblLeftFooter.Appearance.Options.UseTextOptions = true;
            lblLeftFooter.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblLeftFooter.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblLeftFooter.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblLeftFooter.Location = new Point(26, 511);
            lblLeftFooter.Name = "lblLeftFooter";
            lblLeftFooter.Size = new Size(291, 26);
            lblLeftFooter.TabIndex = 4;
            lblLeftFooter.Text = "© 2026 Maliyet Muhasebesi Otomasyonu";
            // 
            // pnlRight
            // 
            pnlRight.Appearance.BackColor = Color.White;
            pnlRight.Appearance.Options.UseBackColor = true;
            pnlRight.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlRight.Controls.Add(lblMessage);
            pnlRight.Controls.Add(lnkForgot);
            pnlRight.Controls.Add(btnLogin);
            pnlRight.Controls.Add(pnlCaptchaResult);
            pnlRight.Controls.Add(lblCaptchaQuestion);
            pnlRight.Controls.Add(pnlPasswordBox);
            pnlRight.Controls.Add(pnlUserNameBox);
            pnlRight.Controls.Add(lblFormSubtitle);
            pnlRight.Controls.Add(lblFormTitle);
            pnlRight.Controls.Add(pnlUserBadge);
            pnlRight.Controls.Add(lblFooter);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Location = new Point(343, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(446, 555);
            pnlRight.TabIndex = 1;
            // 
            // lblMessage
            // 
            lblMessage.Appearance.Font = new Font("Segoe UI", 9F);
            lblMessage.Appearance.ForeColor = Color.FromArgb(239, 68, 68);
            lblMessage.Appearance.Options.UseFont = true;
            lblMessage.Appearance.Options.UseForeColor = true;
            lblMessage.Appearance.Options.UseTextOptions = true;
            lblMessage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblMessage.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblMessage.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblMessage.Location = new Point(51, 426);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(343, 19);
            lblMessage.TabIndex = 6;
            lblMessage.Visible = false;
            // 
            // lnkForgot
            // 
            lnkForgot.Appearance.Font = new Font("Segoe UI", 9F);
            lnkForgot.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            lnkForgot.Appearance.Options.UseFont = true;
            lnkForgot.Appearance.Options.UseForeColor = true;
            lnkForgot.Appearance.Options.UseTextOptions = true;
            lnkForgot.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lnkForgot.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lnkForgot.AppearanceHovered.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            lnkForgot.AppearanceHovered.ForeColor = Color.FromArgb(37, 99, 235);
            lnkForgot.AppearanceHovered.Options.UseFont = true;
            lnkForgot.AppearanceHovered.Options.UseForeColor = true;
            lnkForgot.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lnkForgot.Cursor = Cursors.Hand;
            lnkForgot.Location = new Point(51, 505);
            lnkForgot.Name = "lnkForgot";
            lnkForgot.Size = new Size(343, 21);
            lnkForgot.TabIndex = 8;
            lnkForgot.Text = "Şifremi unuttum";
            // 
            // btnLogin
            // 
            btnLogin.Appearance.BackColor = Color.FromArgb(99, 102, 241);
            btnLogin.Appearance.BorderColor = Color.FromArgb(99, 102, 241);
            btnLogin.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.Appearance.ForeColor = Color.White;
            btnLogin.Appearance.Options.UseBackColor = true;
            btnLogin.Appearance.Options.UseBorderColor = true;
            btnLogin.Appearance.Options.UseFont = true;
            btnLogin.Appearance.Options.UseForeColor = true;
            btnLogin.AppearanceHovered.BackColor = Color.FromArgb(79, 70, 229);
            btnLogin.AppearanceHovered.ForeColor = Color.White;
            btnLogin.AppearanceHovered.Options.UseBackColor = true;
            btnLogin.AppearanceHovered.Options.UseForeColor = true;
            btnLogin.AppearancePressed.BackColor = Color.FromArgb(67, 56, 202);
            btnLogin.AppearancePressed.ForeColor = Color.White;
            btnLogin.AppearancePressed.Options.UseBackColor = true;
            btnLogin.AppearancePressed.Options.UseForeColor = true;
            btnLogin.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnLogin.ImageOptions.SvgImage");
            btnLogin.Location = new Point(51, 450);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(343, 46);
            btnLogin.TabIndex = 7;
            btnLogin.Text = "Giriş Yap";
            btnLogin.ImageOptions.SvgImage = DxIcon.Next;
            btnLogin.ImageOptions.SvgImageSize = new Size(22, 22);
            btnLogin.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.RightCenter;
            btnLogin.Click += btnLogin_Click;
            // 
            // pnlCaptchaResult
            // 
            pnlCaptchaResult.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlCaptchaResult.Appearance.Options.UseBackColor = true;
            pnlCaptchaResult.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlCaptchaResult.Controls.Add(txtCaptchaResult);
            pnlCaptchaResult.Location = new Point(51, 372);
            pnlCaptchaResult.Name = "pnlCaptchaResult";
            pnlCaptchaResult.Size = new Size(343, 48);
            pnlCaptchaResult.TabIndex = 0;
            // 
            // txtCaptchaResult
            // 
            txtCaptchaResult.Location = new Point(12, 12);
            txtCaptchaResult.Name = "txtCaptchaResult";
            txtCaptchaResult.Properties.Appearance.BackColor = Color.Transparent;
            txtCaptchaResult.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtCaptchaResult.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            txtCaptchaResult.Properties.Appearance.Options.UseBackColor = true;
            txtCaptchaResult.Properties.Appearance.Options.UseFont = true;
            txtCaptchaResult.Properties.Appearance.Options.UseForeColor = true;
            txtCaptchaResult.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtCaptchaResult.Properties.NullText = "Sonuç ve kod (örn: 29 5424)";
            txtCaptchaResult.Size = new Size(319, 24);
            txtCaptchaResult.TabIndex = 0;
            // 
            // lblCaptchaQuestion
            // 
            lblCaptchaQuestion.Appearance.BackColor = Color.Transparent;
            lblCaptchaQuestion.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblCaptchaQuestion.Appearance.ForeColor = Color.FromArgb(51, 65, 85);
            lblCaptchaQuestion.Appearance.Options.UseBackColor = true;
            lblCaptchaQuestion.Appearance.Options.UseFont = true;
            lblCaptchaQuestion.Appearance.Options.UseForeColor = true;
            lblCaptchaQuestion.Appearance.Options.UseTextOptions = true;
            lblCaptchaQuestion.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblCaptchaQuestion.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblCaptchaQuestion.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblCaptchaQuestion.Cursor = Cursors.Hand;
            lblCaptchaQuestion.Location = new Point(51, 346);
            lblCaptchaQuestion.Name = "lblCaptchaQuestion";
            lblCaptchaQuestion.Size = new Size(343, 21);
            lblCaptchaQuestion.TabIndex = 0;
            lblCaptchaQuestion.Text = "12 + 30 = ?   •   Kod: 5424";
            lblCaptchaQuestion.Click += lblCaptchaQuestion_Click;
            // 
            // pnlPasswordBox
            // 
            pnlPasswordBox.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlPasswordBox.Appearance.Options.UseBackColor = true;
            pnlPasswordBox.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlPasswordBox.Controls.Add(lblPassIcon);
            pnlPasswordBox.Controls.Add(txtPassword);
            pnlPasswordBox.Controls.Add(lblTogglePassword);
            pnlPasswordBox.Location = new Point(51, 282);
            pnlPasswordBox.Name = "pnlPasswordBox";
            pnlPasswordBox.Size = new Size(343, 48);
            pnlPasswordBox.TabIndex = 4;
            // 
            // lblPassIcon
            // 
            lblPassIcon.Appearance.BackColor = Color.Transparent;
            lblPassIcon.Appearance.Font = new Font("Segoe UI Emoji", 12F);
            lblPassIcon.Appearance.Options.UseBackColor = true;
            lblPassIcon.Appearance.Options.UseFont = true;
            lblPassIcon.Appearance.Options.UseTextOptions = true;
            lblPassIcon.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblPassIcon.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblPassIcon.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblPassIcon.Location = new Point(0, 0);
            lblPassIcon.Name = "lblPassIcon";
            lblPassIcon.Size = new Size(42, 48);
            lblPassIcon.TabIndex = 0;
            lblPassIcon.Text = "";
            lblPassIcon.ImageOptions.SvgImage = DxIcon.Key;
            lblPassIcon.ImageOptions.SvgImageSize = new Size(22, 22);
            // 
            // txtPassword
            // 
            txtPassword.EditValue = "1";
            txtPassword.Location = new Point(48, 11);
            txtPassword.Name = "txtPassword";
            txtPassword.Properties.Appearance.BackColor = Color.Transparent;
            txtPassword.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtPassword.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            txtPassword.Properties.Appearance.Options.UseBackColor = true;
            txtPassword.Properties.Appearance.Options.UseFont = true;
            txtPassword.Properties.Appearance.Options.UseForeColor = true;
            txtPassword.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtPassword.Properties.NullText = "Şifre";
            txtPassword.Properties.PasswordChar = '•';
            txtPassword.Size = new Size(250, 24);
            txtPassword.TabIndex = 0;
            txtPassword.ToolTip = "Şifre <Giriniz>";
            // 
            // lblTogglePassword
            // 
            lblTogglePassword.Appearance.BackColor = Color.Transparent;
            lblTogglePassword.Appearance.Font = new Font("Segoe UI Emoji", 11F);
            lblTogglePassword.Appearance.Options.UseBackColor = true;
            lblTogglePassword.Appearance.Options.UseFont = true;
            lblTogglePassword.Appearance.Options.UseTextOptions = true;
            lblTogglePassword.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblTogglePassword.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblTogglePassword.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblTogglePassword.Cursor = Cursors.Hand;
            lblTogglePassword.Location = new Point(303, 10);
            lblTogglePassword.Name = "lblTogglePassword";
            lblTogglePassword.Size = new Size(34, 28);
            lblTogglePassword.TabIndex = 1;
            lblTogglePassword.Text = "";
            lblTogglePassword.ImageOptions.SvgImage = DxIcon.Eye;
            lblTogglePassword.ImageOptions.SvgImageSize = new Size(22, 22);
            lblTogglePassword.Click += lblTogglePassword_Click;
            // 
            // pnlUserNameBox
            // 
            pnlUserNameBox.Appearance.BackColor = Color.FromArgb(248, 250, 252);
            pnlUserNameBox.Appearance.Options.UseBackColor = true;
            pnlUserNameBox.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlUserNameBox.Controls.Add(lblUserIcon);
            pnlUserNameBox.Controls.Add(txtUserName);
            pnlUserNameBox.Location = new Point(51, 228);
            pnlUserNameBox.Name = "pnlUserNameBox";
            pnlUserNameBox.Size = new Size(343, 48);
            pnlUserNameBox.TabIndex = 3;
            // 
            // lblUserIcon
            // 
            lblUserIcon.Appearance.BackColor = Color.Transparent;
            lblUserIcon.Appearance.Font = new Font("Segoe UI Emoji", 12F);
            lblUserIcon.Appearance.Options.UseBackColor = true;
            lblUserIcon.Appearance.Options.UseFont = true;
            lblUserIcon.Appearance.Options.UseTextOptions = true;
            lblUserIcon.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblUserIcon.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblUserIcon.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblUserIcon.Location = new Point(0, 0);
            lblUserIcon.Name = "lblUserIcon";
            lblUserIcon.Size = new Size(42, 48);
            lblUserIcon.TabIndex = 0;
            lblUserIcon.Text = "";
            lblUserIcon.ImageOptions.SvgImage = DxIcon.User;
            lblUserIcon.ImageOptions.SvgImageSize = new Size(22, 22);
            // 
            // txtUserName
            // 
            txtUserName.EditValue = "Admin";
            txtUserName.Location = new Point(48, 11);
            txtUserName.Name = "txtUserName";
            txtUserName.Properties.Appearance.BackColor = Color.Transparent;
            txtUserName.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtUserName.Properties.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            txtUserName.Properties.Appearance.Options.UseBackColor = true;
            txtUserName.Properties.Appearance.Options.UseFont = true;
            txtUserName.Properties.Appearance.Options.UseForeColor = true;
            txtUserName.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtUserName.Properties.NullText = "Kullanıcı adı veya e-posta";
            txtUserName.Size = new Size(287, 24);
            txtUserName.TabIndex = 0;
            txtUserName.ToolTip = "Kullanıcı adı veya e-posta";
            // 
            // lblFormSubtitle
            // 
            lblFormSubtitle.Appearance.Font = new Font("Segoe UI", 10F);
            lblFormSubtitle.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            lblFormSubtitle.Appearance.Options.UseFont = true;
            lblFormSubtitle.Appearance.Options.UseForeColor = true;
            lblFormSubtitle.Appearance.Options.UseTextOptions = true;
            lblFormSubtitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblFormSubtitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblFormSubtitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblFormSubtitle.Location = new Point(34, 190);
            lblFormSubtitle.Name = "lblFormSubtitle";
            lblFormSubtitle.Size = new Size(377, 21);
            lblFormSubtitle.TabIndex = 2;
            lblFormSubtitle.Text = "Hesabınıza giriş yaparak devam edin";
            // 
            // lblFormTitle
            // 
            lblFormTitle.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblFormTitle.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            lblFormTitle.Appearance.Options.UseFont = true;
            lblFormTitle.Appearance.Options.UseForeColor = true;
            lblFormTitle.Appearance.Options.UseTextOptions = true;
            lblFormTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblFormTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblFormTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblFormTitle.Location = new Point(34, 152);
            lblFormTitle.Name = "lblFormTitle";
            lblFormTitle.Size = new Size(377, 34);
            lblFormTitle.TabIndex = 1;
            lblFormTitle.Text = "Hoş Geldiniz";
            // 
            // pnlUserBadge
            // 
            pnlUserBadge.Appearance.BackColor = Color.White;
            pnlUserBadge.Appearance.Options.UseBackColor = true;
            pnlUserBadge.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlUserBadge.Controls.Add(_lblLogoIcon);
            pnlUserBadge.Location = new Point(177, 36);
            pnlUserBadge.Name = "pnlUserBadge";
            pnlUserBadge.Size = new Size(92, 92);
            pnlUserBadge.TabIndex = 0;
            pnlUserBadge.Paint += pnlUserBadge_Paint;
            // 
            // _lblLogoIcon
            // 
            _lblLogoIcon.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            _lblLogoIcon.Appearance.Options.UseTextOptions = true;
            _lblLogoIcon.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            _lblLogoIcon.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            _lblLogoIcon.Dock = DockStyle.Fill;
            _lblLogoIcon.Location = new Point(0, 0);
            _lblLogoIcon.Name = "_lblLogoIcon";
            _lblLogoIcon.Size = new Size(92, 92);
            _lblLogoIcon.TabIndex = 0;
            _lblLogoIcon.Text = "";
            _lblLogoIcon.ImageOptions.SvgImage = DxIcon.Shield;
            _lblLogoIcon.ImageOptions.SvgImageSize = new Size(54, 54);
            // 
            // lblFooter
            // 
            lblFooter.Appearance.Font = new Font("Segoe UI", 8F);
            lblFooter.Appearance.ForeColor = Color.FromArgb(148, 163, 184);
            lblFooter.Appearance.Options.UseFont = true;
            lblFooter.Appearance.Options.UseForeColor = true;
            lblFooter.Appearance.Options.UseTextOptions = true;
            lblFooter.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblFooter.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblFooter.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblFooter.Dock = DockStyle.Bottom;
            lblFooter.Location = new Point(0, 529);
            lblFooter.Name = "lblFooter";
            lblFooter.Size = new Size(446, 26);
            lblFooter.TabIndex = 9;
            lblFooter.Text = "Maliyet Muhasebesi Otomasyonu v0.0.01";
            // 
            // XtraLoginForm
            // 
            AcceptButton = btnLogin;
            Appearance.BackColor = Color.White;
            Appearance.Options.UseBackColor = true;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(789, 555);
            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            IconOptions.Image = (Image)resources.GetObject("XtraLoginForm.IconOptions.Image");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "XtraLoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Maliyet Muhasebesi Otomasyonu - Giriş";
            ((System.ComponentModel.ISupportInitialize)pnlLeft).EndInit();
            pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlLeftBadge).EndInit();
            pnlLeftBadge.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlRight).EndInit();
            pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlCaptchaResult).EndInit();
            pnlCaptchaResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtCaptchaResult.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlPasswordBox).EndInit();
            pnlPasswordBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlUserNameBox).EndInit();
            pnlUserNameBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtUserName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlUserBadge).EndInit();
            pnlUserBadge.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.PanelControl pnlLeft;
        private DevExpress.XtraEditors.PanelControl pnlLeftBadge;
        private DevExpress.XtraEditors.LabelControl pnlLeftLockGlyph;
        private DevExpress.XtraEditors.LabelControl lblLeftTitle;
        private DevExpress.XtraEditors.LabelControl lblLeftCompany;
        private DevExpress.XtraEditors.LabelControl lblLeftFeatures;
        private DevExpress.XtraEditors.LabelControl lblLeftFooter;
        private DevExpress.XtraEditors.PanelControl pnlRight;
        private DevExpress.XtraEditors.PanelControl pnlUserBadge;
        private DevExpress.XtraEditors.LabelControl lblFormTitle;
        private DevExpress.XtraEditors.LabelControl lblFormSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlUserNameBox;
        private DevExpress.XtraEditors.LabelControl lblUserIcon;
        private DevExpress.XtraEditors.TextEdit txtUserName;
        private DevExpress.XtraEditors.PanelControl pnlPasswordBox;
        private DevExpress.XtraEditors.LabelControl lblPassIcon;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.LabelControl lblTogglePassword;
        private DevExpress.XtraEditors.LabelControl lblCaptchaQuestion;
        private DevExpress.XtraEditors.PanelControl pnlCaptchaResult;
        private DevExpress.XtraEditors.TextEdit txtCaptchaResult;
        private DevExpress.XtraEditors.LabelControl lblMessage;
        private DevExpress.XtraEditors.SimpleButton btnLogin;
        private DevExpress.XtraEditors.HyperlinkLabelControl lnkForgot;
private DevExpress.XtraEditors.LabelControl lblFooter;
        private DevExpress.XtraEditors.LabelControl _lblLogoIcon;
    }
}