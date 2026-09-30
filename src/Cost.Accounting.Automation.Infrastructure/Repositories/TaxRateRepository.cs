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
}