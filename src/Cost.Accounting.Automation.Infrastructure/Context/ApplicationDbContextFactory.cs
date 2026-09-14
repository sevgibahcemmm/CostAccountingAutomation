using Cost.Accounting.Automation.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cost.Accounting.Automation.Infrastructure.Context;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        string connectionString = configuration.GetConnectionString("SqlServer")
            ?? "Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = CostAccountingAutomationDb; Integrated Security = True; Connect Timeout = 30; Encrypt = False; Trust Server Certificate = False; Application Intent = ReadWrite; Multi Subnet Failover = False";// Server=.;Database=CostAccountingAutomation;Trusted_Connection=True;TrustServerCertificate=True;";*/

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeClaimContext());
    }

    private sealed class DesignTimeClaimContext : IClaimContext
    {
        public Guid GetUserId() => Guid.Empty;
        public Guid GetCompanyId() => Guid.Empty;
        public string GetRoleName() => string.Empty;
        public Guid? GetUserIdOrDefault() => null;
    }
}