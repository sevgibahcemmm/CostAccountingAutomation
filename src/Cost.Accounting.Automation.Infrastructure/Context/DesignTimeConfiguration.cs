using Cost.Accounting.Automation.Infrastructure.Context.Conventions;
using Microsoft.Extensions.Configuration;

namespace Cost.Accounting.Automation.Infrastructure.Context;

internal static class DesignTimeConfiguration
{
    internal const string DefaultMasterConnectionString =
        "Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = CostAccountingAutomationMaster; " +
        "Integrated Security = True; Connect Timeout = 30; Encrypt = False; " +
        "Trust Server Certificate = False; Application Intent = ReadWrite; Multi Subnet Failover = False";

    internal const string YearDesignTimeCatalog = "EA_DesignTime_Year_NotForUpdates";

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
