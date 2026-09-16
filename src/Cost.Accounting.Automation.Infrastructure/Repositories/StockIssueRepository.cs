using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class StockIssueRepository : AuditableRepository<StockIssue, ApplicationDbContext>, IStockIssueRepository
{
    public StockIssueRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<StockIssue?> GetWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => Context.Set<StockIssue>()
            .Include(x => x.SourceWarehouse)
            .Include(x => x.TargetAccount)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p!.ProductUnitType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
