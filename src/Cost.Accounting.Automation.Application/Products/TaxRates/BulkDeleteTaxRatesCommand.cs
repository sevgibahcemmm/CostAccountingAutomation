using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.TaxRates;

/// <summary>Seçili KDV oranlarını tek transaction'da siler.</summary>
[Permission("product:delete")]
public sealed record BulkDeleteTaxRatesCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteTaxRatesCommandHandler(
    ITaxRateRepository taxRateRepository)
    : IRequestHandler<BulkDeleteTaxRatesCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteTaxRatesCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<TaxRate>(
            taxRateRepository,
            (ids, token) => taxRateRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "KDV oranı",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "KDV oranı");
    }
}