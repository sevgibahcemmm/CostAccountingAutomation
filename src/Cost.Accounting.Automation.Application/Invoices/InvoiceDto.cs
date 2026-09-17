using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Invoices;

namespace Cost.Accounting.Automation.Application.Invoices;

public sealed class InvoiceLineDto
{
    [Column("Id", IsVisible = false)]
    public Guid Id { get; set; }

    [Column("Ürün", Order = 10, Width = 220)]
    public Guid ProductId { get; set; }

    [Column("Ürün Adı", IsVisible = false)]
    public string ProductName { get; set; } = default!;

    [Column("Ürün Kodu", IsVisible = false)]
    public string ProductCode { get; set; } = default!;

    [Column("Miktar", Order = 20, Width = 75, Alignment = "Right")]
    public decimal Quantity { get; set; }

    [Column("Birim Fiyat", Order = 30, Width = 95, Alignment = "Right")]
    public decimal UnitPrice { get; set; }

    [Column("İskonto %", Order = 40, Width = 80, Alignment = "Right")]
    public decimal DiscountRate { get; set; }

    [Column("İskonto Tutarı", Order = 50, Width = 95, Alignment = "Right")]
    public decimal DiscountAmount => Math.Round(
        Quantity * UnitPrice * (DiscountRate / 100m), 2);

    [Column("KDV %", Order = 60, Width = 65, Alignment = "Right")]
    public decimal TaxRateRate { get; set; }

    [Column("KDV Tutarı", Order = 70, Width = 90, Alignment = "Right")]
    public decimal TaxAmount { get; set; }

    [Column("Toplam Tutar", Order = 80, Width = 105, Alignment = "Right")]
    public decimal TotalAmount { get; set; }

    [Column("Satır Açıklaması", Order = 90, Width = 140)]
    public string Description { get; set; } = default!;
}

public sealed class InvoiceDto : EntityDto
{
    [Column("Fatura No", Order = 10, Width = 120)]
    public string InvoiceNumber { get; set; } = default!;

    [Column("Fatura Tipi", IsVisible = false)]
    public InvoiceType InvoiceType { get; set; }

    [Column("Fatura Tipi", Order = 15, Width = 110, Alignment = "Center")]
    public string InvoiceTypeName => InvoiceType == InvoiceType.Sales ? "Satış Faturası" : "Satın Alma Faturası";

    [Column("Durum", IsVisible = false)]
    public InvoiceStatus Status { get; set; }

    [Column("Durum", Order = 55, Width = 80, Alignment = "Center")]
    public string StatusName => Status == InvoiceStatus.Approved ? "Onaylı" : "Taslak";

    [Column("Tarih", Order = 20, Width = 90, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly Date { get; set; }

    [Column("Cari Adı", Order = 30, Width = 180)]
    public string CurrentAccountName => InvoiceType == InvoiceType.Sales
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

    [Column("Açıklama", IsVisible = false)]
    public string Description { get; set; } = default!;

    [Column("Ara Toplam", Order = 40, Width = 100, Format = "n2", Alignment = "Right")]
    public decimal SubTotal { get; set; }

    [Column("İskonto Tutarı", Order = 45, Width = 100, Format = "n2", Alignment = "Right")]
    public decimal DiscountTotal { get; set; }

    [Column("KDV Tutarı", Order = 50, Width = 100, Format = "n2", Alignment = "Right")]
    public decimal TaxTotal { get; set; }

    [Column("Genel Toplam", Order = 60, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal GrandTotal { get; set; }

    public List<InvoiceLineDto> Lines { get; set; } = [];
}

public static class InvoiceExtensions
{
    public static IQueryable<InvoiceDto> MapTo(this IQueryable<EntityWithAuditDto<Invoice>> entity)
    {
        return entity
            .Select(s => new InvoiceDto
            {
                Id = s.Entity.Id,
                InvoiceNumber = s.Entity.InvoiceNumber,
                InvoiceType = s.Entity.InvoiceType,
                Status = s.Entity.Status,
                Date = s.Entity.Date,

                CustomerId = s.Entity.CustomerId == null ? null : s.Entity.CustomerId.Value,
                CustomerName = s.Entity.Customer == null ? null : s.Entity.Customer.Name.Value,

                SupplierId = s.Entity.SupplierId == null ? null : s.Entity.SupplierId.Value,
                SupplierName = s.Entity.Supplier == null ? null : s.Entity.Supplier.Name.Value,

                Description = s.Entity.Description.Value,
                SubTotal = s.Entity.SubTotal,
                DiscountTotal = s.Entity.DiscountTotal,
                TaxTotal = s.Entity.TaxTotal,
                GrandTotal = s.Entity.GrandTotal,

                Lines = s.Entity.Lines.Select(l => new InvoiceLineDto
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    ProductName = l.Product == null ? string.Empty : l.Product.Name.Value,
                    ProductCode = l.Product == null ? string.Empty : l.Product.ProductCode.Value,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountRate = l.DiscountRate,
                    TaxRateRate = l.TaxRateRate,
                    TaxAmount = l.TaxAmount,
                    TotalAmount = l.TotalAmount,
                    Description = l.Description.Value
                }).ToList(),

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
