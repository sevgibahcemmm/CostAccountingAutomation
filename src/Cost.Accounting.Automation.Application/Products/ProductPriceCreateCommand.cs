using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:update")]
public sealed record ProductPriceCreateCommand(
    Guid ProductId,
    ProductPriceType PriceType,
    decimal UnitPrice,
    DateOnly StartDate) : IRequest<Result<string>>;

public sealed class ProductPriceCreateCommandValidator : AbstractValidator<ProductPriceCreateCommand>
{
    public ProductPriceCreateCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün seçilmelidir.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Birim fiyat sıfırdan büyük olmalıdır.");
    }
}

internal sealed class ProductPriceCreateCommandHandler(IProductRepository productRepository)
    : IRequestHandler<ProductPriceCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductPriceCreateCommand request, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdWithDetailsAsync(new IdentityId(request.ProductId), cancellationToken);
        if (product is null)
        {
            return Result<string>.Failure("Ürün bulunamadı.");
        }

        bool duplicate = product.Prices.Any(p =>
            p.PriceType == request.PriceType
            && p.StartDate == request.StartDate);

        if (duplicate)
        {
            return Result<string>.Failure($"Bu tarih için {request.PriceType} türünde bir fiyat zaten mevcut.");
        }

        product.AddPrice(new Price(request.UnitPrice), request.PriceType, request.StartDate);
        productRepository.Update(product);

        return Result<string>.Succeed("Fiyat başarıyla kaydedildi.");
    }
}