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

public enum ProductMovementReason
{
    [System.ComponentModel.DataAnnotations.Display(Name = "Genel")]
    General = 1,          // Genel / Normal işlem
    [System.ComponentModel.DataAnnotations.Display(Name = "Sayım Fazlası")]
    CountingSurplus = 2,  // Sayım Fazlası (Giriş)
    [System.ComponentModel.DataAnnotations.Display(Name = "Sayım Noksanı")]
    CountingDeficit = 3,  // Sayım Noksanı (Çıkış)
    [System.ComponentModel.DataAnnotations.Display(Name = "Fire / Zayi")]
    Fire = 4,             // Fire / Zayi (Çıkış)
    [System.ComponentModel.DataAnnotations.Display(Name = "Numune / Hediye")]
    Sample = 5,           // Numune / Hediye
    [System.ComponentModel.DataAnnotations.Display(Name = "Devir / Transfer")]
    Transfer = 6          // Devir / Transfer
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

    public static string? BuildDuplicateKey(string name)
        => DuplicateKeyRule.From(name);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(Name.Value));

    public void SetName(Name name)
    {
        Name = name;
        ResolveDuplicateKey();
    }
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

public record ProductPriceQueryResult(Guid ProductId, Guid Id, ProductPriceType PriceType, decimal UnitPrice, DateOnly StartDate, DateOnly? EndDate);

public record ProductMovementQueryResult(
    Guid ProductId,
    Guid Id,
    ProductMovementType MovementType,
    ProductMovementReason Reason,
    decimal Quantity,
    decimal? UnitPrice,
    DateOnly Date,
    string? ReferenceNo,
    string Description,
    Guid? InvoiceId,
    Guid? StockIssueId);

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
        Description description,
        ProductMovementReason reason = ProductMovementReason.General)
    {
        MovementType = movementType;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Date = date;
        ReferenceNo = referenceNo;
        Description = description;
        Reason = reason;
    }

    public ProductMovement(
        IdentityId productId,
        ProductMovementType movementType,
        decimal quantity,
        Price? unitPrice,
        DateOnly date,
        string? referenceNo,
        Description description,
        ProductMovementReason reason = ProductMovementReason.General,
        IdentityId? invoiceId = null,
        IdentityId? stockIssueId = null)
        : this(movementType, quantity, unitPrice, date, referenceNo, description, reason)
    {
        ProductId = productId;
        InvoiceId = invoiceId;
        StockIssueId = stockIssueId;
        ResolveDuplicateKey();
    }

    public static string? BuildDuplicateKey(
        IdentityId? productId,
        ProductMovementType movementType,
        decimal quantity,
        decimal? unitPrice,
        DateOnly date,
        string? referenceNo)
    {
        if (productId is null || string.IsNullOrWhiteSpace(referenceNo))
        {
            return null;
        }

        return DuplicateKeyRule.From(
            productId.Value,
            (int)movementType,
            quantity,
            unitPrice,
            date.ToString("yyyy-MM-dd"),
            referenceNo);
    }

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(ProductId, MovementType, Quantity, UnitPrice?.Value, Date, ReferenceNo));

    public IdentityId ProductId { get; private set; } = default!;
    public Product? Product { get; private set; }
    public IdentityId? InvoiceId { get; private set; }
    public IdentityId? StockIssueId { get; private set; }
    public ProductMovementType MovementType { get; private set; }
    public ProductMovementReason Reason { get; private set; }
    public decimal Quantity { get; private set; }
    public Price? UnitPrice { get; private set; }
    public DateOnly Date { get; private set; }
    public string? ReferenceNo { get; private set; }
    public Description Description { get; private set; } = default!;

    public void SetProduct(IdentityId productId)
    {
        ProductId = productId;
        ResolveDuplicateKey();
    }
    public void SetInvoiceId(IdentityId? invoiceId) => InvoiceId = invoiceId;
    public void SetStockIssueId(IdentityId? stockIssueId) => StockIssueId = stockIssueId;
}