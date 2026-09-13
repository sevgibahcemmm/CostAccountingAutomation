using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Invoices;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Invoices;

[Permission("invoice:view")]
public sealed record InvoiceGetAllQuery(
    InvoiceType? InvoiceType = null,
    bool OnlyDeleted = false) : IRequest<IQueryable<InvoiceDto>>
{
    public InvoiceGetAllQuery() : this(null, false) { }
}

internal sealed class InvoiceGetAllQueryHandler(
    IInvoiceRepository invoiceRepository) : IRequestHandler<InvoiceGetAllQuery, IQueryable<InvoiceDto>>
{
    public Task<IQueryable<InvoiceDto>> Handle(InvoiceGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<Invoice>> source = request.OnlyDeleted
            ? invoiceRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : invoiceRepository.GetAllWithAudit();

        if (request.InvoiceType.HasValue)
        {
            source = source.Where(i => i.Entity.InvoiceType == request.InvoiceType.Value);
        }

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}
