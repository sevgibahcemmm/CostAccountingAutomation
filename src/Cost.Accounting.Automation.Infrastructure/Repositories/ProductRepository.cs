using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ProductRepository : AuditableRepository<Product, ApplicationDbContext>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<List<Product>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        => Context.Set<Product>()
            .Include(p => p.Warehouse)
            .Include(p => p.Category)
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

    public Task<Product?> GetByIdWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => Context.Set<Product>()
            .Include(p => p.Warehouse)
            .Include(p => p.Category)
            .Include(p => p.ProductUnitType)
            .Include(p => p.TaxRate)
            .Include(p => p.ChartOfAccount)
            .Include(p => p.SemiFinishedProduct)
            .Include(p => p.Prices)
            .Include(p => p.Movements)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Dictionary<Guid, decimal>> GetStockByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> ids = productIds.Select(id => new IdentityId(id)).ToHashSet();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var stock = await Context.Set<ProductMovement>()
            .AsNoTracking()
            .Where(m => ids.Contains(m.ProductId))
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Stock = g.Sum(m => m.MovementType == ProductMovementType.Input ? m.Quantity : -m.Quantity)
            })
            .ToListAsync(cancellationToken);

        return stock.ToDictionary(x => x.ProductId.Value, x => x.Stock);
    }

    public async Task<Dictionary<Guid, List<ProductPriceQueryResult>>> GetPricesByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> ids = productIds.Select(id => new IdentityId(id)).ToHashSet();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, List<ProductPriceQueryResult>>();
        }

        List<ProductPriceQueryResult> results = await Context.Set<Product>()
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .SelectMany(p => p.Prices, (p, price) => new ProductPriceQueryResult(
                p.Id.Value,
                price.Id.Value,
                price.PriceType,
                price.UnitPrice.Value,
                price.StartDate,
                price.EndDate))
            .ToListAsync(cancellationToken);

        return results
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}