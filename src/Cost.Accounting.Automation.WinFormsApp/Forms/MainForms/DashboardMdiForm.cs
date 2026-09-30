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
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class DashboardMdiForm : XtraFormMdiBase
    {
        private const int DashboardAutoRefreshIntervalMs = 30_000;

        private readonly SessionClaimContext _session;
        private readonly IAccountingDbSelector _dbSelector;
        private readonly ISender _sender;
        private readonly Dictionary<int, Label> _kpiValues = new();
        private readonly System.Windows.Forms.Timer _refreshTimer;

        private CancellationTokenSource? _loadCts;
        private bool _refreshing;
        private bool _hasLoadedOnce;
        private bool _tableColumnsConfigured;
        private string _lastFingerprint = string.Empty;

        public DashboardMdiForm() : base("Dashboard")
        {
            _session =
                Program.Services
                    .GetRequiredService<SessionClaimContext>();

            _dbSelector =
                Program.Services
                    .GetRequiredService<IAccountingDbSelector>();

            _sender =
                Program.Services
                    .GetRequiredService<ISender>();

            Size =
                new Size(1280, 720);

            MinimumSize =
                new Size(1024, 640);

            IconOptions.SvgImage =
                DxIcon.Home;

            InitializeComponent();

            btnRefresh.Click += BtnRefresh_Click;

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

            // Bekleme formu veri yüklenirken açılır; tüm veri uygulandıktan
            // sonra "tüm veriler yüklendi" onayı gösterip kapanır.
            _ = LoadDashboardDataAsync(showLoading: true);

            _refreshTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _refreshTimer.Stop();
            _refreshTimer.Tick -= RefreshDashboardTimer_Tick;
            _refreshTimer.Dispose();

            // Form kapanırken devam eden yükleme isteğini iptal ediyoruz.
            _loadCts?.Cancel();
            _loadCts?.Dispose();

            // Yarım kalmış yüklemenin bekleme penceresini de kapat.
            LoadingHelper.CloseCurrent();

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
                await LoadDashboardDataAsync(quiet: true, forceRefresh: false);            }
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
                await LoadDashboardDataAsync(
                quiet: true,
                forceRefresh: true,
                showLoading: true);
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
            string? userFullName = "-";

            try
            {
                roleName =
                    _session.GetRoleName();
                userFullName =
                    _session.GetUserFullName();
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

            int? activeYear = _dbSelector.Year?.Value;

            lblSub.Text =
                $"Kurum: {companyName}  •  Kullanıcı: {userFullName}  •  Rol: {roleName}  •  Aktif Yıl: {activeYear?.ToString() ?? "-"}";

            lblDate.Text =
                DateTime.Now.ToString(
                    "dddd, dd MMMM yyyy",
                    CultureInfo.GetCultureInfo("tr-TR"));
        }

        private async Task LoadDashboardDataAsync(
            bool quiet = false,
            bool forceRefresh = false,
            bool showLoading = false)
        {
            // Önceki yükleme hâlâ sürüyorsa onu iptal edip yenisini başlatıyoruz.
            CancellationTokenSource? previousCts =
                Interlocked.Exchange(
                    ref _loadCts,
                    new CancellationTokenSource());

            previousCts?.Cancel();
            previousCts?.Dispose();

            CancellationTokenSource cts =
                _loadCts!;

            CancellationToken token =
                cts.Token;

            string loadedSummary = string.Empty;

            // Bekleme penceresi yalnızca kullanıcıya görünür yüklemelerde açılır;
            // 30 saniyelik arka plan yenilemesi sessizdir. Tüm KPI, tablo ve
            // grafikler uygulandıktan SONRA "tüm veriler yüklendi" onayı
            // gösterilir.
            Func<Task> loadTask = async () =>
            {
                try
                {
                    TS.Result.Result<DashboardSnapshot> result =
                        await _sender.Send(
                            new DashboardGetOverviewQuery(forceRefresh),
                            token);

                    if (result.IsSuccessful && result.Data is DashboardSnapshot snapshot)
                    {
                        // İptal edildiyse veya form kapanmışsa sonucu uygulama.
                        if (token.IsCancellationRequested
                            || IsDisposed
                            || Disposing)
                        {
                            return;
                        }

                        SetKpis(snapshot);

                        string fingerprint = BuildFingerprint(snapshot);

                        // Veri değişmediyse grafikler yeniden çizilmez; bu, 30 saniyelik
                        // otomatik yenilemede gereksiz yeniden boyamayı engeller.
                        if (fingerprint != _lastFingerprint)
                        {
                            _lastFingerprint = fingerprint;
                            RenderDashboard(snapshot);
                        }

                        _hasLoadedOnce = true;
                        loadedSummary = BuildLoadedSummary(snapshot);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Beklenen: yeni bir yenileme bu isteği iptal etti.
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

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
            };

            if (showLoading)
            {
                await LoadingHelper.RunAsync(
                    loadTask,
                    caption: "Veriler yükleniyor...",
                    description: "Lütfen bekleyin...",
                    showCompleted: true,
                    completionSummary: () => loadedSummary);
            }
            else
            {
                await loadTask();
            }

            if (ReferenceEquals(
                    Interlocked.CompareExchange(
                        ref _loadCts,
                        null,
                        cts),
                    cts))
            {
                cts.Dispose();
            }

            // "Son güncelleme" damgası, veri adımlarından biri hata verse bile
            // yükleme girişiminde bulunulduğunu göstersin.
            UpdateLastUpdatedStamp();
        }


        /// <summary>
        /// Bekleme katmanında gösterilecek özet satır. Tüm seriler dolduğunda
        /// grafik sayıları burada görünür olur.
        /// </summary>
        private static string BuildLoadedSummary(DashboardSnapshot snapshot)
        {
            int charts =
                snapshot.InvoiceStatus.Count
                + snapshot.InvoiceTypes.Count
                + snapshot.InvoiceTrend.Count
                + snapshot.MonthlyBalances.Count
                + snapshot.StockMovements.Count
                + snapshot.CategoryStocks.Count
                + snapshot.TopMovementProducts.Count;

            int tables =
                snapshot.Receivables.Count
                + snapshot.Payables.Count
                + snapshot.CriticalStocks.Count
                + snapshot.ProductStocks.Count;

            return $"{charts} grafik noktası, {tables} tablo satırı";
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
                    DashboardChartPalette.Success);

                LoadMoneyBar(
                    chartPayables,
                    snapshot.Payables
                        .Take(10)
                        .Select(r => new DashboardBalancePoint(r.AccountName, -r.Balance))
                        .ToList(),
                    DashboardChartPalette.Danger);

                LoadDoughnut(
                    chartCriticalStock,
                    snapshot.CriticalStocks
                        .GroupBy(c => c.CategoryName)
                        .Select(g => new DashboardChartPoint(g.Key, g.Count()))
                        .OrderByDescending(p => p.Count)
                        .ToList());

                LoadDoughnut(
                    chartInvoiceStatus,
                    snapshot.InvoiceStatus);

                // Yeni: fatura türü dağılımı
                LoadDoughnut(
                    chartInvoiceTypes,
                    snapshot.InvoiceTypes);

                LoadTrend(
                    chartInvoiceTrend,
                    snapshot.InvoiceTrend);

                LoadStockBar(
                    chartStockMovements,
                    snapshot.StockMovements);

                // Yeni: aylık alacak / borç seyri
                DashboardChartLoader.LoadDualArea(
                    chartMonthlyBalances,
                    snapshot.MonthlyBalances,
                    "Alacak",
                    "Borç",
                    DashboardChartPalette.Secondary,
                    DashboardChartPalette.Danger);

                // Yeni: kategori bazında stok değeri
                DashboardChartLoader.LoadHorizontalBar(
                    chartCategoryStocks,
                    snapshot.CategoryStocks,
                    DashboardChartPalette.Accent);

                // Yeni: en hareketli ürünler
                DashboardChartLoader.LoadRankedBar(
                    chartTopMovements,
                    snapshot.TopMovementProducts);
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
                  .Append(point.First.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(point.Second.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
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

            sb.Append('|');

            foreach (var point in snapshot.InvoiceTypes)
            {
                sb.Append(point.Label).Append(':')
                  .Append(point.Count.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            sb.Append('|');

            foreach (var point in snapshot.MonthlyBalances)
            {
                sb.Append(point.Label).Append(':')
                  .Append(point.First.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(point.Second.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            sb.Append('|');

            foreach (var point in snapshot.CategoryStocks)
            {
                sb.Append(point.Label).Append(':')
                  .Append(point.Amount.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
            }

            sb.Append('|');

            foreach (var point in snapshot.TopMovementProducts)
            {
                sb.Append(point.Label).Append(':')
                  .Append(point.Quantity.ToString("G29", CultureInfo.InvariantCulture)).Append(':')
                  .Append(point.Amount.ToString("G29", CultureInfo.InvariantCulture)).Append('#');
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
            => DashboardChartLoader.LoadHorizontalBar(
                chart,
                data,
                color);

        private static void LoadDoughnut(
            ChartControl chart,
            IReadOnlyList<DashboardChartPoint> data)
            => DashboardChartLoader.LoadDoughnut(
                chart,
                data);

        private static void LoadTrend(
            ChartControl chart,
            IReadOnlyList<DashboardBalancePoint> data)
            => DashboardChartLoader.LoadArea(
                chart,
                data,
                DashboardChartPalette.Primary);

        private static void LoadStockBar(
            ChartControl chart,
            IReadOnlyList<DashboardDualPoint> data)
            => DashboardChartLoader.LoadGroupedBar(
                chart,
                data,
                "Giriş",
                "Çıkış",
                DashboardChartPalette.Success,
                DashboardChartPalette.Danger);
    }
}
