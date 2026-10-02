using Cost.Accounting.Automation.Domain.CostSlips;

namespace Cost.Accounting.Automation.Application.WorkshopAnalysis;

/// <summary>
/// Atölye gelir/gider analizinin salt-okunur sorgu uygulaması.
///
/// Her metot kendi kısa ömürlü <c>DbContext</c>'ini açar; gruplama ve
/// toplamlar SQL'e aşağı iner, belleğe yalnızca sonuç kümesi kadar satır gelir.
/// </summary>
public interface IWorkshopAnalysisReadRepository
{
    /// <summary>Lookup için tüm aktif atölye hesapları.</summary>
    Task<IReadOnlyList<WorkshopOption>> GetWorkshopOptionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Dönemdeki onaylı pusula giderlerini atölye ve hesap tipi bazında toplar.
    /// </summary>
    Task<IReadOnlyList<WorkshopExpenseFact>> GetExpenseFactsAsync(
        DateOnly from,
        DateOnly to,
        Guid? workshopId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Dönemdeki onaylı satış faturası satırlarını, satılan ürünün en son
    /// üretim atölyesiyle eşleştirilmiş hâlde döner.
    /// </summary>
    Task<IReadOnlyList<WorkshopRevenueFact>> GetRevenueFactsAsync(
        DateOnly from,
        DateOnly to,
        Guid? workshopId,
        CancellationToken cancellationToken);
}

/// <summary>Atölye + hesap tipi bazında gider.</summary>
public sealed record WorkshopExpenseFact(
    Guid WorkshopId,
    ExpenseAccountType AccountType,
    int Year,
    int Month,
    decimal Amount);

/// <summary>
/// Atölyeye bağlanmış satış geliri. <c>WorkshopId</c> null ise ürün hiçbir
/// atölyede üretilmemiştir ve gelir atölyelere dağıtılmaz.
/// </summary>
public sealed record WorkshopRevenueFact(
    Guid? WorkshopId,
    int Year,
    int Month,
    decimal Amount);