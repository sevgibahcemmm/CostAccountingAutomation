using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Application.StockIssues;

internal static class StockIssueCostingHelper
{
    public static async Task<List<ProductMovement>> LoadMovementsAsync(
        IReadOnlyCollection<IdentityId> productIds,
        IProductMovementRepository productMovementRepository,
        CancellationToken cancellationToken)
    {
        HashSet<IdentityId> ids = productIds.ToHashSet();

        List<ProductMovement> movements = await productMovementRepository.GetAll()
            .Where(m => ids.Contains(m.ProductId) && !m.IsDeleted)
            .ToListAsync(cancellationToken);

        return movements
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id.Value)
            .ToList();
    }

    /// <summary>
    /// FIFO/LIFO giriş takibi için bir ürünün belirli bir tarihe kadar
    /// tüketilmemiş giriş (layer) toplamını hesaplar.
    /// Hareketler tarih sırasıyla işlenir; bir çıkış kendinden önceki girişleri
    /// aşamaz. Gelecek tarihli girişler (<paramref name="asOfDate"/> sonrası)
    /// mevcut stoka dahil edilmez.
    /// </summary>
    public static decimal ComputeAvailableQuantity(
        List<ProductMovement> movements,
        IdentityId productId,
        DateOnly asOfDate)
    {
        decimal balance = 0m;

        foreach (ProductMovement movement in movements
                     .Where(m => m.ProductId == productId && m.Date <= asOfDate))
        {
            balance += movement.MovementType == ProductMovementType.Input
                ? movement.Quantity
                : -movement.Quantity;
        }

        return balance;
    }

    public static Dictionary<IdentityId, decimal> BuildUnitCostMap(
        List<ProductMovement> movements,
        IReadOnlyDictionary<IdentityId, decimal> quantities,
        StockCostingMethod costingMethod,
        DateOnly asOfDate)
    {
        Dictionary<IdentityId, decimal> result = [];

        foreach (KeyValuePair<IdentityId, decimal> requested in quantities)
        {
            decimal quantity = requested.Value;

            if (quantity <= 0)
            {
                continue;
            }

            decimal totalCost = BuildConsumptionLayers(
                    movements, requested.Key, quantity, costingMethod, asOfDate)
                .Sum(layer => layer.Quantity * layer.UnitPrice);

            result[requested.Key] = totalCost / quantity;
        }

        return result;
    }

    /// <summary>
    /// FIFO/LIFO sırasına göre bir ürünün tüketilecek miktarını giriş katmanlarına
    /// böler ve her katmandan ne kadar alındığını döner.
    ///
    /// FIFO'da en eski giriş TAMAMEN tüketilir, kalan miktar bir sonraki girişten
    /// alınır; katman bitmeden diğerine geçilmez. LIFO'da sıra ters çevrilir.
    /// Önceki çıkışlar önceden tüketilmiş sayılır, yalnızca kalan katmanlar kullanılır.
    ///
    /// Katman kırılımı döndürüldüğü için çağıran, tüketilen miktarı giriş
    /// fiyatlarıyla eşleşen AYRI çıkış hareketleri olarak yazabilir. Tek bir
    /// ortalama fiyatlı satır yazılsaydı o fiyat hiçbir girişe uymaz ve
    /// fiyat bazlı gruplayan stok raporunda bakiyesi eksi satırlar doğardı.
    /// </summary>
    public static List<(decimal Quantity, decimal UnitPrice)> BuildConsumptionLayers(
        List<ProductMovement> movements,
        IdentityId productId,
        decimal quantity,
        StockCostingMethod costingMethod,
        DateOnly asOfDate)
    {
        // Katman sırası hareket sırasıdır (LoadMovementsAsync tarih, sonra id'ye
        // göre sıralar); FIFO ilk katmandan, LIFO son katmandan başlar.
        List<(decimal Quantity, decimal UnitPrice)> remaining = [];

        foreach (ProductMovement input in movements
                     .Where(m => m.ProductId == productId
                         && m.MovementType == ProductMovementType.Input
                         && m.Date <= asOfDate))
        {
            if (input.UnitPrice is { } price)
            {
                remaining.Add((input.Quantity, price.Value));
            }
        }

        ConsumeLayers(
            remaining,
            movements
                .Where(m => m.ProductId == productId
                    && m.MovementType == ProductMovementType.Output
                    && m.Date <= asOfDate)
                .Sum(m => m.Quantity),
            costingMethod);

        return TakeLayers(remaining, quantity, costingMethod);
    }

    private static void ConsumeLayers(
        List<(decimal Quantity, decimal UnitPrice)> layers,
        decimal quantity,
        StockCostingMethod method)
    {
        decimal remaining = quantity;

        while (remaining > 0 && layers.Count > 0)
        {
            int index = method == StockCostingMethod.Fifo ? 0 : layers.Count - 1;
            (decimal layerQuantity, decimal unitPrice) = layers[index];
            decimal consumed = Math.Min(layerQuantity, remaining);
            remaining -= consumed;

            if (layerQuantity - consumed <= 0)
            {
                layers.RemoveAt(index);
            }
            else
            {
                layers[index] = (layerQuantity - consumed, unitPrice);
            }
        }
    }

    /// <summary>
    /// Katmanlardan <paramref name="quantity"/> kadar alır ve hangi katmandan
    /// ne kadar alındığını SIRAYLA döner. Katman bitmeden diğerine geçilmez.
    /// </summary>
    private static List<(decimal Quantity, decimal UnitPrice)> TakeLayers(
        List<(decimal Quantity, decimal UnitPrice)> layers,
        decimal quantity,
        StockCostingMethod method)
    {
        List<(decimal Quantity, decimal UnitPrice)> taken = [];
        decimal remaining = quantity;

        while (remaining > 0 && layers.Count > 0)
        {
            int index = method == StockCostingMethod.Fifo ? 0 : layers.Count - 1;
            (decimal layerQuantity, decimal unitPrice) = layers[index];
            decimal consumed = Math.Min(layerQuantity, remaining);

            taken.Add((consumed, unitPrice));
            remaining -= consumed;

            if (layerQuantity - consumed <= 0)
            {
                layers.RemoveAt(index);
            }
            else
            {
                layers[index] = (layerQuantity - consumed, unitPrice);
            }
        }

        // Fiyatı olmayan girişler katman listesine hiç girmez. Böyle bir
        // durumda miktarın karşılanamayan kısmı, maliyeti bilinmeyen çıkış
        // olarak sıfır birim maliyetle kaydedilir; miktar ASLA düşürülmez,
        // aksi hâlde stok miktarı eksik düşerdi.
        if (remaining > 0)
        {
            taken.Add((remaining, 0m));
        }

        return taken;
    }
}
