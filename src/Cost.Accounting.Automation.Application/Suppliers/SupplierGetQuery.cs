using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;
[Permission("supplier:view")]
public sealed record SupplierGetQuery(
    Guid Id) : IRequest<Result<SupplierDto>>;

internal sealed class SupplierGetQueryHandler(
    ISupplierRepository supplierRepository) : IRequestHandler<SupplierGetQuery, Result<SupplierDto>>
{
    public async Task<Result<SupplierDto>> Handle(SupplierGetQuery request, CancellationToken cancellationToken)
    {
        var supplier = await supplierRepository
            .GetAllWithAudit()
            .MapTo()
            .Where(i => i.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier is null)
        {
            return Result<SupplierDto>.Failure("Tedarikçi bulunamadı");
        }

        return supplier;
    }
}