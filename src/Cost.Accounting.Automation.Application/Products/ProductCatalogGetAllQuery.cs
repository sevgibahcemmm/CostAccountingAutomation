using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Fiyat &amp; Stok Listesi ve fatura ekranındaki katalog için ürünleri, stok bakiyelerini ve
/// fiyatları döndürür. Stok ve fiyat, her ürün için bir literal subquery yerine tek GROUP BY /
/// tek JOIN ile toplu çekildiği için binlerce üründe bile hızlıdır.
/// </summary>
[Permission("product:view")]
public sealed record ProductCatalogGetAllQuery(
    Guid? WarehouseId = null,
    bool OnlyDeleted = false) : IRequest<List<ProductCatalogDto>>
{
    public ProductCatalogGetAllQuery() : this(null, false) { }
}

internal sealed class ProductCatalogGetAllQueryHandler(
    IProductRepository productRepository) : IRequestHandler<ProductCatalogGetAllQuery, List<ProductCatalogDto>>
{
    public async Task<List<ProductCatalogDto>> Handle(ProductCatalogGetAllQuery request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        IQueryable<EntityWithAuditDto<Product>> source = request.OnlyDeleted
            ? productRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : productRepository.GetAllWithAudit();

        if (request.WarehouseId.HasValue)
        {
            IdentityId warehouseId = new(request.WarehouseId.Value);
            source = source.Where(i => i.Entity.WarehouseId == warehouseId);
        }

        List<ProductCatalogDto> items = await source.AsNoTracking()
            .MapToCatalog()
            .ToListAsync(cancellationToken);
        items = items
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToList();
        long baseMs = sw.ElapsedMilliseconds;

        if (items.Count == 0)
        {
            Debug.WriteLine($"[CATALOG] base={baseMs}ms stock=0ms prices=0ms total={sw.ElapsedMilliseconds}ms count=0 (empty)");
            return items;
        }

        List<Guid> productIds = items.Select(i => i.Id).ToList();

        Dictionary<Guid, decimal> stockByProductId = await productRepository.GetStockByProductIdsAsync(productIds, cancellationToken);
        long stockMs = sw.ElapsedMilliseconds;

        Dictionary<Guid, List<ProductPriceQueryResult>> pricesByProductId = await productRepository.GetPricesByProductIdsAsync(productIds, cancellationToken);
        long pricesMs = sw.ElapsedMilliseconds;

        foreach (ProductCatalogDto item in items)
        {
            if (stockByProductId.TryGetValue(item.Id, out decimal stock))
            {
                item.StockQuantity = stock;
            }

            if (pricesByProductId.TryGetValue(item.Id, out List<ProductPriceQueryResult>? priceResults))
            {
                item.Prices = priceResults.Select(p => new ProductPriceDto
                {
                    Id = p.Id,
                    PriceType = p.PriceType,
                    UnitPrice = p.UnitPrice,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                }).ToList();
            }
        }

        Debug.WriteLine($"[CATALOG] base={baseMs}ms stock={stockMs - baseMs}ms prices={pricesMs - stockMs}ms total={sw.ElapsedMilliseconds}ms count={items.Count}");

        return items;
    }
}