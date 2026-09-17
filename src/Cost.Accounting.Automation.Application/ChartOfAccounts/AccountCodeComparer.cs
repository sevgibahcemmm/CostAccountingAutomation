namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

/// <summary>
/// "150.02" → "150.10" → "150.98.01" gibi nokta ile ayrılmış hesap kodlarını
/// her bir segmenti sayısal olarak (sayı değilse metinsel) karşılaştırarak sıralar.
/// Böylece "150.10", "150.2"den sonra gelir.
/// </summary>
internal sealed class AccountCodeComparer : IComparer<string>
{
    public static AccountCodeComparer Instance { get; } = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        string[] xs = x.Split('.');
        string[] ys = y.Split('.');
        int count = Math.Max(xs.Length, ys.Length);

        for (int i = 0; i < count; i++)
        {
            string? a = i < xs.Length ? xs[i] : null;
            string? b = i < ys.Length ? ys[i] : null;

            if (a is null)
            {
                return -1;
            }

            if (b is null)
            {
                return 1;
            }

            int cmp = CompareSegment(a, b);
            if (cmp != 0)
            {
                return cmp;
            }
        }

        return 0;
    }

    private static int CompareSegment(string a, string b)
    {
        bool aIsNumber = long.TryParse(a, out long aNumber);
        bool bIsNumber = long.TryParse(b, out long bNumber);

        if (aIsNumber && bIsNumber)
        {
            return aNumber.CompareTo(bNumber);
        }

        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }
}