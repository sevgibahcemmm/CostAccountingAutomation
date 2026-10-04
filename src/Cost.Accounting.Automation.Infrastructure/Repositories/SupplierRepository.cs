using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class SupplierRepository : AuditableRepository<Supplier, ApplicationDbContext>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// Seçilen tedarikçilerin tamamını <c>IN</c> listesiyle denetler. Hareket
    /// denetimi <see cref="CurrentAccountMovement"/> üzerinden yapılır; irsaliye
    /// yalnızca ilişkili referans sayılır.
    /// </summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> supplierIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid?> keys = [.. supplierIds.Select(id => (Guid?)id)];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        List<Guid> movementIds = await this.Context.Set<CurrentAccountMovement>()
            .Where(m => m.SupplierId != null && keys.Contains(m.SupplierId))
            .Select(m => m.SupplierId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> invoiceIds = await this.Context.Set<Invoice>()
            .Where(i => i.SupplierId != null && keys.Contains(i.SupplierId))
            .Select(i => i.SupplierId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new DeletionCheck(movementIds, invoiceIds);
    }
}
