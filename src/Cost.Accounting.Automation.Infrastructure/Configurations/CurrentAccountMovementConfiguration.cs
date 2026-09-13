using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class CurrentAccountMovementConfiguration : IEntityTypeConfiguration<CurrentAccountMovement>
{
    public void Configure(EntityTypeBuilder<CurrentAccountMovement> builder)
    {
        builder.ToTable("CurrentAccountMovements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentNo)
            .HasMaxLength(100);

        builder.Property(x => x.Debit)
            .HasColumnType("money");

        builder.Property(x => x.Credit)
            .HasColumnType("money");

        builder.OwnsOne(x => x.Description, desc =>
            desc.Property(p => p.Value).HasColumnName("Description"));

        builder.HasIndex(x => x.CurrentAccountType);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.SupplierId);
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.MovementType);
        builder.HasIndex(x => x.InvoiceId);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
