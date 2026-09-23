using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Recipes;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Recipes;

[Permission("recipe:manage")]
public sealed record RecipeDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class RecipeDeleteCommandHandler(
    IRecipeRepository recipeRepository) : IRequestHandler<RecipeDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RecipeDeleteCommand request, CancellationToken cancellationToken)
    {
        await recipeRepository.SoftDeleteAsync(new IdentityId(request.Id), cancellationToken);
        return Result<string>.Succeed("Reçete silindi.");
    }
}