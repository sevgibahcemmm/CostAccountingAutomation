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

    /// <summary>
    /// GetAllWithAudit sorgusu sonuçları belleğe materyalize ettiği için Include
    /// uygulanmazsa navigasyonlar null gelir: "Cari Adı" boş kalır ve master-detail
    /// satırı açılamaz. Bu yüzden cari/tedarikçi, kalemler ve kalem ürünleri
    /// burada açıkça yüklenir.
    /// </summary>
    protected override IQueryable<Invoice> ApplyDetailIncludes(IQueryable<Invoice> query)
        => query
            .AsSplitQuery()
            .Include(i => i.Customer)
            .Include(i => i.Supplier)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product);
}
