using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class CurrentAccountMovementRepository : AuditableRepository<CurrentAccountMovement, ApplicationDbContext>, ICurrentAccountMovementRepository
{
    public CurrentAccountMovementRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// GetAllWithAudit sorgusu sonuçları belleğe materyalize ettiği için cari
    /// adı için müşteri/tedarikçi navigasyonlarının <c>Include</c> ile
    /// yüklenmesi gerekir; aksi halde "Cari Adı" sütunu boş gelir.
    /// </summary>
    protected override IQueryable<CurrentAccountMovement> ApplyDetailIncludes(IQueryable<CurrentAccountMovement> query)
        => query
            .Include(m => m.Customer)
            .Include(m => m.Supplier);
}
