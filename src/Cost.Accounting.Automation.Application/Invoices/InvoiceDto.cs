using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Invoices;

namespace Cost.Accounting.Automation.Application.Invoices;

public static class InvoiceTypeInfo
{
    public static bool IsPurchaseSide(this InvoiceType type)
        => type == InvoiceType.Purchase || type == InvoiceType.PurchaseReturn;

    public static bool IsSalesSide(this InvoiceType type)
        => type == InvoiceType.Sales || type == InvoiceType.SalesReturn ;

    public static bool RequiresCustomer(this InvoiceType type)
        => type.IsSalesSide();

    public static bool RequiresSupplier(this InvoiceType type)
        => type.IsPurchaseSide();


    public static bool IsReturn(this InvoiceType type)
        => type == InvoiceType.PurchaseReturn || type == InvoiceType.SalesReturn;
}

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

public sealed class InvoiceDto : EntityDto, IApprovalStatusDto
{
    [Column("Fatura No", Order = 10, Width = 120)]
    public string InvoiceNumber { get; set; } = default!;

    [Column("Fatura Tipi", IsVisible = false)]
    public InvoiceType InvoiceType { get; set; }

    [Column("Fatura Tipi", Order = 15, Width = 130, Alignment = "Center")]
    public string InvoiceTypeName => EnumDisplay.GetDisplayName(InvoiceType);

    [Column("Durum", IsVisible = false)]
    public InvoiceStatus Status { get; set; }

    /// <summary>
    /// Ortak durum sözleşmesi: liste ekranı taslak/onaylı ayrımını renk olarak
    /// yansıtır. <c>[Column]</c> taşımadığı için gridde ayrı kolon olusmaz.
    /// </summary>
    public bool IsApproved => Status == InvoiceStatus.Approved;

    [Column("Durum", Order = 55, Width = 80, Alignment = "Center")]
    public string StatusName => Status == InvoiceStatus.Approved ? "Onaylı" : "Taslak";

    [Column("Tarih", Order = 20, Width = 90, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly Date { get; set; }

    [Column("Cari Adı", Order = 30, Width = 180)]
    public string CurrentAccountName => InvoiceType.IsSalesSide()
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
    /// <summary>
    /// Fatura LİSTESİ için projeksiyon (veritabanında çalışır).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satırlar bilinçli olarak bu projeksiyonda <b>yer almaz</b>. Alt ızgara
    /// satırları gösterir, ama satırları ana sorguya katmak her fatura için
    /// satırları JOIN edip tek satıra indirger; bu kartesian çarpım sorguyu
    /// üstel olarak şişirir (aynı sorun maliyet pusulasında 25 saniyelik bir
    /// sorguya yol açmıştı).
    /// </para>
    /// <para>
    /// Satırlar <c>InvoiceGetAllQuery</c> içinde ikinci bir sorguda, yalnızca
    /// gelen faturalar için alınır.
    /// </para>
    /// </remarks>
    public static IQueryable<InvoiceDto> MapTo(this IQueryable<Invoice> entity)
    {
        return entity
            .Select(s => new InvoiceDto
            {
                Id = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                InvoiceType = s.InvoiceType,
                Status = s.Status,
                Date = s.Date,

                CustomerId = s.CustomerId == null ? null : s.CustomerId.Value,
                CustomerName = s.Customer == null ? null : s.Customer.Name.Value,

                SupplierId = s.SupplierId == null ? null : s.SupplierId.Value,
                SupplierName = s.Supplier == null ? null : s.Supplier.Name.Value,

                Description = s.Description.Value,
                SubTotal = s.SubTotal,
                DiscountTotal = s.DiscountTotal,
                TaxTotal = s.TaxTotal,
                GrandTotal = s.GrandTotal,

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value,
            });
    }

    /// <summary>
    /// Tek bir fatura kaydını liste DTO'suna çevirir; satırlar dahildir.
    /// Yalnızca tek faturanın açıldığı ekranlar kullanır.
    /// </summary>
    public static InvoiceDto ToDto(this Invoice invoice)
    {
        InvoiceDto dto = new()
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            Status = invoice.Status,
            Date = invoice.Date,

            CustomerId = invoice.CustomerId?.Value,
            CustomerName = invoice.Customer?.Name.Value,

            SupplierId = invoice.SupplierId?.Value,
            SupplierName = invoice.Supplier?.Name.Value,

            Description = invoice.Description?.Value ?? string.Empty,
            SubTotal = invoice.SubTotal,
            DiscountTotal = invoice.DiscountTotal,
            TaxTotal = invoice.TaxTotal,
            GrandTotal = invoice.GrandTotal,

            IsActive = invoice.IsActive,
            CreatedAt = invoice.CreatedAt,
            CreatedBy = invoice.CreatedBy,
            UpdatedAt = invoice.UpdatedAt,
            UpdatedBy = invoice.UpdatedBy?.Value,
            CreatedFullName = string.Empty
        };

        dto.Lines = [.. invoice.Lines.Select(l => new InvoiceLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            ProductName = l.Product?.Name.Value ?? string.Empty,
            ProductCode = l.Product?.ProductCode.Value ?? string.Empty,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            DiscountRate = l.DiscountRate,
            TaxRateRate = l.TaxRateRate,
            TaxAmount = l.TaxAmount,
            TotalAmount = l.TotalAmount,
            Description = l.Description.Value
        })];

        return dto;
    }
}
