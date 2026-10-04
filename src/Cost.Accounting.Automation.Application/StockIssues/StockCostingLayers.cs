using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.StockIssues;

/// <summary>
/// Tüketilmemiş tek bir giriş (layer): miktar ve o girişin birim maliyeti.
/// </summary>
/// <param name="Quantity">Bu girişten kalan miktar.</param>
/// <param name="UnitPrice">Bu girişin birim maliyeti.</param>
public readonly record struct CostingLayer(decimal Quantity, decimal UnitPrice)
{
    /// <summary>Katmandaki kalan tutar (kuruşa yuvarlanmış).</summary>
    public decimal Amount => Math.Round(Quantity * UnitPrice, 2);
}

/// <summary>
/// Katman listesi üzerindeki FIFO/LIFO yürüyüşünün TEK uygulaması.
///
/// <para>
/// Saf fonksiyonlardır; hem komut katmanı (stok hareketi ve yevmiye yazımı)
/// hem de liste ekranı (kullanıcının girdiği tutarı miktara çevirmek) aynı
/// kırılımı üretsin diye ayrılmıştır. Ekranda üretilen miktar ile onay
/// anında üretilen miktar farklı çıkarsa yevmiye ile pusula tutarı
/// birbirinden kopar.
/// </para>
/// </summary>
public static class StockCostingLayers
{
    /// <summary>
    /// Miktar sütununun veritabanındaki ondalık basamak sayısı
    /// (decimal(18,4)). Hesap bu hassasiyete göre yuvarlanır.
    /// </summary>
    public const int QuantityScale = 4;

    private const decimal QuantityStep = 0.0001m;

    /// <summary>Miktarı veritabanı hassasiyetine indirir.</summary>
    public static decimal NormalizeQuantity(decimal quantity)
        => Math.Round(quantity, QuantityScale, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Verilen <paramref name="amount"/> tutarını karşılayacak katman
    /// kırılımını üretir.
    ///
    /// <para>
    /// FIFO'da en eski katman TAMAMEN karşılanır, kalan tutar bir sonraki
    /// katmandan alınır; katman bitmeden diğerine geçilmez. LIFO'da sıra ters
    /// çevrilir. Katman miktarı dört basamağa yuvarlanır ve yuvarlama tutarı
    /// kuruş altına düşürürse tek basamaklık düzeltmeyle tamamlanır.
    /// </para>
    /// </summary>
    public static List<CostingLayer> TakeByAmount(
        IReadOnlyList<CostingLayer> remainingLayers,
        decimal amount,
        StockCostingMethod costingMethod)
    {
        List<CostingLayer> layers = [.. remainingLayers];
        List<CostingLayer> taken = [];

        if (amount <= 0m || layers.Count == 0)
        {
            return taken;
        }

        decimal remainingAmount = amount;

        while (remainingAmount > 0m && layers.Count > 0)
        {
            int index = costingMethod == StockCostingMethod.Fifo ? 0 : layers.Count - 1;
            CostingLayer layer = layers[index];

            decimal layerAmount = layer.Amount;
            if (layerAmount <= 0m)
            {
                // Fiyatsız/bedeli sıfır katman tutarı karşılayamaz.
                layers.RemoveAt(index);
                continue;
            }

            if (remainingAmount >= layerAmount)
            {
                taken.Add(layer);
                remainingAmount -= layerAmount;
                layers.RemoveAt(index);
                continue;
            }

            // Kısmi katman: kalan tutarı bu girişten al.
            taken.Add(new CostingLayer(
                TakePartialQuantity(layer, remainingAmount),
                layer.UnitPrice));

            remainingAmount = 0m;
        }

        return taken;
    }

    /// <summary>Katmanların toplam tutarı.</summary>
    public static decimal TotalAmount(IEnumerable<CostingLayer> layers)
        => layers.Sum(layer => layer.Amount);

    /// <summary>Katmanların toplam miktarı.</summary>
    public static decimal TotalQuantity(IEnumerable<CostingLayer> layers)
        => layers.Sum(layer => layer.Quantity);

    /// <summary>
    /// Bir katmandan <paramref name="targetAmount"/> tutarını karşılayacak
    /// miktarı bulur. Sonuç katman miktarını aşmaz.
    /// </summary>
    private static decimal TakePartialQuantity(CostingLayer layer, decimal targetAmount)
    {
        if (layer.UnitPrice <= 0m)
        {
            return 0m;
        }

        decimal quantity = NormalizeQuantity(targetAmount / layer.UnitPrice);

        // Dört basamağa yuvarlama tutarı kuruş altına düşürmüş olabilir
        // (ör. 1/3 → 0,3333). Basamak basamak yukarı alınarak tam tutar
        // yakalanır; katman miktarı asla aşılmaz.
        for (int step = 0; step < 64; step++)
        {
            if (quantity >= layer.Quantity)
            {
                break;
            }

            if (Math.Round(quantity * layer.UnitPrice, 2) >= targetAmount)
            {
                break;
            }

            quantity += QuantityStep;
        }

        return Math.Min(quantity, layer.Quantity);
    }
}