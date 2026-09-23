using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Recipes;

namespace Cost.Accounting.Automation.Application.Recipes;

public sealed class RecipeItemDto
{
    [Column("Id", IsVisible = false)]
    public Guid? Id { get; set; }

    [Column("Malzeme Id", IsVisible = false)]
    public Guid ProductId { get; set; }

    [Column("Malzeme", Order = 10, Width = 220)]
    public string ProductName { get; set; } = default!;

    [Column("Birim", Order = 20, Width = 80, Alignment = "Center")]
    public string ProductUnitTypeName { get; set; } = string.Empty;

    [Column("1 Birim İçin Miktar", Order = 30, Width = 140, Format = "n2", Alignment = "Right")]
    public decimal Quantity { get; set; }
}

public sealed class RecipeListDto : EntityDto
{
    [Column("Mamül Ürün", Order = 10, Width = 260)]
    public string ProductName { get; set; } = default!;

    [Column("Birim", Order = 20, Width = 90, Alignment = "Center")]
    public string ProductUnitTypeName { get; set; } = string.Empty;

    [Column("Kalem Sayısı", Order = 30, Width = 90, Alignment = "Center")]
    public int ItemsCount { get; set; }

    [Column("Ürün Id", IsVisible = false)]
    public Guid ProductId { get; set; }

    public List<RecipeItemDto> Items { get; set; } = [];
}

public sealed class RecipeDto : EntityDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductUnitTypeName { get; set; } = string.Empty;
    public List<RecipeItemDto> Items { get; set; } = [];
}

public static class RecipeExtensions
{
    public static IQueryable<RecipeListDto> MapTo(this IQueryable<EntityWithAuditDto<Recipe>> entity)
    {
        return entity
            .Select(s => new RecipeListDto
            {
                Id = s.Entity.Id,
                ProductId = s.Entity.ProductId,
                ProductName = s.Entity.Product == null ? string.Empty : s.Entity.Product.Name.Value,
                ProductUnitTypeName = s.Entity.Product == null || s.Entity.Product.ProductUnitType == null
                    ? string.Empty
                    : s.Entity.Product.ProductUnitType.Name.Value,
                ItemsCount = s.Entity.Items.Count,
                Items = s.Entity.Items.Select(i => new RecipeItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product == null ? string.Empty : i.Product.Name.Value,
                    ProductUnitTypeName = i.Product == null || i.Product.ProductUnitType == null
                        ? string.Empty
                        : i.Product.ProductUnitType.Name.Value,
                    Quantity = i.Quantity
                }).ToList(),
                IsActive = s.Entity.IsActive,
                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy == null ? null : s.Entity.UpdatedBy.Value,
                CreatedFullName = s.CreatedUser.FullName.Value,
                UpdatedFullName = s.UpdatedUser == null ? null : s.UpdatedUser.FullName.Value
            })
            .AsQueryable();
    }

    public static RecipeDto ToDto(this Recipe recipe)
    {
        return new RecipeDto
        {
            Id = recipe.Id,
            ProductId = recipe.ProductId,
            ProductName = recipe.Product?.Name.Value ?? string.Empty,
            ProductUnitTypeName = recipe.Product?.ProductUnitType?.Name.Value ?? string.Empty,
            IsActive = recipe.IsActive,
            CreatedAt = recipe.CreatedAt,
            CreatedBy = recipe.CreatedBy,
            UpdatedAt = recipe.UpdatedAt,
            UpdatedBy = recipe.UpdatedBy == null ? null : recipe.UpdatedBy.Value,
            Items = recipe.Items
                .Select(i => new RecipeItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name.Value ?? string.Empty,
                    ProductUnitTypeName = i.Product?.ProductUnitType?.Name.Value ?? string.Empty,
                    Quantity = i.Quantity
                })
                .ToList()
        };
    }
}