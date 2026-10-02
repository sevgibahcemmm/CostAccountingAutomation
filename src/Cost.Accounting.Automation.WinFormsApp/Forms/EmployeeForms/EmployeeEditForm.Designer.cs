using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.EmployeeForms
{
    public partial class EmployeeEditForm : XtraForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;

        private DevExpress.XtraTab.XtraTabControl tabMain;
        private DevExpress.XtraTab.XtraTabPage tabPersonal;
        private DevExpress.XtraTab.XtraTabPage tabDuties;
        private DevExpress.XtraTab.XtraTabPage tabPhotos;

        private DevExpress.XtraEditors.LabelControl lblIdentityNumber;
        private DevExpress.XtraEditors.TextEdit txtIdentityNumber;
        private DevExpress.XtraEditors.LabelControl lblFirstName;
        private DevExpress.XtraEditors.TextEdit txtFirstName;
        private DevExpress.XtraEditors.LabelControl lblLastName;
        private DevExpress.XtraEditors.TextEdit txtLastName;
        private DevExpress.XtraEditors.LabelControl lblTitleField;
        private DevExpress.XtraEditors.TextEdit txtTitle;
        private DevExpress.XtraEditors.LabelControl lblPhone1;
        private DevExpress.XtraEditors.TextEdit txtPhone1;
        private DevExpress.XtraEditors.LabelControl lblPhone2;
        private DevExpress.XtraEditors.TextEdit txtPhone2;
        private DevExpress.XtraEditors.LabelControl lblEmail;
        private DevExpress.XtraEditors.TextEdit txtEmail;
        private DevExpress.XtraEditors.CheckEdit chkActive;
        private DevExpress.XtraEditors.LabelControl lblNote;
        private DevExpress.XtraEditors.SimpleButton btnAddDutyShortcut;

        private DevExpress.XtraEditors.LabelControl lblIconIdentityNumber;
        private DevExpress.XtraEditors.LabelControl lblIconFirstName;
        private DevExpress.XtraEditors.LabelControl lblIconLastName;
        private DevExpress.XtraEditors.LabelControl lblIconTitle;
        private DevExpress.XtraEditors.LabelControl lblIconPhone1;
        private DevExpress.XtraEditors.LabelControl lblIconEmail;

        private DevExpress.XtraEditors.SimpleButton btnAddDuty;
        private DevExpress.XtraEditors.SimpleButton btnRemoveDuty;
        private DevExpress.XtraEditors.LabelControl lblDutyNote;
        private DevExpress.XtraGrid.GridControl gridDuties;
        private DevExpress.XtraGrid.Views.Grid.GridView viewDuties;

        private DevExpress.XtraEditors.PictureEdit picPhoto;
        private DevExpress.XtraEditors.SimpleButton btnAddPhoto;
        private DevExpress.XtraEditors.SimpleButton btnRemovePhoto;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EmployeeEditForm));
            pnlHeader = new PanelControl();
            lblSubtitle = new LabelControl();
            lblTitle = new LabelControl();
            lblHeaderIcon = new LabelControl();
            pnlHeaderLine = new PanelControl();
            tabMain = new DevExpress.XtraTab.XtraTabControl();
            tabPersonal = new DevExpress.XtraTab.XtraTabPage();
            lblIconEmail = new LabelControl();
            txtEmail = new TextEdit();
            lblIconPhone1 = new LabelControl();
            txtPhone1 = new TextEdit();
            lblIconTitle = new LabelControl();
            txtTitle = new TextEdit();
            lblIconLastName = new LabelControl();
            txtLastName = new TextEdit();
            lblIconFirstName = new LabelControl();
            txtFirstName = new TextEdit();
            lblIconIdentityNumber = new LabelControl();
            txtIdentityNumber = new TextEdit();
            lblNote = new LabelControl();
            btnAddDutyShortcut = new SimpleButton();
            chkActive = new CheckEdit();
            lblEmail = new LabelControl();
            txtPhone2 = new TextEdit();
            lblPhone2 = new LabelControl();
            lblPhone1 = new LabelControl();
            lblTitleField = new LabelControl();
            lblLastName = new LabelControl();
            lblFirstName = new LabelControl();
            lblIdentityNumber = new LabelControl();
            tabDuties = new DevExpress.XtraTab.XtraTabPage();
            gridDuties = new GridControl();
            viewDuties = new GridView();
            lblDutyNote = new LabelControl();
            btnRemoveDuty = new SimpleButton();
            btnAddDuty = new SimpleButton();
            tabPhotos = new DevExpress.XtraTab.XtraTabPage();
            btnRemovePhoto = new SimpleButton();
            btnAddPhoto = new SimpleButton();
            picPhoto = new PictureEdit();
            pnlFooter = new PanelControl();
            btnSave = new SimpleButton();
            btnCancel = new SimpleButton();
            pnlFooterLine = new PanelControl();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tabMain).BeginInit();
            tabMain.SuspendLayout();
            tabPersonal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtTitle.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtLastName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtFirstName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtIdentityNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone2.Properties).BeginInit();
            tabDuties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridDuties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewDuties).BeginInit();
            tabPhotos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPhoto.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(560, 58);
            pnlHeader.TabIndex = 0;
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
            lblSubtitle.Size = new Size(190, 13);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "Personel bilgilerini eksiksiz doldurun";
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
            lblTitle.Text = "Personel Bilgileri";
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblHeaderIcon.ImageOptions.SvgImage");
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(32, 32);
            lblHeaderIcon.Location = new Point(18, 13);
            lblHeaderIcon.Margin = new Padding(3, 2, 3, 2);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(32, 32);
            lblHeaderIcon.TabIndex = 2;
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.BackColor = Color.FromArgb(224, 226, 230);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = DockStyle.Bottom;
            pnlHeaderLine.Location = new Point(0, 57);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new Size(560, 1);
            pnlHeaderLine.TabIndex = 2;
            // 
            // tabMain
            // 
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 58);
            tabMain.Name = "tabMain";
            tabMain.SelectedTabPage = tabPersonal;
            tabMain.Size = new Size(560, 437);
            tabMain.TabIndex = 1;
            tabMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabPersonal, tabDuties, tabPhotos });
            // 
            // tabPersonal
            // 
            tabPersonal.Controls.Add(lblIconEmail);
            tabPersonal.Controls.Add(lblIconPhone1);
            tabPersonal.Controls.Add(lblIconTitle);
            tabPersonal.Controls.Add(lblIconLastName);
            tabPersonal.Controls.Add(lblIconFirstName);
            tabPersonal.Controls.Add(lblIconIdentityNumber);
            tabPersonal.Controls.Add(lblNote);
            tabPersonal.Controls.Add(btnAddDutyShortcut);
            tabPersonal.Controls.Add(chkActive);
            tabPersonal.Controls.Add(txtEmail);
            tabPersonal.Controls.Add(lblEmail);
            tabPersonal.Controls.Add(txtPhone2);
            tabPersonal.Controls.Add(lblPhone2);
            tabPersonal.Controls.Add(txtPhone1);
            tabPersonal.Controls.Add(lblPhone1);
            tabPersonal.Controls.Add(txtTitle);
            tabPersonal.Controls.Add(lblTitleField);
            tabPersonal.Controls.Add(txtLastName);
            tabPersonal.Controls.Add(lblLastName);
            tabPersonal.Controls.Add(txtFirstName);
            tabPersonal.Controls.Add(lblFirstName);
            tabPersonal.Controls.Add(txtIdentityNumber);
            tabPersonal.Controls.Add(lblIdentityNumber);
            tabPersonal.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("tabPersonal.ImageOptions.SvgImage");
            tabPersonal.ImageOptions.SvgImageSize = new Size(16, 16);
            tabPersonal.Name = "tabPersonal";
            tabPersonal.Size = new Size(558, 409);
            tabPersonal.Text = "Kişisel Bilgiler";
            // 
            // lblIconEmail
            // 
            lblIconEmail.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblIconEmail.ImageOptions.SvgImage");
            lblIconEmail.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconEmail.Location = new Point(24, 253);
            lblIconEmail.Name = "lblIconEmail";
            lblIconEmail.Size = new Size(18, 18);
            lblIconEmail.TabIndex = 21;
            lblIconEmail.Tag = txtEmail;
            lblIconEmail.MouseDown += FieldIcon_MouseDown;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(20, 249);
            txtEmail.Margin = new Padding(3, 2, 3, 2);
            txtEmail.Name = "txtEmail";
            txtEmail.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtEmail.Properties.Appearance.Options.UseFont = true;
            txtEmail.Properties.NullText = "ornek@kurum.gov.tr";
            txtEmail.Properties.Padding = new Padding(26, 2, 2, 2);
            txtEmail.Size = new Size(507, 28);
            txtEmail.TabIndex = 13;
            // 
            // lblIconPhone1
            // 
            lblIconPhone1.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblIconPhone1.ImageOptions.SvgImage");
            lblIconPhone1.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconPhone1.Location = new Point(24, 197);
            lblIconPhone1.Name = "lblIconPhone1";
            lblIconPhone1.Size = new Size(18, 18);
            lblIconPhone1.TabIndex = 20;
            lblIconPhone1.Tag = txtPhone1;
            lblIconPhone1.MouseDown += FieldIcon_MouseDown;
            // 
            // txtPhone1
            // 
            txtPhone1.Location = new Point(20, 193);
            txtPhone1.Margin = new Padding(3, 2, 3, 2);
            txtPhone1.Name = "txtPhone1";
            txtPhone1.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtPhone1.Properties.Appearance.Options.UseFont = true;
            txtPhone1.Properties.NullText = "0(5xx) xxx xx xx";
            txtPhone1.Properties.Padding = new Padding(26, 2, 2, 2);
            txtPhone1.Size = new Size(247, 28);
            txtPhone1.TabIndex = 9;
            // 
            // lblIconTitle
            // 
            lblIconTitle.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblIconTitle.ImageOptions.SvgImage");
            lblIconTitle.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconTitle.Location = new Point(291, 141);
            lblIconTitle.Name = "lblIconTitle";
            lblIconTitle.Size = new Size(18, 18);
            lblIconTitle.TabIndex = 19;
            lblIconTitle.Tag = txtTitle;
            lblIconTitle.MouseDown += FieldIcon_MouseDown;
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(287, 137);
            txtTitle.Margin = new Padding(3, 2, 3, 2);
            txtTitle.Name = "txtTitle";
            txtTitle.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtTitle.Properties.Appearance.Options.UseFont = true;
            txtTitle.Properties.NullText = "Raporlarda imza satırına basılır";
            txtTitle.Properties.Padding = new Padding(26, 2, 2, 2);
            txtTitle.Size = new Size(247, 28);
            txtTitle.TabIndex = 7;
            // 
            // lblIconLastName
            // 
            lblIconLastName.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblIconLastName.ImageOptions.SvgImage");
            lblIconLastName.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconLastName.Location = new Point(24, 141);
            lblIconLastName.Name = "lblIconLastName";
            lblIconLastName.Size = new Size(18, 18);
            lblIconLastName.TabIndex = 18;
            lblIconLastName.Tag = txtLastName;
            lblIconLastName.MouseDown += FieldIcon_MouseDown;
            // 
            // txtLastName
            // 
            txtLastName.Location = new Point(20, 137);
            txtLastName.Margin = new Padding(3, 2, 3, 2);
            txtLastName.Name = "txtLastName";
            txtLastName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtLastName.Properties.Appearance.Options.UseFont = true;
            txtLastName.Properties.NullText = "Soyad";
            txtLastName.Properties.Padding = new Padding(26, 2, 2, 2);
            txtLastName.Size = new Size(247, 28);
            txtLastName.TabIndex = 5;
            // 
            // lblIconFirstName
            // 
            lblIconFirstName.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblIconFirstName.ImageOptions.SvgImage");
            lblIconFirstName.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconFirstName.Location = new Point(27, 85);
            lblIconFirstName.Name = "lblIconFirstName";
            lblIconFirstName.Size = new Size(18, 18);
            lblIconFirstName.TabIndex = 17;
            lblIconFirstName.Tag = txtFirstName;
            lblIconFirstName.MouseDown += FieldIcon_MouseDown;
            // 
            // txtFirstName
            // 
            txtFirstName.Location = new Point(23, 81);
            txtFirstName.Margin = new Padding(3, 2, 3, 2);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtFirstName.Properties.Appearance.Options.UseFont = true;
            txtFirstName.Properties.NullText = "Ad";
            txtFirstName.Properties.Padding = new Padding(26, 2, 2, 2);
            txtFirstName.Size = new Size(247, 28);
            txtFirstName.TabIndex = 3;
            // 
            // lblIconIdentityNumber
            // 
            lblIconIdentityNumber.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblIconIdentityNumber.ImageOptions.SvgImage");
            lblIconIdentityNumber.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconIdentityNumber.Location = new Point(24, 34);
            lblIconIdentityNumber.Name = "lblIconIdentityNumber";
            lblIconIdentityNumber.Size = new Size(18, 18);
            lblIconIdentityNumber.TabIndex = 16;
            lblIconIdentityNumber.Tag = txtIdentityNumber;
            lblIconIdentityNumber.MouseDown += FieldIcon_MouseDown;
            // 
            // txtIdentityNumber
            // 
            txtIdentityNumber.Location = new Point(20, 30);
            txtIdentityNumber.Margin = new Padding(3, 2, 3, 2);
            txtIdentityNumber.Name = "txtIdentityNumber";
            txtIdentityNumber.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtIdentityNumber.Properties.Appearance.Options.UseFont = true;
            txtIdentityNumber.Properties.MaxLength = 11;
            txtIdentityNumber.Properties.NullText = "11 haneli TC kimlik no";
            txtIdentityNumber.Properties.Padding = new Padding(26, 2, 2, 2);
            txtIdentityNumber.Size = new Size(247, 28);
            txtIdentityNumber.TabIndex = 1;
            // 
            // lblNote
            // 
            lblNote.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblNote.Appearance.ForeColor = Color.FromArgb(130, 138, 150);
            lblNote.Appearance.Options.UseFont = true;
            lblNote.Appearance.Options.UseForeColor = true;
            lblNote.Location = new Point(20, 326);
            lblNote.Margin = new Padding(3, 2, 3, 2);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(138, 13);
            lblNote.TabIndex = 15;
            lblNote.Text = "Yetkili görev tanımlanmadı.";
            // 
            // btnAddDutyShortcut
            // 
            btnAddDutyShortcut.Appearance.Font = new Font("Segoe UI", 9F);
            btnAddDutyShortcut.Appearance.Options.UseFont = true;
            btnAddDutyShortcut.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnAddDutyShortcut.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnAddDutyShortcut.ImageOptions.SvgImage");
            btnAddDutyShortcut.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddDutyShortcut.Location = new Point(376, 288);
            btnAddDutyShortcut.Margin = new Padding(3, 2, 3, 2);
            btnAddDutyShortcut.Name = "btnAddDutyShortcut";
            btnAddDutyShortcut.Size = new Size(151, 32);
            btnAddDutyShortcut.TabIndex = 16;
            btnAddDutyShortcut.Text = "Görev Ekle";
            // 
            // chkActive
            // 
            chkActive.Location = new Point(24, 286);
            chkActive.Margin = new Padding(3, 2, 3, 2);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.Caption = "Aktif personel";
            chkActive.Size = new Size(180, 21);
            chkActive.TabIndex = 14;
            // 
            // lblEmail
            // 
            lblEmail.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblEmail.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblEmail.Appearance.Options.UseFont = true;
            lblEmail.Appearance.Options.UseForeColor = true;
            lblEmail.Location = new Point(20, 231);
            lblEmail.Margin = new Padding(3, 2, 3, 2);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(38, 13);
            lblEmail.TabIndex = 12;
            lblEmail.Text = "E-Posta";
            // 
            // txtPhone2
            // 
            txtPhone2.Location = new Point(287, 193);
            txtPhone2.Margin = new Padding(3, 2, 3, 2);
            txtPhone2.Name = "txtPhone2";
            txtPhone2.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtPhone2.Properties.Appearance.Options.UseFont = true;
            txtPhone2.Properties.NullText = "İsteğe bağlı";
            txtPhone2.Properties.Padding = new Padding(26, 2, 2, 2);
            txtPhone2.Size = new Size(247, 28);
            txtPhone2.TabIndex = 11;
            // 
            // lblPhone2
            // 
            lblPhone2.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPhone2.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblPhone2.Appearance.Options.UseFont = true;
            lblPhone2.Appearance.Options.UseForeColor = true;
            lblPhone2.Location = new Point(287, 175);
            lblPhone2.Margin = new Padding(3, 2, 3, 2);
            lblPhone2.Name = "lblPhone2";
            lblPhone2.Size = new Size(47, 13);
            lblPhone2.TabIndex = 10;
            lblPhone2.Text = "Telefon 2";
            // 
            // lblPhone1
            // 
            lblPhone1.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPhone1.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblPhone1.Appearance.Options.UseFont = true;
            lblPhone1.Appearance.Options.UseForeColor = true;
            lblPhone1.Location = new Point(20, 175);
            lblPhone1.Margin = new Padding(3, 2, 3, 2);
            lblPhone1.Name = "lblPhone1";
            lblPhone1.Size = new Size(47, 13);
            lblPhone1.TabIndex = 8;
            lblPhone1.Text = "Telefon 1";
            // 
            // lblTitleField
            // 
            lblTitleField.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblTitleField.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblTitleField.Appearance.Options.UseFont = true;
            lblTitleField.Appearance.Options.UseForeColor = true;
            lblTitleField.Location = new Point(287, 119);
            lblTitleField.Margin = new Padding(3, 2, 3, 2);
            lblTitleField.Name = "lblTitleField";
            lblTitleField.Size = new Size(36, 13);
            lblTitleField.TabIndex = 6;
            lblTitleField.Text = "Ünvanı";
            // 
            // lblLastName
            // 
            lblLastName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblLastName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblLastName.Appearance.Options.UseFont = true;
            lblLastName.Appearance.Options.UseForeColor = true;
            lblLastName.Location = new Point(20, 119);
            lblLastName.Margin = new Padding(3, 2, 3, 2);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(34, 13);
            lblLastName.TabIndex = 4;
            lblLastName.Text = "Soyadı";
            // 
            // lblFirstName
            // 
            lblFirstName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblFirstName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblFirstName.Appearance.Options.UseFont = true;
            lblFirstName.Appearance.Options.UseForeColor = true;
            lblFirstName.Location = new Point(23, 63);
            lblFirstName.Margin = new Padding(3, 2, 3, 2);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.Size = new Size(17, 13);
            lblFirstName.TabIndex = 2;
            lblFirstName.Text = "Adı";
            // 
            // lblIdentityNumber
            // 
            lblIdentityNumber.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblIdentityNumber.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblIdentityNumber.Appearance.Options.UseFont = true;
            lblIdentityNumber.Appearance.Options.UseForeColor = true;
            lblIdentityNumber.Location = new Point(20, 12);
            lblIdentityNumber.Margin = new Padding(3, 2, 3, 2);
            lblIdentityNumber.Name = "lblIdentityNumber";
            lblIdentityNumber.Size = new Size(63, 13);
            lblIdentityNumber.TabIndex = 0;
            lblIdentityNumber.Text = "TC Kimlik No";
            // 
            // tabDuties
            // 
            tabDuties.Controls.Add(gridDuties);
            tabDuties.Controls.Add(lblDutyNote);
            tabDuties.Controls.Add(btnRemoveDuty);
            tabDuties.Controls.Add(btnAddDuty);
            tabDuties.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("tabDuties.ImageOptions.SvgImage");
            tabDuties.ImageOptions.SvgImageSize = new Size(16, 16);
            tabDuties.Name = "tabDuties";
            tabDuties.Size = new Size(558, 409);
            tabDuties.Text = "Yetkili Görevler";
            // 
            // gridDuties
            // 
            gridDuties.Dock = DockStyle.Fill;
            gridDuties.Location = new Point(0, 0);
            gridDuties.MainView = viewDuties;
            gridDuties.Name = "gridDuties";
            gridDuties.Size = new Size(558, 409);
            gridDuties.TabIndex = 3;
            gridDuties.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewDuties });
            // 
            // viewDuties
            // 
            viewDuties.GridControl = gridDuties;
            viewDuties.Name = "viewDuties";
            viewDuties.OptionsBehavior.AutoPopulateColumns = false;
            viewDuties.OptionsView.EnableAppearanceEvenRow = true;
            viewDuties.OptionsView.EnableAppearanceOddRow = true;
            viewDuties.OptionsView.ShowGroupPanel = false;
            // 
            // lblDutyNote
            // 
            lblDutyNote.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblDutyNote.Appearance.ForeColor = Color.FromArgb(130, 138, 150);
            lblDutyNote.Appearance.Options.UseFont = true;
            lblDutyNote.Appearance.Options.UseForeColor = true;
            lblDutyNote.Location = new Point(316, 22);
            lblDutyNote.Margin = new Padding(3, 2, 3, 2);
            lblDutyNote.Name = "lblDutyNote";
            lblDutyNote.Size = new Size(240, 13);
            lblDutyNote.TabIndex = 2;
            lblDutyNote.Text = "Raporlar bu görevlere göre imza yetkilisi bulur.";
            // 
            // btnRemoveDuty
            // 
            btnRemoveDuty.Appearance.Font = new Font("Segoe UI", 9F);
            btnRemoveDuty.Appearance.Options.UseFont = true;
            btnRemoveDuty.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnRemoveDuty.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnRemoveDuty.ImageOptions.SvgImage");
            btnRemoveDuty.ImageOptions.SvgImageSize = new Size(16, 16);
            btnRemoveDuty.Location = new Point(168, 18);
            btnRemoveDuty.Margin = new Padding(3, 2, 3, 2);
            btnRemoveDuty.Name = "btnRemoveDuty";
            btnRemoveDuty.Size = new Size(140, 28);
            btnRemoveDuty.TabIndex = 1;
            btnRemoveDuty.Text = "Görevi Sil";
            // 
            // btnAddDuty
            // 
            btnAddDuty.Appearance.Font = new Font("Segoe UI", 9F);
            btnAddDuty.Appearance.Options.UseFont = true;
            btnAddDuty.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnAddDuty.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnAddDuty.ImageOptions.SvgImage");
            btnAddDuty.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddDuty.Location = new Point(20, 18);
            btnAddDuty.Margin = new Padding(3, 2, 3, 2);
            btnAddDuty.Name = "btnAddDuty";
            btnAddDuty.Size = new Size(140, 28);
            btnAddDuty.TabIndex = 0;
            btnAddDuty.Text = "Görev Ekle";
            // 
            // tabPhotos
            // 
            tabPhotos.Controls.Add(btnRemovePhoto);
            tabPhotos.Controls.Add(btnAddPhoto);
            tabPhotos.Controls.Add(picPhoto);
            tabPhotos.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("tabPhotos.ImageOptions.SvgImage");
            tabPhotos.ImageOptions.SvgImageSize = new Size(16, 16);
            tabPhotos.Name = "tabPhotos";
            tabPhotos.Size = new Size(558, 409);
            tabPhotos.Text = "Fotoğraf";
            // 
            // btnRemovePhoto
            // 
            btnRemovePhoto.Appearance.Font = new Font("Segoe UI", 9F);
            btnRemovePhoto.Appearance.Options.UseFont = true;
            btnRemovePhoto.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnRemovePhoto.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnRemovePhoto.ImageOptions.SvgImage");
            btnRemovePhoto.ImageOptions.SvgImageSize = new Size(16, 16);
            btnRemovePhoto.Location = new Point(190, 362);
            btnRemovePhoto.Margin = new Padding(3, 2, 3, 2);
            btnRemovePhoto.Name = "btnRemovePhoto";
            btnRemovePhoto.Size = new Size(150, 28);
            btnRemovePhoto.TabIndex = 2;
            btnRemovePhoto.Text = "Fotoğrafı Kaldır";
            // 
            // btnAddPhoto
            // 
            btnAddPhoto.Appearance.Font = new Font("Segoe UI", 9F);
            btnAddPhoto.Appearance.Options.UseFont = true;
            btnAddPhoto.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnAddPhoto.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnAddPhoto.ImageOptions.SvgImage");
            btnAddPhoto.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddPhoto.Location = new Point(20, 362);
            btnAddPhoto.Margin = new Padding(3, 2, 3, 2);
            btnAddPhoto.Name = "btnAddPhoto";
            btnAddPhoto.Size = new Size(150, 28);
            btnAddPhoto.TabIndex = 1;
            btnAddPhoto.Text = "Fotoğraf Ekle";
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
            picPhoto.Size = new Size(507, 330);
            picPhoto.TabIndex = 0;
            // 
            // pnlFooter
            // 
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 495);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(560, 60);
            pnlFooter.TabIndex = 2;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSave.Appearance.Options.UseFont = true;
            btnSave.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnSave.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnSave.ImageOptions.SvgImage");
            btnSave.ImageOptions.SvgImageSize = new Size(20, 20);
            btnSave.Location = new Point(392, 13);
            btnSave.Margin = new Padding(3, 2, 3, 2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(148, 32);
            btnSave.TabIndex = 2;
            btnSave.Text = "Kaydet";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnCancel.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnCancel.ImageOptions.SvgImage");
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.Location = new Point(280, 13);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(104, 32);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "İptal";
            // 
            // pnlFooterLine
            // 
            pnlFooterLine.Appearance.BackColor = Color.FromArgb(224, 226, 230);
            pnlFooterLine.Appearance.Options.UseBackColor = true;
            pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooterLine.Dock = DockStyle.Top;
            pnlFooterLine.Location = new Point(0, 0);
            pnlFooterLine.Name = "pnlFooterLine";
            pnlFooterLine.Size = new Size(560, 1);
            pnlFooterLine.TabIndex = 0;
            // 
            // EmployeeEditForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 555);
            Controls.Add(tabMain);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EmployeeEditForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Personel";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabMain).EndInit();
            tabMain.ResumeLayout(false);
            tabPersonal.ResumeLayout(false);
            tabPersonal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtTitle.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtLastName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtFirstName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtIdentityNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone2.Properties).EndInit();
            tabDuties.ResumeLayout(false);
            tabDuties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridDuties).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewDuties).EndInit();
            tabPhotos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPhoto.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }
    }
}