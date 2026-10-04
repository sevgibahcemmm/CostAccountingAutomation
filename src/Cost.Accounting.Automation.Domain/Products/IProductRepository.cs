using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Products;

public interface IProductRepository : IAuditableRepository<Product>
{
    Task<List<Product>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, decimal>> GetStockByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, List<ProductPriceQueryResult>>> GetPricesByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);

/// <summary>
    /// Fiyat kaydı olmayan ürünlerde (örn. 151/152 depoları) maliyet fiyatı: giriş stok
    /// hareketlerinin birim maliyetlerinin ağırlıklı ortalaması.
    /// </summary>
    Task<Dictionary<Guid, decimal>> GetCostByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seçilen ürünlerin tamamını tek sorguda denetler: stok hareketi gören ürün
    /// silinemez; yalnızca irsaliye/maliyet pusulası/stok çıkışında geçen ürün
    /// silinir ve not üretir.
    /// </summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}