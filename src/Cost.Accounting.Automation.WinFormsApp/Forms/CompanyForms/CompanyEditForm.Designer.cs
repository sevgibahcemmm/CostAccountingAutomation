using Cost.Accounting.Automation.WinFormsApp.Utils;
namespace Cost.Accounting.Automation.WinFormsApp.Forms.CompanyForms
{
    partial class CompanyEditForm
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
        private DevExpress.XtraTab.XtraTabPage tabBasic;
        private DevExpress.XtraTab.XtraTabPage tabInvoice;
        private DevExpress.XtraTab.XtraTabPage tabAddress;
        private DevExpress.XtraTab.XtraTabPage tabUnits;

        // Sekme 1 - Temel Bilgiler
        private DevExpress.XtraEditors.LabelControl lblName;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.LabelControl lblTaxOffice;
        private DevExpress.XtraEditors.TextEdit txtTaxOffice;
        private DevExpress.XtraEditors.LabelControl lblTaxNumber;
        private DevExpress.XtraEditors.TextEdit txtTaxNumber;
        private DevExpress.XtraEditors.LabelControl lblPrefix;
        private DevExpress.XtraEditors.TextEdit txtPrefix;
        private DevExpress.XtraEditors.CheckEdit chkActive;
        private DevExpress.XtraEditors.LabelControl lblDescription;
        private DevExpress.XtraEditors.MemoEdit memoDescription;

        // Sekme 2 - Fatura ve Antet
        private DevExpress.XtraEditors.LabelControl lblInvoice;
        private DevExpress.XtraEditors.MemoEdit memoInvoice;
        private DevExpress.XtraEditors.LabelControl lblLetterhead;
        private DevExpress.XtraEditors.MemoEdit memoLetterhead;

        // Sekme 3 - Adres ve İletişim
        private DevExpress.XtraEditors.LabelControl lblCity;
        private DevExpress.XtraEditors.TextEdit txtCity;
        private DevExpress.XtraEditors.LabelControl lblDistrict;
        private DevExpress.XtraEditors.TextEdit txtDistrict;
        private DevExpress.XtraEditors.LabelControl lblPhone1;
        private DevExpress.XtraEditors.TextEdit txtPhone1;
        private DevExpress.XtraEditors.LabelControl lblPhone2;
        private DevExpress.XtraEditors.TextEdit txtPhone2;
        private DevExpress.XtraEditors.LabelControl lblEmail;
        private DevExpress.XtraEditors.TextEdit txtEmail;
        private DevExpress.XtraEditors.LabelControl lblFullAddress;
        private DevExpress.XtraEditors.MemoEdit memoAddress;

        // Sekme 4 - Birimler
        private DevExpress.XtraEditors.LabelControl lblExpName;
        private DevExpress.XtraEditors.TextEdit txtExpName;
        private DevExpress.XtraEditors.LabelControl lblExpCode;
        private DevExpress.XtraEditors.TextEdit txtExpCode;
        private DevExpress.XtraEditors.LabelControl lblAccName;
        private DevExpress.XtraEditors.TextEdit txtAccName;
        private DevExpress.XtraEditors.LabelControl lblAccCode;
        private DevExpress.XtraEditors.TextEdit txtAccCode;

        // Alan simgeleri
        private DevExpress.XtraEditors.LabelControl lblIconName;
        private DevExpress.XtraEditors.LabelControl lblIconTaxOffice;
        private DevExpress.XtraEditors.LabelControl lblIconTaxNumber;
        private DevExpress.XtraEditors.LabelControl lblIconPrefix;
        private DevExpress.XtraEditors.LabelControl lblIconCity;
        private DevExpress.XtraEditors.LabelControl lblIconPhone1;
        private DevExpress.XtraEditors.LabelControl lblIconPhone2;
        private DevExpress.XtraEditors.LabelControl lblIconEmail;
        private DevExpress.XtraEditors.LabelControl lblIconExpName;
        private DevExpress.XtraEditors.LabelControl lblIconExpCode;
        private DevExpress.XtraEditors.LabelControl lblIconAccName;
        private DevExpress.XtraEditors.LabelControl lblIconAccCode;

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
            tabBasic = new DevExpress.XtraTab.XtraTabPage();
            tabInvoice = new DevExpress.XtraTab.XtraTabPage();
            tabAddress = new DevExpress.XtraTab.XtraTabPage();
            tabUnits = new DevExpress.XtraTab.XtraTabPage();

