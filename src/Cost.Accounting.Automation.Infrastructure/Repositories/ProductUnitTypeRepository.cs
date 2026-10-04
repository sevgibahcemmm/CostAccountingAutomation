using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ProductUnitTypeRepository : AuditableRepository<ProductUnitType, ApplicationDbContext>, IProductUnitTypeRepository
{
    public ProductUnitTypeRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    public Task<List<ProductUnitType>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        => this.Context.Set<ProductUnitType>().IgnoreQueryFilters().ToListAsync(cancellationToken);

    /// <summary>Birim cinsinin geçtiği ürün ve maliyet pusulası kayıtlarını tek sorguda bulur.</summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> unitTypeIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid> keys = [.. unitTypeIds];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        List<Guid?> nullableKeys = [.. unitTypeIds.Select(id => (Guid?)id)];

        List<Guid> productIds = await this.Context.Set<Product>()
            .Where(p => keys.Contains(p.ProductUnitTypeId))
            .Select(p => p.ProductUnitTypeId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> slipItemIds = await this.Context.Set<CostSlipItem>()
            .Where(i => i.ProductUnitTypeId != null && nullableKeys.Contains(i.ProductUnitTypeId))
            .Select(i => i.ProductUnitTypeId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new DeletionCheck([], [.. productIds, .. slipItemIds]);
    }
}