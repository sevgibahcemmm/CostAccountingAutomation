using Cost.Accounting.Automation.Domain.ChartOfAccounts;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

internal static class ConsumptionUnitHelper
{
    public const string RootCode = "900";
    public const string RootName = "Tüketimler";

    public static ChartOfAccount? FindRoot(IEnumerable<ChartOfAccount> accounts)
        => accounts.FirstOrDefault(a =>
            !a.IsDeleted &&
            string.Equals(a.Code.Value, RootCode, StringComparison.OrdinalIgnoreCase));

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

    public static string BuildNextCode(IEnumerable<ChartOfAccount> accounts)
    {
        int max = 0;

        foreach (ChartOfAccount account in accounts)
        {
            string code = account.Code.Value;
            if (!code.StartsWith(RootCode + ".", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string[] segments = code.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 2 && int.TryParse(segments[1], out int number))
            {
                max = Math.Max(max, number);
            }
        }

        return $"{RootCode}.{(max + 1):D2}";
    }
}
