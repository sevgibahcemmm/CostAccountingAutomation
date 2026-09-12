using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
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
    ISupplierRepository supplierRepository) : IRequestHandler<SupplierGetAllQuery, IQueryable<SupplierDto>>
{
    public Task<IQueryable<SupplierDto>> Handle(SupplierGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<Supplier>> source = request.OnlyDeleted
            ? supplierRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : supplierRepository.GetAllWithAudit();

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}