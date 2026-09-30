using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class InvoiceRepository : AuditableRepository<Invoice, ApplicationDbContext>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }
}
