using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.StockIssues;

/// <summary>
/// Atölye transferi tek bir olayda iki hareket üretir: depodan çıkış (-)
/// ve "Atölye Transferi Girişi" (+). Maliyet pusulası onayı ise atölyede
/// tüketilen malzemeler için "Maliyet Pusulası Tüketimi" (-) çıkışı üretir.
/// İkisi de atölye stokunu temsil eder ve ürün/depo stok raporlarına girerse
/// depo bakiyesi bozulur:
///   - Atölye girişi (+) depo bakiyesine girerse transfer edilen miktar
///     depodan hiç düşmemiş gibi görünür.
///   - Atölye tüketimi (-) depo bakiyesine girerse tüketim depodan yapılmış
///     gibi görünür.
/// Bu yüzden ürün bazlı (depo) bakiye hesaplarında bu iki hareket hariç
/// tutulur; çıkış/transfer bacakları depodan düşülmeye devam eder.
/// Atölye tarafı için ise bakiye = atölye girişleri (+) - atölye tüketimleri (-).
/// </summary>
public static class ProductStockBalanceHelper
{
    public const string AtelierTransferInputDescriptionPrefix = "Atölye Transferi Girişi - ";
    public const string CostSlipConsumptionOutputDescriptionPrefix = "Maliyet Pusulası Tüketimi - ";
    public const string ProductionInputDescriptionPrefix = "Maliyet Pusulası Girişi - ";

    public static IQueryable<ProductMovement> WhereCountsAsProductStock(this IQueryable<ProductMovement> movements)
    {
        return movements.Where(m =>
            !(m.MovementType == ProductMovementType.Input
              && m.Description.Value != null
              && m.Description.Value.StartsWith(AtelierTransferInputDescriptionPrefix))
            && !(m.MovementType == ProductMovementType.Output
              && m.Description.Value != null
              && m.Description.Value.StartsWith(CostSlipConsumptionOutputDescriptionPrefix)));
    }

    public static bool CountsAsProductStock(ProductMovement movement)
    {
        return !IsAtelierTransferInput(movement) && !IsCostSlipConsumptionOutput(movement);
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