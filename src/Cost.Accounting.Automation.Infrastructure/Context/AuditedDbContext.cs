using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cost.Accounting.Automation.Infrastructure.Context;

/// <summary>
/// Yıl veritabanı (<see cref="ApplicationDbContext"/>) ile master veritabanının
/// (<see cref="MasterDbContext"/>) paylaştığı ortak davranış:
/// denetim (audit) alanlarının doldurulması, global filtreler,
/// DuplicateKey indeksleri ve sütun tipi konvansiyonları.
/// </summary>
public abstract class AuditedDbContext(DbContextOptions options, IClaimContext claimContext)
    : DbContext(options), IUnitOfWork
{
    private Guid? _seedAdminUserId;

    public void SetSeedAdminUserId(Guid? userId)
    {
        _seedAdminUserId = userId;
    }

    public void ClearSeedAdminUserId()
    {
        _seedAdminUserId = null;
    }

    /// <summary>
    /// Her context kendi entity konfigürasyonlarını uygular; bu metot yalnızca
    /// tüm context'lerde ortak olan kısımları içerir.
    /// </summary>
    protected void ApplySharedModelConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyGlobalFilters();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var duplicateKeyProperty = entityType.FindProperty(nameof(Entity.DuplicateKey));

            if (duplicateKeyProperty is null)
            {
                continue;
            }

            duplicateKeyProperty.SetMaxLength(512);
            duplicateKeyProperty.SetColumnType("nvarchar(512)");
            entityType.AddIndex(duplicateKeyProperty);
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<IdentityId>().HaveConversion<IdentityIdValueConverter>();
        configurationBuilder.Properties<decimal>().HaveColumnType("money");
        configurationBuilder.Properties<string>().HaveColumnType("nvarchar(MAX)");
        configurationBuilder.Properties<TimeOnly>().HaveColumnType("time(7)");
        base.ConfigureConventions(configurationBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        Guid? currentUserGuid = _seedAdminUserId ?? claimContext.GetUserIdOrDefault();
        IdentityId? currentUserId = currentUserGuid is null ? null : new IdentityId(currentUserGuid.Value);
        DateTimeOffset now = DateTimeOffset.Now;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(p => p.CreatedAt).CurrentValue = now;

                    if (currentUserId is not null)
                        entry.Property(p => p.CreatedBy).CurrentValue = currentUserId;

                    break;

                case EntityState.Modified:
                    {
                        bool isDeletedTouched = entry.Property(p => p.IsDeleted).IsModified;
                        bool isDeletedNow = entry.Property(p => p.IsDeleted).CurrentValue;

                        if (isDeletedTouched && isDeletedNow)
                        {
                            entry.Property(p => p.DeletedAt).CurrentValue = now;
                            entry.Property(p => p.DeletedBy).CurrentValue = currentUserId;
                        }
                        else if (isDeletedTouched && !isDeletedNow)
                        {
                            entry.Property(p => p.DeletedAt).CurrentValue = null;
                            entry.Property(p => p.DeletedBy).CurrentValue = null;
                            entry.Property(p => p.UpdatedAt).CurrentValue = now;
                            entry.Property(p => p.UpdatedBy).CurrentValue = currentUserId;
                        }
                        else
                        {
                            entry.Property(p => p.UpdatedAt).CurrentValue = now;
                            entry.Property(p => p.UpdatedBy).CurrentValue = currentUserId;
                        }

                        break;
                    }

                case EntityState.Deleted:
                    if (entry.Entity is not IHardDeletable)
                    {
                        throw new ArgumentException(
                            $"'{entry.Entity.GetType().Name}' için Db'den direkt silme işlemi yapamazsınız. " +
                            "Hard delete için entity IHardDeletable interface'ini implemente etmeli.");
                    }

                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class IdentityIdValueConverter : ValueConverter<IdentityId, Guid>
{
    public IdentityIdValueConverter() : base(m => m.Value, m => new IdentityId(m)) { }
}
