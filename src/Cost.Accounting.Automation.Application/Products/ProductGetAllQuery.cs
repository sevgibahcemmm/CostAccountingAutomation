using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:view")]
public sealed record ProductGetAllQuery(
    Guid? WarehouseId = null,
    bool OnlyDeleted = false) : IRequest<IQueryable<ProductDto>>
{
    public ProductGetAllQuery() : this(null, false) { }
}

internal sealed class ProductGetAllQueryHandler(
    IProductRepository productRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<ProductGetAllQuery, IQueryable<ProductDto>>
{
    public async Task<IQueryable<ProductDto>> Handle(ProductGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Product> source = request.OnlyDeleted
            ? productRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : productRepository.GetAllActive();

        if (request.WarehouseId is Guid warehouseId)
        {
            IdentityId id = new(warehouseId);
            source = source.Where(i => i.WarehouseId == id);
        }

        List<ProductDto> items = await source
            .MapTo()
            .ToListAsync(cancellationToken);

        // Stok miktarları tek bir GROUP BY sorgusuyla gelir. Ürün başına
        // korelasyonlu alt sorgu kurmak yerine toplu yapmak, sorgu sayısını
        // ürün sayısından bağımsız tutar ve hareket satırlarını belleğe taşımaz.
        Dictionary<Guid, decimal> stock = await productRepository
            .GetStockByProductIdsAsync(items.Select(i => i.Id), cancellationToken);

        foreach (ProductDto item in items)
        {
            item.StockQuantity = stock.GetValueOrDefault(item.Id);
        }

        await auditUserNames.FillAsync(items, cancellationToken);

        return items.AsQueryable();
    }
}
