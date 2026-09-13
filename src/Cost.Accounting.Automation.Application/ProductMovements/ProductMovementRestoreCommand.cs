using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ProductMovements;

[Permission("stock_movement:restore")]
public sealed record ProductMovementRestoreCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class ProductMovementRestoreCommandHandler(
    IProductMovementRepository productMovementRepository) : IRequestHandler<ProductMovementRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductMovementRestoreCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);
        ProductMovement? movement = await productMovementRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);

        if (movement is null)
        {
            return Result<string>.Failure("Stok hareketi bulunamadı.");
        }

        productMovementRepository.Restore(movement);
        return Result<string>.Succeed("Stok hareketi başarıyla geri yüklendi.");
    }
}
