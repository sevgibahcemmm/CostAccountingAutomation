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
    bool CreatePair,
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
    IBarcodeGeneratorService barcodeGeneratorService,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<ProductCreateCommand, Result<string>>
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

        string? duplicateKey = Product.BuildDuplicateKey(request.Name, warehouse.Id, category.Id);

        Product? nameDuplicate = await duplicateCheckService.FindDuplicateAsync<Product>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Bu depo ve kategori altında aynı adla bir ürün zaten mevcut");
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

        bool isSemi = warehouse.Code.Value == "151" || warehouse.Code.Value == "152" || warehouse.Code.Value.StartsWith("151.") || warehouse.Code.Value.StartsWith("152.");

        if (request.CreatePair && isSemi)
        {
            string pairWarehouseCode = warehouse.Code.Value == "151" || warehouse.Code.Value.StartsWith("151.")
                ? ("152" + warehouse.Code.Value.Substring(3))
                : ("151" + warehouse.Code.Value.Substring(3));

            ChartOfAccount? pairWarehouse = accounts.FirstOrDefault(a => a.Type == ChartOfAccountType.Warehouse && !a.IsDeleted && (a.Code.Value == pairWarehouseCode || a.Code.Value.StartsWith(pairWarehouseCode + ".")));

            string pairCategoryCode = warehouse.Code.Value == "151" || warehouse.Code.Value.StartsWith("151.")
                ? ("152" + category.Code.Value.Substring(3))
                : ("151" + category.Code.Value.Substring(3));

            ChartOfAccount? pairCategory = accounts.FirstOrDefault(a =>
                a.Type == ChartOfAccountType.Category &&
                !a.IsDeleted &&
                a.Code.Value == pairCategoryCode &&
                IsDescendantOf(a, pairWarehouse, accounts));

            if (pairWarehouse is null || pairCategory is null)
            {
                return Result<string>.Failure("151/152 karşılık depo veya kategori hesap planında bulunamadı");
            }

            string baseName = request.Name.Trim();
            string mamulName = baseName + " (MAMÜL)";
            string yarimamulName = baseName + " (YARIMAMÜL)";

            // 152 = Mamüller (MAMÜL ürünü), 151 = Yarı Mamüller (YARIMAMÜL ürünü).
            // Kullanıcı hangi depodan başlarsa başlasın, tür her zaman doğru tarafa yazılır.
            bool selectedIs152 = warehouse.Code.Value.StartsWith("152");

            ChartOfAccount mamulWarehouse = selectedIs152 ? warehouse : pairWarehouse;
            ChartOfAccount mamulCategory = selectedIs152 ? category : pairCategory;
            ChartOfAccount yarimamulWarehouse = selectedIs152 ? pairWarehouse : warehouse;
            ChartOfAccount yarimamulCategory = selectedIs152 ? pairCategory : category;

            (string mamulCode, string mamulNodeCode) = ProductCodeHelper.BuildNextCodes(mamulCategory.Code.Value, productCodes, accountCodes);
            (string yarimamulCode, string yarimamulNodeCode) = ProductCodeHelper.BuildNextCodes(yarimamulCategory.Code.Value, productCodes, accountCodes);

            productCodes.Add(mamulCode); accountCodes.Add(mamulNodeCode);
            productCodes.Add(yarimamulCode); accountCodes.Add(yarimamulNodeCode);

            ChartOfAccount mamulNode = new(
                new AccountCode(mamulNodeCode),
                new Name(mamulName),
                mamulCategory.Level + 1,
                ChartOfAccountType.Stok);
            mamulNode.SetParent(mamulCategory.Id);
            await chartOfAccountRepository.AddAsync(mamulNode, cancellationToken);

            ChartOfAccount yarimamulNode = new(
                new AccountCode(yarimamulNodeCode),
                new Name(yarimamulName),
                yarimamulCategory.Level + 1,
                ChartOfAccountType.Stok);
            yarimamulNode.SetParent(yarimamulCategory.Id);
            await chartOfAccountRepository.AddAsync(yarimamulNode, cancellationToken);

            string gtinMamul = barcodeGeneratorService.GenerateGtin(companyPrefix, mamulCode);
            string qrMamul = ProductQrContentBuilder.Build(
                mamulCode, gtinMamul, mamulName, taxRate.Rate,
                mamulWarehouse.Name.Value, mamulCategory.Name.Value, unitType.Name.Value);

            string gtinYarimamul = barcodeGeneratorService.GenerateGtin(companyPrefix, yarimamulCode);
            string qrYarimamul = ProductQrContentBuilder.Build(
                yarimamulCode, gtinYarimamul, yarimamulName, taxRate.Rate,
                yarimamulWarehouse.Name.Value, yarimamulCategory.Name.Value, unitType.Name.Value);

            Product productMamul = new(
                new Name(mamulName),
                new ProductCode(mamulCode),
                new Barcode(gtinMamul),
                new QRCode(qrMamul),
                request.MinimumProductLevel,
                new IdentityId(request.TaxRateId),
                mamulWarehouse.Id,
                mamulCategory.Id,
                new IdentityId(request.ProductUnitTypeId),
                mamulNode.Id,
                new Description(request.Description),
                request.IsActive);

            productMamul.ReplacePrices(request.Prices.Select(p => new ProductPrice(
                new Price(p.UnitPrice), p.PriceType, p.StartDate, p.EndDate)));

            productMamul.ReplaceImages(request.ImagePaths
                .Select((path, index) => new Photo(
                    System.IO.Path.GetFileName(path),
                    GetContentType(path),
                    path,
                    index == 0)));

            await productRepository.AddAsync(productMamul, cancellationToken);

            Product productYarimamul = new(
                new Name(yarimamulName),
                new ProductCode(yarimamulCode),
                new Barcode(gtinYarimamul),
                new QRCode(qrYarimamul),
                request.MinimumProductLevel,
                new IdentityId(request.TaxRateId),
                yarimamulWarehouse.Id,
                yarimamulCategory.Id,
                new IdentityId(request.ProductUnitTypeId),
                yarimamulNode.Id,
                new Description(request.Description),
                request.IsActive);

            productYarimamul.ReplacePrices(request.Prices.Select(p => new ProductPrice(
                new Price(p.UnitPrice), p.PriceType, p.StartDate, p.EndDate)));

            productYarimamul.ReplaceImages(request.ImagePaths
                .Select((path, index) => new Photo(
                    System.IO.Path.GetFileName(path),
                    GetContentType(path),
                    path,
                    index == 0)));

            productYarimamul.SetSemiFinishedProduct(productMamul.Id);
            await productRepository.AddAsync(productYarimamul, cancellationToken);

            return "Mamül ve yarımamül ürünleri başarıyla kaydedildi";
        }

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
                System.IO.Path.GetFileName(path),
                GetContentType(path),
                path,
                index == 0)));

        await productRepository.AddAsync(product, cancellationToken);

        return "Ürün başarıyla kaydedildi";
    }

    private static bool IsDescendantOf(
        ChartOfAccount? account,
        ChartOfAccount? ancestor,
        List<ChartOfAccount> accounts)
    {
        if (account is null || ancestor is null)
        {
            return false;
        }

        HashSet<Guid> visited = new();
        while (account.ParentId is { } parentId)
        {
            if (parentId == ancestor.Id)
            {
                return true;
            }

            if (!visited.Add(parentId.Value))
            {
                return false;
            }

            account = accounts.FirstOrDefault(a => a.Id == parentId);
            if (account is null)
            {
                return false;
            }
        }

        return false;
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