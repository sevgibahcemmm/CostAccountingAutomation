using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Customers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;

/// <summary>
/// Seçili müşterileri tek transaction'da siler.
///
/// <para>
/// Müşteri başına ayrı komut göndermek yerine tek çağrıda toplu silinir:
/// hareket denetimi tek sorguda yapılır ve ya hep silinir ya hiçbiri
/// silinmez.
/// </para>
/// </summary>
[Permission("customer:delete")]
public sealed record BulkDeleteCustomersCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteCustomersCommandHandler(
    ICustomerRepository customerRepository)
    : IRequestHandler<BulkDeleteCustomersCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteCustomersCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Customer>(
            customerRepository,
            (ids, token) => customerRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "müşteri",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "müşteri");
    }
}