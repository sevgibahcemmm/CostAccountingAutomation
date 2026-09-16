using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Companies;
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
    IBarcodeGeneratorService barcodeGeneratorService,
    ICompanyRepository companyRepository) : IRequestHandler<ProductBarcodePreviewQuery, Result<ProductBarcodePreviewDto>>
{
    public async Task<Result<ProductBarcodePreviewDto>> Handle(ProductBarcodePreviewQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            return Result<ProductBarcodePreviewDto>.Failure("Ürün kodu boş olamaz");
        }

        string companyPrefix = await ProductBarcodePrefixResolver.ResolveAsync(companyRepository, cancellationToken);
        string gtin = barcodeGeneratorService.GenerateGtin(companyPrefix, request.ProductCode);

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
        return Result<ProductBarcodePreviewDto>.Succeed(dto);
    }
}

/// <summary>
/// Aktif şirketin CompanyPrefix'i bulunamazsa kullanılan varsayılan firma ön eki.
/// </summary>
public static class ProductBarcodeDefaults
{
    public const string CompanyGtinPrefix = "8690000";
}