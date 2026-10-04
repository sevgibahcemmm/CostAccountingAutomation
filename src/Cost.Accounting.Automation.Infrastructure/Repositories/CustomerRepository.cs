using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class CustomerRepository : AuditableRepository<Customer, ApplicationDbContext>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// Seçilen müşterilerin tamamını <c>IN</c> listesiyle denetler. Hareket
    /// denetimi <see cref="CurrentAccountMovement"/> üzerinden yapılır; irsaliye
    /// ve maliyet pusulası yalnızca ilişkili referans sayılır.
    /// </summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid?> keys = [.. customerIds.Select(id => (Guid?)id)];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        List<Guid> movementIds = await this.Context.Set<CurrentAccountMovement>()
            .Where(m => m.CustomerId != null && keys.Contains(m.CustomerId))
            .Select(m => m.CustomerId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> invoiceIds = await this.Context.Set<Invoice>()
            .Where(i => i.CustomerId != null && keys.Contains(i.CustomerId))
            .Select(i => i.CustomerId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> costSlipIds = await this.Context.Set<CostSlip>()
            .Where(c => c.CustomerId != null && keys.Contains(c.CustomerId))
            .Select(c => c.CustomerId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new DeletionCheck(movementIds, [.. invoiceIds, .. costSlipIds]);
    }
}
