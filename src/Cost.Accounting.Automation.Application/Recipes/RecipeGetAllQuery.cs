using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Recipes;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Recipes;

[Permission("recipe:view")]
public sealed record RecipeGetAllQuery(bool OnlyDeleted = false) : IRequest<IQueryable<RecipeListDto>>
{
    public RecipeGetAllQuery() : this(false) { }
}

internal sealed class RecipeGetAllQueryHandler(
    IRecipeRepository recipeRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<RecipeGetAllQuery, IQueryable<RecipeListDto>>
{
    public async Task<IQueryable<RecipeListDto>> Handle(RecipeGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Recipe> source = request.OnlyDeleted
            ? recipeRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : recipeRepository.GetAllActive();

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
