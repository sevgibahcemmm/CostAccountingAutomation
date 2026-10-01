using Cost.Accounting.Automation.Application.Dashboards;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

/// <summary>
/// Dashboard için salt-okunur sorgu uygulaması.
///
/// Performans kuralları:
/// <list type="bullet">
///   <item>Her metot kendi kısa ömürlü <see cref="ApplicationDbContext"/>'ini açar;
///   böylece handler bu metotları paralel çalıştırabilir (DbContext thread-safe değildir).</item>
///   <item>Toplam, gruplama, sıralama ve <c>Take</c> işlemleri SQL'e aşağı iner;
///   yalnızca sonuç kümesi kadar satır belleğe taşınır. Önceki sürüm tüm ürün
///   ve tüm cari hareket tablosunu belleğe çekiyordu.</item>
///   <item>Cari hareketlerde filtre + toplam tek sorguda; yalnızca ilk N cari
///   adı sonradan <c>IN</c> ile çekilir.</item>
/// </list>
/// </summary>
internal sealed class DashboardReadRepository(
    IDbContextFactory<ApplicationDbContext> dbFactory) : IDashboardReadRepository
{
    /// <summary>
    /// Yarı mamul üretim deposu kodları. Bu depodaki ürünler kritik stok
    /// raporuna girmez; sayım/üretim akışının kendi deposudur.
    /// </summary>
    private const string SemiFinishedWarehouseCode = "151";
    private const string SemiFinishedSubWarehousePrefix = "151.";

    public async Task<DashboardCounters> GetCountersAsync(CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        int customers = await db.Set<Customer>().AsNoTracking().CountAsync(cancellationToken);
        int suppliers = await db.Set<Supplier>().AsNoTracking().CountAsync(cancellationToken);
        int totalInvoices = await db.Set<Invoice>().AsNoTracking().CountAsync(cancellationToken);
        int approvedInvoices = await db.Set<Invoice>().AsNoTracking()
            .CountAsync(i => i.Status == InvoiceStatus.Approved, cancellationToken);
        int draftInvoices = await db.Set<Invoice>().AsNoTracking()
            .CountAsync(i => i.Status == InvoiceStatus.Draft, cancellationToken);
        int stockIssues = await db.Set<StockIssue>().AsNoTracking().CountAsync(cancellationToken);
        int totalProducts = await db.Set<Product>().AsNoTracking().CountAsync(cancellationToken);

        // Yarı mamul sayısı: üretim girdisi olarak referans verilen ürün satırı.
        // Tam ürün sayısı: bu referansların işaret ettiği benzersiz ürün.
        int semiFinishedProducts = await db.Set<Product>().AsNoTracking()
            .CountAsync(p => p.SemiFinishedProductId != null, cancellationToken);

        int finishedProducts = await db.Set<Product>().AsNoTracking()
            .Where(p => p.SemiFinishedProductId != null)
            .Select(p => p.SemiFinishedProductId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Net stok: giriş - çıkış, ürün bazında SQL'de hesaplanır.
        var netStock = db.Set<ProductMovement>()
            .AsNoTracking()
            .WhereCountsAsProductStock()
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Stock = g.Sum(m => m.MovementType == ProductMovementType.Input
                    ? m.Quantity
                    : -m.Quantity),
                StockValue = g.Sum(m => (m.MovementType == ProductMovementType.Input
                        ? m.Quantity
                        : -m.Quantity)
                    * (m.UnitPrice != null ? m.UnitPrice.Value : 0m))
            });

        var stockSummary = await netStock
            .GroupBy(_ => 1)
            .Select(g => new
            {
                InStockProducts = g.Count(x => x.Stock > 0),
                TotalQuantity = g.Sum(x => x.Stock),
                TotalValue = g.Sum(x => x.StockValue)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new DashboardCounters(
            Customers: customers,
            Suppliers: suppliers,
            ApprovedInvoices: approvedInvoices,
            DraftInvoices: draftInvoices,
            TotalInvoices: totalInvoices,
            StockIssues: stockIssues,
            TotalProducts: totalProducts,
            InStockProducts: stockSummary?.InStockProducts ?? 0,
            SemiFinishedProducts: semiFinishedProducts,
            FinishedProducts: finishedProducts,
            TotalStockQuantity: stockSummary?.TotalQuantity ?? 0m,
            TotalStockValue: stockSummary?.TotalValue ?? 0m);
    }

    public async Task<DashboardBalanceTotals> GetBalanceTotalsAsync(
        int topCount,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // Borç/alacak toplamları ve gruplama tamamen SQL'de.
        var customerTotals = await db.Set<CurrentAccountMovement>().AsNoTracking()
            .Where(m => m.CurrentAccountType == CurrentAccountType.Customer && m.CustomerId != null)
            .GroupBy(m => m.CustomerId)
            .Select(g => new
            {
                AccountId = g.Key,
                Debit = g.Sum(m => m.Debit),
                Credit = g.Sum(m => m.Credit)
            })
            .ToListAsync(cancellationToken);

        var supplierTotals = await db.Set<CurrentAccountMovement>().AsNoTracking()
            .Where(m => m.CurrentAccountType == CurrentAccountType.Supplier && m.SupplierId != null)
            .GroupBy(m => m.SupplierId)
            .Select(g => new
            {
                AccountId = g.Key,
                Debit = g.Sum(m => m.Debit),
                Credit = g.Sum(m => m.Credit)
            })
            .ToListAsync(cancellationToken);

        // En büyük N cari önce belirlenir; yalnızca onların adları sorgulanır
        // (eskiden tüm müşteri/tedarikçi tablosu belleğe alınıyordu).
        List<BalanceCandidate> topReceivables = customerTotals
            .Select(t => new BalanceCandidate(
                t.AccountId,
                t.Debit,
                t.Credit,
                t.Debit - t.Credit))
            .Where(c => c.AccountId != null && c.Balance > 0)
            .OrderByDescending(c => c.Balance)
            .Take(topCount)
            .ToList();

        List<BalanceCandidate> topPayables = supplierTotals
            .Select(t => new BalanceCandidate(
                t.AccountId,
                t.Debit,
                t.Credit,
                t.Debit - t.Credit))
            .Where(c => c.AccountId != null && c.Balance < 0)
            .OrderBy(c => c.Balance)
            .Take(topCount)
            .ToList();

        Dictionary<IdentityId, string> customerNames =
            await LoadCustomerNamesAsync(
                topReceivables.Select(c => c.AccountId),
                cancellationToken);

        Dictionary<IdentityId, string> supplierNames =
            await LoadSupplierNamesAsync(
                topPayables.Select(c => c.AccountId),
                cancellationToken);

        List<DashboardBalanceRow> receivables = topReceivables
            .Select(c => new DashboardBalanceRow(
                ResolveName(customerNames, c.AccountId),
                c.Debit,
                c.Credit,
                c.Balance))
            .ToList();

        List<DashboardBalanceRow> payables = topPayables
            .Select(c => new DashboardBalanceRow(
                ResolveName(supplierNames, c.AccountId),
                c.Debit,
                c.Credit,
                c.Balance))
            .ToList();

        // Toplamlar tüm cari hareketlerden gelir; SQL'de SUM(...) ile tek satır.
        decimal totalReceivables = await db.Set<CurrentAccountMovement>().AsNoTracking()
            .Where(m => m.CurrentAccountType == CurrentAccountType.Customer)
            .SumAsync(m => m.Debit - m.Credit, cancellationToken);

        decimal totalPayables = await db.Set<CurrentAccountMovement>().AsNoTracking()
            .Where(m => m.CurrentAccountType == CurrentAccountType.Supplier)
            .SumAsync(m => m.Credit - m.Debit, cancellationToken);

        return new DashboardBalanceTotals(
            receivables,
            payables,
            Math.Max(0m, totalReceivables),
            Math.Max(0m, totalPayables));
    }

    public async Task<IReadOnlyList<DashboardMonthAmount>> GetMonthlyInvoiceTotalsAsync(
        DateOnly firstMonth,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Set<Invoice>().AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Approved && i.Date >= firstMonth)
            .GroupBy(i => new { i.Date.Year, i.Date.Month })
            .Select(g => new DashboardMonthAmount(
                g.Key.Year,
                g.Key.Month,
                g.Sum(i => i.GrandTotal)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardMonthDualAmount>> GetMonthlyBalanceTotalsAsync(
        DateOnly firstMonth,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // Müşteri tarafı alacak, tedarikçi tarafı borç tarafıdır.
        return await db.Set<CurrentAccountMovement>().AsNoTracking()
            .Where(m => m.Date >= firstMonth)
            .GroupBy(m => new
            {
                m.Date.Year,
                m.Date.Month,
                m.CurrentAccountType
            })
            .Select(g => new DashboardMonthDualAmount(
                g.Key.Year,
                g.Key.Month,
                g.Key.CurrentAccountType == CurrentAccountType.Customer
                    ? g.Sum(m => m.Debit - m.Credit)
                    : 0m,
                g.Key.CurrentAccountType == CurrentAccountType.Supplier
                    ? g.Sum(m => m.Credit - m.Debit)
                    : 0m))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardMonthDualAmount>> GetMonthlyMovementTotalsAsync(
        DateOnly firstMonth,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Set<ProductMovement>().AsNoTracking()
            .WhereCountsAsProductStock()
            .Where(m => m.Date >= firstMonth)
            .GroupBy(m => new { m.Date.Year, m.Date.Month, m.MovementType })
            .Select(g => new DashboardMonthDualAmount(
                g.Key.Year,
                g.Key.Month,
                g.Key.MovementType == ProductMovementType.Input ? g.Sum(m => m.Quantity) : 0m,
                g.Key.MovementType == ProductMovementType.Output ? g.Sum(m => m.Quantity) : 0m))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardChartPoint>> GetInvoiceTypeDistributionAsync(
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Set<Invoice>().AsNoTracking()
            .GroupBy(i => i.InvoiceType)
            .Select(g => new DashboardChartPoint(
                InvoiceTypeLabels.Get(g.Key),
                g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardBalancePoint>> GetCategoryStockValuesAsync(
        int take,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // NOT: Bu sürümde JOIN + GroupBy kombinasyonunda EF Core, SUM
        // aggregate'ini client-side'a düşürüp "could not be translated" hatası
        // veriyor. Bu yüzden her stok sorgusu "önce ürün başına tek satır
        // grupla, sonra bellekte birleştir" desenini kullanır.
        Dictionary<IdentityId, ProductMovementAggregate> aggregatesByProduct =
            await LoadProductMovementAggregatesAsync(db, cancellationToken);

        List<ProductStockCandidate> products = await db.Set<Product>().AsNoTracking()
            .Select(p => new ProductStockCandidate(
                p.Id,
                p.ProductCode.Value,
                p.Name.Value,
                p.Category!.Name.Value,
                p.MinimumProductLevel,
                p.Warehouse!.Code.Value))
            .ToListAsync(cancellationToken);

        return products
            .Select(p => new DashboardBalancePoint(
                p.CategoryName,
                aggregatesByProduct.TryGetValue(p.Id, out ProductMovementAggregate? aggregate)
                    ? aggregate.InputCost - aggregate.OutputCost
                    : 0m))
            .GroupBy(x => x.Label)
            .Select(categoryGroup => new DashboardBalancePoint(
                categoryGroup.Key,
                categoryGroup.Sum(x => x.Amount)))
            .OrderByDescending(x => x.Amount)
            .Take(take)
            .ToList();
    }

    public async Task<IReadOnlyList<DashboardRankPoint>> GetTopMovementProductsAsync(
        int take,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        Dictionary<IdentityId, ProductMovementAggregate> aggregatesByProduct =
            await LoadProductMovementAggregatesAsync(db, cancellationToken);

        List<ProductStockCandidate> products = await db.Set<Product>().AsNoTracking()
            .Select(p => new ProductStockCandidate(
                p.Id,
                p.ProductCode.Value,
                p.Name.Value,
                p.Category!.Name.Value,
                p.MinimumProductLevel,
                p.Warehouse!.Code.Value))
            .ToListAsync(cancellationToken);

        return products
            // Hareketi hiç olmayan ürünler eski davranışta da listelenmiyordu.
            .Where(p => aggregatesByProduct.ContainsKey(p.Id))
            .Select(p =>
            {
                ProductMovementAggregate aggregate = aggregatesByProduct[p.Id];
                decimal balance = aggregate.InputQuantity - aggregate.OutputQuantity;

                return (Row: new DashboardRankPoint(
                    p.ProductName,
                    balance,
                    aggregate.InputCost - aggregate.OutputCost),
                    // Bu grafik yalnizca hareket gormus urunleri zaten gosterir;
                    // yarı mamül depoları (ara üretim) hariç tutulur.
                    Keep: !DashboardProductStockFilter.IsSemiFinishedWarehousePublic(p.WarehouseCode));
            })
            .Where(x => x.Keep)
            .Select(x => x.Row)
            .OrderByDescending(x => x.Quantity)
            .ThenBy(x => x.Label)
            .Take(take)
            .ToList();
    }

    public async Task<IReadOnlyList<DashboardProductStockRow>> GetProductStocksAsync(
        int take,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        Dictionary<IdentityId, ProductMovementAggregate> aggregatesByProduct =
            await LoadProductMovementAggregatesAsync(db, cancellationToken);

        List<ProductStockCandidate> products = await db.Set<Product>().AsNoTracking()
            .Select(p => new ProductStockCandidate(
                p.Id,
                p.ProductCode.Value,
                p.Name.Value,
                p.Category!.Name.Value,
                p.MinimumProductLevel,
                p.Warehouse!.Code.Value))
            .ToListAsync(cancellationToken);

        // Kural: maliyet tablosunda TUM urunler yerine yalnizca su urunler
        // listelenir:
        //   1) son hafta icinde hareket gorulmus urunler, ya da
        //   2) kritik seviyeye YAKLASAN urunler
        //      (minimum seviye 5 ise 2 katindan az olanlar => esik 10).
        DateOnly recentSince = DateOnly.FromDateTime(DateTime.Today)
            .AddDays(-DashboardProductStockFilter.RecentDays);

        HashSet<IdentityId> recentlyMovedIds = (await db.Set<ProductMovement>().AsNoTracking()
                .Where(m => m.Date >= recentSince)
                .Select(m => m.ProductId)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();

        return products
            .Select(p =>
            {
                aggregatesByProduct.TryGetValue(p.Id, out ProductMovementAggregate? aggregate);

                decimal inputQuantity = aggregate?.InputQuantity ?? 0m;
                decimal outputQuantity = aggregate?.OutputQuantity ?? 0m;
                decimal inputCost = aggregate?.InputCost ?? 0m;
                decimal outputCost = aggregate?.OutputCost ?? 0m;

                decimal balanceQuantity = inputQuantity - outputQuantity;

                return (Row: new DashboardProductStockRow(
                    p.ProductCode,
                    p.ProductName,
                    p.CategoryName,
                    inputQuantity,
                    outputQuantity,
                    balanceQuantity,
                    inputCost,
                    outputCost,
                    inputCost - outputCost),
                    Keep: DashboardProductStockFilter.ShouldInclude(
                        recentlyMovedIds.Contains(p.Id),
                        p.MinimumLevel,
                        balanceQuantity,
                        p.WarehouseCode));
            })
            .Where(x => x.Keep)
            .Select(x => x.Row)
            .OrderByDescending(x => x.BalanceCost)
            .ThenBy(x => x.ProductName)
            .Take(take)
            .ToList();
    }

    public async Task<IReadOnlyList<DashboardCriticalStockRow>> GetCriticalStocksAsync(
        int take,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        List<ProductNetStock> netStocks = await db.Set<ProductMovement>().AsNoTracking()
            .WhereCountsAsProductStock()
            .GroupBy(m => m.ProductId)
            .Select(g => new ProductNetStock(
                g.Key,
                g.Sum(m => m.MovementType == ProductMovementType.Input
                    ? m.Quantity
                    : -m.Quantity)))
            .ToListAsync(cancellationToken);

        Dictionary<IdentityId, decimal> stockByProduct =
            netStocks.ToDictionary(s => s.ProductId, s => s.Stock);

        // Depo kodu gezgini üzerinden WHERE yazmak da çevrilmediği için kod
        // düz sütun olarak okunup filtre bellekte uygulanır.
        List<CriticalStockCandidate> products = await db.Set<Product>().AsNoTracking()
            .Select(p => new CriticalStockCandidate(
                p.Id,
                p.ProductCode.Value,
                p.Name.Value,
                p.Category!.Name.Value,
                p.MinimumProductLevel,
                p.Warehouse!.Code.Value))
            .ToListAsync(cancellationToken);

        // Hareketi hiç olmayan ürünlerin stoğu 0 kabul edilir (kritik sayılır).
        return products
            .Where(p => p.WarehouseCode == null
                || (!p.WarehouseCode.StartsWith(SemiFinishedWarehouseCode, StringComparison.Ordinal)
                    && !p.WarehouseCode.StartsWith(SemiFinishedSubWarehousePrefix, StringComparison.Ordinal)))
            .Select(p => new DashboardCriticalStockRow(
                p.ProductCode,
                p.ProductName,
                p.CategoryName,
                stockByProduct.TryGetValue(p.Id, out decimal stock) ? stock : 0m,
                p.MinimumLevel))
            .Where(r => r.Stock <= 0
                || (r.MinimumLevel != null
                    && r.Stock <= r.MinimumLevel.Value * DashboardProductStockFilter.ApproachingFactor))
            .OrderBy(r => r.Stock)
            .ThenBy(r => r.ProductCode)
            .Take(take)
            .ToList();
    }

    // ------------------------------------------------------------------ alt sorgular

    /// <summary>
    /// Ürün başına giriş/çıkış miktar ve maliyet toplamları. Satır sayısı ürün
    /// adediyle sınırlıdır; ham hareket tablosunun tamamı belleğe gelmez.
    /// </summary>
    private static async Task<Dictionary<IdentityId, ProductMovementAggregate>>
        LoadProductMovementAggregatesAsync(
            ApplicationDbContext db,
            CancellationToken cancellationToken)
    {
        List<ProductMovementAggregate> aggregates = await db.Set<ProductMovement>().AsNoTracking()
            .WhereCountsAsProductStock()
            .GroupBy(m => m.ProductId)
            .Select(g => new ProductMovementAggregate(
                g.Key,
                g.Sum(m => m.MovementType == ProductMovementType.Input ? m.Quantity : 0m),
                g.Sum(m => m.MovementType == ProductMovementType.Output ? m.Quantity : 0m),
                g.Sum(m => m.MovementType == ProductMovementType.Input
                    ? m.Quantity * (m.UnitPrice != null ? m.UnitPrice.Value : 0m)
                    : 0m),
                g.Sum(m => m.MovementType == ProductMovementType.Output
                    ? m.Quantity * (m.UnitPrice != null ? m.UnitPrice.Value : 0m)
                    : 0m)))
            .ToListAsync(cancellationToken);

        return aggregates.ToDictionary(a => a.ProductId);
    }

    private async Task<Dictionary<IdentityId, string>> LoadCustomerNamesAsync(
        IEnumerable<IdentityId?> ids,
        CancellationToken cancellationToken)
    {
        HashSet<IdentityId> keys =
            [
                .. ids.Where(id => id != null).Select(id => id!)
            ];

        if (keys.Count == 0)
        {
            return [];
        }

        await using ApplicationDbContext db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        // HashSet<IdentityId>.Contains SQL'e IN (...) olarak iner; IdentityId.Value
        // kullanmak EF'de çevrilmediği için kıyaslama doğrudan IdentityId ile yapılır.
        return await db.Set<Customer>().AsNoTracking()
            .Where(c => keys.Contains(c.Id))
            .Select(c => new { c.Id, Name = c.Name.Value })
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
    }

    private async Task<Dictionary<IdentityId, string>> LoadSupplierNamesAsync(
        IEnumerable<IdentityId?> ids,
        CancellationToken cancellationToken)
    {
        HashSet<IdentityId> keys =
            [
                .. ids.Where(id => id != null).Select(id => id!)
            ];

        if (keys.Count == 0)
        {
            return [];
        }

        await using ApplicationDbContext db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Set<Supplier>().AsNoTracking()
            .Where(s => keys.Contains(s.Id))
            .Select(s => new { s.Id, Name = s.Name.Value })
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);
    }

    private static string ResolveName(
        Dictionary<IdentityId, string> names,
        IdentityId? accountId)
        => accountId != null
            && names.TryGetValue(accountId, out string? name)
                ? name ?? "-"
                : "-";

    // ------------------------------------------------------------------ yardımcı tipler

    /// <summary>
    /// LINQ sorgusunda kullanılabilen değer tüneleri. EF Core bunları SQL'e çevirir.
    /// </summary>
    private sealed record ProductMovementAggregate(
        IdentityId ProductId,
        decimal InputQuantity,
        decimal OutputQuantity,
        decimal InputCost,
        decimal OutputCost);

    private sealed record ProductNetStock(IdentityId ProductId, decimal Stock);

    private sealed record ProductStockCandidate(
        IdentityId Id,
        string ProductCode,
        string ProductName,
        string CategoryName,
        decimal? MinimumLevel,
        string? WarehouseCode);

    private sealed record CriticalStockCandidate(
        IdentityId Id,
        string ProductCode,
        string ProductName,
        string CategoryName,
        decimal? MinimumLevel,
        string? WarehouseCode);

    private readonly record struct BalanceCandidate(
        IdentityId? AccountId,
        decimal Debit,
        decimal Credit,
        decimal Balance);
}

internal static class InvoiceTypeLabels
{
    public static string Get(InvoiceType type) => type switch
    {
        InvoiceType.Purchase => "Alış Faturası",
        InvoiceType.Sales => "Satış Faturası",
        InvoiceType.PurchaseReturn => "Alış İadesi",
        InvoiceType.SalesReturn => "Satış İadesi",
        _ => type.ToString()
    };
}
