using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
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
        var runner = new BulkDeletionRunner<Recipe>(recipeRepository);
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "reçete",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "reçete");
    }
}