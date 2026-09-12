namespace Cost.Accounting.Automation.WinFormsApp.Forms.SupplierForms
{
    partial class SupplierEditForm
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
        private DevExpress.XtraTab.XtraTabPage tabContact;

        // Sekme 1 - Temel Bilgiler
        private DevExpress.XtraEditors.LabelControl lblName;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.LabelControl lblTaxOffice;
        private DevExpress.XtraEditors.TextEdit txtTaxOffice;
        private DevExpress.XtraEditors.LabelControl lblTaxNumber;
        private DevExpress.XtraEditors.TextEdit txtTaxNumber;
        private DevExpress.XtraEditors.CheckEdit chkActive;
        private DevExpress.XtraEditors.LabelControl lblDescription;
        private DevExpress.XtraEditors.MemoEdit memoDescription;

        // Sekme 2 - Adres ve İletişim
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
            tabContact = new DevExpress.XtraTab.XtraTabPage();

            lblName = new DevExpress.XtraEditors.LabelControl();
            txtName = new DevExpress.XtraEditors.TextEdit();
            lblTaxOffice = new DevExpress.XtraEditors.LabelControl();
            txtTaxOffice = new DevExpress.XtraEditors.TextEdit();
            lblTaxNumber = new DevExpress.XtraEditors.LabelControl();
            txtTaxNumber = new DevExpress.XtraEditors.TextEdit();
            chkActive = new DevExpress.XtraEditors.CheckEdit();
            lblDescription = new DevExpress.XtraEditors.LabelControl();
            memoDescription = new DevExpress.XtraEditors.MemoEdit();

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
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoDescription.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCity.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtDistrict.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone2.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoAddress.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();
            pnlHeader.SuspendLayout();
            tabMain.SuspendLayout();
            tabBasic.SuspendLayout();
            tabContact.SuspendLayout();
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
            lblTitle.Text = "Tedarikçi Bilgileri";
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
            lblSubtitle.Size = new Size(230, 14);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "Tedarikçi bilgilerini eksiksiz doldurun";
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
            tabMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabBasic, tabContact });
            //
            // tabBasic
            //
            tabBasic.Controls.Add(memoDescription);
            tabBasic.Controls.Add(lblDescription);
            tabBasic.Controls.Add(chkActive);
            tabBasic.Controls.Add(txtTaxNumber);
            tabBasic.Controls.Add(lblTaxNumber);
            tabBasic.Controls.Add(txtTaxOffice);
            tabBasic.Controls.Add(lblTaxOffice);
            tabBasic.Controls.Add(txtName);
            tabBasic.Controls.Add(lblName);
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
            lblName.Size = new Size(54, 14);
            lblName.TabIndex = 0;
            lblName.Text = "Firma / Kişi Adı";
            //
            // txtName
            //
            txtName.Location = new Point(20, 40);
            txtName.Margin = new Padding(3, 2, 3, 2);
            txtName.Name = "txtName";
            txtName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtName.Properties.Appearance.Options.UseFont = true;
            txtName.Properties.MaxLength = 200;
            txtName.Properties.NullText = "Örn. Anadolu Tedarik A.Ş.";
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
            txtTaxOffice.Properties.NullText = "Opsiyonel";
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
            txtTaxNumber.Properties.NullText = "Opsiyonel";
            txtTaxNumber.Size = new Size(207, 26);
            txtTaxNumber.TabIndex = 5;
            //
            // chkActive
            //
            chkActive.Location = new Point(20, 156);
            chkActive.Margin = new Padding(3, 2, 3, 2);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.Caption = "Aktif Tedarikçi";
            chkActive.Size = new Size(160, 21);
            chkActive.TabIndex = 6;
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
            lblDescription.TabIndex = 7;
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
            memoDescription.TabIndex = 8;
            //
            // tabContact
            //
            tabContact.Controls.Add(memoAddress);
            tabContact.Controls.Add(lblFullAddress);
            tabContact.Controls.Add(txtEmail);
            tabContact.Controls.Add(lblEmail);
            tabContact.Controls.Add(txtPhone2);
            tabContact.Controls.Add(lblPhone2);
            tabContact.Controls.Add(txtPhone1);
            tabContact.Controls.Add(lblPhone1);
            tabContact.Controls.Add(txtDistrict);
            tabContact.Controls.Add(lblDistrict);
            tabContact.Controls.Add(txtCity);
            tabContact.Controls.Add(lblCity);
            tabContact.Name = "tabContact";
            tabContact.Size = new Size(474, 297);
            tabContact.Text = "Adres & İletişim";
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
            lblCity.Text = "Şehir";
            //
            // txtCity
            //
            txtCity.Location = new Point(20, 40);
            txtCity.Margin = new Padding(3, 2, 3, 2);
            txtCity.Name = "txtCity";
            txtCity.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtCity.Properties.Appearance.Options.UseFont = true;
            txtCity.Properties.NullText = "Ankara";
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
            txtDistrict.Properties.NullText = "Çankaya";
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
            //
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
            btnSave.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSave.Appearance.ForeColor = Color.White;
            btnSave.Appearance.Options.UseBackColor = true;
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Appearance.Options.UseForeColor = true;
            btnSave.Location = new Point(312, 13);
            btnSave.Margin = new Padding(3, 2, 3, 2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(148, 32);
            btnSave.TabIndex = 2;
            btnSave.Text = "Kaydet";
            //
            // SupplierEditForm
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
            Name = "SupplierEditForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Tedarikçi";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            tabBasic.ResumeLayout(false);
            tabBasic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtTaxOffice.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtTaxNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoDescription.Properties).EndInit();
            tabContact.ResumeLayout(false);
            tabContact.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtCity.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtDistrict.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPhone2.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtEmail.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoAddress.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabMain).EndInit();
            tabMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }
    }
}