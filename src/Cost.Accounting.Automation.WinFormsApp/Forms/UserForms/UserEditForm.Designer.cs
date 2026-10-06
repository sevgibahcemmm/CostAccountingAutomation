using Cost.Accounting.Automation.WinFormsApp.Utils;
namespace Cost.Accounting.Automation.WinFormsApp.Forms.UserForms
{
    partial class UserEditForm
    {
        private System.ComponentModel.IContainer components = null;

        // Header
        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;

        // Sekmeler
        private DevExpress.XtraTab.XtraTabControl tabMain;
        private DevExpress.XtraTab.XtraTabPage tabPersonal;
        private DevExpress.XtraTab.XtraTabPage tabAccount;
        private DevExpress.XtraTab.XtraTabPage tabPhotos;

        // Sekme 1 - Kişisel Bilgiler
        private DevExpress.XtraEditors.LabelControl lblFirstName;
        private DevExpress.XtraEditors.TextEdit txtFirstName;
        private DevExpress.XtraEditors.LabelControl lblLastName;
        private DevExpress.XtraEditors.TextEdit txtLastName;
        private DevExpress.XtraEditors.LabelControl lblUserName;
        private DevExpress.XtraEditors.TextEdit txtUserName;
        private DevExpress.XtraEditors.LabelControl lblEmail;
        private DevExpress.XtraEditors.TextEdit txtEmail;

        // Sekme 2 - Hesap ve Yetkilendirme
        private DevExpress.XtraEditors.LabelControl lblTcNo;
        private DevExpress.XtraEditors.TextEdit txtTcNo;
        private DevExpress.XtraEditors.LabelControl lblRegistryNumber;
        private DevExpress.XtraEditors.TextEdit txtRegistryNumber;
        private DevExpress.XtraEditors.LabelControl lblCompany;
        private DevExpress.XtraEditors.SearchLookUpEdit cmbCompany;
        private DevExpress.XtraEditors.LabelControl lblRole;
        private DevExpress.XtraEditors.SearchLookUpEdit cmbRole;
        private DevExpress.XtraEditors.ToggleSwitch chkActive;
        private DevExpress.XtraEditors.LabelControl lblPasswordNote;

        // Sekme 3 - Avatar
        private DevExpress.XtraEditors.PictureEdit picPhoto;
        private DevExpress.XtraEditors.SimpleButton btnAddPhoto;
        private DevExpress.XtraEditors.SimpleButton btnRemovePhoto;

        // Alan simgeleri
        private DevExpress.XtraEditors.LabelControl lblIconFirstName;
        private DevExpress.XtraEditors.LabelControl lblIconLastName;
        private DevExpress.XtraEditors.LabelControl lblIconUserName;
        private DevExpress.XtraEditors.LabelControl lblIconEmail;
        private DevExpress.XtraEditors.LabelControl lblIconTcNo;
        private DevExpress.XtraEditors.LabelControl lblIconCompany;
        private DevExpress.XtraEditors.LabelControl lblIconRole;

        // Footer
        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;

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
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();

            tabMain = new DevExpress.XtraTab.XtraTabControl();
            tabPersonal = new DevExpress.XtraTab.XtraTabPage();
            tabAccount = new DevExpress.XtraTab.XtraTabPage();
            tabPhotos = new DevExpress.XtraTab.XtraTabPage();

            lblFirstName = new DevExpress.XtraEditors.LabelControl();
            txtFirstName = new DevExpress.XtraEditors.TextEdit();
            lblLastName = new DevExpress.XtraEditors.LabelControl();
            txtLastName = new DevExpress.XtraEditors.TextEdit();
            lblUserName = new DevExpress.XtraEditors.LabelControl();
            txtUserName = new DevExpress.XtraEditors.TextEdit();
            lblEmail = new DevExpress.XtraEditors.LabelControl();
            txtEmail = new DevExpress.XtraEditors.TextEdit();

            lblTcNo = new DevExpress.XtraEditors.LabelControl();
            txtTcNo = new DevExpress.XtraEditors.TextEdit();
            lblRegistryNumber = new DevExpress.XtraEditors.LabelControl();
            txtRegistryNumber = new DevExpress.XtraEditors.TextEdit();
            lblCompany = new DevExpress.XtraEditors.LabelControl();
            cmbCompany = new DevExpress.XtraEditors.SearchLookUpEdit();
            lblRole = new DevExpress.XtraEditors.LabelControl();
            cmbRole = new DevExpress.XtraEditors.SearchLookUpEdit();
            chkActive = new DevExpress.XtraEditors.ToggleSwitch();
            lblPasswordNote = new DevExpress.XtraEditors.LabelControl();

