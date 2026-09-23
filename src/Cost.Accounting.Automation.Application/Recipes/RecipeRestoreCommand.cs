using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Recipes;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Recipes;

[Permission("recipe:manage")]
public sealed record RecipeRestoreCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class RecipeRestoreCommandHandler(
    IRecipeRepository recipeRepository) : IRequestHandler<RecipeRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RecipeRestoreCommand request, CancellationToken cancellationToken)
    {
        await recipeRepository.RestoreAsync(new IdentityId(request.Id), cancellationToken);
        return Result<string>.Succeed("Reçete geri getirildi.");
    }
}