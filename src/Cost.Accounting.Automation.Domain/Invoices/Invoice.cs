using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Suppliers;

namespace Cost.Accounting.Automation.Domain.Invoices;

public sealed class Invoice : Entity, IHardDeletable
{
    private readonly List<InvoiceLine> _lines = [];

    private Invoice()
    {
    }

    public Invoice(
        string invoiceNumber,
        InvoiceType invoiceType,
        DateOnly date,
        IdentityId? customerId,
        IdentityId? supplierId,
        Description description,
        bool isActive = true)
    {
        SetInvoiceNumber(invoiceNumber);
        InvoiceType = invoiceType;
        Date = date;
        CustomerId = customerId;
        SupplierId = supplierId;
        Description = description;
        SetStatus(isActive);
        ResolveDuplicateKey();
    }

    public static string? BuildDuplicateKey(string invoiceNumber, InvoiceType invoiceType)
        => DuplicateKeyRule.From(invoiceNumber, ((int)invoiceType).ToString());

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(InvoiceNumber, InvoiceType));

    public string InvoiceNumber { get; private set; } = default!;
    public InvoiceType InvoiceType { get; private set; }
    public DateOnly Date { get; private set; }

    public IdentityId? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public IdentityId? SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }

    public Description Description { get; private set; } = default!;

    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    public decimal SubTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }

    public IReadOnlyCollection<InvoiceLine> Lines => _lines;

    public void SetInvoiceNumber(string invoiceNumber)
    {
        InvoiceNumber = invoiceNumber;
        ResolveDuplicateKey();
    }
    public void SetDate(DateOnly date) => Date = date;
    public void SetCustomer(IdentityId? customerId) => CustomerId = customerId;
    public void SetSupplier(IdentityId? supplierId) => SupplierId = supplierId;
    public void SetDescription(Description description) => Description = description;

    public void Approve()
    {
        if (Status == InvoiceStatus.Draft)
        {
            Status = InvoiceStatus.Approved;
        }
    }

    public void AddLine(InvoiceLine line)
    {
        line.SetInvoiceId(Id);
        _lines.Add(line);
        CalculateTotals();
    }

    public void ReplaceLines(IEnumerable<InvoiceLine> lines)
    {
        _lines.Clear();
        foreach (var line in lines)
        {
            line.SetInvoiceId(Id);
            _lines.Add(line);
        }
        CalculateTotals();
    }

    public void CalculateTotals()
    {
        SubTotal = _lines.Sum(l => l.Quantity * l.UnitPrice);
        DiscountTotal = _lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountRate / 100m);
        TaxTotal = _lines.Sum(l => l.TaxAmount);
        GrandTotal = _lines.Sum(l => l.TotalAmount);
    }
}
