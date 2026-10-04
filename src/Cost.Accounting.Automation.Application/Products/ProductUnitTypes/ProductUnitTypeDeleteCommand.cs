using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

[Permission("product:delete")]
public sealed record ProductUnitTypeDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

/// <summary>
/// Birim cinsi ürün/maliyet pusulası kayıtlarında kullanılıyorsa silme sonrası not üretir;
/// engelleyici hareket yoktur.
/// </summary>
internal sealed class ProductUnitTypeDeleteCommandHandler(
    IProductUnitTypeRepository unitTypeRepository)
    : IRequestHandler<ProductUnitTypeDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        ProductUnitTypeDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<ProductUnitType>(
            unitTypeRepository,
            (ids, token) => unitTypeRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "birim cinsi",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "birim cinsi");
    }
}