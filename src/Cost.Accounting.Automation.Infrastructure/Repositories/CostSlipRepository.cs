using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class CostSlipRepository : AuditableRepository<CostSlip, ApplicationDbContext>, ICostSlipRepository
{
    public CostSlipRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// Maliyet pusulası listesi atölye, ürün, müşteri ve satır bilgilerini
    /// gösterir; denetimli liste sorgusu bellekte materyalize edildiği için bu
    /// navigasyonlar <c>Include</c> ile yüklenmelidir.
    /// </summary>
    protected override IQueryable<CostSlip> ApplyDetailIncludes(IQueryable<CostSlip> query)
        => query
            .Include(x => x.Workshop)
            .Include(x => x.Customer)
            .AsSplitQuery()
            .Include(x => x.ProducedProduct)
                .ThenInclude(p => p!.ProductUnitType)
            .Include(x => x.CostSlipItems)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p!.ProductUnitType)
            .Include(x => x.CostSlipItems)
                .ThenInclude(l => l.ProductUnitType);

    public Task<CostSlip?> GetWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => this.Context.Set<CostSlip>()
            .Include(x => x.Workshop)
            .Include(x => x.ProducedProduct)
                .ThenInclude(p => p!.ProductUnitType)
            .Include(x => x.Customer)
            .Include(x => x.CostSlipItems)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p!.ProductUnitType)
            .Include(x => x.CostSlipItems)
                .ThenInclude(l => l.ProductUnitType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}