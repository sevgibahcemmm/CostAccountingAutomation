using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Recipes;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Recipes;

public sealed record RecipeItemRow(
    Guid ProductId,
    decimal Quantity);

[Permission("recipe:manage")]
public sealed record RecipeSaveCommand(
    Guid ProductId,
    decimal OutputQuantity,
    bool IsActive,
    IReadOnlyCollection<RecipeItemRow> Items,
    Guid WorkshopId) : IRequest<Result<string>>;

public sealed class RecipeSaveCommandValidator : AbstractValidator<RecipeSaveCommand>
{
    public RecipeSaveCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Mamül ürün seçilmelidir.");

        RuleFor(x => x.OutputQuantity)
            .GreaterThan(0).WithMessage("Üretim miktarı sıfırdan büyük olmalıdır.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Reçetede en az bir malzeme satırı bulunmalıdır.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Malzeme ürünü seçilmelidir.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Malzeme miktarı sıfırdan büyük olmalıdır.");
        });
    }
}

internal sealed class RecipeSaveCommandHandler(
    IRecipeRepository recipeRepository,
    IProductRepository productRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<RecipeSaveCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RecipeSaveCommand request, CancellationToken cancellationToken)
    {
        IdentityId productId = new(request.ProductId);

        Product? producedProduct = await productRepository.GetAll()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (producedProduct is null || producedProduct.IsDeleted)
        {
            return Result<string>.Failure("Seçilen mamül ürün bulunamadı.");
        }

        List<Guid> duplicateMaterials = request.Items
            .GroupBy(i => i.ProductId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateMaterials.Count > 0)
        {
            return Result<string>.Failure("Aynı malzeme ürünü reçetede birden fazla kez bulunamaz.");
        }

        foreach (RecipeItemRow row in request.Items)
        {
            Product? material = await productRepository.GetAll()
                .FirstOrDefaultAsync(p => p.Id == new IdentityId(row.ProductId), cancellationToken);

            if (material is null || material.IsDeleted)
            {
                return Result<string>.Failure("Reçete malzemesi olarak seçilen ürün bulunamadı.");
            }
        }

        Recipe? recipe = await recipeRepository.GetByProductIdAsync(productId, cancellationToken);

        IdentityId? workshopId = request.WorkshopId == Guid.Empty
            ? null
            : new IdentityId(request.WorkshopId);

        if (recipe is not null)
        {
            recipe.SetOutputQuantity(request.OutputQuantity);
            recipe.SetWorkshop(workshopId);
            recipe.ClearItems();
            foreach (RecipeItemRow row in request.Items)
            {
                recipe.AddItem(new RecipeItem(new IdentityId(row.ProductId), row.Quantity));
            }
            recipe.SetStatus(request.IsActive);
            recipeRepository.Update(recipe);
            return Result<string>.Succeed("Reçete güncellendi.");
        }

        Recipe? existing = await duplicateCheckService.FindDuplicateAsync<Recipe>(
            Recipe.BuildDuplicateKey(productId),
            includeDeleted: true,
            cancellationToken: cancellationToken);

        Recipe target;
        if (existing is not null)
        {
            target = existing;
            target.Restore();
            target.SetStatus(request.IsActive);
            target.SetOutputQuantity(request.OutputQuantity);
            target.SetWorkshop(workshopId);
        }
        else
        {
            target = new Recipe(productId);
            target.SetStatus(request.IsActive);
            target.SetOutputQuantity(request.OutputQuantity);
            target.SetWorkshop(workshopId);
        }

        target.ClearItems();
        foreach (RecipeItemRow row in request.Items)
        {
            target.AddItem(new RecipeItem(new IdentityId(row.ProductId), row.Quantity));
        }

        if (existing is not null)
        {
            recipeRepository.Update(target);
        }
        else
        {
            await recipeRepository.AddAsync(target, cancellationToken);
        }

        return Result<string>.Succeed(existing is not null
            ? "Silinen reçete geri getirilerek güncellendi."
            : "Reçete başarıyla kaydedildi.");
    }
}