namespace Cost.Accounting.Automation.Infrastructure.Context.Conventions;

internal static class DbConventionDefaults
{
    public const string MoneyColumnType = "money";
    public const string StringColumnType = "nvarchar(MAX)";
    public const string TimeColumnType = "time(7)";
    public const int DuplicateKeyMaxLength = 512;
    public const string DuplicateKeyColumnType = "nvarchar(512)";
}
