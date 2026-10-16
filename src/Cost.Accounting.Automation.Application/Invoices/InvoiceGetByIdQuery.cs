using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Invoices;
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
        // Tek fatura: liste sorgusu üzerinden filtrelemek yerine doğrudan
        // kimlikle okunur. Önceden liste belleğe alınıp bellekte aranıyordu.
        Invoice? invoice = await invoiceRepository.GetByIdWithLinesAsync(
            new IdentityId(request.Id), cancellationToken);

        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure("Fatura bulunamadı.");
        }

        return Result<InvoiceDto>.Succeed(invoice.ToDto());
    }
}
