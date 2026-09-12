using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

public sealed record ProductPriceRow(
    Guid? Id,
    ProductPriceType PriceType,
    decimal UnitPrice,
    DateOnly StartDate,
    DateOnly? EndDate);

public sealed record ProductImageRow(
    Guid? Id,
    string Path,
    bool IsPrimary);

[Permission("product:create")]
public sealed record ProductCreateCommand(
    string Name,
    string? Barcode,
    string? QRCode,
    decimal TaxRate,
    decimal? MinimumProductLevel,
    Guid WarehouseId,
    Guid CategoryId,
    Guid ProductUnitTypeId,
    string Description,
    bool IsActive,
    IReadOnlyCollection<ProductPriceRow> Prices,
    IReadOnlyCollection<string> ImagePaths) : IRequest<Result<string>>;

public sealed class ProductCreateCommandValidator : AbstractValidator<ProductCreateCommand>
{
    public ProductCreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Geçerli bir ürün adı girin")
            .MaximumLength(300).WithMessage("Ürün adı en fazla 300 karakter olabilir");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("KDV oranı sıfırdan küçük olamaz")
            .LessThanOrEqualTo(1).WithMessage("KDV oranı %100'den büyük olamaz");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Depo seçmelisiniz");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçmelisiniz");

        RuleFor(x => x.ProductUnitTypeId)
            .NotEmpty().WithMessage("Birim cinsi seçmelisiniz");

        RuleForEach(x => x.Prices)
            .Must(p => p.UnitPrice >= 0).WithMessage("Fiyat sıfırdan küçük olamaz");
    }
}

internal sealed class ProductCreateCommandHandler(
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IProductUnitTypeRepository unitTypeRepository) : IRequestHandler<ProductCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
    {
        List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);

        ChartOfAccount? category = ProductCodeHelper.ResolveCategory(accounts, request.CategoryId);
        if (category is null)
        {
            return Result<string>.Failure("Seçilen kategori hesap planında bulunamadı");
        }

        ChartOfAccount? warehouse = ProductCodeHelper.ResolveWarehouse(accounts, request.WarehouseId);
        if (warehouse is null)
        {
            return Result<string>.Failure("Seçilen depo hesap planında bulunamadı");
        }

        bool unitExists = await unitTypeRepository.AnyAsync(
            u => u.Id == new IdentityId(request.ProductUnitTypeId),
            cancellationToken);
        if (!unitExists)
        {
            return Result<string>.Failure("Geçerli bir birim cinsi seçmelisiniz");
        }

        List<string> productCodes = (await productRepository.GetAllIncludingDeletedAsync(cancellationToken))
            .Select(p => p.ProductCode.Value)
            .ToList();
        List<string> accountCodes = accounts.Select(a => a.Code.Value).ToList();

        (string productCode, string nodeCode) = ProductCodeHelper.BuildNextCodes(category.Code.Value, productCodes, accountCodes);

        // Ürün kartı = hesap planında kategorinin altında açılan atölye/cilt düğümü
        ChartOfAccount node = new(
            new AccountCode(nodeCode),
            new Name(request.Name),
            category.Level + 1,
            ChartOfAccountType.Stok);

        node.SetParent(category.Id);
        await chartOfAccountRepository.AddAsync(node, cancellationToken);

        Product product = new(
            new Name(request.Name),
            new ProductCode(productCode),
            new Barcode(string.IsNullOrWhiteSpace(request.Barcode) ? string.Empty : request.Barcode.Trim()),
            new QRCode(string.IsNullOrWhiteSpace(request.QRCode) ? string.Empty : request.QRCode.Trim()),
            request.MinimumProductLevel,
            request.TaxRate,
            warehouse.Id,
            category.Id,
            new IdentityId(request.ProductUnitTypeId),
            node.Id,
            new Description(request.Description),
            request.IsActive);

        product.ReplacePrices(request.Prices.Select(p => new ProductPrice(
            new Price(p.UnitPrice), p.PriceType, p.StartDate, p.EndDate)));

        product.ReplaceImages(request.ImagePaths
            .Select((path, index) => new ProductImage(path, index == 0)));

        await productRepository.AddAsync(product, cancellationToken);

        return "Ürün başarıyla kaydedildi";
    }
}