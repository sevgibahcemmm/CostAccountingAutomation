using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Recipes;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class RecipeRepository : AuditableRepository<Recipe, ApplicationDbContext>, IRecipeRepository
{
    public RecipeRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }

    /// <summary>
    /// Reçete listesi ürün ve reçete satırlarını gösterir; denetimli liste
    /// sorgusu bellekte materyalize edildiği için burada yüklenmelidir.
    /// </summary>
    protected override IQueryable<Recipe> ApplyDetailIncludes(IQueryable<Recipe> query)
        => query
            .Include(x => x.Product)
            .AsSplitQuery()
            .Include(x => x.Items)
                .ThenInclude(i => i.Product);

    public Task<Recipe?> GetByProductIdAsync(IdentityId productId, CancellationToken cancellationToken = default)
        => this.Context.Set<Recipe>()
            .Include(x => x.Product)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

    public Task<Recipe?> GetWithItemsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => this.Context.Set<Recipe>()
            .Include(x => x.Product)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}