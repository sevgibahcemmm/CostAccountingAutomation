using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Invoices;

[Permission("invoice:view")]
public sealed record InvoiceGetAllQuery(
    InvoiceType? InvoiceType = null,
    bool OnlyDeleted = false,
    InvoiceStatus? Status = null) : IRequest<IQueryable<InvoiceDto>>
{
    public InvoiceGetAllQuery() : this(null, false, null) { }
}

internal sealed class InvoiceGetAllQueryHandler(
    IInvoiceRepository invoiceRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<InvoiceGetAllQuery, IQueryable<InvoiceDto>>
{
    public async Task<IQueryable<InvoiceDto>> Handle(InvoiceGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Invoice> source = request.OnlyDeleted
            ? invoiceRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : invoiceRepository.GetAllActive();

        if (request.InvoiceType is InvoiceType invoiceType)
        {
            source = source.Where(i => i.InvoiceType == invoiceType);
        }

        if (request.Status is InvoiceStatus status)
        {
            source = source.Where(i => i.Status == status);
        }

        List<InvoiceDto> invoices = await source
            .MapTo()
            .ToListAsync(cancellationToken);

        await auditUserNames.FillAsync(invoices, cancellationToken);

        // Satırlar ayrı sorguda gelir: ana liste projeksiyonuna katılırsa her
        // fatura için satırları JOIN edip tek satıra indirgemek kartesian
        // çarpım üretir ve liste sorgusunu üstel olarak şişirir.
        if (invoices.Count > 0)
        {
            Dictionary<Guid, List<InvoiceLine>> linesByInvoice =
                (await invoiceRepository.GetLinesByInvoiceIdsAsync(
                        [.. invoices.Select(i => i.Id)], cancellationToken))
                    .GroupBy(l => l.InvoiceId.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());

            foreach (InvoiceDto invoice in invoices)
            {
                invoice.Lines = linesByInvoice.TryGetValue(invoice.Id, out List<InvoiceLine>? lines)
                    ? [.. lines.Select(l => new InvoiceLineDto
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
                    })]
                    : [];
            }
        }

        return invoices.AsQueryable();
    }
}
