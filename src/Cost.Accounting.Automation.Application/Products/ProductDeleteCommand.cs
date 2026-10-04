using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:delete")]
public sealed record ProductDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

/// <summary>
/// Tekil silme de toplu silmeyle aynı koruma ve hesap bağlantısı yolunu
/// kullanır.
/// </summary>
internal sealed class ProductDeleteCommandHandler(
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ProductDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductDeleteCommand request, CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Product>(
            productRepository,
            (ids, token) => productRepository.GetDeletionCheckAsync(ids, token),
            (ids, token) => DeleteLinkedAccountsAsync(ids, token));

        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "ürün",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "ürün");
    }

    /// <summary>Ürüne bağlı hesap planı düğümünü de aynı transaction içinde siler.</summary>
    private async Task DeleteLinkedAccountsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        HashSet<IdentityId> keys = ids.Select(id => new IdentityId(id)).ToHashSet();

        List<Guid> linkedAccountIds = await productRepository.GetAll()
            .Where(p => keys.Contains(p.Id) && p.ChartOfAccountId != null)
            .Select(p => p.ChartOfAccountId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (linkedAccountIds.Count == 0)
        {
            return;
        }

        HashSet<Guid> accountIds = linkedAccountIds.ToHashSet();
        List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);
        List<ChartOfAccount> toDelete = accounts
            .Where(a => !a.IsDeleted && accountIds.Contains(a.Id.Value))
            .ToList();

        if (toDelete.Count > 0)
        {
            chartOfAccountRepository.SoftDeleteRange(toDelete);
        }
    }
}