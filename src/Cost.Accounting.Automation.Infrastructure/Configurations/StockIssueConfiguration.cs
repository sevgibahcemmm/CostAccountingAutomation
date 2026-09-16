using Cost.Accounting.Automation.Domain.StockIssues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class StockIssueConfiguration : IEntityTypeConfiguration<StockIssue>
{
    public void Configure(EntityTypeBuilder<StockIssue> builder)
    {
        builder.ToTable("StockIssues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentNumber)
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IssueType).IsRequired();
        builder.Property(x => x.CostingMethod).IsRequired();

        builder.OwnsOne(x => x.Description, desc =>
            desc.Property(p => p.Value)
                .HasColumnName("Description")
                .HasMaxLength(500));

        builder.HasIndex(x => x.DocumentNumber);
        builder.HasIndex(x => x.IssueType);
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.SourceWarehouseId);
        builder.HasIndex(x => x.TargetAccountId);

        builder.HasOne(x => x.SourceWarehouse)
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetAccount)
            .WithMany()
            .HasForeignKey(x => x.TargetAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.StockIssueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class StockIssueLineConfiguration : IEntityTypeConfiguration<StockIssueLine>
{
    public void Configure(EntityTypeBuilder<StockIssueLine> builder)
    {
        builder.ToTable("StockIssueLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .HasColumnType("decimal(18,4)");

        builder.OwnsOne(x => x.UnitCost, cost =>
            cost.Property(x => x.Value).HasColumnName("UnitCost"));

        builder.OwnsOne(x => x.Description, desc =>
            desc.Property(p => p.Value).HasColumnName("Description"));

        builder.HasIndex(x => x.StockIssueId);
        builder.HasIndex(x => x.ProductId);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
