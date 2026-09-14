namespace Cost.Accounting.Automation.WinFormsApp.Forms.InvoiceForms
{
    partial class InvoiceEditForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.LabelControl lblTypeLabel;
        private DevExpress.XtraEditors.ComboBoxEdit cmbInvoiceType;
        private DevExpress.XtraEditors.LabelControl lblNumberLabel;
        private DevExpress.XtraEditors.TextEdit txtInvoiceNumber;
        private DevExpress.XtraEditors.LabelControl lblDateLabel;
        private DevExpress.XtraEditors.DateEdit dtDate;
        private DevExpress.XtraEditors.LabelControl lblAccountLabel;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpAccount;
        private DevExpress.XtraGrid.Views.Grid.GridView lookUpAccountView;
        private DevExpress.XtraEditors.LabelControl lblStatusValue;
        private DevExpress.XtraEditors.LabelControl lblDescLabel;
        private DevExpress.XtraEditors.TextEdit txtDescription;
        private DevExpress.XtraEditors.PanelControl pnlCatalog;
        private DevExpress.XtraEditors.LabelControl lblCatalogTitle;
        private DevExpress.XtraEditors.SearchLookUpEdit cmbCatalogWarehouse;
        private DevExpress.XtraGrid.Views.Grid.GridView cmbCatalogWarehouseView;
        private DevExpress.XtraGrid.GridControl gridCatalog;
        private DevExpress.XtraGrid.Views.Grid.GridView gridCatalogView;
        private DevExpress.XtraEditors.SimpleButton btnAddProduct;
        private DevExpress.XtraEditors.PanelControl pnlItemsPanel;
        private DevExpress.XtraEditors.PanelControl pnlItemsHeader;
        private DevExpress.XtraEditors.LabelControl lblItemsTitle;
        private DevExpress.XtraEditors.SimpleButton btnAddLine;
        private DevExpress.XtraEditors.SimpleButton btnDeleteLine;
        private DevExpress.XtraGrid.GridControl gridLines;
        private DevExpress.XtraGrid.Views.Grid.GridView gridLinesView;
        private DevExpress.XtraEditors.PanelControl pnlTotals;
        private DevExpress.XtraEditors.LabelControl lblSubTotalTitle;
        private DevExpress.XtraEditors.LabelControl lblSubTotalValue;
        private DevExpress.XtraEditors.LabelControl lblDiscountTotalTitle;
        private DevExpress.XtraEditors.LabelControl lblDiscountTotalValue;
        private DevExpress.XtraEditors.LabelControl lblTaxTotalTitle;
        private DevExpress.XtraEditors.LabelControl lblTaxTotalValue;
        private DevExpress.XtraEditors.LabelControl lblGrandTotalTitle;
        private DevExpress.XtraEditors.LabelControl lblGrandTotalValue;
        private DevExpress.XtraEditors.LabelControl lblTaxBrkTitle;
        private System.Windows.Forms.Label lblTaxBreakdown;
        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private DevExpress.XtraEditors.SimpleButton btnSaveDraft;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnApprove;
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
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new DevExpress.XtraEditors.PanelControl();
            this.lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            this.pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.lblTypeLabel = new DevExpress.XtraEditors.LabelControl();
            this.cmbInvoiceType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblNumberLabel = new DevExpress.XtraEditors.LabelControl();
            this.txtInvoiceNumber = new DevExpress.XtraEditors.TextEdit();
            this.lblDateLabel = new DevExpress.XtraEditors.LabelControl();
            this.dtDate = new DevExpress.XtraEditors.DateEdit();
            this.lblAccountLabel = new DevExpress.XtraEditors.LabelControl();
            this.lookUpAccount = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.lookUpAccountView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.lblStatusValue = new DevExpress.XtraEditors.LabelControl();
            this.lblDescLabel = new DevExpress.XtraEditors.LabelControl();
            this.txtDescription = new DevExpress.XtraEditors.TextEdit();
            this.pnlCatalog = new DevExpress.XtraEditors.PanelControl();
            this.lblCatalogTitle = new DevExpress.XtraEditors.LabelControl();
            this.cmbCatalogWarehouse = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.cmbCatalogWarehouseView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridCatalog = new DevExpress.XtraGrid.GridControl();
            this.gridCatalogView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnAddProduct = new DevExpress.XtraEditors.SimpleButton();
            this.pnlItemsPanel = new DevExpress.XtraEditors.PanelControl();
            this.pnlItemsHeader = new DevExpress.XtraEditors.PanelControl();
            this.lblItemsTitle = new DevExpress.XtraEditors.LabelControl();
            this.btnAddLine = new DevExpress.XtraEditors.SimpleButton();
            this.btnDeleteLine = new DevExpress.XtraEditors.SimpleButton();
            this.gridLines = new DevExpress.XtraGrid.GridControl();
            this.gridLinesView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.pnlTotals = new DevExpress.XtraEditors.PanelControl();
            this.lblSubTotalTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblSubTotalValue = new DevExpress.XtraEditors.LabelControl();
            this.lblDiscountTotalTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblDiscountTotalValue = new DevExpress.XtraEditors.LabelControl();
            this.lblTaxTotalTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblTaxTotalValue = new DevExpress.XtraEditors.LabelControl();
            this.lblGrandTotalTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblGrandTotalValue = new DevExpress.XtraEditors.LabelControl();
            this.lblTaxBrkTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblTaxBreakdown = new System.Windows.Forms.Label();
            this.pnlFooter = new DevExpress.XtraEditors.PanelControl();
            this.pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            this.btnSaveDraft = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnApprove = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).BeginInit();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlHeaderLine)).BeginInit();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbInvoiceType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtInvoiceNumber.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpAccount.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpAccountView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlCatalog)).BeginInit();
            this.pnlCatalog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCatalogWarehouse.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCatalogWarehouseView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCatalog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCatalogView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlItemsPanel)).BeginInit();
            this.pnlItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlItemsHeader)).BeginInit();
            this.pnlItemsHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLines)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridLinesView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlTotals)).BeginInit();
            this.pnlTotals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooter)).BeginInit();
            this.pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooterLine)).BeginInit();
            this.SuspendLayout();

            // 
            // pnlHeader
            // 
            this.pnlHeader.Appearance.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            this.pnlHeader.Appearance.Options.UseBackColor = true;
            this.pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblHeaderIcon);
            this.pnlHeader.Controls.Add(this.pnlHeaderLine);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1420, 68);
            this.pnlHeader.TabIndex = 0;

            // 
            // lblHeaderIcon
            // 
            this.lblHeaderIcon.Location = new System.Drawing.Point(20, 18);
            this.lblHeaderIcon.Name = "lblHeaderIcon";
            this.lblHeaderIcon.Size = new System.Drawing.Size(32, 32);

            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Location = new System.Drawing.Point(62, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(89, 21);
            this.lblTitle.Text = "Yeni Fatura";

            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblSubtitle.Appearance.Options.UseFont = true;
            this.lblSubtitle.Appearance.Options.UseForeColor = true;
            this.lblSubtitle.Location = new System.Drawing.Point(62, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(340, 13);
            this.lblSubtitle.Text = "Fatura ve kalem bilgilerini eksiksiz doldurunuz";

            // 
            // pnlHeaderLine
            // 
            this.pnlHeaderLine.Appearance.BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.pnlHeaderLine.Appearance.Options.UseBackColor = true;
            this.pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlHeaderLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlHeaderLine.Location = new System.Drawing.Point(0, 67);
            this.pnlHeaderLine.Name = "pnlHeaderLine";
            this.pnlHeaderLine.Size = new System.Drawing.Size(1420, 1);

            // 
            // pnlBody
            // 
            this.pnlBody.Controls.Add(this.pnlItemsPanel);
            this.pnlBody.Controls.Add(this.pnlCatalog);
            this.pnlBody.Controls.Add(this.txtDescription);
            this.pnlBody.Controls.Add(this.lblDescLabel);
            this.pnlBody.Controls.Add(this.lblStatusValue);
            this.pnlBody.Controls.Add(this.lookUpAccount);
            this.pnlBody.Controls.Add(this.lblAccountLabel);
            this.pnlBody.Controls.Add(this.dtDate);
            this.pnlBody.Controls.Add(this.lblDateLabel);
            this.pnlBody.Controls.Add(this.txtInvoiceNumber);
            this.pnlBody.Controls.Add(this.lblNumberLabel);
            this.pnlBody.Controls.Add(this.cmbInvoiceType);
            this.pnlBody.Controls.Add(this.lblTypeLabel);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 68);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(20);
            this.pnlBody.Size = new System.Drawing.Size(1420, 592);
            this.pnlBody.TabIndex = 1;

            // 
            // lblTypeLabel
            // 
            this.lblTypeLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTypeLabel.Location = new System.Drawing.Point(20, 15);
            this.lblTypeLabel.Name = "lblTypeLabel";
            this.lblTypeLabel.Size = new System.Drawing.Size(65, 15);
            this.lblTypeLabel.Text = "Fatura Türü:";

            // 
            // cmbInvoiceType
            // 
            this.cmbInvoiceType.Location = new System.Drawing.Point(20, 35);
            this.cmbInvoiceType.Name = "cmbInvoiceType";
            this.cmbInvoiceType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbInvoiceType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbInvoiceType.Size = new System.Drawing.Size(150, 26);

            // 
            // lblNumberLabel
            // 
            this.lblNumberLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNumberLabel.Location = new System.Drawing.Point(185, 15);
            this.lblNumberLabel.Name = "lblNumberLabel";
            this.lblNumberLabel.Size = new System.Drawing.Size(58, 15);
            this.lblNumberLabel.Text = "Fatura No:";

            // 
            // txtInvoiceNumber
            // 
            this.txtInvoiceNumber.Location = new System.Drawing.Point(185, 35);
            this.txtInvoiceNumber.Name = "txtInvoiceNumber";
            this.txtInvoiceNumber.Size = new System.Drawing.Size(170, 26);

            // 
            // lblDateLabel
            // 
            this.lblDateLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDateLabel.Location = new System.Drawing.Point(370, 15);
            this.lblDateLabel.Name = "lblDateLabel";
            this.lblDateLabel.Size = new System.Drawing.Size(32, 15);
            this.lblDateLabel.Text = "Tarih:";

            // 
            // dtDate
            // 
            this.dtDate.EditValue = null;
            this.dtDate.Location = new System.Drawing.Point(370, 35);
            this.dtDate.Name = "dtDate";
            this.dtDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDate.Size = new System.Drawing.Size(140, 26);

            // 
            // lblAccountLabel
            // 
            this.lblAccountLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAccountLabel.Location = new System.Drawing.Point(525, 15);
            this.lblAccountLabel.Name = "lblAccountLabel";
            this.lblAccountLabel.Size = new System.Drawing.Size(117, 15);
            this.lblAccountLabel.Text = "Cari (Müşteri/Tedarikçi):";

            // 
            // lookUpAccount
            // 
            this.lookUpAccount.Location = new System.Drawing.Point(525, 35);
            this.lookUpAccount.Name = "lookUpAccount";
            this.lookUpAccount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpAccount.Properties.PopupView = this.lookUpAccountView;
            this.lookUpAccount.Size = new System.Drawing.Size(380, 26);

            // 
            // lblStatusValue
            // 
            this.lblStatusValue.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatusValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(30, 64, 175);
            this.lblStatusValue.Appearance.Options.UseFont = true;
            this.lblStatusValue.Appearance.Options.UseForeColor = true;
            this.lblStatusValue.Location = new System.Drawing.Point(1080, 38);
            this.lblStatusValue.Name = "lblStatusValue";
            this.lblStatusValue.Size = new System.Drawing.Size(120, 15);
            this.lblStatusValue.Text = "";

            // 
            // lblDescLabel
            // 
            this.lblDescLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDescLabel.Location = new System.Drawing.Point(20, 75);
            this.lblDescLabel.Name = "lblDescLabel";
            this.lblDescLabel.Size = new System.Drawing.Size(53, 15);
            this.lblDescLabel.Text = "Açıklama:";

            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(20, 95);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(1380, 26);

            // 
            // pnlCatalog
            // 
            this.pnlCatalog.Appearance.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
            this.pnlCatalog.Appearance.Options.UseBackColor = true;
            this.pnlCatalog.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.pnlCatalog.Controls.Add(this.btnAddProduct);
            this.pnlCatalog.Controls.Add(this.gridCatalog);
            this.pnlCatalog.Controls.Add(this.cmbCatalogWarehouse);
            this.pnlCatalog.Controls.Add(this.lblCatalogTitle);
            this.pnlCatalog.Location = new System.Drawing.Point(20, 135);
            this.pnlCatalog.Name = "pnlCatalog";
            this.pnlCatalog.Size = new System.Drawing.Size(500, 380);
            this.pnlCatalog.TabIndex = 5;

            // 
            // lblCatalogTitle
            // 
            this.lblCatalogTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCatalogTitle.Location = new System.Drawing.Point(5, 6);
            this.lblCatalogTitle.Name = "lblCatalogTitle";
            this.lblCatalogTitle.Size = new System.Drawing.Size(105, 17);
            this.lblCatalogTitle.Text = "Tanımlı Ürünler";

            // 
            // cmbCatalogWarehouse
            // 
            this.cmbCatalogWarehouse.Location = new System.Drawing.Point(5, 28);
            this.cmbCatalogWarehouse.Name = "cmbCatalogWarehouse";
            this.cmbCatalogWarehouse.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbCatalogWarehouse.Properties.NullText = "Depo (Tümü)";
            this.cmbCatalogWarehouse.Properties.PopupView = this.cmbCatalogWarehouseView;
            this.cmbCatalogWarehouse.Size = new System.Drawing.Size(490, 26);

            // 
            // gridCatalog
            // 
            this.gridCatalog.Location = new System.Drawing.Point(5, 62);
            this.gridCatalog.MainView = this.gridCatalogView;
            this.gridCatalog.Name = "gridCatalog";
            this.gridCatalog.Size = new System.Drawing.Size(490, 275);
            this.gridCatalog.TabIndex = 6;
            this.gridCatalog.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridCatalogView});

            // 
            // gridCatalogView
            // 
            this.gridCatalogView.GridControl = this.gridCatalog;
            this.gridCatalogView.Name = "gridCatalogView";
            this.gridCatalogView.OptionsBehavior.AutoPopulateColumns = false;
            this.gridCatalogView.OptionsBehavior.Editable = false;
            this.gridCatalogView.OptionsSelection.MultiSelect = false;
            this.gridCatalogView.OptionsView.ShowGroupPanel = false;
            this.gridCatalogView.RowHeight = 26;

            // 
            // btnAddProduct
            // 
            this.btnAddProduct.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddProduct.Location = new System.Drawing.Point(5, 345);
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Size = new System.Drawing.Size(490, 30);
            this.btnAddProduct.Text = "+ Ürünü Faturaya Ekle";

            // 
            // pnlItemsPanel
            // 
            this.pnlItemsPanel.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pnlItemsPanel.Appearance.Options.UseBackColor = true;
            this.pnlItemsPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlItemsPanel.Controls.Add(this.pnlTotals);
            this.pnlItemsPanel.Controls.Add(this.gridLines);
            this.pnlItemsPanel.Controls.Add(this.pnlItemsHeader);
            this.pnlItemsPanel.Location = new System.Drawing.Point(540, 135);
            this.pnlItemsPanel.Name = "pnlItemsPanel";
            this.pnlItemsPanel.Size = new System.Drawing.Size(860, 420);
            this.pnlItemsPanel.TabIndex = 7;

            // 
            // pnlItemsHeader
            // 
            this.pnlItemsHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlItemsHeader.Controls.Add(this.btnDeleteLine);
            this.pnlItemsHeader.Controls.Add(this.btnAddLine);
            this.pnlItemsHeader.Controls.Add(this.lblItemsTitle);
            this.pnlItemsHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlItemsHeader.Name = "pnlItemsHeader";
            this.pnlItemsHeader.Size = new System.Drawing.Size(860, 36);

            // 
            // lblItemsTitle
            // 
            this.lblItemsTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblItemsTitle.Location = new System.Drawing.Point(5, 8);
            this.lblItemsTitle.Name = "lblItemsTitle";
            this.lblItemsTitle.Size = new System.Drawing.Size(100, 17);
            this.lblItemsTitle.Text = "Fatura Kalemleri";

            // 
            // btnAddLine
            // 
            this.btnAddLine.Location = new System.Drawing.Point(640, 4);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new System.Drawing.Size(105, 28);
            this.btnAddLine.Text = "+ Satır Ekle";

            // 
            // btnDeleteLine
            // 
            this.btnDeleteLine.Location = new System.Drawing.Point(755, 4);
            this.btnDeleteLine.Name = "btnDeleteLine";
            this.btnDeleteLine.Size = new System.Drawing.Size(105, 28);
            this.btnDeleteLine.Text = "- Satır Sil";

            // 
            // gridLines
            // 
            this.gridLines.Location = new System.Drawing.Point(0, 40);
            this.gridLines.MainView = this.gridLinesView;
            this.gridLines.Name = "gridLines";
            this.gridLines.Size = new System.Drawing.Size(860, 250);
            this.gridLines.TabIndex = 4;
            this.gridLines.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridLinesView});

            // 
            // gridLinesView
            // 
            this.gridLinesView.GridControl = this.gridLines;
            this.gridLinesView.Name = "gridLinesView";
            this.gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
            this.gridLinesView.OptionsView.ShowGroupPanel = false;

            // 
            // pnlTotals
            // 
            this.pnlTotals.Appearance.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
            this.pnlTotals.Appearance.Options.UseBackColor = true;
            this.pnlTotals.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.pnlTotals.Controls.Add(this.lblTaxBreakdown);
            this.pnlTotals.Controls.Add(this.lblTaxBrkTitle);
            this.pnlTotals.Controls.Add(this.lblGrandTotalValue);
            this.pnlTotals.Controls.Add(this.lblGrandTotalTitle);
            this.pnlTotals.Controls.Add(this.lblTaxTotalValue);
            this.pnlTotals.Controls.Add(this.lblTaxTotalTitle);
            this.pnlTotals.Controls.Add(this.lblDiscountTotalValue);
            this.pnlTotals.Controls.Add(this.lblDiscountTotalTitle);
            this.pnlTotals.Controls.Add(this.lblSubTotalValue);
            this.pnlTotals.Controls.Add(this.lblSubTotalTitle);
            this.pnlTotals.Location = new System.Drawing.Point(0, 300);
            this.pnlTotals.Name = "pnlTotals";
            this.pnlTotals.Size = new System.Drawing.Size(860, 120);

            // 
            // lblSubTotalTitle
            // 
            this.lblSubTotalTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubTotalTitle.Location = new System.Drawing.Point(15, 8);
            this.lblSubTotalTitle.Name = "lblSubTotalTitle";
            this.lblSubTotalTitle.Size = new System.Drawing.Size(65, 15);
            this.lblSubTotalTitle.Text = "Ara Toplam:";

            // 
            // lblSubTotalValue
            // 
            this.lblSubTotalValue.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblSubTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblSubTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblSubTotalValue.Location = new System.Drawing.Point(105, 8);
            this.lblSubTotalValue.Name = "lblSubTotalValue";
            this.lblSubTotalValue.Size = new System.Drawing.Size(130, 15);
            this.lblSubTotalValue.Text = "0,00 ₺";

            // 
            // lblDiscountTotalTitle
            // 
            this.lblDiscountTotalTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDiscountTotalTitle.Location = new System.Drawing.Point(15, 30);
            this.lblDiscountTotalTitle.Name = "lblDiscountTotalTitle";
            this.lblDiscountTotalTitle.Size = new System.Drawing.Size(65, 15);
            this.lblDiscountTotalTitle.Text = "İskonto:";

            // 
            // lblDiscountTotalValue
            // 
            this.lblDiscountTotalValue.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblDiscountTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblDiscountTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblDiscountTotalValue.Location = new System.Drawing.Point(105, 30);
            this.lblDiscountTotalValue.Name = "lblDiscountTotalValue";
            this.lblDiscountTotalValue.Size = new System.Drawing.Size(130, 15);
            this.lblDiscountTotalValue.Text = "0,00 ₺";

            // 
            // lblTaxTotalTitle
            // 
            this.lblTaxTotalTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTaxTotalTitle.Location = new System.Drawing.Point(15, 52);
            this.lblTaxTotalTitle.Name = "lblTaxTotalTitle";
            this.lblTaxTotalTitle.Size = new System.Drawing.Size(64, 15);
            this.lblTaxTotalTitle.Text = "KDV Toplam:";

            // 
            // lblTaxTotalValue
            // 
            this.lblTaxTotalValue.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTaxTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTaxTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTaxTotalValue.Location = new System.Drawing.Point(105, 52);
            this.lblTaxTotalValue.Name = "lblTaxTotalValue";
            this.lblTaxTotalValue.Size = new System.Drawing.Size(130, 15);
            this.lblTaxTotalValue.Text = "0,00 ₺";

            // 
            // lblGrandTotalTitle
            // 
            this.lblGrandTotalTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblGrandTotalTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(30, 64, 175);
            this.lblGrandTotalTitle.Location = new System.Drawing.Point(15, 76);
            this.lblGrandTotalTitle.Name = "lblGrandTotalTitle";
            this.lblGrandTotalTitle.Size = new System.Drawing.Size(82, 17);
            this.lblGrandTotalTitle.Text = "Genel Toplam:";

            // 
            // lblGrandTotalValue
            // 
            this.lblGrandTotalValue.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblGrandTotalValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(30, 64, 175);
            this.lblGrandTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblGrandTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblGrandTotalValue.Location = new System.Drawing.Point(60, 96);
            this.lblGrandTotalValue.Name = "lblGrandTotalValue";
            this.lblGrandTotalValue.Size = new System.Drawing.Size(175, 20);
            this.lblGrandTotalValue.Text = "0,00 ₺";

            // 
            // lblTaxBrkTitle
            // 
            this.lblTaxBrkTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTaxBrkTitle.Location = new System.Drawing.Point(280, 10);
            this.lblTaxBrkTitle.Name = "lblTaxBrkTitle";
            this.lblTaxBrkTitle.Size = new System.Drawing.Size(80, 15);
            this.lblTaxBrkTitle.Text = "KDV Kırılımı:";

            // 
            // lblTaxBreakdown
            // 
            this.lblTaxBreakdown.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.lblTaxBreakdown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTaxBreakdown.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblTaxBreakdown.Location = new System.Drawing.Point(280, 28);
            this.lblTaxBreakdown.Name = "lblTaxBreakdown";
            this.lblTaxBreakdown.Size = new System.Drawing.Size(560, 80);
            this.lblTaxBreakdown.TabIndex = 9;
            this.lblTaxBreakdown.Text = "";

            // 
            // pnlFooter
            // 
            this.pnlFooter.Appearance.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            this.pnlFooter.Appearance.Options.UseBackColor = true;
            this.pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Controls.Add(this.btnSave);
            this.pnlFooter.Controls.Add(this.btnSaveDraft);
            this.pnlFooter.Controls.Add(this.btnApprove);
            this.pnlFooter.Controls.Add(this.pnlFooterLine);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 660);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1420, 60);
            this.pnlFooter.TabIndex = 2;

            // 
            // pnlFooterLine
            // 
            this.pnlFooterLine.Appearance.BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.pnlFooterLine.Appearance.Options.UseBackColor = true;
            this.pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlFooterLine.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFooterLine.Location = new System.Drawing.Point(0, 0);
            this.pnlFooterLine.Name = "pnlFooterLine";
            this.pnlFooterLine.Size = new System.Drawing.Size(1420, 1);

            // 
            // btnSaveDraft
            // 
            this.btnSaveDraft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveDraft.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSaveDraft.Location = new System.Drawing.Point(1028, 14);
            this.btnSaveDraft.Name = "btnSaveDraft";
            this.btnSaveDraft.Size = new System.Drawing.Size(130, 34);
            this.btnSaveDraft.Text = "Taslak Kaydet";

            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(1166, 14);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(130, 34);
            this.btnSave.Text = "Kaydet ve Onayla";

            // 
            // btnApprove
            // 
            this.btnApprove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApprove.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnApprove.Location = new System.Drawing.Point(1166, 14);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(130, 34);
            this.btnApprove.Text = "Faturayı Onayla";

            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Location = new System.Drawing.Point(1304, 14);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(94, 34);
            this.btnCancel.Text = "Vazgeç";

            // 
            // InvoiceEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 720);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InvoiceEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fatura";

            ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlHeaderLine)).EndInit();
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbInvoiceType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtInvoiceNumber.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpAccount.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpAccountView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlCatalog)).EndInit();
            this.pnlCatalog.ResumeLayout(false);
            this.pnlCatalog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCatalogWarehouse.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCatalogWarehouseView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCatalog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCatalogView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlItemsPanel)).EndInit();
            this.pnlItemsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlItemsHeader)).EndInit();
            this.pnlItemsHeader.ResumeLayout(false);
            this.pnlItemsHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLines)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridLinesView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlTotals)).EndInit();
            this.pnlTotals.ResumeLayout(false);
            this.pnlTotals.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooter)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooterLine)).EndInit();
            this.ResumeLayout(false);
        }
    }
}