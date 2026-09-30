using Cost.Accounting.Automation.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Context;

/// <summary>
/// Seçili şirket + mali yılün iş veritabanı. Bağlantı dizesi çalışma anında
/// <c>IAccountingDbSelector</c> üzerinden gelir; bu sayede yıl değiştiğinde
/// aynı kayıt tipleri farklı veritabanına yönlendirilebilir.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IClaimContext claimContext)
    : AuditedDbContext(options, claimContext)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Not: Bu overload'a EF Core "Type" geçirir. Parametre tipi acikca
        // yazilmazsa derleyici IEntityTypeConfiguration overload'uni secebilir
        // ve filtre sessizce hicbir seyi eleyemez.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            (Type configurationType) => configurationType.Namespace
                ?.StartsWith(MasterDbContext.ConfigurationNamespace, StringComparison.Ordinal) != true);

        ApplySharedModelConfiguration(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}
