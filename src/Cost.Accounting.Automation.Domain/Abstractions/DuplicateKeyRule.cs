using System.Globalization;

namespace Cost.Accounting.Automation.Domain.Abstractions;

public static class DuplicateKeyRule
{
    public static string Normalize(string? value)
        => value?.Trim().ToUpperInvariant() ?? string.Empty;

    public static string? From(params object?[] parts)
    {
        string joined = string.Join("|", parts.Select(p =>
        {
            if (p is null)
            {
                return "-";
            }

            string value = p switch
            {
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => p.ToString() ?? string.Empty
            };

            return string.IsNullOrWhiteSpace(value) ? "-" : Normalize(value);
        }));

        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }
}