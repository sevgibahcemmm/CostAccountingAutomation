using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.StockIssues;

[Permission("stock_issue:view")]
public sealed record AtelierTransferProductsQuery(Guid TargetAccountId) : IRequest<Result<List<AtelierTransferProductDto>>>;

public sealed class AtelierTransferProductDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string UnitTypeName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal TransferredQuantity { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public decimal DraftQuantity { get; set; }

    public decimal AvailableQuantity => Math.Max(0m, TransferredQuantity - ConsumedQuantity);
}

internal sealed class AtelierTransferProductsQueryHandler(
    IStockIssueRepository stockIssueRepository,
    ICostSlipRepository costSlipRepository) : IRequestHandler<AtelierTransferProductsQuery, Result<List<AtelierTransferProductDto>>>
{
    public async Task<Result<List<AtelierTransferProductDto>>> Handle(
        AtelierTransferProductsQuery request,
        CancellationToken cancellationToken)
    {
        List<StockIssue> transfers = await stockIssueRepository.GetAll()
            .Where(i => i.IssueType == StockIssueType.AtelierTransfer && !i.IsDeleted)
            .Where(i => i.TargetAccountId == new IdentityId(request.TargetAccountId))
            .Include(i => i.Lines).ThenInclude(l => l.Product!).ThenInclude(p => p.ProductUnitType)
            .OrderBy(i => i.Date)
            .ThenBy(i => i.DocumentNumber)
            .ToListAsync(cancellationToken);

        // Onaylı transferler stoğu GERÇEKTEN artırır; taslak transferler
        // stok hareketi üretmediği için TransferredQuantity'ye sayılmaz, yalnızca
        // "Taslak (Bekleyen)" olarak görünür. Böylece taslak transfer atölyeye
        // malzeme getirmiş gibi davranılmaz.
        List<StockIssue> approvedTransfers = transfers
            .Where(i => i.Status == StockIssueStatus.Approved)
            .ToList();

        List<StockIssueLine> approvedLines = approvedTransfers
            .SelectMany(i => i.Lines)
            .Where(l => l.Product is not null)
            .ToList();

        List<StockIssueLine> draftLines = transfers
            .Where(i => i.Status == StockIssueStatus.Draft)
            .SelectMany(i => i.Lines)
            .Where(l => l.Product is not null)
            .ToList();

        // Bekleyen miktar: onaylanmamış transfer satırları (atölyeye gelen malzeme).
        Dictionary<Guid, decimal> pendingTransferMap = draftLines
            .GroupBy(l => l.ProductId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        Dictionary<Guid, decimal> transferredMap = approvedLines
            .GroupBy(l => l.ProductId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        Dictionary<Guid, StockIssueLine> latestApprovedLine = approvedLines
            .GroupBy(l => l.ProductId!.Value)
            .ToDictionary(g => g.Key, g => g.Last());

        HashSet<Guid> productIds = approvedLines
            .Concat(draftLines)
            .Select(l => l.ProductId!.Value)
            .ToHashSet();

        Dictionary<Guid, decimal> consumedMap = [];

        List<CostSlip> slips = productIds.Count > 0
            ? await costSlipRepository.GetAll()
                .Where(s => s.WorkshopId == new IdentityId(request.TargetAccountId)
                    && !s.IsDeleted)
                .Include(s => s.CostSlipItems)
                .ToListAsync(cancellationToken)
            : [];

        Dictionary<Guid, decimal> draftMap = [];
        if (slips.Count > 0)
        {
            var groupedSlips = slips.GroupBy(s => s.Status);
            List<CostSlip> approvedSlips = groupedSlips
                .FirstOrDefault(g => g.Key == CostSlipStatus.Approved)?
                .ToList() ?? [];

            consumedMap = approvedSlips
                .SelectMany(s => s.CostSlipItems)
                .Where(i => i.ProductId is not null)
                .GroupBy(i => i.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            List<CostSlip> draftSlips = groupedSlips
                .FirstOrDefault(g => g.Key == CostSlipStatus.Draft)?
                .ToList() ?? [];

            draftMap = draftSlips
                .SelectMany(s => s.CostSlipItems)
                .Where(i => i.ProductId is not null)
                .GroupBy(i => i.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
        }

        // Ürün listesi onaylı VE taslak transfer satırlarının birleşiminden kurulur.
        // Yalnızca onaylı transferler taranırsa, sadece taslak transferi olan bir
        // ürün hiç listelenmez ve "Taslak" kolonu hiçbir zaman değer gösteremez.
        var result = approvedLines
            .Concat(draftLines)
            .GroupBy(l => l.ProductId!.Value)
            .Select(g =>
            {
                Guid productId = g.Key;
                Product product = g.First().Product!;

                // Birim maliyet onaylı transferden gelir; yalnızca taslak transferi
                // varsa son satırın maliyeti kullanılır.
                if (!latestApprovedLine.TryGetValue(productId, out StockIssueLine? priceSource))
                {
                    priceSource = g.Last();
                }

                return new AtelierTransferProductDto
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode.Value,
                    ProductName = product.Name.Value,
                    UnitTypeName = product.ProductUnitType?.Name.Value ?? string.Empty,
                    UnitPrice = priceSource.UnitCost.Value,
                    TransferredQuantity = transferredMap.TryGetValue(productId, out decimal transferred) ? transferred : 0m,
                    ConsumedQuantity = consumedMap.TryGetValue(productId, out decimal consumed) ? consumed : 0m,
                    // Bekleyen = onaylanmamış transfer + maliyet pusulası taslağı
                    DraftQuantity = (pendingTransferMap.TryGetValue(productId, out decimal pending) ? pending : 0m)
                        + (draftMap.TryGetValue(productId, out decimal drafted) ? drafted : 0m)
                };
            })
            .OrderBy(p => p.TransferredQuantity > 0m)
            .ThenBy(p => p.ProductCode)
            .ThenBy(p => p.ProductName)
            .ToList();

        return result;
    }
}