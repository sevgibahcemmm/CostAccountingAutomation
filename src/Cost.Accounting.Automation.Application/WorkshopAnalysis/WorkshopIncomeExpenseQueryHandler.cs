using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.CostSlips;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.WorkshopAnalysis;

/// <summary>
/// Atölye gelir/gider analizi sorgusunu işler.
///
/// Sonuçlar kısa süreli bellek cache'ine yazılır; lookup veya tarih
/// değişimi aynı aralık için sorguyu tekrar tetiklemez.
/// </summary>
internal sealed class WorkshopIncomeExpenseQueryHandler(
    IWorkshopAnalysisReadRepository readRepository,
    IMemoryCache cache,
    ILogger<WorkshopIncomeExpenseQueryHandler> logger)
    : IRequestHandler<WorkshopIncomeExpenseQuery, Result<WorkshopAnalysisSnapshot>>
{
    private const string CachePrefix = "workshop:analysis:v1";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(45);

    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public async Task<Result<WorkshopAnalysisSnapshot>> Handle(
        WorkshopIncomeExpenseQuery request,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"{CachePrefix}:{request.From:yyyyMMdd}:{request.To:yyyyMMdd}:{request.WorkshopId?.ToString() ?? "all"}";

        if (cache.TryGetValue(cacheKey, out WorkshopAnalysisSnapshot? cached) && cached is not null)
        {
            return Result<WorkshopAnalysisSnapshot>.Succeed(cached);
        }

        // Lookup'taki "(Tüm Atölyeler)" satırı Guid.Empty taşır; bu da genel sayfa
        // demektir.
        Guid? scopeId = request.WorkshopId is Guid raw && raw != Guid.Empty ? raw : null;

        // Tarih aralığı ters yazılmışsa kullanıcıyı şaşırtmamak adına düzeltilir.
        DateOnly from = request.From;
        DateOnly to = request.To;

        if (from > to)
        {
            (from, to) = (to, from);
        }

        IReadOnlyList<WorkshopOption> options =
            await readRepository.GetWorkshopOptionsAsync(cancellationToken);

        IReadOnlyList<WorkshopExpenseFact> expenses = await SafeAsync(
            () => readRepository.GetExpenseFactsAsync(from, to, scopeId, cancellationToken),
            []);

        IReadOnlyList<WorkshopRevenueFact> revenues = await SafeAsync(
            () => readRepository.GetRevenueFactsAsync(from, to, scopeId, cancellationToken),
            []);

        WorkshopAnalysisSnapshot snapshot = Build(from, to, scopeId, options, expenses, revenues);

        cache.Set(cacheKey, snapshot, CacheTtl);

        return Result<WorkshopAnalysisSnapshot>.Succeed(snapshot);
    }

    private WorkshopAnalysisSnapshot Build(
        DateOnly from,
        DateOnly to,
        Guid? workshopId,
        IReadOnlyList<WorkshopOption> options,
        IReadOnlyList<WorkshopExpenseFact> expenses,
        IReadOnlyList<WorkshopRevenueFact> revenues)
    {
        Dictionary<Guid, (decimal Revenue, decimal Expense)> totals = [];

        void Accumulate(Guid? key, decimal revenue, decimal expense)
        {
            if (key is not Guid id)
            {
                return;
            }

            totals.TryGetValue(id, out (decimal Revenue, decimal Expense) current);
            totals[id] = (current.Revenue + revenue, current.Expense + expense);
        }

        foreach (WorkshopRevenueFact fact in revenues)
        {
            Accumulate(fact.WorkshopId, fact.Amount, 0m);
        }

        foreach (WorkshopExpenseFact fact in expenses)
        {
            Accumulate(fact.WorkshopId, 0m, fact.Amount);
        }

        Dictionary<Guid, WorkshopOption> optionById =
            options.ToDictionary(o => o.WorkshopId);

        List<WorkshopAnalysisRow> rows =
        [
            .. totals
                .Where(t => optionById.ContainsKey(t.Key))
                .Select(t => new WorkshopAnalysisRow(
                    t.Key,
                    optionById[t.Key].Code,
                    optionById[t.Key].Name,
                    Revenue: t.Value.Revenue,
                    Expense: t.Value.Expense))
                // Hareketi olmayan atölyeler karşılaştırma grafiğini
                // 178 boş çubukla dolduracağı için listeye alınmaz.
                .Where(r => r.Revenue != 0m || r.Expense != 0m)
                .OrderByDescending(r => r.Expense)
                .ThenByDescending(r => r.Revenue)
        ];

        decimal totalRevenue = rows.Sum(r => r.Revenue);
        decimal totalExpense = rows.Sum(r => r.Expense);

        decimal unattributed = revenues
            .Where(r => r.WorkshopId is null)
            .Sum(r => r.Amount);

        // Tek atölye seçildiyse yalnızca o atölyenin satırı gösterilir; genel
        // sayfa seçildiyse tüm hareketli atölyeler listelenir.
        if (workshopId is Guid single
            && rows.FirstOrDefault(r => r.WorkshopId == single) is WorkshopAnalysisRow only)
        {
            rows = [only];
        }
        else if (workshopId is not null)
        {
            rows = [];
        }

        return new WorkshopAnalysisSnapshot
        {
            From = from,
            To = to,
            IsAllWorkshops = workshopId is null,
            ScopeTitle = workshopId is Guid id && optionById.TryGetValue(id, out WorkshopOption? option)
                ? option.DisplayName
                : "Tüm Atölyeler",
            TotalRevenue = totalRevenue,
            TotalExpense = totalExpense,
            UnattributedRevenue = unattributed,
            Workshops = rows,
            MonthlyTrend = BuildTrend(from, to, revenues, expenses),
            ExpenseBreakdown = BuildExpenseBreakdown(expenses),
            WorkshopOptions = options
        };
    }

    /// <summary>
    /// Aylık gelir/gider serisi. Aralıktaki <b>her ay</b> üretilir; ayda
    /// hareket olmasa da grafikte boşluk görünür, aksi hâlde grafik
    /// hareketli olmayan ayları atlayıp yanıltıcı bir süreklilik gösterirdi.
    /// </summary>
    private static IReadOnlyList<WorkshopTrendPoint> BuildTrend(
        DateOnly from,
        DateOnly to,
        IReadOnlyList<WorkshopRevenueFact> revenues,
        IReadOnlyList<WorkshopExpenseFact> expenses)
    {
        Dictionary<(int Year, int Month), decimal> revenueByMonth = [];
        Dictionary<(int Year, int Month), decimal> expenseByMonth = [];

        foreach (WorkshopRevenueFact fact in revenues)
        {
            (int Year, int Month) key = (fact.Year, fact.Month);
            revenueByMonth.TryGetValue(key, out decimal current);
            revenueByMonth[key] = current + fact.Amount;
        }

        foreach (WorkshopExpenseFact fact in expenses)
        {
            (int Year, int Month) key = (fact.Year, fact.Month);
            expenseByMonth.TryGetValue(key, out decimal current);
            expenseByMonth[key] = current + fact.Amount;
        }

        List<WorkshopTrendPoint> points = [];

        DateOnly cursor = new(from.Year, from.Month, 1);
        DateOnly last = new(to.Year, to.Month, 1);

        // Aşırı geniş aralıkta (örn. 10 yıl) nokta sayısı grafiği boğar.
        const int MaxMonths = 60;
        int guard = 0;

        while (cursor <= last && guard < MaxMonths)
        {
            (int Year, int Month) key = (cursor.Year, cursor.Month);

            revenueByMonth.TryGetValue(key, out decimal revenue);
            expenseByMonth.TryGetValue(key, out decimal expense);

            points.Add(new WorkshopTrendPoint(
                cursor.ToString("MMM yy", Tr),
                revenue,
                expense));

            cursor = cursor.AddMonths(1);
            guard++;
        }

        return points;
    }

    /// <summary>
    /// Giderleri hesap tipine göre toplar. Etiketler
    /// <c>[Display(Name=...)]</c> özniteliklerinden gelir; alınamazsa
    /// enum adı kullanılır.
    /// </summary>
    private static IReadOnlyList<ExpenseBreakdownPoint> BuildExpenseBreakdown(
        IReadOnlyList<WorkshopExpenseFact> expenses)
    {
        Dictionary<ExpenseAccountType, decimal> byAccount = [];

        foreach (WorkshopExpenseFact fact in expenses)
        {
            byAccount.TryGetValue(fact.AccountType, out decimal current);
            byAccount[fact.AccountType] = current + fact.Amount;
        }

        return
        [
            .. byAccount
                .Where(kv => kv.Value != 0m)
                .Select(kv => new ExpenseBreakdownPoint(
                    LabelFor(kv.Key),
                    kv.Value,
                    kv.Key))
                .OrderByDescending(p => p.Amount)
        ];
    }

    private static string LabelFor(ExpenseAccountType account)
    {
        string label = EnumDisplay.GetDisplayName(account);

        return string.IsNullOrWhiteSpace(label) ? account.ToString() : label;
    }

    /// <summary>
    /// Alt sorgu hata verirse analiz boş döner; dashboard'un tamamının
    /// patlamasındansa tek bölümün eksik kalması yeğdir.
    /// </summary>
    private async Task<T> SafeAsync<T>(Func<Task<T>> factory, T fallback)
    {
        try
        {
            return await factory().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Atölye analizi alt sorgusu başarısız");
            return fallback;
        }
    }
}

/// <summary>Lookup listesini döndüren hafif sorgunun işleyicisi.</summary>
internal sealed class WorkshopOptionsQueryHandler(
    IWorkshopAnalysisReadRepository readRepository)
    : IRequestHandler<WorkshopOptionsQuery, Result<IReadOnlyList<WorkshopOption>>>
{
    public async Task<Result<IReadOnlyList<WorkshopOption>>> Handle(
        WorkshopOptionsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkshopOption> options =
            await readRepository.GetWorkshopOptionsAsync(cancellationToken);

        return Result<IReadOnlyList<WorkshopOption>>.Succeed(options);
    }
}