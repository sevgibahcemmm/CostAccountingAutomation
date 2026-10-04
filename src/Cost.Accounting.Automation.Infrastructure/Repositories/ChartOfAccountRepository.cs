using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ChartOfAccountRepository : AuditableRepository<ChartOfAccount, ApplicationDbContext>, IChartOfAccountRepository
{
    public ChartOfAccountRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    public Task<List<ChartOfAccount>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        => this.Context.Set<ChartOfAccount>().IgnoreQueryFilters().ToListAsync(cancellationToken);

    /// <summary>
    /// Seçilen hesapların tamamını <c>IN</c> listesiyle denetler; sorgu sayısı
    /// seçim büyüklüğünden bağımsızdır.
    ///
    /// <para>
    /// <b>EF çeviri kuralı:</b> <see cref="IdentityId"/> değer converter ile
    /// eşlendiği için yabancı anahtar sütunu bir <c>IN</c> listesiyle
    /// karşılaştırılırken liste elemanı <b>sütunun kendi tipinde</b>
    /// olmalıdır (nullable olmayan sütun → <c>List&lt;Guid&gt;</c>, nullable
    /// sütun → <c>List&lt;Guid?&gt;</c>). Sütunun <c>.Value</c> alanına
    /// <c>Contains</c> argümanı olarak erişmek SQL'e çevrilmez; <c>.Value</c>
    /// yalnızca projeksiyonda kullanılabilir.
    /// </para>
    /// </summary>
    public async Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default)
    {
        List<Guid> keys = [.. accountIds];
        if (keys.Count == 0)
        {
            return DeletionCheck.Empty;
        }

        List<Guid?> nullableKeys = [.. accountIds.Select(id => (Guid?)id)];

        // Hareket: hesap üzerinde gerçekleşen işlem/fiş kayıtları (engellenir).
        List<Guid> movementIds = await this.Context.Set<ChartOfAccountLedger>()
            .Where(x => keys.Contains(x.ChartOfAccountId))
            .Select(x => x.ChartOfAccountId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        // İlişkili (yapısal): hareket değil, yalnızca başka kayıtlar tarafından
        // referans verilmiş. Silinir ama not üretir.
        List<Guid> warehouseIds = await this.Context.Set<Product>()
            .Where(p => keys.Contains(p.WarehouseId))
            .Select(p => p.WarehouseId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> categoryIds = await this.Context.Set<Product>()
            .Where(p => keys.Contains(p.CategoryId))
            .Select(p => p.CategoryId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> chartAccountIds = await this.Context.Set<Product>()
            .Where(p => p.ChartOfAccountId != null && nullableKeys.Contains(p.ChartOfAccountId))
            .Select(p => p.ChartOfAccountId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> workshopIds = await this.Context.Set<CostSlip>()
            .Where(c => keys.Contains(c.WorkshopId))
            .Select(c => c.WorkshopId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> sourceWarehouseIds = await this.Context.Set<StockIssue>()
            .Where(s => keys.Contains(s.SourceWarehouseId))
            .Select(s => s.SourceWarehouseId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> targetAccountIds = await this.Context.Set<StockIssue>()
            .Where(s => keys.Contains(s.TargetAccountId))
            .Select(s => s.TargetAccountId.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<Guid> unitTypeIds = await this.Context.Set<CostSlipItem>()
            .Where(i => i.ProductUnitTypeId != null && nullableKeys.Contains(i.ProductUnitTypeId))
            .Select(i => i.ProductUnitTypeId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new DeletionCheck(
            movementIds,
            [.. warehouseIds, .. categoryIds, .. chartAccountIds, .. workshopIds, .. sourceWarehouseIds, .. targetAccountIds, .. unitTypeIds]);
    }
}
