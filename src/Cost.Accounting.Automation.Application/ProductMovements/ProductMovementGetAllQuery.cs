using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
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
    IProductMovementRepository productMovementRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<ProductMovementGetAllQuery, IQueryable<ProductMovementListDto>>
{
    public async Task<IQueryable<ProductMovementListDto>> Handle(
        ProductMovementGetAllQuery request,
        CancellationToken cancellationToken)
    {
        // Stok hareketi en hızlı büyüyen tablodur: her fatura satırı, her pusula
        // kalemi ve her stok belgesi satırı buraya yazar. Bu yüzden filtreler
        // veritabanında uygulanmalıdır.
        IQueryable<ProductMovement> source = request.OnlyDeleted
            ? productMovementRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : productMovementRepository.GetAllActive();

        if (request.ProductId is Guid productId)
        {
            IdentityId id = new(productId);
            source = source.Where(i => i.ProductId == id);
        }

        if (request.MovementType is ProductMovementType movementType)
        {
            source = source.Where(i => i.MovementType == movementType);
        }

        if (request.WarehouseId is Guid warehouseId)
        {
            IdentityId id = new(warehouseId);
            source = source.Where(i => i.Product != null && i.Product.WarehouseId == id);
        }

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
