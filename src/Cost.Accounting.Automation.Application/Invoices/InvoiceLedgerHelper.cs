using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Invoices;

internal static class InvoiceLedgerHelper
{
    /// <summary>
    /// Stok ÇIKIŞI üreten faturalar (Satış, Alış İade) için onaylanmadan önce
    /// "girişi olmayanın çıkışı olamaz" kuralını doğrular.
    /// Başarılıysa null, aksi halde hata mesajı döndürür.
    /// </summary>
    internal static async Task<string?> ValidateOutputStockAsync(
        Invoice invoice,
        IProductMovementRepository productMovementRepository,
        IProductRepository productRepository,
        CancellationToken cancellationToken)
    {
        if (invoice.InvoiceType != InvoiceType.Sales && invoice.InvoiceType != InvoiceType.PurchaseReturn)
        {
            return null;
        }

        if (invoice.Lines.Count == 0)
        {
            return null;
        }

        Dictionary<IdentityId, decimal> requestedQuantities = invoice.Lines
            .GroupBy(l => l.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        List<IdentityId> productIds = requestedQuantities.Keys.ToList();

        List<ProductMovement> movements = await StockIssueCostingHelper.LoadMovementsAsync(
            productIds,
            productMovementRepository,
            cancellationToken);

        Dictionary<IdentityId, string> productNames;
        var products = await productRepository.GetAll()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
        productNames = products.ToDictionary(p => p.Id, p => p.Name.Value);

        foreach (KeyValuePair<IdentityId, decimal> requested in requestedQuantities)
        {
            decimal available = StockIssueCostingHelper.ComputeAvailableQuantity(
                movements,
                requested.Key,
                invoice.Date);

            if (requested.Value > available)
            {
                string productName = productNames.TryGetValue(requested.Key, out string? name)
                    ? name
                    : requested.Key.Value.ToString();

                return $"'{productName}' için bu tarihe kadar yeterli giriş (stok) yok. Mevcut: {available:n2}, istenen: {requested.Value:n2}.";
            }
        }

        return null;
    }

    public static async Task CreateLedgerMovementsAsync(
        Invoice invoice,
        IProductMovementRepository productMovementRepository,
        ICurrentAccountMovementRepository currentAccountMovementRepository,
        IProductRepository productRepository,
        IChartOfAccountLedgerPoster ledgerPoster,
        StockCostingMethod costingMethod,
        CancellationToken cancellationToken)
    {
        InvoiceType type = invoice.InvoiceType;
        bool isPurchaseSide = type == InvoiceType.Purchase || type == InvoiceType.PurchaseReturn;
        bool isSalesSide = type == InvoiceType.Sales || type == InvoiceType.SalesReturn;
        bool isCustomerSide = isSalesSide;
        bool isReturn = type == InvoiceType.PurchaseReturn || type == InvoiceType.SalesReturn;

        ProductMovementType? stockMovementType = type switch
        {
            InvoiceType.Purchase => ProductMovementType.Input,
            InvoiceType.PurchaseReturn => ProductMovementType.Output,
            InvoiceType.Sales => ProductMovementType.Output,
            InvoiceType.SalesReturn => ProductMovementType.Input,
            _ => null
        };

        string movementPrefix = type switch
        {
            InvoiceType.Purchase => "Satın Alma Faturası",
            InvoiceType.PurchaseReturn => "Alış İade Faturası",
            InvoiceType.Sales => "Satış Faturası",
            InvoiceType.SalesReturn => "Satış İade Faturası",
            _ => "Fatura"
        };

        Dictionary<IdentityId, decimal> costMap = [];
        if (isSalesSide)
        {
            costMap = await BuildFifoLifoCostMapAsync(invoice, productMovementRepository, costingMethod, cancellationToken);
        }

        decimal customerBalance = 0;

        if (stockMovementType is { } stockType)
        {
            HashSet<IdentityId> productIds = invoice.Lines.Select(l => l.ProductId).ToHashSet();
            Dictionary<Guid, IdentityId?> productAccountMap = (await productRepository
                    .GetAll()
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync(cancellationToken))
                .ToDictionary(p => p.Id.Value, p => p.ChartOfAccountId);

            foreach (var line in invoice.Lines)
            {
                decimal unitCost = isSalesSide && costMap.TryGetValue(line.ProductId, out decimal cost)
                    ? cost
                    : line.UnitPrice;

                ProductMovement movement = new(
                    productId: line.ProductId,
                    movementType: stockType,
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

                    if (stockType == ProductMovementType.Input)
                    {
                        string tag = type == InvoiceType.SalesReturn ? "SatisIadeFaturasi" : "SatinalmaFaturasi";
                        await ledgerPoster.PostAsync(accountId, amount, 0, tag, movement.Id, cancellationToken);
                    }
                    else
                    {
                        string tag = type == InvoiceType.PurchaseReturn ? "AlisIadeFaturasi" : "SatisFaturasi";
                        await ledgerPoster.PostAsync(accountId, 0, amount, tag, movement.Id, cancellationToken);
                    }
                }

                if (!isPurchaseSide && !isReturn)
                {
                    decimal rate = line.TaxRateRate / 100m;
                    decimal discountRate = line.DiscountRate / 100m;
                    decimal lineCostedNet = line.Quantity * unitCost * (1m - discountRate);
                    decimal lineTax = Math.Round(lineCostedNet * rate, 2);
                    customerBalance += lineCostedNet + lineTax;
                }
            }
        }

        if (isPurchaseSide && !isReturn)
        {
            // Satın alma faturası onaylandığında birim fiyat Fiyat Tablosu'na (Alış) işlenir.
            foreach (var line in invoice.Lines)
            {
                await RecordPurchasePriceAsync(
                    productRepository, line.ProductId, line.UnitPrice, invoice.Date, cancellationToken);
            }
        }

        CurrentAccountMovement currentAccountMovement;
        if (isCustomerSide)
        {
            decimal debit = type switch
            {
                InvoiceType.SalesReturn => 0,
                _ => Math.Round(customerBalance, 2)
            };
            decimal credit = type == InvoiceType.SalesReturn ? invoice.GrandTotal : 0;

            currentAccountMovement = new CurrentAccountMovement(
                currentAccountType: CurrentAccountType.Customer,
                customerId: invoice.CustomerId,
                supplierId: null,
                date: invoice.Date,
                movementType: type switch
                {
                    InvoiceType.SalesReturn => CurrentAccountMovementType.SalesReturnInvoice,
                    _ => CurrentAccountMovementType.SalesInvoice
                },
                documentNo: invoice.InvoiceNumber,
                debit: debit,
                credit: credit,
                description: new Description($"{movementPrefix} - {invoice.InvoiceNumber}"),
                invoiceId: invoice.Id);
        }
        else if (type == InvoiceType.PurchaseReturn)
        {
            currentAccountMovement = new CurrentAccountMovement(
                currentAccountType: CurrentAccountType.Supplier,
                customerId: null,
                supplierId: invoice.SupplierId,
                date: invoice.Date,
                movementType: CurrentAccountMovementType.PurchaseReturnInvoice,
                documentNo: invoice.InvoiceNumber,
                debit: invoice.GrandTotal,
                credit: 0,
                description: new Description($"Alış İade Faturası - {invoice.InvoiceNumber}"),
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

    private static async Task RecordPurchasePriceAsync(
        IProductRepository productRepository,
        IdentityId productId,
        decimal unitPrice,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdWithDetailsAsync(productId, cancellationToken);
        if (product is null)
        {
            return;
        }

        // Aynı gün ve aynı tutarla tekrar kayıt eklenmesin (yeniden onay/geri yükleme senaryoları).
        bool alreadyRecorded = product.Prices.Any(p =>
            p.PriceType == ProductPriceType.Purchase
            && p.StartDate == date
            && p.UnitPrice.Value == unitPrice);

        if (alreadyRecorded)
        {
            return;
        }

        product.AddPrice(new Price(unitPrice), ProductPriceType.Purchase, date);
        productRepository.Update(product);
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
            .ToListAsync(cancellationToken);

        return ComputeFifoLifoCosts(invoice, all.OrderBy(m => m.Date).ThenBy(m => m.Id.Value).ToList(), costingMethod);
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
            .Where(m => m.Date <= invoice.Date)
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
