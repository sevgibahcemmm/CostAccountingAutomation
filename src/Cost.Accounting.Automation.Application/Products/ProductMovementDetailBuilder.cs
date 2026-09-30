using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Ham stok hareketlerini, ilişkili belgelerle (fatura, stok çıkışı, maliyet pusulası)
/// eşleştirip liste detaylarında gösterilecek satırlara dönüştürür.
/// </summary>
internal static class ProductMovementDetailBuilder
{
    private sealed record DocumentInfo(string TypeName, string? Number, DateOnly? Date);

    public static async Task<Dictionary<Guid, List<ProductStockMovementDetailDto>>> BuildByProductAsync(
        IEnumerable<Guid> productIds,
        IProductMovementRepository movementRepository,
        IInvoiceRepository invoiceRepository,
        IStockIssueRepository stockIssueRepository,
        ICostSlipRepository costSlipRepository,
        CancellationToken cancellationToken,
        int? perProductLimit = null)
    {
        List<ProductMovementQueryResult> movements = await movementRepository.GetByProductIdsAsync(productIds, cancellationToken);
        if (movements.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, DocumentInfo> invoices = await LoadInvoicesAsync(movements, invoiceRepository, cancellationToken);
        Dictionary<Guid, DocumentInfo> stockIssues = await LoadStockIssuesAsync(movements, stockIssueRepository, cancellationToken);
        Dictionary<string, DocumentInfo> costSlips = await LoadCostSlipsAsync(movements, costSlipRepository, cancellationToken);

        return movements
            .GroupBy(m => m.ProductId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    IEnumerable<ProductMovementQueryResult> ordered = g
                        .OrderByDescending(m => m.Date)
                        .ThenByDescending(m => m.Id);

                    // Liste detayında ürün başına hareket sayısı sınırlanabilir; en yeni hareketler gösterilir.
                    if (perProductLimit is > 0)
                    {
                        ordered = ordered.Take(perProductLimit.Value);
                    }

                    return ordered
                        .Select(m => Map(m, ResolveDocument(m, invoices, stockIssues, costSlips)))
                        .ToList();
                });
    }

    private static async Task<Dictionary<Guid, DocumentInfo>> LoadInvoicesAsync(
        List<ProductMovementQueryResult> movements,
        IInvoiceRepository invoiceRepository,
        CancellationToken cancellationToken)
    {
        HashSet<IdentityId> ids = movements
            .Where(m => m.InvoiceId.HasValue)
            .Select(m => new IdentityId(m.InvoiceId!.Value))
            .ToHashSet();

        if (ids.Count == 0)
        {
            return [];
        }

        var invoices = await invoiceRepository.GetAll()
            .AsNoTracking()
            .Where(i => ids.Contains(i.Id))
            .Select(i => new { i.Id, i.InvoiceNumber, i.Date, i.InvoiceType })
            .ToListAsync(cancellationToken);

        return invoices.ToDictionary(
            i => i.Id.Value,
            i => new DocumentInfo(EnumDisplay.GetDisplayName(i.InvoiceType), i.InvoiceNumber, i.Date));
    }

    private static async Task<Dictionary<Guid, DocumentInfo>> LoadStockIssuesAsync(
        List<ProductMovementQueryResult> movements,
        IStockIssueRepository stockIssueRepository,
        CancellationToken cancellationToken)
    {
        HashSet<IdentityId> ids = movements
            .Where(m => m.StockIssueId.HasValue)
            .Select(m => new IdentityId(m.StockIssueId!.Value))
            .ToHashSet();

        if (ids.Count == 0)
        {
            return [];
        }

        var issues = await stockIssueRepository.GetAll()
            .AsNoTracking()
            .Where(i => ids.Contains(i.Id))
            .Select(i => new { i.Id, i.DocumentNumber, i.Date, i.IssueType })
            .ToListAsync(cancellationToken);

        return issues.ToDictionary(
            i => i.Id.Value,
            i => new DocumentInfo(StockIssueDocumentName(i.IssueType), i.DocumentNumber, i.Date));
    }

