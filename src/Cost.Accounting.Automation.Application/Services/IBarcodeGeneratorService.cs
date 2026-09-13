namespace Cost.Accounting.Automation.Application.Services;

public interface IBarcodeGeneratorService
{
    byte[] GenerateEan13Barcode(string gtin);
    byte[] GenerateQrCode(string data);
    string GenerateGtin(string companyCode, string productCode);
}