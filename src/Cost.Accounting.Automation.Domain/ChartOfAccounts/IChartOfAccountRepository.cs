using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.ChartOfAccounts;

public interface IChartOfAccountRepository : IAuditableRepository<ChartOfAccount>
{
    Task<List<ChartOfAccount>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
}