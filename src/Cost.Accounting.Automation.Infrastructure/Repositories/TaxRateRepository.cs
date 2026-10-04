using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class TaxRateRepository : AuditableRepository<TaxRate, ApplicationDbContext>, ITaxRateRepository
{
    public TaxRateRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    public Task<List<TaxRate>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        => this.Context.Set<TaxRate>().IgnoreQueryFilters().ToListAsync(cancellationToken);

    /// <summary>KDV oranının bağlı olduğu ürünleri tek sorguda bulur.</summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> taxRateIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid> keys = [.. taxRateIds];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        List<Guid> usedBy = await this.Context.Set<Product>()
            .Where(p => keys.Contains(p.TaxRateId))
            .Select(p => p.TaxRateId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new DeletionCheck([], usedBy);
    }
}