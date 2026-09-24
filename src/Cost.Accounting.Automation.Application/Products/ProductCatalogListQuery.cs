using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Ürün katalog listesi için hafif token sorgu: ürün/kolon başına korelasyonlu alt sorgu
/// (stok toplamı, fiyatlar) içermez. Stok ve fiyat bilgilerini toplu olarak dolu döndüren
/// <see cref="ProductCatalogGetAllQuery"/> ile birlikte kullanılır.
/// </summary>
[Permission("product:view")]
public sealed record ProductCatalogListQuery(
    Guid? WarehouseId = null,
    bool OnlyDeleted = false) : IRequest<IQueryable<ProductCatalogDto>>
{
    public ProductCatalogListQuery() : this(null, false) { }
}

internal sealed class ProductCatalogListQueryHandler(
    IProductRepository productRepository) : IRequestHandler<ProductCatalogListQuery, IQueryable<ProductCatalogDto>>
{
    public Task<IQueryable<ProductCatalogDto>> Handle(ProductCatalogListQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<Product>> source = request.OnlyDeleted
            ? productRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : productRepository.GetAllWithAudit();

        if (request.WarehouseId.HasValue)
        {
            IdentityId warehouseId = new(request.WarehouseId.Value);
            source = source.Where(i => i.Entity.WarehouseId == warehouseId);
        }

        return Task.FromResult(source.MapToCatalog());
    }
}