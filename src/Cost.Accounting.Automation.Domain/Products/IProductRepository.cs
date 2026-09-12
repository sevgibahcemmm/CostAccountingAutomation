using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Products;

public interface IProductRepository : IAuditableRepository<Product>
{
    Task<List<Product>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default);
}