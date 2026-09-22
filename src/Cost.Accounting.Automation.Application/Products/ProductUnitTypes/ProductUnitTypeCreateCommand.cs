using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

[Permission("product:create")]
public sealed record ProductUnitTypeCreateCommand(
    string Name,
    bool IsActive) : IRequest<Result<string>>;

public sealed class ProductUnitTypeCreateCommandValidator : AbstractValidator<ProductUnitTypeCreateCommand>
{
    public ProductUnitTypeCreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Geçerli bir birim adı girin")
            .MaximumLength(120).WithMessage("Birim adı en fazla 120 karakter olabilir");
    }
}

internal sealed class ProductUnitTypeCreateCommandHandler(
    IProductUnitTypeRepository unitTypeRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<ProductUnitTypeCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductUnitTypeCreateCommand request, CancellationToken cancellationToken)
    {
        string? duplicateKey = ProductUnitType.BuildDuplicateKey(request.Name);

        ProductUnitType? nameDuplicate = await duplicateCheckService.FindDuplicateAsync<ProductUnitType>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Bu birim cinsi daha önce kullanılmış");
        }

        ProductUnitType unitType = new(
            new Name(request.Name),
            request.IsActive);

        await unitTypeRepository.AddAsync(unitType, cancellationToken);

        return "Birim cinsi başarıyla kaydedildi";
    }
}