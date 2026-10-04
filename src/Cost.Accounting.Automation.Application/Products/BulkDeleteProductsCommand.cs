using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Seçili ürünleri tek transaction'da siler ve bağlı hesap planı düğümlerini
/// de yumuşak siler.
/// </summary>
[Permission("product:delete")]
public sealed record BulkDeleteProductsCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteProductsCommandHandler(
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository)
    : IRequestHandler<BulkDeleteProductsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteProductsCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Product>(
            productRepository,
            (ids, token) => productRepository.GetDeletionCheckAsync(ids, token),
            (ids, token) => DeleteLinkedAccountsAsync(ids, token));

        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "ürün",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "ürün");
    }

    /// <summary>
    /// Ürünlere bağlı hesap planı düğümlerini toplu olarak yumuşak siler.
    /// Ürünün kendisiyle aynı DbContext örneğinde çalıştığı için bu işlem de
    /// ürün silmesiyle aynı transaction'a dâhil olur.
    /// </summary>
    private async Task DeleteLinkedAccountsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        List<Guid> productIds = ids.Distinct().ToList();
        if (productIds.Count == 0)
        {
            return;
        }

        HashSet<IdentityId> keys = productIds.Select(id => new IdentityId(id)).ToHashSet();

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