using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Invoices;

[Permission("invoice:view")]
public sealed record InvoiceGetByIdQuery(Guid Id) : IRequest<Result<InvoiceDto>>;

internal sealed class InvoiceGetByIdQueryHandler(
    IInvoiceRepository invoiceRepository) : IRequestHandler<InvoiceGetByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(InvoiceGetByIdQuery request, CancellationToken cancellationToken)
    {
        var invoiceDto = await invoiceRepository.GetAllWithAudit()
            .Where(i => i.Entity.Id == request.Id)
            .MapTo()
            .FirstOrDefaultSafeAsync(cancellationToken);

        if (invoiceDto is null)
        {
            return Result<InvoiceDto>.Failure("Fatura bulunamadı.");
        }

        return Result<InvoiceDto>.Succeed(invoiceDto);
    }
}
