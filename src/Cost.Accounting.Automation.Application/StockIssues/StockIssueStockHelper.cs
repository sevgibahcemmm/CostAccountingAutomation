using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.Domain.Shared;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

/// <summary>
/// Bir stok çıkışı/tüketim belgesinin ONAYDA üreteceği hareketler.
/// </summary>
/// <param name="Movements">Depo çıkışı ve (atölye transferinde) atölye girişi.</param>
/// <param name="LedgerEntries">Yevmiye kayıtları.</param>
internal sealed record StockIssueStockPlan(
    IReadOnlyList<ProductMovement> Movements,
    IReadOnlyList<StockIssueLedgerEntry> LedgerEntries);

internal sealed record StockIssueLedgerEntry(
    IdentityId AccountId,
    decimal Debit,
    decimal Credit,
    string SourceType,
    IdentityId? SourceId);

internal static class StockIssueStockHelper
{
    /// <summary>
    /// Taslak bir belgenin üreteceği stok ve yevmiye hareketlerini HESAPLAR ama
    /// YAZMAZ.
    ///
    /// Hesaplama kayıt anında yapılır ve satırda saklanan birim maliyet
    /// (<see cref="StockIssueLine.UnitCost"/>) olarak kalır; böylece onay anında
    /// FIFO/LIFO yeniden hesaplanmaz ve belgenin maliyeti kayıt anındakiyle
    /// birebir aynıdır. Onay yalnızca bu planı yazar.
    /// </summary>
    public static StockIssueStockPlan PlanStockEffects(
        StockIssue issue,
        IReadOnlyDictionary<IdentityId, ChartOfAccount> targetAccountMap)
    {
        bool isConsumption = issue.IssueType == StockIssueType.Consumption;
        string sourceType = isConsumption ? "StokTuketimi" : "AtolyeTransferi";
        string movementPrefix = isConsumption ? "Tüketim" : "Atölye Transferi";

        ChartOfAccount? target = targetAccountMap.GetValueOrDefault(issue.TargetAccountId);

        List<ProductMovement> movements = [];
        List<StockIssueLedgerEntry> ledgerEntries = [];

        foreach (StockIssueLine line in issue.Lines)
        {
            decimal unitCost = line.UnitCost.Value;

            ProductMovement output = new(
                productId: line.ProductId,
                movementType: ProductMovementType.Output,
                quantity: line.Quantity,
                unitPrice: new Price(unitCost),
                date: issue.Date,
                referenceNo: issue.DocumentNumber,
                description: new Description($"{movementPrefix} - {issue.DocumentNumber}"),
                stockIssueId: issue.Id);

            movements.Add(output);

            if (!isConsumption)
            {
                movements.Add(new ProductMovement(
                    productId: line.ProductId,
                    movementType: ProductMovementType.Input,
                    quantity: line.Quantity,
                    unitPrice: new Price(unitCost),
                    date: issue.Date,
                    referenceNo: issue.DocumentNumber,
                    description: new Description(
                        $"{ProductStockBalanceHelper.AtelierTransferInputDescriptionPrefix}{target?.Name.Value ?? string.Empty}"),
                    stockIssueId: issue.Id));
            }

            decimal amount = Math.Round(line.Quantity * unitCost, 2);
            if (amount <= 0)
            {
                continue;
            }

            if (targetAccountMap.TryGetValue(line.ProductId, out ChartOfAccount? productAccount))
            {
                ledgerEntries.Add(new StockIssueLedgerEntry(
                    productAccount.Id, 0, amount, sourceType, output.Id));
            }

            ledgerEntries.Add(new StockIssueLedgerEntry(
                issue.TargetAccountId, amount, 0, sourceType, output.Id));
        }

        return new StockIssueStockPlan(movements, ledgerEntries);
    }

    /// <summary>
    /// Hesaplanmış planı yazar. Tüm yazmalar aynı DbContext'te birikir ve
    /// TransactionBehavior tarafından TEK SaveChangesAsync ile tek transaction
    /// içinde yazılır.
    /// </summary>
    public static async Task WritePlanAsync(
        StockIssueStockPlan plan,
        IProductMovementRepository productMovementRepository,
        IChartOfAccountLedgerPoster ledgerPoster,
        CancellationToken cancellationToken)
    {
        foreach (ProductMovement movement in plan.Movements)
        {
            await productMovementRepository.AddAsync(movement, cancellationToken);
        }

        foreach (StockIssueLedgerEntry entry in plan.LedgerEntries)
        {
            await ledgerPoster.PostAsync(
                entry.AccountId, entry.Debit, entry.Credit, entry.SourceType, entry.SourceId, cancellationToken);
        }
    }
}