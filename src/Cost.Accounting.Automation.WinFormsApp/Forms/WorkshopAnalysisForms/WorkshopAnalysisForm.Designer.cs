using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.WorkshopAnalysisForms
{
partial class WorkshopAnalysisForm
    {
        /// <summary>
        /// Formun renk paleti. Görsel tanımların tamamı bu dosyada toplanır;
        /// kod-behind yalnızca koşula göre renk seçer, tanım yapmaz.
        /// </summary>
        private static readonly Color RevenueColor = Color.FromArgb(22, 160, 133);
        private static readonly Color ExpenseColor = Color.FromArgb(220, 53, 69);
        private static readonly Color NetColor = Color.FromArgb(111, 66, 193);

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Bu dosya formun TÜM görsel tanımını içerir ve Visual Studio tasarım
        /// modunda açılabilir. Başka bir dosyaya taşınmış görsel ayar
        /// (Properties.*, Appearance.*, Options*, Dock, Font, konum) bulunmaz;
        /// InitializeComponent dışında hiçbir metot çağrısı yapılmaz, çünkü
        /// tasarım modu bu çağrıları çalıştırmaz ve dosya yeniden üretildiğinde
        /// kaybolurlar.
        ///
        /// Yerleşim bilinçli olarak DOCK + TableLayoutPanel/FlowLayoutPanel ile
        /// kuruludur; kontrol konumları elle piksel olarak verilmez. Böylece
        /// pencere boyutu değiştiğinde grafikler ve KPI kartları birbirinin
        /// üstüne taşmaz.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkshopAnalysisForm));
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            btnClosePage = new DevExpress.XtraEditors.SimpleButton();
            pnlFilter = new DevExpress.XtraEditors.PanelControl();
            lblSummary = new DevExpress.XtraEditors.LabelControl();
            fltFilter = new FlowLayoutPanel();
            lblWorkshop = new DevExpress.XtraEditors.LabelControl();
            lookUpWorkshop = new DevExpress.XtraEditors.SearchLookUpEdit();
            lookUpWorkshopView = new DevExpress.XtraGrid.Views.Grid.GridView();
            lblFrom = new DevExpress.XtraEditors.LabelControl();
            dtFrom = new DevExpress.XtraEditors.DateEdit();
            lblTo = new DevExpress.XtraEditors.LabelControl();
            dtTo = new DevExpress.XtraEditors.DateEdit();
            btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            pnlFilterLine = new DevExpress.XtraEditors.PanelControl();
            pnlBody = new Panel();
            tblBody = new TableLayoutPanel();
            tblCharts = new TableLayoutPanel();
            pnlNetChart = new DevExpress.XtraEditors.PanelControl();
            chartNet = new DevExpress.XtraCharts.ChartControl();
            lblNetChartTitle = new DevExpress.XtraEditors.LabelControl();
            pnlTrendChart = new DevExpress.XtraEditors.PanelControl();
            chartTrend = new DevExpress.XtraCharts.ChartControl();
            lblTrendChartTitle = new DevExpress.XtraEditors.LabelControl();
            pnlCompareChart = new DevExpress.XtraEditors.PanelControl();
            chartCompare = new DevExpress.XtraCharts.ChartControl();
            lblCompareChartTitle = new DevExpress.XtraEditors.LabelControl();
            pnlExpenseChart = new DevExpress.XtraEditors.PanelControl();
            chartExpense = new DevExpress.XtraCharts.ChartControl();
            lblExpenseChartTitle = new DevExpress.XtraEditors.LabelControl();
            lblUnattributed = new DevExpress.XtraEditors.LabelControl();
            tblKpi = new TableLayoutPanel();
            kpiRevenue = new DevExpress.XtraEditors.PanelControl();
            lblRevenueTitle = new DevExpress.XtraEditors.LabelControl();
            lblRevenueValue = new DevExpress.XtraEditors.LabelControl();
            accRevenue = new Panel();
            kpiExpense = new DevExpress.XtraEditors.PanelControl();
            lblExpenseTitle = new DevExpress.XtraEditors.LabelControl();
            lblExpenseValue = new DevExpress.XtraEditors.LabelControl();
            accExpense = new Panel();
            kpiNet = new DevExpress.XtraEditors.PanelControl();
            lblNetTitle = new DevExpress.XtraEditors.LabelControl();
            lblNetValue = new DevExpress.XtraEditors.LabelControl();
            accNet = new Panel();
            kpiRatio = new DevExpress.XtraEditors.PanelControl();
            lblRatioTitle = new DevExpress.XtraEditors.LabelControl();
            lblRatioValue = new DevExpress.XtraEditors.LabelControl();
            accRatio = new Panel();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFilter).BeginInit();
            pnlFilter.SuspendLayout();
            fltFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)lookUpWorkshop.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookUpWorkshopView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtFrom.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtFrom.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtTo.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtTo.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFilterLine).BeginInit();
            pnlBody.SuspendLayout();
            tblBody.SuspendLayout();
            tblCharts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlNetChart).BeginInit();
            pnlNetChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartNet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlTrendChart).BeginInit();
            pnlTrendChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartTrend).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlCompareChart).BeginInit();
            pnlCompareChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartCompare).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlExpenseChart).BeginInit();
            pnlExpenseChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartExpense).BeginInit();
            tblKpi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpiRevenue).BeginInit();
            kpiRevenue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpiExpense).BeginInit();
            kpiExpense.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpiNet).BeginInit();
            kpiNet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpiRatio).BeginInit();
            kpiRatio.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(btnClosePage);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 2, 3, 2);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(15, 0, 15, 0);
            pnlHeader.Size = new Size(1070, 49);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(120, 120, 120);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(48, 32);
            lblSubtitle.Margin = new Padding(3, 2, 3, 2);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(310, 13);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "Gider: onaylı pusula kalemleri   |   Gelir: onaylı satış faturaları";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new Point(48, 8);
            lblTitle.Margin = new Padding(3, 2, 3, 2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(202, 23);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Atölye Gelir / Gider Analizi";
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblHeaderIcon.ImageOptions.SvgImage");
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(30, 30);
            lblHeaderIcon.Location = new Point(15, 14);
            lblHeaderIcon.Margin = new Padding(3, 2, 3, 2);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(30, 30);
            lblHeaderIcon.TabIndex = 2;
            // 
            // btnClosePage
            // 
            btnClosePage.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClosePage.Appearance.Options.UseFont = true;
            btnClosePage.Dock = DockStyle.Right;
            btnClosePage.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnClosePage.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnClosePage.ImageOptions.SvgImage");
            btnClosePage.ImageOptions.SvgImageSize = new Size(16, 16);
            btnClosePage.Location = new Point(974, 0);
            btnClosePage.Margin = new Padding(3, 2, 3, 2);
            btnClosePage.Name = "btnClosePage";
            btnClosePage.Size = new Size(81, 49);
            btnClosePage.TabIndex = 3;
            btnClosePage.Text = "Kapat";
            btnClosePage.Click += BtnClosePage_Click;
            // 
            // pnlFilter
            // 
            pnlFilter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFilter.Controls.Add(lblSummary);
            pnlFilter.Controls.Add(fltFilter);
            pnlFilter.Controls.Add(pnlFilterLine);
            pnlFilter.Dock = DockStyle.Top;
            pnlFilter.Location = new Point(0, 49);
            pnlFilter.Margin = new Padding(3, 2, 3, 2);
            pnlFilter.Name = "pnlFilter";
            pnlFilter.Padding = new Padding(15, 6, 15, 6);
            pnlFilter.Size = new Size(1070, 57);
            pnlFilter.TabIndex = 1;
            // 
            // lblSummary
            // 
            lblSummary.Appearance.Font = new Font("Segoe UI", 9F);
            lblSummary.Appearance.ForeColor = Color.FromArgb(110, 110, 110);
            lblSummary.Appearance.Options.UseFont = true;
            lblSummary.Appearance.Options.UseForeColor = true;
            lblSummary.Dock = DockStyle.Fill;
            lblSummary.Location = new Point(15, 27);
            lblSummary.Margin = new Padding(3, 2, 3, 2);
            lblSummary.Name = "lblSummary";
            lblSummary.Size = new Size(3, 15);
            lblSummary.TabIndex = 2;
            lblSummary.Text = " ";
            // 
            // fltFilter
            // 
            fltFilter.Controls.Add(lblWorkshop);
            fltFilter.Controls.Add(lookUpWorkshop);
            fltFilter.Controls.Add(lblFrom);
            fltFilter.Controls.Add(dtFrom);
            fltFilter.Controls.Add(lblTo);
            fltFilter.Controls.Add(dtTo);
            fltFilter.Controls.Add(btnRefresh);
            fltFilter.Dock = DockStyle.Top;
            fltFilter.Location = new Point(15, 6);
            fltFilter.Margin = new Padding(3, 2, 3, 2);
            fltFilter.Name = "fltFilter";
            fltFilter.Size = new Size(1040, 21);
            fltFilter.TabIndex = 1;
            fltFilter.WrapContents = false;
            // 
            // lblWorkshop
            // 
            lblWorkshop.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblWorkshop.Appearance.Options.UseFont = true;
            lblWorkshop.Location = new Point(3, 2);
            lblWorkshop.Margin = new Padding(3, 2, 3, 2);
            lblWorkshop.Name = "lblWorkshop";
            lblWorkshop.Size = new Size(39, 15);
            lblWorkshop.TabIndex = 0;
            lblWorkshop.Text = "Atölye:";
            // 
            // lookUpWorkshop
            // 
            lookUpWorkshop.Location = new Point(48, 2);
            lookUpWorkshop.Margin = new Padding(3, 2, 3, 2);
            lookUpWorkshop.Name = "lookUpWorkshop";
            lookUpWorkshop.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Search) });
            lookUpWorkshop.Properties.DisplayMember = "DisplayName";
            lookUpWorkshop.Properties.NullValuePrompt = "(Tüm Atölyeler)";
            lookUpWorkshop.Properties.PopupView = lookUpWorkshopView;
            lookUpWorkshop.Properties.ValueMember = "WorkshopId";
            lookUpWorkshop.Size = new Size(257, 20);
            lookUpWorkshop.TabIndex = 1;
            // 
            // lookUpWorkshopView
            // 
            lookUpWorkshopView.DetailHeight = 232;
            lookUpWorkshopView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            lookUpWorkshopView.Name = "lookUpWorkshopView";
            lookUpWorkshopView.OptionsEditForm.PopupEditFormWidth = 686;
            lookUpWorkshopView.OptionsSelection.EnableAppearanceFocusedCell = false;
            lookUpWorkshopView.OptionsView.ShowGroupPanel = false;
            lookUpWorkshopView.RowHeight = 18;
            // 
            // lblFrom
            // 
            lblFrom.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFrom.Appearance.Options.UseFont = true;
            lblFrom.Location = new Point(311, 2);
            lblFrom.Margin = new Padding(3, 2, 3, 2);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(54, 15);
            lblFrom.TabIndex = 2;
            lblFrom.Text = "Başlangıç:";
            // 
            // dtFrom
            // 
            dtFrom.EditValue = null;
            dtFrom.Location = new Point(371, 2);
            dtFrom.Margin = new Padding(3, 2, 3, 2);
            dtFrom.Name = "dtFrom";
            dtFrom.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            dtFrom.Size = new Size(103, 20);
            dtFrom.TabIndex = 3;
            // 
            // lblTo
            // 
            lblTo.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTo.Appearance.Options.UseFont = true;
            lblTo.Location = new Point(480, 2);
            lblTo.Margin = new Padding(3, 2, 3, 2);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(27, 15);
            lblTo.TabIndex = 4;
            lblTo.Text = "Bitiş:";
            // 
            // dtTo
            // 
            dtTo.EditValue = null;
            dtTo.Location = new Point(513, 2);
            dtTo.Margin = new Padding(3, 2, 3, 2);
            dtTo.Name = "dtTo";
            dtTo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            dtTo.Size = new Size(103, 20);
            dtTo.TabIndex = 5;
            // 
            // btnRefresh
            // 
            btnRefresh.Appearance.Font = new Font("Segoe UI", 9F);
            btnRefresh.Appearance.Options.UseFont = true;
            btnRefresh.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnRefresh.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnRefresh.ImageOptions.SvgImage");
            btnRefresh.ImageOptions.SvgImageSize = new Size(16, 16);
            btnRefresh.Location = new Point(622, 2);
            btnRefresh.Margin = new Padding(3, 2, 3, 2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(72, 20);
            btnRefresh.TabIndex = 6;
            btnRefresh.Text = "Yenile";
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // pnlFilterLine
            // 
            pnlFilterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFilterLine.Dock = DockStyle.Bottom;
            pnlFilterLine.Location = new Point(15, 50);
            pnlFilterLine.Margin = new Padding(3, 2, 3, 2);
            pnlFilterLine.Name = "pnlFilterLine";
            pnlFilterLine.Size = new Size(1040, 1);
            pnlFilterLine.TabIndex = 0;
            // 
            // pnlBody
            // 
            pnlBody.Controls.Add(tblBody);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(0, 106);
            pnlBody.Margin = new Padding(3, 2, 3, 2);
            pnlBody.Name = "pnlBody";
            pnlBody.Size = new Size(1070, 603);
            pnlBody.TabIndex = 2;
            // 
            // tblBody
            // 
            tblBody.ColumnCount = 1;
            tblBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblBody.Controls.Add(tblCharts, 0, 3);
            tblBody.Controls.Add(lblUnattributed, 0, 2);
            tblBody.Controls.Add(tblKpi, 0, 0);
            tblBody.Dock = DockStyle.Fill;
            tblBody.Location = new Point(0, 0);
            tblBody.Margin = new Padding(0);
            tblBody.Name = "tblBody";
            tblBody.Padding = new Padding(0, 0, 0, 13);
            tblBody.RowCount = 4;
            tblBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            tblBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 3F));
            tblBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 17F));
            tblBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBody.Size = new Size(1070, 603);
            tblBody.TabIndex = 0;
            // 
            // tblCharts
            // 
            tblCharts.ColumnCount = 2;
            tblCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblCharts.Controls.Add(pnlNetChart, 0, 0);
            tblCharts.Controls.Add(pnlTrendChart, 1, 0);
            tblCharts.Controls.Add(pnlCompareChart, 0, 1);
            tblCharts.Controls.Add(pnlExpenseChart, 1, 1);
            tblCharts.Dock = DockStyle.Fill;
            tblCharts.Location = new Point(0, 86);
            tblCharts.Margin = new Padding(0);
            tblCharts.Name = "tblCharts";
            tblCharts.RowCount = 2;
            tblCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblCharts.Size = new Size(1070, 504);
            tblCharts.TabIndex = 2;
            // 
            // pnlNetChart
            // 
            pnlNetChart.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlNetChart.Controls.Add(chartNet);
            pnlNetChart.Controls.Add(lblNetChartTitle);
            pnlNetChart.Location = new Point(0, 0);
            pnlNetChart.Margin = new Padding(0, 0, 5, 5);
            pnlNetChart.Name = "pnlNetChart";
            pnlNetChart.Size = new Size(530, 247);
            pnlNetChart.TabIndex = 0;
            // 
            // chartNet
            // 
            chartNet.Dock = DockStyle.Fill;
            chartNet.Location = new Point(2, 26);
            chartNet.Margin = new Padding(3, 2, 3, 2);
            chartNet.Name = "chartNet";
            chartNet.Size = new Size(526, 219);
            chartNet.TabIndex = 1;
            // 
            // lblNetChartTitle
            // 
            lblNetChartTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblNetChartTitle.Appearance.Options.UseFont = true;
            lblNetChartTitle.Dock = DockStyle.Top;
            lblNetChartTitle.Location = new Point(2, 2);
            lblNetChartTitle.Margin = new Padding(3, 2, 3, 2);
            lblNetChartTitle.Name = "lblNetChartTitle";
            lblNetChartTitle.Padding = new Padding(11, 7, 11, 0);
            lblNetChartTitle.Size = new Size(172, 24);
            lblNetChartTitle.TabIndex = 0;
            lblNetChartTitle.Text = "Aylık Fark (Gelir - Gider)";
            // 
            // pnlTrendChart
            // 
            pnlTrendChart.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlTrendChart.Controls.Add(chartTrend);
            pnlTrendChart.Controls.Add(lblTrendChartTitle);
            pnlTrendChart.Location = new Point(535, 0);
            pnlTrendChart.Margin = new Padding(0);
            pnlTrendChart.Name = "pnlTrendChart";
            pnlTrendChart.Size = new Size(535, 252);
            pnlTrendChart.TabIndex = 1;
            // 
            // chartTrend
            // 
            chartTrend.Dock = DockStyle.Fill;
            chartTrend.Location = new Point(2, 26);
            chartTrend.Margin = new Padding(3, 2, 3, 2);
            chartTrend.Name = "chartTrend";
            chartTrend.Size = new Size(531, 224);
            chartTrend.TabIndex = 1;
            // 
            // lblTrendChartTitle
            // 
            lblTrendChartTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTrendChartTitle.Appearance.Options.UseFont = true;
            lblTrendChartTitle.Dock = DockStyle.Top;
            lblTrendChartTitle.Location = new Point(2, 2);
            lblTrendChartTitle.Margin = new Padding(3, 2, 3, 2);
            lblTrendChartTitle.Name = "lblTrendChartTitle";
            lblTrendChartTitle.Padding = new Padding(11, 7, 11, 0);
            lblTrendChartTitle.Size = new Size(177, 24);
            lblTrendChartTitle.TabIndex = 0;
            lblTrendChartTitle.Text = "Aylık Gelir / Gider Trendi";
            // 
            // pnlCompareChart
            // 
            pnlCompareChart.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlCompareChart.Controls.Add(chartCompare);
            pnlCompareChart.Controls.Add(lblCompareChartTitle);
            pnlCompareChart.Location = new Point(0, 257);
            pnlCompareChart.Margin = new Padding(0, 5, 5, 0);
            pnlCompareChart.Name = "pnlCompareChart";
            pnlCompareChart.Size = new Size(530, 247);
            pnlCompareChart.TabIndex = 2;
            // 
            // chartCompare
            // 
            chartCompare.Dock = DockStyle.Fill;
            chartCompare.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            chartCompare.Location = new Point(2, 26);
            chartCompare.Margin = new Padding(3, 2, 3, 2);
            chartCompare.Name = "chartCompare";
            chartCompare.Size = new Size(526, 219);
            chartCompare.TabIndex = 1;
            // 
            // lblCompareChartTitle
            // 
            lblCompareChartTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCompareChartTitle.Appearance.Options.UseFont = true;
            lblCompareChartTitle.Dock = DockStyle.Top;
            lblCompareChartTitle.Location = new Point(2, 2);
            lblCompareChartTitle.Margin = new Padding(3, 2, 3, 2);
            lblCompareChartTitle.Name = "lblCompareChartTitle";
            lblCompareChartTitle.Padding = new Padding(11, 7, 11, 0);
            lblCompareChartTitle.Size = new Size(203, 24);
            lblCompareChartTitle.TabIndex = 0;
            lblCompareChartTitle.Text = "Atölye Karşılaştırması (Gider)";
            // 
            // pnlExpenseChart
            // 
            pnlExpenseChart.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlExpenseChart.Controls.Add(chartExpense);
            pnlExpenseChart.Controls.Add(lblExpenseChartTitle);
            pnlExpenseChart.Location = new Point(535, 257);
            pnlExpenseChart.Margin = new Padding(0, 5, 0, 0);
            pnlExpenseChart.Name = "pnlExpenseChart";
            pnlExpenseChart.Size = new Size(535, 247);
            pnlExpenseChart.TabIndex = 3;
            // 
            // chartExpense
            // 
            chartExpense.Dock = DockStyle.Fill;
            chartExpense.Location = new Point(2, 26);
            chartExpense.Margin = new Padding(3, 2, 3, 2);
            chartExpense.Name = "chartExpense";
            chartExpense.Size = new Size(531, 219);
            chartExpense.TabIndex = 1;
            // 
            // lblExpenseChartTitle
            // 
            lblExpenseChartTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblExpenseChartTitle.Appearance.Options.UseFont = true;
            lblExpenseChartTitle.Dock = DockStyle.Top;
            lblExpenseChartTitle.Location = new Point(2, 2);
            lblExpenseChartTitle.Margin = new Padding(3, 2, 3, 2);
            lblExpenseChartTitle.Name = "lblExpenseChartTitle";
            lblExpenseChartTitle.Padding = new Padding(11, 7, 11, 0);
            lblExpenseChartTitle.Size = new Size(192, 24);
            lblExpenseChartTitle.TabIndex = 0;
            lblExpenseChartTitle.Text = "Gider Dağılımı (Hesap Tipi)";
            // 
            // lblUnattributed
            // 
            lblUnattributed.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblUnattributed.Appearance.ForeColor = Color.FromArgb(150, 110, 20);
            lblUnattributed.Appearance.Options.UseFont = true;
            lblUnattributed.Appearance.Options.UseForeColor = true;
            lblUnattributed.Dock = DockStyle.Fill;
            lblUnattributed.Location = new Point(0, 69);
            lblUnattributed.Margin = new Padding(0);
            lblUnattributed.Name = "lblUnattributed";
            lblUnattributed.Size = new Size(1070, 17);
            lblUnattributed.TabIndex = 1;
            lblUnattributed.Text = " ";
            // 
            // tblKpi
            // 
            tblKpi.ColumnCount = 4;
            tblKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblKpi.Controls.Add(kpiRevenue, 0, 0);
            tblKpi.Controls.Add(kpiExpense, 1, 0);
            tblKpi.Controls.Add(kpiNet, 2, 0);
            tblKpi.Controls.Add(kpiRatio, 3, 0);
            tblKpi.Dock = DockStyle.Fill;
            tblKpi.Location = new Point(0, 0);
            tblKpi.Margin = new Padding(0);
            tblKpi.Name = "tblKpi";
            tblKpi.RowCount = 1;
            tblKpi.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblKpi.Size = new Size(1070, 66);
            tblKpi.TabIndex = 0;
            // 
            // kpiRevenue
            // 
            kpiRevenue.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            kpiRevenue.Controls.Add(lblRevenueTitle);
            kpiRevenue.Controls.Add(lblRevenueValue);
            kpiRevenue.Controls.Add(accRevenue);
            kpiRevenue.Location = new Point(0, 0);
            kpiRevenue.Margin = new Padding(0, 0, 5, 0);
            kpiRevenue.Name = "kpiRevenue";
            kpiRevenue.Size = new Size(262, 66);
            kpiRevenue.TabIndex = 0;
            // 
            // lblRevenueTitle
            // 
            lblRevenueTitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblRevenueTitle.Appearance.ForeColor = Color.FromArgb(120, 120, 120);
            lblRevenueTitle.Appearance.Options.UseFont = true;
            lblRevenueTitle.Appearance.Options.UseForeColor = true;
            lblRevenueTitle.Dock = DockStyle.Fill;
            lblRevenueTitle.Location = new Point(6, 44);
            lblRevenueTitle.Margin = new Padding(3, 2, 3, 2);
            lblRevenueTitle.Name = "lblRevenueTitle";
            lblRevenueTitle.Padding = new Padding(15, 2, 7, 6);
            lblRevenueTitle.Size = new Size(103, 23);
            lblRevenueTitle.TabIndex = 2;
            lblRevenueTitle.Text = "TOPLAM GELİR";
            // 
            // lblRevenueValue
            // 
            lblRevenueValue.Appearance.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            lblRevenueValue.Appearance.ForeColor = Color.FromArgb(22, 160, 133);
            lblRevenueValue.Appearance.Options.UseFont = true;
            lblRevenueValue.Appearance.Options.UseForeColor = true;
            lblRevenueValue.Dock = DockStyle.Top;
            lblRevenueValue.Location = new Point(6, 2);
            lblRevenueValue.Margin = new Padding(3, 2, 3, 2);
            lblRevenueValue.Name = "lblRevenueValue";
            lblRevenueValue.Padding = new Padding(15, 11, 7, 0);
            lblRevenueValue.Size = new Size(31, 42);
            lblRevenueValue.TabIndex = 1;
            lblRevenueValue.Text = "-";
            // 
            // accRevenue
            // 
            accRevenue.BackColor = Color.FromArgb(22, 160, 133);
            accRevenue.Dock = DockStyle.Left;
            accRevenue.Location = new Point(2, 2);
            accRevenue.Margin = new Padding(3, 2, 3, 2);
            accRevenue.Name = "accRevenue";
            accRevenue.Size = new Size(4, 62);
            accRevenue.TabIndex = 0;
            // 
            // kpiExpense
            // 
            kpiExpense.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            kpiExpense.Controls.Add(lblExpenseTitle);
            kpiExpense.Controls.Add(lblExpenseValue);
            kpiExpense.Controls.Add(accExpense);
            kpiExpense.Location = new Point(267, 0);
            kpiExpense.Margin = new Padding(0);
            kpiExpense.Name = "kpiExpense";
            kpiExpense.Size = new Size(267, 66);
            kpiExpense.TabIndex = 1;
            // 
            // lblExpenseTitle
            // 
            lblExpenseTitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblExpenseTitle.Appearance.ForeColor = Color.FromArgb(120, 120, 120);
            lblExpenseTitle.Appearance.Options.UseFont = true;
            lblExpenseTitle.Appearance.Options.UseForeColor = true;
            lblExpenseTitle.Dock = DockStyle.Fill;
            lblExpenseTitle.Location = new Point(6, 44);
            lblExpenseTitle.Margin = new Padding(3, 2, 3, 2);
            lblExpenseTitle.Name = "lblExpenseTitle";
            lblExpenseTitle.Padding = new Padding(15, 2, 7, 6);
            lblExpenseTitle.Size = new Size(105, 23);
            lblExpenseTitle.TabIndex = 2;
            lblExpenseTitle.Text = "TOPLAM GİDER";
            // 
            // lblExpenseValue
            // 
            lblExpenseValue.Appearance.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            lblExpenseValue.Appearance.ForeColor = Color.FromArgb(220, 53, 69);
            lblExpenseValue.Appearance.Options.UseFont = true;
            lblExpenseValue.Appearance.Options.UseForeColor = true;
            lblExpenseValue.Dock = DockStyle.Top;
            lblExpenseValue.Location = new Point(6, 2);
            lblExpenseValue.Margin = new Padding(3, 2, 3, 2);
            lblExpenseValue.Name = "lblExpenseValue";
            lblExpenseValue.Padding = new Padding(15, 11, 7, 0);
            lblExpenseValue.Size = new Size(31, 42);
            lblExpenseValue.TabIndex = 1;
            lblExpenseValue.Text = "-";
            // 
            // accExpense
            // 
            accExpense.BackColor = Color.FromArgb(220, 53, 69);
            accExpense.Dock = DockStyle.Left;
            accExpense.Location = new Point(2, 2);
            accExpense.Margin = new Padding(3, 2, 3, 2);
            accExpense.Name = "accExpense";
            accExpense.Size = new Size(4, 62);
            accExpense.TabIndex = 0;
            // 
            // kpiNet
            // 
            kpiNet.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            kpiNet.Controls.Add(lblNetTitle);
            kpiNet.Controls.Add(lblNetValue);
            kpiNet.Controls.Add(accNet);
            kpiNet.Location = new Point(534, 0);
            kpiNet.Margin = new Padding(0);
            kpiNet.Name = "kpiNet";
            kpiNet.Size = new Size(267, 66);
            kpiNet.TabIndex = 2;
            // 
            // lblNetTitle
            // 
            lblNetTitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblNetTitle.Appearance.ForeColor = Color.FromArgb(120, 120, 120);
            lblNetTitle.Appearance.Options.UseFont = true;
            lblNetTitle.Appearance.Options.UseForeColor = true;
            lblNetTitle.Dock = DockStyle.Fill;
            lblNetTitle.Location = new Point(6, 44);
            lblNetTitle.Margin = new Padding(3, 2, 3, 2);
            lblNetTitle.Name = "lblNetTitle";
            lblNetTitle.Padding = new Padding(15, 2, 7, 6);
            lblNetTitle.Size = new Size(83, 23);
            lblNetTitle.TabIndex = 2;
            lblNetTitle.Text = "FARK (NET)";
            // 
            // lblNetValue
            // 
            lblNetValue.Appearance.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            lblNetValue.Appearance.ForeColor = Color.FromArgb(22, 160, 133);
            lblNetValue.Appearance.Options.UseFont = true;
            lblNetValue.Appearance.Options.UseForeColor = true;
            lblNetValue.Dock = DockStyle.Top;
            lblNetValue.Location = new Point(6, 2);
            lblNetValue.Margin = new Padding(3, 2, 3, 2);
            lblNetValue.Name = "lblNetValue";
            lblNetValue.Padding = new Padding(15, 11, 7, 0);
            lblNetValue.Size = new Size(31, 42);
            lblNetValue.TabIndex = 1;
            lblNetValue.Text = "-";
            // 
            // accNet
            // 
            accNet.BackColor = Color.FromArgb(111, 66, 193);
            accNet.Dock = DockStyle.Left;
            accNet.Location = new Point(2, 2);
            accNet.Margin = new Padding(3, 2, 3, 2);
            accNet.Name = "accNet";
            accNet.Size = new Size(4, 62);
            accNet.TabIndex = 0;
            // 
            // kpiRatio
            // 
            kpiRatio.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            kpiRatio.Controls.Add(lblRatioTitle);
            kpiRatio.Controls.Add(lblRatioValue);
            kpiRatio.Controls.Add(accRatio);
            kpiRatio.Location = new Point(806, 0);
            kpiRatio.Margin = new Padding(5, 0, 0, 0);
            kpiRatio.Name = "kpiRatio";
            kpiRatio.Size = new Size(262, 66);
            kpiRatio.TabIndex = 3;
            // 
            // lblRatioTitle
            // 
            lblRatioTitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblRatioTitle.Appearance.ForeColor = Color.FromArgb(120, 120, 120);
            lblRatioTitle.Appearance.Options.UseFont = true;
            lblRatioTitle.Appearance.Options.UseForeColor = true;
            lblRatioTitle.Dock = DockStyle.Fill;
            lblRatioTitle.Location = new Point(6, 44);
            lblRatioTitle.Margin = new Padding(3, 2, 3, 2);
            lblRatioTitle.Name = "lblRatioTitle";
            lblRatioTitle.Padding = new Padding(15, 2, 7, 6);
            lblRatioTitle.Size = new Size(134, 23);
            lblRatioTitle.TabIndex = 2;
            lblRatioTitle.Text = "GİDER / GELİR ORANI";
            // 
            // lblRatioValue
            // 
            lblRatioValue.Appearance.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            lblRatioValue.Appearance.ForeColor = Color.FromArgb(237, 108, 2);
            lblRatioValue.Appearance.Options.UseFont = true;
            lblRatioValue.Appearance.Options.UseForeColor = true;
            lblRatioValue.Dock = DockStyle.Top;
            lblRatioValue.Location = new Point(6, 2);
            lblRatioValue.Margin = new Padding(3, 2, 3, 2);
            lblRatioValue.Name = "lblRatioValue";
            lblRatioValue.Padding = new Padding(15, 11, 7, 0);
            lblRatioValue.Size = new Size(31, 42);
            lblRatioValue.TabIndex = 1;
            lblRatioValue.Text = "-";
            // 
            // accRatio
            // 
            accRatio.BackColor = Color.FromArgb(237, 108, 2);
            accRatio.Dock = DockStyle.Left;
            accRatio.Location = new Point(2, 2);
            accRatio.Margin = new Padding(3, 2, 3, 2);
            accRatio.Name = "accRatio";
            accRatio.Size = new Size(4, 62);
            accRatio.TabIndex = 0;
            // 
            // WorkshopAnalysisForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1070, 709);
            Controls.Add(pnlBody);
            Controls.Add(pnlFilter);
            Controls.Add(pnlHeader);
            Margin = new Padding(3, 2, 3, 2);
            Name = "WorkshopAnalysisForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Atölye Gelir / Gider Analizi";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFilter).EndInit();
            pnlFilter.ResumeLayout(false);
            pnlFilter.PerformLayout();
            fltFilter.ResumeLayout(false);
            fltFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)lookUpWorkshop.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookUpWorkshopView).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtFrom.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtFrom.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtTo.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtTo.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFilterLine).EndInit();
            pnlBody.ResumeLayout(false);
            tblBody.ResumeLayout(false);
            tblBody.PerformLayout();
            tblCharts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlNetChart).EndInit();
            pnlNetChart.ResumeLayout(false);
            pnlNetChart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chartNet).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlTrendChart).EndInit();
            pnlTrendChart.ResumeLayout(false);
            pnlTrendChart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chartTrend).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlCompareChart).EndInit();
            pnlCompareChart.ResumeLayout(false);
            pnlCompareChart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chartCompare).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlExpenseChart).EndInit();
            pnlExpenseChart.ResumeLayout(false);
            pnlExpenseChart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chartExpense).EndInit();
            tblKpi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kpiRevenue).EndInit();
            kpiRevenue.ResumeLayout(false);
            kpiRevenue.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kpiExpense).EndInit();
            kpiExpense.ResumeLayout(false);
            kpiExpense.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kpiNet).EndInit();
            kpiNet.ResumeLayout(false);
            kpiNet.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kpiRatio).EndInit();
            kpiRatio.ResumeLayout(false);
            kpiRatio.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.SimpleButton btnClosePage;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;

        private DevExpress.XtraEditors.PanelControl pnlFilter;
        private DevExpress.XtraEditors.PanelControl pnlFilterLine;
        private System.Windows.Forms.FlowLayoutPanel fltFilter;
        private DevExpress.XtraEditors.LabelControl lblWorkshop;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpWorkshop;
        private DevExpress.XtraGrid.Views.Grid.GridView lookUpWorkshopView;
        private DevExpress.XtraEditors.LabelControl lblFrom;
        private DevExpress.XtraEditors.DateEdit dtFrom;
        private DevExpress.XtraEditors.LabelControl lblTo;
        private DevExpress.XtraEditors.DateEdit dtTo;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
        private DevExpress.XtraEditors.LabelControl lblSummary;

        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.TableLayoutPanel tblBody;
        private System.Windows.Forms.TableLayoutPanel tblKpi;
        private DevExpress.XtraEditors.PanelControl kpiRevenue;
        private System.Windows.Forms.Panel accRevenue;
        private DevExpress.XtraEditors.LabelControl lblRevenueValue;
        private DevExpress.XtraEditors.LabelControl lblRevenueTitle;
        private DevExpress.XtraEditors.PanelControl kpiExpense;
        private System.Windows.Forms.Panel accExpense;
        private DevExpress.XtraEditors.LabelControl lblExpenseValue;
        private DevExpress.XtraEditors.LabelControl lblExpenseTitle;
        private DevExpress.XtraEditors.PanelControl kpiNet;
        private System.Windows.Forms.Panel accNet;
        private DevExpress.XtraEditors.LabelControl lblNetValue;
        private DevExpress.XtraEditors.LabelControl lblNetTitle;
        private DevExpress.XtraEditors.PanelControl kpiRatio;
        private System.Windows.Forms.Panel accRatio;
        private DevExpress.XtraEditors.LabelControl lblRatioValue;
        private DevExpress.XtraEditors.LabelControl lblRatioTitle;
        private DevExpress.XtraEditors.LabelControl lblUnattributed;

        private System.Windows.Forms.TableLayoutPanel tblCharts;
        private DevExpress.XtraEditors.PanelControl pnlNetChart;
        private DevExpress.XtraEditors.LabelControl lblNetChartTitle;
        private DevExpress.XtraCharts.ChartControl chartNet;
        private DevExpress.XtraEditors.PanelControl pnlTrendChart;
        private DevExpress.XtraEditors.LabelControl lblTrendChartTitle;
        private DevExpress.XtraCharts.ChartControl chartTrend;
        private DevExpress.XtraEditors.PanelControl pnlCompareChart;
        private DevExpress.XtraEditors.LabelControl lblCompareChartTitle;
        private DevExpress.XtraCharts.ChartControl chartCompare;
        private DevExpress.XtraEditors.PanelControl pnlExpenseChart;
        private DevExpress.XtraEditors.LabelControl lblExpenseChartTitle;
        private DevExpress.XtraCharts.ChartControl chartExpense;
    }
}