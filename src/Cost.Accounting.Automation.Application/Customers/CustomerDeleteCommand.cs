using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Customers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;

[Permission("customer:delete")]
public sealed record CustomerDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

/// <summary>
/// Tekil silme de toplu silmeyle aynı koruma yolunu kullanır; böylece
/// "cari hareket gördüğü için silinemez" kuralı tek ve tek yerde tanımlıdır.
/// </summary>
internal sealed class CustomerDeleteCommandHandler(
    ICustomerRepository customerRepository) : IRequestHandler<CustomerDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerDeleteCommand request, CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Customer>(
            customerRepository,
            (ids, token) => customerRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "müşteri",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "müşteri");
    }
}