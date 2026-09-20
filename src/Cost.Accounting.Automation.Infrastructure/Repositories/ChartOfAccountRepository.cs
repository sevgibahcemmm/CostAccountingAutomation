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
    public ChartOfAccountRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<List<ChartOfAccount>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        => Context.Set<ChartOfAccount>().IgnoreQueryFilters().ToListAsync(cancellationToken);

    public async Task<AccountDeletionCheck> GetDeletionCheckAsync(IReadOnlyCollection<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> ids = accountIds.Select(id => new IdentityId(id)).ToHashSet();
        if (ids.Count == 0)
        {
            return new AccountDeletionCheck([], []);
        }

        HashSet<Guid> movementIds = new();
        HashSet<Guid> relatedIds = new();

        // Hareket: hesap üzerinde gerçekleşen işlem/fiş kayıtları (engellenir).
        List<IdentityId> ledgerAccounts = await Context.Set<ChartOfAccountLedger>()
            .Where(x => ids.Contains(x.ChartOfAccountId))
            .Select(x => x.ChartOfAccountId)
            .Distinct()
            .ToListAsync(cancellationToken);
        AddAll(movementIds, ledgerAccounts);

        // İlişkili (yapısal): hareket değil, yalnızca başka kayıtlar tarafından referans verilmiş.
        List<IdentityId> warehouseIds = await Context.Set<Product>()
            .Where(p => ids.Contains(p.WarehouseId))
            .Select(p => p.WarehouseId)
            .Distinct()
            .ToListAsync(cancellationToken);
        AddAll(relatedIds, warehouseIds);

        List<IdentityId> categoryIds = await Context.Set<Product>()
            .Where(p => ids.Contains(p.CategoryId))
            .Select(p => p.CategoryId)
            .Distinct()
            .ToListAsync(cancellationToken);
        AddAll(relatedIds, categoryIds);

        List<IdentityId?> chartAccountIds = await Context.Set<Product>()
            .Where(p => p.ChartOfAccountId != null)
            .Select(p => p.ChartOfAccountId)
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (IdentityId? id in chartAccountIds)
        {
            if (id is { } concreteId && ids.Contains(concreteId))
            {
                relatedIds.Add(concreteId.Value);
            }
        }

        List<IdentityId> workshopIds = await Context.Set<CostSlip>()
            .Where(c => ids.Contains(c.WorkshopId))
            .Select(c => c.WorkshopId)
            .Distinct()
            .ToListAsync(cancellationToken);
        AddAll(relatedIds, workshopIds);

        List<IdentityId> sourceWarehouseIds = await Context.Set<StockIssue>()
            .Where(s => ids.Contains(s.SourceWarehouseId))
            .Select(s => s.SourceWarehouseId)
            .Distinct()
            .ToListAsync(cancellationToken);
        AddAll(relatedIds, sourceWarehouseIds);

        List<IdentityId> targetAccountIds = await Context.Set<StockIssue>()
            .Where(s => ids.Contains(s.TargetAccountId))
            .Select(s => s.TargetAccountId)
            .Distinct()
            .ToListAsync(cancellationToken);
        AddAll(relatedIds, targetAccountIds);

        List<IdentityId?> consumptionRefs = await Context.Set<CostSlipItem>()
            .Where(i => i.ProductUnitTypeId != null)
            .Select(i => i.ProductUnitTypeId)
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (IdentityId? id in consumptionRefs)
        {
            if (id is { } refId && ids.Contains(refId))
            {
                relatedIds.Add(refId.Value);
            }
        }

        return new AccountDeletionCheck(movementIds.ToList(), relatedIds.ToList());
    }

    private static void AddAll(HashSet<Guid> target, IEnumerable<IdentityId> source)
    {
        foreach (IdentityId id in source)
        {
            target.Add(id.Value);
        }
    }
}