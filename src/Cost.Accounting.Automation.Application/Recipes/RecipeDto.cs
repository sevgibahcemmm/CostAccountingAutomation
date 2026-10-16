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

    [Column("Reçete Miktarı", Order = 30, Width = 140, Format = "n2", Alignment = "Right")]
    public decimal Quantity { get; set; }

    /// <summary>Kalem miktarının tanımlı olduğu üretim miktarı (çıktı). Bu değer üst reçeteden gelir.</summary>
    public decimal OutputQuantity { get; set; }

    /// <summary>Bir birim mamul/hizmet üretimi için gereken malzeme miktarı (Quantity / OutputQuantity).</summary>
    public decimal UnitQuantity { get; set; }
}

public sealed class RecipeListDto : EntityDto
{
    [Column("Mamül Ürün", Order = 10, Width = 260)]
    public string ProductName { get; set; } = default!;

    [Column("Atölye", Order = 15, Width = 160)]
    public string WorkshopName { get; set; } = string.Empty;

    [Column("Birim", Order = 20, Width = 90, Alignment = "Center")]
    public string ProductUnitTypeName { get; set; } = string.Empty;

    [Column("Üretim Miktarı", Order = 25, Width = 110, Format = "n0", Alignment = "Center")]
    public decimal OutputQuantity { get; set; }

    [Column("Kalem Sayısı", Order = 30, Width = 90, Alignment = "Center")]
    public int ItemsCount { get; set; }

    [Column("Ürün Id", IsVisible = false)]
    public Guid ProductId { get; set; }

    [Column("Atölye Id", IsVisible = false)]
    public Guid? WorkshopId { get; set; }

    public List<RecipeItemDto> Items { get; set; } = [];
}

public sealed class RecipeDto : EntityDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductUnitTypeName { get; set; } = string.Empty;
    public Guid? WorkshopId { get; set; }
    public string WorkshopName { get; set; } = string.Empty;
    public decimal OutputQuantity { get; set; }
    public List<RecipeItemDto> Items { get; set; } = [];
}

public static class RecipeExtensions
{
    public static IQueryable<RecipeListDto> MapTo(this IQueryable<Recipe> entity)
    {
        return entity
            .Select(s => new RecipeListDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                ProductName = s.Product == null ? string.Empty : s.Product.Name.Value,
                WorkshopId = s.WorkshopId == null ? (Guid?)null : s.WorkshopId.Value,
                WorkshopName = s.Workshop == null ? string.Empty : s.Workshop.Name.Value,
                ProductUnitTypeName = s.Product == null || s.Product.ProductUnitType == null
                    ? string.Empty
                    : s.Product.ProductUnitType.Name.Value,
                OutputQuantity = s.OutputQuantity,
                ItemsCount = s.Items.Count,
                Items = s.Items.Select(i => new RecipeItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product == null ? string.Empty : i.Product.Name.Value,
                    ProductUnitTypeName = i.Product == null || i.Product.ProductUnitType == null
                        ? string.Empty
                        : i.Product.ProductUnitType.Name.Value,
                    Quantity = i.Quantity,
                    OutputQuantity = s.OutputQuantity,
                    UnitQuantity = s.OutputQuantity <= 0
                        ? i.Quantity
                        : i.Quantity / s.OutputQuantity
                }).ToList(),
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value,
            });
    }

    public static RecipeDto ToDto(this Recipe recipe)
    {
        return new RecipeDto
        {
            Id = recipe.Id,
            ProductId = recipe.ProductId,
            ProductName = recipe.Product?.Name.Value ?? string.Empty,
            ProductUnitTypeName = recipe.Product?.ProductUnitType?.Name.Value ?? string.Empty,
            WorkshopId = recipe.WorkshopId?.Value,
            WorkshopName = recipe.Workshop?.Name.Value ?? string.Empty,
            OutputQuantity = recipe.OutputQuantity,
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
                    Quantity = i.Quantity,
                    OutputQuantity = recipe.OutputQuantity,
                    UnitQuantity = recipe.OutputQuantity <= 0
                        ? i.Quantity
                        : i.Quantity / recipe.OutputQuantity
                })
                .ToList()
        };
    }
}