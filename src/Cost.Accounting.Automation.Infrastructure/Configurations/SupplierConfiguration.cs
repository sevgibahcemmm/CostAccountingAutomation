using Cost.Accounting.Automation.Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(c => c.Id);

        // HasColumnType zorunlu: ConfigureConventions nvarchar(MAX) uygular ve
        // HasMaxLength bunu ezmez. Gerekçesi CustomerConfiguration ile aynıdır.
        builder.OwnsOne(c => c.Name, name =>
            name.Property(x => x.Value).HasColumnName("Name_Value").HasColumnType("nvarchar(200)").HasMaxLength(200));

        builder.OwnsOne(c => c.TaxOffice);
        builder.OwnsOne(c => c.TaxNumber);
        builder.OwnsOne(c => c.Contact);
        builder.OwnsOne(c => c.Address);
        builder.OwnsOne(c => c.Description);
    }
}