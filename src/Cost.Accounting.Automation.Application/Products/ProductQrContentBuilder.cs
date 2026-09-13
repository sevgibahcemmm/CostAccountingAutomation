namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Karekodun içine gömülecek, ürünle ilgili tüm temel bilgileri barındıran metni üretir.
/// Önizleme (ProductBarcodePreviewQuery) ve kayıt (ProductCreateCommand/ProductUpdateCommand)
/// aynı formatı kullanmalı; bu yüzden tek merkezden üretiliyor.
/// </summary>
public static class ProductQrContentBuilder
{
    public static string Build(
        string productCode,
        string gtin,
        string productName,
        decimal taxRate,
        string warehouseName,
        string categoryName,
        string unitTypeName)
    {
        return $"""
            Ürün: {productName}
            Stok Kodu: {productCode}
            Barkod: {gtin}
            KDV: %{taxRate * 100:0.##}
            Depo: {warehouseName}
            Kategori: {categoryName}
            Birim: {unitTypeName}
            """;
    }
}