            picPhoto = new DevExpress.XtraEditors.PictureEdit();
            btnAddPhoto = new DevExpress.XtraEditors.SimpleButton();
            btnRemovePhoto = new DevExpress.XtraEditors.SimpleButton();

            lblIconFirstName = new DevExpress.XtraEditors.LabelControl();
            lblIconLastName = new DevExpress.XtraEditors.LabelControl();
            lblIconUserName = new DevExpress.XtraEditors.LabelControl();
            lblIconEmail = new DevExpress.XtraEditors.LabelControl();
            lblIconTcNo = new DevExpress.XtraEditors.LabelControl();
            lblIconCompany = new DevExpress.XtraEditors.LabelControl();
            lblIconRole = new DevExpress.XtraEditors.LabelControl();

            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tabMain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtFirstName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtLastName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtUserName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtTcNo.Properties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)txtRegistryNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbCompany.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbRole.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picPhoto.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();
            pnlHeader.SuspendLayout();
            tabMain.SuspendLayout();
            tabPersonal.SuspendLayout();
            tabAccount.SuspendLayout();
            tabPhotos.SuspendLayout();
            pnlFooter.SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.Appearance.Options.UseBackColor = false;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(480, 58);
            pnlHeader.TabIndex = 0;
            //
            // lblHeaderIcon
            //
            lblHeaderIcon.Location = new Point(18, 13);
            lblHeaderIcon.Margin = new Padding(3, 2, 3, 2);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(32, 32);
            lblHeaderIcon.TabIndex = 2;
            lblHeaderIcon.ImageOptions.SvgImage = DxIcon.User;
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(32, 32);
            //
            // lblTitle
            //
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 12F);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new Point(62, 12);
            lblTitle.Margin = new Padding(3, 2, 3, 2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(122, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Kullanıcı Bilgileri";
            //
            // lblSubtitle
            //
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(130, 138, 150);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(62, 35);
            lblSubtitle.Margin = new Padding(3, 2, 3, 2);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(151, 14);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "Kullanıcı bilgilerini eksiksiz doldurun";
            //
            // pnlHeaderLine
            //
            pnlHeaderLine.Appearance.BackColor = Color.FromArgb(224, 226, 230);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = DockStyle.Bottom;
            pnlHeaderLine.Location = new Point(0, 57);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new Size(480, 1);
            pnlHeaderLine.TabIndex = 2;
            //
            // tabMain
            //
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 58);
            tabMain.Name = "tabMain";
            tabMain.SelectedTabPage = tabPersonal;
            tabMain.Size = new Size(480, 328);
            tabMain.TabIndex = 1;
            tabMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabPersonal, tabAccount, tabPhotos });
            //
            // tabPersonal
            //
            tabPersonal.Controls.Add(lblIconEmail);
            tabPersonal.Controls.Add(lblIconUserName);
            tabPersonal.Controls.Add(lblIconLastName);
            tabPersonal.Controls.Add(lblIconFirstName);
            tabPersonal.Controls.Add(txtEmail);
            tabPersonal.Controls.Add(lblEmail);
            tabPersonal.Controls.Add(txtUserName);
            tabPersonal.Controls.Add(lblUserName);
            tabPersonal.Controls.Add(txtLastName);
            tabPersonal.Controls.Add(lblLastName);
            tabPersonal.Controls.Add(txtFirstName);
            tabPersonal.Controls.Add(lblFirstName);
            tabPersonal.ImageOptions.SvgImage = DxIcon.User;
            tabPersonal.ImageOptions.SvgImageSize = new Size(16, 16);
            tabPersonal.Name = "tabPersonal";
            tabPersonal.Size = new Size(474, 355);
            tabPersonal.Text = "Kişisel Bilgiler";
            //
            // lblFirstName
            //
            lblFirstName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblFirstName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblFirstName.Appearance.Options.UseFont = true;
            lblFirstName.Appearance.Options.UseForeColor = true;
            lblFirstName.Location = new Point(20, 22);
            lblFirstName.Margin = new Padding(3, 2, 3, 2);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.Size = new Size(14, 14);
            lblFirstName.TabIndex = 0;
            lblFirstName.Text = "Ad";
            //
            // txtFirstName
            //
            txtFirstName.Location = new Point(20, 40);
            txtFirstName.Margin = new Padding(3, 2, 3, 2);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtFirstName.Properties.Appearance.Options.UseFont = true;
            txtFirstName.Properties.NullText = "Ahmet";
            txtFirstName.Properties.Padding = new Padding(26, 2, 2, 2);
            txtFirstName.Size = new Size(207, 26);
            txtFirstName.TabIndex = 1;
            //
            // lblLastName
            //
            lblLastName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblLastName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblLastName.Appearance.Options.UseFont = true;
            lblLastName.Appearance.Options.UseForeColor = true;
            lblLastName.Location = new Point(247, 22);
            lblLastName.Margin = new Padding(3, 2, 3, 2);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(31, 14);
            lblLastName.TabIndex = 2;
            lblLastName.Text = "Soyad";
            //
            // txtLastName
            //
            txtLastName.Location = new Point(247, 40);
            txtLastName.Margin = new Padding(3, 2, 3, 2);
            txtLastName.Name = "txtLastName";
            txtLastName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtLastName.Properties.Appearance.Options.UseFont = true;
            txtLastName.Properties.NullText = "Yılmaz";
            txtLastName.Properties.Padding = new Padding(26, 2, 2, 2);
            txtLastName.Size = new Size(207, 26);
            txtLastName.TabIndex = 3;
            //
            // lblUserName
            //
            lblUserName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblUserName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblUserName.Appearance.Options.UseFont = true;
            lblUserName.Appearance.Options.UseForeColor = true;
            lblUserName.Location = new Point(20, 80);
            lblUserName.Margin = new Padding(3, 2, 3, 2);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new Size(63, 14);
            lblUserName.TabIndex = 4;
            lblUserName.Text = "Kullanıcı Adı";
            //
            // txtUserName
            //
            txtUserName.Location = new Point(20, 98);
            txtUserName.Margin = new Padding(3, 2, 3, 2);
            txtUserName.Name = "txtUserName";
            txtUserName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtUserName.Properties.Appearance.Options.UseFont = true;
            txtUserName.Properties.NullText = "ayilmaz";
            txtUserName.Properties.Padding = new Padding(26, 2, 2, 2);
            txtUserName.Size = new Size(207, 26);
            txtUserName.TabIndex = 5;
            //
            // lblEmail
            //
            lblEmail.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblEmail.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblEmail.Appearance.Options.UseFont = true;
            lblEmail.Appearance.Options.UseForeColor = true;
            lblEmail.Location = new Point(247, 80);
            lblEmail.Margin = new Padding(3, 2, 3, 2);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(39, 14);
            lblEmail.TabIndex = 6;
            lblEmail.Text = "E-Posta";
            //
            // txtEmail
            //
            txtEmail.Location = new Point(247, 98);
            txtEmail.Margin = new Padding(3, 2, 3, 2);
            txtEmail.Name = "txtEmail";
            txtEmail.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtEmail.Properties.Appearance.Options.UseFont = true;
            txtEmail.Properties.NullText = "ahmet@firma.com";
            txtEmail.Properties.Padding = new Padding(26, 2, 2, 2);
            txtEmail.Size = new Size(207, 26);
            txtEmail.TabIndex = 7;
            //
            // tabAccount
            //
            tabAccount.Controls.Add(lblIconRole);
            tabAccount.Controls.Add(lblIconCompany);
            tabAccount.Controls.Add(lblIconTcNo);
            tabAccount.Controls.Add(lblPasswordNote);
            tabAccount.Controls.Add(chkActive);
            tabAccount.Controls.Add(cmbRole);
            tabAccount.Controls.Add(lblRole);
            tabAccount.Controls.Add(cmbCompany);
            tabAccount.Controls.Add(lblCompany);
            tabAccount.Controls.Add(txtTcNo);
            tabAccount.Controls.Add(lblTcNo);
            tabAccount.Controls.Add(txtRegistryNumber);
            tabAccount.Controls.Add(lblRegistryNumber);
            tabAccount.ImageOptions.SvgImage = DxIcon.Shield;
            tabAccount.ImageOptions.SvgImageSize = new Size(16, 16);
            tabAccount.Name = "tabAccount";
            tabAccount.Size = new Size(474, 355);
            tabAccount.Text = "Hesap ve Yetki";
            //
            // lblTcNo
            //
            lblTcNo.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblTcNo.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblTcNo.Appearance.Options.UseFont = true;
            lblTcNo.Appearance.Options.UseForeColor = true;
            lblTcNo.Location = new Point(20, 22);
            lblTcNo.Margin = new Padding(3, 2, 3, 2);
            lblTcNo.Name = "lblTcNo";
            lblTcNo.Size = new Size(63, 14);
            lblTcNo.TabIndex = 0;
            lblTcNo.Text = "TC Kimlik No";
            //
            // txtTcNo
            //
            txtTcNo.Location = new Point(20, 40);
            txtTcNo.Margin = new Padding(3, 2, 3, 2);
            txtTcNo.Name = "txtTcNo";
            txtTcNo.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtTcNo.Properties.Appearance.Options.UseFont = true;
            txtTcNo.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Simple;
            txtTcNo.Properties.Mask.EditMask = "000-0000-0000";
            txtTcNo.Properties.Mask.UseMaskAsDisplayFormat = true;
            txtTcNo.Properties.NullText = "12345678901";
            txtTcNo.Properties.Padding = new Padding(26, 2, 2, 2);
            txtTcNo.Size = new Size(434, 26);
            txtTcNo.TabIndex = 1;
            //
            // lblRegistryNumber
            //
            lblRegistryNumber.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblRegistryNumber.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblRegistryNumber.Appearance.Options.UseFont = true;
            lblRegistryNumber.Appearance.Options.UseForeColor = true;
            lblRegistryNumber.Location = new Point(20, 76);
            lblRegistryNumber.Margin = new Padding(3, 2, 3, 2);
            lblRegistryNumber.Name = "lblRegistryNumber";
            lblRegistryNumber.Size = new Size(80, 14);
            lblRegistryNumber.TabIndex = 2;
            lblRegistryNumber.Text = "Sicil Numarası";
            lblRegistryNumber.ToolTip = "Personel kaydıyla eşleştirmek için kullanılan sicil numarası.\r\nMesajlaşmada alıcı bu numarayla aranabilir.";
            //
            // txtRegistryNumber
            //
            txtRegistryNumber.Location = new Point(20, 94);
            txtRegistryNumber.Margin = new Padding(3, 2, 3, 2);
            txtRegistryNumber.Name = "txtRegistryNumber";
            txtRegistryNumber.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtRegistryNumber.Properties.Appearance.Options.UseFont = true;
            txtRegistryNumber.Properties.NullText = "Örn. ab12244";
            txtRegistryNumber.Properties.Padding = new Padding(26, 2, 2, 2);
            txtRegistryNumber.Size = new Size(434, 26);
            txtRegistryNumber.TabIndex = 3;
            //
            // lblCompany
            //
            lblCompany.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblCompany.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblCompany.Appearance.Options.UseFont = true;
            lblCompany.Appearance.Options.UseForeColor = true;
            lblCompany.Location = new Point(20, 134);
            lblCompany.Margin = new Padding(3, 2, 3, 2);
            lblCompany.Name = "lblCompany";
            lblCompany.Size = new Size(28, 14);
            lblCompany.TabIndex = 5;
            lblCompany.Text = "Şirket";
            //
            // cmbCompany
            //
            cmbCompany.Location = new Point(20, 152);
            cmbCompany.Margin = new Padding(3, 2, 3, 2);
            cmbCompany.Name = "cmbCompany";
            cmbCompany.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            cmbCompany.Properties.Appearance.Options.UseFont = true;
            cmbCompany.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbCompany.Properties.NullText = "Şirket ara / seç (boş = kendi şirketiniz)";
            cmbCompany.Properties.PopupFilterMode = DevExpress.XtraEditors.PopupFilterMode.Contains;
            cmbCompany.Properties.Padding = new Padding(26, 2, 2, 2);
            cmbCompany.Size = new Size(434, 26);
            cmbCompany.TabIndex = 4;
            //
            // lblRole
            //
            lblRole.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblRole.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblRole.Appearance.Options.UseFont = true;
            lblRole.Appearance.Options.UseForeColor = true;
            lblRole.Location = new Point(20, 192);
            lblRole.Margin = new Padding(3, 2, 3, 2);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(15, 14);
            lblRole.TabIndex = 6;
            lblRole.Text = "Rol";
            //
            // cmbRole
            //
            cmbRole.Location = new Point(20, 210);
            cmbRole.Margin = new Padding(3, 2, 3, 2);
            cmbRole.Name = "cmbRole";
            cmbRole.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            cmbRole.Properties.Appearance.Options.UseFont = true;
            cmbRole.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbRole.Properties.NullText = "Rol ara / seç";
            cmbRole.Properties.PopupFilterMode = DevExpress.XtraEditors.PopupFilterMode.Contains;
            cmbRole.Properties.Padding = new Padding(26, 2, 2, 2);
            cmbRole.Size = new Size(434, 26);
            cmbRole.TabIndex = 7;
            //
            // chkActive
            //
            chkActive.Location = new Point(20, 250);
            chkActive.Margin = new Padding(3, 2, 3, 2);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.ShowText = true;
            chkActive.Properties.OnText = "Aktif";
            chkActive.Properties.OffText = "Pasif";
            chkActive.ToolTip = "Aktif Kullanıcı";
            chkActive.Size = new Size(160, 21);
            chkActive.TabIndex = 8;
            //
            // lblPasswordNote
            //
            lblPasswordNote.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPasswordNote.Appearance.ForeColor = Color.FromArgb(130, 138, 150);
            lblPasswordNote.Appearance.Options.UseFont = true;
            lblPasswordNote.Appearance.Options.UseForeColor = true;
            lblPasswordNote.Location = new Point(20, 280);
            lblPasswordNote.Margin = new Padding(3, 2, 3, 2);
            lblPasswordNote.Name = "lblPasswordNote";
            lblPasswordNote.Size = new Size(87, 14);
            lblPasswordNote.TabIndex = 9;
            lblPasswordNote.Text = "İlk giriş şifresi: 123";
            //
            // lblIconFirstName
            //
            lblIconFirstName.Cursor = Cursors.Default;
            lblIconFirstName.Location = new Point(24, 44);
            lblIconFirstName.Name = "lblIconFirstName";
            lblIconFirstName.Size = new Size(18, 18);
            lblIconFirstName.ImageOptions.SvgImage = DxIcon.User;
            lblIconFirstName.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconFirstName.Tag = txtFirstName;
            lblIconFirstName.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconLastName
            //
            lblIconLastName.Cursor = Cursors.Default;
            lblIconLastName.Location = new Point(251, 44);
            lblIconLastName.Name = "lblIconLastName";
            lblIconLastName.Size = new Size(18, 18);
            lblIconLastName.ImageOptions.SvgImage = DxIcon.User;
            lblIconLastName.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconLastName.Tag = txtLastName;
            lblIconLastName.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconUserName
            //
            lblIconUserName.Cursor = Cursors.Default;
            lblIconUserName.Location = new Point(24, 102);
            lblIconUserName.Name = "lblIconUserName";
            lblIconUserName.Size = new Size(18, 18);
            lblIconUserName.ImageOptions.SvgImage = DxIcon.At;
            lblIconUserName.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconUserName.Tag = txtUserName;
            lblIconUserName.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconEmail
            //
            lblIconEmail.Cursor = Cursors.Default;
            lblIconEmail.Location = new Point(251, 102);
            lblIconEmail.Name = "lblIconEmail";
            lblIconEmail.Size = new Size(18, 18);
            lblIconEmail.ImageOptions.SvgImage = DxIcon.Mail;
            lblIconEmail.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconEmail.Tag = txtEmail;
            lblIconEmail.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconTcNo
            //
            lblIconTcNo.Cursor = Cursors.Default;
            lblIconTcNo.Location = new Point(24, 44);
            lblIconTcNo.Name = "lblIconTcNo";
            lblIconTcNo.Size = new Size(18, 18);
            lblIconTcNo.ImageOptions.SvgImage = DxIcon.IdCard;
            lblIconTcNo.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconTcNo.Tag = txtTcNo;
            lblIconTcNo.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconCompany
            //
            lblIconCompany.Cursor = Cursors.Default;
            lblIconCompany.Location = new Point(24, 102);
            lblIconCompany.Name = "lblIconCompany";
            lblIconCompany.Size = new Size(18, 18);
            lblIconCompany.ImageOptions.SvgImage = DxIcon.Company;
            lblIconCompany.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconCompany.Tag = cmbCompany;
            lblIconCompany.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconRole
            //
            lblIconRole.Cursor = Cursors.Default;
            lblIconRole.Location = new Point(24, 160);
            lblIconRole.Name = "lblIconRole";
            lblIconRole.Size = new Size(18, 18);
            lblIconRole.ImageOptions.SvgImage = DxIcon.Key;
            lblIconRole.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconRole.Tag = cmbRole;
            lblIconRole.MouseDown += FieldIcon_MouseDown;
            //
            // tabPhotos
            //
            tabPhotos.Controls.Add(btnRemovePhoto);
            tabPhotos.Controls.Add(btnAddPhoto);
            tabPhotos.Controls.Add(picPhoto);
            tabPhotos.ImageOptions.SvgImage = DxIcon.Photo;
            tabPhotos.ImageOptions.SvgImageSize = new Size(16, 16);
            tabPhotos.Name = "tabPhotos";
            tabPhotos.Size = new Size(474, 355);
            tabPhotos.Text = "Fotoğraflar";
            //
            // picPhoto
            //
            picPhoto.Location = new Point(20, 20);
            picPhoto.Margin = new Padding(3, 2, 3, 2);
            picPhoto.Name = "picPhoto";
            picPhoto.Properties.Appearance.BackColor = Color.FromArgb(245, 246, 248);
            picPhoto.Properties.Appearance.Options.UseBackColor = true;
            picPhoto.Properties.ShowMenu = false;
            picPhoto.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            picPhoto.Size = new Size(434, 178);
            picPhoto.TabIndex = 1;
            //
            // btnAddPhoto
            //
            btnAddPhoto.Appearance.Font = new Font("Segoe UI", 9F);
            btnAddPhoto.Appearance.Options.UseFont = true;
            btnAddPhoto.Location = new Point(20, 170);
            btnAddPhoto.Margin = new Padding(3, 2, 3, 2);
            btnAddPhoto.Name = "btnAddPhoto";
            btnAddPhoto.Size = new Size(136, 28);
            btnAddPhoto.TabIndex = 2;
            btnAddPhoto.Text = "Fotoğraf Ekle";
            btnAddPhoto.ImageOptions.SvgImage = DxIcon.Add;
            btnAddPhoto.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddPhoto.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
