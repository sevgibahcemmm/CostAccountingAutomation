using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

internal static class CostSlipStockHelper
{
    private const string LedgerSourceType = "MaliyetTuketimi";

    /// <summary>
    /// Maliyet pusulası onaylandığında stok yan etkilerini üretir:
    /// - Malzeme kalemleri için atölyedeki stoklardan FIFO/LIFO birim maliyetle ÇIKIŞ hareketi
    ///   ve karşılığında yevmiye kaydı: atölye hesabı ALACAK (malzeme tutarı atölye bakiyesinden düşülür).
    /// - Mamul / Yarı mamul için birim maliyet = Genel Toplam / Miktar ile GİRİŞ hareketi.
    /// </summary>
    public static async Task<Result<string>> ApplyStockEffectsAsync(
        CostSlip slip,
        StockCostingMethod costingMethod,
        IProductMovementRepository productMovementRepository,
        IChartOfAccountLedgerPoster ledgerPoster,
        IProductRepository productRepository,
        CancellationToken cancellationToken)
    {
        Result<CostSlipStockPlan> plan = await PlanStockEffectsAsync(
            slip,
            costingMethod,
            productMovementRepository,
            productRepository,
            accumulatedMovements: null,
            cancellationToken);

        if (plan.Data is null)
        {
            return Result<string>.Failure(plan.ErrorMessages);
        }

        return await WritePlanAsync(plan.Data, productMovementRepository, ledgerPoster, cancellationToken);
    }

