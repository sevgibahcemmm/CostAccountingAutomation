
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

        // DİKKAT: HasMaxLength tek başına yetmez.
        //
        // AuditedDbContext.ConfigureConventions tüm string sütunlara açıkça
        // "nvarchar(MAX)" uygular. Bu bir kolon TİPİ atamasıdır ve
        // HasMaxLength tarafından ezilmez; sonuç, sütun yine de nvarchar(MAX)
        // olarak kalır. nvarchar(MAX) kolonlar SQL Server'da indekslenemez,
        // bu yüzden indeks tanımlanmadan önce tipi sabitlemek gerekir.
        //
        // SourceType yalnızca birkaç sabit etiketten biridir (StokGirisi,
        // SatisFaturasi, ...); mevcut kayıtlarda en uzun değer 17 karakter.
        // 50 karakter seçilmiştir.
        builder.Property(x => x.SourceType)
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.ChartOfAccountId);

        // Yevmiye satırının kaynak belgesine (stok hareketi, fatura satırı)
        // erişim yolu. Onarım ve senkronizasyon akışları bu ikili üzerinden
        // sorgu yapar; indeksli olmadığında her arama tam tablo taramasıdır.
        builder.HasIndex(x => new { x.SourceType, x.SourceId });

        builder.HasOne<ChartOfAccount>()
            .WithMany()
            .HasForeignKey(x => x.ChartOfAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}