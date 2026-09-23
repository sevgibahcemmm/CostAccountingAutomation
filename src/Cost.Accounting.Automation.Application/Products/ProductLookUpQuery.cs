using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Products;

public sealed class ProductLookUpDto
{
    [Column("Id", IsVisible = false)]
    public Guid Id { get; set; }

    [Column("Ürün Adı", Order = 10, Width = 220)]
    public string Name { get; set; } = default!;

    [Column("Ürün Kodu", Order = 20, Width = 110)]
    public string ProductCode { get; set; } = default!;

    [Column("Birim", Order = 30, Width = 80, Alignment = "Center")]
    public string ProductUnitTypeName { get; set; } = string.Empty;

    [Column("Depo", Order = 40, Width = 130)]
    public string WarehouseName { get; set; } = string.Empty;

    [Column("Depo Kodu", IsVisible = false)]
    public string WarehouseCode { get; set; } = string.Empty;

    [Column("Kategori", Order = 45, Width = 120)]
    public string CategoryName { get; set; } = string.Empty;

    [Column("Kategori Id", IsVisible = false)]
    public Guid? CategoryId { get; set; }

    public string Display => $"{Name} ({ProductCode}) [{ProductUnitTypeName}]";
}

[Permission("product:view")]
public sealed record ProductLookUpQuery(string WarehouseCode) : IRequest<List<ProductLookUpDto>>;

internal sealed class ProductLookUpQueryHandler(
    IProductRepository productRepository) : IRequestHandler<ProductLookUpQuery, List<ProductLookUpDto>>
{
    public async Task<List<ProductLookUpDto>> Handle(ProductLookUpQuery request, CancellationToken cancellationToken)
    {
        List<Product> products = await productRepository.GetAllIncludingDeletedAsync(cancellationToken);

        return products
            .Where(p => !p.IsDeleted)
            .Where(p => p.Warehouse != null && p.Warehouse.Code.Value.StartsWith(request.WarehouseCode))
            .Select(p => new ProductLookUpDto
            {
                Id = p.Id,
                Name = p.Name.Value,
                ProductCode = p.ProductCode.Value,
                ProductUnitTypeName = p.ProductUnitType?.Name.Value ?? string.Empty,
                WarehouseName = p.Warehouse?.Name.Value ?? string.Empty,
                WarehouseCode = p.Warehouse?.Code.Value ?? string.Empty,
                CategoryName = p.Category?.Name.Value ?? string.Empty,
                CategoryId = p.CategoryId.Value
            })
            .OrderBy(p => p.Name)
            .ToList();
    }
}