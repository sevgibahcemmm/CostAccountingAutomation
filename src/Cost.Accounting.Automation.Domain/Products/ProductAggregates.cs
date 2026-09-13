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

    public ProductMovementType MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public Price? UnitPrice { get; private set; }
    public DateOnly Date { get; private set; }
    public string? ReferenceNo { get; private set; }
    public Description Description { get; private set; } = default!;
}