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

[Permission("product:update")]
public sealed record ProductUpdateCommand(
    Guid Id,
    string Name,
    Guid TaxRateId,
    decimal? MinimumProductLevel,
    Guid WarehouseId,
    Guid CategoryId,
    Guid ProductUnitTypeId,
    string Description,
    bool IsActive,
    Guid? SemiFinishedProductId,
    IReadOnlyCollection<ProductPriceRow> Prices,
    IReadOnlyCollection<ProductImageRow> Images) : IRequest<Result<string>>;

public sealed class ProductUpdateCommandValidator : AbstractValidator<ProductUpdateCommand>
{
    public ProductUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir ID girin");

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
    }
}

internal sealed class ProductUpdateCommandHandler(
    IProductRepository productRepository,
    IChartOfAccountRepository chartOfAccountRepository,
    IProductUnitTypeRepository unitTypeRepository,
    ITaxRateRepository taxRateRepository,
    ICompanyRepository companyRepository,
    IBarcodeGeneratorService barcodeGeneratorService,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<ProductUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductUpdateCommand request, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdWithDetailsAsync(new IdentityId(request.Id), cancellationToken);
        if (product is null)
        {
            return Result<string>.Failure("Ürün bulunamadı");
        }

        string companyPrefix = await ProductBarcodePrefixResolver.ResolveAsync(companyRepository, cancellationToken);

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
            excludeId: request.Id,
            cancellationToken: cancellationToken);

        if (nameDuplicate is not null)
        {
            return Result<string>.Failure("Bu depo ve kategori altında aynı adla başka bir ürün zaten mevcut");
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

        ChartOfAccount? node = product.ChartOfAccountId is { } nodeId
            ? accounts.FirstOrDefault(a => a.Id == nodeId)
            : null;

        bool categoryChanged = product.CategoryId != new IdentityId(request.CategoryId);

        if (categoryChanged)
        {
            List<string> productCodes = (await productRepository.GetAllIncludingDeletedAsync(cancellationToken))
                .Where(p => p.Id != product.Id)
                .Select(p => p.ProductCode.Value)
                .ToList();
            List<string> accountCodes = accounts
                .Where(a => a.Id != product.ChartOfAccountId)
                .Select(a => a.Code.Value)
                .ToList();

            (string productCode, string nodeCode) = ProductCodeHelper.BuildNextCodes(category.Code.Value, productCodes, accountCodes);

            product.SetProductCode(new ProductCode(productCode));

            string gtin = barcodeGeneratorService.GenerateGtin(companyPrefix, productCode);
            product.SetBarcode(new Barcode(gtin));

            if (node is not null)
            {
                node.SetCode(new AccountCode(nodeCode));
                node.SetParent(category.Id);
            }
        }

        product.SetName(new Name(request.Name));
        product.SetTaxRate(new IdentityId(request.TaxRateId));
        product.SetMinimumProductLevel(request.MinimumProductLevel);
        product.SetWarehouse(warehouse.Id);
        product.SetCategory(category.Id);
        product.SetProductUnitType(new IdentityId(request.ProductUnitTypeId));
        product.SetDescription(new Description(request.Description));
        product.SetStatus(request.IsActive);
        product.SetSemiFinishedProduct(request.SemiFinishedProductId is null
            ? null
            : new IdentityId(request.SemiFinishedProductId.Value));

        // Eski kayıtlarda boş kalmış olabilecek barkodu eksikse yeniden üret
        if (string.IsNullOrWhiteSpace(product.Barcode.Value))
        {
            string gtin = barcodeGeneratorService.GenerateGtin(companyPrefix, product.ProductCode.Value);
            product.SetBarcode(new Barcode(gtin));
        }

        string qrContent = ProductQrContentBuilder.Build(
            product.ProductCode.Value,
            product.Barcode.Value!,
            request.Name,
            taxRate.Rate,
            warehouse.Name.Value,
            category.Name.Value,
            unitType.Name.Value);
        product.SetQRCode(new QRCode(qrContent));

        if (node is not null)
        {
            node.SetName(new Name(request.Name));
            chartOfAccountRepository.Update(node);
        }

        product.ReplacePrices(request.Prices.Select(p => new ProductPrice(
            new Price(p.UnitPrice), p.PriceType, p.StartDate, p.EndDate)));

        product.ReplaceImages(request.Images.Select(i => new Photo(
            PhotoOwnerType.Product,
            product.Id,
            System.IO.Path.GetFileName(i.Path),
            GetContentType(i.Path),
            i.Path,
            i.IsPrimary)));

        const string mamulSuffix = " (MAMÜL)";
        const string yarimamulSuffix = " (YARIMAMÜL)";

        Product? companion = null;
        ChartOfAccount? companionNode = null;

        if (product.SemiFinishedProductId is { } semiId)
        {
            // Bu ürün YARIMAMÜL kartı ise eşi MAMÜL kartıdır.
            companion = await productRepository.GetByIdWithDetailsAsync(semiId, cancellationToken);
        }
        else if (warehouse.Code.Value == "151" || warehouse.Code.Value == "152"
            || warehouse.Code.Value.StartsWith("151.") || warehouse.Code.Value.StartsWith("152."))
        {
            // Bu ürün MAMÜL kartı olabilir; kendisini işaret eden YARIMAMÜL kartını bul.
            Product? yarimamul = await productRepository.FirstOrDefaultAsync(
                p => p.SemiFinishedProductId == product.Id,
                cancellationToken);
            if (yarimamul is not null)
            {
                companion = await productRepository.GetByIdWithDetailsAsync(yarimamul.Id, cancellationToken);
            }
        }

        if (companion is not null)
        {
            string baseName = request.Name.Trim();
            if (baseName.EndsWith(mamulSuffix, StringComparison.OrdinalIgnoreCase))
            {
                baseName = baseName[..^mamulSuffix.Length].TrimEnd();
            }
            else if (baseName.EndsWith(yarimamulSuffix, StringComparison.OrdinalIgnoreCase))
            {
                baseName = baseName[..^yarimamulSuffix.Length].TrimEnd();
            }

            // Eşi olan ürünün adı hangi kart düzenlenirse düzenlensin, kendi takısıyla kaydedilir.
            string productName = product.SemiFinishedProductId is not null
                ? baseName + yarimamulSuffix
                : baseName + mamulSuffix;

            string companionName = product.SemiFinishedProductId is not null
                ? baseName + mamulSuffix
                : baseName + yarimamulSuffix;

            product.SetName(new Name(productName));
            product.SetQRCode(new QRCode(ProductQrContentBuilder.Build(
                product.ProductCode.Value,
                product.Barcode.Value!,
                productName,
                taxRate.Rate,
                warehouse.Name.Value,
                category.Name.Value,
                unitType.Name.Value)));

            if (node is not null)
            {
                node.SetName(new Name(productName));
            }

            companion.SetName(new Name(companionName));
            companion.SetDescription(new Description(request.Description));
            companion.SetTaxRate(new IdentityId(request.TaxRateId));
            companion.SetMinimumProductLevel(request.MinimumProductLevel);
            companion.SetProductUnitType(new IdentityId(request.ProductUnitTypeId));
            companion.SetStatus(request.IsActive);

            ChartOfAccount? companionWarehouse = accounts.FirstOrDefault(a => a.Id == companion.WarehouseId);
            ChartOfAccount? companionCategory = accounts.FirstOrDefault(a => a.Id == companion.CategoryId);

            if (companionWarehouse is not null && companionCategory is not null)
            {
                string companionQr = ProductQrContentBuilder.Build(
                    companion.ProductCode.Value,
                    companion.Barcode.Value!,
                    companionName,
                    taxRate.Rate,
                    companionWarehouse.Name.Value,
                    companionCategory.Name.Value,
                    unitType.Name.Value);
                companion.SetQRCode(new QRCode(companionQr));
            }

            companion.ReplacePrices(request.Prices.Select(p => new ProductPrice(
                new Price(p.UnitPrice), p.PriceType, p.StartDate, p.EndDate)));

            companion.ReplaceImages(request.Images.Select(i => new Photo(
                PhotoOwnerType.Product,
                companion.Id,
                System.IO.Path.GetFileName(i.Path),
                GetContentType(i.Path),
                i.Path,
                i.IsPrimary)));

            if (companion.ChartOfAccountId is { } companionNodeId)
            {
                companionNode = accounts.FirstOrDefault(a => a.Id == companionNodeId);
            }

            if (companionNode is not null)
            {
                companionNode.SetName(new Name(companionName));
            }
        }

        try
        {
            productRepository.Update(product);
            if (node is not null)
            {
                chartOfAccountRepository.Update(node);
            }

            if (companion is not null)
            {
                productRepository.Update(companion);
                if (companionNode is not null)
                {
                    chartOfAccountRepository.Update(companionNode);
                }
            }

            // UnitOfWork enjekte edilmişse:
            // await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return Result<string>.Failure($"Veritabanı kayıt hatası: {innerMessage}");
        }

        return companion is not null ? "Ürün ve çifti başarıyla güncellendi" : "Ürün başarıyla güncellendi";
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