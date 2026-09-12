using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

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
    IProductUnitTypeRepository unitTypeRepository) : IRequestHandler<ProductUnitTypeCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductUnitTypeCreateCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await unitTypeRepository.AnyAsync(
            p => p.Name.Value == request.Name,
            cancellationToken);

        if (nameExists)
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