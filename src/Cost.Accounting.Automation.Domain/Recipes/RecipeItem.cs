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

    // Kalem miktarı, reçetenin tanımlı olduğu üretim miktarına (Recipe.OutputQuantity)
    // göre girilir. Beklenen malzeme = kalem miktarı / üretim miktarı * fiilen üretilen.
    // OutputQuantity = 1 olduğunda davranış "bir birim mamul için miktar" ile birebirdir.
    public decimal EffectiveQuantity(int producedQuantity, decimal outputQuantity)
    {
        if (producedQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(producedQuantity), "Üretilen miktar pozitif olmalıdır.");
        if (outputQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(outputQuantity), "Reçete üretim miktarı pozitif olmalıdır.");

        return Quantity * producedQuantity / outputQuantity;
    }

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