using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Invoices;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.WorkshopAnalysis;

/// <summary>
/// Atölye gelir/gider analizinin tek okuma sorgusu.
///
/// <para><b>Gider</b> tanımı: onaylı maliyet pusulası kalemlerinin
/// <c>Quantity * UnitPrice</c> toplamıdır (710-780 + 151 gider hesapları).
/// Onaylanmamış pusulalar sayılmaz; onaylanmamış maliyet henüz harcanmış
/// gider değildir.</para>
///
/// <para><b>Gelir</b> tanımı: onaylı <b>SATIŞ</b> faturalarının satır
/// tutarlarıdır (Satış İade faturaları düşülerek). Faturada atölye alanı
/// bulunmadığı için satılan ürün, onu üreten atölyeye bağlanır:
/// <c>InvoiceLine.ProductId</c> → o ürünü üreten <b>en son onaylı</b> pusulanın
/// atölyesi. Birden fazla atölyede üretilmiş ürünlerde "en son üretim"
/// kuralı uygulanır, çünkü eldeki malın maliyetini o üretim belirler.</para>
///
/// <para>Hiçbir atölyede üretilmemiş ürünlerin satış geliri
/// <see cref="WorkshopAnalysisSnapshot.UnattributedRevenue"/> olarak ayrı
/// tutulur ve <see cref="WorkshopAnalysisSnapshot.Net"/> hesabına
/// katılmaz; aksi hâlde atölyelere ait olmayan gelir yanlışlıkla
/// kâra yazılırdı.</para>
/// </summary>
/// <param name="From">Başlangıç tarihi (dahil).</param>
/// <param name="To">Bitiş tarihi (dahil).</param>
/// <param name="WorkshopId">
/// null ise TÜM atölyeler genel sayfa gösterilir. Değer verilirse yalnızca
/// o atölyenin verileri döner.
/// </param>
[Permission("costslip:view")]
public sealed record WorkshopIncomeExpenseQuery(
    DateOnly From,
    DateOnly To,
    Guid? WorkshopId = null) : IRequest<Result<WorkshopAnalysisSnapshot>>;

/// <summary>Atölye gelir/gider analizinin tam sonucu.</summary>
public sealed record WorkshopAnalysisSnapshot
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }

    /// <summary>true ise genel sayfa (tüm atölyeler), false ise tek atölye.</summary>
    public bool IsAllWorkshops { get; init; }

    public string ScopeTitle { get; init; } = "Tüm Atölyeler";

    /// <summary>Dönemdeki onaylı SATIŞ faturası satır tutarı (iade düşülmüş).</summary>
    public decimal TotalRevenue { get; init; }

    /// <summary>Dönemdeki onaylı pusula gider kalemleri toplamı.</summary>
    public decimal TotalExpense { get; init; }

    /// <summary>Hiçbir atölyeye bağlanamayan satış geliri.</summary>
    public decimal UnattributedRevenue { get; init; }

    /// <summary>Gelir - Gider. Atölyelere bağlanan gelir esas alınır.</summary>
    public decimal Net => TotalRevenue - TotalExpense;

    /// <summary>Giderin gelire oranı (%). Gider gelirden büyükse 100+ olur.</summary>
    public decimal ExpenseRatio => TotalRevenue <= 0m ? 0m : TotalExpense / TotalRevenue * 100m;

    /// <summary>Atölye bazlı karşılaştırma satırları (yalnızca verisi olanlar).</summary>
    public IReadOnlyList<WorkshopAnalysisRow> Workshops { get; init; } = [];

    /// <summary>Aylık gelir/gider serisi.</summary>
    public IReadOnlyList<WorkshopTrendPoint> MonthlyTrend { get; init; } = [];

    /// <summary>Gider hesap tipine göre dağılım.</summary>
    public IReadOnlyList<ExpenseBreakdownPoint> ExpenseBreakdown { get; init; } = [];

    /// <summary>Lookup listesini doldurmak için tüm atölyeler.</summary>
    public IReadOnlyList<WorkshopOption> WorkshopOptions { get; init; } = [];
}

/// <summary>Bir atölyenin dönem içi gelir/gider özeti.</summary>
public sealed record WorkshopAnalysisRow(
    Guid WorkshopId,
    string Code,
    string Name,
    decimal Revenue,
    decimal Expense)
{
    public decimal Net => Revenue - Expense;
}

/// <summary>Aylık kırılım noktası.</summary>
public sealed record WorkshopTrendPoint(string Label, decimal Revenue, decimal Expense)
{
    public decimal Net => Revenue - Expense;
}

/// <summary>Gider hesap tipi dağılımı.</summary>
public sealed record ExpenseBreakdownPoint(string Label, decimal Amount, ExpenseAccountType AccountType);

/// <summary>Lookup satırı. <see cref="Code"/> yol göstergesi olarak kullanılır.</summary>
public sealed record WorkshopOption(Guid WorkshopId, string Code, string Name)
{
    /// <summary>Genel sayfayı seçen sentetik satırın kimliği.</summary>
    public static readonly Guid AllWorkshopsId = Guid.Empty;

    /// <summary>Lookup'ın en üstünde görünen "tümü" satırı.</summary>
    public static WorkshopOption All { get; } =
        new(AllWorkshopsId, "", "(Tüm Atölyeler)");

    public string DisplayName
    {
        get
        {
            // "(Tüm Atölyeler)" satırının kodu boştur; boş kodu "-" ile
            // birleştirmek lookup'ta " - (Tüm Atölyeler)" gibi bozuk metin üretir.
            if (string.IsNullOrWhiteSpace(Code))
            {
                return Name;
            }

            return string.IsNullOrWhiteSpace(Name) ? Code : $"{Code} - {Name}";
        }
    }

    /// <summary>Lookup'ta genel sayfa satırı mı?</summary>
    public bool IsAll => WorkshopId == AllWorkshopsId;
}

/// <summary>Lookup doldurmak için yalnızca atölye listesi isteyen hafif sorgu.</summary>
[Permission("chartofaccount:view")]
public sealed record WorkshopOptionsQuery : IRequest<Result<IReadOnlyList<WorkshopOption>>>;

/// <summary>
/// Fatura türü seçimi: gelir yalnızca satış tarafindan gelir.
/// Satış İade, Satış'ın tersidir ve net geliri düşürür.
/// </summary>
internal static class WorkshopRevenueTypes
{
    public static readonly InvoiceType[] Included = [InvoiceType.Sales];

    public static readonly InvoiceType[] Excluded = [InvoiceType.SalesReturn];
}