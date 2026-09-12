using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Suppliers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;

[Permission("supplier:delete")]
public sealed record SupplierDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class SupplierDeleteCommandHandler(
    ISupplierRepository supplierRepository) : IRequestHandler<SupplierDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SupplierDeleteCommand request, CancellationToken cancellationToken)
    {
        var supplier = await supplierRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (supplier is null)
        {
            return Result<string>.Failure("Tedarikçi bulunamadı");
        }

        supplier.Delete();
        supplierRepository.Update(supplier);

        return "Tedarikçi başarıyla silindi";
    }
}