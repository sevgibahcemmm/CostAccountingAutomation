using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Products;

public interface ITaxRateRepository : IAuditableRepository<TaxRate>
{
    Task<List<TaxRate>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
}