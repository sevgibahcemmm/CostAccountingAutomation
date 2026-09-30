using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class StockIssueRepository : AuditableRepository<StockIssue, ApplicationDbContext>, IStockIssueRepository
{
    public StockIssueRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// Atölye transferi listesi kaynak/target hesap ve satır bilgilerini
    /// gösterir; denetimli liste sorgusu bellekte materyalize edildiği için bu
    /// navigasyonlar <c>Include</c> ile yüklenmelidir.
    /// </summary>
    protected override IQueryable<StockIssue> ApplyDetailIncludes(IQueryable<StockIssue> query)
        => query
            .Include(x => x.SourceWarehouse)
            .Include(x => x.TargetAccount)
            .AsSplitQuery()
            .Include(x => x.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p!.ProductUnitType);

    public Task<StockIssue?> GetWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => this.Context.Set<StockIssue>()
            .Include(x => x.SourceWarehouse)
            .Include(x => x.TargetAccount)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p!.ProductUnitType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}