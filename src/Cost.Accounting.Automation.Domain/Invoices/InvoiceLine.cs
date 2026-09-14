using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Invoices;

public sealed class InvoiceLine : Entity, IHardDeletable
{
    private InvoiceLine()
    {
    }

    public InvoiceLine(
        IdentityId productId,
        decimal quantity,
        decimal unitPrice,
        decimal discountRate,
        decimal taxRateRate,
        decimal taxAmount,
        decimal totalAmount,
        Description description)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountRate = discountRate;
        TaxRateRate = taxRateRate;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        Description = description;
    }

    public IdentityId InvoiceId { get; private set; } = default!;
    public IdentityId ProductId { get; private set; } = default!;
    public Product? Product { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountRate { get; private set; }
    public decimal TaxRateRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Description Description { get; private set; } = default!;

    public void SetInvoiceId(IdentityId invoiceId) => InvoiceId = invoiceId;

    public void Update(
        IdentityId productId,
        decimal quantity,
        decimal unitPrice,
        decimal discountRate,
        decimal taxRateRate,
        decimal taxAmount,
        decimal totalAmount,
        Description description)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountRate = discountRate;
        TaxRateRate = taxRateRate;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        Description = description;
    }
}
