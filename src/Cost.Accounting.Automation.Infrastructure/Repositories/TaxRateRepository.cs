using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class TaxRateRepository : AuditableRepository<TaxRate, ApplicationDbContext>, ITaxRateRepository
{
    public TaxRateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<List<TaxRate>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        => Context.Set<TaxRate>().IgnoreQueryFilters().ToListAsync(cancellationToken);
}