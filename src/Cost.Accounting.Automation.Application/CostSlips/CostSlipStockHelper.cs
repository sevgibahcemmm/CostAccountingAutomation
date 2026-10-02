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
    /// Stok yetersizliği hatasında gösterilecek ürün tanımını kurar.
    /// Ürün kodu varsa "KOD - Ad", adı yoksa sadece kod.
    /// </summary>
    private static string BuildProductLabel(Product product)
    {
        string code = product.ProductCode.Value;
        string name = product.Name.Value;

        if (string.IsNullOrWhiteSpace(code))
        {
            return name;
        }

        return string.IsNullOrWhiteSpace(name) ? code : $"{code} - {name}";
    }

    /// <summary>
    /// Tüketimin alacak yazılacağı hesabı çözer.
    ///
    /// Yarı mamul tüketendiğinde değer yarı mamulün kendi hesabında
    /// (151.10.xx) taşınır; bu yüzden düşüm oraya alacak yazılmalıdır.
    /// Atölyeye transfer edilmiş normal malzemelerde ise atölye hesabına
    /// alacak yazılır (değer zaten transfer ile atölyede toplanmıştır).
    /// </summary>
    private static IdentityId ResolveConsumptionAccount(
        Dictionary<IdentityId, Product> productMap,
        IdentityId productId,
        IdentityId workshopId)
    {
        Product? product = productMap.TryGetValue(productId, out Product? p) ? p : null;

        return product is not null
            && product.SemiFinishedProductId is not null
            && product.ChartOfAccountId is IdentityId productAccountId
                ? productAccountId
                : workshopId;
    }

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
                // Kullanıcıya ham GUID gösterilemez; ekranda neyin yetmediğini
                // ürün kodu ve adıyla söylemek gerekir.
                string label = productMap.TryGetValue(requested.Key, out Product? product)
                    ? BuildProductLabel(product)
                    : requested.Key.Value.ToString();

                return Result<CostSlipStockPlan>.Failure(
                    $"'{label}' için {slip.CostDate:dd.MM.yyyy} tarihine kadar yeterli stok yok. "
                    + $"Mevcut: {available:n2}, istenen: {requested.Value:n2}.");
            }
        }

        List<ProductMovement> plannedMovements = [];
        List<PlannedLedgerEntry> plannedLedger = [];

        // Tüketim ÜRÜN bazında planlanır, satır bazında değil. Aynı ürün
        // pusulada birden fazla satırda geçse bile katmanlar TEK SEFER tüketilir;
        // satır bazında planlansaydı her satır ilk girişten bağımsız olarak
        // düşer ve ilk giriş aşılırdı.
        foreach (KeyValuePair<IdentityId, decimal> requested in requestedQuantities)
        {
            IdentityId productId = requested.Key;
            decimal quantity = requested.Value;

            if (quantity <= 0)
            {
                continue;
            }

            // FIFO/LIFO kırılımı: ilk giriş tamamen tüketilir, kalan miktar bir
            // sonraki girişten alınır.
            List<(decimal Quantity, decimal UnitPrice)> layers =
                StockIssueCostingHelper.BuildConsumptionLayers(
                    movements, productId, quantity, costingMethod, slip.CostDate);

            IdentityId ledgerAccountId =
                ResolveConsumptionAccount(productMap, productId, slip.WorkshopId);

            // Her katman için AYRI çıkış hareketi yazılır. Tek bir ortalama
            // fiyatlı satır yazılsaydı o fiyat hiçbir giriş kaydına uymaz ve
            // stok hareketi raporunda (fiyat grup anahtarı) girişi olmayan,
            // bakiyesi eksi satırlar oluşurdu.
            foreach ((decimal layerQuantity, decimal layerUnitPrice) in layers)
            {
                ProductMovement output = new(
                    productId: productId,
                    movementType: ProductMovementType.Output,
                    quantity: layerQuantity,
                    unitPrice: new Price(layerUnitPrice),
                    date: slip.CostDate,
                    referenceNo: slip.SlipNumber,
                    description: new Description($"{ProductStockBalanceHelper.CostSlipConsumptionOutputDescriptionPrefix}{slip.SlipNumber}"));

                plannedMovements.Add(output);

                decimal amount = Math.Round(layerQuantity * layerUnitPrice, 2);

                if (amount > 0)
                {
                    plannedLedger.Add(new PlannedLedgerEntry(
                        ledgerAccountId, 0, amount, LedgerSourceType, output.Id));
                }
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