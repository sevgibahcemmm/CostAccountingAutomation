
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class ChartOfAccountLedgerEntryConfiguration : IEntityTypeConfiguration<ChartOfAccountLedger>
{
    public void Configure(EntityTypeBuilder<ChartOfAccountLedger> builder)
    {
        builder.ToTable("ChartOfAccountLedger");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DebitAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CreditAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SourceType).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.ChartOfAccountId);

        builder.HasOne<ChartOfAccount>()
            .WithMany()
            .HasForeignKey(x => x.ChartOfAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}