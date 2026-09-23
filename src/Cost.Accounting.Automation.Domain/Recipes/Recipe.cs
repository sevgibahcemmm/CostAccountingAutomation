using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Domain.Recipes;

// Bir mamul ürün için "tek güncel reçete" kuralı:
// DuplicateKey = mamul ürün id'si üzerinden üretilir; böylece bir ürün için
// aynı anda yalnızca bir aktif reçete bulunabilir. Reçete düzenlendikçe
// güncellenir (sürüm zinciri tutulmaz).
public sealed class Recipe : Entity
{
    private readonly List<RecipeItem> _items = [];

    private Recipe()
    {
    }

    public Recipe(IdentityId productId)
    {
        SetProduct(productId);
        ResolveDuplicateKey();
    }

    public IdentityId ProductId { get; private set; } = default!;
    public Product? Product { get; private set; }

    public IReadOnlyCollection<RecipeItem> Items => _items.AsReadOnly();

    public static string? BuildDuplicateKey(IdentityId productId)
        => DuplicateKeyRule.From(productId);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(ProductId));

    public void SetProduct(IdentityId productId)
    {
        ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
        ResolveDuplicateKey();
    }

    public void AddItem(RecipeItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.SetRecipe(Id);
        _items.Add(item);
    }

    public void RemoveItem(RecipeItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Remove(item);
    }

    public void RemoveItem(IdentityId itemId)
    {
        var item = _items.FirstOrDefault(x => x.Id == itemId);
        if (item is not null)
        {
            _items.Remove(item);
        }
    }

    public void ClearItems()
        => _items.Clear();

    public void ReplaceItems(IEnumerable<RecipeItem> items)
    {
        _items.Clear();
        foreach (RecipeItem item in items)
        {
            item.SetRecipe(Id);
            _items.Add(item);
        }
    }
}