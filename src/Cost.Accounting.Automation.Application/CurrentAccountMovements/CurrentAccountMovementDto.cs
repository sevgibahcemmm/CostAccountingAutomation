using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;

namespace Cost.Accounting.Automation.Application.CurrentAccountMovements;

public sealed class CurrentAccountMovementDto : EntityDto
{
    [Column("Cari Türü", IsVisible = false)]
    public CurrentAccountType CurrentAccountType { get; set; }

    [Column("Cari Türü", Order = 15, Width = 80, Alignment = "Center")]
    public string CurrentAccountTypeName => CurrentAccountType == CurrentAccountType.Customer ? "Müşteri" : "Tedarikçi";

    [Column("Cari Adı", Order = 20, Width = 180)]
    public string CurrentAccountName => CurrentAccountType == CurrentAccountType.Customer
        ? (CustomerName ?? "-")
        : (SupplierName ?? "-");

    [Column("Müşteri Id", IsVisible = false)]
    public Guid? CustomerId { get; set; }

    [Column("Müşteri Adı", IsVisible = false)]
    public string? CustomerName { get; set; }

    [Column("Tedarikçi Id", IsVisible = false)]
    public Guid? SupplierId { get; set; }

    [Column("Tedarikçi Adı", IsVisible = false)]
    public string? SupplierName { get; set; }

    [Column("Tarih", Order = 10, Width = 90, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly Date { get; set; }

    [Column("İşlem Türü", IsVisible = false)]
    public CurrentAccountMovementType MovementType { get; set; }

    [Column("İşlem Türü", Order = 30, Width = 100, Alignment = "Center")]
    public string MovementTypeName => MovementType switch
    {
        CurrentAccountMovementType.SalesInvoice => "Satış Faturası",
        CurrentAccountMovementType.PurchaseInvoice => "Satın Alma Faturası",
        CurrentAccountMovementType.Collection => "Tahsilat",
        CurrentAccountMovementType.Payment => "Ödeme",
        CurrentAccountMovementType.OpeningBalance => "Devir / Açılış",
        CurrentAccountMovementType.DebitVoucher => "Borç Dekontu",
        CurrentAccountMovementType.CreditVoucher => "Alacak Dekontu",
        CurrentAccountMovementType.SalesReturnInvoice => "Satış İade Faturası",
        CurrentAccountMovementType.PurchaseReturnInvoice => "Alış İade Faturası",
        CurrentAccountMovementType.ServiceInvoice => "Hizmet Faturası",
        _ => MovementType.ToString()
    };

    [Column("Belge No", Order = 40, Width = 110)]
    public string? DocumentNo { get; set; }

    [Column("Borç", Order = 50, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal Debit { get; set; }

    [Column("Alacak", Order = 60, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal Credit { get; set; }

    [Column("Bakiye", Order = 70, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal Balance => Debit - Credit;

    [Column("Açıklama", Order = 80, Width = 180)]
    public string Description { get; set; } = default!;

    [Column("Fatura Id", IsVisible = false)]
    public Guid? InvoiceId { get; set; }
}

public static class CurrentAccountMovementExtensions
{
    public static IQueryable<CurrentAccountMovementDto> MapTo(this IQueryable<CurrentAccountMovement> entity)
    {
        return entity
            .Select(s => new CurrentAccountMovementDto
            {
                Id = s.Id,
                CurrentAccountType = s.CurrentAccountType,
                CustomerId = s.CustomerId == null ? null : s.CustomerId.Value,
                CustomerName = s.Customer == null ? null : s.Customer.Name.Value,
                SupplierId = s.SupplierId == null ? null : s.SupplierId.Value,
                SupplierName = s.Supplier == null ? null : s.Supplier.Name.Value,

                Date = s.Date,
                MovementType = s.MovementType,
                DocumentNo = s.DocumentNo,
                Debit = s.Debit,
                Credit = s.Credit,
                Description = s.Description.Value,
                InvoiceId = s.InvoiceId == null ? null : s.InvoiceId.Value,

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value,
            });
    }
}
