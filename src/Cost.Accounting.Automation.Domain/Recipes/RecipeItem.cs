using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Domain.Recipes;

public sealed class RecipeItem : Entity
{
    private RecipeItem()
    {
    }

    public RecipeItem(IdentityId productId, decimal quantity)
    {
        SetProduct(productId);
        SetQuantity(quantity);
    }

    public IdentityId RecipeId { get; private set; } = default!;
    public Recipe? Recipe { get; private set; }

    public IdentityId ProductId { get; private set; } = default!;
    public Product? Product { get; private set; }

    public decimal Quantity { get; private set; }

    // Bir birim mamul için kullanılacak malzeme miktarıdır.
    public decimal EffectiveQuantity(int producedQuantity)
        => Quantity * producedQuantity;

    public void SetRecipe(IdentityId recipeId) => RecipeId = recipeId;

    public void SetProduct(IdentityId productId)
    {
        ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
    }

    public void SetQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Reçete kalemi miktarı pozitif olmalıdır.");

        Quantity = quantity;
    }
}