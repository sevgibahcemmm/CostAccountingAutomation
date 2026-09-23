using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Recipes;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Recipes;

[Permission("recipe:view")]
public sealed record RecipeGetAllQuery(bool OnlyDeleted = false) : IRequest<IQueryable<RecipeListDto>>
{
    public RecipeGetAllQuery() : this(false) { }
}

internal sealed class RecipeGetAllQueryHandler(
    IRecipeRepository recipeRepository) : IRequestHandler<RecipeGetAllQuery, IQueryable<RecipeListDto>>
{
    public Task<IQueryable<RecipeListDto>> Handle(RecipeGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<Recipe>> source = request.OnlyDeleted
            ? recipeRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : recipeRepository.GetAllWithAudit();

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}