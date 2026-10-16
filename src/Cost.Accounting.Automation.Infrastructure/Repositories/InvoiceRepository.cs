using Cost.Accounting.Automation.Domain.Abstractions;
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
    /// Liste sorgusu artık SQL üzerinde projekte edildiği için projeksiyon
    /// kendi JOIN'lerini kurar; burada ek bir Include zinciri gerekmez.
    /// Yalnızca hâlâ bellek içi çalışan
    /// <see cref="AuditableRepository{TEntity,TContext}.GetAllWithAudit"/>
    /// çağrıları için navigasyonlar yüklenir.
    /// </summary>
    protected override IQueryable<Invoice> ApplyDetailIncludes(IQueryable<Invoice> query)
        => query
            .AsSplitQuery()
            .Include(i => i.Customer)
            .Include(i => i.Supplier)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product);

    public async Task<List<InvoiceLine>> GetLinesByInvoiceIdsAsync(
        IReadOnlyCollection<Guid> invoiceIds,
        CancellationToken cancellationToken = default)
    {
        List<IdentityId> ids = [.. invoiceIds.Select(id => new IdentityId(id))];
        if (ids.Count == 0)
        {
            return [];
        }

        return await this.Context.Set<InvoiceLine>()
            .AsNoTracking()
            .Where(l => ids.Contains(l.InvoiceId))
            .Include(l => l.Product)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetByIdWithLinesAsync(
        IdentityId id,
        CancellationToken cancellationToken = default)
        => await this.Context.Set<Invoice>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Customer)
            .Include(i => i.Supplier)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
}
