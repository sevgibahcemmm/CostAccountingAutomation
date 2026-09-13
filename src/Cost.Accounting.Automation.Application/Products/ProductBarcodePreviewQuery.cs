using Cost.Accounting.Automation.Application.Services;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Ürünün güncel form alanlarından üretilen barkod (GTIN) ve karekod önizleme görselleri.
/// Sadece görüntüleme amaçlıdır; kalıcı barkod/karekod değerleri kayıt sırasında
/// ProductCreateCommandHandler / ProductUpdateCommandHandler içinde aynı kurala göre üretilir.
/// </summary>
public sealed record ProductBarcodePreviewDto(string Gtin, string QrContent, byte[] BarcodeImage, byte[] QrImage);

public sealed record ProductBarcodePreviewQuery(
    string ProductCode,
    string ProductName,
    decimal TaxRate,
    string WarehouseName,
    string CategoryName,
    string UnitTypeName) : IRequest<Result<ProductBarcodePreviewDto>>;

internal sealed class ProductBarcodePreviewQueryHandler(
    IBarcodeGeneratorService barcodeGeneratorService) : IRequestHandler<ProductBarcodePreviewQuery, Result<ProductBarcodePreviewDto>>
{
    public Task<Result<ProductBarcodePreviewDto>> Handle(ProductBarcodePreviewQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            return Task.FromResult(Result<ProductBarcodePreviewDto>.Failure("Ürün kodu boş olamaz"));
        }

        string gtin = barcodeGeneratorService.GenerateGtin(ProductBarcodeDefaults.CompanyGtinPrefix, request.ProductCode);

        string qrContent = ProductQrContentBuilder.Build(
            request.ProductCode,
            gtin,
            request.ProductName,
            request.TaxRate,
            request.WarehouseName,
            request.CategoryName,
            request.UnitTypeName);

        byte[] barcodeImage = barcodeGeneratorService.GenerateEan13Barcode(gtin);
        byte[] qrImage = barcodeGeneratorService.GenerateQrCode(qrContent);

        ProductBarcodePreviewDto dto = new(gtin, qrContent, barcodeImage, qrImage);
        return Task.FromResult<Result<ProductBarcodePreviewDto>>(dto);
    }
}

/// <summary>
/// GS1 firma kodu burada merkezi olarak tutulur; gerçek atanmış firma prefiksinizle değiştirin
/// (ör. appsettings üzerinden IOptions ile de sağlanabilir).
/// </summary>
public static class ProductBarcodeDefaults
{
    public const string CompanyGtinPrefix = "8690000";
}