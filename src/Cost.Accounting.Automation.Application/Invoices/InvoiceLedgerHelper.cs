using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Application.Invoices;

internal static class InvoiceLedgerHelper
{
    public static async Task CreateLedgerMovementsAsync(
        Invoice invoice,
        IProductMovementRepository productMovementRepository,
        ICurrentAccountMovementRepository currentAccountMovementRepository,
        IProductRepository productRepository,
        IChartOfAccountLedgerPoster ledgerPoster,
        StockCostingMethod costingMethod,
        CancellationToken cancellationToken)
    {
        bool isSales = invoice.InvoiceType == InvoiceType.Sales;
        ProductMovementType movementType = isSales ? ProductMovementType.Output : ProductMovementType.Input;

        string movementPrefix = isSales ? "Satış Faturası" : "Satın Alma Faturası";

        Dictionary<IdentityId, decimal> costMap = [];
        if (isSales)
        {
            costMap = await BuildFifoLifoCostMapAsync(invoice, productMovementRepository, costingMethod, cancellationToken);
        }

        HashSet<IdentityId> productIds = invoice.Lines.Select(l => l.ProductId).ToHashSet();
        Dictionary<Guid, IdentityId?> productAccountMap = (await productRepository
                .GetAll()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken))
            .ToDictionary(p => p.Id.Value, p => p.ChartOfAccountId);

        decimal salesCariDebit = 0;

        foreach (var line in invoice.Lines)
        {
            decimal unitCost = isSales && costMap.TryGetValue(line.ProductId, out decimal cost) ? cost : line.UnitPrice;

            ProductMovement movement = new(
                productId: line.ProductId,
                movementType: movementType,
                quantity: line.Quantity,
                unitPrice: new Price(unitCost),
                date: invoice.Date,
                referenceNo: invoice.InvoiceNumber,
                description: new Description($"{movementPrefix} - {invoice.InvoiceNumber}"),
                invoiceId: invoice.Id);

            await productMovementRepository.AddAsync(movement, cancellationToken);

            if (productAccountMap.TryGetValue(line.ProductId.Value, out IdentityId? accountId) && accountId is not null)
            {
                decimal amount = Math.Round(line.Quantity * unitCost, 2);

                if (isSales)
                {
                    await ledgerPoster.PostAsync(accountId, 0, amount, "SatisFaturasi", movement.Id, cancellationToken);
                }
                else
                {
                    await ledgerPoster.PostAsync(accountId, amount, 0, "SatinalmaFaturasi", movement.Id, cancellationToken);
                }
            }

            if (isSales)
            {
                decimal rate = line.TaxRateRate > 1 ? line.TaxRateRate / 100m : line.TaxRateRate;
                decimal discountRate = line.DiscountRate > 1 && line.DiscountRate <= 100 ? line.DiscountRate / 100m : line.DiscountRate;
                decimal lineCostedNet = line.Quantity * unitCost * (1m - discountRate);
                decimal lineTax = Math.Round(lineCostedNet * rate, 2);
                salesCariDebit += lineCostedNet + lineTax;
            }
        }

        CurrentAccountMovement currentAccountMovement;
        if (isSales)
        {
            currentAccountMovement = new CurrentAccountMovement(
                currentAccountType: CurrentAccountType.Customer,
                customerId: invoice.CustomerId,
                supplierId: null,
                date: invoice.Date,
                movementType: CurrentAccountMovementType.SalesInvoice,
                documentNo: invoice.InvoiceNumber,
                debit: Math.Round(salesCariDebit, 2),
                credit: 0,
                description: new Description($"Satış Faturası - {invoice.InvoiceNumber}"),
                invoiceId: invoice.Id);
        }
        else
        {
            currentAccountMovement = new CurrentAccountMovement(
                currentAccountType: CurrentAccountType.Supplier,
                customerId: null,
                supplierId: invoice.SupplierId,
                date: invoice.Date,
                movementType: CurrentAccountMovementType.PurchaseInvoice,
                documentNo: invoice.InvoiceNumber,
                debit: 0,
                credit: invoice.GrandTotal,
                description: new Description($"Satın Alma Faturası - {invoice.InvoiceNumber}"),
                invoiceId: invoice.Id);
        }

        await currentAccountMovementRepository.AddAsync(currentAccountMovement, cancellationToken);
    }

    private static async Task<Dictionary<IdentityId, decimal>> BuildFifoLifoCostMapAsync(
        Invoice invoice,
        IProductMovementRepository productMovementRepository,
        StockCostingMethod costingMethod,
        CancellationToken cancellationToken)
    {
        HashSet<IdentityId> productIds = invoice.Lines.Select(l => l.ProductId).ToHashSet();

        List<ProductMovement> all = await productMovementRepository.GetAll()
            .Where(m => productIds.Contains(m.ProductId))
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id.Value)
            .ToListAsync(cancellationToken);

        return ComputeFifoLifoCosts(invoice, all, costingMethod);
    }

    private static Dictionary<IdentityId, decimal> ComputeFifoLifoCosts(
        Invoice invoice,
        List<ProductMovement> priorMovements,
        StockCostingMethod costingMethod)
    {
        Dictionary<IdentityId, decimal> result = [];

        var group = priorMovements
            .Where(m => m.InvoiceId != invoice.Id)
            .Where(m => !m.IsDeleted)
            .GroupBy(m => m.ProductId);

        foreach (var product in group)
        {
            var layers = new List<(decimal Quantity, decimal UnitPrice)>();

            foreach (var m in product.Where(x => x.MovementType == ProductMovementType.Input))
            {
                if (m.UnitPrice is { } price)
                {
                    layers.Add((m.Quantity, price.Value));
                }
            }

            var priorOutputs = product.Where(x => x.MovementType == ProductMovementType.Output).ToList();
            ConsumeLayers(layers, priorOutputs.Sum(o => o.Quantity), costingMethod);

            decimal invoiceQty = invoice.Lines.First(l => l.ProductId == product.Key).Quantity;
            decimal cost = ConsumeLayerCost(layers, invoiceQty, costingMethod);

            result[product.Key] = cost > 0 ? cost / invoiceQty : 0;
        }

        return result;
    }

    private static void ConsumeLayers(List<(decimal Quantity, decimal UnitPrice)> layers, decimal quantity, StockCostingMethod method)
    {
        decimal remaining = quantity;
        int count = layers.Count;

        while (remaining > 0 && count > 0)
        {
            int index = method == StockCostingMethod.Fifo ? 0 : count - 1;
            var layer = layers[index];
            decimal consumed = Math.Min(layer.Quantity, remaining);
            remaining -= consumed;
            layer.Quantity -= consumed;
            layers[index] = layer;

            if (layer.Quantity <= 0)
            {
                layers.RemoveAt(index);
            }

            count = layers.Count;
        }
    }

    private static decimal ConsumeLayerCost(List<(decimal Quantity, decimal UnitPrice)> layers, decimal quantity, StockCostingMethod method)
    {
        decimal remaining = quantity;
        decimal totalCost = 0;
        int count = layers.Count;

        while (remaining > 0 && count > 0)
        {
            int index = method == StockCostingMethod.Fifo ? 0 : count - 1;
            var layer = layers[index];
            decimal consumed = Math.Min(layer.Quantity, remaining);
            totalCost += consumed * layer.UnitPrice;
            remaining -= consumed;
            layer.Quantity -= consumed;
            layers[index] = layer;

            if (layer.Quantity <= 0)
            {
                layers.RemoveAt(index);
            }

            count = layers.Count;
        }

        return totalCost;
    }
}
