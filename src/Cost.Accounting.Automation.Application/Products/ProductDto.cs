using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.Products;

public sealed class ProductPriceDto
{
    public Guid Id { get; set; }

    public ProductPriceType PriceType { get; set; }

    [Column("Fiyat Türü", Order = 10, Width = 90, Alignment = "Center",
        Tip = "Alış: ürünün maliyet/almış fiyatı. Satış: müşteriye uygulanan fiyat.")]
    public string PriceTypeName
    {
        get => PriceType == ProductPriceType.Sale ? "Satış" : "Alış";
        set => PriceType = value == "Alış" ? ProductPriceType.Purchase : ProductPriceType.Sale;
    }

    [Column("Birim Fiyat", Order = 20, Width = 110, Format = "n2", Alignment = "Right",
        Tip = "Fiyat tutarı (Kdv hariç).")]
    public decimal UnitPrice { get; set; }

    [Column("Fiyat Başlangıç", Order = 30, Width = 120, Format = "dd.MM.yyyy", Alignment = "Center",
        Tip = "Bu fiyatın geçerli olmaya başladığı tarih. Fiyat listelerinde ve stok kartında bu tarihten sonraki belgelere bu fiyat uygulanır.")]
    public DateOnly StartDate { get; set; }

    [Column("Fiyat Bitiş", Order = 40, Width = 120, Format = "dd.MM.yyyy", Alignment = "Center",
        Tip = "Bu fiyatın geçerliliğinin bittiği tarih. Boş bırakılırsa fiyat süresiz olarak geçerli sayılır.")]
    public DateOnly? EndDate { get; set; }
}

