using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

[Permission("product:delete")]
public sealed record ProductUnitTypeRestoreCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class ProductUnitTypeRestoreCommandHandler(
    IProductUnitTypeRepository unitTypeRepository) : IRequestHandler<ProductUnitTypeRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductUnitTypeRestoreCommand request, CancellationToken cancellationToken)
    {
        var unitType = await unitTypeRepository.GetByIdIncludingDeletedAsync(new IdentityId(request.Id), cancellationToken);
        if (unitType is null)
        {
            return Result<string>.Failure("Birim cinsi bulunamadı");
        }

        if (!unitType.IsDeleted)
        {
            return Result<string>.Failure("Birim cinsi zaten silinmiş durumda değil");
        }

        unitType.Restore();
        unitTypeRepository.Update(unitType);

        return "Birim cinsi başarıyla geri yüklendi";
    }
}