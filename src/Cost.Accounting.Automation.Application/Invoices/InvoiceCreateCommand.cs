using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Invoices;

public sealed record InvoiceCreateLineModel(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRateRate,
    string? Description,
    decimal DiscountRate = 0);

[Permission("invoice:create")]
public sealed record InvoiceCreateCommand(
    string InvoiceNumber,
    InvoiceType InvoiceType,
    DateOnly Date,
    Guid? CustomerId,
    Guid? SupplierId,
    string Description,
    List<InvoiceCreateLineModel> Lines,
    bool IsApproved = false,
    StockCostingMethod CostingMethod = StockCostingMethod.Fifo) : IRequest<Result<string>>;

public sealed class InvoiceCreateCommandValidator : AbstractValidator<InvoiceCreateCommand>
{
    public InvoiceCreateCommandValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Fatura numarası boş olamaz.")
            .MaximumLength(100).WithMessage("Fatura numarası en fazla 100 karakter olabilir.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Faturada en az bir kalem bulunmalıdır.");

        RuleFor(x => x.Lines)
            .Must(lines => lines.GroupBy(l => l.ProductId).All(g => g.Count() == 1))
            .WithMessage("Aynı ürün faturada yalnızca bir kez yer alabilir.");

        When(x => x.InvoiceType == InvoiceType.Sales, () =>
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Satış faturası için müşteri seçilmelidir.");
        });

        When(x => x.InvoiceType == InvoiceType.Purchase, () =>
        {
            RuleFor(x => x.SupplierId)
                .NotEmpty().WithMessage("Satın alma faturası için tedarikçi seçilmelidir.");
        });

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId)
                .NotEmpty().WithMessage("Ürün seçilmelidir.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThan(0).WithMessage("Birim fiyat sıfırdan büyük olmalıdır.");

            line.RuleFor(l => l.DiscountRate)
                .InclusiveBetween(0, 100).WithMessage("İskonto oranı %0 ile %100 arasında olmalıdır.");
        });
    }
}

internal sealed class InvoiceCreateCommandHandler(
    IInvoiceRepository invoiceRepository,
    IProductMovementRepository productMovementRepository,
    ICurrentAccountMovementRepository currentAccountMovementRepository,
    IProductRepository productRepository,
    IChartOfAccountLedgerPoster ledgerPoster) : IRequestHandler<InvoiceCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InvoiceCreateCommand request, CancellationToken cancellationToken)
    {
        bool invoiceExists = await invoiceRepository.AnyAsync(
            i => i.InvoiceNumber == request.InvoiceNumber && i.InvoiceType == request.InvoiceType,
            cancellationToken);

        if (invoiceExists)
        {
            return Result<string>.Failure("Bu fatura numarası ile kaydedilmiş bir fatura zaten mevcut.");
        }

        IdentityId? customerId = request.CustomerId.HasValue ? new IdentityId(request.CustomerId.Value) : null;
        IdentityId? supplierId = request.SupplierId.HasValue ? new IdentityId(request.SupplierId.Value) : null;

        Invoice invoice = new(
            request.InvoiceNumber.Trim(),
            request.InvoiceType,
            request.Date,
            customerId,
            supplierId,
            new Description(request.Description ?? string.Empty));

        foreach (var lineItem in request.Lines)
        {
            decimal lineSubTotal = lineItem.Quantity * lineItem.UnitPrice;
            decimal discountAmount = Math.Round(lineSubTotal * (lineItem.DiscountRate > 1 && lineItem.DiscountRate <= 100 ? lineItem.DiscountRate / 100m : lineItem.DiscountRate), 2);
            decimal netAmount = lineSubTotal - discountAmount;
            decimal taxAmount = Math.Round(netAmount * (lineItem.TaxRateRate > 1 ? lineItem.TaxRateRate / 100m : lineItem.TaxRateRate), 2);
            decimal totalAmount = netAmount + taxAmount;

            InvoiceLine line = new(
                new IdentityId(lineItem.ProductId),
                lineItem.Quantity,
                lineItem.UnitPrice,
                lineItem.DiscountRate,
                lineItem.TaxRateRate,
                taxAmount,
                totalAmount,
                new Description(lineItem.Description ?? string.Empty));

            invoice.AddLine(line);
        }

        await invoiceRepository.AddAsync(invoice, cancellationToken);

        if (request.IsApproved)
        {
            invoice.Approve();
            await InvoiceLedgerHelper.CreateLedgerMovementsAsync(
                invoice,
                productMovementRepository,
                currentAccountMovementRepository,
                productRepository,
                ledgerPoster,
                request.CostingMethod,
                cancellationToken);

            return Result<string>.Succeed("Fatura, stok hareketleri ve cari hareketleri başarıyla kaydedildi.");
        }

        return Result<string>.Succeed("Fatura taslak olarak kaydedildi. Onaylanınca stok ve cari hareketleri oluşturulacak.");
    }
}
