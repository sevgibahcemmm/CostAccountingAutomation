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
    public static IQueryable<ProductUnitTypeDto> MapTo(this IQueryable<EntityWithAuditDto<ProductUnitType>> entity)
    {
        return entity
            .Select(s => new ProductUnitTypeDto
            {
                Id = s.Entity.Id,
                Name = s.Entity.Name.Value,

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