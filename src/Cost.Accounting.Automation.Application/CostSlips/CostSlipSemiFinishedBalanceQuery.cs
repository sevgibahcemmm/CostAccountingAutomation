using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

public sealed record CostSlipSemiFinishedBalanceDto(
    Guid? SemiProductId,
    decimal Balance,
    decimal UnitPrice,
    IReadOnlyList<CostSlipExpenseBreakdownDto> ExpenseBreakdown);

/// <summary>
/// Yarımamülün birim maliyetinin gider hesabı (710 / 720 / 730 ...) bazında
/// dağılımı. Mamül pusulasına yarımamül yansıtılırken her bileşen kendi
/// hesabına yazılmalıdır; aksi halde 720/730 bileşenleri de 710'a yığılır.
/// </summary>
public sealed record CostSlipExpenseBreakdownDto(ExpenseAccountType AccountType, decimal UnitPrice);

[Permission("costslip:view")]
public sealed record CostSlipSemiFinishedBalanceQuery(Guid WorkshopId, Guid ProductId) : IRequest<Result<CostSlipSemiFinishedBalanceDto>>;

internal sealed class CostSlipSemiFinishedBalanceQueryHandler(
    ICostSlipRepository costSlipRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IProductRepository productRepository) : IRequestHandler<CostSlipSemiFinishedBalanceQuery, Result<CostSlipSemiFinishedBalanceDto>>
{
    public async Task<Result<CostSlipSemiFinishedBalanceDto>> Handle(
        CostSlipSemiFinishedBalanceQuery request,
        CancellationToken cancellationToken)
    {
        IdentityId selectedWorkshopId = new(request.WorkshopId);
        IdentityId productId = new(request.ProductId);

        var workshopIds = new List<IdentityId> { selectedWorkshopId };

        ChartOfAccount? linked = await chartOfAccountRepository.GetAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Type == ChartOfAccountType.Workshop
                    && a.Id == selectedWorkshopId,
                cancellationToken);

        if (linked is not null)
        {
            if (linked.SemiFinishedAccountId is IdentityId semiId
                && !workshopIds.Contains(semiId))
            {
                workshopIds.Add(semiId);
            }

            if (linked.FinishedAccountId is IdentityId finishedId
                && !workshopIds.Contains(finishedId))
            {
                workshopIds.Add(finishedId);
            }
        }

        // Kullanıcı MAMÜL pusulasında 152 (mamül) kategorisindeki ürünü seçer.
        // Yarı mamül fişlerinde ise ProducedProductId aynı isimdeki 151 (yarımamül)
        // ürününe aittir; bu yüzden seçilen ürünün 151 karşılığını eşleştiririz.
        Product? selectedProduct = await productRepository.GetAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == productId, cancellationToken);

        IdentityId? semiProductId = null;
        if (selectedProduct is not null)
        {
            if (selectedProduct.SemiFinishedProductId is IdentityId)
            {
                // Seçilen ürün zaten bir yarımamül ise bakiyeye konu olan ürünün kendisidir.
                semiProductId = selectedProduct.Id;
            }
            else
            {
                // Seçilen ürün bir mamüldür; yarımamül kartı SemiFinishedProductId ile
                // mamülüne işaret ettiği için ters bağlantı ile yarımamülü buluruz.
                Product? semi = await productRepository.GetAll()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        p => !p.IsDeleted && p.SemiFinishedProductId == selectedProduct.Id,
                        cancellationToken);

                if (semi is not null)
                {
                    semiProductId = semi.Id;
                }
                else if (linked?.SemiFinishedAccountId is IdentityId semiCategoryId)
                {
                    // Bağlantı kurulmadıysa eski yöntem: kategori + birebir isim eşleştirmesi.
                    Product? semiByName = await productRepository.GetAll()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            p => !p.IsDeleted
                                && p.CategoryId == semiCategoryId
                                && p.Name.Value == selectedProduct.Name.Value,
                            cancellationToken);
                    semiProductId = semiByName?.Id;
                }
            }
        }

        List<CostSlip> approvedSlips = await costSlipRepository.GetAll()
            .Where(s => workshopIds.Contains(s.WorkshopId)
                && !s.IsDeleted)
            .Include(s => s.CostSlipItems)
            .ToListAsync(cancellationToken);

        decimal produced = approvedSlips
            .Where(s => s.CostSlipType == CostSlipType.SemiFinishedProduct
                && s.ProducedProductId == (semiProductId ?? productId))
            .Sum(s => (decimal)s.Quantity);

        decimal consumed = approvedSlips
            .SelectMany(s => s.CostSlipItems)
            .Where(i => i.ProductId == productId || i.ProductId == semiProductId)
            .Sum(i => i.Quantity);

        decimal balance = Math.Max(0m, produced - consumed);

        decimal unitPrice = 0m;
        IReadOnlyList<CostSlipExpenseBreakdownDto> expenseBreakdown = [];

        if (semiProductId is IdentityId semiProductVal && produced > 0)
        {
            List<CostSlip> semiSlips = approvedSlips
                .Where(s => s.CostSlipType == CostSlipType.SemiFinishedProduct
                    && s.ProducedProductId == semiProductVal)
                .ToList();

            decimal producedCost = semiSlips.Sum(s => s.GrandTotal);

            unitPrice = Math.Round(producedCost / produced, 2);

            // Gider hesabı bazında kırılım: her hesabın toplam maliyeti üretilen
            // miktara bölünür. Kuruş yuvarlamasından doğan fark en büyük
            // kaleme eklenir; böylece kalemler toplamı birim maliyete eşit olur.
            List<(ExpenseAccountType Account, decimal Amount)> amounts = semiSlips
                .SelectMany(s => s.CostSlipItems)
                .GroupBy(i => i.ExpenseAccountType)
                .Select(g => (Account: g.Key, Amount: g.Sum(i => i.TotalAmount)))
                .Where(x => x.Amount != 0m)
                .OrderByDescending(x => Math.Abs(x.Amount))
                .ThenBy(x => x.Account)
                .ToList();

            if (amounts.Count == 0)
            {
                // Pusula kalemi yoksa mevcut davranış korunur: tek satır, 710.
                expenseBreakdown = [new CostSlipExpenseBreakdownDto(ExpenseAccountType.Account710, unitPrice)];
            }
            else
            {
                List<CostSlipExpenseBreakdownDto> parts = amounts
                    .Select(x => new CostSlipExpenseBreakdownDto(
                        x.Account,
                        Math.Round(x.Amount / produced, 2)))
                    .ToList();

                decimal partsSum = parts.Sum(p => p.UnitPrice);
                decimal remainder = Math.Round(unitPrice - partsSum, 2);
                if (remainder != 0m)
                {
                    parts[0] = parts[0] with { UnitPrice = Math.Round(parts[0].UnitPrice + remainder, 2) };
                }

                expenseBreakdown = parts
                    .Where(p => p.UnitPrice != 0m)
                    .OrderBy(p => p.AccountType)
                    .ToList();
            }
        }
        else if (semiProductId is null && productId.Value != Guid.Empty)
        {
            Price productPrice = (await productRepository.GetAll()
                .AsNoTracking()
                .Where(p => !p.IsDeleted
                    && p.SemiFinishedProductId == productId
                    && p.Prices.Any(x => x.IsActive))
                .SelectMany(p => p.Prices)
                .OrderByDescending(x => x.StartDate)
                .Select(x => x.UnitPrice)
                .FirstOrDefaultAsync(cancellationToken)) ?? new Price(0m);

            unitPrice = productPrice.Value;
            expenseBreakdown = [new CostSlipExpenseBreakdownDto(ExpenseAccountType.Account710, unitPrice)];
        }

        System.Diagnostics.Debug.WriteLine($"[SEMI-DIAG] workshopIds={string.Join(",", workshopIds.Select(w => w.Value))} " +
            $"productId={productId.Value} semiProductId={(semiProductId == null ? "null" : semiProductId.Value.ToString())} " +
            $"produced={produced:0.##} consumed={consumed:0.##} balance={balance:0.##} unitPrice={unitPrice:0.##}");

        return Result<CostSlipSemiFinishedBalanceDto>.Succeed(new CostSlipSemiFinishedBalanceDto(
            semiProductId?.Value,
            balance,
            unitPrice,
            expenseBreakdown));
    }
}