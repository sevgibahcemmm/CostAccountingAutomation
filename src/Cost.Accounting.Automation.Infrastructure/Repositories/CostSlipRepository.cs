using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
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
    /// Maliyet pusulası listesi atölye, ürün ve müşteri bilgilerini gösterir;
    /// denetimli liste sorgusu bellekte materyalize edildiği için bu
    /// navigasyonlar <c>Include</c> ile yüklenmelidir.
    /// </summary>
    /// <remarks>
    /// <c>CostSlipItems</c> bilinçli olarak <c>Include</c> edilmez ve
    /// <c>AsSplitQuery</c> kullanılmaz. Koleksiyon navigasyonu eklemek, liste
    /// projeksiyonunda kullanılmasa bile EF'in ayrı bir "split" komutu
    /// üretmesine yol açar ve liste yüklenmesini 25 saniyeye çıkarmıştır.
    /// Satırlar <see cref="GetItemsAsync"/> ile ayrı ve basit bir sorguda
    /// getirilir.
    /// </remarks>
    protected override IQueryable<CostSlip> ApplyDetailIncludes(IQueryable<CostSlip> query)
        => query
            .Include(x => x.Workshop)
            .Include(x => x.Customer)
            .Include(x => x.ProducedProduct)
                .ThenInclude(p => p!.ProductUnitType);

    public async Task<IReadOnlyList<CostSlipItem>> GetItemsAsync(
        IReadOnlyCollection<IdentityId> costSlipIds,
        CancellationToken cancellationToken = default)
    {
        if (costSlipIds.Count == 0)
        {
            return [];
        }

        List<Guid> ids = costSlipIds.Select(x => x.Value).ToList();

        return await this.Context.Set<CostSlipItem>()
            .Include(x => x.Product)
                .ThenInclude(p => p!.ProductUnitType)
            .Include(x => x.ProductUnitType)
            .Where(x => ids.Contains(x.CostSlipId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

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