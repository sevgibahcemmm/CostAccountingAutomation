namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    partial class CostSlipEditForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.LabelControl lblTypeLabel;
        private DevExpress.XtraEditors.ComboBoxEdit cmbCostSlipType;
        private DevExpress.XtraEditors.LabelControl lblNumberLabel;
        private DevExpress.XtraEditors.TextEdit txtSlipNumber;
        private DevExpress.XtraEditors.LabelControl lblDateLabel;
        private DevExpress.XtraEditors.DateEdit dtCostDate;
        private DevExpress.XtraEditors.LabelControl lblWorkshopLabel;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpWorkshop;
        private DevExpress.XtraGrid.Views.Grid.GridView lookUpWorkshopView;
        private DevExpress.XtraEditors.LabelControl lblStatusValue;
        private DevExpress.XtraEditors.LabelControl lblProductLabel;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpProducedProduct;
        private DevExpress.XtraGrid.Views.Grid.GridView lookUpProducedProductView;
        private DevExpress.XtraEditors.LabelControl lblCustomerLabel;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpCustomer;
        private DevExpress.XtraGrid.Views.Grid.GridView lookUpCustomerView;
        private DevExpress.XtraEditors.LabelControl lblQuantityLabel;
        private DevExpress.XtraEditors.TextEdit txtQuantity;
        private DevExpress.XtraEditors.LabelControl lblDescLabel;
        private DevExpress.XtraEditors.TextEdit txtDescription;
        private DevExpress.XtraEditors.PanelControl pnlItemsPanel;
        private DevExpress.XtraEditors.PanelControl pnlItemsHeader;
        private DevExpress.XtraEditors.LabelControl lblItemsTitle;
        private DevExpress.XtraEditors.SimpleButton btnAddLine;
        private DevExpress.XtraEditors.SimpleButton btnDeleteLine;
        private DevExpress.XtraGrid.GridControl gridLines;
        private DevExpress.XtraGrid.Views.Grid.GridView gridLinesView;
        private DevExpress.XtraEditors.PanelControl pnlSummary;
        private DevExpress.XtraEditors.LabelControl lblDirectTitle;
        private DevExpress.XtraEditors.LabelControl lblDirectValue;
        private DevExpress.XtraEditors.LabelControl lblOtherTitle;
        private DevExpress.XtraEditors.LabelControl lblOtherValue;
        private DevExpress.XtraEditors.LabelControl lblGrandTotalTitle;
        private DevExpress.XtraEditors.LabelControl lblGrandTotalValue;
        private System.Windows.Forms.Label lblBreakdownLabel;
        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private DevExpress.XtraEditors.SimpleButton btnSaveDraft;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnApprove;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private DevExpress.XtraEditors.SimpleButton btnPrintSlip;

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
            this.cmbCostSlipType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblNumberLabel = new DevExpress.XtraEditors.LabelControl();
            this.txtSlipNumber = new DevExpress.XtraEditors.TextEdit();
            this.lblDateLabel = new DevExpress.XtraEditors.LabelControl();
            this.dtCostDate = new DevExpress.XtraEditors.DateEdit();
            this.lblWorkshopLabel = new DevExpress.XtraEditors.LabelControl();
            this.lookUpWorkshop = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.lookUpWorkshopView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.lblStatusValue = new DevExpress.XtraEditors.LabelControl();
            this.lblProductLabel = new DevExpress.XtraEditors.LabelControl();
            this.lookUpProducedProduct = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.lookUpProducedProductView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.lblCustomerLabel = new DevExpress.XtraEditors.LabelControl();
            this.lookUpCustomer = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.lookUpCustomerView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.lblQuantityLabel = new DevExpress.XtraEditors.LabelControl();
            this.txtQuantity = new DevExpress.XtraEditors.TextEdit();
            this.lblDescLabel = new DevExpress.XtraEditors.LabelControl();
            this.txtDescription = new DevExpress.XtraEditors.TextEdit();
            this.pnlItemsPanel = new DevExpress.XtraEditors.PanelControl();
            this.pnlItemsHeader = new DevExpress.XtraEditors.PanelControl();
            this.lblItemsTitle = new DevExpress.XtraEditors.LabelControl();
            this.btnAddLine = new DevExpress.XtraEditors.SimpleButton();
            this.btnDeleteLine = new DevExpress.XtraEditors.SimpleButton();
            this.gridLines = new DevExpress.XtraGrid.GridControl();
            this.gridLinesView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.pnlSummary = new DevExpress.XtraEditors.PanelControl();
            this.lblDirectTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblDirectValue = new DevExpress.XtraEditors.LabelControl();
            this.lblOtherTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblOtherValue = new DevExpress.XtraEditors.LabelControl();
            this.lblGrandTotalTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblGrandTotalValue = new DevExpress.XtraEditors.LabelControl();
            this.lblBreakdownLabel = new System.Windows.Forms.Label();
            this.pnlFooter = new DevExpress.XtraEditors.PanelControl();
            this.pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            this.btnSaveDraft = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnApprove = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btnPrintSlip = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).BeginInit();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlHeaderLine)).BeginInit();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCostSlipType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSlipNumber.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtCostDate.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtCostDate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpWorkshop.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpWorkshopView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpProducedProduct.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpProducedProductView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpCustomer.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpCustomerView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQuantity.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlItemsPanel)).BeginInit();
            this.pnlItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlItemsHeader)).BeginInit();
            this.pnlItemsHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLines)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridLinesView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlSummary)).BeginInit();
            this.pnlSummary.SuspendLayout();
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
            this.lblTitle.Size = new System.Drawing.Size(140, 21);
            this.lblTitle.Text = "Yeni Maliyet Pusulası";

            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblSubtitle.Appearance.Options.UseFont = true;
            this.lblSubtitle.Appearance.Options.UseForeColor = true;
            this.lblSubtitle.Location = new System.Drawing.Point(62, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(380, 13);
            this.lblSubtitle.Text = "Pusula ve gider kalemi bilgilerini eksiksiz doldurunuz";

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
            this.pnlBody.Controls.Add(this.txtDescription);
            this.pnlBody.Controls.Add(this.lblDescLabel);
            this.pnlBody.Controls.Add(this.txtQuantity);
            this.pnlBody.Controls.Add(this.lblQuantityLabel);
            this.pnlBody.Controls.Add(this.lookUpCustomer);
            this.pnlBody.Controls.Add(this.lblCustomerLabel);
            this.pnlBody.Controls.Add(this.lookUpProducedProduct);
            this.pnlBody.Controls.Add(this.lblProductLabel);
            this.pnlBody.Controls.Add(this.lblStatusValue);
            this.pnlBody.Controls.Add(this.lookUpWorkshop);
            this.pnlBody.Controls.Add(this.lblWorkshopLabel);
            this.pnlBody.Controls.Add(this.dtCostDate);
            this.pnlBody.Controls.Add(this.lblDateLabel);
            this.pnlBody.Controls.Add(this.txtSlipNumber);
            this.pnlBody.Controls.Add(this.lblNumberLabel);
            this.pnlBody.Controls.Add(this.cmbCostSlipType);
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
            this.lblTypeLabel.Size = new System.Drawing.Size(70, 15);
            this.lblTypeLabel.Text = "Pusula Türü:";

            // 
            // cmbCostSlipType
            // 
            this.cmbCostSlipType.Location = new System.Drawing.Point(20, 35);
            this.cmbCostSlipType.Name = "cmbCostSlipType";
            this.cmbCostSlipType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbCostSlipType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbCostSlipType.Size = new System.Drawing.Size(150, 26);

            // 
            // lblNumberLabel
            // 
            this.lblNumberLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNumberLabel.Location = new System.Drawing.Point(185, 15);
            this.lblNumberLabel.Name = "lblNumberLabel";
            this.lblNumberLabel.Size = new System.Drawing.Size(62, 15);
            this.lblNumberLabel.Text = "Pusula No:";

            // 
            // txtSlipNumber
            // 
            this.txtSlipNumber.Location = new System.Drawing.Point(185, 35);
            this.txtSlipNumber.Name = "txtSlipNumber";
            this.txtSlipNumber.Size = new System.Drawing.Size(150, 26);

            // 
            // lblDateLabel
            // 
            this.lblDateLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDateLabel.Location = new System.Drawing.Point(350, 15);
            this.lblDateLabel.Name = "lblDateLabel";
            this.lblDateLabel.Size = new System.Drawing.Size(32, 15);
            this.lblDateLabel.Text = "Tarih:";

            // 
            // dtCostDate
            // 
            this.dtCostDate.EditValue = null;
            this.dtCostDate.Location = new System.Drawing.Point(350, 35);
            this.dtCostDate.Name = "dtCostDate";
            this.dtCostDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtCostDate.Size = new System.Drawing.Size(110, 26);

            // 
            // lblWorkshopLabel
            // 
            this.lblWorkshopLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWorkshopLabel.Location = new System.Drawing.Point(475, 15);
            this.lblWorkshopLabel.Name = "lblWorkshopLabel";
            this.lblWorkshopLabel.Size = new System.Drawing.Size(40, 15);
            this.lblWorkshopLabel.Text = "Atölye:";

            // 
            // lookUpWorkshop
            // 
            this.lookUpWorkshop.Location = new System.Drawing.Point(475, 35);
            this.lookUpWorkshop.Name = "lookUpWorkshop";
            this.lookUpWorkshop.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpWorkshop.Properties.PopupView = this.lookUpWorkshopView;
            this.lookUpWorkshop.Size = new System.Drawing.Size(300, 26);

            // 
            // lblStatusValue
            // 
            this.lblStatusValue.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatusValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(30, 64, 175);
            this.lblStatusValue.Appearance.Options.UseFont = true;
            this.lblStatusValue.Appearance.Options.UseForeColor = true;
            this.lblStatusValue.Location = new System.Drawing.Point(1050, 38);
            this.lblStatusValue.Name = "lblStatusValue";
            this.lblStatusValue.Size = new System.Drawing.Size(150, 15);
            this.lblStatusValue.Text = "";

            // 
            // lblProductLabel
            // 
            this.lblProductLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProductLabel.Location = new System.Drawing.Point(20, 75);
            this.lblProductLabel.Name = "lblProductLabel";
            this.lblProductLabel.Size = new System.Drawing.Size(77, 15);
            this.lblProductLabel.Text = "Üretilen Ürün:";

            // 
            // lookUpProducedProduct
            // 
            this.lookUpProducedProduct.Location = new System.Drawing.Point(20, 95);
            this.lookUpProducedProduct.Name = "lookUpProducedProduct";
            this.lookUpProducedProduct.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpProducedProduct.Properties.PopupView = this.lookUpProducedProductView;
            this.lookUpProducedProduct.Size = new System.Drawing.Size(380, 26);

            // 
            // lblCustomerLabel
            // 
            this.lblCustomerLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCustomerLabel.Location = new System.Drawing.Point(420, 75);
            this.lblCustomerLabel.Name = "lblCustomerLabel";
            this.lblCustomerLabel.Size = new System.Drawing.Size(48, 15);
            this.lblCustomerLabel.Text = "Müşteri:";

            // 
            // lookUpCustomer
            // 
            this.lookUpCustomer.Location = new System.Drawing.Point(420, 95);
            this.lookUpCustomer.Name = "lookUpCustomer";
            this.lookUpCustomer.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpCustomer.Properties.PopupView = this.lookUpCustomerView;
            this.lookUpCustomer.Size = new System.Drawing.Size(320, 26);

            // 
            // lblQuantityLabel
            // 
            this.lblQuantityLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblQuantityLabel.Location = new System.Drawing.Point(760, 75);
            this.lblQuantityLabel.Name = "lblQuantityLabel";
            this.lblQuantityLabel.Size = new System.Drawing.Size(40, 15);
            this.lblQuantityLabel.Text = "Miktar:";

            // 
            // txtQuantity
            // 
            this.txtQuantity.Location = new System.Drawing.Point(760, 95);
            this.txtQuantity.Name = "txtQuantity";
            this.txtQuantity.Size = new System.Drawing.Size(90, 26);

            // 
            // lblDescLabel
            // 
            this.lblDescLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDescLabel.Location = new System.Drawing.Point(20, 135);
            this.lblDescLabel.Name = "lblDescLabel";
            this.lblDescLabel.Size = new System.Drawing.Size(53, 15);
            this.lblDescLabel.Text = "Açıklama:";

            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(20, 155);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(1380, 26);

            // 
            // pnlItemsPanel
            // 
            this.pnlItemsPanel.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.pnlItemsPanel.Appearance.Options.UseBackColor = true;
            this.pnlItemsPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlItemsPanel.Controls.Add(this.pnlSummary);
            this.pnlItemsPanel.Controls.Add(this.gridLines);
            this.pnlItemsPanel.Controls.Add(this.pnlItemsHeader);
            this.pnlItemsPanel.Location = new System.Drawing.Point(20, 195);
            this.pnlItemsPanel.Name = "pnlItemsPanel";
            this.pnlItemsPanel.Size = new System.Drawing.Size(1380, 345);
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
            this.pnlItemsHeader.Size = new System.Drawing.Size(1380, 36);

            // 
            // lblItemsTitle
            // 
            this.lblItemsTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblItemsTitle.Location = new System.Drawing.Point(5, 8);
            this.lblItemsTitle.Name = "lblItemsTitle";
            this.lblItemsTitle.Size = new System.Drawing.Size(110, 17);
            this.lblItemsTitle.Text = "Gider Kalemleri";

            // 
            // btnAddLine
            // 
            this.btnAddLine.Location = new System.Drawing.Point(1160, 4);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new System.Drawing.Size(105, 28);
            this.btnAddLine.Text = "+ Satır Ekle";

            // 
            // btnDeleteLine
            // 
            this.btnDeleteLine.Location = new System.Drawing.Point(1275, 4);
            this.btnDeleteLine.Name = "btnDeleteLine";
            this.btnDeleteLine.Size = new System.Drawing.Size(105, 28);
            this.btnDeleteLine.Text = "- Satır Sil";

            // 
            // gridLines
            // 
            this.gridLines.Location = new System.Drawing.Point(0, 40);
            this.gridLines.MainView = this.gridLinesView;
            this.gridLines.Name = "gridLines";
            this.gridLines.Size = new System.Drawing.Size(1380, 205);
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
            // pnlSummary
            // 
            this.pnlSummary.Appearance.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
            this.pnlSummary.Appearance.Options.UseBackColor = true;
            this.pnlSummary.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.pnlSummary.Controls.Add(this.lblBreakdownLabel);
            this.pnlSummary.Controls.Add(this.lblGrandTotalValue);
            this.pnlSummary.Controls.Add(this.lblGrandTotalTitle);
            this.pnlSummary.Controls.Add(this.lblOtherValue);
            this.pnlSummary.Controls.Add(this.lblOtherTitle);
            this.pnlSummary.Controls.Add(this.lblDirectValue);
            this.pnlSummary.Controls.Add(this.lblDirectTitle);
            this.pnlSummary.Location = new System.Drawing.Point(0, 255);
            this.pnlSummary.Name = "pnlSummary";
            this.pnlSummary.Size = new System.Drawing.Size(1380, 90);

            // 
            // lblDirectTitle
            // 
            this.lblDirectTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDirectTitle.Location = new System.Drawing.Point(15, 10);
            this.lblDirectTitle.Name = "lblDirectTitle";
            this.lblDirectTitle.Size = new System.Drawing.Size(90, 15);
            this.lblDirectTitle.Text = "Direkt Giderler:";

            // 
            // lblDirectValue
            // 
            this.lblDirectValue.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDirectValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblDirectValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblDirectValue.Location = new System.Drawing.Point(210, 10);
            this.lblDirectValue.Name = "lblDirectValue";
            this.lblDirectValue.Size = new System.Drawing.Size(180, 17);
            this.lblDirectValue.Text = "0,00 ₺";

            // 
            // lblOtherTitle
            // 
            this.lblOtherTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblOtherTitle.Location = new System.Drawing.Point(15, 34);
            this.lblOtherTitle.Name = "lblOtherTitle";
            this.lblOtherTitle.Size = new System.Drawing.Size(90, 15);
            this.lblOtherTitle.Text = "Diğer Giderler:";

            // 
            // lblOtherValue
            // 
            this.lblOtherValue.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblOtherValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblOtherValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblOtherValue.Location = new System.Drawing.Point(210, 34);
            this.lblOtherValue.Name = "lblOtherValue";
            this.lblOtherValue.Size = new System.Drawing.Size(180, 17);
            this.lblOtherValue.Text = "0,00 ₺";

            // 
            // lblGrandTotalTitle
            // 
            this.lblGrandTotalTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblGrandTotalTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(30, 64, 175);
            this.lblGrandTotalTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblGrandTotalTitle.Location = new System.Drawing.Point(15, 58);
            this.lblGrandTotalTitle.Name = "lblGrandTotalTitle";
            this.lblGrandTotalTitle.Size = new System.Drawing.Size(110, 18);
            this.lblGrandTotalTitle.Text = "Genel Toplam:";

            // 
            // lblGrandTotalValue
            // 
            this.lblGrandTotalValue.Appearance.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);
            this.lblGrandTotalValue.Appearance.ForeColor = System.Drawing.Color.FromArgb(30, 64, 175);
            this.lblGrandTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblGrandTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblGrandTotalValue.Location = new System.Drawing.Point(210, 58);
            this.lblGrandTotalValue.Name = "lblGrandTotalValue";
            this.lblGrandTotalValue.Size = new System.Drawing.Size(200, 20);
            this.lblGrandTotalValue.Text = "0,00 ₺";

            // 
            // lblBreakdownLabel
            // 
            this.lblBreakdownLabel.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.lblBreakdownLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBreakdownLabel.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblBreakdownLabel.Location = new System.Drawing.Point(440, 10);
            this.lblBreakdownLabel.Name = "lblBreakdownLabel";
            this.lblBreakdownLabel.Size = new System.Drawing.Size(925, 70);
            this.lblBreakdownLabel.TabIndex = 9;
            this.lblBreakdownLabel.Text = "";

            // 
            // pnlFooter
            // 
            this.pnlFooter.Appearance.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            this.pnlFooter.Appearance.Options.UseBackColor = true;
            this.pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlFooter.Controls.Add(this.btnPrintSlip);
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
            // btnPrintSlip
            // 
            this.btnPrintSlip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPrintSlip.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPrintSlip.Enabled = false;
            this.btnPrintSlip.Location = new System.Drawing.Point(15, 14);
            this.btnPrintSlip.Name = "btnPrintSlip";
            this.btnPrintSlip.Size = new System.Drawing.Size(200, 34);
            this.btnPrintSlip.Text = "Maliyet Pusulası Yazdır";

            // 
            // btnSaveDraft
            // 
            this.btnSaveDraft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveDraft.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSaveDraft.Location = new System.Drawing.Point(1010, 14);
            this.btnSaveDraft.Name = "btnSaveDraft";
            this.btnSaveDraft.Size = new System.Drawing.Size(130, 34);
            this.btnSaveDraft.Text = "Taslak Kaydet";

            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(1148, 14);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(140, 34);
            this.btnSave.Text = "Kaydet ve Onayla";

            // 
            // btnApprove
            // 
            this.btnApprove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApprove.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnApprove.Location = new System.Drawing.Point(1148, 14);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(140, 34);
            this.btnApprove.Text = "Yazdır";
            this.btnApprove.Visible = false;

            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Location = new System.Drawing.Point(1296, 14);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(94, 34);
            this.btnCancel.Text = "Vazgeç";

            // 
            // CostSlipEditForm
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
            this.Name = "CostSlipEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Maliyet Pusulası";

            ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlHeaderLine)).EndInit();
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCostSlipType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSlipNumber.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtCostDate.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtCostDate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpWorkshop.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpWorkshopView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpProducedProduct.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpProducedProductView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpCustomer.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpCustomerView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQuantity.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlItemsPanel)).EndInit();
            this.pnlItemsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlItemsHeader)).EndInit();
            this.pnlItemsHeader.ResumeLayout(false);
            this.pnlItemsHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLines)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridLinesView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlSummary)).EndInit();
            this.pnlSummary.ResumeLayout(false);
            this.pnlSummary.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooter)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooterLine)).EndInit();
            this.ResumeLayout(false);
        }
    }
}