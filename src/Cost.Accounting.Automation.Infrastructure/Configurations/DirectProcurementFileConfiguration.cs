using Cost.Accounting.Automation.Domain.DirectProcurement;
using Cost.Accounting.Automation.Domain.DirectProcurement.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

/// <summary>
/// Doğrudan temin dosyasının (kaynak: <c>IHALE</c>) EF yapılandırması.
/// </summary>
/// <remarks>
/// <para>
/// <b>CompanyId yabancı anahtar değildir.</b> <c>Company</c> merkezî (Master)
/// veritabanında, bu kayıtlar ise yıl veritabanında tutulur. Bu yüzden gezinti
/// özelliği tanımlanamaz ve sütunda <c>NO CONSTRAINT</c> bırakılır.
/// </para>
/// <para>
/// Metin alanlarında <c>HasColumnType</c> zorunludur: <c>ConfigureConventions</c>
/// tüm string'leri <c>nvarchar(MAX)</c> yapar ve <c>HasMaxLength</c> bunu ezmez.
/// Arama ve sıralama yapılan alanlar bu yüzden açıkça daraltılmıştır.
/// </para>
/// </remarks>
internal sealed class DirectProcurementFileConfiguration : IEntityTypeConfiguration<DirectProcurementFile>
{
    // Zorunlu alanlar için dönüştürücü; boş değer tutulmaz.
    private static readonly ValueConverter<FileNumber, string> FileNumberConverter =
        new(f => f.Value, v => new FileNumber(v));

    private static readonly ValueConverter<WorkName, string> WorkNameConverter =
        new(w => w.Value, v => new WorkName(v));

    // Aşağıdakiler isteğe bağlı alanlardır: null hem "girilmemiş" hem de
    // "bilinmiyor" anlamına geldiği için korunmalıdır.
    private static readonly ValueConverter<ProjectNumber?, string?> ProjectNumberConverter =
        new(v => v == null ? null : v.Value, v => v == null ? null : new ProjectNumber(v));

    private static readonly ValueConverter<DecisionNumber?, string?> DecisionNumberConverter =
        new(v => v == null ? null : v.Value, v => v == null ? null : new DecisionNumber(v));

    private static readonly ValueConverter<WorkNature?, string?> WorkNatureConverter =
        new(v => v == null ? null : v.Value, v => v == null ? null : new WorkNature(v));

    private static readonly ValueConverter<WorkQuantity?, string?> WorkQuantityConverter =
        new(v => v == null ? null : v.Value, v => v == null ? null : new WorkQuantity(v));

    public void Configure(EntityTypeBuilder<DirectProcurementFile> builder)
    {
        builder.ToTable("DirectProcurementFiles");

        builder.HasKey(c => c.Id);

        // Tek alanlı değer nesneleri sütuna dönüştürülür (ayrı tablo açılmaz).
        // Aşırı yüklemeler belirsiz kaldığı için dönüştürücüler açıkça yazıldı:
        // lambda çifti tek başına referans tipli alanlarda "depolama tipi"
        // parametresi sanılabiliyor.
        builder.Property(c => c.FileNumber)
            .HasConversion(FileNumberConverter)
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50);

        builder.Property(c => c.WorkName)
            .HasConversion(WorkNameConverter)
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500);

        builder.Property(c => c.ProjectNumber)
            .HasConversion(ProjectNumberConverter)
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50);

        builder.Property(c => c.DecisionNumber)
            .HasConversion(DecisionNumberConverter)
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50);

        builder.Property(c => c.WorkNature)
            .HasConversion(WorkNatureConverter)
            .HasColumnType("nvarchar(MAX)");

        builder.Property(c => c.WorkQuantity)
            .HasConversion(WorkQuantityConverter)
            .HasColumnType("nvarchar(MAX)");

        builder.Property(c => c.PurchasingUnit).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(c => c.RelatedPerson).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(c => c.ApprovalOffice).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(c => c.ApprovalNumber).HasColumnType("nvarchar(50)").HasMaxLength(50);
        builder.Property(c => c.ApprovingPersonnel).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(c => c.ApprovingPersonnelTitle).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(c => c.ConformityPersonnel).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(c => c.ConformityPersonnelTitle).HasColumnType("nvarchar(200)").HasMaxLength(200);

        // ---- Gezintiler ----

        builder.HasOne(c => c.Supplier)
            .WithMany()
            .HasForeignKey(c => c.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ChartOfAccount)
            .WithMany()
            .HasForeignKey(c => c.ChartOfAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Alt koleksiyonlar ----

        builder.HasMany(c => c.Items)
            .WithOne(i => i.File)
            .HasForeignKey(i => i.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.HistoryPurchases)
            .WithOne(p => p.File)
            .HasForeignKey(p => p.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.QuotedFirms)
            .WithOne(q => q.File)
            .HasForeignKey(q => q.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ApprovedFirms)
            .WithOne(q => q.File)
            .HasForeignKey(q => q.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.AwardedFirms)
            .WithOne(q => q.File)
            .HasForeignKey(q => q.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.TeamMembers)
            .WithOne(m => m.File)
            .HasForeignKey(m => m.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.BudgetSelections)
            .WithOne(s => s.File)
            .HasForeignKey(s => s.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---- Dizinler ----

        // Liste ekranının varsayılan sıralaması ve en sık kullanılan süzgeci.
        builder.HasIndex(c => new { c.WorkingYear, c.Stage });
        builder.HasIndex(c => c.Date);
        builder.HasIndex(c => c.SupplierId);
        builder.HasIndex(c => c.CompanyId);
        builder.HasIndex(c => c.ChartOfAccountId);
    }
}