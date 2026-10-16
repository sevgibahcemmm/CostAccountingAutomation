using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;

[Permission("supplier:view")]
public sealed record SupplierGetQuery(
    Guid Id) : IRequest<Result<SupplierDto>>;

internal sealed class SupplierGetQueryHandler(
    ISupplierRepository supplierRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<SupplierGetQuery, Result<SupplierDto>>
{
    public async Task<Result<SupplierDto>> Handle(SupplierGetQuery request, CancellationToken cancellationToken)
    {
        SupplierDto? supplier = await supplierRepository
            .GetAllActive()
            .Where(i => i.Id == new IdentityId(request.Id))
            .MapTo()
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier is null)
        {
            return Result<SupplierDto>.Failure("Tedarikçi bulunamadı");
        }

        await auditUserNames.FillAsync([supplier], cancellationToken);

        return supplier;
    }
}
