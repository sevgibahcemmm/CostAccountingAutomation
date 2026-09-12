using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Products;

public sealed class Product : Entity, IHardDeletable
{
    private readonly List<ProductPrice> _prices = [];
    private readonly List<ProductMovement> _movements = [];
    private readonly List<ProductImage> _images = [];

    private Product()
    {
    }

    public Product(
        Name name,
        ProductCode productCode,
        Barcode barcode,
        QRCode qrCode,
        decimal? minimumProductLevel,
        decimal taxRate,
        IdentityId warehouseId,
        IdentityId categoryId,
        IdentityId productUnitTypeId,
        IdentityId? chartOfAccountId,
        Description description,
        bool isActive)
    {
        SetName(name);
        SetProductCode(productCode);
        SetBarcode(barcode);
        SetQRCode(qrCode);
        SetMinimumProductLevel(minimumProductLevel);
        SetTaxRate(taxRate);
        SetWarehouse(warehouseId);
        SetCategory(categoryId);
        SetProductUnitType(productUnitTypeId);
        SetChartOfAccountId(chartOfAccountId);
        SetDescription(description);
        SetStatus(isActive);
    }

    public Name Name { get; private set; } = default!;
    public ProductCode ProductCode { get; private set; } = default!;
    public Barcode Barcode { get; private set; } = default!;
    public QRCode QRCode { get; private set; } = default!;
    public decimal? MinimumProductLevel { get; private set; }
    public decimal TaxRate { get; private set; }

    public IdentityId WarehouseId { get; private set; } = default!;
    public IdentityId CategoryId { get; private set; } = default!;
    public IdentityId ProductUnitTypeId { get; private set; } = default!;
    public IdentityId? ChartOfAccountId { get; private set; }

    public Description Description { get; private set; } = default!;

    public ChartOfAccount? Warehouse { get; private set; }
    public ChartOfAccount? Category { get; private set; }
    public ProductUnitType? ProductUnitType { get; private set; }
    public IReadOnlyCollection<ProductPrice> Prices => _prices;
    public IReadOnlyCollection<ProductMovement> Movements => _movements;
    public IReadOnlyCollection<ProductImage> Images => _images;

    public void SetName(Name name) => Name = name;

    public void SetProductCode(ProductCode productCode) => ProductCode = productCode;

    public void SetBarcode(Barcode barcode) => Barcode = barcode;

    public void SetQRCode(QRCode qrCode) => QRCode = qrCode;

    public void SetMinimumProductLevel(decimal? minimumProductLevel) => MinimumProductLevel = minimumProductLevel;

    public void SetTaxRate(decimal taxRate) => TaxRate = taxRate;

    public void SetWarehouse(IdentityId warehouseId) => WarehouseId = warehouseId;

    public void SetCategory(IdentityId categoryId) => CategoryId = categoryId;

    public void SetProductUnitType(IdentityId productUnitTypeId) => ProductUnitTypeId = productUnitTypeId;

    public void SetChartOfAccountId(IdentityId? chartOfAccountId) => ChartOfAccountId = chartOfAccountId;

    public void SetDescription(Description description) => Description = description;

    public void ChangeCategory(IdentityId categoryId) => SetCategory(categoryId);

    public void ReplacePrices(IEnumerable<ProductPrice> prices)
    {
        _prices.Clear();
        _prices.AddRange(prices);
    }

    public void ReplaceImages(IEnumerable<ProductImage> images)
    {
        _images.Clear();
        _images.AddRange(images);
    }

    public void AddMovement(ProductMovement movement) => _movements.Add(movement);
}