public sealed class ProductMovementDto
{
    [Column("Tarih", Order = 10, Width = 90, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly Date { get; set; }

    [Column("Hareket Türü", Order = 20, Width = 80, Alignment = "Center")]
    public ProductMovementType MovementType { get; set; }

    [Column("Miktar", Order = 30, Width = 90, Format = "n2", Alignment = "Right")]
    public decimal Quantity { get; set; }

    [Column("Birim Fiyat", Order = 40, Width = 100, Format = "n2", Alignment = "Right")]
    public decimal? UnitPrice { get; set; }

    [Column("Belge / Ref No", Order = 50, Width = 110)]
    public string? ReferenceNo { get; set; }

    [Column("Açıklama", Order = 60, Width = 180)]
    public string Description { get; set; } = default!;

    [Column("Id", IsVisible = false)]
    public Guid Id { get; set; }
}

public sealed class ProductImageDto
{
    public Guid Id { get; set; }
    public string Path { get; set; } = default!;
    public bool IsPrimary { get; set; }
}

public sealed class ProductDto : EntityDto
{
    [Column("Ürün Adı", Order = 10, Width = 220)]
    public string Name { get; set; } = default!;

    [Column("Ürün Kodu", Order = 85, Width = 100, Alignment = "Right")]
    public string ProductCode { get; set; } = default!;

    [Column("Barkod", IsVisible = false)]
    public string? Barcode { get; set; }

    [Column("QR Kod", IsVisible = false)]
    public string? QRCode { get; set; }

    [Column("Min. Stok Seviyesi", Order = 70, Width = 100, Format = "n2", Alignment = "Right")]
    public decimal? MinimumProductLevel { get; set; }

    [Column("KDV Oranı Id", IsVisible = false)]
    public Guid TaxRateId { get; set; }

    [Column("KDV", Order = 60, Width = 70, Format = "p0", Alignment = "Right")]
    public decimal TaxRateRate { get; set; }

    [Column("KDV Adı", IsVisible = false)]
    public string TaxRateName { get; set; } = default!;

    [Column("Depo Id", IsVisible = false)]
    public Guid WarehouseId { get; set; }

    [Column("Depo Kodu", IsVisible = false)]
    public string WarehouseCode { get; set; } = default!;

    [Column("Depo", Order = 40, Width = 120)]
    public string WarehouseName { get; set; } = default!;

    [Column("Kategori Id", IsVisible = false)]
    public Guid CategoryId { get; set; }

    [Column("Kategori Kodu", IsVisible = false)]
    public string CategoryCode { get; set; } = default!;

    [Column("Kategori", Order = 30, Width = 130)]
    public string CategoryName { get; set; } = default!;

    [Column("Birim Cinsi Id", IsVisible = false)]
    public Guid ProductUnitTypeId { get; set; }

    [Column("Birim", Order = 50, Width = 70, Alignment = "Center")]
    public string ProductUnitTypeName { get; set; } = default!;

    [Column("Hesap Planı Id", IsVisible = false)]
    public Guid? ChartOfAccountId { get; set; }

    [Column("Hesap Planı Kodu", Order = 80, Width = 120)]
    public string? ChartOfAccountCode { get; set; }

    [Column("Yarımamül Ürün Id", IsVisible = false)]
    public Guid? SemiFinishedProductId { get; set; }

    /// <summary>
    /// Bu ürüne bağlı bir YARIMAMÜL kartı var mı? Düzenleme ekranında eksik yarımamül
    /// kartının açılıp açılamayacağını belirlemek için kullanılır.
    /// </summary>
    public bool HasSemiFinishedPair { get; set; }

    [Column("Yarımamül Karşılığı", Order = 25, Width = 220)]
    public string? SemiFinishedProductName
    {
        get => _semiFinishedProductName;
        set => _semiFinishedProductName = value;
    }
    private string? _semiFinishedProductName;

    [Column("Açıklama", IsVisible = false)]
    public string Description { get; set; } = default!;

    [Column("Stok", Order = 75, Width = 90, Format = "n2", Alignment = "Right")]
    public decimal StockQuantity { get; set; }

    public List<ProductPriceDto> Prices { get; set; } = [];
    public List<ProductMovementDto> Movements { get; set; } = [];
    public List<ProductImageDto> Images { get; set; } = [];
}

public static class ProductExtensions
{
    /// <summary>
    /// Ürün LISTESİ için projeksiyon (veritabanında çalışır).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Liste ekranı satır başına yalnızca özet alanlar gösterir: stok
    /// miktarı bir <b>toplam</b>dır, fiyat ve hareket listeleri gösterilmez.
    /// Bu yüzden projeksiyon <c>Movements</c> ve <c>Prices</c> koleksiyonlarını
    /// materyalize etmez.
    /// </para>
    /// <para>
    /// Daha önce liste sorgusu her ürün için tüm hareket geçmişini ve tüm
    /// fiyat kayıtlarını belleğe alıyordu. Taşınan veri ürün sayısıyla değil,
    /// o ürüne bağlı <b>hareket sayısıyla</b> çarpılıyordu: yirmi ürün ve on bin
    /// hareket, ürün listesini açmak için on bin satır okunması anlamına
    /// geliyordu.
    /// </para>
    /// <para>
    /// <b>Stok miktarı burada hesaplanmaz.</b> Atölye transferi girişinin
    /// elenmesi kuralı bir C# yardımcı metoduna dayanır ve projeksiyon içinde
    /// SQL'e çevrilemez. Bunun yerine <c>ProductGetAllQuery</c> tek bir
    /// toplu <c>GROUP BY</c> sorgusuyla stokları getirip <see cref="StockQuantity"/>
    /// alanını doldurur; hareket satırlarının hiçbiri belleğe gelmez.
    /// </para>
    /// </remarks>
    public static IQueryable<ProductDto> MapTo(this IQueryable<Product> entity)
    {
        return entity
            .Select(s => new ProductDto
            {
                Id = s.Id,
                Name = s.Name.Value,
                ProductCode = s.ProductCode.Value,
                Barcode = s.Barcode.Value,
                QRCode = s.QRCode.Value,
                MinimumProductLevel = s.MinimumProductLevel,
                TaxRateId = s.TaxRateId,
                TaxRateName = s.TaxRate == null ? string.Empty : s.TaxRate.Name.Value,
                TaxRateRate = s.TaxRate == null ? 0m : s.TaxRate.Rate,

                WarehouseId = s.WarehouseId,
                WarehouseCode = s.Warehouse == null ? string.Empty : s.Warehouse.Code.Value,
                WarehouseName = s.Warehouse == null ? string.Empty : s.Warehouse.Name.Value,

                CategoryId = s.CategoryId,
                CategoryCode = s.Category == null ? string.Empty : s.Category.Code.Value,
                CategoryName = s.Category == null ? string.Empty : s.Category.Name.Value,

                ProductUnitTypeId = s.ProductUnitTypeId,
                ProductUnitTypeName = s.ProductUnitType == null ? string.Empty : s.ProductUnitType.Name.Value,

                ChartOfAccountId = s.ChartOfAccountId == null ? null : s.ChartOfAccountId.Value,
                ChartOfAccountCode = s.ChartOfAccount == null ? null : s.ChartOfAccount.Code.Value,
                SemiFinishedProductId = s.SemiFinishedProductId == null ? null : s.SemiFinishedProductId.Value,
                SemiFinishedProductName = s.SemiFinishedProduct == null ? null : s.SemiFinishedProduct.Name.Value,
                Description = s.Description.Value,

                Images = s.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    Path = i.Path,
                    IsPrimary = i.IsDefault
                }).ToList(),

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value
            });
    }

