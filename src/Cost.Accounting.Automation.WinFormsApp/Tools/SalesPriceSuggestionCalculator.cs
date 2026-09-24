using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

public static class SalesPriceSuggestionCalculator
{
    /// <summary>
    /// Ürünün maliyet fiyatını bulur: önce en güncel alış fiyatı,
    /// bulunamazsa giriş stok hareketlerinin ağırlıklı ortalama maliyeti.
    /// </summary>
    public static decimal? CostPriceOf(ProductDto product)
        => CostPriceOf(product.Prices, product.Movements);

    /// <summary>
    /// Fiyat ve stok hareketleri ayrı verildiğinde maliyet hesabı yapar.
    /// Hareket listesi, fatura ekranında yalnızca ilgili ürün için tembel yüklenir.
    /// </summary>
    public static decimal? CostPriceOf(IReadOnlyCollection<ProductPriceDto> prices, IReadOnlyCollection<ProductMovementDto> movements)
    {
        ProductPriceDto? purchase = prices
            .Where(p => p.PriceType == ProductPriceType.Purchase)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefault();

        if (purchase is { UnitPrice: > 0 })
        {
            return purchase.UnitPrice;
        }

        List<ProductMovementDto> inputs = movements
            .Where(m => m.MovementType == ProductMovementType.Input
                && m.UnitPrice.HasValue
                && m.UnitPrice.Value > 0)
            .ToList();

        decimal totalQuantity = inputs.Sum(m => m.Quantity);
        if (totalQuantity <= 0)
        {
            return null;
        }

        decimal totalCost = inputs.Sum(m => m.Quantity * m.UnitPrice!.Value);
        return totalCost / totalQuantity;
    }

    /// <summary>KDV oranını ondalık kesre çevirir (0,20 ve 20 -> 0,20).</summary>
    public static decimal KdvRateOf(decimal taxRateRate)
        => taxRateRate > 0 && taxRateRate <= 1 ? taxRateRate : taxRateRate / 100m;

    /// <summary>
    /// Önerilen satış fiyatı: maliyet + KDV -> %10 kâr -> tekrar KDV.
    /// Virgülden sonra 2 hane gelecek şekilde yukarı yuvarlanır.
    /// </summary>
    public static decimal SuggestedPriceOf(decimal costPrice, decimal kdvRate)
    {
        decimal withKdv = costPrice * (1m + kdvRate);
        decimal withProfit = withKdv * 1.10m;
        decimal suggested = withProfit * (1m + kdvRate);

        return Math.Ceiling(suggested * 100m) / 100m;
    }
}