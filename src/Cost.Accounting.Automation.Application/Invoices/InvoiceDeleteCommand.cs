using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Invoices;

[Permission("invoice:delete")]
public sealed record InvoiceDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class InvoiceDeleteCommandHandler(
    IInvoiceRepository invoiceRepository,
    IProductMovementRepository productMovementRepository,
    ICurrentAccountMovementRepository currentAccountMovementRepository) : IRequestHandler<InvoiceDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InvoiceDeleteCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);
        Invoice? invoice = await invoiceRepository.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invoice is null)
        {
            return Result<string>.Failure("Fatura bulunamadı.");
        }

        invoiceRepository.SoftDelete(invoice);

        // Faturaya bağlı stok hareketlerini de soft-delete yap
        var relatedStockMovements = await productMovementRepository
            .Where(m => m.InvoiceId == id)
            .ToListAsync(cancellationToken);

        foreach (var movement in relatedStockMovements)
        {
            productMovementRepository.SoftDelete(movement);
        }

        // Faturaya bağlı cari hareketleri de soft-delete yap
        var relatedCurrentMovements = await currentAccountMovementRepository
            .Where(m => m.InvoiceId == id)
            .ToListAsync(cancellationToken);

        foreach (var movement in relatedCurrentMovements)
        {
            currentAccountMovementRepository.SoftDelete(movement);
        }

        return Result<string>.Succeed("Fatura ve ilişkili hareketler başarıyla silindi.");
    }
}
