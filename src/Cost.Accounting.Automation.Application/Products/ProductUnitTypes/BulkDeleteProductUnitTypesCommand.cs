using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

/// <summary>Seçili birim cinslerini tek transaction'da siler.</summary>
[Permission("product:delete")]
public sealed record BulkDeleteProductUnitTypesCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteProductUnitTypesCommandHandler(
    IProductUnitTypeRepository unitTypeRepository)
    : IRequestHandler<BulkDeleteProductUnitTypesCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteProductUnitTypesCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<ProductUnitType>(
            unitTypeRepository,
            (ids, token) => unitTypeRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "birim cinsi",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "birim cinsi");
    }
}