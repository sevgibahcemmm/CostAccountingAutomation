using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Products;

public interface IProductUnitTypeRepository : IAuditableRepository<ProductUnitType>
{
    Task<List<ProductUnitType>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
}