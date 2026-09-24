using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Products;

public interface IProductRepository : IAuditableRepository<Product>
{
    Task<List<Product>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, decimal>> GetStockByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, List<ProductPriceQueryResult>>> GetPricesByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
}