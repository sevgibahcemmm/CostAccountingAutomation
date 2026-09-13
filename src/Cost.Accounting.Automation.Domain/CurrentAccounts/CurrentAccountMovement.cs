using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Suppliers;

namespace Cost.Accounting.Automation.Domain.CurrentAccounts;

public sealed class CurrentAccountMovement : Entity, IHardDeletable
{
    private CurrentAccountMovement()
    {
    }

    public CurrentAccountMovement(
        CurrentAccountType currentAccountType,
        IdentityId? customerId,
        IdentityId? supplierId,
        DateOnly date,
        CurrentAccountMovementType movementType,
        string? documentNo,
        decimal debit,
        decimal credit,
        Description description,
        IdentityId? invoiceId = null,
        bool isActive = true)
    {
        CurrentAccountType = currentAccountType;
        CustomerId = customerId;
        SupplierId = supplierId;
        Date = date;
        MovementType = movementType;
        DocumentNo = documentNo;
        Debit = debit;
        Credit = credit;
        Description = description;
        InvoiceId = invoiceId;
        SetStatus(isActive);
    }

    public CurrentAccountType CurrentAccountType { get; private set; }
    public IdentityId? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public IdentityId? SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }

    public DateOnly Date { get; private set; }
    public CurrentAccountMovementType MovementType { get; private set; }
    public string? DocumentNo { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public Description Description { get; private set; } = default!;
    public IdentityId? InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    public void SetCustomer(IdentityId? customerId)
    {
        CustomerId = customerId;
        CurrentAccountType = CurrentAccountType.Customer;
    }

    public void SetSupplier(IdentityId? supplierId)
    {
        SupplierId = supplierId;
        CurrentAccountType = CurrentAccountType.Supplier;
    }

    public void Update(
        DateOnly date,
        CurrentAccountMovementType movementType,
        string? documentNo,
        decimal debit,
        decimal credit,
        Description description)
    {
        Date = date;
        MovementType = movementType;
        DocumentNo = documentNo;
        Debit = debit;
        Credit = credit;
        Description = description;
    }
}
