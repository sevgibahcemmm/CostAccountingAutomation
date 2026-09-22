using Cost.Accounting.Automation.WinFormsApp.Utils;
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
private DevExpress.XtraEditors.PanelControl pnlAccounts;
        private DevExpress.XtraEditors.LabelControl lblAccountsTitle;
        private System.Windows.Forms.FlowLayoutPanel flpAccounts;
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
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            pnlBody = new Panel();
            pnlAccounts = new DevExpress.XtraEditors.PanelControl();
            lblAccountsTitle = new DevExpress.XtraEditors.LabelControl();
            flpAccounts = new System.Windows.Forms.FlowLayoutPanel();
            pnlItemsPanel = new DevExpress.XtraEditors.PanelControl();
            gridLines = new DevExpress.XtraGrid.GridControl();
            gridLinesView = new DevExpress.XtraGrid.Views.Grid.GridView();
            pnlItemsHeader = new DevExpress.XtraEditors.PanelControl();
            btnDeleteLine = new DevExpress.XtraEditors.SimpleButton();
            btnAddLine = new DevExpress.XtraEditors.SimpleButton();
            lblItemsTitle = new DevExpress.XtraEditors.LabelControl();
            txtDescription = new DevExpress.XtraEditors.TextEdit();
            lblDescLabel = new DevExpress.XtraEditors.LabelControl();
            txtQuantity = new DevExpress.XtraEditors.TextEdit();
            lblQuantityLabel = new DevExpress.XtraEditors.LabelControl();
            lookUpProducedProduct = new DevExpress.XtraEditors.SearchLookUpEdit();
            lookUpProducedProductView = new DevExpress.XtraGrid.Views.Grid.GridView();
            lblProductLabel = new DevExpress.XtraEditors.LabelControl();
            lblStatusValue = new DevExpress.XtraEditors.LabelControl();
            lookUpWorkshop = new DevExpress.XtraEditors.SearchLookUpEdit();
            lookUpWorkshopView = new DevExpress.XtraGrid.Views.Grid.GridView();
            lblWorkshopLabel = new DevExpress.XtraEditors.LabelControl();
            dtCostDate = new DevExpress.XtraEditors.DateEdit();
            lblDateLabel = new DevExpress.XtraEditors.LabelControl();
            txtSlipNumber = new DevExpress.XtraEditors.TextEdit();
            lblNumberLabel = new DevExpress.XtraEditors.LabelControl();
            cmbCostSlipType = new DevExpress.XtraEditors.ComboBoxEdit();
            lblTypeLabel = new DevExpress.XtraEditors.LabelControl();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            btnPrintSlip = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnSaveDraft = new DevExpress.XtraEditors.SimpleButton();
            btnApprove = new DevExpress.XtraEditors.SimpleButton();
            pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlAccounts).BeginInit();
            pnlAccounts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlItemsPanel).BeginInit();
            pnlItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridLines).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridLinesView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlItemsHeader).BeginInit();
            pnlItemsHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtDescription.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtQuantity.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookUpProducedProduct.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookUpProducedProductView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookUpWorkshop.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookUpWorkshopView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtCostDate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtCostDate.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtSlipNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbCostSlipType.Properties).BeginInit();
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
            lblSubtitle.Size = new Size(272, 13);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "Pusula ve gider kalemi bilgilerini eksiksiz doldurunuz";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(17, 24, 39);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Location = new Point(53, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(150, 21);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Yeni Maliyet Pusulası";
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.Location = new Point(17, 16);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(0, 13);
            lblHeaderIcon.TabIndex = 2;
            lblHeaderIcon.ImageOptions.SvgImage = DxIcon.Percent;
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = DockStyle.Bottom;
            pnlHeaderLine.Location = new Point(0, 58);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new Size(1217, 1);
            pnlHeaderLine.TabIndex = 3;
            // 
            // pnlBody
            // 
            pnlBody.Controls.Add(pnlAccounts);
            pnlBody.Controls.Add(pnlItemsPanel);
            pnlBody.Controls.Add(pnlItemsHeader);
            pnlBody.Controls.Add(txtDescription);
            pnlBody.Controls.Add(lblDescLabel);
            pnlBody.Controls.Add(txtQuantity);
            pnlBody.Controls.Add(lblQuantityLabel);
            pnlBody.Controls.Add(lookUpProducedProduct);
            pnlBody.Controls.Add(lblProductLabel);
            pnlBody.Controls.Add(lblStatusValue);
            pnlBody.Controls.Add(lookUpWorkshop);
            pnlBody.Controls.Add(lblWorkshopLabel);
            pnlBody.Controls.Add(dtCostDate);
            pnlBody.Controls.Add(lblDateLabel);
            pnlBody.Controls.Add(txtSlipNumber);
            pnlBody.Controls.Add(lblNumberLabel);
            pnlBody.Controls.Add(cmbCostSlipType);
            pnlBody.Controls.Add(lblTypeLabel);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(0, 59);
            pnlBody.Name = "pnlBody";
            pnlBody.Padding = new Padding(17);
            pnlBody.Size = new Size(1217, 625);
            pnlBody.TabIndex = 1;
            // 
            // pnlAccounts
            // 
            pnlAccounts.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlAccounts.Controls.Add(flpAccounts);
            pnlAccounts.Controls.Add(lblAccountsTitle);
            pnlAccounts.Location = new Point(17, 376);
            pnlAccounts.Name = "pnlAccounts";
            pnlAccounts.Size = new Size(1183, 243);
            pnlAccounts.TabIndex = 0;
            // 
            // lblAccountsTitle
            // 
            lblAccountsTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAccountsTitle.Appearance.Options.UseFont = true;
            lblAccountsTitle.Location = new Point(13, 10);
            lblAccountsTitle.Name = "lblAccountsTitle";
            lblAccountsTitle.Size = new Size(205, 17);
            lblAccountsTitle.TabIndex = 0;
            lblAccountsTitle.Text = "Hesap Bazlı Gider Girişi (710-780)";
            // 
            // flpAccounts
            // 
            flpAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            flpAccounts.AutoScroll = true;
            flpAccounts.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            flpAccounts.Location = new Point(10, 40);
            flpAccounts.Name = "flpAccounts";
            flpAccounts.Size = new Size(1155, 240);
            flpAccounts.TabIndex = 1;
            flpAccounts.WrapContents = true;
            // 
            // pnlItemsPanel
            // 
            pnlItemsPanel.Appearance.BackColor = Color.Transparent;
            pnlItemsPanel.Appearance.Options.UseBackColor = true;
            pnlItemsPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlItemsPanel.Controls.Add(gridLines);
            pnlItemsPanel.Location = new Point(17, 145);
            pnlItemsPanel.Name = "pnlItemsPanel";
            pnlItemsPanel.Size = new Size(1183, 220);
            pnlItemsPanel.TabIndex = 7;
            // 
            // gridLines
            // 
            gridLines.Dock = DockStyle.Fill;
            gridLines.Location = new Point(0, 0);
            gridLines.MainView = gridLinesView;
            gridLines.Name = "gridLines";
            gridLines.Size = new Size(1183, 220);
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
            pnlItemsHeader.Location = new Point(17, 108);
            pnlItemsHeader.Name = "pnlItemsHeader";
            pnlItemsHeader.Size = new Size(1183, 31);
            pnlItemsHeader.TabIndex = 5;
            // 
            // btnDeleteLine
            // 
            btnDeleteLine.Location = new Point(1093, 3);
            btnDeleteLine.Name = "btnDeleteLine";
            btnDeleteLine.Size = new Size(90, 24);
            btnDeleteLine.TabIndex = 0;
            btnDeleteLine.Text = "- Satır Sil";
            btnDeleteLine.ImageOptions.SvgImage = DxIcon.Delete;
            btnDeleteLine.ImageOptions.SvgImageSize = new Size(16, 16);
            btnDeleteLine.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // btnAddLine
            // 
            btnAddLine.Location = new Point(994, 3);
            btnAddLine.Name = "btnAddLine";
            btnAddLine.Size = new Size(90, 24);
            btnAddLine.TabIndex = 1;
            btnAddLine.Text = "+ Satır Ekle";
            btnAddLine.ImageOptions.SvgImage = DxIcon.Add;
            btnAddLine.ImageOptions.SvgImageSize = new Size(16, 16);
            btnAddLine.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // lblItemsTitle
            // 
            lblItemsTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblItemsTitle.Appearance.Options.UseFont = true;
            lblItemsTitle.Location = new Point(4, 7);
            lblItemsTitle.Name = "lblItemsTitle";
            lblItemsTitle.Size = new Size(95, 17);
            lblItemsTitle.TabIndex = 2;
            lblItemsTitle.Text = "Gider Kalemleri";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(359, 82);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(841, 20);
            txtDescription.TabIndex = 8;
            // 
            // lblDescLabel
            // 
            lblDescLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDescLabel.Appearance.Options.UseFont = true;
            lblDescLabel.Location = new Point(359, 65);
            lblDescLabel.Name = "lblDescLabel";
            lblDescLabel.Size = new Size(53, 15);
            lblDescLabel.TabIndex = 9;
            lblDescLabel.Text = "Açıklama:";
            // 
            // txtQuantity
            // 
            txtQuantity.Location = new Point(1120, 30);
            txtQuantity.Name = "txtQuantity";
            txtQuantity.Size = new Size(77, 20);
            txtQuantity.TabIndex = 10;
            // 
            // lblQuantityLabel
            // 
            lblQuantityLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblQuantityLabel.Appearance.Options.UseFont = true;
            lblQuantityLabel.Location = new Point(1120, 13);
            lblQuantityLabel.Name = "lblQuantityLabel";
            lblQuantityLabel.Size = new Size(40, 15);
            lblQuantityLabel.TabIndex = 11;
            lblQuantityLabel.Text = "Miktar:";
            // 
            // lookUpProducedProduct
            // 
            lookUpProducedProduct.Location = new Point(17, 82);
            lookUpProducedProduct.Name = "lookUpProducedProduct";
            lookUpProducedProduct.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            lookUpProducedProduct.Properties.PopupView = lookUpProducedProductView;
            lookUpProducedProduct.Size = new Size(336, 20);
            lookUpProducedProduct.TabIndex = 12;
            // 
            // lookUpProducedProductView
            // 
            lookUpProducedProductView.DetailHeight = 303;
            lookUpProducedProductView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            lookUpProducedProductView.Name = "lookUpProducedProductView";
            lookUpProducedProductView.OptionsEditForm.PopupEditFormWidth = 686;
            lookUpProducedProductView.OptionsSelection.EnableAppearanceFocusedCell = false;
            lookUpProducedProductView.OptionsView.ShowGroupPanel = false;
            // 
            // lblProductLabel
            // 
            lblProductLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProductLabel.Appearance.Options.UseFont = true;
            lblProductLabel.Location = new Point(17, 65);
            lblProductLabel.Name = "lblProductLabel";
            lblProductLabel.Size = new Size(80, 15);
            lblProductLabel.TabIndex = 13;
            lblProductLabel.Text = "Üretilen Ürün:";
            // 
            // lblStatusValue
            // 
            lblStatusValue.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblStatusValue.Appearance.ForeColor = Color.FromArgb(30, 64, 175);
            lblStatusValue.Appearance.Options.UseFont = true;
            lblStatusValue.Appearance.Options.UseForeColor = true;
            lblStatusValue.Location = new Point(900, 33);
            lblStatusValue.Name = "lblStatusValue";
            lblStatusValue.Size = new Size(0, 15);
            lblStatusValue.TabIndex = 14;
            // 
            // lookUpWorkshop
            // 
            lookUpWorkshop.Location = new Point(627, 30);
            lookUpWorkshop.Name = "lookUpWorkshop";
            lookUpWorkshop.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            lookUpWorkshop.Properties.PopupView = lookUpWorkshopView;
            lookUpWorkshop.Size = new Size(487, 20);
            lookUpWorkshop.TabIndex = 15;
            // 
            // lookUpWorkshopView
            // 
            lookUpWorkshopView.DetailHeight = 303;
            lookUpWorkshopView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            lookUpWorkshopView.Name = "lookUpWorkshopView";
            lookUpWorkshopView.OptionsEditForm.PopupEditFormWidth = 686;
            lookUpWorkshopView.OptionsSelection.EnableAppearanceFocusedCell = false;
            lookUpWorkshopView.OptionsView.ShowGroupPanel = false;
            // 
            // lblWorkshopLabel
            // 
            lblWorkshopLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblWorkshopLabel.Appearance.Options.UseFont = true;
            lblWorkshopLabel.Location = new Point(627, 13);
            lblWorkshopLabel.Name = "lblWorkshopLabel";
            lblWorkshopLabel.Size = new Size(39, 15);
            lblWorkshopLabel.TabIndex = 16;
            lblWorkshopLabel.Text = "Atölye:";
            // 
            // dtCostDate
            // 
            dtCostDate.EditValue = null;
            dtCostDate.Location = new Point(500, 30);
            dtCostDate.Name = "dtCostDate";
            dtCostDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            dtCostDate.Size = new Size(94, 20);
            dtCostDate.TabIndex = 17;
            // 
            // lblDateLabel
            // 
            lblDateLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDateLabel.Appearance.Options.UseFont = true;
            lblDateLabel.Location = new Point(500, 13);
            lblDateLabel.Name = "lblDateLabel";
            lblDateLabel.Size = new Size(31, 15);
            lblDateLabel.TabIndex = 18;
            lblDateLabel.Text = "Tarih:";
            // 
            // txtSlipNumber
            // 
            txtSlipNumber.Location = new Point(359, 30);
            txtSlipNumber.Name = "txtSlipNumber";
            txtSlipNumber.Size = new Size(129, 20);
            txtSlipNumber.TabIndex = 19;
            // 
            // lblNumberLabel
            // 
            lblNumberLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNumberLabel.Appearance.Options.UseFont = true;
            lblNumberLabel.Location = new Point(359, 13);
            lblNumberLabel.Name = "lblNumberLabel";
            lblNumberLabel.Size = new Size(57, 15);
            lblNumberLabel.TabIndex = 20;
            lblNumberLabel.Text = "Pusula No:";
            // 
            // cmbCostSlipType
            // 
            cmbCostSlipType.Location = new Point(17, 30);
            cmbCostSlipType.Name = "cmbCostSlipType";
            cmbCostSlipType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbCostSlipType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbCostSlipType.Size = new Size(336, 20);
            cmbCostSlipType.TabIndex = 21;
            // 
            // lblTypeLabel
            // 
            lblTypeLabel.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTypeLabel.Appearance.Options.UseFont = true;
            lblTypeLabel.Location = new Point(17, 13);
            lblTypeLabel.Name = "lblTypeLabel";
            lblTypeLabel.Size = new Size(67, 15);
            lblTypeLabel.TabIndex = 22;
            lblTypeLabel.Text = "Pusula Türü:";
            // 
            // pnlFooter
            // 
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnPrintSlip);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnSaveDraft);
            pnlFooter.Controls.Add(btnApprove);
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 684);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(1217, 60);
            pnlFooter.TabIndex = 2;
            // 
            // btnPrintSlip
            // 
            btnPrintSlip.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPrintSlip.Appearance.Font = new Font("Segoe UI", 9F);
            btnPrintSlip.Appearance.Options.UseFont = true;
            btnPrintSlip.Enabled = false;
            btnPrintSlip.Location = new Point(13, 12);
            btnPrintSlip.Name = "btnPrintSlip";
            btnPrintSlip.Size = new Size(171, 29);
            btnPrintSlip.TabIndex = 0;
            btnPrintSlip.Text = "Maliyet Pusulası Yazdır";
            btnPrintSlip.ImageOptions.SvgImage = DxIcon.Print;
            btnPrintSlip.ImageOptions.SvgImageSize = new Size(18, 18);
            btnPrintSlip.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Appearance.Font = new Font("Segoe UI", 9F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Location = new Point(1111, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(81, 29);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Vazgeç";
            btnCancel.ImageOptions.SvgImage = DxIcon.Close;
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Location = new Point(984, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 29);
            btnSave.TabIndex = 2;
            btnSave.Text = "Kaydet ve Onayla";
            btnSave.ImageOptions.SvgImage = DxIcon.Check;
            btnSave.ImageOptions.SvgImageSize = new Size(18, 18);
            btnSave.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // btnSaveDraft
            // 
            btnSaveDraft.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveDraft.Appearance.Font = new Font("Segoe UI", 9F);
            btnSaveDraft.Appearance.Options.UseFont = true;
            btnSaveDraft.Location = new Point(866, 12);
            btnSaveDraft.Name = "btnSaveDraft";
            btnSaveDraft.Size = new Size(111, 29);
            btnSaveDraft.TabIndex = 3;
            btnSaveDraft.Text = "Taslak Kaydet";
            btnSaveDraft.ImageOptions.SvgImage = DxIcon.Save;
            btnSaveDraft.ImageOptions.SvgImageSize = new Size(18, 18);
            btnSaveDraft.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // btnApprove
            // 
            btnApprove.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnApprove.Appearance.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnApprove.Appearance.Options.UseFont = true;
            btnApprove.Location = new Point(984, 12);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(120, 29);
            btnApprove.TabIndex = 4;
            btnApprove.Text = "Yazdır";
            btnApprove.ImageOptions.SvgImage = DxIcon.CheckAll;
            btnApprove.ImageOptions.SvgImageSize = new Size(18, 18);
            btnApprove.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnApprove.Visible = false;
            // 
            // pnlFooterLine
            // 
            pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooterLine.Dock = DockStyle.Top;
            pnlFooterLine.Location = new Point(0, 0);
            pnlFooterLine.Name = "pnlFooterLine";
            pnlFooterLine.Size = new Size(1217, 1);
            pnlFooterLine.TabIndex = 5;
            // 
            // CostSlipEditForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1217, 744);
            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            IconOptions.SvgImage = DxIcon.Percent;
            lblTitle.Appearance.ForeColor = SkinTheme.Text;
            lblSubtitle.Appearance.ForeColor = SkinTheme.SecondaryText;
            lblStatusValue.Appearance.ForeColor = SkinTheme.Primary;
            Name = "CostSlipEditForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Maliyet Pusulası";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            pnlBody.ResumeLayout(false);
            pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlAccounts).EndInit();
            pnlAccounts.ResumeLayout(false);
            pnlAccounts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlItemsPanel).EndInit();
            pnlItemsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridLines).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridLinesView).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlItemsHeader).EndInit();
            pnlItemsHeader.ResumeLayout(false);
            pnlItemsHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtDescription.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtQuantity.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookUpProducedProduct.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookUpProducedProductView).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookUpWorkshop.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookUpWorkshopView).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtCostDate.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtCostDate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtSlipNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCostSlipType.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }
    }
}