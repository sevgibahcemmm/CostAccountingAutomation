using Cost.Accounting.Automation.Domain.ChartOfAccounts;
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
}