using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.EmployeeForms
{
    /// <summary>
    /// Personel kayıt formu. Kişisel bilgiler iki sütunlu bir ızgaraya yerleşir
    /// (sol sütun 28..408, sağ sütun 432..812), her etiket girdinin 22 piksel
    /// üstündedir ve etiketler 17 piksel yüksekliğindedir; böylece metin
    /// kırpılmaz. Yetkili görevler kendi araç çubuğunu, fotoğraf sekmesi
    /// tam genişlikte bir önizleme alanını taşır.
    ///
    /// <para>
    /// Renkler burada sabitlenmez; hepsi <c>EmployeeEditForm.ApplySkin</c>
    /// tarafından aktif skinden çözülür.
    /// </para>
    /// </summary>
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
        private DevExpress.XtraEditors.LabelControl lblIdentityHint;

        private DevExpress.XtraEditors.LabelControl lblTitleField;
        private DevExpress.XtraEditors.TextEdit txtTitle;
        private DevExpress.XtraEditors.LabelControl lblFirstName;
        private DevExpress.XtraEditors.TextEdit txtFirstName;
        private DevExpress.XtraEditors.LabelControl lblLastName;
        private DevExpress.XtraEditors.TextEdit txtLastName;
        private DevExpress.XtraEditors.LabelControl lblPhone1;
        private DevExpress.XtraEditors.TextEdit txtPhone1;
        private DevExpress.XtraEditors.LabelControl lblPhone2;
        private DevExpress.XtraEditors.TextEdit txtPhone2;
        private DevExpress.XtraEditors.LabelControl lblEmail;
        private DevExpress.XtraEditors.TextEdit txtEmail;
        private DevExpress.XtraEditors.LabelControl lblRegistryNumber;
        private DevExpress.XtraEditors.TextEdit txtRegistryNumber;
        private DevExpress.XtraEditors.ToggleSwitch chkActive;
        private DevExpress.XtraEditors.SimpleButton btnAddDutyShortcut;
        private DevExpress.XtraEditors.LabelControl lblNote;

        private DevExpress.XtraGrid.GridControl gridDuties;
        private DevExpress.XtraGrid.Views.Grid.GridView viewDuties;
        private DevExpress.XtraEditors.PanelControl pnlDutyBar;
        private DevExpress.XtraEditors.SimpleButton btnAddDuty;
        private DevExpress.XtraEditors.SimpleButton btnRemoveDuty;
        private DevExpress.XtraEditors.SimpleButton btnNewSigningRole;
        private DevExpress.XtraEditors.LabelControl lblDutyNote;

        private DevExpress.XtraEditors.PictureEdit picPhoto;
        private DevExpress.XtraEditors.SimpleButton btnAddPhoto;
        private DevExpress.XtraEditors.SimpleButton btnRemovePhoto;

        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _skinBinding?.Dispose();
            }

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
            lblNote = new LabelControl();
            btnAddDutyShortcut = new SimpleButton();
            chkActive = new ToggleSwitch();
            txtEmail = new TextEdit();
            txtRegistryNumber = new TextEdit();
            lblRegistryNumber = new LabelControl();
            lblIdentityHint = new LabelControl();
            lblEmail = new LabelControl();
            txtPhone2 = new TextEdit();
            lblPhone2 = new LabelControl();
            txtPhone1 = new TextEdit();
            lblPhone1 = new LabelControl();
            txtLastName = new TextEdit();
            lblLastName = new LabelControl();
            txtFirstName = new TextEdit();
            lblFirstName = new LabelControl();
            txtTitle = new TextEdit();
            lblTitleField = new LabelControl();
            txtIdentityNumber = new TextEdit();
            lblIdentityNumber = new LabelControl();
            tabDuties = new DevExpress.XtraTab.XtraTabPage();
            gridDuties = new GridControl();
            viewDuties = new GridView();
            pnlDutyBar = new PanelControl();
            lblDutyNote = new LabelControl();
            btnNewSigningRole = new SimpleButton();
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
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtRegistryNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone2.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtLastName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtFirstName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtTitle.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtIdentityNumber.Properties).BeginInit();
            tabDuties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridDuties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewDuties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlDutyBar).BeginInit();
            pnlDutyBar.SuspendLayout();
            tabPhotos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPhoto.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 2, 3, 2);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(720, 52);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Location = new Point(55, 31);
            lblSubtitle.Margin = new Padding(3, 2, 3, 2);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(296, 15);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "Rapor imza bloklarında kullanılacak personeli tanımlayın";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 13F);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new Point(55, 10);
            lblTitle.Margin = new Padding(3, 2, 3, 2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(65, 23);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Personel";
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblHeaderIcon.ImageOptions.SvgImage");
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(26, 26);
            lblHeaderIcon.Location = new Point(24, 15);
            lblHeaderIcon.Margin = new Padding(3, 2, 3, 2);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(26, 26);
            lblHeaderIcon.TabIndex = 2;
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = DockStyle.Bottom;
            pnlHeaderLine.Location = new Point(0, 51);
            pnlHeaderLine.Margin = new Padding(3, 2, 3, 2);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new Size(720, 1);
            pnlHeaderLine.TabIndex = 2;
            // 
            // tabMain
            // 
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 52);
            tabMain.Margin = new Padding(3, 2, 3, 2);
            tabMain.Name = "tabMain";
            tabMain.SelectedTabPage = tabPersonal;
            tabMain.Size = new Size(720, 453);
            tabMain.TabIndex = 1;
            tabMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabPersonal, tabDuties, tabPhotos });
            // 
            // tabPersonal
            // 
            tabPersonal.Controls.Add(lblNote);
            tabPersonal.Controls.Add(btnAddDutyShortcut);
            tabPersonal.Controls.Add(chkActive);
            tabPersonal.Controls.Add(txtEmail);
            tabPersonal.Controls.Add(txtRegistryNumber);
            tabPersonal.Controls.Add(lblRegistryNumber);
            tabPersonal.Controls.Add(lblIdentityHint);
            tabPersonal.Controls.Add(lblEmail);
            tabPersonal.Controls.Add(txtPhone2);
            tabPersonal.Controls.Add(lblPhone2);
            tabPersonal.Controls.Add(txtPhone1);
            tabPersonal.Controls.Add(lblPhone1);
            tabPersonal.Controls.Add(txtLastName);
            tabPersonal.Controls.Add(lblLastName);
            tabPersonal.Controls.Add(txtFirstName);
            tabPersonal.Controls.Add(lblFirstName);
            tabPersonal.Controls.Add(txtTitle);
            tabPersonal.Controls.Add(lblTitleField);
            tabPersonal.Controls.Add(txtIdentityNumber);
            tabPersonal.Controls.Add(lblIdentityNumber);
            tabPersonal.Margin = new Padding(3, 2, 3, 2);
            tabPersonal.Name = "tabPersonal";
            tabPersonal.Size = new Size(718, 428);
            tabPersonal.Text = "Kişisel Bilgiler";
            // 
            // lblNote
            // 
            lblNote.Appearance.BackColor = Color.Transparent;
            lblNote.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNote.Appearance.ForeColor = Color.Red;
            lblNote.Appearance.Options.UseBackColor = true;
            lblNote.Appearance.Options.UseFont = true;
            lblNote.Appearance.Options.UseForeColor = true;
            lblNote.Location = new Point(24, 327);
            lblNote.Margin = new Padding(3, 2, 3, 2);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(245, 15);
            lblNote.TabIndex = 9;
            lblNote.Text = "ZORUNLU: En az bir yetkili görev tanımlayın.";
            // 
            // btnAddDutyShortcut
            // 
            btnAddDutyShortcut.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnAddDutyShortcut.Appearance.Options.UseFont = true;
            btnAddDutyShortcut.Cursor = Cursors.Hand;
            btnAddDutyShortcut.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnAddDutyShortcut.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnAddDutyShortcut.ImageOptions.SvgImage");
            btnAddDutyShortcut.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddDutyShortcut.Location = new Point(525, 290);
            btnAddDutyShortcut.Margin = new Padding(3, 2, 3, 2);
            btnAddDutyShortcut.Name = "btnAddDutyShortcut";
            btnAddDutyShortcut.Size = new Size(171, 26);
            btnAddDutyShortcut.TabIndex = 8;
            btnAddDutyShortcut.Text = "Yetkili Görev Ekle";
            // 
            // chkActive
            // 
            chkActive.Location = new Point(24, 273);
            chkActive.Margin = new Padding(3, 2, 3, 2);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.OffText = "Pasif";
            chkActive.Properties.OnText = "Aktif";
            chkActive.Size = new Size(189, 22);
            chkActive.TabIndex = 7;
            chkActive.ToolTip = "Aktif personel";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(24, 232);
            txtEmail.Margin = new Padding(3, 2, 3, 2);
            txtEmail.Name = "txtEmail";
            txtEmail.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtEmail.Properties.Appearance.Options.UseFont = true;
            txtEmail.Properties.NullText = "ornek@kurum.gov.tr";
            txtEmail.Properties.Padding = new Padding(10, 5, 10, 5);
            txtEmail.Size = new Size(326, 36);
            txtEmail.TabIndex = 6;
            // 
            // txtRegistryNumber
            // 
            txtRegistryNumber.Location = new Point(370, 232);
            txtRegistryNumber.Margin = new Padding(3, 2, 3, 2);
            txtRegistryNumber.Name = "txtRegistryNumber";
            txtRegistryNumber.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtRegistryNumber.Properties.Appearance.Options.UseFont = true;
            txtRegistryNumber.Properties.MaxLength = 50;
            txtRegistryNumber.Properties.NullText = "Kurum sicil numarasi (istege bagli)";
            txtRegistryNumber.Properties.Padding = new Padding(10, 5, 10, 5);
            txtRegistryNumber.Size = new Size(326, 36);
            txtRegistryNumber.TabIndex = 13;
            // 
            // lblRegistryNumber
            // 
            lblRegistryNumber.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblRegistryNumber.Appearance.Options.UseFont = true;
            lblRegistryNumber.Location = new Point(370, 214);
            lblRegistryNumber.Margin = new Padding(3, 2, 3, 2);
            lblRegistryNumber.Name = "lblRegistryNumber";
            lblRegistryNumber.Size = new Size(74, 17);
            lblRegistryNumber.TabIndex = 15;
            lblRegistryNumber.Text = "Sicil No";
            // 
            // lblIdentityHint
            // 
            lblIdentityHint.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblIdentityHint.Appearance.Options.UseFont = true;
            lblIdentityHint.Location = new Point(24, 71);
            lblIdentityHint.Margin = new Padding(3, 2, 3, 2);
            lblIdentityHint.Name = "lblIdentityHint";
            lblIdentityHint.Size = new Size(57, 13);
            lblIdentityHint.TabIndex = 14;
            lblIdentityHint.Text = "0 / 11 hane";
            // 
            // lblEmail
            // 
            lblEmail.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblEmail.Appearance.Options.UseFont = true;
            lblEmail.Location = new Point(24, 214);
            lblEmail.Margin = new Padding(3, 2, 3, 2);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(46, 17);
            lblEmail.TabIndex = 12;
            lblEmail.Text = "E-Posta";
            // 
            // txtPhone2
            // 
            txtPhone2.Location = new Point(370, 171);
            txtPhone2.Margin = new Padding(3, 2, 3, 2);
            txtPhone2.Name = "txtPhone2";
            txtPhone2.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtPhone2.Properties.Appearance.Options.UseFont = true;
            txtPhone2.Properties.NullText = "0 (5xx) xxx xx xx";
            txtPhone2.Properties.Padding = new Padding(10, 5, 10, 5);
            txtPhone2.Size = new Size(326, 36);
            txtPhone2.TabIndex = 5;
            // 
            // lblPhone2
            // 
            lblPhone2.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblPhone2.Appearance.Options.UseFont = true;
            lblPhone2.Location = new Point(370, 153);
            lblPhone2.Margin = new Padding(3, 2, 3, 2);
            lblPhone2.Name = "lblPhone2";
            lblPhone2.Size = new Size(55, 17);
            lblPhone2.TabIndex = 10;
            lblPhone2.Text = "Telefon 2";
            // 
            // txtPhone1
            // 
            txtPhone1.Location = new Point(24, 171);
            txtPhone1.Margin = new Padding(3, 2, 3, 2);
            txtPhone1.Name = "txtPhone1";
            txtPhone1.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtPhone1.Properties.Appearance.Options.UseFont = true;
            txtPhone1.Properties.NullText = "0 (5xx) xxx xx xx";
            txtPhone1.Properties.Padding = new Padding(10, 5, 10, 5);
            txtPhone1.Size = new Size(326, 36);
            txtPhone1.TabIndex = 4;
            // 
            // lblPhone1
            // 
            lblPhone1.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblPhone1.Appearance.Options.UseFont = true;
            lblPhone1.Location = new Point(24, 153);
            lblPhone1.Margin = new Padding(3, 2, 3, 2);
            lblPhone1.Name = "lblPhone1";
            lblPhone1.Size = new Size(63, 17);
            lblPhone1.TabIndex = 8;
            lblPhone1.Text = "Telefon 1 *";
            // 
            // txtLastName
            // 
            txtLastName.Location = new Point(370, 109);
            txtLastName.Margin = new Padding(3, 2, 3, 2);
            txtLastName.Name = "txtLastName";
            txtLastName.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtLastName.Properties.Appearance.Options.UseFont = true;
            txtLastName.Properties.NullText = "Soyad";
            txtLastName.Properties.Padding = new Padding(10, 5, 10, 5);
            txtLastName.Size = new Size(326, 36);
            txtLastName.TabIndex = 2;
            // 
            // lblLastName
            // 
            lblLastName.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblLastName.Appearance.Options.UseFont = true;
            lblLastName.Location = new Point(370, 91);
            lblLastName.Margin = new Padding(3, 2, 3, 2);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(50, 17);
            lblLastName.TabIndex = 6;
            lblLastName.Text = "Soyadı *";
            // 
            // txtFirstName
            // 
            txtFirstName.Location = new Point(24, 109);
            txtFirstName.Margin = new Padding(3, 2, 3, 2);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtFirstName.Properties.Appearance.Options.UseFont = true;
            txtFirstName.Properties.NullText = "Ad";
            txtFirstName.Properties.Padding = new Padding(10, 5, 10, 5);
            txtFirstName.Size = new Size(326, 36);
            txtFirstName.TabIndex = 1;
            // 
            // lblFirstName
            // 
            lblFirstName.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblFirstName.Appearance.Options.UseFont = true;
            lblFirstName.Location = new Point(24, 91);
            lblFirstName.Margin = new Padding(3, 2, 3, 2);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.Size = new Size(30, 17);
            lblFirstName.TabIndex = 4;
            lblFirstName.Text = "Adı *";
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(370, 34);
            txtTitle.Margin = new Padding(3, 2, 3, 2);
            txtTitle.Name = "txtTitle";
            txtTitle.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtTitle.Properties.Appearance.Options.UseFont = true;
            txtTitle.Properties.NullText = "Rapor imza satırına basılır";
            txtTitle.Properties.Padding = new Padding(10, 5, 10, 5);
            txtTitle.Size = new Size(326, 36);
            txtTitle.TabIndex = 3;
            // 
            // lblTitleField
            // 
            lblTitleField.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblTitleField.Appearance.Options.UseFont = true;
            lblTitleField.Location = new Point(370, 16);
            lblTitleField.Margin = new Padding(3, 2, 3, 2);
            lblTitleField.Name = "lblTitleField";
            lblTitleField.Size = new Size(52, 17);
            lblTitleField.TabIndex = 2;
            lblTitleField.Text = "Ünvanı *";
            // 
            // txtIdentityNumber
            // 
            txtIdentityNumber.Location = new Point(24, 34);
            txtIdentityNumber.Margin = new Padding(3, 2, 3, 2);
            txtIdentityNumber.Name = "txtIdentityNumber";
            txtIdentityNumber.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtIdentityNumber.Properties.Appearance.Options.UseFont = true;
            txtIdentityNumber.Properties.NullText = "11 haneli, yalnızca rakam";
            txtIdentityNumber.Properties.Padding = new Padding(10, 5, 10, 5);
            txtIdentityNumber.Size = new Size(326, 36);
            txtIdentityNumber.TabIndex = 0;
            // 
            // lblIdentityNumber
            // 
            lblIdentityNumber.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblIdentityNumber.Appearance.Options.UseFont = true;
            lblIdentityNumber.Location = new Point(24, 16);
            lblIdentityNumber.Margin = new Padding(3, 2, 3, 2);
            lblIdentityNumber.Name = "lblIdentityNumber";
            lblIdentityNumber.Size = new Size(87, 17);
            lblIdentityNumber.TabIndex = 0;
            lblIdentityNumber.Text = "TC Kimlik No *";
            // 
            // tabDuties
            // 
            tabDuties.Controls.Add(gridDuties);
            tabDuties.Controls.Add(pnlDutyBar);
            tabDuties.Margin = new Padding(3, 2, 3, 2);
            tabDuties.Name = "tabDuties";
            tabDuties.Size = new Size(718, 428);
            tabDuties.Text = "Yetkili Görevler";
            // 
            // gridDuties
            // 
            gridDuties.Dock = DockStyle.Fill;
            gridDuties.EmbeddedNavigator.Margin = new Padding(3, 2, 3, 2);
            gridDuties.Location = new Point(0, 39);
            gridDuties.MainView = viewDuties;
            gridDuties.Margin = new Padding(3, 2, 3, 2);
            gridDuties.Name = "gridDuties";
            gridDuties.Size = new Size(718, 389);
            gridDuties.TabIndex = 1;
            gridDuties.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewDuties });
            // 
            // viewDuties
            // 
            viewDuties.DetailHeight = 284;
            viewDuties.GridControl = gridDuties;
            viewDuties.Name = "viewDuties";
            viewDuties.OptionsBehavior.AutoPopulateColumns = false;
            viewDuties.OptionsEditForm.PopupEditFormWidth = 686;
            viewDuties.OptionsView.EnableAppearanceEvenRow = true;
            viewDuties.OptionsView.EnableAppearanceOddRow = true;
            viewDuties.OptionsView.ShowGroupPanel = false;
            // 
            // pnlDutyBar
            // 
            pnlDutyBar.Appearance.BackColor = Color.Transparent;
            pnlDutyBar.Appearance.Options.UseBackColor = true;
            pnlDutyBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlDutyBar.Controls.Add(lblDutyNote);
            pnlDutyBar.Controls.Add(btnNewSigningRole);
            pnlDutyBar.Controls.Add(btnRemoveDuty);
            pnlDutyBar.Controls.Add(btnAddDuty);
            pnlDutyBar.Dock = DockStyle.Top;
            pnlDutyBar.Location = new Point(0, 0);
            pnlDutyBar.Margin = new Padding(3, 2, 3, 2);
            pnlDutyBar.Name = "pnlDutyBar";
            pnlDutyBar.Size = new Size(718, 39);
            pnlDutyBar.TabIndex = 0;
            // 
            // lblDutyNote
            // 
            lblDutyNote.Appearance.Font = new Font("Segoe UI", 9F);
            lblDutyNote.Appearance.Options.UseFont = true;
            lblDutyNote.Appearance.Options.UseTextOptions = true;
            lblDutyNote.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblDutyNote.Location = new Point(435, 13);
            lblDutyNote.Margin = new Padding(3, 2, 3, 2);
            lblDutyNote.Name = "lblDutyNote";
            lblDutyNote.Size = new Size(208, 15);
            lblDutyNote.TabIndex = 3;
            lblDutyNote.Text = "Henüz görev yok. En az bir tane ekleyin.";
            // 
            // btnNewSigningRole
            // 
            btnNewSigningRole.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnNewSigningRole.Appearance.Options.UseFont = true;
            btnNewSigningRole.Cursor = Cursors.Hand;
            btnNewSigningRole.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnNewSigningRole.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnNewSigningRole.ImageOptions.SvgImage");
            btnNewSigningRole.ImageOptions.SvgImageSize = new Size(16, 16);
            btnNewSigningRole.Location = new Point(250, 8);
            btnNewSigningRole.Margin = new Padding(3, 2, 3, 2);
            btnNewSigningRole.Name = "btnNewSigningRole";
            btnNewSigningRole.Size = new Size(171, 23);
            btnNewSigningRole.TabIndex = 2;
            btnNewSigningRole.Text = "Yeni Görev Tanımla";
            // 
            // btnRemoveDuty
            // 
            btnRemoveDuty.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnRemoveDuty.Appearance.Options.UseFont = true;
            btnRemoveDuty.Cursor = Cursors.Hand;
            btnRemoveDuty.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnRemoveDuty.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnRemoveDuty.ImageOptions.SvgImage");
            btnRemoveDuty.ImageOptions.SvgImageSize = new Size(16, 16);
            btnRemoveDuty.Location = new Point(132, 8);
            btnRemoveDuty.Margin = new Padding(3, 2, 3, 2);
            btnRemoveDuty.Name = "btnRemoveDuty";
            btnRemoveDuty.Size = new Size(111, 23);
            btnRemoveDuty.TabIndex = 1;
            btnRemoveDuty.Text = "Görevi Sil";
            // 
            // btnAddDuty
            // 
            btnAddDuty.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnAddDuty.Appearance.Options.UseFont = true;
            btnAddDuty.Cursor = Cursors.Hand;
            btnAddDuty.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnAddDuty.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnAddDuty.ImageOptions.SvgImage");
            btnAddDuty.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddDuty.Location = new Point(14, 8);
            btnAddDuty.Margin = new Padding(3, 2, 3, 2);
            btnAddDuty.Name = "btnAddDuty";
            btnAddDuty.Size = new Size(111, 23);
            btnAddDuty.TabIndex = 0;
            btnAddDuty.Text = "Görev Ekle";
            // 
            // tabPhotos
            // 
            tabPhotos.Controls.Add(btnRemovePhoto);
            tabPhotos.Controls.Add(btnAddPhoto);
            tabPhotos.Controls.Add(picPhoto);
            tabPhotos.Margin = new Padding(3, 2, 3, 2);
            tabPhotos.Name = "tabPhotos";
            tabPhotos.Size = new Size(718, 428);
            tabPhotos.Text = "Fotoğraf";
            // 
            // btnRemovePhoto
            // 
            btnRemovePhoto.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnRemovePhoto.Appearance.Options.UseFont = true;
            btnRemovePhoto.Cursor = Cursors.Hand;
            btnRemovePhoto.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnRemovePhoto.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnRemovePhoto.ImageOptions.SvgImage");
            btnRemovePhoto.ImageOptions.SvgImageSize = new Size(16, 16);
            btnRemovePhoto.Location = new Point(187, 335);
            btnRemovePhoto.Margin = new Padding(3, 2, 3, 2);
            btnRemovePhoto.Name = "btnRemovePhoto";
            btnRemovePhoto.Size = new Size(154, 26);
            btnRemovePhoto.TabIndex = 2;
            btnRemovePhoto.Text = "Fotoğrafı Kaldır";
            // 
            // btnAddPhoto
            // 
            btnAddPhoto.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnAddPhoto.Appearance.Options.UseFont = true;
            btnAddPhoto.Cursor = Cursors.Hand;
            btnAddPhoto.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnAddPhoto.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnAddPhoto.ImageOptions.SvgImage");
            btnAddPhoto.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddPhoto.Location = new Point(24, 335);
            btnAddPhoto.Margin = new Padding(3, 2, 3, 2);
            btnAddPhoto.Name = "btnAddPhoto";
            btnAddPhoto.Size = new Size(154, 26);
            btnAddPhoto.TabIndex = 1;
            btnAddPhoto.Text = "Fotoğraf Ekle";
            // 
            // picPhoto
            // 
            picPhoto.Location = new Point(24, 20);
            picPhoto.Margin = new Padding(3, 2, 3, 2);
            picPhoto.Name = "picPhoto";
            picPhoto.Properties.Appearance.Options.UseBackColor = true;
            picPhoto.Properties.ShowMenu = false;
            picPhoto.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            picPhoto.Size = new Size(672, 302);
            picPhoto.TabIndex = 0;
            // 
            // pnlFooter
            // 
            pnlFooter.Appearance.Options.UseBackColor = true;
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 505);
            pnlFooter.Margin = new Padding(3, 2, 3, 2);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(720, 47);
            pnlFooter.TabIndex = 2;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Appearance.Font = new Font("Segoe UI Semibold", 10F);
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Cursor = Cursors.Hand;
            btnSave.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnSave.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnSave.ImageOptions.SvgImage");
            btnSave.ImageOptions.SvgImageSize = new Size(18, 18);
            btnSave.Location = new Point(605, 9);
            btnSave.Margin = new Padding(3, 2, 3, 2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(94, 28);
            btnSave.TabIndex = 0;
            btnSave.Text = "Kaydet";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Appearance.Font = new Font("Segoe UI", 10F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnCancel.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnCancel.ImageOptions.SvgImage");
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.Location = new Point(506, 9);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(91, 28);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Vazgeç";
            // 
            // pnlFooterLine
            // 
            pnlFooterLine.Appearance.Options.UseBackColor = true;
            pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooterLine.Dock = DockStyle.Top;
            pnlFooterLine.Location = new Point(0, 0);
            pnlFooterLine.Margin = new Padding(3, 2, 3, 2);
            pnlFooterLine.Name = "pnlFooterLine";
            pnlFooterLine.Size = new Size(720, 1);
            pnlFooterLine.TabIndex = 0;
            // 
            // EmployeeEditForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(720, 552);
            Controls.Add(tabMain);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            IconOptions.ShowIcon = false;
            Margin = new Padding(3, 2, 3, 2);
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
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRegistryNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone2.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtLastName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtFirstName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtTitle.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtIdentityNumber.Properties).EndInit();
            tabDuties.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridDuties).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewDuties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlDutyBar).EndInit();
            pnlDutyBar.ResumeLayout(false);
            pnlDutyBar.PerformLayout();
            tabPhotos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPhoto.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }
    }
}
