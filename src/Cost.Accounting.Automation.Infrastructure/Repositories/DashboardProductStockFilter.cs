using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

/// <summary>
/// Dashboard'da maliyet tablosunun hangi satırları göstereceğine dair TEK kural
/// kaynağıdır. Böylece "stok hareketleri" ve "maliyet tablosu" aynı ölçütü
/// kullanır ve birbirinden ayrışmaz.
/// </summary>
internal static class DashboardProductStockFilter
{
    /// <summary>"Son kaydedilenler" penceresi (gün).</summary>
    public const int RecentDays = 7;

    /// <summary>
    /// Kritik seviyeye YAKLAŞAN ürün katsayısı. Minimum seviye 5 ise eşik
    /// 2 x 5 = 10 olur; 10'un altındaki ürünler listelenir.
    /// </summary>
    public const decimal ApproachingFactor = 2m;

    /// <summary>
    /// Bir ürün maliyet tablosunda görünmeli mi?
    /// </summary>
    /// <param name="hasRecentMovement">Son <see cref="RecentDays"/> günde hareket görüldü mü.</param>
    /// <param name="minimumLevel">Ürünün kritik stok seviyesi (null ise tanımlı değil).</param>
    /// <param name="balanceQuantity">Ürünün mevcut stok miktarı.</param>
    /// <param name="warehouseCode">Depo kodu; yarı mamül depoları hariç tutulur.</param>
    public static bool ShouldInclude(
        bool hasRecentMovement,
        decimal? minimumLevel,
        decimal balanceQuantity,
        string? warehouseCode)
    {
        // Son haftada harejet gorulmus urunler her zaman gosterilir; kullanici
        // en son kaydettiklerini aninda gormek istiyor.
        if (hasRecentMovement)
        {
            return true;
        }

        // Yarı mamül depoları (151 / 151.xx) maliyet tablosunda gosterilmez;
        // onlar ara üretimdir, nihai stok kalemi degildir.
        if (IsSemiFinishedWarehouse(warehouseCode))
        {
            return false;
        }

        // Stok hic yoksa her zaman kritiktir.
        if (balanceQuantity <= 0m)
        {
            return true;
        }

        // Kritik seviyesi tanimli degilse kural uygulanamaz; urun ancak hareket
        // ile listelenir.
        if (minimumLevel is null)
        {
            return false;
        }

        return balanceQuantity <= minimumLevel.Value * ApproachingFactor;
    }

    private static bool IsSemiFinishedWarehouse(string? warehouseCode)
        => warehouseCode is not null
            && (warehouseCode.StartsWith("151", StringComparison.Ordinal)
                || warehouseCode.StartsWith("152", StringComparison.Ordinal));

    /// <summary>Depo kodu yarı mamül deposuna (151/152) ait mi?</summary>
    public static bool IsSemiFinishedWarehousePublic(string? warehouseCode)
        => IsSemiFinishedWarehouse(warehouseCode);
}