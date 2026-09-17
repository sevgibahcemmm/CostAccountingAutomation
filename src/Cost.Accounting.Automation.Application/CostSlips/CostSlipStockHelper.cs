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
    /// <summary>
    /// Maliyet pusulası onaylandığında stok yan etkilerini üretir:
    /// - Malzeme kalemleri için atölyedeki stoklardan FIFO/LIFO birim maliyetle ÇIKIŞ hareketi.
    /// - Mamul / Yarı mamul için birim maliyet = Genel Toplam / Miktar ile GİRİŞ hareketi.
    /// Cari / yevmiye hareketi üretilmez.
    /// </summary>
    public static async Task<Result<string>> ApplyStockEffectsAsync(
        CostSlip slip,
        StockCostingMethod costingMethod,
        IProductMovementRepository productMovementRepository,
        CancellationToken cancellationToken)
    {
        var materialLines = slip.CostSlipItems
            .Where(i => i.ProductId is not null)
            .ToList();

        List<IdentityId> productIds = materialLines
            .Select(i => i.ProductId!)
            .Distinct()
            .ToList();

        Dictionary<IdentityId, decimal> requestedQuantities = materialLines
            .GroupBy(i => i.ProductId!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        List<ProductMovement> movements = await StockIssueCostingHelper.LoadMovementsAsync(
            productIds,
            productMovementRepository,
            cancellationToken);

        foreach (KeyValuePair<IdentityId, decimal> requested in requestedQuantities)
        {
            decimal available = StockIssueCostingHelper.ComputeAvailableQuantity(
                movements,
                requested.Key,
                slip.CostDate);

            if (requested.Value > available)
            {
                return Result<string>.Failure(
                    $"'{requested.Key.Value}' için bu tarihe kadar yeterli stok yok. Mevcut: {available:n2}, istenen: {requested.Value:n2}.");
            }
        }

        Dictionary<IdentityId, decimal> costMap = StockIssueCostingHelper.BuildUnitCostMap(
            movements,
            requestedQuantities,
            costingMethod,
            slip.CostDate);

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
                description: new Description($"Maliyet Pusulası Tüketimi - {slip.SlipNumber}"));

            await productMovementRepository.AddAsync(output, cancellationToken);
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
                description: new Description($"Maliyet Pusulası Girişi - {slip.SlipNumber} ({slip.SlipNumber})"));

            await productMovementRepository.AddAsync(input, cancellationToken);
        }

        return Result<string>.Succeed(string.Empty);
    }
}