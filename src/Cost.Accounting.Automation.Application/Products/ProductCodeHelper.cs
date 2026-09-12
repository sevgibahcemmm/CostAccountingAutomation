using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;

namespace Cost.Accounting.Automation.Application.Products;

internal static class ProductCodeHelper
{
    public static (string ProductCode, string AccountCode) BuildNextCodes(
        string categoryCode,
        IEnumerable<string> existingProductCodes,
        IEnumerable<string> existingAccountCodes)
    {
        string prefix = $"STK-{categoryCode}.";
        HashSet<string> products = existingProductCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        HashSet<string> accounts = existingAccountCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        IEnumerable<string> suffixes = products
            .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(c => c[prefix.Length..]);

        int max = suffixes
            .Where(c => c.Length > 0 && c.All(char.IsDigit))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        int seq = max + 1;
        string productCode;
        string accountCode;

        do
        {
            productCode = $"{prefix}{seq:D5}";
            accountCode = $"{categoryCode}.{seq:D5}";
            seq++;
        }
        while (products.Contains(productCode) || accounts.Contains(accountCode));

        return (productCode, accountCode);
    }

    public static ChartOfAccount? ResolveCategory(IEnumerable<ChartOfAccount> accounts, Guid categoryId)
        => accounts.FirstOrDefault(a => a.Id == new IdentityId(categoryId)
            && a.Type == ChartOfAccountType.Category && !a.IsDeleted);

    public static ChartOfAccount? ResolveWarehouse(IEnumerable<ChartOfAccount> accounts, Guid warehouseId)
        => accounts.FirstOrDefault(a => a.Id == new IdentityId(warehouseId)
            && a.Type == ChartOfAccountType.Warehouse && !a.IsDeleted);
}