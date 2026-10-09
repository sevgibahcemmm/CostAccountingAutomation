using Cost.Accounting.Automation.Domain.Updates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations.Master;

internal sealed class AppReleaseConfiguration : IEntityTypeConfiguration<AppRelease>
{
    public void Configure(EntityTypeBuilder<AppRelease> builder)
    {
        builder.ToTable("AppReleases");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnType("nvarchar(32)");

        builder.Property(x => x.Notes)
            .HasMaxLength(2000)
            .HasColumnType("nvarchar(2000)");

        builder.Property(x => x.FileName)
            .HasMaxLength(260)
            .HasColumnType("nvarchar(260)");

        builder.Property(x => x.Sha256)
            .HasMaxLength(64)
            .HasColumnType("nvarchar(64)");

        builder.Property(x => x.FileContent)
            .HasColumnType("varbinary(max)");

        builder.HasIndex(x => x.Version).IsUnique();
        builder.HasIndex(x => x.VersionSort);
    }
}
