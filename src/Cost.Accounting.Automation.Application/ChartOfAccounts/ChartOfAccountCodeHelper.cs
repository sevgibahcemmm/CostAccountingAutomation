using Cost.Accounting.Automation.Domain.ChartOfAccounts;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

public static class ChartOfAccountCodeHelper
{
    public static string NormalizeCode(string code)
    {
        string[] segments = (code ?? string.Empty)
            .Replace('-', '.')
            .Replace(',', '.')
            .Split('.', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(".", segments.Select(NormalizeSegment).Where(s => s.Length > 0));
    }

    private static string NormalizeSegment(string segment)
    {
        segment = segment.Trim();
        if (segment.Length > 0 && segment.All(char.IsDigit) && segment.Length < 2)
        {
            return segment.PadLeft(2, '0');
        }

        return segment;
    }

    /// <summary>
    /// Üst hesabın altındaki en büyük sayısal alt kodu bulup sıradaki kodu üretir.
    /// Örnek: üst "152.12" ise "152.12.01", "152.12.04" ise "152.12.05".
    /// </summary>
    public static string BuildNextChildCode(
        IReadOnlyCollection<ChartOfAccount> accounts,
        string normalizedParentCode)
    {
        string prefix = normalizedParentCode + ".";
        int max = 0;

        foreach (ChartOfAccount account in accounts)
        {
            string code = account.Code.Value;
            if (!code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string remainder = code[prefix.Length..];
            if (remainder.Contains('.') || !int.TryParse(remainder, out int number))
            {
                continue;
            }

            max = Math.Max(max, number);
        }

        return prefix + (max + 1).ToString("D2");
    }

    /// <summary>
    /// Yeni (yaprak) bir hesap için türü hesap planı sınıflandırmasıyla aynı kurallarla belirler:
    /// 150.55 altındaki yapraklar atölye, 900 altındakiler tüketim birimi,
    /// 150 / 150.98 / 151 / 152 altındakiler kategori, diğerleri anagrup.
    /// </summary>
    public static ChartOfAccountType DetermineLeafType(string normalizedCode)
    {
        if (IsUnderWorkshopRoot(normalizedCode))
        {
            return ChartOfAccountType.Workshop;
        }

        if (IsUnderConsumptionRoot(normalizedCode))
        {
            return ChartOfAccountType.ConsumptionUnit;
        }

        if (IsUnderAnyWarehouseRoot(normalizedCode))
        {
            return ChartOfAccountType.Category;
        }

        return ChartOfAccountType.MainGroup;
    }

    public static bool IsChildOf(string normalizedCode, string normalizedParentCode)
        => normalizedCode.Equals(normalizedParentCode, StringComparison.OrdinalIgnoreCase)
           || normalizedCode.StartsWith(normalizedParentCode + ".", StringComparison.OrdinalIgnoreCase);

    private static bool IsUnderWorkshopRoot(string code)
    {
        string[] segments = code.Split('.');
        return segments.Length >= 3 && segments[0] == "150" && segments[1] == "55";
    }

    private static bool IsUnderConsumptionRoot(string code)
    {
        return code.StartsWith("900", StringComparison.Ordinal)
            && (code == "900" || code.StartsWith("900.", StringComparison.Ordinal));
    }

    private static bool IsUnderAnyWarehouseRoot(string code)
    {
        return "150,150.98,151,152"
            .Split(',')
            .Any(root => IsUnderRoot(code, root));
    }

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
            if (segments[i] != rootSegments[i])
            {
                return false;
            }
        }

        return true;
    }
}