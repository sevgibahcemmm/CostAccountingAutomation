using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Products;

public interface IProductMovementRepository : IAuditableRepository<ProductMovement>
{
    Task<List<ProductMovementQueryResult>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
}
