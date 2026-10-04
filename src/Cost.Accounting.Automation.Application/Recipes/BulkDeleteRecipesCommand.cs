using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Recipes;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Recipes;

/// <summary>Seçili reçeteleri tek transaction'da siler.</summary>
[Permission("recipe:manage")]
public sealed record BulkDeleteRecipesCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteRecipesCommandHandler(
    IRecipeRepository recipeRepository)
    : IRequestHandler<BulkDeleteRecipesCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteRecipesCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Recipe>(recipeRepository);
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "reçete",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "reçete");
    }
}