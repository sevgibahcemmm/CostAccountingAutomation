using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Suppliers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Suppliers;

[Permission("supplier:delete")]
public sealed record SupplierDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

/// <summary>
/// Tekil silme de toplu silmeyle aynı koruma yolunu kullanır; böylece
/// "cari hareket gördüğü için silinemez" kuralı tek ve tek yerde tanımlıdır.
/// </summary>
internal sealed class SupplierDeleteCommandHandler(
    ISupplierRepository supplierRepository) : IRequestHandler<SupplierDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SupplierDeleteCommand request, CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Supplier>(
            supplierRepository,
            (ids, token) => supplierRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "tedarikçi",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "tedarikçi");
    }
}