namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public partial class ProductEditForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;

        private DevExpress.XtraTab.XtraTabControl tabMain;
        private DevExpress.XtraTab.XtraTabPage tabBasic;
        private DevExpress.XtraTab.XtraTabPage tabPrices;
        private DevExpress.XtraTab.XtraTabPage tabMovements;
        private DevExpress.XtraTab.XtraTabPage tabImages;

        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.LabelControl lblName;
        private DevExpress.XtraEditors.TextEdit txtProductCode;
        private DevExpress.XtraEditors.LabelControl lblCode;
        private DevExpress.XtraEditors.LabelControl lblBarcode;
        private DevExpress.XtraEditors.PictureEdit picBarcode;
        private DevExpress.XtraEditors.LabelControl lblQR;
        private DevExpress.XtraEditors.PictureEdit picQR;
        private DevExpress.XtraEditors.SpinEdit spinTaxRate;
        private DevExpress.XtraEditors.LabelControl lblTax;
        private DevExpress.XtraEditors.SpinEdit spinMinLevel;
        private DevExpress.XtraEditors.LabelControl lblMinLevel;
        private DevExpress.XtraEditors.SearchLookUpEdit cmbWarehouse;
        private DevExpress.XtraEditors.LabelControl lblWarehouse;
        private DevExpress.XtraEditors.SearchLookUpEdit cmbCategory;
        private DevExpress.XtraEditors.LabelControl lblCategory;
        private DevExpress.XtraEditors.SearchLookUpEdit cmbUnitType;
        private DevExpress.XtraEditors.LabelControl lblUnitType;
        private DevExpress.XtraEditors.MemoEdit memoDescription;
        private DevExpress.XtraEditors.LabelControl lblDescription;
        private DevExpress.XtraEditors.CheckEdit chkActive;
        private DevExpress.XtraEditors.SimpleButton btnAddUnitType;

        private DevExpress.XtraEditors.PanelControl pnlPriceButtons;
        private DevExpress.XtraGrid.GridControl gridPrices;
        private DevExpress.XtraGrid.Views.Grid.GridView gridPriceView;
        private DevExpress.XtraEditors.SimpleButton btnAddPrice;
        private DevExpress.XtraEditors.SimpleButton btnRemovePrice;

        private DevExpress.XtraGrid.GridControl gridMovements;
        private DevExpress.XtraGrid.Views.Grid.GridView gridMovementView;

        private DevExpress.XtraEditors.PanelControl pnlImageButtons;
        private DevExpress.XtraGrid.GridControl gridImages;
        private DevExpress.XtraGrid.Views.Grid.GridView gridImageView;
        private DevExpress.XtraEditors.SimpleButton btnAddImage;
        private DevExpress.XtraEditors.SimpleButton btnRemoveImage;
        private DevExpress.XtraEditors.SimpleButton btnSetPrimary;

        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private DevExpress.XtraEditors.PanelControl pnlFooter;

        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox riCombo;
        private DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit riSpin;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit riDate;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit riDateNull;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit riCheck;

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
            components = new System.ComponentModel.Container();
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            tabMain = new DevExpress.XtraTab.XtraTabControl();
            tabBasic = new DevExpress.XtraTab.XtraTabPage();
            lblName = new DevExpress.XtraEditors.LabelControl();
            txtName = new DevExpress.XtraEditors.TextEdit();
            lblCode = new DevExpress.XtraEditors.LabelControl();
            txtProductCode = new DevExpress.XtraEditors.TextEdit();
            lblTax = new DevExpress.XtraEditors.LabelControl();
            spinTaxRate = new DevExpress.XtraEditors.SpinEdit();
            lblBarcode = new DevExpress.XtraEditors.LabelControl();
            picBarcode = new DevExpress.XtraEditors.PictureEdit();
            lblQR = new DevExpress.XtraEditors.LabelControl();
            picQR = new DevExpress.XtraEditors.PictureEdit();
            lblMinLevel = new DevExpress.XtraEditors.LabelControl();
            spinMinLevel = new DevExpress.XtraEditors.SpinEdit();
            lblWarehouse = new DevExpress.XtraEditors.LabelControl();
            cmbWarehouse = new DevExpress.XtraEditors.SearchLookUpEdit();
            lblCategory = new DevExpress.XtraEditors.LabelControl();
            cmbCategory = new DevExpress.XtraEditors.SearchLookUpEdit();
            lblUnitType = new DevExpress.XtraEditors.LabelControl();
            cmbUnitType = new DevExpress.XtraEditors.SearchLookUpEdit();
            btnAddUnitType = new DevExpress.XtraEditors.SimpleButton();
            lblDescription = new DevExpress.XtraEditors.LabelControl();
            memoDescription = new DevExpress.XtraEditors.MemoEdit();
            chkActive = new DevExpress.XtraEditors.CheckEdit();
            tabPrices = new DevExpress.XtraTab.XtraTabPage();
            pnlPriceButtons = new DevExpress.XtraEditors.PanelControl();
            btnAddPrice = new DevExpress.XtraEditors.SimpleButton();
            btnRemovePrice = new DevExpress.XtraEditors.SimpleButton();
            gridPrices = new DevExpress.XtraGrid.GridControl();
            gridPriceView = new DevExpress.XtraGrid.Views.Grid.GridView();
            riCombo = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            riSpin = new DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit();
            riDate = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            riDateNull = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            tabMovements = new DevExpress.XtraTab.XtraTabPage();
            gridMovements = new DevExpress.XtraGrid.GridControl();
            gridMovementView = new DevExpress.XtraGrid.Views.Grid.GridView();
            tabImages = new DevExpress.XtraTab.XtraTabPage();
            pnlImageButtons = new DevExpress.XtraEditors.PanelControl();
            btnAddImage = new DevExpress.XtraEditors.SimpleButton();
            btnRemoveImage = new DevExpress.XtraEditors.SimpleButton();
            btnSetPrimary = new DevExpress.XtraEditors.SimpleButton();
            gridImages = new DevExpress.XtraGrid.GridControl();
            gridImageView = new DevExpress.XtraGrid.Views.Grid.GridView();
            riCheck = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            pnlHeader.SuspendLayout();
            pnlFooter.SuspendLayout();
            tabMain.SuspendLayout();
            tabBasic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtProductCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinTaxRate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBarcode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picQR.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinMinLevel.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbWarehouse.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbCategory.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbUnitType.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoDescription.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            tabPrices.SuspendLayout();
            pnlPriceButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridPrices).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riCombo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riSpin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riDate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riDateNull).BeginInit();
            tabMovements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridMovements).BeginInit();
            tabImages.SuspendLayout();
            pnlImageButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridImages).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riCheck).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Appearance.Options.UseBackColor = false;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(960, 58);
            pnlHeader.TabIndex = 0;
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.Location = new System.Drawing.Point(18, 13);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new System.Drawing.Size(32, 32);
            lblHeaderIcon.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new System.Drawing.Point(62, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(140, 28);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Ürün Düzenle";
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubtitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(130, 138, 150);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new System.Drawing.Point(62, 35);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new System.Drawing.Size(360, 17);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "Ürün bilgilerini güncelleyin";
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.BackColor = System.Drawing.Color.FromArgb(224, 226, 230);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlHeaderLine.Location = new System.Drawing.Point(0, 57);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new System.Drawing.Size(960, 1);
            pnlHeaderLine.TabIndex = 3;
            // 
            // pnlFooter
            // 
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlFooter.Location = new System.Drawing.Point(0, 636);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new System.Drawing.Size(960, 64);
            pnlFooter.TabIndex = 1;
            // 
            // btnCancel
            // 
            btnCancel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(718, 15);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(112, 34);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Vazgeç";
            // 
            // btnSave
            // 
            btnSave.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
            btnSave.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            btnSave.Appearance.ForeColor = System.Drawing.Color.White;
            btnSave.Appearance.Options.UseBackColor = true;
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Appearance.Options.UseForeColor = true;
            btnSave.Location = new System.Drawing.Point(836, 15);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(110, 34);
            btnSave.TabIndex = 1;
            btnSave.Text = "Kaydet";
            // 
            // tabMain
            // 
            tabMain.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            tabMain.Appearance.Options.UseFont = true;
            tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMain.Location = new System.Drawing.Point(0, 58);
            tabMain.Name = "tabMain";
            tabMain.SelectedTabPage = tabBasic;
            tabMain.Size = new System.Drawing.Size(960, 578);
            tabMain.TabIndex = 2;
            tabMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabBasic, tabPrices, tabMovements, tabImages });
            // 
            // tabBasic
            // 
            tabBasic.Controls.Add(chkActive);
            tabBasic.Controls.Add(lblDescription);
            tabBasic.Controls.Add(memoDescription);
            tabBasic.Controls.Add(lblUnitType);
            tabBasic.Controls.Add(btnAddUnitType);
            tabBasic.Controls.Add(cmbCategory);
            tabBasic.Controls.Add(cmbWarehouse);
            tabBasic.Controls.Add(lblWarehouse);
            tabBasic.Controls.Add(lblCategory);
            tabBasic.Controls.Add(cmbUnitType);
            tabBasic.Controls.Add(lblMinLevel);
            tabBasic.Controls.Add(spinMinLevel);
            tabBasic.Controls.Add(picQR);
            tabBasic.Controls.Add(lblQR);
            tabBasic.Controls.Add(picBarcode);
            tabBasic.Controls.Add(lblBarcode);
            tabBasic.Controls.Add(spinTaxRate);
            tabBasic.Controls.Add(lblTax);
            tabBasic.Controls.Add(txtProductCode);
            tabBasic.Controls.Add(lblCode);
            tabBasic.Controls.Add(txtName);
            tabBasic.Controls.Add(lblName);
            tabBasic.Location = new System.Drawing.Point(1, 27);
            tabBasic.Name = "tabBasic";
            tabBasic.Size = new System.Drawing.Size(952, 540);
            tabBasic.TabIndex = 0;
            tabBasic.Text = "Temel Bilgiler";
            // 
            // lblName
            // 
            lblName.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblName.Appearance.Options.UseFont = true;
            lblName.Location = new System.Drawing.Point(28, 16);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(70, 18);
            lblName.TabIndex = 0;
            lblName.Text = "Ürün Adı *";
            // 
            // txtName
            // 
            txtName.Location = new System.Drawing.Point(28, 36);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(440, 30);
            txtName.TabIndex = 1;
            // 
            // lblCode
            // 
            lblCode.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblCode.Appearance.Options.UseFont = true;
            lblCode.Location = new System.Drawing.Point(500, 16);
            lblCode.Name = "lblCode";
            lblCode.Size = new System.Drawing.Size(62, 18);
            lblCode.TabIndex = 2;
            lblCode.Text = "Stok Kodu";
            // 
            // txtProductCode
            // 
            txtProductCode.Location = new System.Drawing.Point(500, 36);
            txtProductCode.Name = "txtProductCode";
            txtProductCode.Properties.AllowFocused = false;
            txtProductCode.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            txtProductCode.Properties.Appearance.Options.UseBackColor = true;
            txtProductCode.Properties.ReadOnly = true;
            txtProductCode.Size = new System.Drawing.Size(200, 30);
            txtProductCode.TabIndex = 3;
            // 
            // lblTax
            // 
            lblTax.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblTax.Appearance.Options.UseFont = true;
            lblTax.Location = new System.Drawing.Point(720, 16);
            lblTax.Name = "lblTax";
            lblTax.Size = new System.Drawing.Size(56, 18);
            lblTax.TabIndex = 4;
            lblTax.Text = "KDV (%) *";
            // 
            // spinTaxRate
            // 
            spinTaxRate.EditValue = 0D;
            spinTaxRate.Location = new System.Drawing.Point(720, 36);
            spinTaxRate.Name = "spinTaxRate";
            spinTaxRate.Properties.Increment = 1;
            spinTaxRate.Properties.IsFloatValue = true;
            spinTaxRate.Properties.MaxValue = 100;
            spinTaxRate.Properties.MinValue = 0;
            spinTaxRate.Size = new System.Drawing.Size(80, 30);
            spinTaxRate.TabIndex = 5;
            // 
            // lblBarcode
            // 
            lblBarcode.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblBarcode.Appearance.Options.UseFont = true;
            lblBarcode.Location = new System.Drawing.Point(28, 76);
            lblBarcode.Name = "lblBarcode";
            lblBarcode.Size = new System.Drawing.Size(44, 18);
            lblBarcode.TabIndex = 6;
            lblBarcode.Text = "Barkod";
            // 
            // picBarcode
            // 
            picBarcode.Location = new System.Drawing.Point(28, 96);
            picBarcode.Name = "picBarcode";
            picBarcode.Properties.NullText = "";
            picBarcode.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze;
            picBarcode.Size = new System.Drawing.Size(72, 30);
            picBarcode.TabIndex = 7;
            picBarcode.ToolTip = "Barkod";
            // 
            // lblQR
            // 
            lblQR.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblQR.Appearance.Options.UseFont = true;
            lblQR.Location = new System.Drawing.Point(500, 76);
            lblQR.Name = "lblQR";
            lblQR.Size = new System.Drawing.Size(50, 18);
            lblQR.TabIndex = 8;
            lblQR.Text = "Karekod";
            // 
            // picQR
            // 
            picQR.Location = new System.Drawing.Point(500, 96);
            picQR.Name = "picQR";
            picQR.Properties.NullText = "";
            picQR.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze;
            picQR.Size = new System.Drawing.Size(48, 30);
            picQR.TabIndex = 9;
            picQR.ToolTip = "Karekod";
            // 
            // lblMinLevel
            // 
            lblMinLevel.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblMinLevel.Appearance.Options.UseFont = true;
            lblMinLevel.Location = new System.Drawing.Point(720, 76);
            lblMinLevel.Name = "lblMinLevel";
            lblMinLevel.Size = new System.Drawing.Size(116, 18);
            lblMinLevel.TabIndex = 10;
            lblMinLevel.Text = "Min. Stok Seviyesi";
            // 
            // spinMinLevel
            // 
            spinMinLevel.EditValue = 0D;
            spinMinLevel.Location = new System.Drawing.Point(720, 96);
            spinMinLevel.Name = "spinMinLevel";
            spinMinLevel.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
            spinMinLevel.Properties.MinValue = 0;
            spinMinLevel.Size = new System.Drawing.Size(80, 30);
            spinMinLevel.TabIndex = 11;
            // 
            // lblWarehouse
            // 
            lblWarehouse.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblWarehouse.Appearance.Options.UseFont = true;
            lblWarehouse.Location = new System.Drawing.Point(28, 136);
            lblWarehouse.Name = "lblWarehouse";
            lblWarehouse.Size = new System.Drawing.Size(44, 18);
            lblWarehouse.TabIndex = 12;
            lblWarehouse.Text = "Depo *";
            // 
            // cmbWarehouse
            // 
            cmbWarehouse.Location = new System.Drawing.Point(28, 156);
            cmbWarehouse.Name = "cmbWarehouse";
            cmbWarehouse.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbWarehouse.Properties.NullText = "";
            cmbWarehouse.Size = new System.Drawing.Size(440, 30);
            cmbWarehouse.TabIndex = 13;
            // 
            // lblCategory
            // 
            lblCategory.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblCategory.Appearance.Options.UseFont = true;
            lblCategory.Location = new System.Drawing.Point(500, 136);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new System.Drawing.Size(58, 18);
            lblCategory.TabIndex = 14;
            lblCategory.Text = "Kategori *";
            // 
            // cmbCategory
            // 
            cmbCategory.Enabled = false;
            cmbCategory.Location = new System.Drawing.Point(500, 156);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbCategory.Properties.NullText = "";
            cmbCategory.Size = new System.Drawing.Size(200, 30);
            cmbCategory.TabIndex = 15;
            // 
            // lblUnitType
            // 
            lblUnitType.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblUnitType.Appearance.Options.UseFont = true;
            lblUnitType.Location = new System.Drawing.Point(720, 136);
            lblUnitType.Name = "lblUnitType";
            lblUnitType.Size = new System.Drawing.Size(78, 18);
            lblUnitType.TabIndex = 16;
            lblUnitType.Text = "Birim Cinsi *";
            // 
            // cmbUnitType
            // 
            cmbUnitType.Location = new System.Drawing.Point(720, 156);
            cmbUnitType.Name = "cmbUnitType";
            cmbUnitType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbUnitType.Properties.NullText = "";
            cmbUnitType.Size = new System.Drawing.Size(120, 30);
            cmbUnitType.TabIndex = 17;
            // 
            // btnAddUnitType
            // 
            btnAddUnitType.Location = new System.Drawing.Point(848, 156);
            btnAddUnitType.Name = "btnAddUnitType";
            btnAddUnitType.Size = new System.Drawing.Size(52, 30);
            btnAddUnitType.TabIndex = 18;
            btnAddUnitType.Text = "Yeni";
            // 
            // lblDescription
            // 
            lblDescription.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblDescription.Appearance.Options.UseFont = true;
            lblDescription.Location = new System.Drawing.Point(28, 196);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(56, 18);
            lblDescription.TabIndex = 19;
            lblDescription.Text = "Açıklama";
            // 
            // memoDescription
            // 
            memoDescription.Location = new System.Drawing.Point(28, 216);
            memoDescription.Name = "memoDescription";
            memoDescription.Size = new System.Drawing.Size(692, 80);
            memoDescription.TabIndex = 20;
            // 
            // chkActive
            // 
            chkActive.Location = new System.Drawing.Point(28, 306);
            chkActive.Name = "chkActive";
            chkActive.Properties.Caption = "Aktif";
            chkActive.Size = new System.Drawing.Size(64, 24);
            chkActive.TabIndex = 21;
            // 
            // tabPrices
            // 
            tabPrices.Controls.Add(gridPrices);
            tabPrices.Controls.Add(pnlPriceButtons);
            tabPrices.Location = new System.Drawing.Point(1, 27);
            tabPrices.Name = "tabPrices";
            tabPrices.Size = new System.Drawing.Size(952, 540);
            tabPrices.TabIndex = 1;
            tabPrices.Text = "Fiyatlar";
            // 
            // pnlPriceButtons
            // 
            pnlPriceButtons.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlPriceButtons.Controls.Add(btnRemovePrice);
            pnlPriceButtons.Controls.Add(btnAddPrice);
            pnlPriceButtons.Dock = System.Windows.Forms.DockStyle.Top;
            pnlPriceButtons.Location = new System.Drawing.Point(0, 0);
            pnlPriceButtons.Name = "pnlPriceButtons";
            pnlPriceButtons.Size = new System.Drawing.Size(952, 50);
            pnlPriceButtons.TabIndex = 0;
            // 
            // btnAddPrice
            // 
            btnAddPrice.Location = new System.Drawing.Point(4, 10);
            btnAddPrice.Name = "btnAddPrice";
            btnAddPrice.Size = new System.Drawing.Size(110, 32);
            btnAddPrice.TabIndex = 0;
            btnAddPrice.Text = "Fiyat Ekle";
            // 
            // btnRemovePrice
            // 
            btnRemovePrice.Location = new System.Drawing.Point(120, 10);
            btnRemovePrice.Name = "btnRemovePrice";
            btnRemovePrice.Size = new System.Drawing.Size(110, 32);
            btnRemovePrice.TabIndex = 1;
            btnRemovePrice.Text = "Seçiliyi Sil";
            // 
            // gridPrices
            // 
            gridPrices.Dock = System.Windows.Forms.DockStyle.Fill;
            gridPrices.Location = new System.Drawing.Point(0, 50);
            gridPrices.MainView = gridPriceView;
            gridPrices.Name = "gridPrices";
            gridPrices.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { riCombo, riSpin, riDate, riDateNull });
            gridPrices.Size = new System.Drawing.Size(952, 490);
            gridPrices.TabIndex = 1;
            gridPrices.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridPriceView });
            // 
            // gridPriceView
            // 
            gridPriceView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Fiyat Tipi", FieldName = "PriceTypeName", ColumnEdit = riCombo, Name = "colPriceType", Visible = true, Width = 110 });
            gridPriceView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Birim Fiyat", FieldName = "UnitPrice", ColumnEdit = riSpin, Name = "colUnitPrice", Visible = true, Width = 140 });
            gridPriceView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Başlangıç", FieldName = "StartDate", ColumnEdit = riDate, Name = "colStartDate", Visible = true, Width = 120 });
            gridPriceView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Bitiş", FieldName = "EndDate", ColumnEdit = riDateNull, Name = "colEndDate", Visible = true, Width = 120 });
            gridPriceView.GridControl = gridPrices;
            gridPriceView.Name = "gridPriceView";
            gridPriceView.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.RowSelect;
            gridPriceView.OptionsView.ShowGroupPanel = false;
            gridPriceView.Columns["colUnitPrice"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridPriceView.Columns["colUnitPrice"].DisplayFormat.FormatString = "n2";
            gridPriceView.Columns["colStartDate"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gridPriceView.Columns["colStartDate"].DisplayFormat.FormatString = "d";
            gridPriceView.Columns["colEndDate"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gridPriceView.Columns["colEndDate"].DisplayFormat.FormatString = "d";
            // 
            // riCombo
            // 
            riCombo.Items.AddRange(new object[] { "Alış", "Satış" });
            riCombo.Name = "riCombo";
            // 
            // riSpin
            // 
            riSpin.IsFloatValue = true;
            riSpin.Mask.EditMask = "n2";
            riSpin.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            riSpin.MinValue = 0;
            riSpin.Name = "riSpin";
            // 
            // riDate
            // 
            riDate.Name = "riDate";
            // 
            // riDateNull
            // 
            riDateNull.Name = "riDateNull";
            // 
            // tabMovements
            // 
            tabMovements.Controls.Add(gridMovements);
            tabMovements.Location = new System.Drawing.Point(1, 27);
            tabMovements.Name = "tabMovements";
            tabMovements.Size = new System.Drawing.Size(952, 540);
            tabMovements.TabIndex = 2;
            tabMovements.Text = "Stok Hareketleri";
            // 
            // gridMovements
            // 
            gridMovements.Dock = System.Windows.Forms.DockStyle.Fill;
            gridMovements.Location = new System.Drawing.Point(0, 0);
            gridMovements.MainView = gridMovementView;
            gridMovements.Name = "gridMovements";
            gridMovements.Size = new System.Drawing.Size(952, 540);
            gridMovements.TabIndex = 0;
            gridMovements.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridMovementView });
            // 
            // gridMovementView
            // 
            gridMovementView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Tarih", FieldName = "Date", Name = "colMovDate", Visible = true, Width = 100 });
            gridMovementView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Hareket", FieldName = "MovementType", Name = "colMovType", Visible = true, Width = 80 });
            gridMovementView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Miktar", FieldName = "Quantity", Name = "colMovQuantity", Visible = true, Width = 90 });
            gridMovementView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Birim Fiyat", FieldName = "UnitPrice", Name = "colMovUnitPrice", Visible = true, Width = 110 });
            gridMovementView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Referans No", FieldName = "ReferenceNo", Name = "colMovRefNo", Visible = true, Width = 120 });
            gridMovementView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Açıklama", FieldName = "Description", Name = "colMovDesc", Visible = true, Width = 200 });
            gridMovementView.GridControl = gridMovements;
            gridMovementView.Name = "gridMovementView";
            gridMovementView.OptionsBehavior.ReadOnly = true;
            gridMovementView.OptionsView.ShowGroupPanel = false;
            gridMovementView.Columns["colMovDate"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gridMovementView.Columns["colMovDate"].DisplayFormat.FormatString = "d";
            gridMovementView.Columns["colMovQuantity"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridMovementView.Columns["colMovQuantity"].DisplayFormat.FormatString = "n2";
            gridMovementView.Columns["colMovUnitPrice"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridMovementView.Columns["colMovUnitPrice"].DisplayFormat.FormatString = "n2";
            // 
            // tabImages
            // 
            tabImages.Controls.Add(gridImages);
            tabImages.Controls.Add(pnlImageButtons);
            tabImages.Location = new System.Drawing.Point(1, 27);
            tabImages.Name = "tabImages";
            tabImages.Size = new System.Drawing.Size(952, 540);
            tabImages.TabIndex = 3;
            tabImages.Text = "Resimler";
            // 
            // pnlImageButtons
            // 
            pnlImageButtons.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlImageButtons.Controls.Add(btnSetPrimary);
            pnlImageButtons.Controls.Add(btnRemoveImage);
            pnlImageButtons.Controls.Add(btnAddImage);
            pnlImageButtons.Dock = System.Windows.Forms.DockStyle.Top;
            pnlImageButtons.Location = new System.Drawing.Point(0, 0);
            pnlImageButtons.Name = "pnlImageButtons";
            pnlImageButtons.Size = new System.Drawing.Size(952, 50);
            pnlImageButtons.TabIndex = 0;
            // 
            // btnAddImage
            // 
            btnAddImage.Location = new System.Drawing.Point(4, 10);
            btnAddImage.Name = "btnAddImage";
            btnAddImage.Size = new System.Drawing.Size(110, 32);
            btnAddImage.TabIndex = 0;
            btnAddImage.Text = "Resim Ekle";
            // 
            // btnRemoveImage
            // 
            btnRemoveImage.Location = new System.Drawing.Point(120, 10);
            btnRemoveImage.Name = "btnRemoveImage";
            btnRemoveImage.Size = new System.Drawing.Size(80, 32);
            btnRemoveImage.TabIndex = 1;
            btnRemoveImage.Text = "Kaldır";
            // 
            // btnSetPrimary
            // 
            btnSetPrimary.Location = new System.Drawing.Point(206, 10);
            btnSetPrimary.Name = "btnSetPrimary";
            btnSetPrimary.Size = new System.Drawing.Size(80, 32);
            btnSetPrimary.TabIndex = 2;
            btnSetPrimary.Text = "Ana Yap";
            // 
            // gridImages
            // 
            gridImages.Dock = System.Windows.Forms.DockStyle.Fill;
            gridImages.Location = new System.Drawing.Point(0, 50);
            gridImages.MainView = gridImageView;
            gridImages.Name = "gridImages";
            gridImages.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { riCheck });
            gridImages.Size = new System.Drawing.Size(952, 490);
            gridImages.TabIndex = 1;
            gridImages.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridImageView });
            // 
            // gridImageView
            // 
            gridImageView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Dosya Yolu", FieldName = "Path", Name = "colImagePath", Visible = true, Width = 560 });
            gridImageView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn { Caption = "Ana", FieldName = "IsPrimary", ColumnEdit = riCheck, Name = "colImagePrimary", Visible = true, Width = 60 });
            gridImageView.GridControl = gridImages;
            gridImageView.Name = "gridImageView";
            gridImageView.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.RowSelect;
            gridImageView.OptionsView.ShowGroupPanel = false;
            // 
            // riCheck
            // 
            riCheck.Name = "riCheck";
            // 
            // ProductEditForm
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(960, 700);
            Controls.Add(tabMain);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProductEditForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Ürün Düzenle";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlFooter.ResumeLayout(false);
            tabMain.ResumeLayout(false);
            tabBasic.ResumeLayout(false);
            tabBasic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtProductCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinTaxRate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBarcode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)picQR.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinMinLevel.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbWarehouse.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCategory.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbUnitType.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoDescription.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            tabPrices.ResumeLayout(false);
            pnlPriceButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridPrices).EndInit();
            ((System.ComponentModel.ISupportInitialize)riCombo).EndInit();
            ((System.ComponentModel.ISupportInitialize)riSpin).EndInit();
            ((System.ComponentModel.ISupportInitialize)riDate).EndInit();
            ((System.ComponentModel.ISupportInitialize)riDateNull).EndInit();
            tabMovements.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridMovements).EndInit();
            tabImages.ResumeLayout(false);
            pnlImageButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridImages).EndInit();
            ((System.ComponentModel.ISupportInitialize)riCheck).EndInit();
            ResumeLayout(false);
        }
    }
}