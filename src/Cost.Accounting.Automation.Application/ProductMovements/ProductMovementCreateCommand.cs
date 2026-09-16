using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
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
    IProductRepository productRepository,
    IChartOfAccountLedgerPoster ledgerPoster) : IRequestHandler<ProductMovementCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductMovementCreateCommand request, CancellationToken cancellationToken)
    {
        IdentityId productId = new(request.ProductId);
        Product? product = await productRepository.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
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

        if (product.ChartOfAccountId is { } accountId && request.UnitPrice.HasValue && request.UnitPrice.Value > 0)
        {
            decimal amount = Math.Round(request.Quantity * request.UnitPrice.Value, 2);

            if (request.MovementType == ProductMovementType.Input)
            {
                await ledgerPoster.PostAsync(accountId, amount, 0, "StokGirisi", movement.Id, cancellationToken);
            }
            else
            {
                await ledgerPoster.PostAsync(accountId, 0, amount, "StokCikisi", movement.Id, cancellationToken);
            }
        }

        string actionName = request.MovementType == ProductMovementType.Input ? "Stok girişi" : "Stok çıkışı";
        return Result<string>.Succeed($"{actionName} başarıyla kaydedildi.");
    }
}
