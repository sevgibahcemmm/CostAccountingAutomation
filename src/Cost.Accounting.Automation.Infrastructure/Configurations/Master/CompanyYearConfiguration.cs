using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cost.Accounting.Automation.Infrastructure.Configurations.Master;

/// <summary>
/// Yalnızca master veritabanında geçerli olan entity konfigürasyonları.
/// <c>ApplicationDbContext</c> bu namespace'i tarama dışı bırakır; aksi halde
/// master entity'leri her yıl veritabanına da sızar.
/// </summary>
internal sealed class CompanyYearConfiguration : IEntityTypeConfiguration<CompanyYear>
{
    public void Configure(EntityTypeBuilder<CompanyYear> builder)
    {
        builder.ToTable("CompanyYears");

        builder.HasKey(cy => cy.Id);

        // ==========================
        // Value Objects (tek alanlı scalar)
        // ==========================
        // OwnsOne yerine value converter kullanılır: Owned navigation üzerine
        // composite unique index kurulamıyor ve bu iki değer zaten tek sütun.

        builder.Property(cy => cy.Year)
            .HasConversion<YearValueConverter>();

        // Unique index'e gireceği için nvarchar(MAX) olamaz.
        builder.Property(cy => cy.DatabaseName)
            .HasConversion<DatabaseNameValueConverter>()
            .HasMaxLength(DatabaseName.MaxLength)
            .HasColumnType($"nvarchar({DatabaseName.MaxLength})");

        // ==========================
        // Scalars
        // ==========================

        builder.Property(cy => cy.IsClosed);
        builder.Property(cy => cy.OpeningDate);
        builder.Property(cy => cy.ClosedAt);

        // ==========================
        // Indexes
        // ==========================

        // Aynı şirket için aynı mali yıl iki kez açılamaz.
        builder.HasIndex(cy => new { cy.CompanyId, cy.Year }).IsUnique();

        // Veritabanı adı tekildir; aynı ada ikinci bir yıl kaydı açılamaz.
        builder.HasIndex(cy => cy.DatabaseName).IsUnique();
    }
}

internal sealed class YearValueConverter : ValueConverter<Year, int>
{
    public YearValueConverter() : base(m => m.Value, m => new Year(m)) { }
}

internal sealed class DatabaseNameValueConverter : ValueConverter<DatabaseName, string>
{
    public DatabaseNameValueConverter() : base(m => m.Value, m => new DatabaseName(m)) { }
}
