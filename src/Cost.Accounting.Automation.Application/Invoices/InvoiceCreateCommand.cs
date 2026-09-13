using Cost.Accounting.Automation.Application.Behaviors;
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
    string? Description);

[Permission("invoice:create")]
public sealed record InvoiceCreateCommand(
    string InvoiceNumber,
    InvoiceType InvoiceType,
    DateOnly Date,
    Guid? CustomerId,
    Guid? SupplierId,
    string Description,
    List<InvoiceCreateLineModel> Lines) : IRequest<Result<string>>;

public sealed class InvoiceCreateCommandValidator : AbstractValidator<InvoiceCreateCommand>
{
    public InvoiceCreateCommandValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Fatura numarası boş olamaz.")
            .MaximumLength(100).WithMessage("Fatura numarası en fazla 100 karakter olabilir.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Faturada en az bir kalem bulunmalıdır.");

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
                .GreaterThanOrEqualTo(0).WithMessage("Birim fiyat negatif olamaz.");
        });
    }
}

internal sealed class InvoiceCreateCommandHandler(
    IInvoiceRepository invoiceRepository,
    IProductMovementRepository productMovementRepository,
    ICurrentAccountMovementRepository currentAccountMovementRepository) : IRequestHandler<InvoiceCreateCommand, Result<string>>
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
            decimal taxAmount = Math.Round(lineSubTotal * (lineItem.TaxRateRate > 1 ? lineItem.TaxRateRate / 100m : lineItem.TaxRateRate), 2);
            decimal totalAmount = lineSubTotal + taxAmount;

            InvoiceLine line = new(
                new IdentityId(lineItem.ProductId),
                lineItem.Quantity,
                lineItem.UnitPrice,
                lineItem.TaxRateRate,
                taxAmount,
                totalAmount,
                new Description(lineItem.Description ?? string.Empty));

            invoice.AddLine(line);
        }

        await invoiceRepository.AddAsync(invoice, cancellationToken);

        // 1. Otomatik Stok Hareketleri (Ürün bazlı giriş/çıkış)
        ProductMovementType movementType = request.InvoiceType == InvoiceType.Purchase
            ? ProductMovementType.Input
            : ProductMovementType.Output;

        string movementPrefix = request.InvoiceType == InvoiceType.Purchase ? "Satın Alma Faturası" : "Satış Faturası";

        foreach (var line in invoice.Lines)
        {
            ProductMovement movement = new(
                productId: line.ProductId,
                movementType: movementType,
                quantity: line.Quantity,
                unitPrice: new Price(line.UnitPrice),
                date: invoice.Date,
                referenceNo: invoice.InvoiceNumber,
                description: new Description($"{movementPrefix} - {invoice.InvoiceNumber}"),
                invoiceId: invoice.Id);

            await productMovementRepository.AddAsync(movement, cancellationToken);
        }

        // 2. Otomatik Cari Hareketi (Müşteri/Tedarikçi borç/alacak)
        CurrentAccountMovement currentAccountMovement;
        if (request.InvoiceType == InvoiceType.Sales)
        {
            currentAccountMovement = new CurrentAccountMovement(
                currentAccountType: CurrentAccountType.Customer,
                customerId: customerId,
                supplierId: null,
                date: invoice.Date,
                movementType: CurrentAccountMovementType.SalesInvoice,
                documentNo: invoice.InvoiceNumber,
                debit: invoice.GrandTotal, // Satışta müşteri borçlanır
                credit: 0,
                description: new Description($"Satış Faturası - {invoice.InvoiceNumber}"),
                invoiceId: invoice.Id);
        }
        else
        {
            currentAccountMovement = new CurrentAccountMovement(
                currentAccountType: CurrentAccountType.Supplier,
                customerId: null,
                supplierId: supplierId,
                date: invoice.Date,
                movementType: CurrentAccountMovementType.PurchaseInvoice,
                documentNo: invoice.InvoiceNumber,
                debit: 0,
                credit: invoice.GrandTotal, // Alışta tedarikçi alacaklanır
                description: new Description($"Satın Alma Faturası - {invoice.InvoiceNumber}"),
                invoiceId: invoice.Id);
        }

        await currentAccountMovementRepository.AddAsync(currentAccountMovement, cancellationToken);

        return Result<string>.Succeed("Fatura, stok hareketleri ve cari hareketleri başarıyla kaydedildi.");
    }
}
