using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context.Conventions;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cost.Accounting.Automation.Infrastructure.Context;

public abstract class AuditedDbContext(DbContextOptions options, IClaimContext claimContext)
    : DbContext(options), IUnitOfWork
{
    private readonly EntityAuditTracker _auditTracker = new(claimContext);

    public void SetSeedAdminUserId(Guid? userId) => _auditTracker.SetSeedAdminUserId(userId);

    public void ClearSeedAdminUserId() => _auditTracker.ClearSeedAdminUserId();

    protected void ApplySharedModelConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyGlobalFilters();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var duplicateKeyProperty = entityType.FindProperty(nameof(Entity.DuplicateKey));

            if (duplicateKeyProperty is not null)
            {
                duplicateKeyProperty.SetMaxLength(DbConventionDefaults.DuplicateKeyMaxLength);
                duplicateKeyProperty.SetColumnType(DbConventionDefaults.DuplicateKeyColumnType);
                entityType.AddIndex(duplicateKeyProperty);
            }

            ApplyRowVersion(entityType);
        }
    }


    private static void ApplyRowVersion(IMutableEntityType entityType)
    {
        if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
        {
            return;
        }

        var rowVersionProperty = entityType.FindProperty(nameof(Entity.RowVersion));

        if (rowVersionProperty is null)
        {
            return;
        }

        rowVersionProperty.IsConcurrencyToken = true;
        rowVersionProperty.ValueGenerated = ValueGenerated.OnAddOrUpdate;
        rowVersionProperty.SetColumnType(DbConventionDefaults.RowVersionColumnType);
        rowVersionProperty.SetMaxLength(DbConventionDefaults.RowVersionLength);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<IdentityId>().HaveConversion<IdentityIdValueConverter>();
        configurationBuilder.Properties<decimal>().HaveColumnType(DbConventionDefaults.MoneyColumnType);
        configurationBuilder.Properties<string>().HaveColumnType(DbConventionDefaults.StringColumnType);
        configurationBuilder.Properties<TimeOnly>().HaveColumnType(DbConventionDefaults.TimeColumnType);
        base.ConfigureConventions(configurationBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _auditTracker.ApplyAudit(ChangeTracker);
        return base.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class IdentityIdValueConverter : ValueConverter<IdentityId, Guid>
{
    public IdentityIdValueConverter() : base(m => m.Value, m => new IdentityId(m)) { }
}
