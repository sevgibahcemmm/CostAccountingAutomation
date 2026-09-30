using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ProductMovementRepository : AuditableRepository<ProductMovement, ApplicationDbContext>, IProductMovementRepository
{
    public ProductMovementRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    public async Task<List<ProductMovementQueryResult>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> ids = productIds.Select(id => new IdentityId(id)).ToHashSet();
        if (ids.Count == 0)
        {
            return [];
        }

        return await this.Context.Set<ProductMovement>()
            .AsNoTracking()
            .Where(m => ids.Contains(m.ProductId))
            .Select(m => new ProductMovementQueryResult(
                m.ProductId.Value,
                m.Id.Value,
                m.MovementType,
                m.Reason,
                m.Quantity,
                m.UnitPrice == null ? null : m.UnitPrice.Value,
                m.Date,
                m.ReferenceNo,
                m.Description.Value,
                m.InvoiceId == null ? null : m.InvoiceId.Value,
                m.StockIssueId == null ? null : m.StockIssueId.Value))
            .ToListAsync(cancellationToken);
    }
}