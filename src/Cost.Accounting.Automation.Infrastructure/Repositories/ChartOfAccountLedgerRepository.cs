
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class ChartOfAccountLedgerRepository : IChartOfAccountLedgerRepository
{
    private readonly ApplicationDbContext _context;

    public ChartOfAccountLedgerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ChartOfAccountLedger entry, CancellationToken cancellationToken = default)
    {
        await _context.Set<ChartOfAccountLedger>().AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<ChartOfAccountLedger> entries, CancellationToken cancellationToken = default)
    {
        List<ChartOfAccountLedger> list = entries.ToList();
        if (list.Count == 0) return;

        await _context.Set<ChartOfAccountLedger>().AddRangeAsync(list, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, (decimal Debit, decimal Credit)>> GetTotalsByAccountAsync(CancellationToken cancellationToken = default)
    {
        var grouped = await _context.Set<ChartOfAccountLedger>()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.ChartOfAccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Debit = g.Sum(x => x.DebitAmount),
                Credit = g.Sum(x => x.CreditAmount)
            })
            .ToListAsync(cancellationToken);

        return grouped.ToDictionary(x => x.AccountId.Value, x => (x.Debit, x.Credit));
    }

    public async Task<List<ChartOfAccountLedger>> GetBySourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default)
    {
        IdentityId id = new(sourceId);
        return await _context.Set<ChartOfAccountLedger>()
            .Where(x => x.SourceType == sourceType && x.SourceId == id)
            .ToListAsync(cancellationToken);
    }

    public async Task<HashSet<string>> GetExistingSourceKeysAsync(CancellationToken cancellationToken = default)
    {
        var keys = await _context.Set<ChartOfAccountLedger>()
            .Where(x => !x.IsDeleted && x.SourceId != null)
            .Select(x => x.SourceType + "|" + x.SourceId!.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return keys.ToHashSet();
    }

    public void SoftDeleteRange(IEnumerable<ChartOfAccountLedger> entries)
    {
        foreach (var entry in entries)
        {
            entry.Delete();
        }
        _context.Set<ChartOfAccountLedger>().UpdateRange(entries);
    }

    public void RestoreRange(IEnumerable<ChartOfAccountLedger> entries)
    {
        foreach (var entry in entries)
        {
            entry.Restore();
        }
        _context.Set<ChartOfAccountLedger>().UpdateRange(entries);
    }
}
