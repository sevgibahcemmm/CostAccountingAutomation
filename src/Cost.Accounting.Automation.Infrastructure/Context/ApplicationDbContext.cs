using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Infrastructure.Context;

public sealed class ApplicationDbContext(DbContextOptions options, IClaimContext claimContext)
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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplyGlobalFilters();
        base.OnModelCreating(modelBuilder);
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