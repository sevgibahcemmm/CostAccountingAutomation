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
    public static IQueryable<ProductMovementListDto> MapTo(this IQueryable<EntityWithAuditDto<ProductMovement>> entity)
    {
        return entity
            .Select(s => new ProductMovementListDto
            {
                Id = s.Entity.Id,
                ProductId = s.Entity.ProductId,
                ProductName = s.Entity.Product == null ? string.Empty : s.Entity.Product.Name.Value,
                ProductCode = s.Entity.Product == null ? string.Empty : s.Entity.Product.ProductCode.Value,
                Barcode = s.Entity.Product == null ? null : s.Entity.Product.Barcode.Value,
                WarehouseName = s.Entity.Product == null || s.Entity.Product.Warehouse == null ? string.Empty : s.Entity.Product.Warehouse.Name.Value,
                UnitTypeName = s.Entity.Product == null || s.Entity.Product.ProductUnitType == null ? string.Empty : s.Entity.Product.ProductUnitType.Name.Value,

                MovementType = s.Entity.MovementType,
                Reason = s.Entity.Reason,
                Quantity = s.Entity.Quantity,
                UnitPrice = s.Entity.UnitPrice == null ? null : s.Entity.UnitPrice.Value,
                Date = s.Entity.Date,
                ReferenceNo = s.Entity.ReferenceNo,
                Description = s.Entity.Description.Value,
                InvoiceId = s.Entity.InvoiceId == null ? null : s.Entity.InvoiceId.Value,

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
