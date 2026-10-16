
namespace Cost.Accounting.Automation.Domain.ChartOfAccounts;

public interface IChartOfAccountLedgerRepository
{
    Task AddAsync(ChartOfAccountLedger entry, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<ChartOfAccountLedger> entries, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, (decimal Debit, decimal Credit)>> GetTotalsByAccountAsync(CancellationToken cancellationToken = default);

    Task<List<ChartOfAccountLedger>> GetBySourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen kaynak türü ve kimlik listesine ait tüm yevmiye satırlarını
    /// <b>tek</b> sorguda getirir.
    /// </summary>
    /// <remarks>
    /// Kaynak kimliği listesi döngüde tek tek okunduğunda sorgu sayısı kayıt
    /// sayısıyla doğrusal büyür. Bu metot sorgu sayısını seçim büyüklüğünden
    /// bağımsız tutar.
    /// </remarks>
    Task<List<ChartOfAccountLedger>> GetBySourcesAsync(
        string sourceType,
        IEnumerable<Guid> sourceIds,
        CancellationToken cancellationToken = default);

    Task<HashSet<string>> GetExistingSourceKeysAsync(CancellationToken cancellationToken = default);

    void SoftDeleteRange(IEnumerable<ChartOfAccountLedger> entries);

    void RestoreRange(IEnumerable<ChartOfAccountLedger> entries);
}