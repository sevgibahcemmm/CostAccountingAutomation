using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.TaxRates;

[Permission("product:delete")]
public sealed record TaxRateDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

/// <summary>
/// KDV oranı ürünlerde kullanılıyorsa silme sonrası not üretir; engelleyici hareket yoktur.
/// </summary>
internal sealed class TaxRateDeleteCommandHandler(
    ITaxRateRepository taxRateRepository) : IRequestHandler<TaxRateDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(TaxRateDeleteCommand request, CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<TaxRate>(
            taxRateRepository,
            (ids, token) => taxRateRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "KDV oranı",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "KDV oranı");
    }
}