using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ProductRepository : AuditableRepository<Product, ApplicationDbContext>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// <c>GetAllWithAudit</c> sonucu belleğe materyalize edildiği için navigasyon
    /// özellikleri ancak burada <c>Include</c> edilirse dolar. Aksi hâlde
    /// <c>ProductDto</c> eşlemesinde birim, KDV, depo, kategori, hesap kodu,
    /// stok, fiyat ve görsel alanları sessizce boş kalır.
    ///
    /// Hem referans hem koleksiyon navigasyonları birlikte istendiği için
    /// <c>AsSplitQuery</c> kullanılır; aksi hâlde EF kartesian patlama üretir.
    /// </summary>
    protected override IQueryable<Product> ApplyDetailIncludes(IQueryable<Product> query)
        => query
            .Include(p => p.Warehouse)
            .Include(p => p.Category)
            .Include(p => p.ProductUnitType)
            .Include(p => p.TaxRate)
            .Include(p => p.ChartOfAccount)
            .Include(p => p.SemiFinishedProduct)
            .AsSplitQuery()
            .Include(p => p.Prices)
            .Include(p => p.Movements)
            .Include(p => p.Images);

    public Task<List<Product>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        => this.Context.Set<Product>()
            .Include(p => p.Warehouse)
            .Include(p => p.Category)
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

    public Task<Product?> GetByIdWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => this.Context.Set<Product>()
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

        var stock = await this.Context.Set<ProductMovement>()
            .AsNoTracking()
            .WhereCountsAsProductStock()
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

        List<ProductPriceQueryResult> results = await this.Context.Set<Product>()
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

    public async Task<Dictionary<Guid, decimal>> GetCostByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> ids = productIds.Select(id => new IdentityId(id)).ToHashSet();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var costs = await this.Context.Set<ProductMovement>()
            .AsNoTracking()
            .WhereCountsAsProductStock()
            .Where(m => ids.Contains(m.ProductId)
                && m.MovementType == ProductMovementType.Input
                && m.UnitPrice != null
                && m.UnitPrice.Value > 0)
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(m => m.Quantity),
                TotalCost = g.Sum(m => m.Quantity * m.UnitPrice!.Value)
            })
            .ToListAsync(cancellationToken);

        return costs
            .Where(x => x.Quantity > 0)
            .ToDictionary(x => x.ProductId.Value, x => Math.Round(x.TotalCost / x.Quantity, 4));
    }

    /// <summary>
    /// Seçilen ürünlerin tamamını <c>IN</c> listesiyle denetler. Stok hareketi
    /// engelleyicidir; irsaliye satırı, maliyet pusulası, stok çıkışı satırı ve
    /// yarı mamul bağlantısı yalnızca ilişkili referans sayılır.
    /// </summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid> keys = [.. productIds];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        List<Guid?> nullableKeys = [.. productIds.Select(id => (Guid?)id)];

        List<Guid> movementIds = await this.Context.Set<ProductMovement>()
            .Where(m => keys.Contains(m.ProductId))
            .Select(m => m.ProductId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> invoiceIds = await this.Context.Set<InvoiceLine>()
            .Where(l => keys.Contains(l.ProductId))
            .Select(l => l.ProductId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> producedIds = await this.Context.Set<CostSlip>()
            .Where(c => c.ProducedProductId != null && nullableKeys.Contains(c.ProducedProductId))
            .Select(c => c.ProducedProductId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> slipItemIds = await this.Context.Set<CostSlipItem>()
            .Where(i => i.ProductId != null && nullableKeys.Contains(i.ProductId))
            .Select(i => i.ProductId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> stockIssueIds = await this.Context.Set<StockIssueLine>()
            .Where(l => keys.Contains(l.ProductId))
            .Select(l => l.ProductId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> linkedProductIds = await this.Context.Set<Product>()
            .Where(p => p.SemiFinishedProductId != null && nullableKeys.Contains(p.SemiFinishedProductId))
            .Select(p => p.SemiFinishedProductId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new DeletionCheck(
            movementIds,
            [.. invoiceIds, .. producedIds, .. slipItemIds, .. stockIssueIds, .. linkedProductIds]);
    }
}