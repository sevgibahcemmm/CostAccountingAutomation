using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.StockIssues;

/// <summary>
/// Ürün bazlı (depo) bakiye, tüm hareketlerin netiyle hesaplanır:
/// bakiye = tüm girişler (+) - tüm çıkışlar (-).
/// Atölye transferi tek olayda "Atölye Transferi" çıkışı (-) ve
/// "Atölye Transferi Girişi" (+) ikilisi üretir; bu ikisi birlikte net
/// sıfır etki yapar, dolayısıyla ayrıca hariç tutulmaz. Rafiye pusulası
/// onayı da tüketim için "Maliyet Pusulası Tüketimi" (-) çıkışı, üretim
/// için "Maliyet Pusulası Girişi" (+) girişi üretir; her ikisi de gerçek
/// stok değişimidir ve bakiyeye dahil edilir. Böylece örneğin üretilip
/// tüketilen bir yarımamülün net bakiyesi sıfır olur.
/// </summary>
public static class ProductStockBalanceHelper
{
    public const string AtelierTransferInputDescriptionPrefix = "Atölye Transferi Girişi - ";
    public const string CostSlipConsumptionOutputDescriptionPrefix = "Maliyet Pusulası Tüketimi - ";
    public const string ProductionInputDescriptionPrefix = "Maliyet Pusulası Girişi - ";

    public static IQueryable<ProductMovement> WhereCountsAsProductStock(this IQueryable<ProductMovement> movements)
    {
        return movements;
    }

    public static bool CountsAsProductStock(ProductMovement movement)
    {
        return true;
    }

    public static bool IsAtelierTransferInput(ProductMovement movement)
    {
        return movement.MovementType == ProductMovementType.Input
               && movement.Description.Value != null
               && movement.Description.Value.StartsWith(AtelierTransferInputDescriptionPrefix);
    }

    public static bool IsCostSlipConsumptionOutput(ProductMovement movement)
    {
        return movement.MovementType == ProductMovementType.Output
               && movement.Description.Value != null
               && movement.Description.Value.StartsWith(CostSlipConsumptionOutputDescriptionPrefix);
    }
}