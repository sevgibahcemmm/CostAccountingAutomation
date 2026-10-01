using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

/// <summary>
/// Seçilen maliyet pusulalarını TEK TRANSACTION içinde toplu onaylar.
/// Kullanıcı seçtiği kayıtların hepsi onaylanır ya da HİÇBİRİ onaylanmaz;
/// biri stok yetersizliği nedeniyle düşerse önceki onaylar geri alınmaz çünkü
/// hiçbiri yazılmamış olur.
///
/// Pusulalar kayıt (maliyet) tarihine göre eskiden yeniye planlanır. Her pusula
/// planlanırken öncekilerin tükettiği miktar da hesaba katılır; böylece aynı
/// ambalaj malzemesini kullanan beş pusula birlikte onaylandığında stok
/// kümülatif doğru değerlendirilir.
/// </summary>
[Permission("costslip:approve")]
public sealed record BulkApproveCostSlipsCommand(
    IReadOnlyList<Guid> Ids,
    StockCostingMethod CostingMethod = StockCostingMethod.Fifo) : IRequest<Result<string>>;



internal sealed class BulkApproveCostSlipsCommandHandler(
    ICostSlipRepository costSlipRepository,
    IProductMovementRepository productMovementRepository,
    IProductRepository productRepository,
    IChartOfAccountLedgerPoster ledgerPoster) : IRequestHandler<BulkApproveCostSlipsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkApproveCostSlipsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
        {
            return Result<string>.Failure("Onaylanacak pusula seçilmedi.");
        }

        List<IdentityId> ids = request.Ids.Select(i => new IdentityId(i)).ToList();

        List<CostSlip> slips = await costSlipRepository.WhereWithTracking(s => ids.Contains(s.Id))
            .Include(s => s.CostSlipItems)
            .ToListAsync(cancellationToken);

        if (slips.Count == 0)
        {
            return Result<string>.Failure("Seçilen pusulalar bulunamadı.");
        }

        // Sıralama kritiktir: ComputeAvailableQuantity tarihe göre işler ve
        // planlanmış hareketler biriktirildiği için eskiden yeniye gitmek
        // doğru kümülatif stoğu verir.
        List<CostSlip> ordered = slips
            .OrderBy(s => s.CostDate)
            .ThenBy(s => s.SlipNumber)
            .ToList();

        List<string> failures = [];

        foreach (CostSlip slip in ordered)
        {
            if (slip.Status == CostSlipStatus.Approved)
            {
                failures.Add($"{slip.SlipNumber}: zaten onaylanmış.");
            }
        }

        if (failures.Count > 0)
        {
            return BuildFailure(failures);
        }

        List<ProductMovement> accumulatedMovements = [];

        foreach (CostSlip slip in ordered)
        {
            bool hasMovements = await productMovementRepository
                .AnyAsync(m => m.ReferenceNo == slip.SlipNumber, cancellationToken);

            if (hasMovements)
            {
                failures.Add($"{slip.SlipNumber}: stok hareketleri zaten kayıtlı.");
            }
        }

        if (failures.Count > 0)
        {
            return BuildFailure(failures);
        }

        // 1. AŞAMA — YAZMADAN doğrulama ve planlama. Burada hiçbir AddAsync
        // çağrılmaz; bir hata olursa hiçbir veri değişmez.
        List<CostSlipStockPlan> plans = [];

        foreach (CostSlip slip in ordered)
        {
            Result<CostSlipStockPlan> plan = await CostSlipStockHelper.PlanStockEffectsAsync(
                slip,
                request.CostingMethod,
                productMovementRepository,
                productRepository,
                accumulatedMovements,
                cancellationToken);

            if (plan.Data is null)
            {
                failures.Add($"{slip.SlipNumber}: {string.Join(" ", plan.ErrorMessages ?? [])}");
                continue;
            }

            plans.Add(plan.Data);
            accumulatedMovements.AddRange(plan.Data.Movements);
        }

        if (failures.Count > 0)
        {
            return BuildFailure(failures);
        }

        // 2. AŞAMA — yazma. TransactionBehavior tek SaveChangesAsync çağıracak;
        // tüm hareketler, yevmiye kayıtları ve durum değişiklikleri tek
        // transaction içinde kalıcı olur.
        foreach ((CostSlip slip, CostSlipStockPlan plan) in ordered.Zip(plans))
        {
            foreach (ProductMovement movement in plan.Movements)
            {
                await productMovementRepository.AddAsync(movement, cancellationToken);
            }

            foreach (PlannedLedgerEntry entry in plan.LedgerEntries)
            {
                await ledgerPoster.PostAsync(
                    entry.AccountId, entry.Debit, entry.Credit, entry.SourceType, entry.SourceId, cancellationToken);
            }

            slip.Approve();
        }

        return Result<string>.Succeed($"{ordered.Count} maliyet pusulası onaylandı; stok hareketleri oluşturuldu.");
    }

    private static Result<string> BuildFailure(List<string> failures)
        => Result<string>.Failure(
            "Hiçbir pusula onaylanmadı. Onaylanamayan kayıtlar:\n"
            + string.Join("\n", failures));
}