using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Suppliers;

public interface ISupplierRepository : IAuditableRepository<Supplier>
{
    /// <summary>
    /// Seçilen tedarikçilerin tamamını tek sorguda denetler: cari hareket gören
    /// tedarikçi silinemez; yalnızca irsaliyede geçen tedarikçi silinir ve not
    /// üretir.
    /// </summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> supplierIds,
        CancellationToken cancellationToken = default);
}
