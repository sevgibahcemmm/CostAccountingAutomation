using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:view")]
public sealed record ProductGetQuery(
    Guid Id) : IRequest<Result<ProductDto>>;

internal sealed class ProductGetQueryHandler(
    IProductRepository productRepository) : IRequestHandler<ProductGetQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(ProductGetQuery request, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdWithDetailsAsync(new IdentityId(request.Id), cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Ürün bulunamadı");
        }

        ProductDto dto = new()
        {
            Id = product.Id,
            Name = product.Name.Value,
            ProductCode = product.ProductCode.Value,
            Barcode = product.Barcode.Value,
            QRCode = product.QRCode.Value,
            MinimumProductLevel = product.MinimumProductLevel,
            TaxRateId = product.TaxRateId,
            TaxRateName = product.TaxRate?.Name.Value ?? string.Empty,
            TaxRateRate = product.TaxRate?.Rate ?? 0m,

            // Sadece null gelme ihtimali olan navigation nesnelerine güvenlik eklendi
            WarehouseId = product.WarehouseId,
            WarehouseCode = product.Warehouse?.Code.Value ?? string.Empty,
            WarehouseName = product.Warehouse?.Name.Value ?? string.Empty,

            CategoryId = product.CategoryId,
            CategoryCode = product.Category?.Code.Value ?? string.Empty,
            CategoryName = product.Category?.Name.Value ?? string.Empty,

            ProductUnitTypeId = product.ProductUnitTypeId,
            ProductUnitTypeName = product.ProductUnitType?.Name.Value ?? string.Empty,

            ChartOfAccountId = product.ChartOfAccountId == null ? null : product.ChartOfAccountId.Value,
            ChartOfAccountCode = product.ChartOfAccount?.Code.Value,
            SemiFinishedProductId = product.SemiFinishedProductId == null ? null : product.SemiFinishedProductId.Value,
            SemiFinishedProductName = product.SemiFinishedProduct?.Name.Value,
            Description = product.Description.Value,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            CreatedFullName = "-",

            Prices = product.Prices
                .Select(p => new ProductPriceDto
                {
                    Id = p.Id,
                    PriceType = p.PriceType,
                    UnitPrice = p.UnitPrice.Value,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                })
                .ToList(),

            Movements = product.Movements
                .Select(m => new ProductMovementDto
                {
                    Id = m.Id,
                    MovementType = m.MovementType,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice?.Value,
                    Date = m.Date,
                    ReferenceNo = m.ReferenceNo,
                    Description = m.Description.Value
                })
                .ToList(),

            Images = product.Images
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    Path = i.Path,
                    IsPrimary = i.IsDefault
                })
                .ToList()
        };

        return dto;
    }
}