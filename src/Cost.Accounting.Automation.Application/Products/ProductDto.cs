using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.Products;

public sealed class ProductPriceDto
{
    public Guid Id { get; set; }
    public ProductPriceType PriceType { get; set; }
    public decimal UnitPrice { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public sealed class ProductMovementDto
{
    public Guid Id { get; set; }
    public ProductMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public DateOnly Date { get; set; }
    public string? ReferenceNo { get; set; }
    public string Description { get; set; } = default!;
}

public sealed class ProductImageDto
{
    public Guid Id { get; set; }
    public string Path { get; set; } = default!;
    public bool IsPrimary { get; set; }
}

public sealed class ProductDto : EntityDto
{
    public string Name { get; set; } = default!;
    public string ProductCode { get; set; } = default!;
    public string? Barcode { get; set; }
    public string? QRCode { get; set; }
    public decimal? MinimumProductLevel { get; set; }
    public decimal TaxRate { get; set; }

    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = default!;
    public string WarehouseName { get; set; } = default!;

    public Guid CategoryId { get; set; }
    public string CategoryCode { get; set; } = default!;
    public string CategoryName { get; set; } = default!;

    public Guid ProductUnitTypeId { get; set; }
    public string ProductUnitTypeName { get; set; } = default!;

    public Guid? ChartOfAccountId { get; set; }
    public string? ChartOfAccountCode { get; set; }

    public string Description { get; set; } = default!;
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
                TaxRate = s.Entity.TaxRate,

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
                    IsPrimary = i.IsPrimary
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