using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ProductMovements;

[Permission("stock_movement:delete")]
public sealed record ProductMovementDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class ProductMovementDeleteCommandHandler(
    IProductMovementRepository productMovementRepository) : IRequestHandler<ProductMovementDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductMovementDeleteCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);
        ProductMovement? movement = await productMovementRepository.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (movement is null)
        {
            return Result<string>.Failure("Stok hareketi bulunamadı.");
        }

        productMovementRepository.SoftDelete(movement);
        return Result<string>.Succeed("Stok hareketi başarıyla silindi.");
    }
}
