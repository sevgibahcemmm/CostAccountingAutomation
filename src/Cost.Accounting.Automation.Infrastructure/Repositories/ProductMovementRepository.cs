using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ProductMovementRepository : AuditableRepository<ProductMovement, ApplicationDbContext>, IProductMovementRepository
{
    public ProductMovementRepository(ApplicationDbContext context) : base(context)
    {
    }
}
