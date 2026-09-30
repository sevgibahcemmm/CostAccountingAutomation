using Microsoft.Extensions.Configuration;

namespace Cost.Accounting.Automation.Infrastructure.Context;

/// <summary>
/// <c>dotnet ef</c> gibi tasarım zamanı araçlarının bağlantı dizesini
/// bulabilmesi için ortak yardımcı.
///
/// Master için bağlantı dizesi <c>appsettings.json</c> dosyasından okunur.
/// Yıl veritabanı için ise <u>asla</u> gerçek bir katalog kullanılmaz: migration
/// üretimi sırasında EF modeli inceleyerek şemayı oluşturur ve hiçbir bağlantı
/// açmaz. Bu nedenle var olmayan bir yer tutucu katalog adı kullanılır; böylece
/// yanlışlıkla <c>dotnet ef database update --context ApplicationDbContext</c>
/// komutu çalıştırılsa bile iş verisi master ya da başka bir gerçek veritabanına
/// yazılmaz, "veritabanı bulunamadı" hatasıyla durur.
///
/// Yıl veritabanları yalnızca çalışma anında <c>IAccountingYearProvisioner</c>
/// tarafından, kullanıcının seçtiği adla açılır.
/// </summary>
internal static class DesignTimeConfiguration
{
    /// <summary>Master veritabanı için gerçek bağlantı dizesi.</summary>
    internal const string DefaultMasterConnectionString =
        "Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = CostAccountingAutomationMaster; " +
        "Integrated Security = True; Connect Timeout = 30; Encrypt = False; " +
        "Trust Server Certificate = False; Application Intent = ReadWrite; Multi Subnet Failover = False";

    /// <summary>
    /// Migration üretimi için yer tutucu katalog adı. Bilerek mevcut değildir.
    /// </summary>
    internal const string YearDesignTimeCatalog = "CAA_DesignTime_Year_NotForUpdates";

    internal static string GetMasterConnectionString()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(FindSettingsDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return configuration.GetConnectionString("Master") is { Length: > 0 } configured
            ? configured
            : DefaultMasterConnectionString;
    }

    /// <summary>
    /// Migration üretimi sırasında bağlantı açılmayacağı için katalog adının
    /// ne olduğu önemsizdir; yine de var olmayan bir yer tutucu kullanılır.
    /// </summary>
    internal static string GetYearDesignTimeConnectionString()
        => DefaultMasterConnectionString.Replace(
            "Initial Catalog = CostAccountingAutomationMaster",
            $"Initial Catalog = {YearDesignTimeCatalog}",
            StringComparison.Ordinal);

    private static string FindSettingsDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
