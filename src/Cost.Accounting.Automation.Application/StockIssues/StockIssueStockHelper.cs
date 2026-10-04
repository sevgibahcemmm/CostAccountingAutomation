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
    /// <para>
    /// Çıkış (ve atölye transferinde atölye girişi) <b>giriş katmanlarına bölünerek</b>
    /// yazılır: tüketilen miktarın hangi alımdan ne kadar karşılandığı ancak böyle
    /// bellidir. Tek bir ortalama fiyatlı satır yazılsaydı o fiyat hiçbir giriş
    /// kaydına uymazdı; stok hareketi listesi raporu fiyatı grup anahtarı olarak
    /// kullandığı için girişi olmayan, bakiyesi eksi satırlar oluşurdu.
    /// </para>
    ///
    /// <para>
    /// Katmanlar <paramref name="movements"/> havuzundan FIFO/LIFO ile hesaplanır.
    /// Katman paylaşıldığı için toplu onayda aynı işlemde onaylanan önceki
    /// belgelerin hareketleri de bu havuza eklenmelidir; aksi hâlde belgeler aynı
    /// giriş katmanını defalarca tüketir.
    /// </para>
    /// </summary>
    public static StockIssueStockPlan PlanStockEffects(
        StockIssue issue,
        ChartOfAccount? targetAccount,
        IReadOnlyDictionary<IdentityId, ChartOfAccount> productAccountMap,
        List<ProductMovement> movements)
    {
        bool isConsumption = issue.IssueType == StockIssueType.Consumption;
        string sourceType = isConsumption ? "StokTuketimi" : "AtolyeTransferi";
        string movementPrefix = isConsumption ? "Tüketim" : "Atölye Transferi";

        List<ProductMovement> plannedMovements = [];
        List<StockIssueLedgerEntry> ledgerEntries = [];

        foreach (StockIssueLine line in issue.Lines)
        {
            // Belge satırı zaten GERÇEK bir giriş katmanıdır (bkz.
            // StockIssueCreateCommand): miktarı ve fiyatı o katmanın kendi
            // değerleridir. Hareketler bu satırlardan birebir üretilir; böylece
            // belgedeki kalem ile stok hareketi aynı satırı gösterir. Satır
            // başına yeniden katman hesabı yapılmaz; onaydan önce satırlar
            // güncel FIFO kırılımıyla eşitlenir (SyncLinesWithFifo).
            decimal layerQuantity = line.Quantity;
            decimal layerUnitPrice = line.UnitCost.Value;

            ProductMovement output = new(
                productId: line.ProductId,
                movementType: ProductMovementType.Output,
                quantity: layerQuantity,
                unitPrice: new Price(layerUnitPrice),
                date: issue.Date,
                referenceNo: issue.DocumentNumber,
                description: new Description($"{movementPrefix} - {issue.DocumentNumber}"),
                stockIssueId: issue.Id);

            plannedMovements.Add(output);

            if (!isConsumption)
            {
                plannedMovements.Add(new ProductMovement(
                    productId: line.ProductId,
                    movementType: ProductMovementType.Input,
                    quantity: layerQuantity,
                    unitPrice: new Price(layerUnitPrice),
                    date: issue.Date,
                    referenceNo: issue.DocumentNumber,
                    description: new Description(
                        $"{ProductStockBalanceHelper.AtelierTransferInputDescriptionPrefix}{targetAccount?.Name.Value ?? string.Empty}"),
                    stockIssueId: issue.Id));
            }

            decimal amount = Math.Round(layerQuantity * layerUnitPrice, 2);
            if (amount > 0)
            {
                if (productAccountMap.TryGetValue(line.ProductId, out ChartOfAccount? productAccount))
                {
                    ledgerEntries.Add(new StockIssueLedgerEntry(
                        productAccount.Id, 0, amount, sourceType, output.Id));
                }

                ledgerEntries.Add(new StockIssueLedgerEntry(
                    issue.TargetAccountId, amount, 0, sourceType, output.Id));
            }
        }

        return new StockIssueStockPlan(plannedMovements, ledgerEntries);
    }

    /// <summary>
    /// Taslak satırlarını güncel FIFO kırılımıyla eşitler.
    ///
    /// <para>
    /// Taslak ile onay arasında başka bir belge onaylanmış ya da yeni alım
    /// yapılmış olabilir. Belgenin satırları eskide kalmışsa yazılacak çıkış
    /// hareketleri de eski olur ve belgedeki kalem ile hareketler ayrışır.
    /// Bu yüzden onaydan hemen önce satırlar, o anki katmanlarla değiştirilir;
    /// böylece <see cref="StockIssueLine"/> ile <c>ProductMovement</c> kayıtları
    /// tanım gereği aynıdır.
    /// </para>
    /// </summary>
    public static void SyncLinesWithFifo(StockIssue issue, List<ProductMovement> movements)
    {
        List<StockIssueLine> synced = [];

        foreach (IGrouping<IdentityId, StockIssueLine> group in issue.Lines.GroupBy(l => l.ProductId))
        {
            decimal requestedQuantity = group.Sum(l => l.Quantity);
            string description = group.First().Description.Value;

            List<(decimal Quantity, decimal UnitPrice)> layers =
                StockIssueCostingHelper.BuildConsumptionLayers(
                    movements, group.Key, requestedQuantity, StockCostingMethod.Fifo, issue.Date);

            if (layers.Count == 0)
            {
                continue;
            }

            foreach ((decimal layerQuantity, decimal layerUnitPrice) in layers)
            {
                synced.Add(new StockIssueLine(
                    stockIssueId: issue.Id,
                    productId: group.Key,
                    quantity: layerQuantity,
                    unitCost: new Price(layerUnitPrice),
                    description: new Description(description)));
            }
        }

        issue.ReplaceLines(synced);
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