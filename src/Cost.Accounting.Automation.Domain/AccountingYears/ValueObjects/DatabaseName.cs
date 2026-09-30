namespace Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;

/// <summary>
/// Yıl veritabanının adı. Bu değer bağlantı dizesindeki "Initial Catalog"
/// alanına doğrudan yazıldığı için güvenli SQL tanımlayıcı kurallarına
/// uygunluğu burada zorlanır.
/// </summary>
public sealed record DatabaseName
{
    public const int MaxLength = 96;

    public DatabaseName(string Value)
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            throw new ArgumentException("Veritabanı adı boş olamaz.", nameof(Value));
        }

        var trimmed = Value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException(
                $"Veritabanı adı en fazla {MaxLength} karakter olabilir.",
                nameof(Value));
        }

        if (!IsValid(trimmed))
        {
            throw new ArgumentException(
                "Veritabanı adı yalnızca harf, rakam, boşluk, tire ve alt çizgi içerebilir; " +
                "harf veya rakamla başlamalı ve ' - ' dışında boşluk içermemelidir.",
                nameof(Value));
        }

        this.Value = trimmed;
    }

    public string Value { get; }

    public static implicit operator string(DatabaseName databaseName) => databaseName.Value;

    public override string ToString() => Value;

    /// <summary>
    /// Aday veritabanı adının geçerli olup olmadığını denetler ve geçerliyse
    /// <see cref="DatabaseName"/> üretir. Formlardaki anlık geri bildirim için
    /// kullanılır; kurallar <see cref="DatabaseName(string)"/> ile aynıdır.
    /// </summary>
    public static bool TryCreate(string? value, out DatabaseName? databaseName, out string? error)
    {
        databaseName = null;
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Veritabanı adı boş olamaz.";
            return false;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            error = $"Veritabanı adı en fazla {MaxLength} karakter olabilir.";
            return false;
        }

        if (!IsValid(trimmed))
        {
            error =
                "Veritabanı adı yalnızca harf, rakam, boşluk, tire ve alt çizgi içerebilir; " +
                "harf veya rakamla başlamalı ve ' - ' dışında boşluk içermemelidir.";
            return false;
        }

        databaseName = new DatabaseName(trimmed);
        return true;
    }

    private static bool IsValid(string value)
    {
        if (!char.IsLetterOrDigit(value[0]))
        {
            return false;
        }

        var previousWasSpace = false;

        foreach (var c in value)
        {
            if (c == ' ')
            {
                if (previousWasSpace)
                {
                    return false;
                }

                previousWasSpace = true;
                continue;
            }

            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
            {
                return false;
            }

            previousWasSpace = false;
        }

        return true;
    }
}
