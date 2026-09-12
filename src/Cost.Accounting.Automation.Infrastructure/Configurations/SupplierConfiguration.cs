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

        builder.OwnsOne(c => c.Name);
        builder.OwnsOne(c => c.TaxOffice);
        builder.OwnsOne(c => c.TaxNumber);
        builder.OwnsOne(c => c.Contact);
        builder.OwnsOne(c => c.Address);
        builder.OwnsOne(c => c.Description);
    }
}