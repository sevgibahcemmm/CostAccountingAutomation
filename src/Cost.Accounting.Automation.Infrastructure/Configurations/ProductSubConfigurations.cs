using Cost.Accounting.Automation.Domain.Products;
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

        builder.HasIndex("ProductId");
    }
}

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Path).HasColumnName("Path");
        builder.HasIndex("ProductId");
    }
}