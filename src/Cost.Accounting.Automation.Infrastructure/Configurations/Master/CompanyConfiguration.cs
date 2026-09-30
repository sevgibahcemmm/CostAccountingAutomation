using Cost.Accounting.Automation.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations.Master;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.Name);
        builder.OwnsOne(c => c.TaxOffice);
        builder.OwnsOne(c => c.TaxNumber);
        builder.OwnsOne(c => c.Description);
        builder.OwnsOne(c => c.Invoiceinformation);
        builder.OwnsOne(c => c.Letterhead);
        builder.OwnsOne(c => c.CompanyPrefix);
        builder.OwnsOne(c => c.Address);
        builder.OwnsOne(c => c.Contact);

        builder.OwnsOne(c => c.ExpenditureUnit);
        builder.OwnsOne(c => c.AccountingUnit);

        builder
            .HasMany(c => c.Users)
            .WithOne()
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}