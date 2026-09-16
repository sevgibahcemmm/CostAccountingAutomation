namespace Cost.Accounting.Automation.Application.Services;

public interface IBarcodeGeneratorService
{
    byte[] GenerateEan13Barcode(string gtin);
    byte[] GenerateQrCode(string data);
    /// <summary>
    /// EAN-13 GTIN üretir: Ülke kodu + Firma kodu (companyPrefix) + Ürün kodu + kontrol rakamı.
    /// </summary>
    string GenerateGtin(string companyPrefix, string productCode);
}