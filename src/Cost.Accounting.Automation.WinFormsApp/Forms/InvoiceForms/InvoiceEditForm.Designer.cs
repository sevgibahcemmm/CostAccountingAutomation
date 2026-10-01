using Cost.Accounting.Automation.WinFormsApp.Utils;

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
        private DevExpress.XtraEditors.TextEdit txtCatalogProductSearch;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InvoiceEditForm));
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            pnlBody = new Panel();
            pnlItemsPanel = new DevExpress.XtraEditors.PanelControl();
            pnlTotals = new DevExpress.XtraEditors.PanelControl();
            lblTaxBreakdown = new Label();
            lblTaxBrkTitle = new DevExpress.XtraEditors.LabelControl();
            lblGrandTotalValue = new DevExpress.XtraEditors.LabelControl();
            lblGrandTotalTitle = new DevExpress.XtraEditors.LabelControl();
            lblTaxTotalValue = new DevExpress.XtraEditors.LabelControl();
            lblTaxTotalTitle = new DevExpress.XtraEditors.LabelControl();
            lblDiscountTotalValue = new DevExpress.XtraEditors.LabelControl();
            lblDiscountTotalTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubTotalValue = new DevExpress.XtraEditors.LabelControl();
            lblSubTotalTitle = new DevExpress.XtraEditors.LabelControl();
            gridLines = new DevExpress.XtraGrid.GridControl();
            gridLinesView = new DevExpress.XtraGrid.Views.Grid.GridView();
            pnlItemsHeader = new DevExpress.XtraEditors.PanelControl();
            btnDeleteLine = new DevExpress.XtraEditors.SimpleButton();
            btnAddLine = new DevExpress.XtraEditors.SimpleButton();
            lblItemsTitle = new DevExpress.XtraEditors.LabelControl();
            pnlCatalog = new DevExpress.XtraEditors.PanelControl();
            btnAddProduct = new DevExpress.XtraEditors.SimpleButton();
            txtCatalogProductSearch = new DevExpress.XtraEditors.TextEdit();
            gridCatalog = new DevExpress.XtraGrid.GridControl();
            gridCatalogView = new DevExpress.XtraGrid.Views.Grid.GridView();
            cmbCatalogWarehouse = new DevExpress.XtraEditors.SearchLookUpEdit();
            cmbCatalogWarehouseView = new DevExpress.XtraGrid.Views.Grid.GridView();
            lblCatalogTitle = new DevExpress.XtraEditors.LabelControl();
            txtDescription = new DevExpress.XtraEditors.TextEdit();
            lblDescLabel = new DevExpress.XtraEditors.LabelControl();
            lblStatusValue = new DevExpress.XtraEditors.LabelControl();
            lookUpAccount = new DevExpress.XtraEditors.SearchLookUpEdit();
            lookUpAccountView = new DevExpress.XtraGrid.Views.Grid.GridView();
            lblAccountLabel = new DevExpress.XtraEditors.LabelControl();
            dtDate = new DevExpress.XtraEditors.DateEdit();
            lblDateLabel = new DevExpress.XtraEditors.LabelControl();
            txtInvoiceNumber = new DevExpress.XtraEditors.TextEdit();
            lblNumberLabel = new DevExpress.XtraEditors.LabelControl();
            cmbInvoiceType = new DevExpress.XtraEditors.ComboBoxEdit();
            lblTypeLabel = new DevExpress.XtraEditors.LabelControl();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            btnPrintSlip = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            btnSaveDraft = new DevExpress.XtraEditors.SimpleButton();
            pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlItemsPanel).BeginInit();
            pnlItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlTotals).BeginInit();
            pnlTotals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridLines).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridLinesView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlItemsHeader).BeginInit();
            pnlItemsHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlCatalog).BeginInit();
            pnlCatalog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtCatalogProductSearch.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridCatalog).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridCatalogView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbCatalogWarehouse.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbCatalogWarehouseView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtDescription.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookUpAccount.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookUpAccountView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtDate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtDate.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtInvoiceNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbInvoiceType.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Appearance.BackColor = Color.FromArgb(248, 249, 250);
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1217, 59);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(107, 114, 128);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(53, 33);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(238, 13);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "Fatura ve kalem bilgilerini eksiksiz doldurunuz";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(17, 24, 39);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Location = new Point(53, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(80, 21);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Yeni Fatura";
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblHeaderIcon.ImageOptions.SvgImage");
            lblHeaderIcon.Location = new Point(17, 16);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(32, 32);
            lblHeaderIcon.TabIndex = 2;
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.BackColor = Color.FromArgb(229, 231, 235);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = DockStyle.Bottom;
            pnlHeaderLine.Location = new Point(0, 58);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new Size(1217, 1);
            pnlHeaderLine.TabIndex = 3;
            // 
            // pnlBody
            // 
            pnlBody.Controls.Add(pnlItemsPanel);
            pnlBody.Controls.Add(pnlCatalog);
            pnlBody.Controls.Add(txtDescription);
            pnlBody.Controls.Add(lblDescLabel);
            pnlBody.Controls.Add(lblStatusValue);
            pnlBody.Controls.Add(lookUpAccount);
            pnlBody.Controls.Add(lblAccountLabel);
            pnlBody.Controls.Add(dtDate);
            pnlBody.Controls.Add(lblDateLabel);
            pnlBody.Controls.Add(txtInvoiceNumber);
            pnlBody.Controls.Add(lblNumberLabel);
            pnlBody.Controls.Add(cmbInvoiceType);
            pnlBody.Controls.Add(lblTypeLabel);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(0, 59);
            pnlBody.Name = "pnlBody";
            pnlBody.Padding = new Padding(17);
            pnlBody.Size = new Size(1217, 616);
            pnlBody.TabIndex = 1;
            // 
            // pnlItemsPanel
            // 
            pnlItemsPanel.Appearance.BackColor = Color.Transparent;
            pnlItemsPanel.Appearance.Options.UseBackColor = true;
            pnlItemsPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlItemsPanel.Controls.Add(pnlTotals);
            pnlItemsPanel.Controls.Add(gridLines);
            pnlItemsPanel.Controls.Add(pnlItemsHeader);
            pnlItemsPanel.Location = new Point(463, 97);
            pnlItemsPanel.Name = "pnlItemsPanel";
            pnlItemsPanel.Size = new Size(737, 513);
            pnlItemsPanel.TabIndex = 7;
            // 
            // pnlTotals
            // 
            pnlTotals.Appearance.BackColor = Color.FromArgb(249, 250, 251);
            pnlTotals.Appearance.Options.UseBackColor = true;
            pnlTotals.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlTotals.Controls.Add(lblTaxBreakdown);
            pnlTotals.Controls.Add(lblTaxBrkTitle);
            pnlTotals.Controls.Add(lblGrandTotalValue);
            pnlTotals.Controls.Add(lblGrandTotalTitle);
            pnlTotals.Controls.Add(lblTaxTotalValue);
            pnlTotals.Controls.Add(lblTaxTotalTitle);
            pnlTotals.Controls.Add(lblDiscountTotalValue);
            pnlTotals.Controls.Add(lblDiscountTotalTitle);
            pnlTotals.Controls.Add(lblSubTotalValue);
            pnlTotals.Controls.Add(lblSubTotalTitle);
            pnlTotals.Location = new Point(3, 389);
            pnlTotals.Name = "pnlTotals";
            pnlTotals.Size = new Size(733, 104);
            pnlTotals.TabIndex = 0;
            // 
            // lblTaxBreakdown
            // 
            lblTaxBreakdown.BackColor = Color.FromArgb(255, 255, 255);
            lblTaxBreakdown.BorderStyle = BorderStyle.FixedSingle;
            lblTaxBreakdown.Font = new Font("Consolas", 9F);
            lblTaxBreakdown.Location = new Point(240, 24);
            lblTaxBreakdown.Name = "lblTaxBreakdown";
            lblTaxBreakdown.Size = new Size(480, 70);
            lblTaxBreakdown.TabIndex = 9;
            // 
            // lblTaxBrkTitle
            // 
            lblTaxBrkTitle.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTaxBrkTitle.Appearance.Options.UseFont = true;
            lblTaxBrkTitle.Location = new Point(240, 9);
            lblTaxBrkTitle.Name = "lblTaxBrkTitle";
            lblTaxBrkTitle.Size = new Size(70, 15);
            lblTaxBrkTitle.TabIndex = 10;
            lblTaxBrkTitle.Text = "KDV Kırılımı:";
            // 
            // lblGrandTotalValue
            // 
            lblGrandTotalValue.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblGrandTotalValue.Appearance.ForeColor = Color.FromArgb(30, 64, 175);
            lblGrandTotalValue.Appearance.Options.UseFont = true;
            lblGrandTotalValue.Appearance.Options.UseForeColor = true;
            lblGrandTotalValue.Appearance.Options.UseTextOptions = true;
            lblGrandTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblGrandTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblGrandTotalValue.Location = new Point(99, 66);
            lblGrandTotalValue.Name = "lblGrandTotalValue";
            lblGrandTotalValue.Size = new Size(103, 17);
            lblGrandTotalValue.TabIndex = 11;
            lblGrandTotalValue.Text = "0,00 ₺";
            // 
            // lblGrandTotalTitle
            // 
            lblGrandTotalTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGrandTotalTitle.Appearance.ForeColor = Color.FromArgb(30, 64, 175);
            lblGrandTotalTitle.Appearance.Options.UseFont = true;
            lblGrandTotalTitle.Appearance.Options.UseForeColor = true;
            lblGrandTotalTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblGrandTotalTitle.Location = new Point(13, 66);
            lblGrandTotalTitle.Name = "lblGrandTotalTitle";
            lblGrandTotalTitle.Size = new Size(86, 15);
            lblGrandTotalTitle.TabIndex = 12;
            lblGrandTotalTitle.Text = "Genel Toplam:";
            // 
            // lblTaxTotalValue
            // 
            lblTaxTotalValue.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblTaxTotalValue.Appearance.Options.UseFont = true;
            lblTaxTotalValue.Appearance.Options.UseTextOptions = true;
            lblTaxTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblTaxTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblTaxTotalValue.Location = new Point(90, 45);
            lblTaxTotalValue.Name = "lblTaxTotalValue";
            lblTaxTotalValue.Size = new Size(111, 13);
            lblTaxTotalValue.TabIndex = 13;
            lblTaxTotalValue.Text = "0,00 ₺";
            // 
            // lblTaxTotalTitle
            // 
            lblTaxTotalTitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblTaxTotalTitle.Appearance.Options.UseFont = true;
            lblTaxTotalTitle.Location = new Point(13, 45);
            lblTaxTotalTitle.Name = "lblTaxTotalTitle";
            lblTaxTotalTitle.Size = new Size(69, 15);
            lblTaxTotalTitle.TabIndex = 14;
            lblTaxTotalTitle.Text = "KDV Toplam:";
            // 
            // lblDiscountTotalValue
            // 
            lblDiscountTotalValue.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblDiscountTotalValue.Appearance.Options.UseFont = true;
            lblDiscountTotalValue.Appearance.Options.UseTextOptions = true;
            lblDiscountTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblDiscountTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblDiscountTotalValue.Location = new Point(90, 26);
            lblDiscountTotalValue.Name = "lblDiscountTotalValue";
            lblDiscountTotalValue.Size = new Size(111, 13);
            lblDiscountTotalValue.TabIndex = 15;
            lblDiscountTotalValue.Text = "0,00 ₺";
            // 
            // lblDiscountTotalTitle
            // 
            lblDiscountTotalTitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblDiscountTotalTitle.Appearance.Options.UseFont = true;
            lblDiscountTotalTitle.Location = new Point(13, 26);
            lblDiscountTotalTitle.Name = "lblDiscountTotalTitle";
            lblDiscountTotalTitle.Size = new Size(42, 15);
            lblDiscountTotalTitle.TabIndex = 16;
            lblDiscountTotalTitle.Text = "İskonto:";
            // 
            // lblSubTotalValue
            // 
            lblSubTotalValue.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblSubTotalValue.Appearance.Options.UseFont = true;
            lblSubTotalValue.Appearance.Options.UseTextOptions = true;
            lblSubTotalValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblSubTotalValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSubTotalValue.Location = new Point(99, 7);
            lblSubTotalValue.Name = "lblSubTotalValue";
            lblSubTotalValue.Size = new Size(111, 13);
            lblSubTotalValue.TabIndex = 17;
            lblSubTotalValue.Text = "0,00 ₺";
            // 
            // lblSubTotalTitle
            // 
            lblSubTotalTitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblSubTotalTitle.Appearance.Options.UseFont = true;
            lblSubTotalTitle.Location = new Point(13, 7);
            lblSubTotalTitle.Name = "lblSubTotalTitle";
            lblSubTotalTitle.Size = new Size(72, 15);
            lblSubTotalTitle.TabIndex = 18;
            lblSubTotalTitle.Text = "KDV'siz Tutar:";
            // 
            // gridLines
            // 
            gridLines.Location = new Point(0, 35);
            gridLines.MainView = gridLinesView;
            gridLines.Name = "gridLines";
            gridLines.Size = new Size(737, 348);
            gridLines.TabIndex = 4;
            gridLines.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridLinesView });
            // 
            // gridLinesView
            // 
            gridLinesView.DetailHeight = 303;
            gridLinesView.GridControl = gridLines;
            gridLinesView.Name = "gridLinesView";
            gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
            gridLinesView.OptionsEditForm.PopupEditFormWidth = 686;
            gridLinesView.OptionsView.ShowGroupPanel = false;
            // 
            // pnlItemsHeader
            // 
            pnlItemsHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlItemsHeader.Controls.Add(btnDeleteLine);
            pnlItemsHeader.Controls.Add(btnAddLine);
            pnlItemsHeader.Controls.Add(lblItemsTitle);
            pnlItemsHeader.Location = new Point(0, 0);
            pnlItemsHeader.Name = "pnlItemsHeader";
            pnlItemsHeader.Size = new Size(737, 29);
            pnlItemsHeader.TabIndex = 5;
            // 
            // btnDeleteLine
            // 
            btnDeleteLine.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnDeleteLine.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnDeleteLine.ImageOptions.SvgImage");
            btnDeleteLine.ImageOptions.SvgImageSize = new Size(16, 16);
            btnDeleteLine.Location = new Point(647, 3);
            btnDeleteLine.Name = "btnDeleteLine";
            btnDeleteLine.Size = new Size(90, 24);
            btnDeleteLine.TabIndex = 0;
            btnDeleteLine.Text = "- Satır Sil";
            // 
            // btnAddLine
            // 
            btnAddLine.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnAddLine.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnAddLine.ImageOptions.SvgImage");
            btnAddLine.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddLine.Location = new Point(549, 3);
            btnAddLine.Name = "btnAddLine";
            btnAddLine.Size = new Size(90, 24);
            btnAddLine.TabIndex = 1;
            btnAddLine.Text = "+ Satır Ekle";
            // 
            // lblItemsTitle
            // 
            lblItemsTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblItemsTitle.Appearance.Options.UseFont = true;
            lblItemsTitle.Location = new Point(4, 7);
            lblItemsTitle.Name = "lblItemsTitle";
            lblItemsTitle.Size = new Size(101, 17);
            lblItemsTitle.TabIndex = 2;
            lblItemsTitle.Text = "Fatura Kalemleri";
            // 
            // pnlCatalog
            // 
            pnlCatalog.Appearance.BackColor = Color.FromArgb(249, 250, 251);
            pnlCatalog.Appearance.Options.UseBackColor = true;
            pnlCatalog.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlCatalog.Controls.Add(btnAddProduct);
            pnlCatalog.Controls.Add(txtCatalogProductSearch);
            pnlCatalog.Controls.Add(gridCatalog);
            pnlCatalog.Controls.Add(cmbCatalogWarehouse);
            pnlCatalog.Controls.Add(lblCatalogTitle);
            pnlCatalog.Location = new Point(17, 97);
            pnlCatalog.Name = "pnlCatalog";
            pnlCatalog.Size = new Size(429, 513);
            pnlCatalog.TabIndex = 5;
            // 
            // btnAddProduct
            // 
            btnAddProduct.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnAddProduct.Appearance.Options.UseFont = true;
            btnAddProduct.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnAddProduct.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnAddProduct.ImageOptions.SvgImage");
            btnAddProduct.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddProduct.Location = new Point(5, 462);
            btnAddProduct.Name = "btnAddProduct";
            btnAddProduct.Size = new Size(420, 26);
            btnAddProduct.TabIndex = 0;
            btnAddProduct.Text = "+ Ürünü Faturaya Ekle";
            // 
            // txtCatalogProductSearch
            // 
            txtCatalogProductSearch.Location = new Point(4, 50);
            txtCatalogProductSearch.Name = "txtCatalogProductSearch";
            txtCatalogProductSearch.Properties.NullText = "Ürün ara (ad, kod veya barkod)";
            txtCatalogProductSearch.Size = new Size(420, 20);
            txtCatalogProductSearch.TabIndex = 1;
            // 
            // gridCatalog
            // 
            gridCatalog.Location = new Point(4, 80);
            gridCatalog.MainView = gridCatalogView;
            gridCatalog.Name = "gridCatalog";
            gridCatalog.Size = new Size(420, 376);
            gridCatalog.TabIndex = 6;
            gridCatalog.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridCatalogView });
            // 
            // gridCatalogView
            // 
            gridCatalogView.DetailHeight = 303;
            gridCatalogView.GridControl = gridCatalog;
            gridCatalogView.Name = "gridCatalogView";
            gridCatalogView.OptionsBehavior.AutoPopulateColumns = false;
            gridCatalogView.OptionsBehavior.Editable = false;
            gridCatalogView.OptionsEditForm.PopupEditFormWidth = 686;
            gridCatalogView.OptionsView.ShowGroupPanel = false;
            gridCatalogView.RowHeight = 23;
            // 
            // cmbCatalogWarehouse
            // 
            cmbCatalogWarehouse.Location = new Point(4, 24);
            cmbCatalogWarehouse.Name = "cmbCatalogWarehouse";
            cmbCatalogWarehouse.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbCatalogWarehouse.Properties.NullText = "Depo (Tümü)";
            cmbCatalogWarehouse.Properties.PopupView = cmbCatalogWarehouseView;
            cmbCatalogWarehouse.Size = new Size(420, 20);
            cmbCatalogWarehouse.TabIndex = 7;
            // 
            // cmbCatalogWarehouseView
            // 
            cmbCatalogWarehouseView.DetailHeight = 303;
            cmbCatalogWarehouseView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            cmbCatalogWarehouseView.Name = "cmbCatalogWarehouseView";
            cmbCatalogWarehouseView.OptionsEditForm.PopupEditFormWidth = 686;
            cmbCatalogWarehouseView.OptionsSelection.EnableAppearanceFocusedCell = false;
            cmbCatalogWarehouseView.OptionsView.ShowGroupPanel = false;
            // 
            // lblCatalogTitle
            // 
            lblCatalogTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCatalogTitle.Appearance.Options.UseFont = true;
            lblCatalogTitle.Location = new Point(4, 5);
            lblCatalogTitle.Name = "lblCatalogTitle";
            lblCatalogTitle.Size = new Size(97, 17);
            lblCatalogTitle.TabIndex = 8;
            lblCatalogTitle.Text = "Tanımlı Ürünler";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(17, 71);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(1183, 20);
            txtDescription.TabIndex = 8;
            // 
            // lblDescLabel
            // 
            lblDescLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDescLabel.Appearance.Options.UseFont = true;
            lblDescLabel.Location = new Point(17, 54);
            lblDescLabel.Name = "lblDescLabel";
            lblDescLabel.Size = new Size(53, 15);
            lblDescLabel.TabIndex = 9;
            lblDescLabel.Text = "Açıklama:";
            // 
            // lblStatusValue
            // 
            lblStatusValue.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblStatusValue.Appearance.ForeColor = Color.FromArgb(30, 64, 175);
            lblStatusValue.Appearance.Options.UseFont = true;
            lblStatusValue.Appearance.Options.UseForeColor = true;
            lblStatusValue.Location = new Point(926, 33);
            lblStatusValue.Name = "lblStatusValue";
            lblStatusValue.Size = new Size(0, 15);
            lblStatusValue.TabIndex = 10;
            // 
            // lookUpAccount
            // 
            lookUpAccount.Location = new Point(450, 30);
            lookUpAccount.Name = "lookUpAccount";
            lookUpAccount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            lookUpAccount.Properties.PopupView = lookUpAccountView;
            lookUpAccount.Size = new Size(747, 20);
            lookUpAccount.TabIndex = 11;
            // 
            // lookUpAccountView
            // 
            lookUpAccountView.DetailHeight = 303;
            lookUpAccountView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            lookUpAccountView.Name = "lookUpAccountView";
            lookUpAccountView.OptionsEditForm.PopupEditFormWidth = 686;
            lookUpAccountView.OptionsSelection.EnableAppearanceFocusedCell = false;
            lookUpAccountView.OptionsView.ShowGroupPanel = false;
            // 
            // lblAccountLabel
            // 
            lblAccountLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAccountLabel.Appearance.Options.UseFont = true;
            lblAccountLabel.Location = new Point(450, 13);
            lblAccountLabel.Name = "lblAccountLabel";
            lblAccountLabel.Size = new Size(134, 15);
            lblAccountLabel.TabIndex = 12;
            lblAccountLabel.Text = "Cari (Müşteri/Tedarikçi):";
            // 
            // dtDate
            // 
            dtDate.EditValue = null;
            dtDate.Location = new Point(317, 30);
            dtDate.Name = "dtDate";
            dtDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            dtDate.Size = new Size(120, 20);
            dtDate.TabIndex = 13;
            // 
            // lblDateLabel
            // 
            lblDateLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDateLabel.Appearance.Options.UseFont = true;
            lblDateLabel.Location = new Point(317, 13);
            lblDateLabel.Name = "lblDateLabel";
            lblDateLabel.Size = new Size(31, 15);
            lblDateLabel.TabIndex = 14;
            lblDateLabel.Text = "Tarih:";
            // 
            // txtInvoiceNumber
            // 
            txtInvoiceNumber.Location = new Point(159, 30);
            txtInvoiceNumber.Name = "txtInvoiceNumber";
            txtInvoiceNumber.Size = new Size(146, 20);
            txtInvoiceNumber.TabIndex = 15;
            // 
            // lblNumberLabel
            // 
            lblNumberLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNumberLabel.Appearance.Options.UseFont = true;
            lblNumberLabel.Location = new Point(159, 13);
            lblNumberLabel.Name = "lblNumberLabel";
            lblNumberLabel.Size = new Size(57, 15);
            lblNumberLabel.TabIndex = 16;
            lblNumberLabel.Text = "Fatura No:";
            // 
            // cmbInvoiceType
            // 
            cmbInvoiceType.Location = new Point(17, 30);
            cmbInvoiceType.Name = "cmbInvoiceType";
            cmbInvoiceType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbInvoiceType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbInvoiceType.Size = new Size(189, 20);
            cmbInvoiceType.TabIndex = 17;
            // 
            // lblTypeLabel
            // 
            lblTypeLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTypeLabel.Appearance.Options.UseFont = true;
            lblTypeLabel.Location = new Point(17, 13);
            lblTypeLabel.Name = "lblTypeLabel";
            lblTypeLabel.Size = new Size(67, 15);
            lblTypeLabel.TabIndex = 18;
            lblTypeLabel.Text = "Fatura Türü:";
            // 
            // pnlFooter
            // 
            pnlFooter.Appearance.BackColor = Color.FromArgb(248, 249, 250);
            pnlFooter.Appearance.Options.UseBackColor = true;
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnPrintSlip);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnSaveDraft);
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 675);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(1217, 52);
            pnlFooter.TabIndex = 2;
            // 
            // btnPrintSlip
            // 
            btnPrintSlip.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPrintSlip.Appearance.Font = new Font("Segoe UI", 9F);
            btnPrintSlip.Appearance.Options.UseFont = true;
            btnPrintSlip.Enabled = false;
            btnPrintSlip.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnPrintSlip.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnPrintSlip.ImageOptions.SvgImage");
            btnPrintSlip.ImageOptions.SvgImageSize = new Size(16, 16);
            btnPrintSlip.Location = new Point(13, 12);
            btnPrintSlip.Name = "btnPrintSlip";
            btnPrintSlip.Size = new Size(163, 29);
            btnPrintSlip.TabIndex = 0;
            btnPrintSlip.Text = "Taşınır İşlem Fişi Yazdır";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Appearance.Font = new Font("Segoe UI", 9F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnCancel.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnCancel.ImageOptions.SvgImage");
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.Location = new Point(1118, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(81, 29);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Vazgeç";
            // 
            // btnSaveDraft
            // 
            btnSaveDraft.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveDraft.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnSaveDraft.Appearance.Options.UseFont = true;
            btnSaveDraft.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnSaveDraft.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnSaveDraft.ImageOptions.SvgImage");
            btnSaveDraft.ImageOptions.SvgImageSize = new Size(16, 16);
            btnSaveDraft.Location = new Point(994, 12);
            btnSaveDraft.Name = "btnSaveDraft";
            btnSaveDraft.Size = new Size(111, 29);
            btnSaveDraft.TabIndex = 3;
            btnSaveDraft.Text = "Taslak Kaydet";
            // 
            // pnlFooterLine
            // 
            pnlFooterLine.Appearance.BackColor = Color.FromArgb(229, 231, 235);
            pnlFooterLine.Appearance.Options.UseBackColor = true;
            pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooterLine.Dock = DockStyle.Top;
            pnlFooterLine.Location = new Point(0, 0);
            pnlFooterLine.Name = "pnlFooterLine";
            pnlFooterLine.Size = new Size(1217, 1);
            pnlFooterLine.TabIndex = 5;
            // 
            // InvoiceEditForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1217, 727);
            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("InvoiceEditForm.IconOptions.SvgImage");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InvoiceEditForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Fatura";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            pnlBody.ResumeLayout(false);
            pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlItemsPanel).EndInit();
            pnlItemsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlTotals).EndInit();
            pnlTotals.ResumeLayout(false);
            pnlTotals.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridLines).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridLinesView).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlItemsHeader).EndInit();
            pnlItemsHeader.ResumeLayout(false);
            pnlItemsHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlCatalog).EndInit();
            pnlCatalog.ResumeLayout(false);
            pnlCatalog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtCatalogProductSearch.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridCatalog).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridCatalogView).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCatalogWarehouse.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCatalogWarehouseView).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtDescription.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookUpAccount.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookUpAccountView).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtDate.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtDate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtInvoiceNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbInvoiceType.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }
    }
}
