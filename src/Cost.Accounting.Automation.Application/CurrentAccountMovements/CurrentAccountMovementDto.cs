using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;

namespace Cost.Accounting.Automation.Application.CurrentAccountMovements;

public sealed class CurrentAccountMovementDto : EntityDto
{
    public CurrentAccountType CurrentAccountType { get; set; }
    public string CurrentAccountTypeName => CurrentAccountType == CurrentAccountType.Customer ? "Müşteri" : "Tedarikçi";

    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }

    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }

    public string CurrentAccountName => CurrentAccountType == CurrentAccountType.Customer
        ? (CustomerName ?? "-")
        : (SupplierName ?? "-");

    public DateOnly Date { get; set; }
    public CurrentAccountMovementType MovementType { get; set; }
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

    public string? DocumentNo { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance => Debit - Credit;
    public string Description { get; set; } = default!;
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
