using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

internal sealed class ChartOfAccountConfiguration : IEntityTypeConfiguration<ChartOfAccount>
{
    public void Configure(EntityTypeBuilder<ChartOfAccount> builder)
    {
        builder.ToTable("ChartOfAccounts");

        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.Name, name =>
        {
            name.Property(x => x.Value)
                .HasColumnName("Name")
                // HasColumnType zorunlu: ConfigureConventions tüm string
                // sütunlara nvarchar(MAX) uygular ve bunu HasMaxLength ezmez.
                // Hesap planı binlerce satır olduğundan sıralama ve arama
                // sütunu üzerinde çalışır; nvarchar(MAX) indekslenemez.
                // Mevcut kayıtlarda en uzun ad 109 karakter; 300 seçilmiştir.
                .HasColumnType("nvarchar(300)")
                .HasMaxLength(300);
        });

        builder.Property(c => c.Code)
            .HasConversion(
                v => v.Value,
                v => new AccountCode(v))
            .HasColumnName("Code")
            .HasMaxLength(60);

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.ParentId);
        builder.HasIndex(c => c.SemiFinishedAccountId);
        builder.HasIndex(c => c.FinishedAccountId);

        builder.HasOne<ChartOfAccount>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChartOfAccount>()
            .WithMany()
            .HasForeignKey(c => c.SemiFinishedAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChartOfAccount>()
            .WithMany()
            .HasForeignKey(c => c.FinishedAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}