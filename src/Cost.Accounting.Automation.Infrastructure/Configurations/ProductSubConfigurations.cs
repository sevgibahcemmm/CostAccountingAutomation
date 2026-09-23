using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class ProductUnitTypeConfiguration : IEntityTypeConfiguration<ProductUnitType>
{
    public void Configure(EntityTypeBuilder<ProductUnitType> builder)
    {
        builder.ToTable("ProductUnitTypes");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Name, name =>
            name.Property(x => x.Value).HasColumnName("Name").HasMaxLength(120));
    }
}

internal sealed class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("TaxRates");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Name, name =>
            name.Property(x => x.Value).HasColumnName("Name").HasMaxLength(120));

        builder.Property(x => x.Rate)
            .HasColumnName("Rate")
            .HasColumnType("decimal(18,4)");
    }
}

internal sealed class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.UnitPrice, price =>
            price.Property(x => x.Value).HasColumnName("UnitPrice"));

        builder.HasIndex("ProductId");
    }
}

internal sealed class ProductMovementConfiguration : IEntityTypeConfiguration<ProductMovement>
{
    public void Configure(EntityTypeBuilder<ProductMovement> builder)
    {
        builder.ToTable("ProductMovements");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.UnitPrice, price =>
            price.Property(x => x.Value).HasColumnName("UnitPrice"));

        builder.OwnsOne(x => x.Description, description =>
            description.Property(x => x.Value).HasColumnName("Description"));

        builder.Property(x => x.Reason)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValueSql("1");

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => x.StockIssueId);

        builder.HasIndex(x => new { x.Date, x.MovementType });
        builder.HasIndex(x => new { x.ProductId, x.Date });
    }
}