//
            // btnRemovePhoto
            //
            btnRemovePhoto.Appearance.Font = new Font("Segoe UI", 9F);
            btnRemovePhoto.Appearance.Options.UseFont = true;
            btnRemovePhoto.Location = new Point(308, 170);
            btnRemovePhoto.Margin = new Padding(3, 2, 3, 2);
            btnRemovePhoto.Name = "btnRemovePhoto";
            btnRemovePhoto.Size = new Size(146, 28);
            btnRemovePhoto.TabIndex = 4;
            btnRemovePhoto.Text = "Kaldır";
            btnRemovePhoto.ImageOptions.SvgImage = DxIcon.Delete;
            btnRemovePhoto.ImageOptions.SvgImageSize = new Size(16, 16);
            btnRemovePhoto.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            //
            // pnlFooter
            //
            pnlFooter.Appearance.Options.UseBackColor = false;
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 386);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(480, 60);
            pnlFooter.TabIndex = 2;
            //
            // pnlFooterLine
            //
            pnlFooterLine.Appearance.BackColor = Color.FromArgb(224, 226, 230);
            pnlFooterLine.Appearance.Options.UseBackColor = true;
            pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooterLine.Dock = DockStyle.Top;
            pnlFooterLine.Location = new Point(0, 0);
            pnlFooterLine.Name = "pnlFooterLine";
            pnlFooterLine.Size = new Size(480, 1);
            pnlFooterLine.TabIndex = 0;
            //
            // btnCancel
            //
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Location = new Point(200, 13);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(104, 32);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "İptal";
            btnCancel.ImageOptions.SvgImage = DxIcon.Close;
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            //
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Location = new Point(312, 13);
            btnSave.Margin = new Padding(3, 2, 3, 2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(148, 32);
            btnSave.TabIndex = 2;
            btnSave.Text = "Kaydet";
            btnSave.ImageOptions.SvgImage = DxIcon.Check;
            btnSave.ImageOptions.SvgImageSize = new Size(20, 20);
            btnSave.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            //
            // UserEditForm
            //
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 504);
            Controls.Add(tabMain);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UserEditForm";
            StartPosition = FormStartPosition.CenterParent;
            IconOptions.SvgImage = DxIcon.Users;
            Text = "Kullanıcı";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            tabPersonal.ResumeLayout(false);
            tabPersonal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtFirstName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtLastName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtUserName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).EndInit();
            tabAccount.ResumeLayout(false);
            tabAccount.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtTcNo.Properties).EndInit();
        ((System.ComponentModel.ISupportInitialize)txtRegistryNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCompany.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbRole.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            tabPhotos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPhoto.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabMain).EndInit();
            tabMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }
    }
}
