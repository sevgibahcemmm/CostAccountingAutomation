using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Suppliers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;
[Permission("supplier:delete")]
public sealed record SupplierRestoreCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class SupplierRestoreCommandHandler(
    ISupplierRepository supplierRepository) : IRequestHandler<SupplierRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SupplierRestoreCommand request, CancellationToken cancellationToken)
    {
        var supplier = await supplierRepository.GetByIdIncludingDeletedAsync(new IdentityId(request.Id), cancellationToken);
        if (supplier is null)
        {
            return Result<string>.Failure("Tedarikçi bulunamadı");
        }

        if (!supplier.IsDeleted)
        {
            return Result<string>.Failure("Tedarikçi zaten silinmiş durumda değil");
        }

        supplier.Restore();
        supplierRepository.Update(supplier);

        return "Tedarikçi başarıyla geri yüklendi";
    }
}