namespace Cost.Accounting.Automation.Application.Dashboards;

/// <summary>
/// Dashboard için salt-okunur veri portu (CQRS "Q" tarafı).
/// Uygulama katmanı bu arayüzü bilir; SQL'e çevrilen ağır sorgular
/// Infrastructure'daki <c>DashboardReadRepository</c> tarafından yürütülür.
/// Böylece handler birden fazla DbContext ile paralel sorgu çalıştırabilir.
/// </summary>
public interface IDashboardReadRepository
{
    /// <summary>KPI sayaçları ve stok özetleri (tek turda birkaç aggregate).</summary>
    Task<DashboardCounters> GetCountersAsync(CancellationToken cancellationToken);

    /// <summary>En yüksek alacak/borç listeleri ve toplamlar.</summary>
    Task<DashboardBalanceTotals> GetBalanceTotalsAsync(int topCount, CancellationToken cancellationToken);

    /// <summary>Onaylı fatura aylık toplamları (gruptaki ham satırlar, eksik aylar boş gelir).</summary>
    Task<IReadOnlyList<DashboardMonthAmount>> GetMonthlyInvoiceTotalsAsync(
        DateOnly firstMonth,
        CancellationToken cancellationToken);

    /// <summary>Cari hareketlerinin aylık borç/alacak toplamları.</summary>
    Task<IReadOnlyList<DashboardMonthDualAmount>> GetMonthlyBalanceTotalsAsync(
        DateOnly firstMonth,
        CancellationToken cancellationToken);

    /// <summary>Stok giriş/çıkış miktarlarının aylık toplamları.</summary>
    Task<IReadOnlyList<DashboardMonthDualAmount>> GetMonthlyMovementTotalsAsync(
        DateOnly firstMonth,
        CancellationToken cancellationToken);

    /// <summary>Fatura türüne göre adet dağılımı.</summary>
    Task<IReadOnlyList<DashboardChartPoint>> GetInvoiceTypeDistributionAsync(CancellationToken cancellationToken);

    /// <summary>Kategori bazında stok değeri (en yüksek N kategori).</summary>
    Task<IReadOnlyList<DashboardBalancePoint>> GetCategoryStockValuesAsync(
        int take,
        CancellationToken cancellationToken);

    /// <summary>En çok hareket görmüş ürünler.</summary>
    Task<IReadOnlyList<DashboardRankPoint>> GetTopMovementProductsAsync(
        int take,
        CancellationToken cancellationToken);

    /// <summary>Stok özeti tablosu (bakiye değerine göre en yüksek N ürün).</summary>
    Task<IReadOnlyList<DashboardProductStockRow>> GetProductStocksAsync(
        int take,
        CancellationToken cancellationToken);

    /// <summary>Kritik stok tablosu (en düşük N ürün).</summary>
    Task<IReadOnlyList<DashboardCriticalStockRow>> GetCriticalStocksAsync(
        int take,
        CancellationToken cancellationToken);
}

/// <summary>Tek satırlık sayaç özeti.</summary>
public sealed record DashboardCounters(
    int Customers,
    int Suppliers,
    int ApprovedInvoices,
    int DraftInvoices,
    int TotalInvoices,
    int StockIssues,
    int TotalProducts,
    int InStockProducts,
    int SemiFinishedProducts,
    int FinishedProducts,
    decimal TotalStockQuantity,
    decimal TotalStockValue);

/// <summary>Alacak/borç toplamları ve en yüksek N cari listesi.</summary>
public sealed record DashboardBalanceTotals(
    IReadOnlyList<DashboardBalanceRow> Receivables,
    IReadOnlyList<DashboardBalanceRow> Payables,
    decimal TotalReceivables,
    decimal TotalPayables);

public sealed record DashboardMonthAmount(int Year, int Month, decimal Amount);

public sealed record DashboardMonthDualAmount(int Year, int Month, decimal First, decimal Second);
