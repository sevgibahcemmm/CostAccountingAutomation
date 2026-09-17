using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Infrastructure.Context;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
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
using Cost.Accounting.Automation.WinFormsApp.Utils;

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
                await LoadDashboardDataAsync(quiet: true);
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
                await LoadDashboardDataAsync(quiet: true);
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

        private async Task LoadDashboardDataAsync(bool quiet = false)
        {
            try
            {
                Task<(int Customers, int Suppliers)> countsTask = LoadCountsAsync();

                Task<(List<BalanceRow> Receivables, List<BalanceRow> Payables)> balancesTask =
                    LoadBalanceRowsAsync();

                Task<Dictionary<IdentityId, decimal>> stockTotalsTask = LoadStockTotalsAsync();

                Task<List<BalancePoint>> invoiceTrendTask = LoadInvoiceTrendAsync();

                Task<List<StockPoint>> stockMovementsTask = LoadStockMovementsAsync();

                await Task.WhenAll(
                    countsTask,
                    balancesTask,
                    stockTotalsTask,
                    invoiceTrendTask,
                    stockMovementsTask);

                Dictionary<IdentityId, decimal> stockTotals =
                    await stockTotalsTask;

                Task<(int Approved, int Draft)> invoiceStatusTask =
                    LoadInvoiceStatusAsync();

                Task<List<CriticalStockRow>> criticalStockTask =
                    LoadCriticalStockRowsAsync(stockTotals);

                await Task.WhenAll(invoiceStatusTask, criticalStockTask);

                var counts =
                    await countsTask;

                var (receivables, payables) =
                    await balancesTask;

                var (approvedInvoiceCount, draftInvoiceCount) =
                    await invoiceStatusTask;

                List<CriticalStockRow> criticalStock =
                    await criticalStockTask;

                List<BalancePoint> invoiceTrend =
                    await invoiceTrendTask;

                List<StockPoint> stockMovements =
                    await stockMovementsTask;

                decimal totalReceivables =
                    receivables.Sum(r => r.Balance);

                decimal totalPayables =
                    payables.Sum(r => -r.Balance);

                int inStockCount =
                    stockTotals.Values.Count(v => v > 0);

                List<ChartPoint> invoiceStatus =
                    BuildInvoiceStatus(
                        approvedInvoiceCount,
                        draftInvoiceCount);

                SetKpi(1, counts.Customers.ToString("N0"));
                SetKpi(2, counts.Suppliers.ToString("N0"));
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

        private static async Task<(int Customers, int Suppliers)> LoadCountsAsync()
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            int customers = await db.Set<Customer>().AsNoTracking().CountAsync();
            int suppliers = await db.Set<Supplier>().AsNoTracking().CountAsync();

            return (customers, suppliers);
        }

        private static async Task<(int Approved, int Draft)> LoadInvoiceStatusAsync()
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            int approved = await db.Set<Invoice>().AsNoTracking().CountAsync(i => i.Status == InvoiceStatus.Approved);
            int draft = await db.Set<Invoice>().AsNoTracking().CountAsync(i => i.Status == InvoiceStatus.Draft);

            return (approved, draft);
        }

        private static async Task<Dictionary<IdentityId, decimal>> LoadStockTotalsAsync()
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var rows = await db.Set<ProductMovement>()
                .AsNoTracking()
                .GroupBy(m => m.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Stock = g.Sum(m => m.MovementType == ProductMovementType.Input ? m.Quantity : -m.Quantity)
                })
                .ToListAsync();

            return rows.ToDictionary(r => r.ProductId, r => r.Stock);
        }

        private static async Task<(List<BalanceRow> Receivables, List<BalanceRow> Payables)>
            LoadBalanceRowsAsync()
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Dictionary<IdentityId, string> customerNames =
                await db.Set<Customer>()
                    .AsNoTracking()
                    .ToDictionaryAsync(
                        c => c.Id,
                        c => c.Name.Value);

            Dictionary<IdentityId, string> supplierNames =
                await db.Set<Supplier>()
                    .AsNoTracking()
                    .ToDictionaryAsync(
                        s => s.Id,
                        s => s.Name.Value);

            var receivableTotals =
                (await db.Set<CurrentAccountMovement>()
                    .AsNoTracking()
                    .Where(
                        m => m.CurrentAccountType == CurrentAccountType.Customer
                            && m.CustomerId != null)
                    .GroupBy(m => m.CustomerId)
                    .Select(
                        g => new
                        {
                            CustomerId = g.Key,
                            Debit = g.Sum(m => m.Debit),
                            Credit = g.Sum(m => m.Credit)
                        })
                    .ToListAsync())
                .Select(
                    t => new
                    {
                        t.CustomerId,
                        t.Debit,
                        t.Credit
                    })
                .ToList();

            var payableTotals =
                (await db.Set<CurrentAccountMovement>()
                    .AsNoTracking()
                    .Where(
                        m => m.CurrentAccountType == CurrentAccountType.Supplier
                            && m.SupplierId != null)
                    .GroupBy(m => m.SupplierId)
                    .Select(
                        g => new
                        {
                            SupplierId = g.Key,
                            Debit = g.Sum(m => m.Debit),
                            Credit = g.Sum(m => m.Credit)
                        })
                    .ToListAsync())
                .Select(
                    t => new
                    {
                        t.SupplierId,
                        t.Debit,
                        t.Credit
                    })
                .ToList();

            List<BalanceRow> receivables =
                receivableTotals
                    .Select(
                        b =>
                        {
                            string name =
                                b.CustomerId != null
                                && customerNames.TryGetValue(b.CustomerId, out string? n)
                                    ? n ?? "-"
                                    : "-";

                            return new BalanceRow(
                                "Müşteri",
                                name,
                                b.Debit,
                                b.Credit,
                                b.Debit - b.Credit);
                        })
                    .Where(r => r.Balance > 0)
                    .OrderByDescending(r => r.Balance)
                    .ToList();

            List<BalanceRow> payables =
                payableTotals
                    .Select(
                        b =>
                        {
                            string name =
                                b.SupplierId != null
                                && supplierNames.TryGetValue(b.SupplierId, out string? n)
                                    ? n ?? "-"
                                    : "-";

                            return new BalanceRow(
                                "Tedarikçi",
                                name,
                                b.Debit,
                                b.Credit,
                                b.Debit - b.Credit);
                        })
                    .Where(r => r.Balance < 0)
                    .OrderBy(r => r.Balance)
                    .ToList();

            return (receivables, payables);
        }

        private static async Task<List<CriticalStockRow>>
            LoadCriticalStockRowsAsync(
                Dictionary<IdentityId, decimal> stockTotals)
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var products =
                await db.Set<Product>()
                    .AsNoTracking()
                    .Select(
                        p => new
                        {
                            Id = p.Id,
                            ProductCode = p.ProductCode.Value,
                            ProductName = p.Name.Value,
                            CategoryName = p.Category!.Name.Value,
                            MinimumLevel = p.MinimumProductLevel
                        })
                    .ToListAsync();

            return products
                .Select(
                    p => new CriticalStockRow(
                        p.ProductCode,
                        p.ProductName,
                        p.CategoryName,
                        stockTotals.GetValueOrDefault(p.Id),
                        p.MinimumLevel))
                .Where(
                    r => r.Stock <= 0
                        || (r.MinimumLevel != null && r.Stock <= r.MinimumLevel))
                .OrderBy(r => r.Stock)
                .Take(50)
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

        private static async Task<List<BalancePoint>> LoadInvoiceTrendAsync()
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            DateOnly firstMonth =
                new DateOnly(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1).AddMonths(-5);

            var grouped =
                await db.Set<Invoice>()
                    .AsNoTracking()
                    .Where(i => i.Status == InvoiceStatus.Approved && i.Date >= firstMonth)
                    .GroupBy(i => new { i.Date.Year, i.Date.Month })
                    .Select(
                        g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Total = g.Sum(x => x.GrandTotal)
                        })
                    .ToListAsync();

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

        private static async Task<List<StockPoint>> LoadStockMovementsAsync()
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            DateOnly firstMonth =
                new DateOnly(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1).AddMonths(-5);

            var grouped =
                await db.Set<ProductMovement>()
                    .AsNoTracking()
                    .Where(m => m.Date >= firstMonth)
                    .GroupBy(m => new { m.Date.Year, m.Date.Month, m.MovementType })
                    .Select(
                        g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Type = g.Key.MovementType,
                            Quantity = g.Sum(x => x.Quantity)
                        })
                    .ToListAsync();

            CultureInfo culture =
                CultureInfo.GetCultureInfo("tr-TR");

            List<StockPoint> result = new();

            for (int i = 0; i < 6; i++)
            {
                DateOnly month =
                    firstMonth.AddMonths(i);

                decimal input =
                    grouped
                        .Where(
                            g => g.Year == month.Year
                                && g.Month == month.Month
                                && g.Type == ProductMovementType.Input)
                        .Sum(g => g.Quantity);

                decimal output =
                    grouped
                        .Where(
                            g => g.Year == month.Year
                                && g.Month == month.Month
                                && g.Type == ProductMovementType.Output)
                        .Sum(g => g.Quantity);

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