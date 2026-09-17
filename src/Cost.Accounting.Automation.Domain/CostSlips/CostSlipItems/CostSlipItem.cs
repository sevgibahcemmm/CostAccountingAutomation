using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;

public sealed class CostSlipItem : Entity, IHardDeletable
{
    private CostSlipItem()
    {
    }

    public CostSlipItem(
        IdentityId costSlipId,
        IdentityId? productId,
        IdentityId? productUnitTypeId,
        ExpenseAccountType expenseAccountType,
        decimal quantity,
        decimal unitPrice,
        string? description)
    {
        SetCostSlipId(costSlipId);
        SetProduct(productId);
        SetProductUnitType(productUnitTypeId);
        SetExpenseAccountType(expenseAccountType);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
        SetDescription(description);
    }

    public IdentityId CostSlipId { get; private set; } = default!;
    public CostSlip? CostSlip { get; private set; }

    public IdentityId? ProductId { get; private set; }
    public Product? Product { get; private set; }

    public IdentityId? ProductUnitTypeId { get; private set; }
    public ProductUnitType? ProductUnitType { get; private set; }

    public ExpenseAccountType ExpenseAccountType { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal TotalAmount => Quantity * UnitPrice;

    public Description Description { get; private set; } = default!;

    #region Behaviors

    public void SetCostSlipId(IdentityId costSlipId)
    {
        CostSlipId = costSlipId ?? throw new ArgumentNullException(nameof(costSlipId));
    }

    public void SetProduct(IdentityId? productId)
    {
        ProductId = productId;
    }

    public void SetProductUnitType(IdentityId? productUnitTypeId)
    {
        ProductUnitTypeId = productUnitTypeId;
    }

    public void SetExpenseAccountType(ExpenseAccountType expenseAccountType)
    {
        ExpenseAccountType = expenseAccountType;
    }

    public void SetQuantity(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));
        Quantity = quantity;
    }

    public void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0) throw new ArgumentException("Birim fiyat 0'dan küçük olamaz.", nameof(unitPrice));
        UnitPrice = unitPrice;
    }

    public void SetDescription(string? description)
    {
        Description = new Description(description ?? string.Empty);
    }

    #endregion
}