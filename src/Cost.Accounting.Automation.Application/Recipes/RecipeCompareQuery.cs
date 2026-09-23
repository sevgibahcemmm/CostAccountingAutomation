using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Recipes;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Recipes;

// Maliyet pusulasındaki gerçek malzeme kullanımları reçeteyle karşılaştırılır
// ve güncel satış fiyatına göre uyarı bilgisi üretilir. Kaydı engellemez.
public sealed record RecipeCompareMaterialRow(
    Guid ProductId,
    string ProductName,
    decimal Quantity);

public sealed class RecipeCompareMismatchDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;

    // Reçeteye göre olması gereken (1 birim miktarı x üretim adedi)
    public decimal Expected { get; set; }

    // Pusulada girilen toplam kullanım
    public decimal Actual { get; set; }

    public decimal Difference => Actual - Expected;

    // "Reçeteden fazla kullanıldı" | "Reçeteden eksik kullanıldı" | "Reçetede tanımlı değil"
    public string Reason { get; set; } = default!;
}

public sealed class RecipeCompareResult
{
    public bool HasRecipe { get; set; }
    public string ProducedProductName { get; set; } = string.Empty;

    public decimal UnitCost { get; set; }
    public decimal? SalePrice { get; set; }
    public bool SalePriceExceeded { get; set; }

    public List<RecipeCompareMismatchDto> Mismatches { get; set; } = [];
}

[Permission("recipe:view")]
public sealed record RecipeCompareQuery(
    Guid ProducedProductId,
    int Quantity,
    decimal GrandTotal,
    IReadOnlyCollection<RecipeCompareMaterialRow> Materials) : IRequest<RecipeCompareResult>;

internal sealed class RecipeCompareQueryHandler(
    IRecipeRepository recipeRepository,
    IProductRepository productRepository) : IRequestHandler<RecipeCompareQuery, RecipeCompareResult>
{
    public async Task<RecipeCompareResult> Handle(RecipeCompareQuery request, CancellationToken cancellationToken)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        RecipeCompareResult result = new()
        {
            UnitCost = request.Quantity <= 0
                ? 0m
                : Math.Ceiling(request.GrandTotal / request.Quantity * 100m) / 100m
        };

        Product? producedProduct = await productRepository.GetByIdWithDetailsAsync(
            new IdentityId(request.ProducedProductId), cancellationToken);

        if (producedProduct is not null)
        {
            result.ProducedProductName = producedProduct.Name.Value;
            result.SalePrice = producedProduct.Prices
                .Where(p => p.PriceType == ProductPriceType.Sale
                    && p.StartDate <= today
                    && (p.EndDate is null || p.EndDate >= today))
                .Select(p => (decimal?)p.UnitPrice.Value)
                .FirstOrDefault();

            if (result.SalePrice.HasValue)
            {
                result.SalePriceExceeded = result.UnitCost > result.SalePrice.Value;
            }
        }

        Recipe? recipe = await recipeRepository.GetByProductIdAsync(
            new IdentityId(request.ProducedProductId), cancellationToken);

        if (recipe is null)
        {
            return result;
        }

        result.HasRecipe = true;

        Dictionary<Guid, decimal> actual = request.Materials
            .GroupBy(m => m.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Quantity));

        foreach (RecipeItem item in recipe.Items)
        {
            Guid productId = item.ProductId;
            decimal expected = item.EffectiveQuantity(request.Quantity);
            actual.TryGetValue(productId, out decimal used);

            decimal diff = used - expected;
            if (Math.Abs(diff) > 0.0001m)
            {
                result.Mismatches.Add(new RecipeCompareMismatchDto
                {
                    ProductId = productId,
                    ProductName = item.Product?.Name.Value ?? string.Empty,
                    Expected = expected,
                    Actual = used,
                    Reason = diff > 0 ? "Reçeteden fazla kullanıldı" : "Reçeteden eksik kullanıldı"
                });
            }

            actual.Remove(productId);
        }

        return result;
    }
}