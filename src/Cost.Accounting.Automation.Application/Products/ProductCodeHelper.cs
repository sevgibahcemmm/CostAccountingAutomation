using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;

namespace Cost.Accounting.Automation.Application.Products;

public static class ProductCodeHelper
{
    /// <summary>Ürün tanımında "Depo" gibi seçilebilen giderler hesabı öğesi.</summary>
    public const string ExpenseWarehouseCode = "630";

    /// <summary>
    /// 630 seçildiğinde kategori olarak yalnızca en son kırılım (yaprak)
    /// hesaplarının listelendiği kök gider hesabı.
    /// </summary>
    public const string ExpenseCategoryRootCode = "630.16.03";

    /// <summary>Bu depo seçiminde "Mamül / Yarı Mamül" (630 için "Hizmet / Yarı Mamül") çifti kurulabilir.</summary>
    public static bool IsPairWarehouse(string code)
        => Is151Or152PairCode(code) || IsExpenseWarehouse(code);

    public static bool IsExpenseWarehouse(string code)
        => string.Equals(code, ExpenseWarehouseCode, StringComparison.OrdinalIgnoreCase);

    /// <summary>630.16.03 altında olan ve yapısal alt hesabı bulunmayan (en son kırılım) hesap.</summary>
    public static bool IsExpenseCategoryLeaf(IEnumerable<ChartOfAccount> accounts, ChartOfAccount account)
    {
        if (account.IsDeleted || !IsUnderRoot(account.Code.Value, ExpenseCategoryRootCode))
        {
            return false;
        }

        // Ürün kartları (Stok düğümleri) kategori altında açılır; bunlar hesabın
        // "yaprak" olma özelliğini bozmaz. Yalnızca yapısal alt hesaplar dikkate alınır.
        return !accounts.Any(a =>
            !a.IsDeleted &&
            a.ParentId == account.Id &&
            a.Type != ChartOfAccountType.Stok);
    }

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
    {
        ChartOfAccount? account = accounts.FirstOrDefault(a => a.Id == new IdentityId(categoryId) && !a.IsDeleted);
        if (account is null)
        {
            return null;
        }

        if (account.Type == ChartOfAccountType.Category)
        {
            return account;
        }

        // 630 gider hesabı seçiminde kategori olarak kullanılan 630.16.03 yaprakları da kabul edilir.
        return IsExpenseCategoryLeaf(accounts, account) ? account : null;
    }

    public static ChartOfAccount? ResolveWarehouse(IEnumerable<ChartOfAccount> accounts, Guid warehouseId)
    {
        ChartOfAccount? account = accounts.FirstOrDefault(a => a.Id == new IdentityId(warehouseId) && !a.IsDeleted);
        if (account is null)
        {
            return null;
        }

        // 630 = GİDERLER HESABI, ürün tanımında "Depo" gibi seçilebilir.
        return account.Type == ChartOfAccountType.Warehouse || IsExpenseWarehouse(account.Code.Value)
            ? account
            : null;
    }

    private static bool Is151Or152PairCode(string code)
        => code == "151" || code == "152"
            || code.StartsWith("151.", StringComparison.Ordinal) || code.StartsWith("152.", StringComparison.Ordinal);

    private static bool IsUnderRoot(string code, string rootKey)
    {
        string[] rootSegments = rootKey.Split('.');
        string[] segments = code.Split('.');
        if (segments.Length < rootSegments.Length)
        {
            return false;
        }

        for (int i = 0; i < rootSegments.Length; i++)
        {
            if (!string.Equals(segments[i], rootSegments[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}