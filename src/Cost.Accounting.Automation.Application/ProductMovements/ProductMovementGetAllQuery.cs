using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.ProductMovements;

[Permission("stock_movement:view")]
public sealed record ProductMovementGetAllQuery(
    Guid? ProductId = null,
    ProductMovementType? MovementType = null,
    Guid? WarehouseId = null,
    bool OnlyDeleted = false) : IRequest<IQueryable<ProductMovementListDto>>
{
    public ProductMovementGetAllQuery() : this(null, null, null, false) { }
}

internal sealed class ProductMovementGetAllQueryHandler(
    IProductMovementRepository productMovementRepository) : IRequestHandler<ProductMovementGetAllQuery, IQueryable<ProductMovementListDto>>
{
    public Task<IQueryable<ProductMovementListDto>> Handle(ProductMovementGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<ProductMovement>> source = request.OnlyDeleted
            ? productMovementRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : productMovementRepository.GetAllWithAudit();

        if (request.ProductId.HasValue)
        {
            IdentityId prodId = new(request.ProductId.Value);
            source = source.Where(i => i.Entity.ProductId == prodId);
        }

        if (request.MovementType.HasValue)
        {
            source = source.Where(i => i.Entity.MovementType == request.MovementType.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            IdentityId warehouseId = new(request.WarehouseId.Value);
            source = source.Where(i => i.Entity.Product != null && i.Entity.Product.WarehouseId == warehouseId);
        }

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}
