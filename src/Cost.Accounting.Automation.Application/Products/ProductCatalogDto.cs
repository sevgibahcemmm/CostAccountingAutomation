using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Ürün listeleme/katalog ekranları için hafif projeksiyon. <see cref="ProductDto"/>'nun aksine
/// hareket (Movements) ve görsel (Images) verisini içermez; böylece yüzlerce ürünün tüm stok
/// hareketleri her liste açılışında veritabanından çekilmez.
/// </summary>
public sealed class ProductCatalogDto : EntityDto
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

    [Column("Depo", Order = 40, Width = 120)]
    public string WarehouseGroup { get; set; } = default!;

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

    [Column("Yarımamül Karşılığı", Order = 25, Width = 220)]
    public string? SemiFinishedProductName { get; set; }

    [Column("Açıklama", IsVisible = false)]
    public string Description { get; set; } = default!;

    [Column("Stok", Order = 75, Width = 90, Format = "n2", Alignment = "Right")]
    public decimal StockQuantity { get; set; }

    public List<ProductPriceDto> Prices { get; set; } = [];
}

public static class ProductCatalogExtensions
{
    public static IQueryable<ProductCatalogDto> MapToCatalog(this IQueryable<EntityWithAuditDto<Product>> entity)
    {
        return entity
            .Select(s => new ProductCatalogDto
            {
                Id = s.Entity.Id,
                Name = s.Entity.Name.Value,
                ProductCode = s.Entity.ProductCode.Value,
                Barcode = s.Entity.Barcode.Value,
                QRCode = s.Entity.QRCode.Value,
                MinimumProductLevel = s.Entity.MinimumProductLevel,
                TaxRateId = s.Entity.TaxRateId,
                TaxRateName = s.Entity.TaxRate!.Name.Value,
                TaxRateRate = s.Entity.TaxRate!.Rate,

                WarehouseId = s.Entity.WarehouseId,
                WarehouseCode = s.Entity.Warehouse!.Code.Value,
                WarehouseName = s.Entity.Warehouse.Name.Value,
                WarehouseGroup = $"{s.Entity.Warehouse.Code.Value} - {s.Entity.Warehouse.Name.Value}",

                CategoryId = s.Entity.CategoryId,
                CategoryCode = s.Entity.Category!.Code.Value,
                CategoryName = s.Entity.Category.Name.Value,

                ProductUnitTypeId = s.Entity.ProductUnitTypeId,
                ProductUnitTypeName = s.Entity.ProductUnitType!.Name.Value,

                ChartOfAccountId = s.Entity.ChartOfAccountId == null ? null : s.Entity.ChartOfAccountId.Value,
                ChartOfAccountCode = s.Entity.ChartOfAccount == null ? null : s.Entity.ChartOfAccount.Code.Value,
                SemiFinishedProductId = s.Entity.SemiFinishedProductId == null ? null : s.Entity.SemiFinishedProductId.Value,
                SemiFinishedProductName = s.Entity.SemiFinishedProduct == null ? null : s.Entity.SemiFinishedProduct.Name.Value,
                Description = s.Entity.Description.Value,

                StockQuantity = 0,

                Prices = new List<ProductPriceDto>(),
                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy,
                IsActive = s.Entity.IsActive,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy == null ? null : s.Entity.UpdatedBy.Value,
                CreatedFullName = s.CreatedUser.FullName.Value,
                UpdatedFullName = s.UpdatedUser == null ? null : s.UpdatedUser.FullName.Value
            })
            .AsQueryable();
    }
}