using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

/// <summary>
/// Taslak bir atölye transferi veya tüketim belgesini onaylar ve stok/yevmiye
/// hareketlerini oluşturur. Onaylanana kadar belge stoğu ETKİLEMEZ.
/// </summary>
[Permission("stockissue:approve")]
public sealed record StockIssueApproveCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class StockIssueApproveCommandHandler(
    IStockIssueRepository stockIssueRepository,
    IProductMovementRepository productMovementRepository,
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IChartOfAccountLedgerPoster ledgerPoster) : IRequestHandler<StockIssueApproveCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        StockIssueApproveCommand request,
        CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        StockIssue? issue = await stockIssueRepository.WhereWithTracking(i => i.Id == id)
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(cancellationToken);

        if (issue is null)
        {
            return Result<string>.Failure("Belge bulunamadı.");
        }

        if (issue.Status == StockIssueStatus.Approved)
        {
            return Result<string>.Failure("Bu belge zaten onaylanmış durumda.");
        }

        // Çift stok hareketi koruması: onaylı belgenin hareketleri silinmiş
        // olabilir (özel durum), yine de tekrar onaylanmamalı.
        bool hasMovements = await productMovementRepository
            .AnyAsync(m => m.StockIssueId == id, cancellationToken);

        if (hasMovements)
        {
            return Result<string>.Failure("Bu belge için stok hareketleri zaten kayıtlı.");
        }

        // Hareketler kayıt anında hesaplanmış birim maliyetlerle üretilir; onay
        // yalnızca bu planı yazar, maliyeti yeniden hesaplamaz.
        List<ChartOfAccount> accounts = await chartOfAccountRepository
            .GetAllIncludingDeletedAsync(cancellationToken);

        List<IdentityId> productIds = issue.Lines
            .Select(l => l.ProductId)
            .Distinct()
            .ToList();

        List<Product> products = await productRepository.GetAll()
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        // Onay anındaki gerçek katmanlar kullanılır. Taslak ile onay arasında
        // başka belgeler onaylanmış olabilir; eski bakiyeye göre yazılan
        // çıkış, alınmamış bir girişten düşülür ve FIFO bozulur.
        List<ProductMovement> movements = await StockIssueCostingHelper.LoadMovementsAsync(
            productIds,
            productMovementRepository,
            cancellationToken);

        Dictionary<IdentityId, ChartOfAccount> accountById = accounts.ToDictionary(a => a.Id);

        Dictionary<IdentityId, ChartOfAccount> productAccountMap = products
            .Where(p => p.ChartOfAccountId is not null
                        && accountById.ContainsKey(new IdentityId(p.ChartOfAccountId.Value)))
            .ToDictionary(p => p.Id, p => accountById[new IdentityId(p.ChartOfAccountId!.Value)]);

        // STOK YETERLİLİĞİ KAPISI. Taslak kaydında (StockIssueCreateCommand) ve
        // toplu onayda bu kontrol vardır; TEKİL onayda yoktu. Taslaktan sonra
        // alım faturası silinmiş/iptal edilmiş ya da başka bir belge stoğu
        // tüketmiş olursa katman kırılımı miktarı karşılayamaz ve çıkış,
        // fiyatı 0 olan uydurma bir hareketle yazılırdı. Fiyat bazlı gruplayan
        // stok raporunda bu, girişi olmayan ve bakiyesi eksi olan ayrı bir
        // satır olarak görünüyordu.
        // KONTROL ÜRÜN BAZINDA YAPILIR. Belge satırları giriş fiyatı başına birer
        // katmandır; 174 adetlik bir çıkış 144 + 30 olarak iki satırda durur.
        // Satır satır kontrol edilirse "144 <= mevcut" ve "30 <= mevcut"
        // denetlenir ve toplam 174 adetlik çıkış mevcut 150 ikinde de geçer;
        // gerçek yetersizlik kaçardı.
        foreach (IGrouping<IdentityId, StockIssueLine> group in issue.Lines.GroupBy(l => l.ProductId))
        {
            decimal requestedQuantity = group.Sum(l => l.Quantity);

            decimal available = StockIssueCostingHelper.ComputeAvailableQuantity(
                movements, group.Key, issue.Date);

            if (requestedQuantity <= available)
            {
                continue;
            }

            string productName = products.FirstOrDefault(p => p.Id == group.Key)?.Name.Value
                ?? group.Key.Value.ToString();

            return Result<string>.Failure(
                $"'{productName}' için bu tarihe kadar yeterli giriş (stok) yok. "
                + $"Mevcut: {available:n2}, istenen: {requestedQuantity:n2}. "
                + "Belge onaylanmadı; önce giriş kaydını kontrol edin.");
        }

        // Taslak ile onay arasında stok değişmiş olabilir. Satırlar, onay anındaki
        // gerçek FIFO kırılımıyla eşitlenir; böylece belgedeki kalem ile
        // yazılacak stok hareketi tanım gereği aynı olur.
        StockIssueStockHelper.SyncLinesWithFifo(issue, movements);

        StockIssueStockPlan plan = StockIssueStockHelper.PlanStockEffects(
            issue,
            accountById.GetValueOrDefault(issue.TargetAccountId),
            productAccountMap,
            movements);

        await StockIssueStockHelper.WritePlanAsync(
            plan,
            productMovementRepository,
            ledgerPoster,
            cancellationToken);

        issue.Approve();

        string actionName = issue.IssueType == StockIssueType.Consumption ? "Tüketim" : "Atölye transferi";

        // Katman kırılımını onay mesajında göster. Kullanıcı FIFO'nun gerçekten
        // hangi fiyattan ne kadar tükettiğini ekranda görebilmeli; önceden tek
        // bir ortalama/ilk katman fiyatı yazılıyordu ve bu yüzden gerçek çıkış
        // kırılımı görünmüyordu.
        string breakdown = string.Join(", ", plan.Movements
            .Where(m => m.MovementType == ProductMovementType.Output)
            .GroupBy(m => m.UnitPrice?.Value ?? 0m)
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Sum(x => x.Quantity):n0} × {g.Key:n2}"));

        string breakdownText = string.IsNullOrWhiteSpace(breakdown)
            ? string.Empty
            : $" FIFO kırılımı: {breakdown}.";

        return Result<string>.Succeed(
            $"{actionName} belgesi onaylandı; stok hareketleri oluşturuldu.{breakdownText}");
    }
}