    /// <summary>
    /// Ürün DETAYI için projeksiyon: fiyat geçmişi ve hareket listesi de
    /// materyalize edilir. Yalnızca tek ürünün açıldığı ekranlarda kullanılır.
    /// </summary>
    public static ProductDto MapToDetail(this Product product) => new()
    {
        Id = product.Id,
        Name = product.Name.Value,
        ProductCode = product.ProductCode.Value,
        Barcode = product.Barcode.Value,
        QRCode = product.QRCode.Value,
        MinimumProductLevel = product.MinimumProductLevel,
        TaxRateId = product.TaxRateId,
        TaxRateName = product.TaxRate?.Name.Value ?? string.Empty,
        TaxRateRate = product.TaxRate?.Rate ?? 0m,

        WarehouseId = product.WarehouseId,
        WarehouseCode = product.Warehouse?.Code.Value ?? string.Empty,
        WarehouseName = product.Warehouse?.Name.Value ?? string.Empty,

        CategoryId = product.CategoryId,
        CategoryCode = product.Category?.Code.Value ?? string.Empty,
        CategoryName = product.Category?.Name.Value ?? string.Empty,

        ProductUnitTypeId = product.ProductUnitTypeId,
        ProductUnitTypeName = product.ProductUnitType?.Name.Value ?? string.Empty,

        ChartOfAccountId = product.ChartOfAccountId == null ? null : product.ChartOfAccountId.Value,
        ChartOfAccountCode = product.ChartOfAccount?.Code.Value,
        SemiFinishedProductId = product.SemiFinishedProductId == null ? null : product.SemiFinishedProductId.Value,
        SemiFinishedProductName = product.SemiFinishedProduct?.Name.Value,
        Description = product.Description.Value,

        StockQuantity = product.Movements
            .Where(m => ProductStockBalanceHelper.CountsAsProductStock(m))
            .Sum(m => m.MovementType == ProductMovementType.Input ? m.Quantity : -m.Quantity),

        Prices = [.. product.Prices.Select(p => new ProductPriceDto
        {
            Id = p.Id,
            PriceType = p.PriceType,
            UnitPrice = p.UnitPrice.Value,
            StartDate = p.StartDate,
            EndDate = p.EndDate
        })],

        Movements = [.. product.Movements.Select(m => new ProductMovementDto
        {
            Id = m.Id,
            MovementType = m.MovementType,
            Quantity = m.Quantity,
            UnitPrice = m.UnitPrice?.Value,
            Date = m.Date,
            ReferenceNo = m.ReferenceNo,
            Description = m.Description.Value
        })],

        Images = [.. product.Images.Select(i => new ProductImageDto
        {
            Id = i.Id,
            Path = i.Path,
            IsPrimary = i.IsDefault
        })],

        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt,
        CreatedBy = product.CreatedBy,
        UpdatedAt = product.UpdatedAt,
        UpdatedBy = product.UpdatedBy == null ? null : product.UpdatedBy.Value
    };
}