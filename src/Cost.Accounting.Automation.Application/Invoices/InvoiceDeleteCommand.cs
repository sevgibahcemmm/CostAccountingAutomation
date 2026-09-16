using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
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
    ICurrentAccountMovementRepository currentAccountMovementRepository,
    IChartOfAccountLedgerRepository ledgerRepository) : IRequestHandler<InvoiceDeleteCommand, Result<string>>
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

        // Faturaya bağlı hesap planı yevmiye kayıtlarını da soft-delete yap
        await SoftDeleteLedgerEntriesAsync(request.Id, relatedStockMovements, cancellationToken);

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

    private async Task SoftDeleteLedgerEntriesAsync(Guid invoiceId, List<ProductMovement> movements, CancellationToken cancellationToken)
    {
        List<ChartOfAccountLedger> ledgerEntries = [];

        foreach (ProductMovement movement in movements)
        {
            ledgerEntries.AddRange(await ledgerRepository.GetBySourceAsync("SatisFaturasi", movement.Id.Value, cancellationToken));
            ledgerEntries.AddRange(await ledgerRepository.GetBySourceAsync("SatinalmaFaturasi", movement.Id.Value, cancellationToken));
        }

        if (ledgerEntries.Count > 0)
        {
            ledgerRepository.SoftDeleteRange(ledgerEntries);
        }
    }
}