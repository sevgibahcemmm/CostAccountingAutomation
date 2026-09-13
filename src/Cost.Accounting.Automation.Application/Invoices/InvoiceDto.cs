using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Invoices;

namespace Cost.Accounting.Automation.Application.Invoices;

public sealed class InvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ProductCode { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRateRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Description { get; set; } = default!;
}

public sealed class InvoiceDto : EntityDto
{
    public string InvoiceNumber { get; set; } = default!;
    public InvoiceType InvoiceType { get; set; }
    public string InvoiceTypeName => InvoiceType == InvoiceType.Sales ? "Satış Faturası" : "Satın Alma Faturası";
    public DateOnly Date { get; set; }

    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }

    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }

    public string CurrentAccountName => InvoiceType == InvoiceType.Sales
        ? (CustomerName ?? "-")
        : (SupplierName ?? "-");

    public string Description { get; set; } = default!;
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
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
                Date = s.Entity.Date,

                CustomerId = s.Entity.CustomerId == null ? null : s.Entity.CustomerId.Value,
                CustomerName = s.Entity.Customer == null ? null : s.Entity.Customer.Name.Value,

                SupplierId = s.Entity.SupplierId == null ? null : s.Entity.SupplierId.Value,
                SupplierName = s.Entity.Supplier == null ? null : s.Entity.Supplier.Name.Value,

                Description = s.Entity.Description.Value,
                SubTotal = s.Entity.SubTotal,
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
