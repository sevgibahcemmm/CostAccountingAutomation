using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ProductUnitTypeRepository : AuditableRepository<ProductUnitType, ApplicationDbContext>, IProductUnitTypeRepository
{
    public ProductUnitTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<List<ProductUnitType>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        => Context.Set<ProductUnitType>().IgnoreQueryFilters().ToListAsync(cancellationToken);
}