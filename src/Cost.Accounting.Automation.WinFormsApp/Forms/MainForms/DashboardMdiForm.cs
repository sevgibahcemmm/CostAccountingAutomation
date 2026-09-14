using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Infrastructure.Context;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class DashboardMdiForm : XtraFormMdiBase
    {
        private sealed record ChartPoint(string Label, int Count);

        private sealed record BalanceRow(string AccountTypeName, string AccountName, decimal TotalDebit, decimal TotalCredit, decimal Balance);

        private sealed record CriticalStockRow(string ProductCode, string ProductName, string CategoryName, decimal Stock, decimal? MinimumLevel);

        private sealed record BalancePoint(string Label, decimal Amount);

        private sealed record StockPoint(string Label, decimal Input, decimal Output);

        private const int DashboardAutoRefreshIntervalMs = 30_000;

        private readonly SessionClaimContext _session;
        private readonly Dictionary<int, Label> _kpiValues = new();
        private readonly System.Windows.Forms.Timer _refreshTimer;
        private bool _refreshing;
        private bool _hasLoadedOnce;
        private bool _tableColumnsConfigured;

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
                SvgIcons.Modules[0];

            InitializeComponent();

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
                await LoadDashboardDataAsync(quiet: true);
            }
            finally
            {
                _refreshing = false;
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

        private async Task LoadDashboardDataAsync(bool quiet = false)
        {
            try
            {
                using IServiceScope scope =
                    Program.Services.CreateScope();

                ApplicationDbContext db =
                    scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                int customerCount =
                    await db.Set<Customer>()
                        .CountAsync();

                int supplierCount =
                    await db.Set<Supplier>()
                        .CountAsync();

                int approvedInvoiceCount =
                    await db.Set<Invoice>()
                        .CountAsync(
                            i => i.Status == InvoiceStatus.Approved);

                int draftInvoiceCount =
                    await db.Set<Invoice>()
                        .CountAsync(
                            i => i.Status == InvoiceStatus.Draft);

                (List<BalanceRow> receivables, List<BalanceRow> payables) =
                    await LoadBalanceRowsAsync(db);

                decimal totalReceivables =
                    receivables.Sum(r => r.Balance);

                decimal totalPayables =
                    payables.Sum(r => -r.Balance);

                List<CriticalStockRow> criticalStock =
                    await LoadCriticalStockRowsAsync(db);

                int inStockCount =
                    await db.Set<Product>()
                        .Select(
                            p => p.Movements.Sum(
                                m => m.MovementType == ProductMovementType.Input
                                    ? m.Quantity
                                    : -m.Quantity))
                        .CountAsync(q => q > 0);

                List<ChartPoint> invoiceStatus =
                    BuildInvoiceStatus(
                        approvedInvoiceCount,
                        draftInvoiceCount);

                List<BalancePoint> invoiceTrend =
                    await LoadInvoiceTrendAsync(db);

                List<StockPoint> stockMovements =
                    await LoadStockMovementsAsync(db);

                SetKpi(1, customerCount.ToString("N0"));
                SetKpi(2, supplierCount.ToString("N0"));
                SetKpi(3, totalReceivables.ToString("N2"));
                SetKpi(4, totalPayables.ToString("N2"));
                SetKpi(5, criticalStock.Count.ToString("N0"));
                SetKpi(6, inStockCount.ToString("N0"));
                SetKpi(7, approvedInvoiceCount.ToString("N0"));
                SetKpi(8, draftInvoiceCount.ToString("N0"));

                ConfigureTableColumns();

                gridReceivables.DataSource = receivables;
                gridPayables.DataSource = payables;
                gridCriticalStock.DataSource = criticalStock;

                LoadMoneyBar(
                    chartReceivables,
                    receivables
                        .Take(10)
                        .Select(r => new BalancePoint(r.AccountName, r.Balance))
                        .ToList());

                LoadMoneyBar(
                    chartPayables,
                    payables
                        .Take(10)
                        .Select(r => new BalancePoint(r.AccountName, -r.Balance))
                        .ToList());

                LoadDoughnut(
                    chartCriticalStock,
                    criticalStock
                        .GroupBy(c => c.CategoryName)
                        .Select(g => new ChartPoint(g.Key, g.Count()))
                        .OrderByDescending(p => p.Count)
                        .ToList());

                LoadDoughnut(
                    chartInvoiceStatus,
                    invoiceStatus);

                LoadTrend(
                    chartInvoiceTrend,
                    invoiceTrend);

                LoadStockBar(
                    chartStockMovements,
                    stockMovements);

                _hasLoadedOnce =
                    true;

                UpdateLastUpdatedStamp();
            }
            catch
            {
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

        private void ConfigureTableColumns()
        {
            if (_tableColumnsConfigured)
            {
                return;
            }

            _tableColumnsConfigured =
                true;

            AddColumn(viewReceivables, "Müşteri", nameof(BalanceRow.AccountName), 120);
            AddColumn(viewReceivables, "Toplam Borç", nameof(BalanceRow.TotalDebit), 70, "n2");
            AddColumn(viewReceivables, "Toplam Alacak", nameof(BalanceRow.TotalCredit), 70, "n2");
            AddColumn(viewReceivables, "Alacak Bakiyesi", nameof(BalanceRow.Balance), 85, "n2");

            AddColumn(viewPayables, "Tedarikçi", nameof(BalanceRow.AccountName), 120);
            AddColumn(viewPayables, "Toplam Borç", nameof(BalanceRow.TotalDebit), 70, "n2");
            AddColumn(viewPayables, "Toplam Alacak", nameof(BalanceRow.TotalCredit), 70, "n2");
            AddColumn(viewPayables, "Borç Bakiyesi", nameof(BalanceRow.Balance), 85, "n2");

            AddColumn(viewCriticalStock, "Ürün Kodu", nameof(CriticalStockRow.ProductCode), 75);
            AddColumn(viewCriticalStock, "Ürün Adı", nameof(CriticalStockRow.ProductName), 105);
            AddColumn(viewCriticalStock, "Kategori", nameof(CriticalStockRow.CategoryName), 60);
            AddColumn(viewCriticalStock, "Stok", nameof(CriticalStockRow.Stock), 50, "n0");
            AddColumn(viewCriticalStock, "Min. Seviye", nameof(CriticalStockRow.MinimumLevel), 60, "n0");
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

        private static async Task<(List<BalanceRow> Receivables, List<BalanceRow> Payables)>
            LoadBalanceRowsAsync(
                ApplicationDbContext db)
        {
            List<CurrentAccountMovement> movements =
                await db.Set<CurrentAccountMovement>()
                    .Where(
                        m => m.CustomerId != null
                            || m.SupplierId != null)
                    .ToListAsync();

            Dictionary<Guid, string> customerNames =
                await db.Set<Customer>()
                    .ToDictionaryAsync(
                        c => c.Id.Value,
                        c => c.Name.Value);

            Dictionary<Guid, string> supplierNames =
                await db.Set<Supplier>()
                    .ToDictionaryAsync(
                        s => s.Id.Value,
                        s => s.Name.Value);

            List<BalanceRow> receivables =
                movements
                    .Where(
                        m => m.CurrentAccountType == CurrentAccountType.Customer
                            && m.CustomerId != null)
                    .GroupBy(m => m.CustomerId!.Value)
                    .Select(g =>
                    {
                        decimal debit = g.Sum(m => m.Debit);
                        decimal credit = g.Sum(m => m.Credit);

                        return new BalanceRow(
                            "Müşteri",
                            customerNames.TryGetValue(g.Key, out string? name)
                                ? name
                                : "-",
                            debit,
                            credit,
                            debit - credit);
                    })
                    .Where(r => r.Balance > 0)
                    .OrderByDescending(r => r.Balance)
                    .ToList();

            List<BalanceRow> payables =
                movements
                    .Where(
                        m => m.CurrentAccountType == CurrentAccountType.Supplier
                            && m.SupplierId != null)
                    .GroupBy(m => m.SupplierId!.Value)
                    .Select(g =>
                    {
                        decimal debit = g.Sum(m => m.Debit);
                        decimal credit = g.Sum(m => m.Credit);

                        return new BalanceRow(
                            "Tedarikçi",
                            supplierNames.TryGetValue(g.Key, out string? name)
                                ? name
                                : "-",
                            debit,
                            credit,
                            debit - credit);
                    })
                    .Where(r => r.Balance < 0)
                    .OrderBy(r => r.Balance)
                    .ToList();

            return (receivables, payables);
        }

        private static async Task<List<CriticalStockRow>>
            LoadCriticalStockRowsAsync(
                ApplicationDbContext db)
        {
            var rows =
                await db.Set<Product>()
                    .Select(
                        p => new
                        {
                            ProductCode = p.ProductCode.Value,
                            ProductName = p.Name.Value,
                            CategoryName = p.Category!.Name.Value,
                            MinimumLevel = p.MinimumProductLevel,
                            Stock = p.Movements.Sum(
                                m => m.MovementType == ProductMovementType.Input
                                    ? m.Quantity
                                    : -m.Quantity)
                        })
                    .ToListAsync();

            return rows
                .Where(
                    r => r.Stock <= 0
                        || (r.MinimumLevel != null && r.Stock <= r.MinimumLevel))
                .OrderBy(r => r.Stock)
                .Take(50)
                .Select(
                    r => new CriticalStockRow(
                        r.ProductCode,
                        r.ProductName,
                        r.CategoryName,
                        r.Stock,
                        r.MinimumLevel))
                .ToList();
        }

        private static List<ChartPoint> BuildInvoiceStatus(
            int approvedCount,
            int draftCount)
        {
            List<ChartPoint> points = new();

            if (draftCount > 0)
            {
                points.Add(new ChartPoint("Taslak", draftCount));
            }

            if (approvedCount > 0)
            {
                points.Add(new ChartPoint("Onaylı", approvedCount));
            }

            return points;
        }

        private static async Task<List<BalancePoint>> LoadInvoiceTrendAsync(
            ApplicationDbContext db)
        {
            var raw =
                await db.Set<Invoice>()
                    .Where(i => i.Status == InvoiceStatus.Approved)
                    .Select(i => new { i.Date, i.GrandTotal })
                    .ToListAsync();

            DateOnly firstMonth =
                new DateOnly(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1).AddMonths(-5);

            var grouped =
                raw
                    .Where(x => x.Date >= firstMonth)
                    .GroupBy(x => new { x.Date.Year, x.Date.Month })
                    .Select(
                        g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Total = g.Sum(x => x.GrandTotal)
                        })
                    .ToList();

            CultureInfo culture =
                CultureInfo.GetCultureInfo("tr-TR");

            List<BalancePoint> result = new();

            for (int i = 0; i < 6; i++)
            {
                DateOnly month =
                    firstMonth.AddMonths(i);

                var point =
                    grouped.FirstOrDefault(
                        g => g.Year == month.Year
                            && g.Month == month.Month);

                result.Add(
                    new BalancePoint(
                        month.ToString("MMMM", culture),
                        point?.Total ?? 0));
            }

            return result;
        }

        private static async Task<List<StockPoint>> LoadStockMovementsAsync(
            ApplicationDbContext db)
        {
            var raw =
                await db.Set<ProductMovement>()
                    .Select(
                        m => new
                        {
                            m.Date,
                            m.MovementType,
                            m.Quantity
                        })
                    .ToListAsync();

            DateOnly firstMonth =
                new DateOnly(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1).AddMonths(-5);

            CultureInfo culture =
                CultureInfo.GetCultureInfo("tr-TR");

            List<StockPoint> result = new();

            for (int i = 0; i < 6; i++)
            {
                DateOnly month =
                    firstMonth.AddMonths(i);

                decimal input =
                    raw
                        .Where(
                            m => m.Date.Year == month.Year
                                && m.Date.Month == month.Month
                                && m.MovementType == ProductMovementType.Input)
                        .Sum(m => m.Quantity);

                decimal output =
                    raw
                        .Where(
                            m => m.Date.Year == month.Year
                                && m.Date.Month == month.Month
                                && m.MovementType == ProductMovementType.Output)
                        .Sum(m => m.Quantity);

                result.Add(
                    new StockPoint(
                        month.ToString("MMMM", culture),
                        input,
                        output));
            }

            return result;
        }

        private static void LoadMoneyBar(
            ChartControl chart,
            List<BalancePoint> data)
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                chart.Legend.Visibility =
                    DefaultBoolean.False;

                return;
            }

            Series series =
                new Series(
                    "Tutar",
                    ViewType.Bar)
                {
                    DataSource = data,
                    ArgumentDataMember =
                        nameof(BalancePoint.Label)
                };

            series.ValueDataMembers.AddRange(
                nameof(BalancePoint.Amount));

            if (series.View is BarSeriesView barView)
            {
                barView.Border.Visibility = DefaultBoolean.False;
            }

            chart.Series.Add(series);

            chart.Legend.Visibility =
                DefaultBoolean.False;

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
            }
        }

        private static void LoadDoughnut(
            ChartControl chart,
            List<ChartPoint> data)
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                chart.Legend.Visibility =
                    DefaultBoolean.False;

                return;
            }

            Series series =
                new Series(
                    "Dağılım",
                    ViewType.Doughnut)
                {
                    DataSource = data,
                    ArgumentDataMember =
                        nameof(ChartPoint.Label)
                };

            series.ValueDataMembers.AddRange(
                nameof(ChartPoint.Count));

            series.LabelsVisibility =
                DefaultBoolean.False;

            if (series.View is DoughnutSeriesView view)
            {
                view.HoleRadiusPercent =
                    65;
            }

            chart.Series.Add(series);

            chart.Legend.Visibility =
                DefaultBoolean.True;

            chart.Legend.AlignmentHorizontal =
                LegendAlignmentHorizontal.Center;

            chart.Legend.AlignmentVertical =
                LegendAlignmentVertical.Bottom;

            if (chart.Diagram is SimpleDiagram diagram)
            {
                chart.Legend.EnableAntialiasing = DefaultBoolean.True;
            }
        }

        private static void LoadTrend(
            ChartControl chart,
            List<BalancePoint> data)
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                chart.Legend.Visibility =
                    DefaultBoolean.False;

                return;
            }

            Series series =
                new Series(
                    "Aylık Tutar",
                    ViewType.Area)
                {
                    DataSource = data,
                    ArgumentDataMember =
                        nameof(BalancePoint.Label)
                };

            series.ValueDataMembers.AddRange(
                nameof(BalancePoint.Amount));

            series.LabelsVisibility =
                DefaultBoolean.False;

            if (series.View is AreaSeriesView areaView)
            {
                areaView.MarkerVisibility = DefaultBoolean.False;
                areaView.Border.Visibility = DefaultBoolean.False;
            }

            chart.Series.Add(series);

            chart.Legend.Visibility =
                DefaultBoolean.False;

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
            }
        }

        private static void LoadStockBar(
            ChartControl chart,
            List<StockPoint> data)
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                chart.Legend.Visibility =
                    DefaultBoolean.False;

                return;
            }

            Series inputSeries =
                new Series(
                    "Giriş",
                    ViewType.Bar)
                {
                    DataSource = data,
                    ArgumentDataMember =
                        nameof(StockPoint.Label)
                };

            inputSeries.ValueDataMembers.AddRange(
                nameof(StockPoint.Input));

            inputSeries.LabelsVisibility =
                DefaultBoolean.False;

            Series outputSeries =
                new Series(
                    "Çıkış",
                    ViewType.Bar)
                {
                    DataSource = data,
                    ArgumentDataMember =
                        nameof(StockPoint.Label)
                };

            outputSeries.ValueDataMembers.AddRange(
                nameof(StockPoint.Output));

            outputSeries.LabelsVisibility =
                DefaultBoolean.False;

            if (inputSeries.View is BarSeriesView inView)
            {
                inView.Border.Visibility = DefaultBoolean.False;
            }

            if (outputSeries.View is BarSeriesView outView)
            {
                outView.Border.Visibility = DefaultBoolean.False;
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
            }
        }
    }
}