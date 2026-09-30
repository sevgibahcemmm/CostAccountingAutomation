using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.StockIssues;

/// <summary>
/// Ürün bazlı (depo) stok, ürünün gerçek giriş/çıkış hareketleriyle hesaplanır:
/// stok = girişler (+) - çıkışlar (-).
///
/// Atölye transferi tek olayda iki hareket üretir: depodan "Atölye Transferi"
/// çıkışı (-) ve atölyeye "Atölye Transferi Girişi" (+) hareketi. Bunlar
/// ürünün mülkiyetini değiştirir, miktarı yaratmaz; ürün artık atölyede
/// (150.55) olduğu için ürün bazlı stoktan düşmelidir. Bu yüzden atölye
/// transferi girişi ürün stok toplamlarına HİÇ dâhil edilmez; yalnızca
/// çıkışı sayılır. Aksi halde 500 alınan, 20 transfer edilen üründe
/// "Toplam Giren" 520, kalan 500 görünürdü; doğrusu Giren 500 / Çıkan 20 /
/// Kalan 480'dir.
///
/// Maliyet pusulası hareketleri (tüketim çıkışı, üretim girişi) gerçek stok
/// değişimi olduğu için bakiyeye dâhil edilir.
/// </summary>
public static class ProductStockBalanceHelper
{
    public const string AtelierTransferInputDescriptionPrefix = "Atölye Transferi Girişi - ";
    public const string CostSlipConsumptionOutputDescriptionPrefix = "Maliyet Pusulası Tüketimi - ";
    public const string ProductionInputDescriptionPrefix = "Maliyet Pusulası Girişi - ";

    /// <summary>
    /// Ürün stok toplamlarına dâhil edilebilir hareketleri filtreler.
    /// Atölye transferi girişi hariç tutulur; çıkışı korunur.
    /// </summary>
    public static IQueryable<ProductMovement> WhereCountsAsProductStock(this IQueryable<ProductMovement> movements)
    {
        return movements.Where(m =>
            m.MovementType != ProductMovementType.Input
            || m.Description.Value == null
            || !m.Description.Value.StartsWith(AtelierTransferInputDescriptionPrefix));
    }

    public static bool CountsAsProductStock(ProductMovement movement)
        => !IsAtelierTransferInput(movement);

    /// <summary>Açıklama metnine göre atölye transferi girişini tanır (bellek içi eşleme için).</summary>
    public static bool IsAtelierTransferInputByDescription(string? description)
        => !string.IsNullOrEmpty(description)
           && description.StartsWith(AtelierTransferInputDescriptionPrefix, StringComparison.Ordinal);

    public static bool IsAtelierTransferInput(ProductMovement movement)
    {
        return movement.MovementType == ProductMovementType.Input
               && IsAtelierTransferInputByDescription(movement.Description?.Value);
    }

    public static bool IsCostSlipConsumptionOutput(ProductMovement movement)
    {
        return movement.MovementType == ProductMovementType.Output
               && movement.Description.Value != null
               && movement.Description.Value.StartsWith(CostSlipConsumptionOutputDescriptionPrefix);
    }
}