using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Tek bir ürünün liste ekranında gösterilen tüm bilgileri (künye, stok özeti, fiyat geçmişi,
/// belgeli stok hareketleri) tek seferde döndürür. Ürün detay formunun kaynağıdır.
/// </summary>
[Permission("product:view")]
public sealed record ProductDetailQuery(Guid Id) : IRequest<Result<ProductDetailDto>>;

public sealed class ProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? QRCode { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ProductUnitTypeName { get; set; } = string.Empty;
    public string TaxRateName { get; set; } = string.Empty;
    public decimal TaxRateRate { get; set; }
    public decimal? MinimumProductLevel { get; set; }
    public string? ChartOfAccountCode { get; set; }
    public string? SemiFinishedProductName { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public decimal TotalInQuantity { get; set; }
    public decimal TotalOutQuantity { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? SalePrice { get; set; }

    public List<ProductPriceDto> Prices { get; set; } = [];
    public List<ProductStockMovementDetailDto> Movements { get; set; } = [];
}

internal sealed class ProductDetailQueryHandler(
    IProductRepository productRepository,
    IProductMovementRepository productMovementRepository,
    IInvoiceRepository invoiceRepository,
    IStockIssueRepository stockIssueRepository,
    ICostSlipRepository costSlipRepository) : IRequestHandler<ProductDetailQuery, Result<ProductDetailDto>>
{
    public async Task<Result<ProductDetailDto>> Handle(ProductDetailQuery request, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdWithDetailsAsync(new IdentityId(request.Id), cancellationToken);
        if (product is null)
        {
            return Result<ProductDetailDto>.Failure("Ürün bulunamadı");
        }

        List<ProductStockMovementDetailDto> movements =
            (await ProductMovementDetailBuilder.BuildByProductAsync(
                [request.Id],
                productMovementRepository,
                invoiceRepository,
                stockIssueRepository,
                costSlipRepository,
                cancellationToken)).GetValueOrDefault(request.Id) ?? [];

        List<ProductPriceDto> prices = product.Prices
            .Select(p => new ProductPriceDto
            {
                Id = p.Id,
                PriceType = p.PriceType,
                UnitPrice = p.UnitPrice.Value,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            })
            .OrderBy(p => p.PriceType)
            .ThenByDescending(p => p.StartDate)
            .ToList();

        ProductDetailDto dto = new()
        {
            Id = product.Id,
            Name = product.Name.Value,
            ProductCode = product.ProductCode.Value,
            Barcode = product.Barcode.Value,
            QRCode = product.QRCode.Value,
            WarehouseName = product.Warehouse?.Name.Value ?? string.Empty,
            WarehouseCode = product.Warehouse?.Code.Value ?? string.Empty,
            CategoryName = product.Category?.Name.Value ?? string.Empty,
            ProductUnitTypeName = product.ProductUnitType?.Name.Value ?? string.Empty,
            TaxRateName = product.TaxRate?.Name.Value ?? string.Empty,
            TaxRateRate = product.TaxRate?.Rate ?? 0m,
            MinimumProductLevel = product.MinimumProductLevel,
            ChartOfAccountCode = product.ChartOfAccount?.Code.Value,
            SemiFinishedProductName = product.SemiFinishedProduct?.Name.Value,
            Description = product.Description.Value,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            Prices = prices,
            Movements = movements
        };

        // Atölye transferi girişi mülkiyet devridir, miktar yaratmaz; ürün
        // stok toplamlarına dâhil edilmez (500 alınan / 20 transfer = 480 kalan).
        List<ProductStockMovementDetailDto> stockMovements = movements
            .Where(m => m.MovementType != ProductMovementType.Input
                || !ProductStockBalanceHelper.IsAtelierTransferInputByDescription(m.Description))
            .ToList();

        dto.TotalInQuantity = stockMovements
            .Where(m => m.MovementType == ProductMovementType.Input)
            .Sum(m => m.Quantity);
        dto.TotalOutQuantity = stockMovements
            .Where(m => m.MovementType == ProductMovementType.Output)
            .Sum(m => m.Quantity);
        dto.StockQuantity = dto.TotalInQuantity - dto.TotalOutQuantity;

        List<ProductStockMovementDetailDto> inputs = stockMovements
            .Where(m => m.MovementType == ProductMovementType.Input && m.UnitPrice is > 0)
            .ToList();
        decimal totalInputQuantity = inputs.Sum(m => m.Quantity);
        if (totalInputQuantity > 0)
        {
            dto.CostPrice = Math.Round(inputs.Sum(m => m.Quantity * m.UnitPrice!.Value) / totalInputQuantity, 4);
        }

        dto.PurchasePrice = LatestPriceOf(prices, ProductPriceType.Purchase);
        dto.SalePrice = LatestPriceOf(prices, ProductPriceType.Sale);

        return dto;
    }

    private static decimal? LatestPriceOf(List<ProductPriceDto> prices, ProductPriceType priceType)
        => prices
            .Where(p => p.PriceType == priceType)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefault()?.UnitPrice;
}
