using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Invoices;

[Permission("invoice:update")]
public sealed record InvoiceUpdateCommand(
    Guid Id,
    string InvoiceNumber,
    InvoiceType InvoiceType,
    DateOnly Date,
    Guid? CustomerId,
    Guid? SupplierId,
    string Description,
    List<InvoiceCreateLineModel> Lines) : IRequest<Result<string>>;

public sealed class InvoiceUpdateCommandValidator : AbstractValidator<InvoiceUpdateCommand>
{
    public InvoiceUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir fatura ID girin.");

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

internal sealed class InvoiceUpdateCommandHandler(
    IInvoiceRepository invoiceRepository) : IRequestHandler<InvoiceUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InvoiceUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        Invoice? invoice = await invoiceRepository
            .WhereWithTracking(i => i.Id == id)
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            return Result<string>.Failure("Fatura bulunamadı.");
        }

        if (invoice.Status == InvoiceStatus.Approved)
        {
            return Result<string>.Failure("Onaylanmış faturalar düzenlenemez.");
        }

        bool numberExists = await invoiceRepository.AnyAsync(
            i => i.InvoiceNumber == request.InvoiceNumber.Trim()
                && i.InvoiceType == request.InvoiceType
                && i.Id != id,
            cancellationToken);

        if (numberExists)
        {
            return Result<string>.Failure("Bu fatura numarası başka bir fatura tarafından kullanılıyor.");
        }

        invoice.SetInvoiceNumber(request.InvoiceNumber.Trim());
        invoice.SetDate(request.Date);
        invoice.SetCustomer(request.CustomerId.HasValue ? new IdentityId(request.CustomerId.Value) : null);
        invoice.SetSupplier(request.SupplierId.HasValue ? new IdentityId(request.SupplierId.Value) : null);
        invoice.SetDescription(new Description(request.Description ?? string.Empty));

        List<InvoiceLine> lines = request.Lines
            .Select(lineItem =>
            {
                decimal lineSubTotal = lineItem.Quantity * lineItem.UnitPrice;
                decimal discountAmount = Math.Round(
                    lineSubTotal * (lineItem.DiscountRate / 100m), 2);
                decimal netAmount = lineSubTotal - discountAmount;
                decimal taxAmount = Math.Round(netAmount * (lineItem.TaxRateRate / 100m), 2);
                decimal totalAmount = netAmount + taxAmount;

                return new InvoiceLine(
                    new IdentityId(lineItem.ProductId),
                    lineItem.Quantity,
                    lineItem.UnitPrice,
                    lineItem.DiscountRate,
                    lineItem.TaxRateRate,
                    taxAmount,
                    totalAmount,
                    new Description(lineItem.Description ?? string.Empty));
            })
            .ToList();

        invoice.ReplaceLines(lines);
        invoiceRepository.Update(invoice);

        return Result<string>.Succeed("Fatura taslağı başarıyla güncellendi.");
    }
}