using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class CostSlipConfiguration : IEntityTypeConfiguration<CostSlip>
{
    public void Configure(EntityTypeBuilder<CostSlip> builder)
    {
        builder.ToTable("CostSlips");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SlipNumber)
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CostSlipType).IsRequired();
        builder.Property(x => x.CostDate).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();

        builder.OwnsOne(x => x.Description, desc =>
            desc.Property(p => p.Value)
                .HasColumnName("Description")
                .HasMaxLength(500));

        builder.Property(x => x.GrandTotal)
            .HasColumnType("money");

        builder.HasIndex(x => x.SlipNumber);
        builder.HasIndex(x => x.CostSlipType);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CostDate);
        builder.HasIndex(x => x.WorkshopId);
        builder.HasIndex(x => x.ProducedProductId);
        builder.HasIndex(x => x.CustomerId);

        builder.HasOne(x => x.Workshop)
            .WithMany()
            .HasForeignKey(x => x.WorkshopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProducedProduct)
            .WithMany()
            .HasForeignKey(x => x.ProducedProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CostSlipItemConfiguration : IEntityTypeConfiguration<CostSlipItem>
{
    public void Configure(EntityTypeBuilder<CostSlipItem> builder)
    {
        builder.ToTable("CostSlipItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.UnitPrice)
            .HasColumnType("money");

        builder.Property(x => x.ExpenseAccountType)
            .IsRequired();

        builder.OwnsOne(x => x.Description, desc =>
            desc.Property(p => p.Value).HasColumnName("Description"));

        builder.HasIndex(x => x.CostSlipId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.ProductUnitTypeId);

        builder.HasOne(x => x.CostSlip)
            .WithMany(x => x.CostSlipItems)
            .HasForeignKey(x => x.CostSlipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProductUnitType)
            .WithMany()
            .HasForeignKey(x => x.ProductUnitTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}