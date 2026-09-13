using Cost.Accounting.Automation.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.UnitPrice)
            .HasColumnType("money");

        builder.Property(x => x.TaxRateRate)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.TaxAmount)
            .HasColumnType("money");

        builder.Property(x => x.TotalAmount)
            .HasColumnType("money");

        builder.OwnsOne(x => x.Description, desc =>
            desc.Property(p => p.Value).HasColumnName("Description"));

        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => x.ProductId);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
