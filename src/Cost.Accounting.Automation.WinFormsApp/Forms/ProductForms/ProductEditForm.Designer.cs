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
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            tabMain = new DevExpress.XtraTab.XtraTabControl();
            tabBasic = new DevExpress.XtraTab.XtraTabPage();
            chkActive = new DevExpress.XtraEditors.CheckEdit();
            lblDescription = new DevExpress.XtraEditors.LabelControl();
            memoDescription = new DevExpress.XtraEditors.MemoEdit();
            lblUnitType = new DevExpress.XtraEditors.LabelControl();
            btnAddUnitType = new DevExpress.XtraEditors.SimpleButton();
            cmbCategory = new DevExpress.XtraEditors.SearchLookUpEdit();
            cmbWarehouse = new DevExpress.XtraEditors.SearchLookUpEdit();
            lblWarehouse = new DevExpress.XtraEditors.LabelControl();
            lblCategory = new DevExpress.XtraEditors.LabelControl();
            cmbUnitType = new DevExpress.XtraEditors.SearchLookUpEdit();
            lblMinLevel = new DevExpress.XtraEditors.LabelControl();
            spinMinLevel = new DevExpress.XtraEditors.SpinEdit();
            picQR = new DevExpress.XtraEditors.PictureEdit();
            lblQR = new DevExpress.XtraEditors.LabelControl();
            picBarcode = new DevExpress.XtraEditors.PictureEdit();
            lblBarcode = new DevExpress.XtraEditors.LabelControl();
            spinTaxRate = new DevExpress.XtraEditors.SpinEdit();
            lblTax = new DevExpress.XtraEditors.LabelControl();
            txtProductCode = new DevExpress.XtraEditors.TextEdit();
            lblCode = new DevExpress.XtraEditors.LabelControl();
            txtName = new DevExpress.XtraEditors.TextEdit();
            lblName = new DevExpress.XtraEditors.LabelControl();
            tabPrices = new DevExpress.XtraTab.XtraTabPage();
            gridPrices = new DevExpress.XtraGrid.GridControl();
            gridPriceView = new DevExpress.XtraGrid.Views.Grid.GridView();
            gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            riCombo = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            riSpin = new DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit();
            riDate = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            riDateNull = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            pnlPriceButtons = new DevExpress.XtraEditors.PanelControl();
            btnRemovePrice = new DevExpress.XtraEditors.SimpleButton();
            btnAddPrice = new DevExpress.XtraEditors.SimpleButton();
            tabMovements = new DevExpress.XtraTab.XtraTabPage();
            gridMovements = new DevExpress.XtraGrid.GridControl();
            gridMovementView = new DevExpress.XtraGrid.Views.Grid.GridView();
            gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn7 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn8 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn9 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn10 = new DevExpress.XtraGrid.Columns.GridColumn();
            tabImages = new DevExpress.XtraTab.XtraTabPage();
            gridImages = new DevExpress.XtraGrid.GridControl();
            gridImageView = new DevExpress.XtraGrid.Views.Grid.GridView();
            gridColumn11 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn12 = new DevExpress.XtraGrid.Columns.GridColumn();
            riCheck = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            pnlImageButtons = new DevExpress.XtraEditors.PanelControl();
            btnSetPrimary = new DevExpress.XtraEditors.SimpleButton();
            btnRemoveImage = new DevExpress.XtraEditors.SimpleButton();
            btnAddImage = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tabMain).BeginInit();
            tabMain.SuspendLayout();
            tabBasic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoDescription.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbCategory.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbWarehouse.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbUnitType.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinMinLevel.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picQR.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBarcode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinTaxRate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtProductCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).BeginInit();
            tabPrices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridPrices).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPriceView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riCombo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riSpin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riDate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riDate.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riDateNull).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riDateNull.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlPriceButtons).BeginInit();
            pnlPriceButtons.SuspendLayout();
            tabMovements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridMovements).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridMovementView).BeginInit();
            tabImages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridImages).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridImageView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riCheck).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlImageButtons).BeginInit();
            pnlImageButtons.SuspendLayout();
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
            pnlHeader.Size = new Size(753, 58);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(130, 138, 150);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(62, 35);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(140, 13);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "Ürün bilgilerini güncelleyin";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 12F);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new Point(62, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(97, 21);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Ürün Düzenle";
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.Location = new Point(18, 13);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(0, 13);
            lblHeaderIcon.TabIndex = 0;
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.BackColor = Color.FromArgb(224, 226, 230);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = DockStyle.Bottom;
            pnlHeaderLine.Location = new Point(0, 57);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new Size(753, 1);
            pnlHeaderLine.TabIndex = 3;
            // 
            // pnlFooter
            // 
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 636);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(753, 64);
            pnlFooter.TabIndex = 1;
            // 
            // btnCancel
            // 
            btnCancel.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(493, 18);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(112, 34);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Vazgeç";
            // 
            // btnSave
            // 
            btnSave.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
            btnSave.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSave.Appearance.ForeColor = Color.White;
            btnSave.Appearance.Options.UseBackColor = true;
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Appearance.Options.UseForeColor = true;
            btnSave.Location = new Point(611, 18);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 34);
            btnSave.TabIndex = 1;
            btnSave.Text = "Kaydet";
            // 
            // tabMain
            // 
            tabMain.Appearance.Font = new Font("Segoe UI", 10F);
            tabMain.Appearance.Options.UseFont = true;
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 58);
            tabMain.Name = "tabMain";
            tabMain.SelectedTabPage = tabBasic;
            tabMain.Size = new Size(753, 578);
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
            tabBasic.Name = "tabBasic";
            tabBasic.Size = new Size(751, 553);
            tabBasic.Text = "Temel Bilgiler";
            // 
            // chkActive
            // 
            chkActive.Location = new Point(28, 306);
            chkActive.Name = "chkActive";
            chkActive.Properties.Caption = "Aktif";
            chkActive.Size = new Size(64, 20);
            chkActive.TabIndex = 21;
            // 
            // lblDescription
            // 
            lblDescription.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblDescription.Appearance.Options.UseFont = true;
            lblDescription.Location = new Point(28, 196);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(49, 15);
            lblDescription.TabIndex = 19;
            lblDescription.Text = "Açıklama";
            // 
            // memoDescription
            // 
            memoDescription.Location = new Point(28, 216);
            memoDescription.Name = "memoDescription";
            memoDescription.Size = new Size(692, 80);
            memoDescription.TabIndex = 20;
            // 
            // lblUnitType
            // 
            lblUnitType.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblUnitType.Appearance.Options.UseFont = true;
            lblUnitType.Location = new Point(501, 127);
            lblUnitType.Name = "lblUnitType";
            lblUnitType.Size = new Size(64, 15);
            lblUnitType.TabIndex = 16;
            lblUnitType.Text = "Birim Cinsi *";
            // 
            // btnAddUnitType
            // 
            btnAddUnitType.Location = new Point(629, 147);
            btnAddUnitType.Name = "btnAddUnitType";
            btnAddUnitType.Size = new Size(52, 30);
            btnAddUnitType.TabIndex = 18;
            btnAddUnitType.Text = "Yeni";
            // 
            // cmbCategory
            // 
            cmbCategory.Enabled = false;
            cmbCategory.Location = new Point(28, 133);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbCategory.Properties.NullText = "";
            cmbCategory.Size = new Size(440, 20);
            cmbCategory.TabIndex = 15;
            // 
            // cmbWarehouse
            // 
            cmbWarehouse.Location = new Point(28, 83);
            cmbWarehouse.Name = "cmbWarehouse";
            cmbWarehouse.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbWarehouse.Properties.NullText = "";
            cmbWarehouse.Size = new Size(440, 20);
            cmbWarehouse.TabIndex = 13;
            // 
            // lblWarehouse
            // 
            lblWarehouse.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblWarehouse.Appearance.Options.UseFont = true;
            lblWarehouse.Location = new Point(28, 63);
            lblWarehouse.Name = "lblWarehouse";
            lblWarehouse.Size = new Size(37, 15);
            lblWarehouse.TabIndex = 12;
            lblWarehouse.Text = "Depo *";
            // 
            // lblCategory
            // 
            lblCategory.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblCategory.Appearance.Options.UseFont = true;
            lblCategory.Location = new Point(28, 113);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(52, 15);
            lblCategory.TabIndex = 14;
            lblCategory.Text = "Kategori *";
            // 
            // cmbUnitType
            // 
            cmbUnitType.Location = new Point(501, 147);
            cmbUnitType.Name = "cmbUnitType";
            cmbUnitType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbUnitType.Properties.NullText = "";
            cmbUnitType.Size = new Size(120, 20);
            cmbUnitType.TabIndex = 17;
            // 
            // lblMinLevel
            // 
            lblMinLevel.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblMinLevel.Appearance.Options.UseFont = true;
            lblMinLevel.Location = new Point(500, 74);
            lblMinLevel.Name = "lblMinLevel";
            lblMinLevel.Size = new Size(96, 15);
            lblMinLevel.TabIndex = 10;
            lblMinLevel.Text = "Min. Stok Seviyesi";
            // 
            // spinMinLevel
            // 
            spinMinLevel.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spinMinLevel.Location = new Point(500, 94);
            spinMinLevel.Name = "spinMinLevel";
            spinMinLevel.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
            spinMinLevel.Size = new Size(80, 20);
            spinMinLevel.TabIndex = 11;
            // 
            // picQR
            // 
            picQR.Location = new Point(501, 355);
            picQR.Name = "picQR";
            picQR.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze;
            picQR.Size = new Size(207, 165);
            picQR.TabIndex = 9;
            picQR.ToolTip = "Karekod";
            // 
            // lblQR
            // 
            lblQR.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblQR.Appearance.Options.UseFont = true;
            lblQR.Location = new Point(590, 335);
            lblQR.Name = "lblQR";
            lblQR.Size = new Size(43, 15);
            lblQR.TabIndex = 8;
            lblQR.Text = "Karekod";
            // 
            // picBarcode
            // 
            picBarcode.Location = new Point(28, 376);
            picBarcode.Name = "picBarcode";
            picBarcode.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze;
            picBarcode.Size = new Size(393, 76);
            picBarcode.TabIndex = 7;
            picBarcode.ToolTip = "Barkod";
            // 
            // lblBarcode
            // 
            lblBarcode.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblBarcode.Appearance.Options.UseFont = true;
            lblBarcode.Location = new Point(28, 356);
            lblBarcode.Name = "lblBarcode";
            lblBarcode.Size = new Size(37, 15);
            lblBarcode.TabIndex = 6;
            lblBarcode.Text = "Barkod";
            // 
            // spinTaxRate
            // 
            spinTaxRate.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spinTaxRate.Location = new Point(628, 94);
            spinTaxRate.Name = "spinTaxRate";
            spinTaxRate.Properties.MaxValue = new decimal(new int[] { 100, 0, 0, 0 });
            spinTaxRate.Size = new Size(80, 20);
            spinTaxRate.TabIndex = 5;
            // 
            // lblTax
            // 
            lblTax.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblTax.Appearance.Options.UseFont = true;
            lblTax.Location = new Point(628, 74);
            lblTax.Name = "lblTax";
            lblTax.Size = new Size(53, 15);
            lblTax.TabIndex = 4;
            lblTax.Text = "KDV (%) *";
            // 
            // txtProductCode
            // 
            txtProductCode.Location = new Point(500, 36);
            txtProductCode.Name = "txtProductCode";
            txtProductCode.Properties.AllowFocused = false;
            txtProductCode.Properties.Appearance.BackColor = Color.FromArgb(240, 240, 240);
            txtProductCode.Properties.Appearance.Options.UseBackColor = true;
            txtProductCode.Properties.ReadOnly = true;
            txtProductCode.Size = new Size(208, 20);
            txtProductCode.TabIndex = 3;
            // 
            // lblCode
            // 
            lblCode.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblCode.Appearance.Options.UseFont = true;
            lblCode.Location = new Point(500, 16);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(55, 15);
            lblCode.TabIndex = 2;
            lblCode.Text = "Stok Kodu";
            // 
            // txtName
            // 
            txtName.Location = new Point(28, 36);
            txtName.Name = "txtName";
            txtName.Size = new Size(440, 20);
            txtName.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblName.Appearance.Options.UseFont = true;
            lblName.Location = new Point(28, 16);
            lblName.Name = "lblName";
            lblName.Size = new Size(55, 15);
            lblName.TabIndex = 0;
            lblName.Text = "Ürün Adı *";
            // 
            // tabPrices
            // 
            tabPrices.Controls.Add(gridPrices);
            tabPrices.Controls.Add(pnlPriceButtons);
            tabPrices.Name = "tabPrices";
            tabPrices.Size = new Size(961, 553);
            tabPrices.Text = "Fiyatlar";
            // 
            // gridPrices
            // 
            gridPrices.Dock = DockStyle.Fill;
            gridPrices.Location = new Point(0, 50);
            gridPrices.MainView = gridPriceView;
            gridPrices.Name = "gridPrices";
            gridPrices.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { riCombo, riSpin, riDate, riDateNull });
            gridPrices.Size = new Size(961, 503);
            gridPrices.TabIndex = 1;
            gridPrices.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridPriceView });
            // 
            // gridPriceView
            // 
            gridPriceView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { gridColumn1, gridColumn2, gridColumn3, gridColumn4 });
            gridPriceView.GridControl = gridPrices;
            gridPriceView.Name = "gridPriceView";
            gridPriceView.OptionsView.ShowGroupPanel = false;
            // 
            // gridColumn1
            // 
            gridColumn1.Name = "gridColumn1";
            // 
            // gridColumn2
            // 
            gridColumn2.Name = "gridColumn2";
            // 
            // gridColumn3
            // 
            gridColumn3.Name = "gridColumn3";
            // 
            // gridColumn4
            // 
            gridColumn4.Name = "gridColumn4";
            // 
            // riCombo
            // 
            riCombo.Items.AddRange(new object[] { "Alış", "Satış" });
            riCombo.Name = "riCombo";
            // 
            // riSpin
            // 
            riSpin.Mask.EditMask = "n2";
            riSpin.Name = "riSpin";
            // 
            // riDate
            // 
            riDate.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            riDate.Name = "riDate";
            // 
            // riDateNull
            // 
            riDateNull.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            riDateNull.Name = "riDateNull";
            // 
            // pnlPriceButtons
            // 
            pnlPriceButtons.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlPriceButtons.Controls.Add(btnRemovePrice);
            pnlPriceButtons.Controls.Add(btnAddPrice);
            pnlPriceButtons.Dock = DockStyle.Top;
            pnlPriceButtons.Location = new Point(0, 0);
            pnlPriceButtons.Name = "pnlPriceButtons";
            pnlPriceButtons.Size = new Size(961, 50);
            pnlPriceButtons.TabIndex = 0;
            // 
            // btnRemovePrice
            // 
            btnRemovePrice.Location = new Point(120, 10);
            btnRemovePrice.Name = "btnRemovePrice";
            btnRemovePrice.Size = new Size(110, 32);
            btnRemovePrice.TabIndex = 1;
            btnRemovePrice.Text = "Seçiliyi Sil";
            // 
            // btnAddPrice
            // 
            btnAddPrice.Location = new Point(4, 10);
            btnAddPrice.Name = "btnAddPrice";
            btnAddPrice.Size = new Size(110, 32);
            btnAddPrice.TabIndex = 0;
            btnAddPrice.Text = "Fiyat Ekle";
            // 
            // tabMovements
            // 
            tabMovements.Controls.Add(gridMovements);
            tabMovements.Name = "tabMovements";
            tabMovements.Size = new Size(961, 553);
            tabMovements.Text = "Stok Hareketleri";
            // 
            // gridMovements
            // 
            gridMovements.Dock = DockStyle.Fill;
            gridMovements.Location = new Point(0, 0);
            gridMovements.MainView = gridMovementView;
            gridMovements.Name = "gridMovements";
            gridMovements.Size = new Size(961, 553);
            gridMovements.TabIndex = 0;
            gridMovements.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridMovementView });
            // 
            // gridMovementView
            // 
            gridMovementView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { gridColumn5, gridColumn6, gridColumn7, gridColumn8, gridColumn9, gridColumn10 });
            gridMovementView.GridControl = gridMovements;
            gridMovementView.Name = "gridMovementView";
            gridMovementView.OptionsBehavior.ReadOnly = true;
            gridMovementView.OptionsView.ShowGroupPanel = false;
            // 
            // gridColumn5
            // 
            gridColumn5.Name = "gridColumn5";
            // 
            // gridColumn6
            // 
            gridColumn6.Name = "gridColumn6";
            // 
            // gridColumn7
            // 
            gridColumn7.Name = "gridColumn7";
            // 
            // gridColumn8
            // 
            gridColumn8.Name = "gridColumn8";
            // 
            // gridColumn9
            // 
            gridColumn9.Name = "gridColumn9";
            // 
            // gridColumn10
            // 
            gridColumn10.Name = "gridColumn10";
            // 
            // tabImages
            // 
            tabImages.Controls.Add(gridImages);
            tabImages.Controls.Add(pnlImageButtons);
            tabImages.Name = "tabImages";
            tabImages.Size = new Size(961, 553);
            tabImages.Text = "Resimler";
            // 
            // gridImages
            // 
            gridImages.Dock = DockStyle.Fill;
            gridImages.Location = new Point(0, 50);
            gridImages.MainView = gridImageView;
            gridImages.Name = "gridImages";
            gridImages.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { riCheck });
            gridImages.Size = new Size(961, 503);
            gridImages.TabIndex = 1;
            gridImages.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridImageView });
            // 
            // gridImageView
            // 
            gridImageView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { gridColumn11, gridColumn12 });
            gridImageView.GridControl = gridImages;
            gridImageView.Name = "gridImageView";
            gridImageView.OptionsView.ShowGroupPanel = false;
            // 
            // gridColumn11
            // 
            gridColumn11.Name = "gridColumn11";
            // 
            // gridColumn12
            // 
            gridColumn12.Name = "gridColumn12";
            // 
            // riCheck
            // 
            riCheck.Name = "riCheck";
            // 
            // pnlImageButtons
            // 
            pnlImageButtons.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlImageButtons.Controls.Add(btnSetPrimary);
            pnlImageButtons.Controls.Add(btnRemoveImage);
            pnlImageButtons.Controls.Add(btnAddImage);
            pnlImageButtons.Dock = DockStyle.Top;
            pnlImageButtons.Location = new Point(0, 0);
            pnlImageButtons.Name = "pnlImageButtons";
            pnlImageButtons.Size = new Size(961, 50);
            pnlImageButtons.TabIndex = 0;
            // 
            // btnSetPrimary
            // 
            btnSetPrimary.Location = new Point(206, 10);
            btnSetPrimary.Name = "btnSetPrimary";
            btnSetPrimary.Size = new Size(80, 32);
            btnSetPrimary.TabIndex = 2;
            btnSetPrimary.Text = "Ana Yap";
            // 
            // btnRemoveImage
            // 
            btnRemoveImage.Location = new Point(120, 10);
            btnRemoveImage.Name = "btnRemoveImage";
            btnRemoveImage.Size = new Size(80, 32);
            btnRemoveImage.TabIndex = 1;
            btnRemoveImage.Text = "Kaldır";
            // 
            // btnAddImage
            // 
            btnAddImage.Location = new Point(4, 10);
            btnAddImage.Name = "btnAddImage";
            btnAddImage.Size = new Size(110, 32);
            btnAddImage.TabIndex = 0;
            btnAddImage.Text = "Resim Ekle";
            // 
            // ProductEditForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(753, 700);
            Controls.Add(tabMain);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            IconOptions.ShowIcon = false;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProductEditForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ürün Düzenle";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)tabMain).EndInit();
            tabMain.ResumeLayout(false);
            tabBasic.ResumeLayout(false);
            tabBasic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoDescription.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCategory.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbWarehouse.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbUnitType.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinMinLevel.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)picQR.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBarcode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinTaxRate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtProductCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).EndInit();
            tabPrices.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridPrices).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPriceView).EndInit();
            ((System.ComponentModel.ISupportInitialize)riCombo).EndInit();
            ((System.ComponentModel.ISupportInitialize)riSpin).EndInit();
            ((System.ComponentModel.ISupportInitialize)riDate.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)riDate).EndInit();
            ((System.ComponentModel.ISupportInitialize)riDateNull.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)riDateNull).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlPriceButtons).EndInit();
            pnlPriceButtons.ResumeLayout(false);
            tabMovements.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridMovements).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridMovementView).EndInit();
            tabImages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridImages).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridImageView).EndInit();
            ((System.ComponentModel.ISupportInitialize)riCheck).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlImageButtons).EndInit();
            pnlImageButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn7;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn8;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn9;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn10;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn11;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn12;
    }
}