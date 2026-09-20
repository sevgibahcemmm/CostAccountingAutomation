using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostSlipReport;

public sealed record WorkshopStockLineDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid ProductUnitTypeId,
    string ProductUnitTypeName,
    Guid SourceMovementId,
    decimal UnitPrice,
    decimal Quantity);

public sealed class CostSlipDto : EntityDto
{
    [Column("Pusula No", Order = 1, Width = 120)]
    public string SlipNumber { get; set; } = default!;

    [Column("Pusula Tipi", Order = 2, Width = 120)]
    public string CostSlipTypeDisplay => GetDisplayName(CostSlipType);

    [Column("Açıklama", Order = 3, Width = 150)]
    public string Description { get; set; } = default!;

    [Column("Miktarı", Order = 4, Width = 80)]
    public int Quantity { get; set; }

    [Column("Tarih", Order = 5, Width = 80)]
    public DateTime CostDate { get; set; }

    [Column("Atölye", Order = 6, Width = 160)]
    public string WorkshopName { get; set; } = default!;

    [Column("Cari Hesap", Order = 7, Width = 180)]
    public string CurrentAccountName { get; set; } = default!;

    [Column("Genel Toplam", Order = 8, Width = 120)]
    public decimal GrandTotal { get; set; }

    [Browsable(false)]
    public CostSlipType CostSlipType { get; set; }

    [Browsable(false)]
    public Guid WorkshopId { get; set; }

    [Browsable(false)]
    public Guid? CustomerId { get; set; }

    [Browsable(false)]
    public Guid? ProducedProductId { get; set; }

    [Browsable(false)]
    public List<CostSlipItemDto> CostSlipItems { get; set; } = [];

    public static string GetDisplayName<T>(T enumValue) where T : struct, Enum
    {
        var field = typeof(T).GetField(enumValue.ToString());
        if (field != null)
        {
            var attr = field.GetCustomAttributes(typeof(DisplayAttribute), false)
                            .Cast<DisplayAttribute>()
                            .FirstOrDefault();
            if (attr != null && !string.IsNullOrEmpty(attr.Name))
                return attr.Name;
        }
        return enumValue.ToString();
    }
}

public sealed class CostSlipItemDto : EntityDto
{
    [Column("Ürün / Masraf", Order = 1, Width = 220)]
    public string ProductName { get; set; } = default!;

    [Column("Birim", Order = 2, Width = 80)]
    public string ProductUnitTypeName { get; set; } = default!;

    [Column("Hesap Kodu", Order = 3, Width = 260)]
    public string ExpenseAccountTypeDisplay => CostSlipDto.GetDisplayName(ExpenseAccountType);

    [Column("Miktar", Order = 4, Width = 80)]
    public decimal Quantity { get; set; }

    [Column("Birim Fiyat", Order = 5, Width = 100)]
    public decimal UnitPrice { get; set; }

    [Column("Tutar", Order = 6, Width = 110)]
    public decimal TotalAmount { get; set; }

    [Browsable(false)]
    public Guid? ProductId { get; set; }

    [Browsable(false)]
    public Guid? ProductUnitTypeId { get; set; }

    [Browsable(false)]
    public ExpenseAccountType ExpenseAccountType { get; set; }

    [Browsable(false)]
    public string? Description { get; set; }
}

public sealed class CostSlipItemEditDto : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));

    private Guid _id;
    public Guid Id { get => _id; set { _id = value; OnPropertyChanged(); } }

    private Guid? _productId;
    public Guid? ProductId { get => _productId; set { _productId = value; OnPropertyChanged(); } }

    private string _productName = string.Empty;
    public string ProductName { get => _productName; set { _productName = value; OnPropertyChanged(); } }

    private Guid? _productUnitTypeId;
    public Guid? ProductUnitTypeId { get => _productUnitTypeId; set { _productUnitTypeId = value; OnPropertyChanged(); } }

    private string _productUnitTypeName = string.Empty;
    public string ProductUnitTypeName { get => _productUnitTypeName; set { _productUnitTypeName = value; OnPropertyChanged(); } }

    private ExpenseAccountType _expenseAccountType = ExpenseAccountType.Account710;
    public ExpenseAccountType ExpenseAccountType { get => _expenseAccountType; set { _expenseAccountType = value; OnPropertyChanged(); } }

    private decimal _quantity;
    public decimal Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); } }

    private decimal _unitPrice;
    public decimal UnitPrice { get => _unitPrice; set { _unitPrice = value; OnPropertyChanged(); } }

    private decimal _totalAmount;
    public decimal TotalAmount { get => _totalAmount; set { _totalAmount = value; OnPropertyChanged(); } }

    private decimal _transferredQuantity;
    public decimal TransferredQuantity { get => _transferredQuantity; set { _transferredQuantity = value; OnPropertyChanged(); } }

    private decimal _availableQuantity;
    public decimal AvailableQuantity { get => _availableQuantity; set { _availableQuantity = value; OnPropertyChanged(); } }

    private string _description = string.Empty;
    public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
}

public sealed record CostSlipItemRequest(
    Guid? ProductId,
    Guid? ProductUnitTypeId,
    ExpenseAccountType ExpenseAccountType,
    decimal Quantity,
    decimal UnitPrice,
    string? Description);

public sealed record CostSlipCreateRequest(
    string SlipNumber,
    CostSlipType CostSlipType,
    Guid WorkshopId,
    Guid? CustomerId,
    Guid? ProducedProductId,
    int Quantity,
    DateTime CostDate,
    string? Description,
    List<CostSlipItemRequest> Items);

public sealed record CostSlipUpdateRequest(
    Guid Id,
    string SlipNumber,
    CostSlipType CostSlipType,
    Guid WorkshopId,
    Guid? CustomerId,
    Guid? ProducedProductId,
    int Quantity,
    DateTime CostDate,
    string? Description,
    List<CostSlipItemRequest> Items);