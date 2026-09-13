using Cost.Accounting.Automation.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceNumber)
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Description, desc =>
            desc.Property(p => p.Value)
                .HasColumnName("Description")
                .HasMaxLength(500));

        builder.Property(x => x.SubTotal)
            .HasColumnType("money");

        builder.Property(x => x.TaxTotal)
            .HasColumnType("money");

        builder.Property(x => x.GrandTotal)
            .HasColumnType("money");

        builder.HasIndex(x => x.InvoiceNumber);
        builder.HasIndex(x => x.InvoiceType);
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.SupplierId);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}