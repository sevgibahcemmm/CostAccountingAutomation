
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

    /// <summary>
    /// Yevmiye satırını değişiklik takibine ekler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Burada <c>SaveChangesAsync</c> çağrılmaz.</b> Kayıt yazımı MediatR
    /// işlem hattının sonunda (<c>TransactionBehavior</c>) tek seferde yapılır.
    /// Satır başına kaydetme, bir belgede onlarca yevmiye satırı olduğunda
    /// her satır için ayrı bir veritabanına gidiş-dönüş demekti; üstelik
    /// vaat edilen "her şey ya da hiçbir şey" bütünlüğünü bozuyordu: on
    /// satırlık bir belgede hata oluştuğunda ilk satırlar kayıtlı kalıyordu.
    /// </para>
    /// </remarks>
    public Task AddAsync(ChartOfAccountLedger entry, CancellationToken cancellationToken = default)
    {
        _context.Set<ChartOfAccountLedger>().Add(entry);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<ChartOfAccountLedger> entries, CancellationToken cancellationToken = default)
    {
        List<ChartOfAccountLedger> list = entries.ToList();
        if (list.Count == 0)
        {
            return Task.CompletedTask;
        }

        _context.Set<ChartOfAccountLedger>().AddRange(list);
        return Task.CompletedTask;
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

    public async Task<List<ChartOfAccountLedger>> GetBySourcesAsync(
        string sourceType,
        IEnumerable<Guid> sourceIds,
        CancellationToken cancellationToken = default)
    {
        // Nullable yabancı anahtar sütunu: EF'in IN listesini sütunun kendi
        // tipinde (Guid?) yazması gerekir. Liste elemanlarına IdentityId
        // sarmalamak SQL'e çevrilmez.
        List<Guid?> ids = [.. sourceIds.Select(id => (Guid?)id)];
        if (ids.Count == 0)
        {
            return [];
        }

        return await _context.Set<ChartOfAccountLedger>()
            .Where(x => x.SourceType == sourceType && ids.Contains(x.SourceId!.Value))
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
