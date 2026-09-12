using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:view")]
public sealed record ProductGetNextProductCodeQuery(
    Guid CategoryId) : IRequest<Result<string>>;

internal sealed class ProductGetNextProductCodeQueryHandler(
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ProductGetNextProductCodeQuery, Result<string>>
{
    public async Task<Result<string>> Handle(ProductGetNextProductCodeQuery request, CancellationToken cancellationToken)
    {
        List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);

        ChartOfAccount? category = ProductCodeHelper.ResolveCategory(accounts, request.CategoryId);
        if (category is null)
        {
            return Result<string>.Failure("Seçilen kategori hesap planında bulunamadı");
        }

        List<string> productCodes = (await productRepository.GetAllIncludingDeletedAsync(cancellationToken))
            .Select(p => p.ProductCode.Value)
            .ToList();
        List<string> accountCodes = accounts.Select(a => a.Code.Value).ToList();

        (string productCode, _) = ProductCodeHelper.BuildNextCodes(category.Code.Value, productCodes, accountCodes);

        return productCode;
    }
}