using Cost.Accounting.Automation.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cost.Accounting.Automation.Infrastructure.Context;

/// <summary>
/// <c>dotnet ef migrations ...</c> komutları master veritabanı için bu fabrikayı
/// kullanır. Migration'lar <c>Migrations\Master</c> klasöründe üretilir.
/// </summary>
public sealed class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
        optionsBuilder.UseSqlServer(DesignTimeConfiguration.GetMasterConnectionString());

        return new MasterDbContext(optionsBuilder.Options, new DesignTimeClaimContext());
    }
}
