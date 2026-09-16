using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Photos;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ProductUnitTypes;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
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
    Guid TaxRateId,
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

        RuleFor(x => x.TaxRateId)
            .NotEmpty().WithMessage("KDV oranı seçmelisiniz");

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
    IProductUnitTypeRepository unitTypeRepository,
    ITaxRateRepository taxRateRepository,
    ICompanyRepository companyRepository,
    IBarcodeGeneratorService barcodeGeneratorService) : IRequestHandler<ProductCreateCommand, Result<string>>
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

        ProductUnitType? unitType = await unitTypeRepository.FirstOrDefaultAsync(
            u => u.Id == new IdentityId(request.ProductUnitTypeId),
            cancellationToken);
        if (unitType is null)
        {
            return Result<string>.Failure("Geçerli bir birim cinsi seçmelisiniz");
        }

        TaxRate? taxRate = await taxRateRepository.FirstOrDefaultAsync(
            t => t.Id == new IdentityId(request.TaxRateId),
            cancellationToken);
        if (taxRate is null)
        {
            return Result<string>.Failure("Geçerli bir KDV oranı seçmelisiniz");
        }

        List<string> productCodes = (await productRepository.GetAllIncludingDeletedAsync(cancellationToken))
            .Select(p => p.ProductCode.Value)
            .ToList();
        List<string> accountCodes = accounts.Select(a => a.Code.Value).ToList();

        (string productCode, string nodeCode) = ProductCodeHelper.BuildNextCodes(category.Code.Value, productCodes, accountCodes);

        string companyPrefix = await ProductBarcodePrefixResolver.ResolveAsync(companyRepository, cancellationToken);
        string gtin = barcodeGeneratorService.GenerateGtin(companyPrefix, productCode);
        string qrContent = ProductQrContentBuilder.Build(
            productCode,
            gtin,
            request.Name,
            taxRate.Rate,
            warehouse.Name.Value,
            category.Name.Value,
            unitType.Name.Value);

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
            new Barcode(gtin),
            new QRCode(qrContent),
            request.MinimumProductLevel,
            new IdentityId(request.TaxRateId),
            warehouse.Id,
            category.Id,
            new IdentityId(request.ProductUnitTypeId),
            node.Id,
            new Description(request.Description),
            request.IsActive);

        product.ReplacePrices(request.Prices.Select(p => new ProductPrice(
            new Price(p.UnitPrice), p.PriceType, p.StartDate, p.EndDate)));

        product.ReplaceImages(request.ImagePaths
            .Select((path, index) => new Photo(
                PhotoOwnerType.Product,
                product.Id,
                System.IO.Path.GetFileName(path),
                GetContentType(path),
                path,
                index == 0)));

        await productRepository.AddAsync(product, cancellationToken);

        return "Ürün başarıyla kaydedildi";
    }

    private static string GetContentType(string path)
    {
        string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}