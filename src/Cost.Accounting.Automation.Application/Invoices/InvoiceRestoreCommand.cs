using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Invoices;

[Permission("invoice:restore")]
public sealed record InvoiceRestoreCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class InvoiceRestoreCommandHandler(
    IInvoiceRepository invoiceRepository,
    IProductMovementRepository productMovementRepository,
    ICurrentAccountMovementRepository currentAccountMovementRepository) : IRequestHandler<InvoiceRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InvoiceRestoreCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);
        Invoice? invoice = await invoiceRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);

        if (invoice is null)
        {
            return Result<string>.Failure("Fatura bulunamadı.");
        }

        invoiceRepository.Restore(invoice);

        // Faturaya bağlı silinmiş stok hareketlerini geri yükle
        var relatedStockMovements = await productMovementRepository
            .GetAllWithAuditIncludingDeleted()
            .Where(m => m.Entity.InvoiceId == id && m.Entity.IsDeleted)
            .Select(m => m.Entity)
            .ToListAsync(cancellationToken);

        foreach (var movement in relatedStockMovements)
        {
            productMovementRepository.Restore(movement);
        }

        // Faturaya bağlı silinmiş cari hareketleri geri yükle
        var relatedCurrentMovements = await currentAccountMovementRepository
            .GetAllWithAuditIncludingDeleted()
            .Where(m => m.Entity.InvoiceId == id && m.Entity.IsDeleted)
            .Select(m => m.Entity)
            .ToListAsync(cancellationToken);

        foreach (var movement in relatedCurrentMovements)
        {
            currentAccountMovementRepository.Restore(movement);
        }

        return Result<string>.Succeed("Fatura ve ilişkili hareketler başarıyla geri yüklendi.");
    }
}
