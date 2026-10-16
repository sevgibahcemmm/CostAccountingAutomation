using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Suppliers;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Suppliers;
[Permission("supplier:view")]
public sealed record SupplierGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<SupplierDto>>
{
    public SupplierGetAllQuery() : this(false) { }
}

internal sealed class SupplierGetAllQueryHandler(
    ISupplierRepository supplierRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<SupplierGetAllQuery, IQueryable<SupplierDto>>
{
    public async Task<IQueryable<SupplierDto>> Handle(SupplierGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Supplier> source = request.OnlyDeleted
            ? supplierRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : supplierRepository.GetAllActive();

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
