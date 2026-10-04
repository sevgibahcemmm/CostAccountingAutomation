using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Products.TaxRates;

public interface ITaxRateRepository : IAuditableRepository<TaxRate>
{
    Task<List<TaxRate>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);

    /// <summary>Ürünlerde kullanılan oran varsa not üretir; engelleyici hareket yoktur.</summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> taxRateIds,
        CancellationToken cancellationToken = default);
}