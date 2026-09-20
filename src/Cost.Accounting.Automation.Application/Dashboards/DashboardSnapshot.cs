namespace Cost.Accounting.Automation.Application.Dashboards;

public sealed record DashboardSnapshot
{
    public int CustomerCount { get; init; }
    public int SupplierCount { get; init; }
    public int ApprovedInvoiceCount { get; init; }
    public int DraftInvoiceCount { get; init; }
    public decimal TotalReceivables { get; init; }
    public decimal TotalPayables { get; init; }
    public int CriticalStockCount { get; init; }
    public int InStockCount { get; init; }
    public int FinishedProductCount { get; init; }
    public int SemiFinishedProductCount { get; init; }

    public IReadOnlyList<DashboardBalanceRow> Receivables { get; init; } = [];
    public IReadOnlyList<DashboardBalanceRow> Payables { get; init; } = [];
    public IReadOnlyList<DashboardCriticalStockRow> CriticalStocks { get; init; } = [];
    public IReadOnlyList<DashboardChartPoint> InvoiceStatus { get; init; } = [];
    public IReadOnlyList<DashboardBalancePoint> InvoiceTrend { get; init; } = [];
    public IReadOnlyList<DashboardStockPoint> StockMovements { get; init; } = [];
    public IReadOnlyList<DashboardProductStockRow> ProductStocks { get; init; } = [];
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

public sealed record DashboardStockPoint(
    string Label,
    decimal Input,
    decimal Output);

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