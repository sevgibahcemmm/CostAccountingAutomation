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
        => Context.Set<Product>().IgnoreQueryFilters().ToListAsync(cancellationToken);

    public Task<Product?> GetByIdWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => Context.Set<Product>()
            .Include(p => p.TaxRate)
            .Include(p => p.Prices)
            .Include(p => p.Movements)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}