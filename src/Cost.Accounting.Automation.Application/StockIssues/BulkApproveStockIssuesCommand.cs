using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

/// <summary>
/// Seçilen taslak atölye transferi / tüketim belgelerini TEK TRANSACTION
/// içinde toplu onaylar: hepsi onaylanır ya da hiçbiri.
///
/// Onay sırası belge tarihine göre eskiden yeniye belirlenir; böylece aynı
/// malzemeyi kullanan belgelerde stok kümülatif olarak doğru değerlendirilir.
/// Taslak belge stoğu etkilemediği için iki belgenin toplamı mevcut stoğu
/// aşıyorsa ikisi de birlikte reddedilir ve hiçbiri onaylanmaz.
/// </summary>
[Permission("stockissue:approve")]
public sealed record BulkApproveStockIssuesCommand(
    IReadOnlyList<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkApproveStockIssuesCommandHandler(
    IStockIssueRepository stockIssueRepository,
    IProductMovementRepository productMovementRepository,
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IChartOfAccountLedgerPoster ledgerPoster) : IRequestHandler<BulkApproveStockIssuesCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkApproveStockIssuesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
        {
            return Result<string>.Failure("Onaylanacak belge seçilmedi.");
        }

        List<IdentityId> ids = request.Ids.Select(i => new IdentityId(i)).ToList();

        List<StockIssue> issues = await stockIssueRepository.WhereWithTracking(i => ids.Contains(i.Id))
            .Include(i => i.Lines)
            .ToListAsync(cancellationToken);

        if (issues.Count == 0)
        {
            return Result<string>.Failure("Seçilen belgeler bulunamadı.");
        }

        List<StockIssue> ordered = issues
            .OrderBy(i => i.Date)
            .ThenBy(i => i.DocumentNumber)
            .ToList();

        List<string> failures = [];

        foreach (StockIssue issue in ordered)
        {
            if (issue.Status == StockIssueStatus.Approved)
            {
                failures.Add($"{issue.DocumentNumber}: zaten onaylanmış.");
            }
        }

        if (failures.Count > 0)
        {
            return BuildFailure(failures);
        }

        List<IdentityId> allProductIds = ordered
            .SelectMany(i => i.Lines)
            .Select(l => l.ProductId)
            .Distinct()
            .ToList();

        // En son belge tarihi SORGUNUN DIŞINDA hesaplanmalıdır. Bellekteki listeden
// Max(...) çağrısı EF Where ifadesinin içinde kalırsa
// "The LINQ expression 'i => i.Date' could not be translated" hatası verir.
DateOnly maxIssueDate = ordered.Max(i => i.Date);

        // IsDeleted filtresi ZORUNLUDUR. Silinen bir belgenin hareketleri
        // soft-delete edilir ve stoğa geri döner; filtrelenmezse hareketler
        // hem giriş hem çıkış olarak sayılır, mevcut stok yanlış (düşük)
        // hesaplanır ve yeterli giriş varken belge reddedilir.
        // StockIssueCostingHelper.LoadMovementsAsync da aynı filtreyi uygular.
        List<ProductMovement> existingMovements = await productMovementRepository.GetAll()
            .Where(m => allProductIds.Contains(m.ProductId)
                        && m.Date <= maxIssueDate
                        && !m.IsDeleted)
            .ToListAsync(cancellationToken);

        List<ChartOfAccount> accounts = await chartOfAccountRepository
            .GetAllIncludingDeletedAsync(cancellationToken);

        Dictionary<IdentityId, Product> productMap = (await productRepository.GetAll()
                .Where(p => allProductIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync(cancellationToken))
            .ToDictionary(p => p.Id);

        Dictionary<IdentityId, ChartOfAccount> accountById = accounts.ToDictionary(a => a.Id);

        // PlanStockEffects ürün kimliğiyle ürün hesabını eşler. Hesap kimliğiyle
        // eşlenen sözlük verilirse ürün tarafındaki (15x) borç kaydı sessizce
        // atlanır ve yevmiye tek taraflı kalır.
        Dictionary<IdentityId, ChartOfAccount> productAccountMap = productMap
            .Where(p => p.Value.ChartOfAccountId is not null
                        && accountById.ContainsKey(new IdentityId(p.Value.ChartOfAccountId.Value)))
            .ToDictionary(
                p => p.Key,
                p => accountById[new IdentityId(p.Value.ChartOfAccountId!.Value)]);

        // 1. AŞAMA — doğrulama. Hiçbir yazma yapılmadan, planlanmış çıkışlar
        // kümülatif stoktan düşülerek belgeler sırayla kontrol edilir.
        List<ProductMovement> accumulated = [];

        foreach (StockIssue issue in ordered)
        {
            Dictionary<IdentityId, decimal> requested = issue.Lines
                .GroupBy(l => l.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            List<ProductMovement> pool = new(existingMovements);
            pool.AddRange(accumulated);

            foreach (KeyValuePair<IdentityId, decimal> item in requested)
            {
                decimal available = StockIssueCostingHelper.ComputeAvailableQuantity(
                    pool,
                    item.Key,
                    issue.Date);

                if (item.Value > available)
                {
                    failures.Add(
                        $"{issue.DocumentNumber}: '{GetProductName(productMap, item.Key)}' için yeterli stok yok. "
                        + $"Mevcut: {available:n2}, istenen: {item.Value:n2}.");
                }
            }

            if (failures.Count > 0)
            {
                // Sonraki belgeleri kontrol etmeye gerek yok; sonuç zaten
                // onaylanamayacak.
                break;
            }

            // Katman kırılımı havuzdaki birikmiş çıkışları da görmelidir;
            // aksi hâlde belgeler aynı giriş katmanını defalarca tüketir.
            // Satırlar önce havuzla eşitlenir; belgedeki kalem ile yazılacak
            // hareketler tanım gereği aynı olur.
            StockIssueStockHelper.SyncLinesWithFifo(issue, pool);

            StockIssueStockPlan plan = StockIssueStockHelper.PlanStockEffects(
                issue,
                accountById.GetValueOrDefault(issue.TargetAccountId),
                productAccountMap,
                pool);
            accumulated.AddRange(plan.Movements);
        }

        if (failures.Count > 0)
        {
            return BuildFailure(failures);
        }

        // 2. AŞAMA — yazma. TransactionBehavior tek SaveChangesAsync çağıracak.
        // Havuz 1. aşamayla birebir aynı kurulur; planlar iki aşamada da aynı
        // hareketleri üretir.
        accumulated.Clear();

        foreach (StockIssue issue in ordered)
        {
            List<ProductMovement> pool = new(existingMovements);
            pool.AddRange(accumulated);

            StockIssueStockHelper.SyncLinesWithFifo(issue, pool);

            StockIssueStockPlan plan = StockIssueStockHelper.PlanStockEffects(
                issue,
                accountById.GetValueOrDefault(issue.TargetAccountId),
                productAccountMap,
                pool);

            await StockIssueStockHelper.WritePlanAsync(
                plan,
                productMovementRepository,
                ledgerPoster,
                cancellationToken);

            accumulated.AddRange(plan.Movements);

            issue.Approve();
        }

        return Result<string>.Succeed($"{ordered.Count} belge onaylandı; stok hareketleri oluşturuldu.");
    }

    private static string GetProductName(
        IReadOnlyDictionary<IdentityId, Product> productMap,
        IdentityId productId)
        => productMap.TryGetValue(productId, out Product? product)
            ? product.Name.Value
            : productId.Value.ToString();

    private static Result<string> BuildFailure(List<string> failures)
        => Result<string>.Failure(
            "Hiçbir belge onaylanmadı. Onaylanamayan kayıtlar:\n"
            + string.Join("\n", failures));
}