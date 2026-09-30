namespace Cost.Accounting.Automation.Infrastructure.Options;

/// <summary>
/// Yıl veritabanı adlandırma ayarları. Ad, yılın başında yer alacak biçimde
/// üretilir; böylece SSMS Object Explorer'da veritabanları yıla göre gruplanır.
/// </summary>
public sealed class DatabaseNamingOptions
{
    public const string SectionName = "DatabaseNaming";

    /// <summary>Her yıl veritabanı adının başında yer alan kısa ön ek.</summary>
    public string Prefix { get; set; } = "CAA";
}
