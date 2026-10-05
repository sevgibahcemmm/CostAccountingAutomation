namespace Cost.Accounting.Automation.Infrastructure.Context.Conventions;

internal static class DbConventionDefaults
{
    public const string MoneyColumnType = "money";
    public const string StringColumnType = "nvarchar(MAX)";
    public const string TimeColumnType = "time(7)";
    public const int DuplicateKeyMaxLength = 512;
    public const string DuplicateKeyColumnType = "nvarchar(512)";

    /// <summary>
    /// Eşzamanlılık belirtecinin veritabanı türü. SQL Server'da
    /// <c>rowversion</c>, eski adıyla <c>timestamp</c>.
    /// </summary>
    public const string RowVersionColumnType = "rowversion";

    /// <summary>Bir <c>rowversion</c> değeri 8 bayttır.</summary>
    public const int RowVersionLength = 8;
}