            lblName = new DevExpress.XtraEditors.LabelControl();
            txtName = new DevExpress.XtraEditors.TextEdit();
            lblTaxOffice = new DevExpress.XtraEditors.LabelControl();
            txtTaxOffice = new DevExpress.XtraEditors.TextEdit();
            lblTaxNumber = new DevExpress.XtraEditors.LabelControl();
            txtTaxNumber = new DevExpress.XtraEditors.TextEdit();
            lblPrefix = new DevExpress.XtraEditors.LabelControl();
            txtPrefix = new DevExpress.XtraEditors.TextEdit();
            chkActive = new DevExpress.XtraEditors.CheckEdit();
            lblDescription = new DevExpress.XtraEditors.LabelControl();
            memoDescription = new DevExpress.XtraEditors.MemoEdit();

            lblInvoice = new DevExpress.XtraEditors.LabelControl();
            memoInvoice = new DevExpress.XtraEditors.MemoEdit();
            lblLetterhead = new DevExpress.XtraEditors.LabelControl();
            memoLetterhead = new DevExpress.XtraEditors.MemoEdit();

            lblCity = new DevExpress.XtraEditors.LabelControl();
            txtCity = new DevExpress.XtraEditors.TextEdit();
            lblDistrict = new DevExpress.XtraEditors.LabelControl();
            txtDistrict = new DevExpress.XtraEditors.TextEdit();
            lblPhone1 = new DevExpress.XtraEditors.LabelControl();
            txtPhone1 = new DevExpress.XtraEditors.TextEdit();
            lblPhone2 = new DevExpress.XtraEditors.LabelControl();
            txtPhone2 = new DevExpress.XtraEditors.TextEdit();
            lblEmail = new DevExpress.XtraEditors.LabelControl();
            txtEmail = new DevExpress.XtraEditors.TextEdit();
            lblFullAddress = new DevExpress.XtraEditors.LabelControl();
            memoAddress = new DevExpress.XtraEditors.MemoEdit();

            lblExpName = new DevExpress.XtraEditors.LabelControl();
            txtExpName = new DevExpress.XtraEditors.TextEdit();
            lblExpCode = new DevExpress.XtraEditors.LabelControl();
            txtExpCode = new DevExpress.XtraEditors.TextEdit();
            lblAccName = new DevExpress.XtraEditors.LabelControl();
            txtAccName = new DevExpress.XtraEditors.TextEdit();
            lblAccCode = new DevExpress.XtraEditors.LabelControl();
            txtAccCode = new DevExpress.XtraEditors.TextEdit();

