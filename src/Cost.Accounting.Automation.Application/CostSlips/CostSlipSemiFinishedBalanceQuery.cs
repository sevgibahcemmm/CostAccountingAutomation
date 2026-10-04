using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

/// <summary>
/// Yarımamülün tüketilmemiş tek bir giriş katmanı.
/// </summary>
/// <param name="Quantity">Bu girişten kalan miktar.</param>
/// <param name="UnitPrice">Bu girişin birim maliyeti.</param>
public sealed record CostSlipSemiFinishedLayerDto(decimal Quantity, decimal UnitPrice);

/// <summary>
/// <paramref name="Balance"/> yarımamülün stoktaki miktar bakiyesidir ve
/// <paramref name="Layers"/> bu bakiyenin hangi girişlerden oluştuğunu gösterir.
/// <para>
/// <paramref name="AvailableAmount"/> katmanların toplam tutarıdır. Ortalama
/// birim maliyetten hesaplanan bir tutar kullanılırsa, kullanıcı kısmi tutar
/// girdiğinde hangi girişten ne kadar alınacağı bilinemez ve yevmiye pusula
/// tutarıyla tutmaz. Bu yüzden katmanlar tek doğruluk kaynağıdır.
/// </para>
/// </summary>
public sealed record CostSlipSemiFinishedBalanceDto(
    Guid? SemiProductId,
    decimal Balance,
    decimal UnitPrice,
    decimal AvailableAmount,
    IReadOnlyList<CostSlipExpenseBreakdownDto> ExpenseBreakdown,
    IReadOnlyList<CostSlipSemiFinishedLayerDto> Layers)
{
    /// <summary>FIFO kırılımı için katmanları üretir.</summary>
    public List<CostingLayer> ToCostingLayers()
        => Layers.Select(l => new CostingLayer(l.Quantity, l.UnitPrice)).ToList();
}

/// <summary>
/// Yarımamülün birim maliyetinin gider hesabı (710 / 720 / 730 ...) bazında
/// dağılımı. Mamül pusulasına yarımamül yansıtılırken her bileşen kendi
/// hesabına yazılmalıdır; aksi halde 720/730 bileşenleri de 710'a yığılır.
/// </summary>
public sealed record CostSlipExpenseBreakdownDto(ExpenseAccountType AccountType, decimal UnitPrice);

/// <summary>
/// <paramref name="CostDate"/> verilmezse bugünün tarihi kullanılır. Katmanlar
/// yalnızca bu tarihe kadar olan giriş/çıkışlardan hesaplanır; pusulanın
/// tarihinden sonraki üretimler mevcut stoka sayılmaz.
/// </summary>
[Permission("costslip:view")]
public sealed record CostSlipSemiFinishedBalanceQuery(
    Guid WorkshopId,
    Guid ProductId,
    DateOnly? CostDate = null,
    StockCostingMethod CostingMethod = StockCostingMethod.Fifo)
    : IRequest<Result<CostSlipSemiFinishedBalanceDto>>;

internal sealed class CostSlipSemiFinishedBalanceQueryHandler(
    ICostSlipRepository costSlipRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IProductRepository productRepository,
    IProductMovementRepository productMovementRepository)
    : IRequestHandler<CostSlipSemiFinishedBalanceQuery, Result<CostSlipSemiFinishedBalanceDto>>
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

        // Bakiye artık PUSULA kayıtlarından değil, gerçek stok hareketlerinden
        // hesaplanır. Yarımamül stoğu, yarımamül pusulalarının ONAYLANDIĞI anda
        // oluşan giriş hareketlerinden gelir; tüketim de yalnızca onaylı
        // mamül pusulalarının çıkış hareketleriyle olur. Pusula kayıtlarından
        // bakiye tutulduğunda taslaklar da sayılır ve stokla iki ayrı gerçek
        // oluşurdu.
        DateOnly costDate = request.CostDate ?? DateOnly.FromDateTime(DateTime.Today);

        List<CostingLayer> layers = [];
        if (semiProductId is IdentityId semiProductVal)
        {
            List<ProductMovement> movements = await StockIssueCostingHelper.LoadMovementsAsync(
                [semiProductVal],
                productMovementRepository,
                cancellationToken);

            layers = StockIssueCostingHelper.BuildRemainingLayers(
                movements, semiProductVal, request.CostingMethod, costDate);
        }

        decimal balance = StockCostingLayers.TotalQuantity(layers);
        decimal availableAmount = StockCostingLayers.TotalAmount(layers);

        // Ortalama birim maliyet yalnızca tek satırlı gösterim için
        // (bakiye sıfır değilse) ve geriye dönük uyum için saklanır. Tutar
        // hesabında kullanılmaz.
        decimal unitPrice = balance > 0m
            ? Math.Round(availableAmount / balance, 4)
            : 0m;

        decimal produced = balance;
        IReadOnlyList<CostSlipExpenseBreakdownDto> expenseBreakdown = [];

        if (semiProductId is IdentityId semiProductForBreakdown && produced > 0)
        {
            List<CostSlip> semiSlips = await costSlipRepository.GetAll()
                .Where(s => workshopIds.Contains(s.WorkshopId)
                    && !s.IsDeleted
                    && s.CostSlipType == CostSlipType.SemiFinishedProduct
                    && s.ProducedProductId == semiProductForBreakdown)
                .Include(s => s.CostSlipItems)
                .ToListAsync(cancellationToken);

            // Gider hesabı bazında kırılım: her hesabın toplam maliyeti üretilen
            // miktara bölünür. Kuruş yuvarlamasından doğan fark en büyük
            // kaleme eklenir; böylece kalemler toplamı birim maliyete eşit olur.
            // Yalnızca satır açıklamasında gösterildiği için gider satırı
            // olarak YAZILMAZ: o giderler zaten yarımamül pusulasında
            // giderleşmiştir.
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
                // Pusula kalemi yoksa tek satır, 710.
                expenseBreakdown = [new CostSlipExpenseBreakdownDto(ExpenseAccountType.Account710, unitPrice)];
            }
            else
            {
                List<CostSlipExpenseBreakdownDto> parts = amounts
                    .Select(x => new CostSlipExpenseBreakdownDto(
                        x.Account,
                        Math.Round(x.Amount / produced, 4)))
                    .ToList();

                decimal partsSum = parts.Sum(p => p.UnitPrice);
                decimal remainder = Math.Round(unitPrice - partsSum, 4);
                if (remainder != 0m && parts.Count > 0)
                {
                    parts[0] = parts[0] with { UnitPrice = Math.Round(parts[0].UnitPrice + remainder, 4) };
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
            $"asOf={costDate:dd.MM.yyyy} balance={balance:0.####} availableAmount={availableAmount:n2} " +
            $"layers={string.Join(" | ", layers.Select(l => $"{l.Quantity:0.####}@{l.UnitPrice:n2}"))}");

        return Result<CostSlipSemiFinishedBalanceDto>.Succeed(new CostSlipSemiFinishedBalanceDto(
            semiProductId?.Value,
            balance,
            unitPrice,
            availableAmount,
            expenseBreakdown,
            [.. layers.Select(l => new CostSlipSemiFinishedLayerDto(l.Quantity, l.UnitPrice))]));
    }
}