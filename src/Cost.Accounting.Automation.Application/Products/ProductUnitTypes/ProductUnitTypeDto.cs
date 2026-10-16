using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

public sealed class ProductUnitTypeDto : EntityDto
{
    [Column("Birim Cinsi", Order = 10, Width = 250)]
    public string Name { get; set; } = default!;
}

public static class ProductUnitTypeExtensions
{
    public static IQueryable<ProductUnitTypeDto> MapTo(this IQueryable<ProductUnitType> entity)
    {
        return entity
            .Select(s => new ProductUnitTypeDto
            {
                Id = s.Id,
                Name = s.Name.Value,

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value,
            });
    }
}