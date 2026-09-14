using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Products;

public enum ProductPriceType
{
    Purchase = 1,
    Sale = 2
}

public enum ProductMovementType
{
    Input = 1,
    Output = 2
}

public enum StockCostingMethod
{
    Fifo = 1,
    Lifo = 2
}

public sealed class ProductUnitType : Entity
{
    private ProductUnitType()
    {
    }

    public ProductUnitType(Name name, bool isActive)
    {
        SetName(name);
        SetStatus(isActive);
    }

    public Name Name { get; private set; } = default!;

    public void SetName(Name name) => Name = name;
}

public sealed class ProductPrice : Entity, IHardDeletable
{
    private ProductPrice()
    {
    }

    public ProductPrice(Price unitPrice, ProductPriceType priceType, DateOnly startDate, DateOnly? endDate)
    {
        UnitPrice = unitPrice;
        PriceType = priceType;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Price UnitPrice { get; private set; } = default!;
    public ProductPriceType PriceType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
}

public sealed class ProductMovement : Entity, IHardDeletable
{
    private ProductMovement()
    {
    }

    public ProductMovement(
        ProductMovementType movementType,
        decimal quantity,
        Price? unitPrice,
        DateOnly date,
        string? referenceNo,
        Description description)
    {
        MovementType = movementType;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Date = date;
        ReferenceNo = referenceNo;
        Description = description;
    }

    public ProductMovement(
        IdentityId productId,
        ProductMovementType movementType,
        decimal quantity,
        Price? unitPrice,
        DateOnly date,
        string? referenceNo,
        Description description,
        IdentityId? invoiceId = null)
        : this(movementType, quantity, unitPrice, date, referenceNo, description)
    {
        ProductId = productId;
        InvoiceId = invoiceId;
    }

    public IdentityId ProductId { get; private set; } = default!;
    public Product? Product { get; private set; }
    public IdentityId? InvoiceId { get; private set; }
    public ProductMovementType MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public Price? UnitPrice { get; private set; }
    public DateOnly Date { get; private set; }
    public string? ReferenceNo { get; private set; }
    public Description Description { get; private set; } = default!;

    public void SetProduct(IdentityId productId) => ProductId = productId;
    public void SetInvoiceId(IdentityId? invoiceId) => InvoiceId = invoiceId;
}