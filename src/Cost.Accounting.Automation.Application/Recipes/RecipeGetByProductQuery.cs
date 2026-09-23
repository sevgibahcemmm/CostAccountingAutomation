using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Recipes;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Recipes;

[Permission("recipe:view")]
public sealed record RecipeGetByProductQuery(Guid ProductId) : IRequest<RecipeDto?>;

internal sealed class RecipeGetByProductQueryHandler(
    IRecipeRepository recipeRepository) : IRequestHandler<RecipeGetByProductQuery, RecipeDto?>
{
    public async Task<RecipeDto?> Handle(RecipeGetByProductQuery request, CancellationToken cancellationToken)
    {
        Recipe? recipe = await recipeRepository.GetByProductIdAsync(new IdentityId(request.ProductId), cancellationToken);
        return recipe?.ToDto();
    }
}