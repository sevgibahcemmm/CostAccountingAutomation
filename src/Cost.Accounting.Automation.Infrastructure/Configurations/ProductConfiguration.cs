using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.Name, name =>
            name.Property(x => x.Value).HasColumnName("Name").HasMaxLength(300));

        builder.OwnsOne(p => p.Description, description =>
            description.Property(x => x.Value).HasColumnName("Description"));

        builder.Property(p => p.ProductCode)
            .HasConversion(v => v.Value, v => new ProductCode(v))
            .HasColumnName("ProductCode")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(p => p.Barcode)
            .HasConversion(v => v.Value, v => new Barcode(v))
            .HasColumnName("Barcode")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(p => p.QRCode)
            .HasConversion(v => v.Value, v => new QRCode(v))
            .HasColumnName("QRCode")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(p => p.ProductCode).IsUnique();
        builder.HasIndex(p => p.CreatedBy);
        builder.HasIndex(p => p.UpdatedBy);
        builder.HasIndex(p => p.WarehouseId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.ProductUnitTypeId);
        builder.HasIndex(p => p.ChartOfAccountId);
        builder.HasIndex(p => p.TaxRateId);
        builder.HasIndex(p => p.SemiFinishedProductId);

        builder.HasOne(p => p.Warehouse)
            .WithMany()
            .HasForeignKey(p => p.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProductUnitType)
            .WithMany()
            .HasForeignKey(p => p.ProductUnitTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.TaxRate)
            .WithMany()
            .HasForeignKey(p => p.TaxRateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ChartOfAccount)
            .WithMany()
            .HasForeignKey(p => p.ChartOfAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SemiFinishedProduct)
            .WithMany()
            .HasForeignKey(p => p.SemiFinishedProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Prices)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Movements)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}