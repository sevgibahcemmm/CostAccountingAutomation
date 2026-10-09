using Cost.Accounting.Automation.Domain.AppReleases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations.Master;

internal sealed class AppReleaseConfiguration : IEntityTypeConfiguration<AppRelease>
{
    public void Configure(EntityTypeBuilder<AppRelease> builder)
    {
        builder.ToTable("AppReleases");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version).HasMaxLength(32).HasColumnType("nvarchar(64)").IsRequired();
        builder.Property(x => x.SetupPath).HasMaxLength(500).HasColumnType("nvarchar(500)").IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000).HasColumnType("nvarchar(2000)");

        // Aynı sürüm numarasından yalnızca bir kayıt olur; sync-updates.ps1
        // ve caa-provision set-version bu anahtar üzerinden MERGE yapar.
        builder.HasIndex(x => x.Version).IsUnique();
    }
}