using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CarryForwardForms
{
    public sealed partial class CarryForwardOperationsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Üst şerit: aksan çubuğu, başlık, ayırıcı, özet, liste başlığı
        private DevExpress.XtraEditors.PanelControl accentBar;
        private DevExpress.XtraEditors.PanelControl headerPanel;
        private DevExpress.XtraEditors.PictureEdit picHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderTitle;
        private DevExpress.XtraEditors.LabelControl lblHeaderSub;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.PanelControl headerDivider;
        private DevExpress.XtraEditors.PanelControl summaryPanel;
        private System.Windows.Forms.TableLayoutPanel summaryCards;
        private DevExpress.XtraEditors.PanelControl cardStock;
        private DevExpress.XtraEditors.PanelControl cardStockStrip;
        private DevExpress.XtraEditors.LabelControl lblStockTitle;
        private DevExpress.XtraEditors.LabelControl lblStockValue;
        private DevExpress.XtraEditors.LabelControl lblStockNote;
        private DevExpress.XtraEditors.PanelControl cardReceivable;
        private DevExpress.XtraEditors.PanelControl cardReceivableStrip;
        private DevExpress.XtraEditors.LabelControl lblReceivableTitle;
        private DevExpress.XtraEditors.LabelControl lblReceivableValue;
        private DevExpress.XtraEditors.LabelControl lblReceivableNote;
        private DevExpress.XtraEditors.PanelControl cardPayable;
        private DevExpress.XtraEditors.PanelControl cardPayableStrip;
        private DevExpress.XtraEditors.LabelControl lblPayableTitle;
        private DevExpress.XtraEditors.LabelControl lblPayableValue;
        private DevExpress.XtraEditors.LabelControl lblPayableNote;
        private DevExpress.XtraEditors.PanelControl cardChart;
        private DevExpress.XtraEditors.PanelControl cardChartStrip;
        private DevExpress.XtraEditors.LabelControl lblChartTitle;
        private DevExpress.XtraEditors.LabelControl lblChartValue;
        private DevExpress.XtraEditors.LabelControl lblChartNote;
        private DevExpress.XtraEditors.LabelControl lblPreviewTitle;
        private DevExpress.XtraEditors.PanelControl panelFooter;

        // Alt şerit: seçimler, düğmeler, durum
        private DevExpress.XtraEditors.GroupControl grpOptions;
        private System.Windows.Forms.FlowLayoutPanel flpOptions;
        private DevExpress.XtraEditors.CheckEdit chkAccounts;
        private DevExpress.XtraEditors.CheckEdit chkTaxUnits;
        private DevExpress.XtraEditors.CheckEdit chkCurrent;
        private DevExpress.XtraEditors.CheckEdit chkProducts;
        private DevExpress.XtraEditors.CheckEdit chkPrices;
        private DevExpress.XtraEditors.CheckEdit chkRecipes;
        private DevExpress.XtraEditors.CheckEdit chkChartBalances;
        private DevExpress.XtraEditors.SimpleButton btnStart;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
        private DevExpress.XtraEditors.PanelControl statusPanel;
        private DevExpress.XtraEditors.PanelControl statusAccentStrip;
        private DevExpress.XtraEditors.LabelControl lblStatus;

        // Doldurulan alan
        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.accentBar = new DevExpress.XtraEditors.PanelControl();
            this.headerPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblHeaderSub = new DevExpress.XtraEditors.LabelControl();
            this.lblHeaderTitle = new DevExpress.XtraEditors.LabelControl();
            this.picHeader = new DevExpress.XtraEditors.PictureEdit();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            this.headerDivider = new DevExpress.XtraEditors.PanelControl();
            this.summaryPanel = new DevExpress.XtraEditors.PanelControl();
            this.summaryCards = new System.Windows.Forms.TableLayoutPanel();
            this.cardStock = new DevExpress.XtraEditors.PanelControl();
            this.lblStockNote = new DevExpress.XtraEditors.LabelControl();
            this.lblStockValue = new DevExpress.XtraEditors.LabelControl();
            this.lblStockTitle = new DevExpress.XtraEditors.LabelControl();
            this.cardStockStrip = new DevExpress.XtraEditors.PanelControl();
            this.cardReceivable = new DevExpress.XtraEditors.PanelControl();
            this.lblReceivableNote = new DevExpress.XtraEditors.LabelControl();
            this.lblReceivableValue = new DevExpress.XtraEditors.LabelControl();
            this.lblReceivableTitle = new DevExpress.XtraEditors.LabelControl();
            this.cardReceivableStrip = new DevExpress.XtraEditors.PanelControl();
            this.cardPayable = new DevExpress.XtraEditors.PanelControl();
            this.lblPayableNote = new DevExpress.XtraEditors.LabelControl();
            this.lblPayableValue = new DevExpress.XtraEditors.LabelControl();
            this.lblPayableTitle = new DevExpress.XtraEditors.LabelControl();
            this.cardPayableStrip = new DevExpress.XtraEditors.PanelControl();
            this.cardChart = new DevExpress.XtraEditors.PanelControl();
            this.lblChartNote = new DevExpress.XtraEditors.LabelControl();
            this.lblChartValue = new DevExpress.XtraEditors.LabelControl();
            this.lblChartTitle = new DevExpress.XtraEditors.LabelControl();
            this.cardChartStrip = new DevExpress.XtraEditors.PanelControl();
            this.lblPreviewTitle = new DevExpress.XtraEditors.LabelControl();
            this.panelFooter = new DevExpress.XtraEditors.PanelControl();
            this.grpOptions = new DevExpress.XtraEditors.GroupControl();
            this.flpOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.chkAccounts = new DevExpress.XtraEditors.CheckEdit();
            this.chkTaxUnits = new DevExpress.XtraEditors.CheckEdit();
            this.chkCurrent = new DevExpress.XtraEditors.CheckEdit();
            this.chkProducts = new DevExpress.XtraEditors.CheckEdit();
            this.chkPrices = new DevExpress.XtraEditors.CheckEdit();
            this.chkRecipes = new DevExpress.XtraEditors.CheckEdit();
            this.chkChartBalances = new DevExpress.XtraEditors.CheckEdit();
            this.btnStart = new DevExpress.XtraEditors.SimpleButton();
            this.btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            this.statusPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblStatus = new DevExpress.XtraEditors.LabelControl();
            this.statusAccentStrip = new DevExpress.XtraEditors.PanelControl();
            this.gridControl = new DevExpress.XtraGrid.GridControl();
            this.gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)this.accentBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.headerPanel).BeginInit();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.picHeader.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.headerDivider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.summaryPanel).BeginInit();
            this.summaryPanel.SuspendLayout();
            this.summaryCards.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardStock).BeginInit();
            this.cardStock.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardStockStrip).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.cardReceivable).BeginInit();
            this.cardReceivable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardReceivableStrip).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.cardPayable).BeginInit();
            this.cardPayable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardPayableStrip).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.cardChart).BeginInit();
            this.cardChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardChartStrip).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.panelFooter).BeginInit();
            this.panelFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.grpOptions).BeginInit();
            this.grpOptions.SuspendLayout();
            this.flpOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.statusPanel).BeginInit();
            this.statusPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.statusAccentStrip).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.gridView).BeginInit();
            this.SuspendLayout();
            //
            // accentBar
            //
            this.accentBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.accentBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.accentBar.Location = new System.Drawing.Point(0, 0);
            this.accentBar.Margin = new System.Windows.Forms.Padding(0);
            this.accentBar.Name = "accentBar";
            this.accentBar.Size = new System.Drawing.Size(1000, 8);
            this.accentBar.TabIndex = 0;
            //
            // headerPanel
            //
            this.headerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.headerPanel.Controls.Add(this.lblHeaderSub);
            this.headerPanel.Controls.Add(this.lblHeaderTitle);
            this.headerPanel.Controls.Add(this.picHeader);
            this.headerPanel.Controls.Add(this.btnClose);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 8);
            this.headerPanel.Margin = new System.Windows.Forms.Padding(0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1000, 76);
            this.headerPanel.TabIndex = 1;
            //
            // lblHeaderSub
            //
            this.lblHeaderSub.Appearance.Font = new Font("Segoe UI", 9.5F);
            this.lblHeaderSub.Appearance.Options.UseFont = true;
            this.lblHeaderSub.Appearance.Options.UseTextOptions = true;
            this.lblHeaderSub.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblHeaderSub.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblHeaderSub.Location = new System.Drawing.Point(72, 44);
            this.lblHeaderSub.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblHeaderSub.Name = "lblHeaderSub";
            this.lblHeaderSub.Size = new System.Drawing.Size(838, 24);
            this.lblHeaderSub.TabIndex = 2;
            this.lblHeaderSub.Text = "Devir ön izlemesi yükleniyor...";
            //
            // lblHeaderTitle
            //
            this.lblHeaderTitle.Appearance.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            this.lblHeaderTitle.Appearance.Options.UseFont = true;
            this.lblHeaderTitle.Appearance.Options.UseTextOptions = true;
            this.lblHeaderTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblHeaderTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblHeaderTitle.Location = new System.Drawing.Point(72, 16);
            this.lblHeaderTitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(838, 28);
            this.lblHeaderTitle.TabIndex = 1;
            this.lblHeaderTitle.Text = "Devir İşlemleri";
            //
            // picHeader
            //
            this.picHeader.Location = new System.Drawing.Point(24, 20);
            this.picHeader.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.picHeader.Name = "picHeader";
            this.picHeader.Properties.Appearance.BackColor = Color.Transparent;
            this.picHeader.Properties.Appearance.Options.UseBackColor = true;
            this.picHeader.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.picHeader.Size = new System.Drawing.Size(36, 36);
            this.picHeader.TabIndex = 0;
            this.picHeader.SvgImage = DxIcon.Restore;
            //
            // btnClose
            //
            this.btnClose.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClose.Appearance.Options.UseFont = true;
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right))));
            this.btnClose.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnClose.ImageOptions.SvgImage = DxIcon.Close;
            this.btnClose.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            this.btnClose.Location = new System.Drawing.Point(892, 21);
            this.btnClose.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(92, 34);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Kapat";
            //
            // headerDivider
            //
            this.headerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.headerDivider.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerDivider.Location = new System.Drawing.Point(0, 84);
            this.headerDivider.Margin = new System.Windows.Forms.Padding(0);
            this.headerDivider.Name = "headerDivider";
            this.headerDivider.Size = new System.Drawing.Size(1000, 1);
            this.headerDivider.TabIndex = 2;
            //
            // summaryPanel
            //
            this.summaryPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.summaryPanel.Controls.Add(this.summaryCards);
            this.summaryPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.summaryPanel.Location = new System.Drawing.Point(0, 85);
            this.summaryPanel.Margin = new System.Windows.Forms.Padding(0);
            this.summaryPanel.Name = "summaryPanel";
            this.summaryPanel.Padding = new System.Windows.Forms.Padding(16, 10, 16, 12);
            this.summaryPanel.Size = new System.Drawing.Size(1000, 116);
            this.summaryPanel.TabIndex = 3;
            //
            // summaryCards
            //
            this.summaryCards.ColumnCount = 4;
            this.summaryCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.summaryCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.summaryCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.summaryCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.summaryCards.Controls.Add(this.cardStock, 0, 0);
            this.summaryCards.Controls.Add(this.cardReceivable, 1, 0);
            this.summaryCards.Controls.Add(this.cardPayable, 2, 0);
            this.summaryCards.Controls.Add(this.cardChart, 3, 0);
            this.summaryCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summaryCards.Location = new System.Drawing.Point(16, 10);
            this.summaryCards.Margin = new System.Windows.Forms.Padding(0);
            this.summaryCards.Name = "summaryCards";
            this.summaryCards.RowCount = 1;
            this.summaryCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.summaryCards.Size = new System.Drawing.Size(968, 94);
            this.summaryCards.TabIndex = 0;
            //
            // cardStock
            //
            this.cardStock.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.cardStock.Controls.Add(this.lblStockNote);
            this.cardStock.Controls.Add(this.lblStockValue);
            this.cardStock.Controls.Add(this.lblStockTitle);
            this.cardStock.Controls.Add(this.cardStockStrip);
            this.cardStock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardStock.Location = new System.Drawing.Point(0, 0);
            this.cardStock.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.cardStock.Name = "cardStock";
            this.cardStock.Padding = new System.Windows.Forms.Padding(0);
            this.cardStock.Size = new System.Drawing.Size(239, 94);
            this.cardStock.TabIndex = 0;
            //
            // lblStockNote
            //
            this.lblStockNote.Appearance.Font = new Font("Segoe UI", 8.5F);
            this.lblStockNote.Appearance.Options.UseFont = true;
            this.lblStockNote.Appearance.Options.UseTextOptions = true;
            this.lblStockNote.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblStockNote.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStockNote.Location = new System.Drawing.Point(18, 62);
            this.lblStockNote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblStockNote.Name = "lblStockNote";
            this.lblStockNote.Size = new System.Drawing.Size(211, 20);
            this.lblStockNote.TabIndex = 3;
            this.lblStockNote.Text = "-";
            //
            // lblStockValue
            //
            this.lblStockValue.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblStockValue.Appearance.Options.UseFont = true;
            this.lblStockValue.Appearance.Options.UseTextOptions = true;
            this.lblStockValue.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblStockValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStockValue.Location = new System.Drawing.Point(18, 28);
            this.lblStockValue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblStockValue.Name = "lblStockValue";
            this.lblStockValue.Size = new System.Drawing.Size(211, 32);
            this.lblStockValue.TabIndex = 2;
            this.lblStockValue.Text = "-";
            //
            // lblStockTitle
            //
            this.lblStockTitle.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this.lblStockTitle.Appearance.Options.UseFont = true;
            this.lblStockTitle.Appearance.Options.UseTextOptions = true;
            this.lblStockTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblStockTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStockTitle.Location = new System.Drawing.Point(18, 8);
            this.lblStockTitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblStockTitle.Name = "lblStockTitle";
            this.lblStockTitle.Size = new System.Drawing.Size(211, 16);
            this.lblStockTitle.TabIndex = 1;
            this.lblStockTitle.Text = "STOK DEVRİ";
            //
            // cardStockStrip
            //
            this.cardStockStrip.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardStockStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this.cardStockStrip.Location = new System.Drawing.Point(0, 0);
            this.cardStockStrip.Margin = new System.Windows.Forms.Padding(0);
            this.cardStockStrip.Name = "cardStockStrip";
            this.cardStockStrip.Size = new System.Drawing.Size(4, 92);
            this.cardStockStrip.TabIndex = 0;
            //
            // cardReceivable
            //
            this.cardReceivable.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.cardReceivable.Controls.Add(this.lblReceivableNote);
            this.cardReceivable.Controls.Add(this.lblReceivableValue);
            this.cardReceivable.Controls.Add(this.lblReceivableTitle);
            this.cardReceivable.Controls.Add(this.cardReceivableStrip);
            this.cardReceivable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardReceivable.Location = new System.Drawing.Point(249, 0);
            this.cardReceivable.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.cardReceivable.Name = "cardReceivable";
            this.cardReceivable.Padding = new System.Windows.Forms.Padding(0);
            this.cardReceivable.Size = new System.Drawing.Size(239, 94);
            this.cardReceivable.TabIndex = 0;
            //
            // lblReceivableNote
            //
            this.lblReceivableNote.Appearance.Font = new Font("Segoe UI", 8.5F);
            this.lblReceivableNote.Appearance.Options.UseFont = true;
            this.lblReceivableNote.Appearance.Options.UseTextOptions = true;
            this.lblReceivableNote.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblReceivableNote.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblReceivableNote.Location = new System.Drawing.Point(18, 62);
            this.lblReceivableNote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblReceivableNote.Name = "lblReceivableNote";
            this.lblReceivableNote.Size = new System.Drawing.Size(211, 20);
            this.lblReceivableNote.TabIndex = 3;
            this.lblReceivableNote.Text = "-";
            //
            // lblReceivableValue
            //
            this.lblReceivableValue.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblReceivableValue.Appearance.Options.UseFont = true;
            this.lblReceivableValue.Appearance.Options.UseTextOptions = true;
            this.lblReceivableValue.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblReceivableValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblReceivableValue.Location = new System.Drawing.Point(18, 28);
            this.lblReceivableValue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblReceivableValue.Name = "lblReceivableValue";
            this.lblReceivableValue.Size = new System.Drawing.Size(211, 32);
            this.lblReceivableValue.TabIndex = 2;
            this.lblReceivableValue.Text = "-";
            //
            // lblReceivableTitle
            //
            this.lblReceivableTitle.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this.lblReceivableTitle.Appearance.Options.UseFont = true;
            this.lblReceivableTitle.Appearance.Options.UseTextOptions = true;
            this.lblReceivableTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblReceivableTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblReceivableTitle.Location = new System.Drawing.Point(18, 8);
            this.lblReceivableTitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblReceivableTitle.Name = "lblReceivableTitle";
            this.lblReceivableTitle.Size = new System.Drawing.Size(211, 16);
            this.lblReceivableTitle.TabIndex = 1;
            this.lblReceivableTitle.Text = "ALACAK (TAHSİL EDİLMEMİŞ)";
            //
            // cardReceivableStrip
            //
            this.cardReceivableStrip.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardReceivableStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this.cardReceivableStrip.Location = new System.Drawing.Point(0, 0);
            this.cardReceivableStrip.Margin = new System.Windows.Forms.Padding(0);
            this.cardReceivableStrip.Name = "cardReceivableStrip";
            this.cardReceivableStrip.Size = new System.Drawing.Size(4, 92);
            this.cardReceivableStrip.TabIndex = 0;
            //
            // cardPayable
            //
            this.cardPayable.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.cardPayable.Controls.Add(this.lblPayableNote);
            this.cardPayable.Controls.Add(this.lblPayableValue);
            this.cardPayable.Controls.Add(this.lblPayableTitle);
            this.cardPayable.Controls.Add(this.cardPayableStrip);
            this.cardPayable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardPayable.Location = new System.Drawing.Point(498, 0);
            this.cardPayable.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.cardPayable.Name = "cardPayable";
            this.cardPayable.Padding = new System.Windows.Forms.Padding(0);
            this.cardPayable.Size = new System.Drawing.Size(239, 94);
            this.cardPayable.TabIndex = 0;
            //
            // lblPayableNote
            //
            this.lblPayableNote.Appearance.Font = new Font("Segoe UI", 8.5F);
            this.lblPayableNote.Appearance.Options.UseFont = true;
            this.lblPayableNote.Appearance.Options.UseTextOptions = true;
            this.lblPayableNote.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblPayableNote.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPayableNote.Location = new System.Drawing.Point(18, 62);
            this.lblPayableNote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblPayableNote.Name = "lblPayableNote";
            this.lblPayableNote.Size = new System.Drawing.Size(211, 20);
            this.lblPayableNote.TabIndex = 3;
            this.lblPayableNote.Text = "-";
            //
            // lblPayableValue
            //
            this.lblPayableValue.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblPayableValue.Appearance.Options.UseFont = true;
            this.lblPayableValue.Appearance.Options.UseTextOptions = true;
            this.lblPayableValue.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblPayableValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPayableValue.Location = new System.Drawing.Point(18, 28);
            this.lblPayableValue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblPayableValue.Name = "lblPayableValue";
            this.lblPayableValue.Size = new System.Drawing.Size(211, 32);
            this.lblPayableValue.TabIndex = 2;
            this.lblPayableValue.Text = "-";
            //
            // lblPayableTitle
            //
            this.lblPayableTitle.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this.lblPayableTitle.Appearance.Options.UseFont = true;
            this.lblPayableTitle.Appearance.Options.UseTextOptions = true;
            this.lblPayableTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblPayableTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPayableTitle.Location = new System.Drawing.Point(18, 8);
            this.lblPayableTitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblPayableTitle.Name = "lblPayableTitle";
            this.lblPayableTitle.Size = new System.Drawing.Size(211, 16);
            this.lblPayableTitle.TabIndex = 1;
            this.lblPayableTitle.Text = "BORÇ (ÖDENMEMİŞ)";
            //
            // cardPayableStrip
            //
            this.cardPayableStrip.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardPayableStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this.cardPayableStrip.Location = new System.Drawing.Point(0, 0);
            this.cardPayableStrip.Margin = new System.Windows.Forms.Padding(0);
            this.cardPayableStrip.Name = "cardPayableStrip";
            this.cardPayableStrip.Size = new System.Drawing.Size(4, 92);
            this.cardPayableStrip.TabIndex = 0;
            //
            // cardChart
            //
            this.cardChart.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.cardChart.Controls.Add(this.lblChartNote);
            this.cardChart.Controls.Add(this.lblChartValue);
            this.cardChart.Controls.Add(this.lblChartTitle);
            this.cardChart.Controls.Add(this.cardChartStrip);
            this.cardChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardChart.Location = new System.Drawing.Point(747, 0);
            this.cardChart.Margin = new System.Windows.Forms.Padding(0);
            this.cardChart.Name = "cardChart";
            this.cardChart.Padding = new System.Windows.Forms.Padding(0);
            this.cardChart.Size = new System.Drawing.Size(221, 94);
            this.cardChart.TabIndex = 0;
            //
            // lblChartNote
            //
            this.lblChartNote.Appearance.Font = new Font("Segoe UI", 8.5F);
            this.lblChartNote.Appearance.Options.UseFont = true;
            this.lblChartNote.Appearance.Options.UseTextOptions = true;
            this.lblChartNote.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblChartNote.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblChartNote.Location = new System.Drawing.Point(18, 62);
            this.lblChartNote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblChartNote.Name = "lblChartNote";
            this.lblChartNote.Size = new System.Drawing.Size(193, 20);
            this.lblChartNote.TabIndex = 3;
            this.lblChartNote.Text = "-";
            //
            // lblChartValue
            //
            this.lblChartValue.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblChartValue.Appearance.Options.UseFont = true;
            this.lblChartValue.Appearance.Options.UseTextOptions = true;
            this.lblChartValue.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblChartValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblChartValue.Location = new System.Drawing.Point(18, 28);
            this.lblChartValue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblChartValue.Name = "lblChartValue";
            this.lblChartValue.Size = new System.Drawing.Size(193, 32);
            this.lblChartValue.TabIndex = 2;
            this.lblChartValue.Text = "-";
            //
            // lblChartTitle
            //
            this.lblChartTitle.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this.lblChartTitle.Appearance.Options.UseFont = true;
            this.lblChartTitle.Appearance.Options.UseTextOptions = true;
            this.lblChartTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblChartTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblChartTitle.Location = new System.Drawing.Point(18, 8);
            this.lblChartTitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblChartTitle.Name = "lblChartTitle";
            this.lblChartTitle.Size = new System.Drawing.Size(193, 16);
            this.lblChartTitle.TabIndex = 1;
            this.lblChartTitle.Text = "HESAP PLANI / MİZAN";
            //
            // cardChartStrip
            //
            this.cardChartStrip.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardChartStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this.cardChartStrip.Location = new System.Drawing.Point(0, 0);
            this.cardChartStrip.Margin = new System.Windows.Forms.Padding(0);
            this.cardChartStrip.Name = "cardChartStrip";
            this.cardChartStrip.Size = new System.Drawing.Size(4, 92);
            this.cardChartStrip.TabIndex = 0;
            //
            // lblPreviewTitle
            //
            this.lblPreviewTitle.Appearance.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.lblPreviewTitle.Appearance.Options.UseFont = true;
            this.lblPreviewTitle.Appearance.Options.UseTextOptions = true;
            this.lblPreviewTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblPreviewTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPreviewTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPreviewTitle.Location = new System.Drawing.Point(0, 201);
            this.lblPreviewTitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblPreviewTitle.Name = "lblPreviewTitle";
            this.lblPreviewTitle.Padding = new System.Windows.Forms.Padding(26, 0, 0, 0);
            this.lblPreviewTitle.Size = new System.Drawing.Size(1000, 26);
            this.lblPreviewTitle.TabIndex = 4;
            this.lblPreviewTitle.Text = "DEVİR EDİLECEK KAYITLAR";
            //
            // panelFooter
            //
            this.panelFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelFooter.Controls.Add(this.grpOptions);
            this.panelFooter.Controls.Add(this.statusPanel);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 496);
            this.panelFooter.Margin = new System.Windows.Forms.Padding(0);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(1000, 204);
            this.panelFooter.TabIndex = 5;
            //
            // grpOptions
            //
            this.grpOptions.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.grpOptions.Controls.Add(this.flpOptions);
            this.grpOptions.Controls.Add(this.btnStart);
            this.grpOptions.Controls.Add(this.btnRefresh);
            this.grpOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpOptions.Location = new System.Drawing.Point(16, 12);
            this.grpOptions.Margin = new System.Windows.Forms.Padding(0);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.grpOptions.Size = new System.Drawing.Size(968, 148);
            this.grpOptions.TabIndex = 0;
            this.grpOptions.Text = "Devirde Aktarılacak Veriler";
            //
            // flpOptions
            //
            this.flpOptions.Controls.Add(this.chkAccounts);
            this.flpOptions.Controls.Add(this.chkTaxUnits);
            this.flpOptions.Controls.Add(this.chkCurrent);
            this.flpOptions.Controls.Add(this.chkProducts);
            this.flpOptions.Controls.Add(this.chkPrices);
            this.flpOptions.Controls.Add(this.chkRecipes);
            this.flpOptions.Controls.Add(this.chkChartBalances);
            this.flpOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpOptions.Location = new System.Drawing.Point(0, 0);
            this.flpOptions.Margin = new System.Windows.Forms.Padding(0);
            this.flpOptions.Name = "flpOptions";
            this.flpOptions.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flpOptions.Size = new System.Drawing.Size(966, 110);
            this.flpOptions.TabIndex = 0;
            this.flpOptions.WrapContents = true;
            //
            // chkAccounts
            //
            this.chkAccounts.Location = new System.Drawing.Point(0, 2);
            this.chkAccounts.Margin = new System.Windows.Forms.Padding(0, 0, 24, 4);
            this.chkAccounts.Name = "chkAccounts";
            this.chkAccounts.Properties.Caption = "Hesap planı (hesap kodu, ad, üst hesap)";
            this.chkAccounts.Size = new System.Drawing.Size(344, 22);
            this.chkAccounts.TabIndex = 0;
            //
            // chkTaxUnits
            //
            this.chkTaxUnits.Location = new System.Drawing.Point(368, 2);
            this.chkTaxUnits.Margin = new System.Windows.Forms.Padding(0, 0, 24, 4);
            this.chkTaxUnits.Name = "chkTaxUnits";
            this.chkTaxUnits.Properties.Caption = "Birim cinsi ve KDV oranları";
            this.chkTaxUnits.Size = new System.Drawing.Size(344, 22);
            this.chkTaxUnits.TabIndex = 1;
            //
            // chkCurrent
            //
            this.chkCurrent.Location = new System.Drawing.Point(0, 28);
            this.chkCurrent.Margin = new System.Windows.Forms.Padding(0, 0, 24, 4);
            this.chkCurrent.Name = "chkCurrent";
            this.chkCurrent.Properties.Caption = "Cari kartlar ve cari devir bakiyeleri (alacak / borç)";
            this.chkCurrent.Size = new System.Drawing.Size(344, 22);
            this.chkCurrent.TabIndex = 2;
            //
            // chkProducts
            //
            this.chkProducts.Location = new System.Drawing.Point(368, 28);
            this.chkProducts.Margin = new System.Windows.Forms.Padding(0, 0, 24, 4);
            this.chkProducts.Name = "chkProducts";
            this.chkProducts.Properties.Caption = "Stok kartları ve stok devir miktarı / değeri";
            this.chkProducts.Size = new System.Drawing.Size(344, 22);
            this.chkProducts.TabIndex = 3;
            //
            // chkPrices
            //
            this.chkPrices.Location = new System.Drawing.Point(0, 54);
            this.chkPrices.Margin = new System.Windows.Forms.Padding(0, 0, 24, 4);
            this.chkPrices.Name = "chkPrices";
            this.chkPrices.Properties.Caption = "Ürün fiyatları (FIFO/LIFO giriş katmanı)";
            this.chkPrices.Size = new System.Drawing.Size(344, 22);
            this.chkPrices.TabIndex = 4;
            //
            // chkRecipes
            //
            this.chkRecipes.Location = new System.Drawing.Point(368, 54);
            this.chkRecipes.Margin = new System.Windows.Forms.Padding(0, 0, 24, 4);
            this.chkRecipes.Name = "chkRecipes";
            this.chkRecipes.Properties.Caption = "Reçeteler";
            this.chkRecipes.Size = new System.Drawing.Size(344, 22);
            this.chkRecipes.TabIndex = 5;
            //
            // chkChartBalances
            //
            this.chkChartBalances.Location = new System.Drawing.Point(0, 80);
            this.chkChartBalances.Margin = new System.Windows.Forms.Padding(0, 0, 24, 4);
            this.chkChartBalances.Name = "chkChartBalances";
            this.chkChartBalances.Properties.Caption = "Hesap bakiyeleri (mizan açılışı)";
            this.chkChartBalances.Size = new System.Drawing.Size(344, 22);
            this.chkChartBalances.TabIndex = 6;
            //
            // btnStart
            //
            this.btnStart.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnStart.Appearance.Options.UseFont = true;
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right))));
            this.btnStart.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnStart.ImageOptions.SvgImage = DxIcon.Check;
            this.btnStart.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            this.btnStart.Location = new System.Drawing.Point(742, 104);
            this.btnStart.Margin = new System.Windows.Forms.Padding(0);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(212, 36);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Devir İşlemini Başlat";
            //
            // btnRefresh
            //
            this.btnRefresh.Appearance.Font = new Font("Segoe UI", 10F);
            this.btnRefresh.Appearance.Options.UseFont = true;
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right))));
            this.btnRefresh.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnRefresh.ImageOptions.SvgImage = DxIcon.Refresh;
            this.btnRefresh.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            this.btnRefresh.Location = new System.Drawing.Point(604, 104);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(130, 36);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Yenile";
            //
            // statusPanel
            //
            this.statusPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.statusPanel.Controls.Add(this.lblStatus);
            this.statusPanel.Controls.Add(this.statusAccentStrip);
            this.statusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusPanel.Location = new System.Drawing.Point(0, 160);
            this.statusPanel.Margin = new System.Windows.Forms.Padding(0);
            this.statusPanel.Name = "statusPanel";
            this.statusPanel.Size = new System.Drawing.Size(968, 44);
            this.statusPanel.TabIndex = 1;
            //
            // lblStatus
            //
            this.lblStatus.Appearance.Font = new Font("Segoe UI", 9.5F);
            this.lblStatus.Appearance.Options.UseFont = true;
            this.lblStatus.Appearance.Options.UseTextOptions = true;
            this.lblStatus.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblStatus.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Location = new System.Drawing.Point(4, 0);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblStatus.Size = new System.Drawing.Size(964, 44);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "-";
            //
            // statusAccentStrip
            //
            this.statusAccentStrip.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.statusAccentStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this.statusAccentStrip.Location = new System.Drawing.Point(0, 0);
            this.statusAccentStrip.Margin = new System.Windows.Forms.Padding(0);
            this.statusAccentStrip.Name = "statusAccentStrip";
            this.statusAccentStrip.Size = new System.Drawing.Size(4, 44);
            this.statusAccentStrip.TabIndex = 0;
            //
            // gridControl
            //
            this.gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl.Location = new System.Drawing.Point(0, 227);
            this.gridControl.MainView = this.gridView;
            this.gridControl.Name = "gridControl";
            this.gridControl.Size = new System.Drawing.Size(1000, 473);
            this.gridControl.TabIndex = 6;
            this.gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gridView });
            //
            // gridView
            //
            this.gridView.GridControl = this.gridControl;
            this.gridView.Name = "gridView";
            this.gridView.OptionsBehavior.AutoPopulateColumns = false;
            this.gridView.OptionsBehavior.Editable = false;
            this.gridView.OptionsSelection.MultiSelect = false;
            this.gridView.OptionsView.ColumnAutoWidth = true;
            this.gridView.OptionsView.EnableAppearanceEvenRow = true;
            this.gridView.OptionsView.EnableAppearanceOddRow = true;
            this.gridView.OptionsView.ShowGroupPanel = false;
            this.gridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;
            this.gridView.OptionsView.ShowIndicator = false;
            //
            // CarryForwardOperationsForm
            //
            // Docking sırası: önce Fill (grid), sonra alt şerit, sonra üst şerit.
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.gridControl);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.lblPreviewTitle);
            this.Controls.Add(this.summaryPanel);
            this.Controls.Add(this.headerDivider);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.accentBar);
            this.MinimumSize = new System.Drawing.Size(860, 600);
            this.Name = "CarryForwardOperationsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Devir İşlemleri";
            ((System.ComponentModel.ISupportInitialize)this.accentBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.headerPanel).EndInit();
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.picHeader.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.headerDivider).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.summaryPanel).EndInit();
            this.summaryPanel.ResumeLayout(false);
            this.summaryPanel.PerformLayout();
            this.summaryCards.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.cardStock).EndInit();
            this.cardStock.ResumeLayout(false);
            this.cardStock.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardStockStrip).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.cardReceivable).EndInit();
            this.cardReceivable.ResumeLayout(false);
            this.cardReceivable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardReceivableStrip).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.cardPayable).EndInit();
            this.cardPayable.ResumeLayout(false);
            this.cardPayable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardPayableStrip).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.cardChart).EndInit();
            this.cardChart.ResumeLayout(false);
            this.cardChart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.cardChartStrip).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.panelFooter).EndInit();
            this.panelFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.grpOptions).EndInit();
            this.grpOptions.ResumeLayout(false);
            this.flpOptions.ResumeLayout(false);
            this.flpOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.statusPanel).EndInit();
            this.statusPanel.ResumeLayout(false);
            this.statusPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.statusAccentStrip).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.gridView).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}