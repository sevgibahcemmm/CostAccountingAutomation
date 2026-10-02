using Cost.Accounting.Automation.Application.Behaviors;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Dashboards;

/// <summary>
/// Dashboard ana ekranının tek okuma sorgusu. Tüm KPI'lar, tablolar ve grafik
/// serileri bu sorgudan döner; UI tarafı ayrı sorgu çalıştırmaz.
/// </summary>
/// <param name="ForceRefresh">Cache'i atlayıp veritabanından yeniden okur.</param>
[Permission("dashboard:view")]
public sealed record DashboardGetOverviewQuery(bool ForceRefresh = false)
    : IRequest<Result<DashboardSnapshot>>;

/// <summary>
/// Sorguyu 10 paralel alt-sorguya böler, sonuçları <see cref="DashboardSnapshot"/>
/// içinde birleştirir ve kısa süreli bellek cache'ine yazar.
/// </summary>
internal sealed class DashboardGetOverviewQueryHandler(
    IDashboardReadRepository readRepository,
    IMemoryCache cache,
    ILogger<DashboardGetOverviewQueryHandler> logger)
    : IRequestHandler<DashboardGetOverviewQuery, Result<DashboardSnapshot>>
{
    private const string CacheKey = "dashboard:overview:v2";

    /// <summary>Otomatik yenileme 30 sn'de bir; 45 sn cache her zaman taze kalır.</summary>
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(45);

    private const int TopBalanceCount = 10;
    private const int TopCategoryCount = 8;
    private const int TopMovementCount = 10;
    private const int ProductStockCount = 100;
    private const int CriticalStockCount = 50;
    private const int TrendMonthCount = 6;

    private static readonly CultureInfo DashboardCulture = CultureInfo.GetCultureInfo("tr-TR");

    // Alt sorgu hata verirse kullanılacak boş değerler; dashboard'un tamamının
    // boş dönmesi yerine yalnızca ilgili bölüm boş kalır.
    private static readonly DashboardCounters EmptyCounters =
        new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0m, 0m, 0);

    private static readonly DashboardBalanceTotals EmptyBalanceTotals =
        new([], [], 0m, 0m);

    private static readonly IReadOnlyList<DashboardMonthAmount> NoInvoiceMonths = [];
    private static readonly IReadOnlyList<DashboardMonthDualAmount> NoDualMonths = [];
    private static readonly IReadOnlyList<DashboardChartPoint> NoChartPoints = [];
    private static readonly IReadOnlyList<DashboardBalancePoint> NoBalancePoints = [];
    private static readonly IReadOnlyList<DashboardRankPoint> NoRankPoints = [];
    private static readonly IReadOnlyList<DashboardProductStockRow> NoProductStockRows = [];
    private static readonly IReadOnlyList<DashboardCriticalStockRow> NoCriticalStockRows = [];

    /// <summary>
    /// Alt sorguları hata bariyerine alır. Tek bir alt sorgunun başarısız olması
    /// <c>Task.WhenAll</c>'i patlatıp tüm dashboard'un boş gelmesine yol açmasın.
    /// </summary>
    private async Task<T> SafeAsync<T>(string name, Func<Task<T>> factory, T fallback)
    {
        try
        {
            return await factory().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dashboard alt sorgusu başarısız: {Query}", name);
            return fallback;
        }
    }

    public async Task<Result<DashboardSnapshot>> Handle(
        DashboardGetOverviewQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.ForceRefresh
            && cache.TryGetValue(CacheKey, out DashboardSnapshot? cached)
            && cached is not null)
        {
            return Result<DashboardSnapshot>.Succeed(cached);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly firstMonth = new DateOnly(
            today.Year,
            today.Month,
            1)
            .AddMonths(-(TrendMonthCount - 1));

        // Her alt-sorgu kendi DbContext'inde çalışır; DbContext thread-safe olmadığı
        // için paralellik repository katmanında, burada ise yalnızca görevler yönetilir.
        //
        // ÖNEMLİ: Alt görevler SafeAsync ile sarılır. Aksi hâlde tek bir alt sorgunun
        // hatası Task.WhenAll'i patlatıp dashboard'un tamamının boş gelmesine yol açar.
        Task<DashboardCounters> countersTask =
            SafeAsync("GetCounters", () => readRepository.GetCountersAsync(cancellationToken), EmptyCounters);
        Task<DashboardBalanceTotals> balancesTask =
            SafeAsync("GetBalanceTotals", () => readRepository.GetBalanceTotalsAsync(TopBalanceCount, cancellationToken), EmptyBalanceTotals);
        Task<IReadOnlyList<DashboardMonthAmount>> invoiceTrendTask =
            SafeAsync("GetMonthlyInvoiceTotals", () => readRepository.GetMonthlyInvoiceTotalsAsync(firstMonth, cancellationToken), NoInvoiceMonths);
        Task<IReadOnlyList<DashboardMonthDualAmount>> monthlyBalanceTask =
            SafeAsync("GetMonthlyBalanceTotals", () => readRepository.GetMonthlyBalanceTotalsAsync(firstMonth, cancellationToken), NoDualMonths);
        Task<IReadOnlyList<DashboardMonthDualAmount>> movementTask =
            SafeAsync("GetMonthlyMovementTotals", () => readRepository.GetMonthlyMovementTotalsAsync(firstMonth, cancellationToken), NoDualMonths);
        Task<IReadOnlyList<DashboardChartPoint>> invoiceTypeTask =
            SafeAsync("GetInvoiceTypeDistribution", () => readRepository.GetInvoiceTypeDistributionAsync(cancellationToken), NoChartPoints);
        Task<IReadOnlyList<DashboardBalancePoint>> categoryStockTask =
            SafeAsync("GetCategoryStockValues", () => readRepository.GetCategoryStockValuesAsync(TopCategoryCount, cancellationToken), NoBalancePoints);
        Task<IReadOnlyList<DashboardRankPoint>> topMovementsTask =
            SafeAsync("GetTopMovementProducts", () => readRepository.GetTopMovementProductsAsync(TopMovementCount, cancellationToken), NoRankPoints);
        Task<IReadOnlyList<DashboardProductStockRow>> productStocksTask =
            SafeAsync("GetProductStocks", () => readRepository.GetProductStocksAsync(ProductStockCount, cancellationToken), NoProductStockRows);
        Task<IReadOnlyList<DashboardCriticalStockRow>> criticalStocksTask =
            SafeAsync("GetCriticalStocks", () => readRepository.GetCriticalStocksAsync(CriticalStockCount, cancellationToken), NoCriticalStockRows);

        await Task.WhenAll(
            countersTask,
            balancesTask,
            invoiceTrendTask,
            monthlyBalanceTask,
            movementTask,
            invoiceTypeTask,
            categoryStockTask,
            topMovementsTask,
            productStocksTask,
            criticalStocksTask);

        DashboardCounters counters = await countersTask;
        DashboardBalanceTotals balances = await balancesTask;
        IReadOnlyList<DashboardCriticalStockRow> criticalStocks = await criticalStocksTask;

        DashboardSnapshot snapshot = new()
        {
            GeneratedAt = DateTimeOffset.Now,

            CustomerCount = counters.Customers,
            SupplierCount = counters.Suppliers,
            ApprovedInvoiceCount = counters.ApprovedInvoices,
            DraftInvoiceCount = counters.DraftInvoices,
            CriticalStockCount = criticalStocks.Count,
            InStockCount = counters.InStockProducts,
            FinishedProductCount = counters.FinishedProducts,
            SemiFinishedProductCount = counters.SemiFinishedProducts,
            TotalProductCount = counters.TotalProducts,
            StockIssueCount = counters.StockIssues,
            PendingApprovalCount = counters.PendingApprovals,

            TotalReceivables = balances.TotalReceivables,
            TotalPayables = balances.TotalPayables,
            TotalStockValue = counters.TotalStockValue,
            TotalStockQuantity = counters.TotalStockQuantity,

            Receivables = balances.Receivables,
            Payables = balances.Payables,
            CriticalStocks = criticalStocks,
            ProductStocks = await productStocksTask,

            InvoiceStatus = BuildInvoiceStatus(
                counters.DraftInvoices,
                counters.ApprovedInvoices,
                counters.TotalInvoices),
            InvoiceTypes = await invoiceTypeTask,
            InvoiceTrend = BuildInvoiceTrend(
                await invoiceTrendTask,
                firstMonth),
            MonthlyBalances = BuildMonthlyBalances(
                await monthlyBalanceTask,
                firstMonth),
            StockMovements = BuildStockMovements(
                await movementTask,
                firstMonth),
            CategoryStocks = await categoryStockTask,
            TopMovementProducts = await topMovementsTask
        };

        cache.Set(CacheKey, snapshot, CacheTtl);

        return Result<DashboardSnapshot>.Succeed(snapshot);
    }

    private static List<DashboardChartPoint> BuildInvoiceStatus(
        int draftCount,
        int approvedCount,
        int totalCount)
    {
        List<DashboardChartPoint> points = [];

        if (approvedCount > 0)
        {
            points.Add(new DashboardChartPoint("Onaylı", approvedCount));
        }

        if (draftCount > 0)
        {
            points.Add(new DashboardChartPoint("Taslak", draftCount));
        }

        int other = Math.Max(0, totalCount - approvedCount - draftCount);
        if (other > 0)
        {
            points.Add(new DashboardChartPoint("Diğer", other));
        }

        return points;
    }

    private static List<DashboardBalancePoint> BuildInvoiceTrend(
        IReadOnlyList<DashboardMonthAmount> rows,
        DateOnly firstMonth)
        => EnumerateMonths(firstMonth)
            .Select(month => new DashboardBalancePoint(
                MonthLabel(month),
                rows
                    .FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month)
                    ?.Amount ?? 0m))
            .ToList();

    private static List<DashboardDualPoint> BuildMonthlyBalances(
        IReadOnlyList<DashboardMonthDualAmount> rows,
        DateOnly firstMonth)
        => EnumerateMonths(firstMonth)
            .Select(month =>
            {
                DashboardMonthDualAmount? row =
                    rows.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month);

                return new DashboardDualPoint(
                    MonthLabel(month),
                    row?.First ?? 0m,
                    row?.Second ?? 0m);
            })
            .ToList();

    private static List<DashboardDualPoint> BuildStockMovements(
        IReadOnlyList<DashboardMonthDualAmount> rows,
        DateOnly firstMonth)
        => EnumerateMonths(firstMonth)
            .Select(month =>
            {
                DashboardMonthDualAmount? row =
                    rows.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month);

                return new DashboardDualPoint(
                    MonthLabel(month),
                    row?.First ?? 0m,
                    row?.Second ?? 0m);
            })
            .ToList();

    private static IEnumerable<DateOnly> EnumerateMonths(DateOnly firstMonth)
    {
        for (int i = 0; i < TrendMonthCount; i++)
        {
            yield return firstMonth.AddMonths(i);
        }
    }

    private static string MonthLabel(DateOnly month)
        => month.ToString("MMM", DashboardCulture);
}
