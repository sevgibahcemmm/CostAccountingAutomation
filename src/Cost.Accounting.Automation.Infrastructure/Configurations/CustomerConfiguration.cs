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

        builder.OwnsOne(c => c.Name);
        builder.OwnsOne(c => c.TaxOffice);
        builder.OwnsOne(c => c.TaxNumber);
        builder.OwnsOne(c => c.Contact);
        builder.OwnsOne(c => c.Address);
        builder.OwnsOne(c => c.Description);
    }
}