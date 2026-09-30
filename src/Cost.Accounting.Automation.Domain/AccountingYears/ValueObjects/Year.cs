namespace Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;

/// <summary>
/// Mali yıl. Yıl veritabanının adı bu değerden türetildiği için
/// mantıklı bir aralık dışındaki değerler kabul edilmez.
/// </summary>
public sealed record Year
{
    public const int MinSupported = 1900;
    public const int MaxSupported = 2999;

    public Year(int Value)
    {
        if (Value < MinSupported || Value > MaxSupported)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Value),
                Value,
                $"Mali yıl {MinSupported}-{MaxSupported} aralığında olmalıdır.");
        }

        this.Value = Value;
    }

    public int Value { get; }

    public static implicit operator int(Year year) => year.Value;
    public static implicit operator string(Year year) => year.Value.ToString();

    public override string ToString() => Value.ToString();
}
