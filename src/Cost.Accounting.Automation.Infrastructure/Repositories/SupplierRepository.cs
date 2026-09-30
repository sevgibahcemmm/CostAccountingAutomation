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
}