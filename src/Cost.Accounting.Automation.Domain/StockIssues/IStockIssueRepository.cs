using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.StockIssues;

public interface IStockIssueRepository : IAuditableRepository<StockIssue>
{
    Task<StockIssue?> GetWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default);
}
