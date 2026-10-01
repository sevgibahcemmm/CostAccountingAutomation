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

        List<Product> products = await productRepository.GetAll()
            .Where(p => issue.Lines.Select(l => l.ProductId.Value).Contains(p.Id.Value))
            .ToListAsync(cancellationToken);

        Dictionary<IdentityId, ChartOfAccount> targetAccountMap = products
            .Where(p => p.ChartOfAccountId is not null)
            .ToDictionary(p => p.Id, p => accounts.First(a => a.Id.Value == p.ChartOfAccountId!.Value));

        StockIssueStockPlan plan = StockIssueStockHelper.PlanStockEffects(issue, targetAccountMap);

        await StockIssueStockHelper.WritePlanAsync(
            plan,
            productMovementRepository,
            ledgerPoster,
            cancellationToken);

        issue.Approve();

        string actionName = issue.IssueType == StockIssueType.Consumption ? "Tüketim" : "Atölye transferi";
        return Result<string>.Succeed($"{actionName} belgesi onaylandı; stok hareketleri oluşturuldu.");
    }
}