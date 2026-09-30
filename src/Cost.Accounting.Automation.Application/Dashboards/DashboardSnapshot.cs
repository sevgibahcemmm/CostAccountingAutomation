namespace Cost.Accounting.Automation.Application.Dashboards;

/// <summary>
/// Dashboard'un tek seferlik okuma sonucu. Tüm grafik ve tablo serileri
/// bu tek aggregate üzerinden beslenir; böylece UI tarafı ayrı sorgu çalıştırmaz.
/// </summary>
public sealed record DashboardSnapshot
{
    public DateTimeOffset GeneratedAt { get; init; }

    // ----- KPI kartları -----
    public int CustomerCount { get; init; }
    public int SupplierCount { get; init; }
    public int ApprovedInvoiceCount { get; init; }
    public int DraftInvoiceCount { get; init; }
    public int CriticalStockCount { get; init; }
    public int InStockCount { get; init; }
    public int FinishedProductCount { get; init; }
    public int SemiFinishedProductCount { get; init; }
    public int TotalProductCount { get; init; }
    public int StockIssueCount { get; init; }

    public decimal TotalReceivables { get; init; }
    public decimal TotalPayables { get; init; }
    public decimal TotalStockValue { get; init; }
    public decimal TotalStockQuantity { get; init; }

    /// <summary>Alacak - Borç farkı. Pozitifse şirket alacağı tarafındadır.</summary>
    public decimal NetBalance => TotalReceivables - TotalPayables;

    // ----- Tablolar -----
    public IReadOnlyList<DashboardBalanceRow> Receivables { get; init; } = [];
    public IReadOnlyList<DashboardBalanceRow> Payables { get; init; } = [];
    public IReadOnlyList<DashboardCriticalStockRow> CriticalStocks { get; init; } = [];
    public IReadOnlyList<DashboardProductStockRow> ProductStocks { get; init; } = [];

    // ----- Grafik serileri -----
    public IReadOnlyList<DashboardChartPoint> InvoiceStatus { get; init; } = [];
    public IReadOnlyList<DashboardChartPoint> InvoiceTypes { get; init; } = [];
    public IReadOnlyList<DashboardBalancePoint> InvoiceTrend { get; init; } = [];
    public IReadOnlyList<DashboardDualPoint> StockMovements { get; init; } = [];
    public IReadOnlyList<DashboardDualPoint> MonthlyBalances { get; init; } = [];
    public IReadOnlyList<DashboardBalancePoint> CategoryStocks { get; init; } = [];
    public IReadOnlyList<DashboardRankPoint> TopMovementProducts { get; init; } = [];
}

public sealed record DashboardBalanceRow(
    string AccountName,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal Balance);

public sealed record DashboardCriticalStockRow(
    string ProductCode,
    string ProductName,
    string CategoryName,
    decimal Stock,
    decimal? MinimumLevel);

public sealed record DashboardChartPoint(
    string Label,
    int Count);

public sealed record DashboardBalancePoint(
    string Label,
    decimal Amount);

/// <summary>İki serili grafikler için ortak satır (ör. giriş/çıkış, alacak/borç).</summary>
public sealed record DashboardDualPoint(
    string Label,
    decimal First,
    decimal Second);

/// <summary>Sıralamalı grafik satırı: miktar + tutar (detay/ipucu için).</summary>
public sealed record DashboardRankPoint(
    string Label,
    decimal Quantity,
    decimal Amount);

public sealed record DashboardProductStockRow(
    string ProductCode,
    string ProductName,
    string CategoryName,
    decimal TotalInput,
    decimal TotalOutput,
    decimal BalanceQuantity,
    decimal TotalInputCost,
    decimal TotalOutputCost,
    decimal BalanceCost);
