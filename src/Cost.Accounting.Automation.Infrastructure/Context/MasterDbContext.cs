using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.LoginTokens;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Context;

public sealed class MasterDbContext(DbContextOptions<MasterDbContext> options, IClaimContext claimContext)
    : AuditedDbContext(options, claimContext), IMasterUnitOfWork
{
    public const string ConfigurationNamespace = "Cost.Accounting.Automation.Infrastructure.Configurations.Master";

    public DbSet<CompanyYear> CompanyYears => Set<CompanyYear>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<LoginToken> LoginTokens => Set<LoginToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MasterDbContext).Assembly,
            (Type configurationType) => configurationType.Namespace
                ?.StartsWith(ConfigurationNamespace, StringComparison.Ordinal) == true);

        ApplySharedModelConfiguration(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}
