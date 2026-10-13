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
            .Include(x => x.Workshop)
            .Include(x => x.Product)
                .ThenInclude(p => p!.ProductUnitType)
            .AsSplitQuery()
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.ProductUnitType);

    public Task<Recipe?> GetByProductIdAsync(IdentityId productId, CancellationToken cancellationToken = default)
        => this.Context.Set<Recipe>()
            .Include(x => x.Workshop)
            .Include(x => x.Product)
                .ThenInclude(p => p!.ProductUnitType)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.ProductUnitType)
            .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

    public Task<Recipe?> GetWithItemsAsync(IdentityId id, CancellationToken cancellationToken = default)
        => this.Context.Set<Recipe>()
            .Include(x => x.Workshop)
            .Include(x => x.Product)
                .ThenInclude(p => p!.ProductUnitType)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.ProductUnitType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}