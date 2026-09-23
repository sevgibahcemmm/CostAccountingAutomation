using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Recipes;

public interface IRecipeRepository : IAuditableRepository<Recipe>
{
    Task<Recipe?> GetByProductIdAsync(IdentityId productId, CancellationToken cancellationToken = default);

    Task<Recipe?> GetWithItemsAsync(IdentityId id, CancellationToken cancellationToken = default);
}