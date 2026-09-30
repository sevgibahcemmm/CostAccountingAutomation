using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.StockMovements;

[Permission("stockmovement:view")]
internal sealed class StockMovementsListReportQueryHandler(
    IProductMovementRepository productMovementRepository,
    IStockIssueRepository stockIssueRepository,
    ICostSlipRepository costSlipRepository,
    IChartOfAccountRepository chartOfAccountRepository)
    : IRequestHandler<StockMovementsListReportQuery, List<StockMovementReportRowDto>>
{
    public async Task<List<StockMovementReportRowDto>> Handle(
        StockMovementsListReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = productMovementRepository.GetAll()
            .Where(m => m.Date >= request.StartDate && m.Date <= request.EndDate)
            .Where(m => !m.IsDeleted);

        ChartOfAccount? selectedWarehouse = null;
        if (request.WarehouseId.HasValue)
        {
            selectedWarehouse = await chartOfAccountRepository.GetByIdIncludingDeletedAsync(
                new IdentityId(request.WarehouseId.Value),
                cancellationToken);

            if (selectedWarehouse is null)
            {
                return [];
            }
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(m => m.ProductId == request.ProductId.Value);
        }

        var movements = await query
            .Include(m => m.Product)
                .ThenInclude(p => p!.Warehouse!)
            .Include(m => m.Product)
                .ThenInclude(p => p!.ProductUnitType!)
            .ToListAsync(cancellationToken);

        // StockIssue'ları lokasyon bilgisi için önceden yükleyelim
        Dictionary<IdentityId, StockIssue> stockIssues = (await stockIssueRepository.GetAll()
            .AsNoTracking()
            .Where(i => !i.IsDeleted)
            .Include(i => i!.SourceWarehouse!)
            .Include(i => i!.TargetAccount!)
            .ToListAsync(cancellationToken))
            .ToDictionary(i => i.Id);

        // CostSlips'in workshop bilgisi
        List<CostSlip> costSlips = await costSlipRepository.GetAll()
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.CostDate >= request.StartDate && s.CostDate <= request.EndDate)
            .Include(s => s.Workshop!)
            .ToListAsync(cancellationToken);
        Dictionary<string, CostSlip> costSlipByNumber = costSlips.ToDictionary(s => s.SlipNumber);

        var grouped = movements
     .GroupBy(m =>
     {
         var loc = ResolveLocation(m, stockIssues, costSlipByNumber);

         string locationCode = loc.code;
         string locationName = loc.name;
         ChartOfAccountType accountType = loc.accountType;

         // Null gelme ihtimaline karşı güvenli okuma
         string subGroupCode = m.Product?.Warehouse?.Code?.Value ?? "";
         string subGroupName = m.Product?.Warehouse?.Name?.Value ?? "";

         // Ürün adında YARIMAMÜL geçiyorsa VEYA kod 151 ise acımadan hepsini aynı gruba zorluyoruz
         if ((m.Product != null && m.Product.Name.Value.Contains("YARIMAMÜL")) || locationCode.StartsWith("151"))
         {
             locationCode = "151";
             locationName = "Yarı Mamüller-Üretim Hesabı";
             accountType = ChartOfAccountType.Warehouse;
         }

         return new
         {
             LocationCode = locationCode,
             LocationName = locationName,
             AccountType = accountType,
             SubGroupCode = subGroupCode,
             SubGroupName = subGroupName,
             ProductId = m.ProductId,
             UnitPrice = m.UnitPrice != null ? (decimal?)m.UnitPrice.Value : null
         };
     })
                    .Select(g =>
            {
                Product p = g.First().Product!;

                // Bu rapor LOKASYON bazlıdır: atölye transferi girişi atölye
                // konumunda gerçek bir giriştir, çıkışı depoda gerçek bir
                // çıkıştır. Bu yüzden ikisi de sayılır; lokasyon bakiyeleri
                // toplandığında ürünün gerçek stoğu çıkar. Ürün bazlı stok
                // toplamlarındaki kural (ProductStockBalanceHelper) burada
                // uygulanmaz.
                decimal totalInQty = g.Where(m => m.MovementType == ProductMovementType.Input).Sum(m => m.Quantity);
                decimal totalOutQty = g.Where(m => m.MovementType == ProductMovementType.Output).Sum(m => m.Quantity);

                decimal totalInAmt = g.Where(m => m.MovementType == ProductMovementType.Input && m.UnitPrice != null)
                                      .Sum(m => m.Quantity * m.UnitPrice!.Value);
                decimal totalOutAmt = g.Where(m => m.MovementType == ProductMovementType.Output && m.UnitPrice != null)
                                       .Sum(m => m.Quantity * m.UnitPrice!.Value);

                return new StockMovementReportRowDto
                {
                    AccountType = g.Key.AccountType,
                    LocationCode = g.Key.LocationCode,
                    LocationName = g.Key.LocationName,
                    SubGroupCode = g.Key.SubGroupCode,
                    SubGroupName = g.Key.SubGroupName,
                    ProductName = p.Name.Value,
                    ProductCode = p.ProductCode.Value,
                    UnitTypeName = p.ProductUnitType!.Name.Value,
                    TotalInQuantity = totalInQty,
                    TotalOutQuantity = totalOutQty,
                    BalanceQuantity = totalInQty - totalOutQty,
                    UnitCost = g.Where(m => m.MovementType == ProductMovementType.Input && m.UnitPrice != null)
                                .Select(m => (decimal?)m.UnitPrice!.Value)
                                .Average() ?? 0m,
                    TotalInAmount = totalInAmt,
                    TotalOutAmount = totalOutAmt,
                    BalanceAmount = totalInAmt - totalOutAmt,
                    SalesQuantity = 0m,
                    SalesAmount = 0m
                };
            })
            .OrderBy(r => r.LocationCode)
            .ThenBy(r => r.SubGroupCode)
                    .ThenBy(r => r.ProductName)
                    .ToList();

        if (selectedWarehouse is not null)
        {
            string selectedWarehouseCode = selectedWarehouse.Code.Value;
            grouped = grouped
                .Where(r => string.Equals(
                    r.LocationCode,
                    selectedWarehouseCode,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return grouped;
    }

    private static (ChartOfAccountType accountType, string code, string name) ResolveLocation(
        ProductMovement m,
        Dictionary<IdentityId, StockIssue> stockIssues,
        Dictionary<string, CostSlip> costSlipByNumber)
    {
        // StockIssue bağlı hareketler
        if (m.StockIssueId is { } issueId && stockIssues.TryGetValue(issueId, out StockIssue? issue))
        {
            if (issue.TargetAccount != null && issue.TargetAccount.Code.Value.StartsWith("151"))
            {
                return (issue.TargetAccount.Type, issue.TargetAccount.Code.Value, issue.TargetAccount.Name.Value);
            }

            ChartOfAccount account = m.MovementType == ProductMovementType.Input
                ? issue.TargetAccount!
                : issue.SourceWarehouse!;
            return (account.Type, account.Code.Value, account.Name.Value);
        }

        // Maliyet Pusulası hareketleri → CostSlip.Workshop
        string d = m.Description.Value ?? "";
        if (d.StartsWith(ProductStockBalanceHelper.CostSlipConsumptionOutputDescriptionPrefix))
        {
            string after = d[ProductStockBalanceHelper.CostSlipConsumptionOutputDescriptionPrefix.Length..].Trim();
            if (TryFindCostSlip(after, costSlipByNumber, out CostSlip? slip) && slip is not null && slip.Workshop is { })
            {
                return (slip.Workshop.Type, slip.Workshop.Code.Value, slip.Workshop.Name.Value);
            }
            return (ChartOfAccountType.Workshop, "?", "Bilinmeyen Atölye");
        }

        if (d.StartsWith(ProductStockBalanceHelper.ProductionInputDescriptionPrefix))
        {
            return (m.Product!.Warehouse!.Type, m.Product!.Warehouse!.Code.Value, m.Product!.Warehouse!.Name.Value);
        }

        // Fatura / manuel hareketler → Product.Warehouse
        return (m.Product!.Warehouse!.Type, m.Product!.Warehouse!.Code.Value, m.Product!.Warehouse!.Name.Value);
    }

    private static bool TryFindCostSlip(string after, Dictionary<string, CostSlip> costSlipByNumber, out CostSlip? slip)
    {
        string slipNo = after.Split(' ')[0];
        return costSlipByNumber.TryGetValue(slipNo, out slip);
    }
}