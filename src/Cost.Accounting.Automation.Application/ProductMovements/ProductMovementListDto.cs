using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.ProductMovements;

public sealed class ProductMovementListDto : EntityDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ProductCode { get; set; } = default!;
    public string? Barcode { get; set; }
    public string WarehouseName { get; set; } = default!;
    public string UnitTypeName { get; set; } = default!;

    public ProductMovementType MovementType { get; set; }
    public string MovementTypeName => MovementType == ProductMovementType.Input ? "Giriş" : "Çıkış";
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice => UnitPrice.HasValue ? Quantity * UnitPrice.Value : null;
    public DateOnly Date { get; set; }
    public string? ReferenceNo { get; set; }
    public string Description { get; set; } = default!;
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