            lblIconName = new DevExpress.XtraEditors.LabelControl();
            lblIconTaxOffice = new DevExpress.XtraEditors.LabelControl();
            lblIconTaxNumber = new DevExpress.XtraEditors.LabelControl();
            lblIconPrefix = new DevExpress.XtraEditors.LabelControl();
            lblIconCity = new DevExpress.XtraEditors.LabelControl();
            lblIconPhone1 = new DevExpress.XtraEditors.LabelControl();
            lblIconPhone2 = new DevExpress.XtraEditors.LabelControl();
            lblIconEmail = new DevExpress.XtraEditors.LabelControl();
            lblIconExpName = new DevExpress.XtraEditors.LabelControl();
            lblIconExpCode = new DevExpress.XtraEditors.LabelControl();
            lblIconAccName = new DevExpress.XtraEditors.LabelControl();
            lblIconAccCode = new DevExpress.XtraEditors.LabelControl();

            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tabMain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtTaxOffice.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtTaxNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPrefix.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoDescription.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoInvoice.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoLetterhead.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCity.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtDistrict.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone2.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoAddress.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtExpName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtExpCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtAccName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtAccCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();
            pnlHeader.SuspendLayout();
            tabMain.SuspendLayout();
            tabBasic.SuspendLayout();
            tabInvoice.SuspendLayout();
            tabAddress.SuspendLayout();
            tabUnits.SuspendLayout();
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
            lblHeaderIcon.ImageOptions.SvgImage = DxIcon.Company;
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
            lblTitle.Text = "�?irket Bilgileri";
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
            lblSubtitle.Size = new Size(220, 14);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "�?irket bilgilerini eksiksiz doldurun";
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
            tabMain.SelectedTabPage = tabBasic;
            tabMain.Size = new Size(480, 328);
            tabMain.TabIndex = 1;
            tabMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabBasic, tabInvoice, tabAddress, tabUnits });
            //
            // tabBasic
            //
            tabBasic.Controls.Add(lblIconPrefix);
            tabBasic.Controls.Add(lblIconTaxNumber);
            tabBasic.Controls.Add(lblIconTaxOffice);
            tabBasic.Controls.Add(lblIconName);
            tabBasic.Controls.Add(memoDescription);
            tabBasic.Controls.Add(lblDescription);
            tabBasic.Controls.Add(chkActive);
            tabBasic.Controls.Add(txtPrefix);
            tabBasic.Controls.Add(lblPrefix);
            tabBasic.Controls.Add(txtTaxNumber);
            tabBasic.Controls.Add(lblTaxNumber);
            tabBasic.Controls.Add(txtTaxOffice);
            tabBasic.Controls.Add(lblTaxOffice);
            tabBasic.Controls.Add(txtName);
            tabBasic.Controls.Add(lblName);
            tabBasic.ImageOptions.SvgImage = DxIcon.Company;
            tabBasic.ImageOptions.SvgImageSize = new Size(16, 16);
            tabBasic.Name = "tabBasic";
            tabBasic.Size = new Size(474, 297);
            tabBasic.Text = "Temel Bilgiler";
            //
            // lblName
            //
            lblName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblName.Appearance.Options.UseFont = true;
            lblName.Appearance.Options.UseForeColor = true;
            lblName.Location = new Point(20, 22);
            lblName.Margin = new Padding(3, 2, 3, 2);
            lblName.Name = "lblName";
            lblName.Size = new Size(50, 14);
            lblName.TabIndex = 0;
            lblName.Text = "�?irket Adı";
            //
            // txtName
            //
            txtName.Location = new Point(20, 40);
            txtName.Margin = new Padding(3, 2, 3, 2);
            txtName.Name = "txtName";
            txtName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtName.Properties.Appearance.Options.UseFont = true;
            txtName.Properties.MaxLength = 200;
            txtName.Properties.NullText = "Örn. Delta İnşaat A.�?.";
            txtName.Properties.Padding = new Padding(26, 2, 2, 2);
            txtName.Size = new Size(434, 26);
            txtName.TabIndex = 1;
            //
            // lblTaxOffice
            //
            lblTaxOffice.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblTaxOffice.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblTaxOffice.Appearance.Options.UseFont = true;
            lblTaxOffice.Appearance.Options.UseForeColor = true;
            lblTaxOffice.Location = new Point(20, 80);
            lblTaxOffice.Margin = new Padding(3, 2, 3, 2);
            lblTaxOffice.Name = "lblTaxOffice";
            lblTaxOffice.Size = new Size(60, 14);
            lblTaxOffice.TabIndex = 2;
            lblTaxOffice.Text = "Vergi Dairesi";
            //
            // txtTaxOffice
            //
            txtTaxOffice.Location = new Point(20, 98);
            txtTaxOffice.Margin = new Padding(3, 2, 3, 2);
            txtTaxOffice.Name = "txtTaxOffice";
            txtTaxOffice.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtTaxOffice.Properties.Appearance.Options.UseFont = true;
            txtTaxOffice.Properties.NullText = "Örn. Kadıköy VD";
            txtTaxOffice.Properties.Padding = new Padding(26, 2, 2, 2);
            txtTaxOffice.Size = new Size(207, 26);
            txtTaxOffice.TabIndex = 3;
            //
            // lblTaxNumber
            //
            lblTaxNumber.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblTaxNumber.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblTaxNumber.Appearance.Options.UseFont = true;
            lblTaxNumber.Appearance.Options.UseForeColor = true;
            lblTaxNumber.Location = new Point(247, 80);
            lblTaxNumber.Margin = new Padding(3, 2, 3, 2);
            lblTaxNumber.Name = "lblTaxNumber";
            lblTaxNumber.Size = new Size(60, 14);
            lblTaxNumber.TabIndex = 4;
            lblTaxNumber.Text = "Vergi No";
            //
            // txtTaxNumber
            //
            txtTaxNumber.Location = new Point(247, 98);
            txtTaxNumber.Margin = new Padding(3, 2, 3, 2);
            txtTaxNumber.Name = "txtTaxNumber";
            txtTaxNumber.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtTaxNumber.Properties.Appearance.Options.UseFont = true;
            txtTaxNumber.Properties.MaxLength = 11;
            txtTaxNumber.Properties.NullText = "10 veya 11 hane";
            txtTaxNumber.Properties.Padding = new Padding(26, 2, 2, 2);
            txtTaxNumber.Size = new Size(207, 26);
            txtTaxNumber.TabIndex = 5;
            //
            // lblPrefix
            //
            lblPrefix.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPrefix.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblPrefix.Appearance.Options.UseFont = true;
            lblPrefix.Appearance.Options.UseForeColor = true;
            lblPrefix.Location = new Point(20, 138);
            lblPrefix.Margin = new Padding(3, 2, 3, 2);
            lblPrefix.Name = "lblPrefix";
            lblPrefix.Size = new Size(60, 14);
            lblPrefix.TabIndex = 6;
            lblPrefix.Text = "�?irket Ön Eki";
            //
            // txtPrefix
            //
            txtPrefix.Location = new Point(20, 156);
            txtPrefix.Margin = new Padding(3, 2, 3, 2);
            txtPrefix.Name = "txtPrefix";
            txtPrefix.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtPrefix.Properties.Appearance.Options.UseFont = true;
            txtPrefix.Properties.MaxLength = 10;
            txtPrefix.Properties.NullText = "Örn. DELTA";
            txtPrefix.Properties.Padding = new Padding(26, 2, 2, 2);
            txtPrefix.Size = new Size(207, 26);
            txtPrefix.TabIndex = 7;
            //
            // chkActive
            //
            chkActive.Location = new Point(247, 156);
            chkActive.Margin = new Padding(3, 2, 3, 2);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.Caption = "Aktif �?irket";
            chkActive.Size = new Size(160, 21);
            chkActive.TabIndex = 8;
            //
            // lblDescription
            //
            lblDescription.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblDescription.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblDescription.Appearance.Options.UseFont = true;
            lblDescription.Appearance.Options.UseForeColor = true;
            lblDescription.Location = new Point(20, 208);
            lblDescription.Margin = new Padding(3, 2, 3, 2);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(50, 14);
            lblDescription.TabIndex = 9;
            lblDescription.Text = "Açıklama";
            //
            // memoDescription
            //
            memoDescription.Location = new Point(20, 226);
            memoDescription.Margin = new Padding(3, 2, 3, 2);
            memoDescription.Name = "memoDescription";
            memoDescription.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            memoDescription.Properties.Appearance.Options.UseFont = true;
            memoDescription.Properties.NullText = "Kısa açıklama (opsiyonel)";
            memoDescription.Size = new Size(434, 62);
            memoDescription.TabIndex = 10;
            //
            // tabInvoice
            //
            tabInvoice.Controls.Add(memoLetterhead);
            tabInvoice.Controls.Add(lblLetterhead);
            tabInvoice.Controls.Add(memoInvoice);
            tabInvoice.Controls.Add(lblInvoice);
            tabInvoice.ImageOptions.SvgImage = DxIcon.Receipt;
            tabInvoice.ImageOptions.SvgImageSize = new Size(16, 16);
            tabInvoice.Name = "tabInvoice";
            tabInvoice.Size = new Size(474, 297);
            tabInvoice.Text = "Fatura & Antet";
            //
            // lblInvoice
            //
            lblInvoice.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblInvoice.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblInvoice.Appearance.Options.UseFont = true;
            lblInvoice.Appearance.Options.UseForeColor = true;
            lblInvoice.Location = new Point(20, 22);
            lblInvoice.Margin = new Padding(3, 2, 3, 2);
            lblInvoice.Name = "lblInvoice";
            lblInvoice.Size = new Size(72, 14);
            lblInvoice.TabIndex = 0;
            lblInvoice.Text = "Fatura Bilgileri";
            //
            // memoInvoice
            //
            memoInvoice.Location = new Point(20, 40);
            memoInvoice.Margin = new Padding(3, 2, 3, 2);
            memoInvoice.Name = "memoInvoice";
            memoInvoice.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            memoInvoice.Properties.Appearance.Options.UseFont = true;
            memoInvoice.Properties.NullText = "Faturada görünecek bilgiler";
            memoInvoice.Size = new Size(434, 110);
            memoInvoice.TabIndex = 1;
            //
            // lblLetterhead
            //
            lblLetterhead.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblLetterhead.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblLetterhead.Appearance.Options.UseFont = true;
            lblLetterhead.Appearance.Options.UseForeColor = true;
            lblLetterhead.Location = new Point(20, 168);
            lblLetterhead.Margin = new Padding(3, 2, 3, 2);
            lblLetterhead.Name = "lblLetterhead";
            lblLetterhead.Size = new Size(32, 14);
            lblLetterhead.TabIndex = 2;
            lblLetterhead.Text = "Antet";
            //
            // memoLetterhead
            //
            memoLetterhead.Location = new Point(20, 186);
            memoLetterhead.Margin = new Padding(3, 2, 3, 2);
            memoLetterhead.Name = "memoLetterhead";
            memoLetterhead.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            memoLetterhead.Properties.Appearance.Options.UseFont = true;
            memoLetterhead.Properties.NullText = "Antet / logo yazısı bilgileri";
            memoLetterhead.Size = new Size(434, 100);
            memoLetterhead.TabIndex = 3;
            //
            // tabAddress
            //
            tabAddress.Controls.Add(lblIconEmail);
            tabAddress.Controls.Add(lblIconPhone2);
            tabAddress.Controls.Add(lblIconPhone1);
            tabAddress.Controls.Add(lblIconCity);
            tabAddress.Controls.Add(memoAddress);
            tabAddress.Controls.Add(lblFullAddress);
            tabAddress.Controls.Add(txtEmail);
            tabAddress.Controls.Add(lblEmail);
            tabAddress.Controls.Add(txtPhone2);
            tabAddress.Controls.Add(lblPhone2);
            tabAddress.Controls.Add(txtPhone1);
            tabAddress.Controls.Add(lblPhone1);
            tabAddress.Controls.Add(txtDistrict);
            tabAddress.Controls.Add(lblDistrict);
            tabAddress.Controls.Add(txtCity);
            tabAddress.Controls.Add(lblCity);
            tabAddress.ImageOptions.SvgImage = DxIcon.Pin;
            tabAddress.ImageOptions.SvgImageSize = new Size(16, 16);
            tabAddress.Name = "tabAddress";
            tabAddress.Size = new Size(474, 297);
            tabAddress.Text = "Adres & İletişim";
            //
            // lblCity
            //
            lblCity.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblCity.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblCity.Appearance.Options.UseFont = true;
            lblCity.Appearance.Options.UseForeColor = true;
            lblCity.Location = new Point(20, 22);
            lblCity.Margin = new Padding(3, 2, 3, 2);
            lblCity.Name = "lblCity";
            lblCity.Size = new Size(28, 14);
            lblCity.TabIndex = 0;
            lblCity.Text = "�?ehir";
            //
            // txtCity
            //
            txtCity.Location = new Point(20, 40);
            txtCity.Margin = new Padding(3, 2, 3, 2);
            txtCity.Name = "txtCity";
            txtCity.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtCity.Properties.Appearance.Options.UseFont = true;
            txtCity.Properties.NullText = "İstanbul";
            txtCity.Properties.Padding = new Padding(26, 2, 2, 2);
            txtCity.Size = new Size(207, 26);
            txtCity.TabIndex = 1;
            //
            // lblDistrict
            //
            lblDistrict.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblDistrict.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblDistrict.Appearance.Options.UseFont = true;
            lblDistrict.Appearance.Options.UseForeColor = true;
            lblDistrict.Location = new Point(247, 22);
            lblDistrict.Margin = new Padding(3, 2, 3, 2);
            lblDistrict.Name = "lblDistrict";
            lblDistrict.Size = new Size(22, 14);
            lblDistrict.TabIndex = 2;
            lblDistrict.Text = "İlçe";
            //
            // txtDistrict
            //
            txtDistrict.Location = new Point(247, 40);
            txtDistrict.Margin = new Padding(3, 2, 3, 2);
            txtDistrict.Name = "txtDistrict";
            txtDistrict.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtDistrict.Properties.Appearance.Options.UseFont = true;
            txtDistrict.Properties.NullText = "Kadıköy";
            txtDistrict.Size = new Size(207, 26);
            txtDistrict.TabIndex = 3;
            //
            // lblPhone1
            //
            lblPhone1.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPhone1.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblPhone1.Appearance.Options.UseFont = true;
            lblPhone1.Appearance.Options.UseForeColor = true;
            lblPhone1.Location = new Point(20, 80);
            lblPhone1.Margin = new Padding(3, 2, 3, 2);
            lblPhone1.Name = "lblPhone1";
            lblPhone1.Size = new Size(58, 14);
            lblPhone1.TabIndex = 4;
            lblPhone1.Text = "Telefon 1";
            //
            // txtPhone1
            //
            txtPhone1.Location = new Point(20, 98);
            txtPhone1.Margin = new Padding(3, 2, 3, 2);
            txtPhone1.Name = "txtPhone1";
            txtPhone1.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtPhone1.Properties.Appearance.Options.UseFont = true;
            txtPhone1.Properties.NullText = "0(5xx) xxx xx xx";
            txtPhone1.Properties.Padding = new Padding(26, 2, 2, 2);
            txtPhone1.Size = new Size(207, 26);
            txtPhone1.TabIndex = 5;
            //
            // lblPhone2
            //
            lblPhone2.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPhone2.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblPhone2.Appearance.Options.UseFont = true;
            lblPhone2.Appearance.Options.UseForeColor = true;
            lblPhone2.Location = new Point(247, 80);
            lblPhone2.Margin = new Padding(3, 2, 3, 2);
            lblPhone2.Name = "lblPhone2";
            lblPhone2.Size = new Size(58, 14);
            lblPhone2.TabIndex = 6;
            lblPhone2.Text = "Telefon 2";
            //
            // txtPhone2
            //
            txtPhone2.Location = new Point(247, 98);
            txtPhone2.Margin = new Padding(3, 2, 3, 2);
            txtPhone2.Name = "txtPhone2";
            txtPhone2.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtPhone2.Properties.Appearance.Options.UseFont = true;
            txtPhone2.Properties.NullText = "Opsiyonel";
            txtPhone2.Properties.Padding = new Padding(26, 2, 2, 2);
            txtPhone2.Size = new Size(207, 26);
            txtPhone2.TabIndex = 7;
            //
            // lblEmail
            //
            lblEmail.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblEmail.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblEmail.Appearance.Options.UseFont = true;
            lblEmail.Appearance.Options.UseForeColor = true;
            lblEmail.Location = new Point(20, 138);
            lblEmail.Margin = new Padding(3, 2, 3, 2);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(39, 14);
            lblEmail.TabIndex = 8;
            lblEmail.Text = "E-Posta";
            //
            // txtEmail
            //
            txtEmail.Location = new Point(20, 156);
            txtEmail.Margin = new Padding(3, 2, 3, 2);
            txtEmail.Name = "txtEmail";
            txtEmail.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtEmail.Properties.Appearance.Options.UseFont = true;
            txtEmail.Properties.NullText = "info@firma.com";
            txtEmail.Properties.Padding = new Padding(26, 2, 2, 2);
            txtEmail.Size = new Size(434, 26);
            txtEmail.TabIndex = 9;
            //
            // lblFullAddress
            //
            lblFullAddress.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblFullAddress.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblFullAddress.Appearance.Options.UseFont = true;
            lblFullAddress.Appearance.Options.UseForeColor = true;
            lblFullAddress.Location = new Point(20, 196);
            lblFullAddress.Margin = new Padding(3, 2, 3, 2);
            lblFullAddress.Name = "lblFullAddress";
            lblFullAddress.Size = new Size(58, 14);
            lblFullAddress.TabIndex = 10;
            lblFullAddress.Text = "Tam Adres";
            //
            // memoAddress
            //
            memoAddress.Location = new Point(20, 214);
            memoAddress.Margin = new Padding(3, 2, 3, 2);
            memoAddress.Name = "memoAddress";
            memoAddress.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            memoAddress.Properties.Appearance.Options.UseFont = true;
            memoAddress.Properties.NullText = "Mahalle, cadde, no, kat...";
            memoAddress.Size = new Size(434, 72);
            memoAddress.TabIndex = 11;
            //
            // tabUnits
            //
            tabUnits.Controls.Add(lblIconAccCode);
            tabUnits.Controls.Add(lblIconAccName);
            tabUnits.Controls.Add(lblIconExpCode);
            tabUnits.Controls.Add(lblIconExpName);
            tabUnits.Controls.Add(txtAccCode);
            tabUnits.Controls.Add(lblAccCode);
            tabUnits.Controls.Add(txtAccName);
            tabUnits.Controls.Add(lblAccName);
            tabUnits.Controls.Add(txtExpCode);
            tabUnits.Controls.Add(lblExpCode);
            tabUnits.Controls.Add(txtExpName);
            tabUnits.Controls.Add(lblExpName);
            tabUnits.ImageOptions.SvgImage = DxIcon.Tag;
            tabUnits.ImageOptions.SvgImageSize = new Size(16, 16);
            tabUnits.Name = "tabUnits";
            tabUnits.Size = new Size(474, 297);
            tabUnits.Text = "Birimler";
            //
            // lblExpName
            //
            lblExpName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblExpName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblExpName.Appearance.Options.UseFont = true;
            lblExpName.Appearance.Options.UseForeColor = true;
            lblExpName.Location = new Point(20, 22);
            lblExpName.Margin = new Padding(3, 2, 3, 2);
            lblExpName.Name = "lblExpName";
            lblExpName.Size = new Size(94, 14);
            lblExpName.TabIndex = 0;
            lblExpName.Text = "Harcama Birimi Adı";
            //
            // txtExpName
            //
            txtExpName.Location = new Point(20, 40);
            txtExpName.Margin = new Padding(3, 2, 3, 2);
            txtExpName.Name = "txtExpName";
            txtExpName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtExpName.Properties.Appearance.Options.UseFont = true;
            txtExpName.Properties.NullText = "Örn. Genel Yönetim";
            txtExpName.Properties.Padding = new Padding(26, 2, 2, 2);
            txtExpName.Size = new Size(207, 26);
            txtExpName.TabIndex = 1;
            //
            // lblExpCode
            //
            lblExpCode.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblExpCode.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblExpCode.Appearance.Options.UseFont = true;
            lblExpCode.Appearance.Options.UseForeColor = true;
            lblExpCode.Location = new Point(247, 22);
            lblExpCode.Margin = new Padding(3, 2, 3, 2);
            lblExpCode.Name = "lblExpCode";
            lblExpCode.Size = new Size(88, 14);
            lblExpCode.TabIndex = 2;
            lblExpCode.Text = "Harcama Kodu";
            //
            // txtExpCode
            //
            txtExpCode.Location = new Point(247, 40);
            txtExpCode.Margin = new Padding(3, 2, 3, 2);
            txtExpCode.Name = "txtExpCode";
            txtExpCode.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtExpCode.Properties.Appearance.Options.UseFont = true;
            txtExpCode.Properties.NullText = "Örn. 3000";
            txtExpCode.Properties.Padding = new Padding(26, 2, 2, 2);
            txtExpCode.Size = new Size(207, 26);
            txtExpCode.TabIndex = 3;
            //
            // lblAccName
            //
            lblAccName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblAccName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblAccName.Appearance.Options.UseFont = true;
            lblAccName.Appearance.Options.UseForeColor = true;
            lblAccName.Location = new Point(20, 80);
            lblAccName.Margin = new Padding(3, 2, 3, 2);
            lblAccName.Name = "lblAccName";
            lblAccName.Size = new Size(100, 14);
            lblAccName.TabIndex = 4;
            lblAccName.Text = "Muhasebe Birimi Adı";
            //
            // txtAccName
            //
            txtAccName.Location = new Point(20, 98);
            txtAccName.Margin = new Padding(3, 2, 3, 2);
            txtAccName.Name = "txtAccName";
            txtAccName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtAccName.Properties.Appearance.Options.UseFont = true;
            txtAccName.Properties.NullText = "Örn. Mali İşler";
            txtAccName.Properties.Padding = new Padding(26, 2, 2, 2);
            txtAccName.Size = new Size(207, 26);
            txtAccName.TabIndex = 5;
            //
            // lblAccCode
            //
            lblAccCode.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblAccCode.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblAccCode.Appearance.Options.UseFont = true;
            lblAccCode.Appearance.Options.UseForeColor = true;
            lblAccCode.Location = new Point(247, 80);
            lblAccCode.Margin = new Padding(3, 2, 3, 2);
            lblAccCode.Name = "lblAccCode";
            lblAccCode.Size = new Size(86, 14);
            lblAccCode.TabIndex = 6;
            lblAccCode.Text = "Muhasebe Kodu";
            //
            // txtAccCode
            //
            txtAccCode.Location = new Point(247, 98);
            txtAccCode.Margin = new Padding(3, 2, 3, 2);
            txtAccCode.Name = "txtAccCode";
            txtAccCode.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtAccCode.Properties.Appearance.Options.UseFont = true;
            txtAccCode.Properties.NullText = "Örn. 760";
            txtAccCode.Properties.Padding = new Padding(26, 2, 2, 2);
            txtAccCode.Size = new Size(207, 26);
            txtAccCode.TabIndex = 7;
            //
            // lblIconName
            //
            lblIconName.Cursor = Cursors.Default;
            lblIconName.Location = new Point(24, 44);
            lblIconName.Name = "lblIconName";
            lblIconName.Size = new Size(18, 18);
            lblIconName.ImageOptions.SvgImage = DxIcon.Company;
            lblIconName.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconName.Tag = txtName;
            lblIconName.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconTaxOffice
            //
            lblIconTaxOffice.Cursor = Cursors.Default;
            lblIconTaxOffice.Location = new Point(24, 102);
            lblIconTaxOffice.Name = "lblIconTaxOffice";
            lblIconTaxOffice.Size = new Size(18, 18);
            lblIconTaxOffice.ImageOptions.SvgImage = DxIcon.IdCard;
            lblIconTaxOffice.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconTaxOffice.Tag = txtTaxOffice;
            lblIconTaxOffice.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconTaxNumber
            //
            lblIconTaxNumber.Cursor = Cursors.Default;
            lblIconTaxNumber.Location = new Point(251, 102);
            lblIconTaxNumber.Name = "lblIconTaxNumber";
            lblIconTaxNumber.Size = new Size(18, 18);
            lblIconTaxNumber.ImageOptions.SvgImage = DxIcon.Barcode;
            lblIconTaxNumber.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconTaxNumber.Tag = txtTaxNumber;
            lblIconTaxNumber.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconPrefix
            //
            lblIconPrefix.Cursor = Cursors.Default;
            lblIconPrefix.Location = new Point(24, 160);
            lblIconPrefix.Name = "lblIconPrefix";
            lblIconPrefix.Size = new Size(18, 18);
            lblIconPrefix.ImageOptions.SvgImage = DxIcon.Tag;
            lblIconPrefix.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconPrefix.Tag = txtPrefix;
            lblIconPrefix.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconCity
            //
            lblIconCity.Cursor = Cursors.Default;
            lblIconCity.Location = new Point(24, 44);
            lblIconCity.Name = "lblIconCity";
            lblIconCity.Size = new Size(18, 18);
            lblIconCity.ImageOptions.SvgImage = DxIcon.Pin;
            lblIconCity.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconCity.Tag = txtCity;
            lblIconCity.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconPhone1
            //
            lblIconPhone1.Cursor = Cursors.Default;
            lblIconPhone1.Location = new Point(24, 102);
            lblIconPhone1.Name = "lblIconPhone1";
            lblIconPhone1.Size = new Size(18, 18);
            lblIconPhone1.ImageOptions.SvgImage = DxIcon.Phone;
            lblIconPhone1.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconPhone1.Tag = txtPhone1;
            lblIconPhone1.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconPhone2
            //
            lblIconPhone2.Cursor = Cursors.Default;
            lblIconPhone2.Location = new Point(251, 102);
            lblIconPhone2.Name = "lblIconPhone2";
            lblIconPhone2.Size = new Size(18, 18);
            lblIconPhone2.ImageOptions.SvgImage = DxIcon.Phone;
            lblIconPhone2.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconPhone2.Tag = txtPhone2;
            lblIconPhone2.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconEmail
            //
            lblIconEmail.Cursor = Cursors.Default;
            lblIconEmail.Location = new Point(24, 160);
            lblIconEmail.Name = "lblIconEmail";
            lblIconEmail.Size = new Size(18, 18);
            lblIconEmail.ImageOptions.SvgImage = DxIcon.At;
            lblIconEmail.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconEmail.Tag = txtEmail;
            lblIconEmail.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconExpName
            //
            lblIconExpName.Cursor = Cursors.Default;
            lblIconExpName.Location = new Point(24, 44);
            lblIconExpName.Name = "lblIconExpName";
            lblIconExpName.Size = new Size(18, 18);
            lblIconExpName.ImageOptions.SvgImage = DxIcon.Company;
            lblIconExpName.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconExpName.Tag = txtExpName;
            lblIconExpName.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconExpCode
            //
            lblIconExpCode.Cursor = Cursors.Default;
            lblIconExpCode.Location = new Point(251, 44);
            lblIconExpCode.Name = "lblIconExpCode";
            lblIconExpCode.Size = new Size(18, 18);
            lblIconExpCode.ImageOptions.SvgImage = DxIcon.Tag;
            lblIconExpCode.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconExpCode.Tag = txtExpCode;
            lblIconExpCode.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconAccName
            //
            lblIconAccName.Cursor = Cursors.Default;
            lblIconAccName.Location = new Point(24, 102);
            lblIconAccName.Name = "lblIconAccName";
            lblIconAccName.Size = new Size(18, 18);
            lblIconAccName.ImageOptions.SvgImage = DxIcon.Company;
            lblIconAccName.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconAccName.Tag = txtAccName;
            lblIconAccName.MouseDown += FieldIcon_MouseDown;
            //
            // lblIconAccCode
            //
            lblIconAccCode.Cursor = Cursors.Default;
            lblIconAccCode.Location = new Point(251, 102);
            lblIconAccCode.Name = "lblIconAccCode";
            lblIconAccCode.Size = new Size(18, 18);
            lblIconAccCode.ImageOptions.SvgImage = DxIcon.Tag;
            lblIconAccCode.ImageOptions.SvgImageSize = new Size(18, 18);
            lblIconAccCode.Tag = txtAccCode;
            lblIconAccCode.MouseDown += FieldIcon_MouseDown;
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
            // CompanyEditForm
            //
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 446);
            Controls.Add(tabMain);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CompanyEditForm";
            StartPosition = FormStartPosition.CenterParent;
            IconOptions.SvgImage = DxIcon.Company;
            Text = "�?irket";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            tabBasic.ResumeLayout(false);
            tabBasic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtTaxOffice.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtTaxNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPrefix.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoDescription.Properties).EndInit();
            tabInvoice.ResumeLayout(false);
            tabInvoice.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)memoInvoice.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoLetterhead.Properties).EndInit();
            tabAddress.ResumeLayout(false);
            tabAddress.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtCity.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtDistrict.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone2.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoAddress.Properties).EndInit();
            tabUnits.ResumeLayout(false);
            tabUnits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtExpName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtExpCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtAccName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtAccCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabMain).EndInit();
            tabMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }
    }
}