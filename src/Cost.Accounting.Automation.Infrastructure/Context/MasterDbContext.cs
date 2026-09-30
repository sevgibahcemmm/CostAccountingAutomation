using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.LoginTokens;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Configurations.Master;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Context;

/// <summary>
/// Şirket ve mali yıl kayıtlarının tutulduğu merkezi (master) veritabanı.
/// Yıl veritabanlarından bağımsız olarak her zaman erişilebilir olmalıdır;
/// giriş ekranı ve yıl seçim ekranı yıl seçilmeden önce yalnızca bu
/// veritabanına bağlanır.
/// </summary>
public sealed class MasterDbContext(DbContextOptions<MasterDbContext> options, IClaimContext claimContext)
    : AuditedDbContext(options, claimContext), IMasterUnitOfWork
{
    /// <summary>
    /// Yalnızca master veritabanına ait entity konfigürasyonlarının namespace'i.
    /// <see cref="ApplicationDbContext"/> bu namespace'i tarama dışı bırakır;
    /// aksi halde master entity'leri her yıl veritabanına da sızar.
    /// </summary>
    public const string ConfigurationNamespace = "Cost.Accounting.Automation.Infrastructure.Configurations.Master";

    public DbSet<CompanyYear> CompanyYears => Set<CompanyYear>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<LoginToken> LoginTokens => Set<LoginToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CompanyYearConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new LoginTokenConfiguration());
        ApplySharedModelConfiguration(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}
