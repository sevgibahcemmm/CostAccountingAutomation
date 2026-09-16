using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

[Permission("product:view")]
public sealed record ProductUnitTypeGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<ProductUnitTypeDto>>
{
    public ProductUnitTypeGetAllQuery() : this(false) { }
}

internal sealed class ProductUnitTypeGetAllQueryHandler(
    IProductUnitTypeRepository unitTypeRepository) : IRequestHandler<ProductUnitTypeGetAllQuery, IQueryable<ProductUnitTypeDto>>
{
    public Task<IQueryable<ProductUnitTypeDto>> Handle(ProductUnitTypeGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<ProductUnitType>> source = request.OnlyDeleted
            ? unitTypeRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : unitTypeRepository.GetAllWithAudit();

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}