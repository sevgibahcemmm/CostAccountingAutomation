using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Suppliers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;

/// <summary>Seçili tedarikçileri tek transaction'da siler.</summary>
[Permission("supplier:delete")]
public sealed record BulkDeleteSuppliersCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteSuppliersCommandHandler(
    ISupplierRepository supplierRepository)
    : IRequestHandler<BulkDeleteSuppliersCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteSuppliersCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Supplier>(
            supplierRepository,
            (ids, token) => supplierRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "tedarikçi",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "tedarikçi");
    }
}