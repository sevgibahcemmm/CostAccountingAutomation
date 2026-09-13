using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ProductMovements;

[Permission("stock_movement:create")]
public sealed record ProductMovementCreateCommand(
    Guid ProductId,
    ProductMovementType MovementType,
    decimal Quantity,
    decimal? UnitPrice,
    DateOnly Date,
    string? ReferenceNo,
    string Description) : IRequest<Result<string>>;

public sealed class ProductMovementCreateCommandValidator : AbstractValidator<ProductMovementCreateCommand>
{
    public ProductMovementCreateCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün seçilmelidir.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");

        When(x => x.UnitPrice.HasValue, () =>
        {
            RuleFor(x => x.UnitPrice!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Birim fiyat negatif olamaz.");
        });
    }
}

internal sealed class ProductMovementCreateCommandHandler(
    IProductMovementRepository productMovementRepository,
    IProductRepository productRepository) : IRequestHandler<ProductMovementCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductMovementCreateCommand request, CancellationToken cancellationToken)
    {
        IdentityId productId = new(request.ProductId);
        bool productExists = await productRepository.AnyAsync(p => p.Id == productId, cancellationToken);

        if (!productExists)
        {
            return Result<string>.Failure("Seçilen ürün bulunamadı.");
        }

        ProductMovement movement = new(
            productId: productId,
            movementType: request.MovementType,
            quantity: request.Quantity,
            unitPrice: request.UnitPrice.HasValue ? new Price(request.UnitPrice.Value) : null,
            date: request.Date,
            referenceNo: request.ReferenceNo?.Trim(),
            description: new Description(request.Description ?? string.Empty));

        await productMovementRepository.AddAsync(movement, cancellationToken);

        string actionName = request.MovementType == ProductMovementType.Input ? "Stok girişi" : "Stok çıkışı";
        return Result<string>.Succeed($"{actionName} başarıyla kaydedildi.");
    }
}
