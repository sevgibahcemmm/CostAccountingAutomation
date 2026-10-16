using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
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
    IProductUnitTypeRepository unitTypeRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<ProductUnitTypeGetAllQuery, IQueryable<ProductUnitTypeDto>>
{
    public async Task<IQueryable<ProductUnitTypeDto>> Handle(ProductUnitTypeGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<ProductUnitType> source = request.OnlyDeleted
            ? unitTypeRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : unitTypeRepository.GetAllActive();

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
