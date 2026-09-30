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
}