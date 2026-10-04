using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;

public interface IProductUnitTypeRepository : IAuditableRepository<ProductUnitType>
{
    Task<List<ProductUnitType>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);

    /// <summary>Ürün/maliyet pusulası kayıtlarında kullanılıyorsa not üretir; engelleyici hareket yoktur.</summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> unitTypeIds,
        CancellationToken cancellationToken = default);
}