    private static async Task<Dictionary<string, DocumentInfo>> LoadCostSlipsAsync(
        List<ProductMovementQueryResult> movements,
        ICostSlipRepository costSlipRepository,
        CancellationToken cancellationToken)
    {
        HashSet<string> numbers = movements
            .Where(m => IsCostSlipMovement(m))
            .Select(m => ExtractCostSlipNumber(m))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (numbers.Count == 0)
        {
            return [];
        }

        var slips = await costSlipRepository.GetAll()
            .AsNoTracking()
            .Where(s => numbers.Contains(s.SlipNumber))
            .Select(s => new { s.SlipNumber, s.CostDate, s.CostSlipType })
            .ToListAsync(cancellationToken);

        return slips.ToDictionary(
            s => s.SlipNumber,
            s => new DocumentInfo(EnumDisplay.GetDisplayName(s.CostSlipType), s.SlipNumber, s.CostDate),
            StringComparer.OrdinalIgnoreCase);
    }

    private static DocumentInfo ResolveDocument(
        ProductMovementQueryResult movement,
        Dictionary<Guid, DocumentInfo> invoices,
        Dictionary<Guid, DocumentInfo> stockIssues,
        Dictionary<string, DocumentInfo> costSlips)
    {
        if (movement.InvoiceId is { } invoiceId && invoices.TryGetValue(invoiceId, out DocumentInfo? invoice))
        {
            return invoice;
        }

        if (movement.StockIssueId is { } issueId && stockIssues.TryGetValue(issueId, out DocumentInfo? issue))
        {
            return issue;
        }

        if (IsCostSlipMovement(movement))
        {
            string number = ExtractCostSlipNumber(movement);
            if (!string.IsNullOrWhiteSpace(number)
                && costSlips.TryGetValue(number, out DocumentInfo? slip))
            {
                return slip;
            }

            return new DocumentInfo("Maliyet Pusulası", movement.ReferenceNo, movement.Date);
        }

        if (movement.Description.StartsWith(ProductStockBalanceHelper.AtelierTransferInputDescriptionPrefix, StringComparison.Ordinal))
        {
            return new DocumentInfo("Atölye Transferi", movement.ReferenceNo, movement.Date);
        }

        if (movement.Description.Contains("Faturası", StringComparison.Ordinal))
        {
            return new DocumentInfo(InvoiceDocumentName(movement.Description), movement.ReferenceNo, movement.Date);
        }

        return new DocumentInfo(ReasonDocumentName(movement.Reason), movement.ReferenceNo, movement.Date);
    }

    private static ProductStockMovementDetailDto Map(ProductMovementQueryResult movement, DocumentInfo document) => new()
    {
        Date = movement.Date,
        DocumentTypeName = document.TypeName,
        DocumentNumber = string.IsNullOrWhiteSpace(document.Number) ? movement.ReferenceNo : document.Number,
        DocumentDate = document.Date ?? movement.Date,
        MovementType = movement.MovementType,
        Reason = movement.Reason,
        Quantity = movement.Quantity,
        UnitPrice = movement.UnitPrice,
        ReferenceNo = movement.ReferenceNo,
        Description = movement.Description
    };

    private static bool IsCostSlipMovement(ProductMovementQueryResult movement)
        => movement.Description.StartsWith(ProductStockBalanceHelper.ProductionInputDescriptionPrefix, StringComparison.Ordinal)
            || movement.Description.StartsWith(ProductStockBalanceHelper.CostSlipConsumptionOutputDescriptionPrefix, StringComparison.Ordinal);

    private static string ExtractCostSlipNumber(ProductMovementQueryResult movement)
        => string.IsNullOrWhiteSpace(movement.ReferenceNo) ? string.Empty : movement.ReferenceNo.Trim();

    private static string StockIssueDocumentName(StockIssueType issueType)
        => issueType == StockIssueType.AtelierTransfer ? "Atölye Transferi" : "Stok Çıkışı";

    private static string InvoiceDocumentName(string description)
    {
        if (description.Contains("Alış", StringComparison.OrdinalIgnoreCase))
        {
            return "Alış Faturası";
        }

        return "Satış Faturası";
    }

    private static string ReasonDocumentName(ProductMovementReason reason) => reason switch
    {
        ProductMovementReason.CountingSurplus => "Sayım Fazlası",
        ProductMovementReason.CountingDeficit => "Sayım Noksanı",
        ProductMovementReason.Fire => "Fire / Zayi",
        ProductMovementReason.Sample => "Numune / Hediye",
        ProductMovementReason.Transfer => "Devir / Transfer",
        _ => "Manuel Hareket"
    };
}
