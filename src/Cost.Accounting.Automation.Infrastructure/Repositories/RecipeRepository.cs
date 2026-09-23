using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Recipes;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class RecipeRepository : AuditableRepository<Recipe, ApplicationDbContext>, IRecipeRepository
{
    public RecipeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<Recipe?> GetByProductIdAsync(IdentityId productId, CancellationToken cancellationToken = default)
        => Context.Set<Recipe>()
            .Include(x => x.Product)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

    public Task<Recipe?> GetWithItemsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => Context.Set<Recipe>()
            .Include(x => x.Product)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}