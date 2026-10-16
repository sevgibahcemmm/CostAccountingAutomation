using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.ProductMovements;

public sealed class ProductMovementListDto : EntityDto
{
    [Column("Ürün Id", IsVisible = false)]
    public Guid ProductId { get; set; }

    [Column("Ürün Adı", Order = 30, Width = 200)]
    public string ProductName { get; set; } = default!;

    [Column("Ürün Kodu", Order = 20, Width = 100, Alignment = "Right")]
    public string ProductCode { get; set; } = default!;

    [Column("Barkod", Order = 100, IsVisible = false)]
    public string? Barcode { get; set; }

    [Column("Depo", Order = 40, Width = 120)]
    public string WarehouseName { get; set; } = default!;

    [Column("Depo", Order = 40, Width = 120)]
    public string WarehouseGroup { get; set; } = default!;

    [Column("Birim", Order = 55, Width = 70, Alignment = "Center")]
    public string UnitTypeName { get; set; } = default!;

    [Column("Hareket Türü", IsVisible = false)]
    public ProductMovementType MovementType { get; set; }

    [Column("Hareket Türü", Order = 15, Width = 80, Alignment = "Center")]
    public string MovementTypeName => MovementType == ProductMovementType.Input ? "Giriş" : "Çıkış";

    [Column("Neden", IsVisible = false)]
    public ProductMovementReason Reason { get; set; }

    [Column("Neden", Order = 18, Width = 110)]
    public string ReasonName => EnumDisplay.GetDisplayName(Reason);

    [Column("Miktar", Order = 50, Width = 90, Format = "n2", Alignment = "Right")]
    public decimal Quantity { get; set; }

    [Column("Birim Fiyat", Order = 60, Width = 100, Format = "n2", Alignment = "Right")]
    public decimal? UnitPrice { get; set; }

    [Column("Toplam Tutar", Order = 70, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal? TotalPrice => UnitPrice.HasValue ? Quantity * UnitPrice.Value : null;

    [Column("Tarih", Order = 10, Width = 90, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly Date { get; set; }

    [Column("Belge / Ref No", Order = 80, Width = 110)]
    public string? ReferenceNo { get; set; }

    [Column("Açıklama", Order = 90, Width = 180)]
    public string Description { get; set; } = default!;

    [Column("Fatura Id", IsVisible = false)]
    public Guid? InvoiceId { get; set; }
}

public static class ProductMovementExtensions
{
    public static IQueryable<ProductMovementListDto> MapTo(this IQueryable<ProductMovement> entity)
    {
        return entity
            .Select(s => new ProductMovementListDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                ProductName = s.Product == null ? string.Empty : s.Product.Name.Value,
                ProductCode = s.Product == null ? string.Empty : s.Product.ProductCode.Value,
                Barcode = s.Product == null ? null : s.Product.Barcode.Value,
                WarehouseName = s.Product == null || s.Product.Warehouse == null ? string.Empty : s.Product.Warehouse.Name.Value,
                WarehouseGroup = s.Product == null || s.Product.Warehouse == null
                    ? string.Empty
                    : $"{s.Product.Warehouse.Code.Value} - {s.Product.Warehouse.Name.Value}",
                UnitTypeName = s.Product == null || s.Product.ProductUnitType == null ? string.Empty : s.Product.ProductUnitType.Name.Value,

                MovementType = s.MovementType,
                Reason = s.Reason,
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice == null ? null : s.UnitPrice.Value,
                Date = s.Date,
                ReferenceNo = s.ReferenceNo,
                Description = s.Description.Value,
                InvoiceId = s.InvoiceId == null ? null : s.InvoiceId.Value,

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value,
            });
    }
}