    /// <summary>
    /// Bir maliyet pusulasının üreteceği stok/defter hareketlerini HESAPLAR ama
    /// YAZMAZ.
    ///
    /// Toplu onayda belgeler kayıt tarihine göre sırayla planlanır ve
    /// <paramref name="accumulatedMovements"/> içinde biriktirilir; böylece sonraki
    /// pusula, öncekilerin tükettiği miktarı da hesaba katar. Validasyon
    /// BAŞARISIZ olursa hiçbir hareket yazılmadığı için yarım kalmış onay oluşmaz.
    /// </summary>
    /// <param name="accumulatedMovements">
    /// Toplu onayda önceki pusulaların planlanmış hareketleri. Stok
    /// kontrolünde bunlar da hesaba katılır (null ise yalnızca kayıtlı hareketler).
    /// </param>
    public static async Task<Result<CostSlipStockPlan>> PlanStockEffectsAsync(
        CostSlip slip,
        StockCostingMethod costingMethod,
        IProductMovementRepository productMovementRepository,
        IProductRepository productRepository,
        IReadOnlyCollection<ProductMovement>? accumulatedMovements,
        CancellationToken cancellationToken)
    {
        var materialLines = slip.CostSlipItems
            .Where(i => i.ProductId is not null)
            .ToList();

        List<IdentityId> productIds = materialLines
            .Select(i => i.ProductId!)
            .Distinct()
            .ToList();

        List<Product> products = await productRepository.GetAll()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        Dictionary<IdentityId, Product> productMap = products.ToDictionary(p => p.Id);

        Dictionary<IdentityId, decimal> requestedQuantities = materialLines
            .GroupBy(i => i.ProductId!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        List<ProductMovement> movements = await StockIssueCostingHelper.LoadMovementsAsync(
            productIds,
            productMovementRepository,
            cancellationToken);

        // Önceki pusulaların planlanmış hareketleri de hesaba katılır; sıralı
        // işleme bu yüzden kayıt tarihine göre yapılmalıdır.
        if (accumulatedMovements is { Count: > 0 })
        {
            movements.AddRange(accumulatedMovements.Where(m => productIds.Contains(m.ProductId)));
        }

        foreach (KeyValuePair<IdentityId, decimal> requested in requestedQuantities)
        {
            decimal available = StockIssueCostingHelper.ComputeAvailableQuantity(
                movements,
                requested.Key,
                slip.CostDate);

            if (requested.Value > available)
            {
                return Result<CostSlipStockPlan>.Failure(
                    $"'{requested.Key.Value}' için bu tarihe kadar yeterli stok yok. Mevcut: {available:n2}, istenen: {requested.Value:n2}.");
            }
        }

        Dictionary<IdentityId, decimal> costMap = StockIssueCostingHelper.BuildUnitCostMap(
            movements,
            requestedQuantities,
            costingMethod,
            slip.CostDate);

        List<ProductMovement> plannedMovements = [];
        List<PlannedLedgerEntry> plannedLedger = [];

        foreach (var line in materialLines)
        {
            IdentityId productId = line.ProductId!;
            decimal unitCost = costMap.TryGetValue(productId, out decimal cost) ? cost : 0m;

            ProductMovement output = new(
                productId: productId,
                movementType: ProductMovementType.Output,
                quantity: line.Quantity,
                unitPrice: new Price(unitCost),
                date: slip.CostDate,
                referenceNo: slip.SlipNumber,
                description: new Description($"{ProductStockBalanceHelper.CostSlipConsumptionOutputDescriptionPrefix}{slip.SlipNumber}"));

            plannedMovements.Add(output);

            decimal amount = Math.Round(line.Quantity * unitCost, 2);
            if (amount > 0)
            {
                // Yarı mamul tüketendiğinde değer yarı mamulün kendi hesabında
                // (151.10.xx) taşınır; bu yüzden düşüm oraya alacak yazılmalıdır.
                // Atölyeye transfer edilmiş normal malzemelerde ise atölye hesabına
                // alacak yazılır (değer zaten transfer ile atölyede toplanmıştır).
                Product? product = productMap.TryGetValue(productId, out Product? p) ? p : null;
                IdentityId ledgerAccountId = product is not null
                    && product.SemiFinishedProductId is not null
                    && product.ChartOfAccountId is IdentityId productAccountId
                        ? productAccountId
                        : slip.WorkshopId;

                plannedLedger.Add(new PlannedLedgerEntry(
                    ledgerAccountId, 0, amount, LedgerSourceType, output.Id));
            }
        }

        if (slip.ProducedProductId is { } producedProductId)
        {
            decimal unitCost = slip.Quantity > 0
                ? Math.Round(slip.GrandTotal / slip.Quantity, 2)
                : 0m;

            ProductMovement input = new(
                productId: producedProductId,
                movementType: ProductMovementType.Input,
                quantity: slip.Quantity,
                unitPrice: new Price(unitCost),
                date: slip.CostDate,
                referenceNo: slip.SlipNumber,
                description: new Description($"{ProductStockBalanceHelper.ProductionInputDescriptionPrefix}{slip.SlipNumber} ({slip.SlipNumber})"));

            plannedMovements.Add(input);
        }

        return Result<CostSlipStockPlan>.Succeed(
            new CostSlipStockPlan(plannedMovements, plannedLedger));
    }

    /// <summary>
    /// Hesaplanmış planı yazar. Tüm yazmalar aynı DbContext'te birikir ve
    /// TransactionBehavior tarafından TEK SaveChangesAsync ile tek transaction
    /// içinde yazılır.
    /// </summary>
    private static async Task<Result<string>> WritePlanAsync(
        CostSlipStockPlan plan,
        IProductMovementRepository productMovementRepository,
        IChartOfAccountLedgerPoster ledgerPoster,
        CancellationToken cancellationToken)
    {
        foreach (ProductMovement movement in plan.Movements)
        {
            await productMovementRepository.AddAsync(movement, cancellationToken);
        }

        foreach (PlannedLedgerEntry entry in plan.LedgerEntries)
        {
            await ledgerPoster.PostAsync(
                entry.AccountId, entry.Debit, entry.Credit, entry.SourceType, entry.SourceId, cancellationToken);
        }

        return Result<string>.Succeed(string.Empty);
    }
}