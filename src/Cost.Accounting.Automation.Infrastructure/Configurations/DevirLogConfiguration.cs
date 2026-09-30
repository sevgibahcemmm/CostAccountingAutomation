using Cost.Accounting.Automation.Domain.Devirs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class DevirLogConfiguration : IEntityTypeConfiguration<DevirLog>
{
    public void Configure(EntityTypeBuilder<DevirLog> builder)
    {
        builder.ToTable("DevirLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceYear)
            .IsRequired();

        builder.Property(x => x.TargetYear)
            .IsRequired();

        builder.Property(x => x.SourceDatabaseName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.TargetDatabaseName)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.TargetYear);
    }
}
