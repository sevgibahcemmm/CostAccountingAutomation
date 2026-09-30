using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
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
    bool OnlyDeleted = false,
    bool IncludeMovements = false,
    int? MovementLimit = null) : IRequest<List<ProductCatalogDto>>
{
    public ProductCatalogGetAllQuery() : this(null, false, false, null) { }
}

internal sealed class ProductCatalogGetAllQueryHandler(
    IProductRepository productRepository,
    IProductMovementRepository productMovementRepository,
    IInvoiceRepository invoiceRepository,
    IStockIssueRepository stockIssueRepository,
    ICostSlipRepository costSlipRepository) : IRequestHandler<ProductCatalogGetAllQuery, List<ProductCatalogDto>>
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

        Dictionary<Guid, decimal> costByProductId = await productRepository.GetCostByProductIdsAsync(productIds, cancellationToken);

        Dictionary<Guid, List<ProductStockMovementDetailDto>>? movementsByProductId = null;
        if (request.IncludeMovements)
        {
            movementsByProductId = await ProductMovementDetailBuilder.BuildByProductAsync(
                productIds,
                productMovementRepository,
                invoiceRepository,
                stockIssueRepository,
                costSlipRepository,
                cancellationToken,
                request.MovementLimit);
        }
        long movementsMs = sw.ElapsedMilliseconds;

        foreach (ProductCatalogDto item in items)
        {
            if (stockByProductId.TryGetValue(item.Id, out decimal stock))
            {
                item.StockQuantity = stock;
            }

            if (costByProductId.TryGetValue(item.Id, out decimal cost))
            {
                item.CostPrice = cost;
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

            if (movementsByProductId is not null
                && movementsByProductId.TryGetValue(item.Id, out List<ProductStockMovementDetailDto>? movementDetails))
            {
                item.Movements = movementDetails;
            }
        }

        Debug.WriteLine($"[CATALOG] base={baseMs}ms stock={stockMs - baseMs}ms prices={pricesMs - stockMs}ms movements={(request.IncludeMovements ? movementsMs - pricesMs : 0)}ms total={sw.ElapsedMilliseconds}ms count={items.Count}");

        return items;
    }
}
