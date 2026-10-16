using Cost.Accounting.Automation.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        // HasColumnType zorunlu: ConfigureConventions nvarchar(MAX) uygular ve
        // HasMaxLength bunu ezmez. nvarchar(MAX) indekslenemez ve ad üzerinde
        // arama/sıralama yapılan bir liste ekranı vardır. Mevcut kayıtlarda
        // en uzun ad 24 karakter; 200 seçilmiştir.
        builder.OwnsOne(c => c.Name, name =>
            name.Property(x => x.Value).HasColumnName("Name_Value").HasColumnType("nvarchar(200)").HasMaxLength(200));

        builder.OwnsOne(c => c.TaxOffice);
        builder.OwnsOne(c => c.TaxNumber);
        builder.OwnsOne(c => c.Contact);
        builder.OwnsOne(c => c.Address);
        builder.OwnsOne(c => c.Description);
    }
}