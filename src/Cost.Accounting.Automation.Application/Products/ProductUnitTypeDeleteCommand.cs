using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:delete")]
public sealed record ProductUnitTypeDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class ProductUnitTypeDeleteCommandHandler(
    IProductUnitTypeRepository unitTypeRepository) : IRequestHandler<ProductUnitTypeDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductUnitTypeDeleteCommand request, CancellationToken cancellationToken)
    {
        var unitType = await unitTypeRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (unitType is null)
        {
            return Result<string>.Failure("Birim cinsi bulunamadı");
        }

        unitType.Delete();
        unitTypeRepository.Update(unitType);

        return "Birim cinsi başarıyla silindi";
    }
}