using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

[Permission("product:delete")]
public sealed record ProductUnitTypeDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class ProductUnitTypeDeleteCommandHandler(
    IProductUnitTypeRepository unitTypeRepository,
    IProductRepository productRepository,
    ICostSlipRepository costSlipRepository) : IRequestHandler<ProductUnitTypeDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductUnitTypeDeleteCommand request, CancellationToken cancellationToken)
    {
        var unitType = await unitTypeRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (unitType is null)
        {
            return Result<string>.Failure("Birim cinsi bulunamadı");
        }

        bool usedByProduct = await productRepository.AnyAsync(p => p.ProductUnitTypeId == request.Id, cancellationToken);
        bool usedBySlipItem = await costSlipRepository.AnyAsync(
            c => c.CostSlipItems.Any(i => i.ProductUnitTypeId == new IdentityId(request.Id)),
            cancellationToken);

        unitType.Delete();
        unitTypeRepository.Update(unitType);

        if (usedByProduct || usedBySlipItem)
        {
            return DeleteWarnings.Compose(
                $"'{unitType.Name.Value}' birim cinsi silindi, ancak ilişkili kayıtlarda kullanıldığı için " +
                $"ilgili ürün/maliyet pusulası kayıtlarının gözden geçirilmesi gerekir.");
        }

        return "Birim cinsi başarıyla silindi";
    }
}