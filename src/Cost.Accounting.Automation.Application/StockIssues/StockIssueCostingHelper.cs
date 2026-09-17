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

        foreach (IGrouping<IdentityId, ProductMovement> product in movements.GroupBy(m => m.ProductId))
        {
            decimal quantity = quantities.TryGetValue(product.Key, out decimal q) ? q : 0m;
            if (quantity <= 0)
            {
                continue;
            }

            List<(decimal Quantity, decimal UnitPrice)> layers = [];

            foreach (ProductMovement input in product.Where(m =>
                         m.MovementType == ProductMovementType.Input && m.Date <= asOfDate))
            {
                if (input.UnitPrice is { } price)
                {
                    layers.Add((input.Quantity, price.Value));
                }
            }

            decimal priorOutputs = product
                .Where(m => m.MovementType == ProductMovementType.Output && m.Date <= asOfDate)
                .Sum(m => m.Quantity);

            ConsumeLayers(layers, priorOutputs, costingMethod);

            decimal totalCost = ConsumeLayerCost(layers, quantity, costingMethod);
            result[product.Key] = totalCost / quantity;
        }

        return result;
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

    private static decimal ConsumeLayerCost(
        List<(decimal Quantity, decimal UnitPrice)> layers,
        decimal quantity,
        StockCostingMethod method)
    {
        decimal remaining = quantity;
        decimal totalCost = 0m;

        while (remaining > 0 && layers.Count > 0)
        {
            int index = method == StockCostingMethod.Fifo ? 0 : layers.Count - 1;
            (decimal layerQuantity, decimal unitPrice) = layers[index];
            decimal consumed = Math.Min(layerQuantity, remaining);

            totalCost += consumed * unitPrice;
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

        return totalCost;
    }
}
