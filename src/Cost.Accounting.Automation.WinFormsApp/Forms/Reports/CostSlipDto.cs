using Cost.Accounting.Automation.Domain.Abstractions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlips;

#region WinForms Enum Tanımları (Domain referansı olmadan)

public enum CostSlipType : byte
{
    [Display(Name = "Hizmet Maliyet Pusulası")]
    Service = 1,
    [Display(Name = "Mamul Maliyet Pusulası")]
    Product = 2,
    [Display(Name = "Yarı Mamul Maliyet Pusulası")]
    SemiFinishedProduct = 3
}

public sealed record WorkshopStockLineDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid ProductUnitTypeId,
    string ProductUnitTypeName,
    Guid SourceMovementId,
    decimal UnitPrice,
    decimal Quantity);

public enum ExpenseAccountType : byte
{
    [Display(Name = "710 - Direkt İlk Madde ve Malzeme Giderleri", Description = "Mamul")]
    Account710 = 1,

    [Display(Name = "720-01 - Direkt İşçilik Giderleri", Description = "Mamul")]
    Account720_1 = 2,

    [Display(Name = "720-02 - Sosyal Güvenlik Kurumlarına Devlet Primi Giderleri", Description = "Mamul")]
    Account720_2 = 3,

    [Display(Name = "730-01 - Endirek İlk Madde ve Malzeme Gideri Hesabı", Description = "Mamul")]
    Account730_01 = 4,

    [Display(Name = "730-02 - Üretimle İlgili Dışarıya Yaptırılan İşler", Description = "Mamul")]
    Account730_02 = 5,


    [Display(Name = "730-03 - İşçi Üçret ve Giderleri Hesabı", Description = "Mamul")]
    Account730_03 = 6,


    [Display(Name = "730-04 - Dışarıdan Sağlanan Fayda ve Hizmetler", Description = "Mamul")]
    Account730_04 = 7,


    [Display(Name = "730-05 - Çeşitli Giderler Hesabı", Description = "Mamul")]
    Account730_05 = 8,


    [Display(Name = "730-06 - Vergi , Resim ve Harçlar  Hesabı", Description = "Mamul")]
    Account730_06 = 9,

    [Display(Name = "730-07 - Amortisman Giderleri", Description = "Mamul")]
    Account730_07 = 10,

    /// <summary>
    /// / 740-HİZMET
    /// </summary>
    /// 
    [Display(Name = "740-01 - İlk Madde ve Malzeme", Description = "Hizmet")]
    Account740_1 = 11,

    [Display(Name = "740-02 - Üretimle İlgili Dışarıya Yaptırılan İşler", Description = "Hizmet")]
    Account740_2 = 12,

    [Display(Name = "740-03-01 - İşçi Üçret ve Giderleri Hesabı", Description = "Hizmet")]
    Account740_3_01 = 13,

    [Display(Name = "740-03-02 - Sosyal Güvenlik Kurumlarına Devlet Primi Giderleri", Description = "Hizmet")]
    Account740_3_02 = 14,

    [Display(Name = "740-04 - Dışarıdan Sağlanan Fayda ve Hizmetler", Description = "Hizmet")]
    Account740_4 = 15,

    [Display(Name = "740-05 - Çeşitli Giderler Hesabı", Description = "Hizmet")]
    Account740_5 = 16,

    [Display(Name = "740-06 - Vergi , Resim ve Harçlar  Hesabı", Description = "Hizmet")]
    Account740_6 = 17,

    [Display(Name = "740-07 - Amortisman Giderleri", Description = "Hizmet")]
    Account740_7 = 18,

    [Display(Name = "750 - Araştırma ve Geliştirme Giderleri", Description = "Ortak")]
    Account750 = 19,

    [Display(Name = "760 - Pazarlama, Satış ve Dağıtım Giderleri", Description = "Ortak")]
    Account760 = 20,

    [Display(Name = "770 - Genel Yönetim Giderleri", Description = "Ortak")]
    Account770 = 21,

    [Display(Name = "780 - Finansman Giderleri", Description = "Ortak")]
    Account780 = 22
}

#endregion

public sealed class CostSlipDto : EntityDto
{
    [Column("Pusula No", Order = 1, Width = 120)]
    public string SlipNumber { get; set; } = default!;

    [Column("Pusula Tipi", Order = 2, Width = 120)]
    public string CostSlipTypeDisplay => GetDisplayName(CostSlipType);

    [Column("Açıklama", Order = 3, Width = 150)]
    public string Description { get; set; } = default!;

    [Column("Miktarı", Order = 4, Width = 80)]
    public int Quantity { get; set; } = default!;

    [Column("Tarih", Order = 5, Width = 80)]
    public DateTime CostDate { get; set; } = default!;

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
    public Guid CurrentAccountId { get; set; }

    [Browsable(true)]
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
    public Guid ProductId { get; set; }

    [Browsable(false)]
    public Guid ProductUnitTypeId { get; set; }

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

    private Guid _productId;
    public Guid ProductId { get => _productId; set { _productId = value; OnPropertyChanged(); } }

    private string _productName = string.Empty;
    public string ProductName { get => _productName; set { _productName = value; OnPropertyChanged(); } }

    private Guid _productUnitTypeId;
    public Guid ProductUnitTypeId { get => _productUnitTypeId; set { _productUnitTypeId = value; OnPropertyChanged(); } }

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

    private string _description = string.Empty;
    public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
}

public sealed record CostSlipItemRequest(
    Guid ProductId,
    Guid ProductUnitTypeId,
    ExpenseAccountType ExpenseAccountType,
    decimal Quantity,
    decimal UnitPrice,
    string? Description);

public sealed record CostSlipCreateRequest(
    string SlipNumber,
    CostSlipType CostSlipType,
    Guid WorkshopId,
    Guid CurrentAccountId,
    int? Quantity,
     DateTime CostDate,
    string? Description,
    List<CostSlipItemRequest> Items);

public sealed record CostSlipUpdateRequest(
    Guid Id,
    string SlipNumber,
    CostSlipType CostSlipType,
    Guid WorkshopId,
    Guid CurrentAccountId,
    int? Quantity,
    DateTime CostDate,
    string? Description,
    List<CostSlipItemRequest> Items);

public static class ExpenseAccountHelper
{
    public static List<ExpenseAccountType> GetFilteredAccounts(CostSlipType costSlipType)
    {
        string targetDescription = costSlipType == CostSlipType.Product ? "Mamul" : "Hizmet";
        var filteredList = new List<ExpenseAccountType>();

        foreach (ExpenseAccountType account in Enum.GetValues<ExpenseAccountType>())
        {
            var field = typeof(ExpenseAccountType).GetField(account.ToString());
            if (field != null)
            {
                var attr = field.GetCustomAttributes(typeof(DisplayAttribute), false)
                                .Cast<DisplayAttribute>()
                                .FirstOrDefault();

                if (attr != null && (attr.Description == targetDescription || attr.Description == "Ortak"))
                {
                    filteredList.Add(account);
                }
            }
        }

        return filteredList;
    }
}