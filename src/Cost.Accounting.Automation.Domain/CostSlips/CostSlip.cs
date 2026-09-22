using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.CostSlips;

public sealed class CostSlip : Entity, IHardDeletable
{
    private readonly List<CostSlipItem> _costSlipItems = [];

    private CostSlip()
    {
    }

    public CostSlip(
        string slipNumber,
        CostSlipType costSlipType,
        DateOnly costDate,
        IdentityId workshopId,
        IdentityId? producedProductId,
        IdentityId? customerId,
        int quantity,
        Description? description)
    {
        SetSlipNumber(slipNumber);
        SetCostSlipType(costSlipType);
        SetCostDate(costDate);
        SetWorkshop(workshopId);
        SetProducedProduct(producedProductId);
        SetCustomer(customerId);
        SetQuantity(quantity);
        SetDescription(description);
        ResolveDuplicateKey();
    }

    public static string? BuildDuplicateKey(string slipNumber, CostSlipType costSlipType)
        => DuplicateKeyRule.From(slipNumber, ((int)costSlipType).ToString());

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(SlipNumber, CostSlipType));

    public string SlipNumber { get; private set; } = default!;
    public CostSlipType CostSlipType { get; private set; }
    public CostSlipStatus Status { get; private set; } = CostSlipStatus.Draft;
    public DateOnly CostDate { get; private set; } = default!;

    public IdentityId WorkshopId { get; private set; } = default!;
    public ChartOfAccount? Workshop { get; private set; }

    public IdentityId? ProducedProductId { get; private set; }
    public Product? ProducedProduct { get; private set; }

    public IdentityId? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public int Quantity { get; private set; }
    public Description Description { get; private set; } = default!;

    public decimal GrandTotal { get; private set; }

    public IReadOnlyCollection<CostSlipItem> CostSlipItems => _costSlipItems.AsReadOnly();

    // Excel şablonundaki "Maliyet Bedeli / Giderler Yekünü" için hesap bazlı toplamlar
    public decimal GetTotalByAccount(ExpenseAccountType accountType)
        => _costSlipItems.Where(x => x.ExpenseAccountType == accountType).Sum(x => x.TotalAmount);

    // Genel Toplam (Tüm gider hesaplarının toplamı)
    public decimal GrandTotalAmount => _costSlipItems.Sum(x => x.TotalAmount);

    #region Behaviors

    public void SetSlipNumber(string slipNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slipNumber);
        SlipNumber = slipNumber;
        ResolveDuplicateKey();
    }

    public void SetCostSlipType(CostSlipType costSlipType)
    {
        CostSlipType = costSlipType;
    }

    public void SetWorkshop(IdentityId workshopId)
    {
        WorkshopId = workshopId ?? throw new ArgumentNullException(nameof(workshopId));
    }

    public void SetProducedProduct(IdentityId? producedProductId)
    {
        ProducedProductId = producedProductId;
    }

    public void SetCustomer(IdentityId? customerId)
    {
        CustomerId = customerId;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Miktar pozitif bir tam sayı olmalıdır.");

        Quantity = quantity;
    }

    public void SetDescription(Description? description)
    {
        Description = description ?? new Description(string.Empty);
    }

    public void SetCostDate(DateOnly costDate) => CostDate = costDate;

    public void Approve()
    {
        if (Status == CostSlipStatus.Draft)
        {
            Status = CostSlipStatus.Approved;
        }
    }

    public void AddItem(CostSlipItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _costSlipItems.Add(item);
        CalculateTotals();
    }

    public void RemoveItem(CostSlipItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _costSlipItems.Remove(item);
        CalculateTotals();
    }

    // InvoiceUpdateCommandHandler'daki id bazlı RemoveItem çağrısıyla tutarlı olsun diye eklendi.
    public void RemoveItem(IdentityId itemId)
    {
        var item = _costSlipItems.FirstOrDefault(x => x.Id == itemId);
        if (item is not null)
        {
            _costSlipItems.Remove(item);
        }
    }

    public void ClearItems()
    {
        _costSlipItems.Clear();
        CalculateTotals();
    }

    public void ReplaceItems(IEnumerable<CostSlipItem> items)
    {
        _costSlipItems.Clear();
        _costSlipItems.AddRange(items);
        CalculateTotals();
    }

    public void CalculateTotals()
    {
        GrandTotal = _costSlipItems.Sum(x => x.TotalAmount);
    }

    #endregion
}