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
    public static IQueryable<CurrentAccountMovementDto> MapTo(this IQueryable<EntityWithAuditDto<CurrentAccountMovement>> entity)
    {
        return entity
            .Select(s => new CurrentAccountMovementDto
            {
                Id = s.Entity.Id,
                CurrentAccountType = s.Entity.CurrentAccountType,
                CustomerId = s.Entity.CustomerId == null ? null : s.Entity.CustomerId.Value,
                CustomerName = s.Entity.Customer == null ? null : s.Entity.Customer.Name.Value,
                SupplierId = s.Entity.SupplierId == null ? null : s.Entity.SupplierId.Value,
                SupplierName = s.Entity.Supplier == null ? null : s.Entity.Supplier.Name.Value,

                Date = s.Entity.Date,
                MovementType = s.Entity.MovementType,
                DocumentNo = s.Entity.DocumentNo,
                Debit = s.Entity.Debit,
                Credit = s.Entity.Credit,
                Description = s.Entity.Description.Value,
                InvoiceId = s.Entity.InvoiceId == null ? null : s.Entity.InvoiceId.Value,

                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy,
                IsActive = s.Entity.IsActive,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy == null ? null : s.Entity.UpdatedBy.Value,
                CreatedFullName = s.CreatedUser.FullName.Value,
                UpdatedFullName = s.UpdatedUser == null ? null : s.UpdatedUser.FullName.Value
            })
            .AsQueryable();
    }
}
