using Cost.Accounting.Automation.Domain.Companies;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// EAN-13 barkodu için kullanılacak firma ön ekini çözümler.
/// Öncelik aktif şirketin CompanyPrefix değeri kullanılır; bulunamazsa
/// statik varsayılan (ProductBarcodeDefaults.CompanyGtinPrefix) devreye girer.
/// </summary>
internal static class ProductBarcodePrefixResolver
{
    public static async Task<string> ResolveAsync(
        ICompanyRepository companyRepository,
        CancellationToken cancellationToken)
    {
        string? prefix = await companyRepository.GetAllWithAudit()
            .Where(c => c.Entity.IsActive)
            .Select(c => c.Entity.CompanyPrefix.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = ProductBarcodeDefaults.CompanyGtinPrefix;
        }

        string digits = new(prefix.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? ProductBarcodeDefaults.CompanyGtinPrefix : digits;
    }
}