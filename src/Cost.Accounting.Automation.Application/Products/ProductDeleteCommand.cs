using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:delete")]
public sealed record ProductDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class ProductDeleteCommandHandler(
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ProductDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductDeleteCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (product is null)
        {
            return Result<string>.Failure("Ürün bulunamadı");
        }

        if (product.ChartOfAccountId is { } nodeId)
        {
            List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);
            ChartOfAccount? node = accounts.FirstOrDefault(a => a.Id == nodeId && !a.IsDeleted);
            if (node is not null)
            {
                node.Delete();
                chartOfAccountRepository.Update(node);
            }
        }

        product.Delete();
        productRepository.Update(product);

        return "Ürün başarıyla silindi";
    }
}