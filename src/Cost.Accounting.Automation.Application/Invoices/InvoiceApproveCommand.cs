using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Invoices;

[Permission("invoice:approve")]
public sealed record InvoiceApproveCommand(
    Guid Id,
    StockCostingMethod CostingMethod = StockCostingMethod.Fifo) : IRequest<Result<string>>;

internal sealed class InvoiceApproveCommandHandler(
    IInvoiceRepository invoiceRepository,
    IProductMovementRepository productMovementRepository,
    ICurrentAccountMovementRepository currentAccountMovementRepository) : IRequestHandler<InvoiceApproveCommand, Result<string>>
{
    public async Task<Result<string>> Handle(InvoiceApproveCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        Invoice? invoice = await invoiceRepository
            .WhereWithTracking(i => i.Id == id)
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            return Result<string>.Failure("Fatura bulunamadı.");
        }

        if (invoice.Status == InvoiceStatus.Approved)
        {
            return Result<string>.Failure("Bu fatura zaten onaylanmış durumda.");
        }

        bool hasLedger = await productMovementRepository.AnyAsync(m => m.InvoiceId == id, cancellationToken)
            || await currentAccountMovementRepository.AnyAsync(m => m.InvoiceId == id, cancellationToken);

        if (hasLedger)
        {
            return Result<string>.Failure("Bu fatura için stok/cari hareketleri zaten kayıtlı.");
        }

        invoice.Approve();

        await InvoiceLedgerHelper.CreateLedgerMovementsAsync(
            invoice,
            productMovementRepository,
            currentAccountMovementRepository,
            request.CostingMethod,
            cancellationToken);

        return Result<string>.Succeed("Fatura onaylandı; stok hareketleri ve cari hareketi oluşturuldu.");
    }
}