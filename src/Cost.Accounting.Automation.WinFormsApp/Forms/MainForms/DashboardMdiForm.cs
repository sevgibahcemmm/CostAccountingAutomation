using Cost.Accounting.Automation.Application.Dashboards;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class DashboardMdiForm : XtraFormMdiBase
    {
        private const int DashboardAutoRefreshIntervalMs = 30_000;

        private readonly SessionClaimContext _session;
        private readonly Dictionary<int, Label> _kpiValues = new();
        private readonly System.Windows.Forms.Timer _refreshTimer;
        private bool _refreshing;
        private bool _hasLoadedOnce;
        private bool _tableColumnsConfigured;
        private string _lastFingerprint = string.Empty;

        public DashboardMdiForm() : base("Dashboard")
        {
            _session =
                Program.Services
                    .GetRequiredService<SessionClaimContext>();

            Size =
                new Size(1280, 720);

            MinimumSize =
                new Size(1024, 640);

            IconOptions.SvgImage =
                DxIcon.Home;

            InitializeComponent();

            btnRefresh.Click += BtnRefresh_Click;
            btnRefresh.ImageOptions.SvgImage = DxIcon.Refresh;

            _kpiValues[1] = lblKpi1Value;
            _kpiValues[2] = lblKpi2Value;
            _kpiValues[3] = lblKpi3Value;
            _kpiValues[4] = lblKpi4Value;
            _kpiValues[5] = lblKpi5Value;
            _kpiValues[6] = lblKpi6Value;
            _kpiValues[7] = lblKpi7Value;
            _kpiValues[8] = lblKpi8Value;

            _refreshTimer =
                new System.Windows.Forms.Timer
                {
                    Interval = DashboardAutoRefreshIntervalMs
                };

            _refreshTimer.Tick +=
                RefreshDashboardTimer_Tick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LoadSessionInfo();

            _ = LoadDashboardDataAsync();

            _refreshTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _refreshTimer.Stop();
            _refreshTimer.Tick -= RefreshDashboardTimer_Tick;
            _refreshTimer.Dispose();

            base.OnFormClosed(e);
        }

        private async void RefreshDashboardTimer_Tick(object? sender, EventArgs e)
        {
            if (_refreshing || Disposing || IsDisposed)
            {
                return;
            }

            _refreshing = true;
            try
            {
                await LoadDashboardDataAsync(quiet: true, forceRefresh: false);
            }
            finally
            {
                _refreshing = false;
            }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            if (_refreshing || Disposing || IsDisposed)
            {
                return;
            }

            btnRefresh.Enabled = false;
            _refreshing = true;
            UpdateLastUpdatedStamp();
            try
            {
                await LoadDashboardDataAsync(quiet: true, forceRefresh: true);
            }
            finally
            {
                _refreshing = false;
                btnRefresh.Enabled = true;
            }
        }

        private void LoadSessionInfo()
        {
            string roleName;

            try
            {
                roleName =
                    _session.GetRoleName();
            }
            catch
            {
                roleName =
                    "Kullanıcı";
            }

            string companyName =
                "-";

            try
            {
                string? token =
                    _session.Token;

                if (!string.IsNullOrWhiteSpace(token))
                {
                    JwtSecurityToken jwt =
                        new JwtSecurityTokenHandler()
                            .ReadJwtToken(token);

                    companyName =
                        jwt.Claims
                            .FirstOrDefault(
                                c => c.Type == "company")
                            ?.Value
                        ?? "-";
                }
            }
            catch
            {
                companyName =
                    "-";
            }

            lblSub.Text =
                $"Rol: {roleName}   •   Kurum: {companyName}";

            lblDate.Text =
                DateTime.Now.ToString(
                    "dddd, dd MMMM yyyy",
                    CultureInfo.GetCultureInfo("tr-TR"));
        }

        private async Task LoadDashboardDataAsync(
            bool quiet = false,
            bool forceRefresh = false)
        {
            try
            {
                IDashboardDataProvider provider =
                    Program.Services.GetRequiredService<IDashboardDataProvider>();

                DashboardSnapshot snapshot =
                    await provider.GetOverviewAsync(forceRefresh);

                SetKpis(snapshot);

                if (BuildFingerprint(snapshot) != _lastFingerprint)
                {
                    _lastFingerprint = BuildFingerprint(snapshot);
                    RenderDashboard(snapshot);
                }

                _hasLoadedOnce =
                    true;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Dashboard.Load", ex);

                // İlk yükleme başarısızsa kartlar boş gösterilir;
                // arka plandaki otomatik yenileme başarısızsa mevcut değerler korunur.
                if (!quiet || !_hasLoadedOnce)
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        SetKpi(i, null);
                    }
                }
            }
            finally
            {
                // "Son güncelleme" damgası, veri adımlarından biri hata verse bile
                // yükleme girişiminde bulunulduğunu göstersin.
                UpdateLastUpdatedStamp();
            }
        }

        private void UpdateLastUpdatedStamp()
        {
            lblDate.Text =
                $"{DateTime.Now.ToString("dddd, dd MMMM yyyy", CultureInfo.GetCultureInfo("tr-TR"))}   •   Son güncelleme: {DateTime.Now:HH:mm:ss}";
        }

        private void SetKpi(
            int index,
            string? value)
        {
            if (_kpiValues.TryGetValue(
                    index,
                    out Label? label))
            {
                label.Text =
                    value
                    ?? "-";
            }
        }

        private void SetKpis(DashboardSnapshot snapshot)
        {
            SetKpi(1, snapshot.CustomerCount.ToString("N0"));
            SetKpi(2, snapshot.SupplierCount.ToString("N0"));
            SetKpi(3, snapshot.FinishedProductCount.ToString("N0"));
            SetKpi(4, snapshot.InStockCount.ToString("N0"));
            SetKpi(5, snapshot.CriticalStockCount.ToString("N0"));
            SetKpi(6, snapshot.TotalReceivables.ToString("N2"));
            SetKpi(7, snapshot.TotalPayables.ToString("N2"));
            SetKpi(8, snapshot.ApprovedInvoiceCount.ToString("N0"));
        }

        private void RenderDashboard(DashboardSnapshot snapshot)
        {
            SuspendLayout();

            try
            {
                ConfigureTableColumns();

                gridReceivables.BeginUpdate();
                gridPayables.BeginUpdate();
                gridCriticalStock.BeginUpdate();
                gridProductStocks.BeginUpdate();

                try
                {
                    gridReceivables.DataSource = snapshot.Receivables;
                    gridPayables.DataSource = snapshot.Payables;
                    gridCriticalStock.DataSource = snapshot.CriticalStocks;
                    gridProductStocks.DataSource = snapshot.ProductStocks;
                }
                finally
                {
                    gridReceivables.EndUpdate();
                    gridPayables.EndUpdate();
                    gridCriticalStock.EndUpdate();
                    gridProductStocks.EndUpdate();
                }

                LoadMoneyBar(
                    chartReceivables,
                    snapshot.Receivables
                        .Take(10)
                        .Select(r => new DashboardBalancePoint(r.AccountName, r.Balance))
                        .ToList(),
                    DashColors.Blue);

                LoadMoneyBar(
                    chartPayables,
                    snapshot.Payables
                        .Take(10)
                        .Select(r => new DashboardBalancePoint(r.AccountName, -r.Balance))
                        .ToList(),
                    DashColors.Red);

                LoadDoughnut(
                    chartCriticalStock,
                    snapshot.CriticalStocks
                        .GroupBy(c => c.CategoryName)
                        .Select(g => new DashboardChartPoint(g.Key, g.Count()))
                        .OrderByDescending(p => p.Count)
                        .ToList(),
                    DashColors.DoughnutPalette);

                LoadDoughnut(
                    chartInvoiceStatus,
                    snapshot.InvoiceStatus,
                    DashColors.DoughnutPalette);

                LoadTrend(
                    chartInvoiceTrend,
                    snapshot.InvoiceTrend);

                LoadStockBar(
                    chartStockMovements,
                    snapshot.StockMovements);
            }
            finally
            {
                ResumeLayout();
            }
        }

        private static string BuildFingerprint(DashboardSnapshot snapshot)
        {
            StringBuilder sb = new();

            sb.Append(snapshot.CustomerCount).Append('|')
              .Append(snapshot.SupplierCount).Append('|')
              .Append(snapshot.ApprovedInvoiceCount).Append('|')
              .Append(snapshot.DraftInvoiceCount).Append('|')
              .Append(snapshot.TotalReceivables.ToString("G29", CultureInfo.InvariantCulture)).Append('|')
              .Append(snapshot.TotalPayables.ToString("G29", CultureInfo.InvariantCulture)).Append('|')
              .Append(snapshot.CriticalStockCount).Append('|')
              .Append(snapshot.InStockCount).Append('|')
              .Append(snapshot.FinishedProductCount).Append('|')
              .Append(snapshot.SemiFinishedProductCount).Append('|');

            foreach (var row in snapshot.Receivables)
            {
                sb.Append(row.AccountName).Append(':')
                  .Append(row.TotalDebit.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.TotalCredit.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.Balance.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            sb.Append('|');

            foreach (var row in snapshot.Payables)
            {
                sb.Append(row.AccountName).Append(':')
                  .Append(row.TotalDebit.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.TotalCredit.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.Balance.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            sb.Append('|');

            foreach (var row in snapshot.CriticalStocks)
            {
                sb.Append(row.ProductCode).Append(':')
                  .Append(row.Stock.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.MinimumLevel?.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            sb.Append('|');

            foreach (var point in snapshot.InvoiceTrend)
            {
                sb.Append(point.Label).Append(':')
                  .Append(point.Amount.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            sb.Append('|');

            foreach (var point in snapshot.StockMovements)
            {
                sb.Append(point.Label).Append(':')
                  .Append(point.Input.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(point.Output.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            sb.Append('|');

            foreach (var row in snapshot.ProductStocks)
            {
                sb.Append(row.ProductCode).Append(':')
                  .Append(row.TotalInput.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.TotalOutput.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.BalanceQuantity.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.TotalInputCost.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.TotalOutputCost.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(row.BalanceCost.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            return sb.ToString();
        }

        private void ConfigureTableColumns()
        {
            if (_tableColumnsConfigured)
            {
                return;
            }

            _tableColumnsConfigured =
                true;

            AddColumn(viewReceivables, "Müşteri", nameof(DashboardBalanceRow.AccountName), 120);
            AddColumn(viewReceivables, "Toplam Borç", nameof(DashboardBalanceRow.TotalDebit), 70, "n2");
            AddColumn(viewReceivables, "Toplam Alacak", nameof(DashboardBalanceRow.TotalCredit), 70, "n2");
            AddColumn(viewReceivables, "Alacak Bakiyesi", nameof(DashboardBalanceRow.Balance), 85, "n2");

            AddColumn(viewPayables, "Tedarikçi", nameof(DashboardBalanceRow.AccountName), 120);
            AddColumn(viewPayables, "Toplam Borç", nameof(DashboardBalanceRow.TotalDebit), 70, "n2");
            AddColumn(viewPayables, "Toplam Alacak", nameof(DashboardBalanceRow.TotalCredit), 70, "n2");
            AddColumn(viewPayables, "Borç Bakiyesi", nameof(DashboardBalanceRow.Balance), 85, "n2");

            AddColumn(viewCriticalStock, "Ürün Kodu", nameof(DashboardCriticalStockRow.ProductCode), 75);
            AddColumn(viewCriticalStock, "Ürün Adı", nameof(DashboardCriticalStockRow.ProductName), 105);
            AddColumn(viewCriticalStock, "Kategori", nameof(DashboardCriticalStockRow.CategoryName), 60);
            AddColumn(viewCriticalStock, "Stok", nameof(DashboardCriticalStockRow.Stock), 50, "n0");
            AddColumn(viewCriticalStock, "Min. Seviye", nameof(DashboardCriticalStockRow.MinimumLevel), 60, "n0");

            AddColumn(viewProductStocks, "Ürün Kodu", nameof(DashboardProductStockRow.ProductCode), 75);
            AddColumn(viewProductStocks, "Ürün Adı", nameof(DashboardProductStockRow.ProductName), 120);
            AddColumn(viewProductStocks, "Kategori", nameof(DashboardProductStockRow.CategoryName), 60);
            AddColumn(viewProductStocks, "Toplam Giren", nameof(DashboardProductStockRow.TotalInput), 55, "n0");
            AddColumn(viewProductStocks, "Toplam Çıkan", nameof(DashboardProductStockRow.TotalOutput), 55, "n0");
            AddColumn(viewProductStocks, "Kalan Bakiye", nameof(DashboardProductStockRow.BalanceQuantity), 60, "n0");
            AddColumn(viewProductStocks, "Giren Maliyet", nameof(DashboardProductStockRow.TotalInputCost), 65, "n2");
            AddColumn(viewProductStocks, "Çıkan Maliyet", nameof(DashboardProductStockRow.TotalOutputCost), 65, "n2");
            AddColumn(viewProductStocks, "Bakiye Maliyet", nameof(DashboardProductStockRow.BalanceCost), 65, "n2");
        }

        private static void AddColumn(
            GridView view,
            string caption,
            string fieldName,
            int width,
            string? format = null)
        {
            GridColumn column =
                new()
                {
                    Caption = caption,
                    FieldName = fieldName,
                    Visible = true,
                    Width = width
                };

            if (format != null)
            {
                column.DisplayFormat.FormatType =
                    FormatType.Numeric;

                column.DisplayFormat.FormatString =
                    format;
            }

            view.Columns.Add(column);
        }

        private static void LoadMoneyBar(
            ChartControl chart,
            List<DashboardBalancePoint> data,
            Color color)
        {
            chart.BeginInit();

            try
            {
                chart.Series.Clear();

                chart.Legend.Visibility =
                    DefaultBoolean.False;

                if (data.Count == 0)
                {
                    return;
                }

                Series series =
                    new Series(
                        "Tutar",
                        ViewType.Bar)
                    {
                        DataSource = data,
                        ArgumentDataMember =
                            nameof(DashboardBalancePoint.Label)
                    };

                series.ValueDataMembers.AddRange(
                    nameof(DashboardBalancePoint.Amount));

                if (series.View is BarSeriesView barView)
                {
                    barView.Border.Visibility =
                        DefaultBoolean.False;

                    barView.Color =
                        color;
                }

                chart.Series.Add(series);

                if (chart.Diagram is XYDiagram diagram)
                {
                    diagram.Rotated =
                        true;

                    diagram.AxisX.Title.Visibility =
                        DefaultBoolean.False;

                    diagram.AxisY.Title.Visibility =
                        DefaultBoolean.False;

                    diagram.AxisX.Label.TextPattern =
                        "{A}";

                    diagram.AxisY.Label.TextPattern =
                        "{V:N0}";

                    diagram.AxisY.WholeRange.Auto =
                        true;

                    HideGridLines(diagram);
                }
            }
            finally
            {
                chart.EndInit();
            }
        }

        private static void LoadDoughnut(
            ChartControl chart,
            IReadOnlyList<DashboardChartPoint> data,
            Color[] palette)
        {
            chart.BeginInit();

            try
            {
                chart.Series.Clear();

                chart.Legend.Visibility =
                    DefaultBoolean.False;

                if (data.Count == 0)
                {
                    return;
                }

                Series series =
                    new Series(
                        "Dağılım",
                        ViewType.Doughnut);

                series.LabelsVisibility =
                    DefaultBoolean.True;

                series.Label.TextPattern =
                    "{A}: {V}";

                if (series.View is DoughnutSeriesView view)
                {
                    view.HoleRadiusPercent =
                        62;
                }

                for (int i = 0; i < data.Count; i++)
                {
                    int pointIndex =
                        series.Points.Add(
                            new SeriesPoint(
                                data[i].Label,
                                data[i].Count));

                    series.Points[pointIndex].Color =
                        palette[i % palette.Length];
                }

                chart.Series.Add(series);

                chart.Legend.Visibility =
                    DefaultBoolean.True;

                chart.Legend.AlignmentHorizontal =
                    LegendAlignmentHorizontal.Center;

                chart.Legend.AlignmentVertical =
                    LegendAlignmentVertical.Bottom;

                chart.Legend.EnableAntialiasing =
                    DefaultBoolean.True;
            }
            finally
            {
                chart.EndInit();
            }
        }

        private static void LoadTrend(
            ChartControl chart,
            IReadOnlyList<DashboardBalancePoint> data)
        {
            chart.BeginInit();

            try
            {
                chart.Series.Clear();

                chart.Legend.Visibility =
                    DefaultBoolean.False;

                if (data.Count == 0)
                {
                    return;
                }

                Series series =
                    new Series(
                        "Aylık Tutar",
                        ViewType.Area)
                    {
                        DataSource = data,
                        ArgumentDataMember =
                            nameof(DashboardBalancePoint.Label)
                    };

                series.ValueDataMembers.AddRange(
                    nameof(DashboardBalancePoint.Amount));

                series.LabelsVisibility =
                    DefaultBoolean.False;

                if (series.View is AreaSeriesView areaView)
                {
                    areaView.MarkerVisibility =
                        DefaultBoolean.False;

                    areaView.Border.Visibility =
                        DefaultBoolean.False;

                    areaView.Color =
                        DashColors.Blue;

                    areaView.Transparency =
                        200;

                    areaView.EnableAntialiasing =
                        DefaultBoolean.True;
                }

                chart.Series.Add(series);

                if (chart.Diagram is XYDiagram diagram)
                {
                    diagram.AxisX.Title.Visibility =
                        DefaultBoolean.False;

                    diagram.AxisY.Title.Visibility =
                        DefaultBoolean.False;

                    diagram.AxisX.Label.TextPattern =
                        "{A}";

                    diagram.AxisY.Label.TextPattern =
                        "{V:N0}";

                    diagram.AxisY.WholeRange.Auto =
                        true;

                    HideGridLines(diagram);
                }
            }
            finally
            {
                chart.EndInit();
            }
        }

        private static void LoadStockBar(
            ChartControl chart,
            IReadOnlyList<DashboardStockPoint> data)
        {
            chart.BeginInit();

            try
            {
                chart.Series.Clear();

                chart.Legend.Visibility =
                    DefaultBoolean.False;

                if (data.Count == 0)
                {
                    return;
                }

                Series inputSeries =
                    new Series(
                        "Giriş",
                        ViewType.Bar)
                    {
                        DataSource = data,
                        ArgumentDataMember =
                            nameof(DashboardStockPoint.Label)
                    };

                inputSeries.ValueDataMembers.AddRange(
                    nameof(DashboardStockPoint.Input));

                inputSeries.LabelsVisibility =
                    DefaultBoolean.False;

                Series outputSeries =
                    new Series(
                        "Çıkış",
                        ViewType.Bar)
                    {
                        DataSource = data,
                        ArgumentDataMember =
                            nameof(DashboardStockPoint.Label)
                    };

                outputSeries.ValueDataMembers.AddRange(
                    nameof(DashboardStockPoint.Output));

                outputSeries.LabelsVisibility =
                    DefaultBoolean.False;

                if (inputSeries.View is BarSeriesView inView)
                {
                    inView.Border.Visibility =
                        DefaultBoolean.False;

                    inView.Color =
                        DashColors.Green;
                }

                if (outputSeries.View is BarSeriesView outView)
                {
                    outView.Border.Visibility =
                        DefaultBoolean.False;

                    outView.Color =
                        DashColors.Red;
                }

                chart.Series.Add(inputSeries);
                chart.Series.Add(outputSeries);

                chart.Legend.Visibility =
                    DefaultBoolean.True;

                chart.Legend.AlignmentHorizontal =
                    LegendAlignmentHorizontal.Center;

                chart.Legend.AlignmentVertical =
                    LegendAlignmentVertical.Bottom;

                chart.Legend.EnableAntialiasing =
                    DefaultBoolean.True;

                if (chart.Diagram is XYDiagram diagram)
                {
                    diagram.AxisX.Title.Visibility =
                        DefaultBoolean.False;

                    diagram.AxisY.Title.Visibility =
                        DefaultBoolean.False;

                    diagram.AxisX.Label.TextPattern =
                        "{A}";

                    diagram.AxisY.Label.TextPattern =
                        "{V:N0}";

                    diagram.AxisY.WholeRange.Auto =
                        true;

                    HideGridLines(diagram);
                }
            }
            finally
            {
                chart.EndInit();
            }
        }

        private static void HideGridLines(XYDiagram diagram)
        {
            diagram.AxisX.GridLines.Visible =
                false;

            diagram.AxisY.GridLines.Visible =
                false;

            diagram.AxisX.Tickmarks.Visible =
                false;

            diagram.AxisY.Tickmarks.Visible =
                false;
        }

        private static class DashColors
        {
            public static readonly Color Blue = Color.FromArgb(46, 117, 182);
            public static readonly Color Teal = Color.FromArgb(38, 166, 154);
            public static readonly Color Green = Color.FromArgb(40, 167, 69);
            public static readonly Color Red = Color.FromArgb(220, 53, 69);
            public static readonly Color Orange = Color.FromArgb(253, 126, 20);
            public static readonly Color Purple = Color.FromArgb(111, 66, 193);
            public static readonly Color Pink = Color.FromArgb(232, 62, 140);
            public static readonly Color Cyan = Color.FromArgb(23, 162, 184);
            public static readonly Color Gray = Color.FromArgb(134, 142, 150);

            public static readonly Color[] DoughnutPalette =
                [Blue, Teal, Green, Orange, Purple, Red, Pink, Cyan, Gray];
        }
    }
}