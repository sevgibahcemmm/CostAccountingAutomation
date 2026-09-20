using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Suppliers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;

[Permission("supplier:delete")]
public sealed record SupplierDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class SupplierDeleteCommandHandler(
    ISupplierRepository supplierRepository,
    IInvoiceRepository invoiceRepository,
    ICurrentAccountMovementRepository currentAccountMovementRepository) : IRequestHandler<SupplierDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SupplierDeleteCommand request, CancellationToken cancellationToken)
    {
        var supplier = await supplierRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (supplier is null)
        {
            return Result<string>.Failure("Tedarikçi bulunamadı");
        }

        bool hasMovement = await currentAccountMovementRepository.AnyAsync(
            m => m.SupplierId == new IdentityId(request.Id), cancellationToken);
        if (hasMovement)
        {
            return Result<string>.Failure(
                $"'{supplier.Name.Value}' tedarikçisi cari hareket gördüğü için silinemez.");
        }

        bool hasInvoice = await invoiceRepository.AnyAsync(
            i => i.SupplierId == new IdentityId(request.Id), cancellationToken);

        supplier.Delete();
        supplierRepository.Update(supplier);

        if (hasInvoice)
        {
            return DeleteWarnings.Compose(
                $"'{supplier.Name.Value}' tedarikçisi silindi. NOT: irsaliye kayıtlarında kullanılıyor; " +
                $"hareket görmediği için silme gerçekleştirildi.");
        }

        return "Tedarikçi başarıyla silindi";
    }
}