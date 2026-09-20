using System.Globalization;
using Cost.Accounting.Automation.Application.Dashboards;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Cost.Accounting.Automation.Infrastructure.Services;

internal sealed class DashboardDataProvider(
    IMemoryCache cache,
    IDbContextFactory<ApplicationDbContext> dbFactory) : IDashboardDataProvider
{
    private const string CacheKey = "dashboard:overview:v1";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(15);

    public async Task<DashboardSnapshot> GetOverviewAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        if (!forceRefresh
            && cache.TryGetValue(CacheKey, out DashboardSnapshot? cached)
            && cached is not null)
        {
            return cached;
        }

        DashboardSnapshot snapshot =
            await BuildSnapshotAsync(cancellationToken);

        cache.Set(CacheKey, snapshot, CacheTtl);

        return snapshot;
    }

    private async Task<DashboardSnapshot> BuildSnapshotAsync(CancellationToken ct)
    {
        Task<(int Customers, int Suppliers)> countsTask =
            LoadCountsAsync(ct);

        Task<(int Finished, int SemiFinished)> productCountsTask =
            LoadProductCountsAsync(ct);

        Task<(int Approved, int Draft)> invoiceStatusTask =
            LoadInvoiceStatusAsync(ct);

        Task<Dictionary<IdentityId, decimal>> stockTotalsTask =
            LoadStockTotalsAsync(ct);

        Task<(List<DashboardBalanceRow> Receivables, List<DashboardBalanceRow> Payables)> balancesTask =
            LoadBalancesAsync(ct);

        Task<List<DashboardBalancePoint>> invoiceTrendTask =
            LoadInvoiceTrendAsync(ct);

        Task<List<DashboardStockPoint>> stockMovementsTask =
            LoadStockMovementsAsync(ct);

        Task<List<DashboardProductStockRow>> productStocksTask =
            LoadProductStocksAsync(ct);

        await Task.WhenAll(
            countsTask,
            productCountsTask,
            invoiceStatusTask,
            stockTotalsTask,
            balancesTask,
            invoiceTrendTask,
            stockMovementsTask,
            productStocksTask);

        Dictionary<IdentityId, decimal> stockTotals = await stockTotalsTask;

        Task<List<DashboardCriticalStockRow>> criticalStockTask =
            LoadCriticalStockRowsAsync(stockTotals, ct);

        await criticalStockTask;

        var (customers, suppliers) = await countsTask;
        var (finished, semiFinished) = await productCountsTask;
        var (approved, draft) = await invoiceStatusTask;
        var (receivables, payables) = await balancesTask;
        var invoiceTrend = await invoiceTrendTask;
        var stockMovements = await stockMovementsTask;
        var criticalStocks = await criticalStockTask;
        var productStocks = await productStocksTask;

        decimal totalReceivables = receivables.Sum(r => r.Balance);
        decimal totalPayables = payables.Sum(r => -r.Balance);
        int inStockCount = stockTotals.Values.Count(v => v > 0);

        return new DashboardSnapshot
        {
            CustomerCount = customers,
            SupplierCount = suppliers,
            ApprovedInvoiceCount = approved,
            DraftInvoiceCount = draft,
            TotalReceivables = totalReceivables,
            TotalPayables = totalPayables,
            CriticalStockCount = criticalStocks.Count,
            InStockCount = inStockCount,
            FinishedProductCount = finished,
            SemiFinishedProductCount = semiFinished,
            Receivables = receivables,
            Payables = payables,
            CriticalStocks = criticalStocks,
            InvoiceStatus = BuildInvoiceStatus(approved, draft),
            InvoiceTrend = invoiceTrend,
            StockMovements = stockMovements,
            ProductStocks = productStocks
        };
    }

    private async Task<(int Customers, int Suppliers)> LoadCountsAsync(CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        int customers = await db.Set<Customer>().AsNoTracking().CountAsync(ct);
        int suppliers = await db.Set<Supplier>().AsNoTracking().CountAsync(ct);

        return (customers, suppliers);
    }

    private async Task<(int Finished, int SemiFinished)> LoadProductCountsAsync(CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        List<IdentityId?> semiReferences = await db.Set<Product>()
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.SemiFinishedProductId != null)
            .Select(p => p.SemiFinishedProductId)
            .ToListAsync(ct);

        return (
            Finished: semiReferences
                .Where(id => id != null)
                .Select(id => id!.Value)
                .Distinct()
                .Count(),
            SemiFinished: semiReferences.Count);
    }

    private async Task<(int Approved, int Draft)> LoadInvoiceStatusAsync(CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        int approved = await db.Set<Invoice>()
            .AsNoTracking()
            .CountAsync(i => i.Status == InvoiceStatus.Approved, ct);

        int draft = await db.Set<Invoice>()
            .AsNoTracking()
            .CountAsync(i => i.Status == InvoiceStatus.Draft, ct);

        return (approved, draft);
    }

    private async Task<Dictionary<IdentityId, decimal>> LoadStockTotalsAsync(CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        var rows = await db.Set<ProductMovement>()
            .AsNoTracking()
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Stock = g.Sum(m => m.MovementType == ProductMovementType.Input ? m.Quantity : -m.Quantity)
            })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.ProductId, r => r.Stock);
    }

    private async Task<(List<DashboardBalanceRow> Receivables, List<DashboardBalanceRow> Payables)>
        LoadBalancesAsync(CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        Dictionary<IdentityId, string> customerNames =
            await db.Set<Customer>()
                .AsNoTracking()
                .ToDictionaryAsync(
                    c => c.Id,
                    c => c.Name.Value,
                    ct);

        Dictionary<IdentityId, string> supplierNames =
            await db.Set<Supplier>()
                .AsNoTracking()
                .ToDictionaryAsync(
                    s => s.Id,
                    s => s.Name.Value,
                    ct);

        var receivableTotals = await db.Set<CurrentAccountMovement>()
            .AsNoTracking()
            .Where(m => m.CurrentAccountType == CurrentAccountType.Customer && m.CustomerId != null)
            .GroupBy(m => m.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                Debit = g.Sum(m => m.Debit),
                Credit = g.Sum(m => m.Credit)
            })
            .ToListAsync(ct);

        var payableTotals = await db.Set<CurrentAccountMovement>()
            .AsNoTracking()
            .Where(m => m.CurrentAccountType == CurrentAccountType.Supplier && m.SupplierId != null)
            .GroupBy(m => m.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                Debit = g.Sum(m => m.Debit),
                Credit = g.Sum(m => m.Credit)
            })
            .ToListAsync(ct);

        List<DashboardBalanceRow> receivables = receivableTotals
            .Select(b => new DashboardBalanceRow(
                b.CustomerId != null && customerNames.TryGetValue(b.CustomerId, out string? n)
                    ? n ?? "-"
                    : "-",
                b.Debit,
                b.Credit,
                b.Debit - b.Credit))
            .Where(r => r.Balance > 0)
            .OrderByDescending(r => r.Balance)
            .ToList();

        List<DashboardBalanceRow> payables = payableTotals
            .Select(b => new DashboardBalanceRow(
                b.SupplierId != null && supplierNames.TryGetValue(b.SupplierId, out string? n)
                    ? n ?? "-"
                    : "-",
                b.Debit,
                b.Credit,
                b.Debit - b.Credit))
            .Where(r => r.Balance < 0)
            .OrderBy(r => r.Balance)
            .ToList();

        return (receivables, payables);
    }

    private async Task<List<DashboardCriticalStockRow>>
        LoadCriticalStockRowsAsync(
            Dictionary<IdentityId, decimal> stockTotals,
            CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        var products = await db.Set<Product>()
            .AsNoTracking()
            .Select(p => new
            {
                Id = p.Id,
                ProductCode = p.ProductCode.Value,
                ProductName = p.Name.Value,
                CategoryName = p.Category!.Name.Value,
                MinimumLevel = p.MinimumProductLevel
            })
            .ToListAsync(ct);

        return products
            .Select(p => new DashboardCriticalStockRow(
                p.ProductCode,
                p.ProductName,
                p.CategoryName,
                stockTotals.GetValueOrDefault(p.Id),
                p.MinimumLevel))
            .Where(r => r.Stock <= 0
                || (r.MinimumLevel != null && r.Stock <= r.MinimumLevel))
            .OrderBy(r => r.Stock)
            .Take(50)
            .ToList();
    }

    private static List<DashboardChartPoint> BuildInvoiceStatus(
        int approvedCount,
        int draftCount)
    {
        List<DashboardChartPoint> points = [];

        if (draftCount > 0)
        {
            points.Add(new DashboardChartPoint("Taslak", draftCount));
        }

        if (approvedCount > 0)
        {
            points.Add(new DashboardChartPoint("Onaylı", approvedCount));
        }

        return points;
    }

    private async Task<List<DashboardBalancePoint>> LoadInvoiceTrendAsync(CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        DateOnly firstMonth =
            new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1)
                .AddMonths(-5);

        var grouped = await db.Set<Invoice>()
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Approved && i.Date >= firstMonth)
            .GroupBy(i => new { i.Date.Year, i.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(i => i.GrandTotal)
            })
            .ToListAsync(ct);

        CultureInfo culture = CultureInfo.GetCultureInfo("tr-TR");

        List<DashboardBalancePoint> result = [];

        for (int i = 0; i < 6; i++)
        {
            DateOnly month = firstMonth.AddMonths(i);

            var point = grouped.FirstOrDefault(
                g => g.Year == month.Year && g.Month == month.Month);

            result.Add(new DashboardBalancePoint(
                month.ToString("MMMM", culture),
                point?.Total ?? 0));
        }

        return result;
    }

    private async Task<List<DashboardStockPoint>> LoadStockMovementsAsync(CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        DateOnly firstMonth =
            new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1)
                .AddMonths(-5);

        var grouped = await db.Set<ProductMovement>()
            .AsNoTracking()
            .Where(m => m.Date >= firstMonth)
            .GroupBy(m => new { m.Date.Year, m.Date.Month, m.MovementType })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Type = g.Key.MovementType,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToListAsync(ct);

        CultureInfo culture = CultureInfo.GetCultureInfo("tr-TR");

        List<DashboardStockPoint> result = [];

        for (int i = 0; i < 6; i++)
        {
            DateOnly month = firstMonth.AddMonths(i);

            decimal input = grouped
                .Where(g => g.Year == month.Year
                    && g.Month == month.Month
                    && g.Type == ProductMovementType.Input)
                .Sum(g => g.Quantity);

            decimal output = grouped
                .Where(g => g.Year == month.Year
                    && g.Month == month.Month
                    && g.Type == ProductMovementType.Output)
                .Sum(g => g.Quantity);

            result.Add(new DashboardStockPoint(
                month.ToString("MMMM", culture),
                input,
                output));
        }

        return result;
    }

    private async Task<List<DashboardProductStockRow>> LoadProductStocksAsync(CancellationToken ct)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(ct);

        var productInfo = await db.Set<Product>()
            .AsNoTracking()
            .Select(p => new
            {
                Id = p.Id,
                ProductCode = p.ProductCode.Value,
                ProductName = p.Name.Value,
                CategoryName = p.Category!.Name.Value
            })
            .ToListAsync(ct);

        var movements = await db.Set<ProductMovement>()
            .AsNoTracking()
            .Select(m => new
            {
                m.ProductId,
                m.MovementType,
                m.Quantity,
                UnitPrice = m.UnitPrice != null ? m.UnitPrice.Value : 0m
            })
            .ToListAsync(ct);

        Dictionary<IdentityId, (decimal InputQty, decimal OutputQty, decimal InputCost, decimal OutputCost)>
            totals = movements
                .GroupBy(m => m.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        decimal inputQty =
                            g.Where(m => m.MovementType == ProductMovementType.Input)
                                .Sum(m => m.Quantity);

                        decimal outputQty =
                            g.Where(m => m.MovementType == ProductMovementType.Output)
                                .Sum(m => m.Quantity);

                        decimal inputCost =
                            g.Where(m => m.MovementType == ProductMovementType.Input)
                                .Sum(m => m.Quantity * m.UnitPrice);

                        decimal outputCost =
                            g.Where(m => m.MovementType == ProductMovementType.Output)
                                .Sum(m => m.Quantity * m.UnitPrice);

                        return (inputQty, outputQty, inputCost, outputCost);
                    });

        return productInfo
            .Select(p =>
            {
                bool has = totals.TryGetValue(
                    p.Id,
                    out (decimal InputQty, decimal OutputQty, decimal InputCost, decimal OutputCost) t);

                decimal inputQty = has ? t.InputQty : 0m;
                decimal outputQty = has ? t.OutputQty : 0m;
                decimal inputCost = has ? t.InputCost : 0m;
                decimal outputCost = has ? t.OutputCost : 0m;

                return new DashboardProductStockRow(
                    p.ProductCode,
                    p.ProductName,
                    p.CategoryName,
                    inputQty,
                    outputQty,
                    inputQty - outputQty,
                    inputCost,
                    outputCost,
                    inputCost - outputCost);
            })
            .OrderByDescending(r => r.BalanceCost)
            .ThenBy(r => r.ProductName)
            .ToList();
    }
}