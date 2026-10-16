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
            name.Property(x => x.Value).HasColumnName("Name").HasColumnType("nvarchar(120)").HasMaxLength(120));
    }
}

internal sealed class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("TaxRates");

        builder.HasKey(x => x.Id);

        // HasColumnType açıklaması için ProductMovementConfiguration'a bakın:
        // ConfigureConventions'ın nvarchar(MAX) tipi HasMaxLength'i ezdiği için
        // tip ayrıca sabitlenir.
        builder.OwnsOne(x => x.Name, name =>
            name.Property(x => x.Value).HasColumnName("Name").HasColumnType("nvarchar(120)").HasMaxLength(120));

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

        // Referans numarası (pusula/stok belgesi numarası) hareketi belgelenmiş
        // satıra bağlar; onay ve yevmiye akışları bu sütunda sorgular.
        //
        // DİKKAT: ConfigureConventions tüm string sütunlara nvarchar(MAX)
        // uyguladığı için HasMaxLength tek başına sütunu daraltmaz; tip de
        // açıkça sabitlenmelidir. nvarchar(MAX) indekslenemez. Mevcut
        // kayıtlarda en uzun değer 14 karakter; 100 karakter seçilmiştir.
        builder.Property(x => x.ReferenceNo)
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100);

        builder.HasIndex(x => x.ReferenceNo);

        builder.HasIndex(x => new { x.Date, x.MovementType });
        builder.HasIndex(x => new { x.ProductId, x.Date });
    }
}