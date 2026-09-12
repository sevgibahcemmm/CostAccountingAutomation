using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:delete")]
public sealed record ProductRestoreCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class ProductRestoreCommandHandler(
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ProductRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductRestoreCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdIncludingDeletedAsync(new IdentityId(request.Id), cancellationToken);
        if (product is null)
        {
            return Result<string>.Failure("Ürün bulunamadı");
        }

        if (!product.IsDeleted)
        {
            return Result<string>.Failure("Ürün zaten silinmiş durumda değil");
        }

        product.Restore();
        productRepository.Update(product);

        if (product.ChartOfAccountId is { } nodeId)
        {
            List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);
            ChartOfAccount? node = accounts.FirstOrDefault(a => a.Id == nodeId && a.IsDeleted);
            if (node is not null)
            {
                node.Restore();
                chartOfAccountRepository.Update(node);
            }
        }

        return "Ürün başarıyla geri yüklendi";
    }
}