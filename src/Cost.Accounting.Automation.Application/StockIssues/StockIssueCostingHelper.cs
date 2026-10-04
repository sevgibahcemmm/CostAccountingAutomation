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

        // FIFO "ilk giren ilk çıkar" olduğu için sıralama KRONOLOJİK olmalıdır.
        //
        // <para>
        // Önceden yalnızca Date'e göre sıralanıyor, eşit tarihli hareketlerde
        // kırıcı olarak <c>Guid</c> karşılaştırması kullanılıyordu. Guid bir
        // zaman sırası taşımaz; .NET'in Guid karşılaştırması ilk dört baytı
        // KÜÇÜK uçlu bir Int32 olarak yorumlar ve UUIDv7'nin büyük uçlu zaman
        // damgasını doğru sıraya çevirmez. Aynı tarihli iki girişte (ör. 144
        // @ 12,60 ve 500 @ 13,20) bu yüzden katman sırası yer değiştiriyor ve
        // FIFO yanlış katmandan başlıyordu.
        // </para>
        //
        // <para>
        // Kırıcı olarak önce <see cref="Entity.CreatedAt"/> (fiilen kaydedildiği
        // an), sonra <see cref="Entity.Id"/> kullanılır. Böylece aynı tarihte
        // giren iki farklı fiyatlı alım, gerçekten önce kaydedilen fiyattan
        // başlayarak tüketilir.
        // </para>
        return movements
            .OrderBy(m => m.Date)
            .ThenBy(m => m.CreatedAt)
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

    /// <summary>
    /// Bir satırın gösterilecek birim maliyetini üretir: <b>FIFO'da ilk
    /// (en eski) giriş katmanının fiyatı.</b>
    ///
    /// <para>
    /// Burada asla ORTALAMA hesaplanmaz. 144 @ 12,60 ve 500 @ 13,20 girişleri
    /// varken 200 adetlik bir çıkışın ortalaması 12,77 çıkıyor ve bu değer
    /// belgede "Birim Maliyet" olarak görünüyordu. Oysa gerçekte 144 adet
    /// 12,60'dan, 56 adet 13,20'den çıkmalıdır: 12,77 hiçbir alım faturasında
    /// yoktur ve stok hareketi raporunda girişi olmayan bir satır olurdu.
    /// </para>
    ///
    /// <para>
    /// Satırın gerçek, katman katman çıkış fiyatları <b>ProductMovement</b>
    /// kayıtlarıdır; her katman için ayrı bir çıkış hareketi yazılır. Bu
    /// değer yalnızca belge satırının görünen fiyatidir.
    /// </para>
    /// </summary>
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

            List<CostingLayer> layers = BuildRemainingLayers(
                movements, requested.Key, costingMethod, asOfDate);

            if (layers.Count == 0)
            {
                continue;
            }

            // FIFO'da en eski katman; LIFO'da en yeni katman. Tüketim
            // belgelerinde daima FIFO uygulanır.
            CostingLayer leading = costingMethod == StockCostingMethod.Fifo
                ? layers[0]
                : layers[^1];

            if (leading.Quantity <= 0m)
            {
                continue;
            }

            result[requested.Key] = leading.UnitPrice;
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
    /// <summary>
    /// FIFO/LIFO sırasına göre bir ürünün tüketilmemiş giriş katmanlarını
    /// döner.
    ///
    /// <para>
    /// Katman sırası, giriş hareketlerinin kronolojik sırasıdır (tarih, sonra
    /// kaydedilme anı, sonra id) ve <b>bu metot içinde</b> uygulanır; çağıranın
    /// hareketleri hangi sırada getirdiği sonucu değiştirmez. FIFO ilk
    /// katmandan, LIFO son katmandan başlar. Daha önce yazılmış çıkışlar önceden
    /// tüketilmiş sayılır, yalnızca kalan katmanlar döner. Gelecek tarihli
    /// girişler (<paramref name="asOfDate"/> sonrası) mevcut stoka dâhil edilmez.
    /// </para>
    /// </summary>
    public static List<CostingLayer> BuildRemainingLayers(
        List<ProductMovement> movements,
        IdentityId productId,
        StockCostingMethod costingMethod,
        DateOnly asOfDate)
    {
        List<(decimal Quantity, decimal UnitPrice)> layers = [];

        // SIRALAMA DEĞİŞMEZİ BURADA, KATMAN OLUŞTURUCUDA SAĞLANIR.
        // Sıralamayı yalnızca LoadMovementsAsync'ye bırakmak kırılgandı: bu
        // metot çağıran tarafından ne sırayla gelirse gelsin FIFO'nun
        // "ilk giren ilk çıkar" anlamını korumalıdır. Veritabanı bir
        // uniqueidentifier'ı SQL'de HAM BAYT sırasıyla sıalar
        // (01A105D5 < 01A10596), yani sıralama uygulanmazsa 500 @ 13,20'li
        // giriş 144 @ 12,60'lı girişten ÖNCE gelir ve FIFO sessizce en yeni
        // fiyattan başlar.
        foreach (ProductMovement input in movements
                     .Where(m => m.ProductId == productId
                         && m.MovementType == ProductMovementType.Input
                         && m.Date <= asOfDate)
                     .OrderBy(m => m.Date)
                     .ThenBy(m => m.CreatedAt)
                     .ThenBy(m => m.Id.Value))
        {
            // Fiyatı OLMAYAN girişler de katman sayılır (birim maliyet 0).
            //
            // <para>
            // Daha önce bu girişler katman listesine hiç alınmıyordu. Böylece
            // katman toplamı, miktar kontrolünü yapan
            // <see cref="ComputeAvailableQuantity"/> sonucundan KÜÇÜK kalıyordu:
            // miktar kontrolü "yeterli stok var" deyip geçerken fiyat
            // kırılımı miktarı karşılayamıyor, kalan miktar fiyatı 0 olan
            // uydurma bir katmanla dolduruluyordu. Fiyatı 0 olan bu çıkış,
            // fiyat bazlı gruplayan stok raporunda girişi olmayan ve bakiyesi
            // EKSİ olan ayrı bir satır olarak görünüyordu. Katman ile miktar
            // kontrolü artık birebir tutarlıdır.
            // </para>
            layers.Add((input.Quantity, input.UnitPrice?.Value ?? 0m));
        }

        ConsumeLayers(
            layers,
            movements
                .Where(m => m.ProductId == productId
                    && m.MovementType == ProductMovementType.Output
                    && m.Date <= asOfDate)
                .Sum(m => m.Quantity),
            costingMethod);

        return layers.Select(l => new CostingLayer(l.Quantity, l.UnitPrice)).ToList();
    }

    /// <summary>
    /// <see cref="BuildRemainingLayers"/> üzerinden miktar bazlı katman kırılımı.
    /// </summary>
    public static List<(decimal Quantity, decimal UnitPrice)> BuildConsumptionLayers(
        List<ProductMovement> movements,
        IdentityId productId,
        decimal quantity,
        StockCostingMethod costingMethod,
        DateOnly asOfDate)
    {
        List<CostingLayer> layers = BuildRemainingLayers(
            movements, productId, costingMethod, asOfDate);

        return TakeLayers(layers, productId, quantity, costingMethod)
            .Select(layer => (layer.Quantity, layer.UnitPrice))
            .ToList();
    }

    /// <summary>
    /// <see cref="BuildRemainingLayers"/> üzerinden <b>tutar</b> bazlı katman
    /// kırılımı.
    ///
    /// <para>
    /// Kullanıcı bir maliyet tutarı girip karşılığında ne kadar miktarın
    /// tüketileceğini sorar. Bu miktar ORTALAMA birim maliyetten bölünerek
    /// bulunamaz: ortalama, hangi girişten ne kadar alınacağını bilmez. Bu
    /// yüzden kırılım tutar üzerinden yürütülür — ilk giriş tamamen karşılanır,
    /// kalan tutar sonraki girişten alınır.
    /// </para>
    /// </summary>
    public static List<CostingLayer> BuildConsumptionLayersForAmount(
        List<ProductMovement> movements,
        IdentityId productId,
        decimal amount,
        StockCostingMethod costingMethod,
        DateOnly asOfDate)
        => StockCostingLayers.TakeByAmount(
            BuildRemainingLayers(movements, productId, costingMethod, asOfDate),
            amount,
            costingMethod);

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
    private static List<CostingLayer> TakeLayers(
        List<CostingLayer> layers,
        IdentityId productId,
        decimal quantity,
        StockCostingMethod method)
    {
        List<CostingLayer> taken = [];
        decimal remaining = quantity;

        while (remaining > 0 && layers.Count > 0)
        {
            int index = method == StockCostingMethod.Fifo ? 0 : layers.Count - 1;
            CostingLayer layer = layers[index];
            decimal consumed = Math.Min(layer.Quantity, remaining);

            taken.Add(new CostingLayer(consumed, layer.UnitPrice));
            remaining -= consumed;

            if (layer.Quantity - consumed <= 0)
            {
                layers.RemoveAt(index);
            }
            else
            {
                layers[index] = layer with { Quantity = layer.Quantity - consumed };
            }
        }

        // KATMAN BÜTÜNLÜĞÜ: Çağıranlar tüketilecek miktarı
        // ComputeAvailableQuantity ile ÖNCEDEN doğrular. BuildRemainingLayers
        // artık miktar kontrolüyle birebir aynı giriş kümesini kullandığı için
        // kalan katmanların toplamı istenen miktarı karşılar.
        //
        // Buraya ulaşmak bir programlama hatasıdır (ör. eldeki veri bozuk:
        // çıkışlar girişleri aşıyor). Karşılanamayan miktarı eskiden fiyatı 0
        // olan bir katmanla doldurmak, girişi olmayan ve bakiyesi eksi bir
        // rapor satırı üretiyordu; sessizce yanlış veri yazmak yerine sesli
        // hata verilir.
        if (remaining > 0)
        {
            throw new InvalidOperationException(
                $"'{productId.Value}' ürünü için giriş katmanları tüketilecek miktarı karşılamıyor. Eksik miktar: {remaining:n4}.");
        }

        return taken;
    }
}
