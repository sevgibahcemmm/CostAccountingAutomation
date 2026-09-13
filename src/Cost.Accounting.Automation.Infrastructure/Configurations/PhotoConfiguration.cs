using Cost.Accounting.Automation.Domain.Photos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("Photos");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.OwnerType).HasMaxLength(20).HasConversion<string>();
        builder.Property(i => i.FileName).HasMaxLength(255);
        builder.Property(i => i.ContentType).HasMaxLength(100);
        builder.Property(i => i.Path).HasColumnName("Path").HasMaxLength(500);
    }
}