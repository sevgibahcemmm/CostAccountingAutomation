using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.Products;

public sealed class ProductPriceDto
{
    public Guid Id { get; set; }

    public ProductPriceType PriceType { get; set; }

    [Column("Fiyat Türü", Order = 10, Width = 90, Alignment = "Center")]
    public string PriceTypeName
    {
        get => PriceType == ProductPriceType.Sale ? "Satış" : "Alış";
        set => PriceType = value == "Alış" ? ProductPriceType.Purchase : ProductPriceType.Sale;
    }

    [Column("Birim Fiyat", Order = 20, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal UnitPrice { get; set; }

    [Column("Başlangıç", Order = 30, Width = 110, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly StartDate { get; set; }

    [Column("Bitiş", Order = 40, Width = 110, Format = "dd.MM.yyyy", Alignment = "Center")]
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
    public static IQueryable<ProductDto> MapTo(this IQueryable<EntityWithAuditDto<Product>> entity)
    {
        return entity
            .Select(s => new ProductDto
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

                CategoryId = s.Entity.CategoryId,
                CategoryCode = s.Entity.Category!.Code.Value,
                CategoryName = s.Entity.Category.Name.Value,

                ProductUnitTypeId = s.Entity.ProductUnitTypeId,
                ProductUnitTypeName = s.Entity.ProductUnitType!.Name.Value,

                ChartOfAccountId = s.Entity.ChartOfAccountId == null ? null : s.Entity.ChartOfAccountId.Value,
                Description = s.Entity.Description.Value,

                StockQuantity = s.Entity.Movements.Sum(m => m.MovementType == ProductMovementType.Input ? m.Quantity : -m.Quantity),

                Images = s.Entity.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    Path = i.Path,
                    IsPrimary = i.IsDefault
                }).ToList(),

                Prices = s.Entity.Prices.Select(p => new ProductPriceDto
                {
                    Id = p.Id,
                    PriceType = p.PriceType,
                    UnitPrice = p.UnitPrice.Value,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                }).ToList(),
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