using Cost.Accounting.Automation.Application.WorkshopAnalysis;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

/// <summary>
/// Atölye gelir/gider analizinin salt-okunur sorgu uygulaması.
///
/// <para><b>EF çeviri kuralı:</b> <see cref="IdentityId"/> ve
/// <c>AccountCode</c> value converter ile eşlendiği için bunların
/// <c>.Value</c> alanı SQL'e çevrilmez; sorgu içinde yalnızca <c>Id</c> /
/// <c>Code</c> gibi <em>kendi</em> alanlarına erişilir, <c>.Value</c>
/// okuması sonuçları bellekte yapılır. Buna karşılık <c>Name</c> owned
/// entity olduğu için <c>a.Name.Value</c> doğrudan SQL'e iner.</para>
///
/// <para><b>Hesap kuralları:</b></para>
/// <list type="bullet">
///   <item>Yalnızca ONAYLI kayıtlar sayılır. Taslak maliyet pusulası harcanmış
///   gider değildir; taslak fatura satış gerçekleşmemiştir.</item>
///   <item>Gelir, satılan ürünü en son üreten atölyeye yazılır; eldeki malın
///   maliyet kaynağı son üretimdir.</item>
///   <item>Hiçbir atölyede üretilmemiş ürünün satışı atölyeye bağlanmaz,
///   "atfiyon dışı gelir" olarak ayrıca raporlanır.</item>
/// </list>
/// </summary>
internal sealed class WorkshopAnalysisReadRepository(
    IDbContextFactory<ApplicationDbContext> dbFactory)
    : IWorkshopAnalysisReadRepository
{
    /// <summary>
    /// Aylık gider ara sonucu. Value object'ler bellekte çözülmek üzere taşınır.
    /// </summary>
    private sealed record ExpenseRow(
        IdentityId WorkshopId,
        ExpenseAccountType AccountType,
        int Year,
        int Month,
        decimal Amount);

    /// <summary>Satır bazlı ham gelir. Aylık netleştirme bellekte yapılır.</summary>
    private sealed record RevenueRow(
        IdentityId? WorkshopId,
        int Year,
        int Month,
        InvoiceType InvoiceType,
        decimal Amount);

    public async Task<IReadOnlyList<WorkshopOption>> GetWorkshopOptionsAsync(
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // Code bir value converter'a sahip olduğu için OrderBy'de ".Value"
        // değil, doğrudan Code kullanılır.
        List<ChartOfAccount> accounts = await db.Set<ChartOfAccount>()
            .AsNoTracking()
            .Where(a => a.Type == ChartOfAccountType.Workshop && !a.IsDeleted)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);

        // Genel sayfa her zaman ulaşılabilir olmalı; lookup'ta ilk satır
        // "(Tüm Atölyeler)" olarak eklenir.
        return
        [
            WorkshopOption.All,
            .. accounts.Select(a => new WorkshopOption(a.Id.Value, a.Code.Value, a.Name.Value))
        ];
    }

    public async Task<IReadOnlyList<WorkshopExpenseFact>> GetExpenseFactsAsync(
        DateOnly from,
        DateOnly to,
        Guid? workshopId,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<CostSlip> slips = db.Set<CostSlip>()
            .AsNoTracking()
            .Where(s =>
                !s.IsDeleted
                && s.Status == CostSlipStatus.Approved
                && s.CostDate >= from
                && s.CostDate <= to);

        if (workshopId is Guid id)
        {
            // IdentityId, Guid ile karşılaştırılırken nesne üzerinden
            // eşleştirilir; yoksa SQL'e çevrilemez.
            slips = slips.Where(s => s.WorkshopId == new IdentityId(id));
        }

        List<ExpenseRow> rows = await slips
            .SelectMany(s => s.CostSlipItems, (s, i) => new { s, i })
            .Where(x => !x.i.IsDeleted)
            .GroupBy(x => new
            {
                x.s.WorkshopId,
                x.i.ExpenseAccountType,
                Year = x.s.CostDate.Year,
                Month = x.s.CostDate.Month
            })
            .Select(g => new ExpenseRow(
                g.Key.WorkshopId,
                g.Key.ExpenseAccountType,
                g.Key.Year,
                g.Key.Month,
                g.Sum(x => x.i.Quantity * x.i.UnitPrice)))
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(r => new WorkshopExpenseFact(
                r.WorkshopId.Value,
                r.AccountType,
                r.Year,
                r.Month,
                r.Amount))
        ];
    }

    public async Task<IReadOnlyList<WorkshopRevenueFact>> GetRevenueFactsAsync(
        DateOnly from,
        DateOnly to,
        Guid? workshopId,
        CancellationToken cancellationToken)
    {
        await using ApplicationDbContext db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // Ürün -> üreten atölye. Aynı ürün birden fazla atölyede üretilmiş
        // olabilir; maliyeti belirleyen son üretim esas alınır.
        var producingWorkshop = db.Set<CostSlipItem>()
            .AsNoTracking()
            .Where(i => !i.IsDeleted && i.ProductId != null)
            .Join(
                db.Set<CostSlip>().AsNoTracking()
                    .Where(s => !s.IsDeleted && s.Status == CostSlipStatus.Approved),
                i => i.CostSlipId,
                s => s.Id,
                (i, s) => new
                {
                    ProductId = i.ProductId!,
                    s.WorkshopId,
                    s.CostDate
                })
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                WorkshopId = g
                    .OrderByDescending(x => x.CostDate)
                    .Select(x => x.WorkshopId)
                    .First()
            });

        // Onaylı satış faturası satırları. Satış iadeleri net geliri düşürür.
        var revenueLines = db.Set<InvoiceLine>()
            .AsNoTracking()
            .Where(l => !l.IsDeleted)
            .Join(
                db.Set<Invoice>().AsNoTracking()
                    .Where(i =>
                        !i.IsDeleted
                        && i.Status == InvoiceStatus.Approved
                        && i.Date >= from
                        && i.Date <= to),
                l => l.InvoiceId,
                i => i.Id,
                (l, i) => new
                {
                    l.ProductId,
                    l.TotalAmount,
                    i.Date,
                    i.InvoiceType
                });

        // Atölyeye bağlanamayan satırlar da korunur (null workshop).
        var joined = from line in revenueLines
                     join w in producingWorkshop
                         on line.ProductId equals w.ProductId
                     into attributed
                     from w in attributed.DefaultIfEmpty()
                     select new RevenueRow(
                         w.WorkshopId,
                         line.Date.Year,
                         line.Date.Month,
                         line.InvoiceType,
                         line.TotalAmount);

        List<RevenueRow> rows = await joined.ToListAsync(cancellationToken);

        return Collapse(rows, workshopId);
    }

    /// <summary>
    /// Aylık satır toplamlarını net gelire çevirir: <c>Sales</c> toplanır,
    /// <c>SalesReturn</c> düşülür. Alış faturaları gelir değildir ve
    /// filtrelenir. Sonuç ay başına tek kayıttır.
    /// </summary>
    private static IReadOnlyList<WorkshopRevenueFact> Collapse(
        List<RevenueRow> rows,
        Guid? workshopId)
    {
        IdentityId? selected = workshopId is Guid id ? new IdentityId(id) : null;

        Dictionary<(IdentityId? Workshop, int Year, int Month), (decimal Revenue, decimal Return)> buckets = [];

        foreach (RevenueRow row in rows)
        {
            bool isRevenue = row.InvoiceType == InvoiceType.Sales;
            bool isReturn = row.InvoiceType == InvoiceType.SalesReturn;

            if (!isRevenue && !isReturn)
            {
                continue;
            }

            (IdentityId? Workshop, int Year, int Month) key = (row.WorkshopId, row.Year, row.Month);

            buckets.TryGetValue(key, out (decimal Revenue, decimal Return) current);

            buckets[key] = isRevenue
                ? (current.Revenue + row.Amount, current.Return)
                : (current.Revenue, current.Return + row.Amount);
        }

        List<WorkshopRevenueFact> facts = [];

        foreach (KeyValuePair<(IdentityId? Workshop, int Year, int Month), (decimal Revenue, decimal Return)> bucket
            in buckets)
        {
            (IdentityId? Workshop, int Year, int Month) key = bucket.Key;
            decimal amount = bucket.Value.Revenue - bucket.Value.Return;

            if (amount == 0m)
            {
                continue;
            }

            // Tek atölye seçildiyse atfiyon dışı gelir ve diğer atölyeler
            // kapsam dışıdır; genel sayfada atfiyon dışı gelir korunur.
            if (selected is not null && key.Workshop != selected)
            {
                continue;
            }

            facts.Add(new WorkshopRevenueFact(
                key.Workshop?.Value,
                key.Year,
                key.Month,
                amount));
        }

        return facts;
    }
}