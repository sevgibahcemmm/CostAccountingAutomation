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

[Permission("product:update")]
public sealed record ProductUpdateCommand(
    Guid Id,
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

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("KDV oranı sıfırdan küçük olamaz")
            .LessThanOrEqualTo(1).WithMessage("KDV oranı %100'den büyük olamaz");

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
    IProductUnitTypeRepository unitTypeRepository) : IRequestHandler<ProductUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ProductUpdateCommand request, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdWithDetailsAsync(new IdentityId(request.Id), cancellationToken);
        if (product is null)
        {
            return Result<string>.Failure("Ürün bulunamadı");
        }

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

            if (node is not null)
            {
                node.SetCode(new AccountCode(nodeCode));
                node.SetParent(category.Id);
            }
        }

        product.SetName(new Name(request.Name));
        product.SetBarcode(new Barcode(string.IsNullOrWhiteSpace(request.Barcode) ? string.Empty : request.Barcode.Trim()));
        product.SetQRCode(new QRCode(string.IsNullOrWhiteSpace(request.QRCode) ? string.Empty : request.QRCode.Trim()));
        product.SetTaxRate(request.TaxRate);
        product.SetMinimumProductLevel(request.MinimumProductLevel);
        product.SetWarehouse(warehouse.Id);
        product.SetCategory(category.Id);
        product.SetProductUnitType(new IdentityId(request.ProductUnitTypeId));
        product.SetDescription(new Description(request.Description));
        product.SetStatus(request.IsActive);

        if (node is not null)
        {
            node.SetName(new Name(request.Name));
            chartOfAccountRepository.Update(node);
        }

        product.ReplacePrices(request.Prices.Select(p => new ProductPrice(
            new Price(p.UnitPrice), p.PriceType, p.StartDate, p.EndDate)));

        product.ReplaceImages(request.Images.Select(i => new ProductImage(i.Path, i.IsPrimary)));

        productRepository.Update(product);

        return "Ürün başarıyla güncellendi";
    }
}