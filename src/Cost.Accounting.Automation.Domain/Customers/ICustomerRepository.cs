using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Customers;

public interface ICustomerRepository : IAuditableRepository<Customer>
{
    /// <summary>
    /// Seçilen müşterilerin tamamını tek sorguda denetler: cari hareket gören
    /// müşteri silinemez; yalnızca irsaliye/maliyet pusulasında geçen müşteri
    /// silinir ve not üretir.
    /// </summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default);
}
