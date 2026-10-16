using Cost.Accounting.Automation.Domain.DirectProcurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

/// <summary>
/// Doğrudan temin dosyasının alt kayıtları (kaynak: <c>IHMALZEME</c>,
/// <c>IHGECMISALIMLAR</c>, <c>IHTEKLIFFIRMA</c>, <c>IHONAYFIRMA</c>,
/// <c>IHUYGUNFIRMA</c>, <c>IHUYGUNFIRMADETAY</c>, <c>IHYMGOREVLILERI</c>,
/// <c>IHPIYASAGOREVLILERI</c>, <c>IHKOMISYONGOREVLILERI</c>, <c>IHBUTCE</c>).
/// </summary>
/// <remarks>
/// Tüm alt kayıtlar dosyaya bağlı olduğu için silme davranışı <c>Cascade</c>'tir;
/// dosya yumuşak silindiğinde alt kayıtlara da dokunulmaz, çünkü
/// <c>Entity.Delete()</c> yalnızca <c>IsDeleted</c> bayrağını kaldırır.
/// </remarks>
internal sealed class DirectProcurementChildConfiguration :
    IEntityTypeConfiguration<DirectProcurementItem>,
    IEntityTypeConfiguration<ProcurementHistoryPurchase>,
    IEntityTypeConfiguration<ProcurementQuotedFirm>,
    IEntityTypeConfiguration<ProcurementApprovedFirm>,
    IEntityTypeConfiguration<ProcurementAwardedFirm>,
    IEntityTypeConfiguration<ProcurementAwardedFirmAccount>,
    IEntityTypeConfiguration<ProcurementTeamMember>,
    IEntityTypeConfiguration<DirectProcurementBudgetSelection>
{
    // ---------- İhtiyaç listesi satırı ----------

    public void Configure(EntityTypeBuilder<DirectProcurementItem> builder)
    {
        builder.ToTable("DirectProcurementItems");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasColumnType("nvarchar(100)").HasMaxLength(100);
        builder.Property(c => c.Definition).HasColumnType("nvarchar(500)").HasMaxLength(500);
        builder.Property(c => c.Description).HasColumnType("nvarchar(MAX)");
        builder.Property(c => c.Unit).HasColumnType("nvarchar(50)").HasMaxLength(50);

        builder.HasOne(c => c.File)
            .WithMany(f => f.Items)
            .HasForeignKey(c => c.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.DirectProcurementFileId, c.LineNo });
        builder.HasIndex(c => c.ProductId);
    }

    // ---------- Geçmiş alım ----------

    public void Configure(EntityTypeBuilder<ProcurementHistoryPurchase> builder)
    {
        builder.ToTable("ProcurementHistoryPurchases");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasColumnType("nvarchar(100)").HasMaxLength(100);
        builder.Property(c => c.Definition).HasColumnType("nvarchar(500)").HasMaxLength(500);
        builder.Property(c => c.Description).HasColumnType("nvarchar(MAX)");
        builder.Property(c => c.Unit).HasColumnType("nvarchar(50)").HasMaxLength(50);
        builder.Property(c => c.SupplierName).HasColumnType("nvarchar(200)").HasMaxLength(200);

        builder.HasOne(c => c.File)
            .WithMany(f => f.HistoryPurchases)
            .HasForeignKey(c => c.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Item)
            .WithMany()
            .HasForeignKey(c => c.DirectProcurementItemId)
            // NoAction zorunludur: ihtiyaç satırı dosyadan da CASCADE ile
            // gelir, teklif kaydı da dosyadan CASCADE ile gelir. Aynı satıra
            // iki farklı cascade yolu tanımlanırsa SQL Server bu kısıtı
            // hiç oluşturamaz ("multiple cascade paths"). NoAction, satırın
            // dosya silinirken yalnızca dosya→satır yolundan silinmesini
            // sağlar; teklif kayıtları da kendi yolundan bağımsız silinir.
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => c.DirectProcurementFileId);
        builder.HasIndex(c => c.ProductId);
    }

    // ---------- Teklif veren firma ----------

    public void Configure(EntityTypeBuilder<ProcurementQuotedFirm> builder)
    {
        builder.ToTable("ProcurementQuotedFirms");
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.File)
            .WithMany(f => f.QuotedFirms)
            .HasForeignKey(c => c.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Item)
            .WithMany()
            .HasForeignKey(c => c.DirectProcurementItemId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Domain.Suppliers.Supplier>()
            .WithMany()
            .HasForeignKey(c => c.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.DirectProcurementFileId, c.DirectProcurementItemId });
        builder.HasIndex(c => c.SupplierId);
    }

    // ---------- Onaylanan firma ----------

    public void Configure(EntityTypeBuilder<ProcurementApprovedFirm> builder)
    {
        builder.ToTable("ProcurementApprovedFirms");
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.File)
            .WithMany(f => f.ApprovedFirms)
            .HasForeignKey(c => c.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Item)
            .WithMany()
            .HasForeignKey(c => c.DirectProcurementItemId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Domain.Suppliers.Supplier>()
            .WithMany()
            .HasForeignKey(c => c.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.DirectProcurementFileId, c.DirectProcurementItemId });
    }

    // ---------- Kazanılan firma ----------

    public void Configure(EntityTypeBuilder<ProcurementAwardedFirm> builder)
    {
        builder.ToTable("ProcurementAwardedFirms");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.InvoiceNo).HasColumnType("nvarchar(50)").HasMaxLength(50);
        builder.Property(c => c.PaymentOrderNo).HasColumnType("nvarchar(50)").HasMaxLength(50);
        builder.Property(c => c.PaymentOrderNo2).HasColumnType("nvarchar(50)").HasMaxLength(50);
        builder.Property(c => c.ChequeNo).HasColumnType("nvarchar(50)").HasMaxLength(50);
        builder.Property(c => c.Note).HasColumnType("nvarchar(MAX)");
        builder.Property(c => c.Memorandum).HasColumnType("nvarchar(50)").HasMaxLength(50);
        builder.Property(c => c.Attachment).HasColumnType("nvarchar(50)").HasMaxLength(50);
        builder.Property(c => c.NegotiationNote).HasColumnType("nvarchar(MAX)");

        builder.HasOne(c => c.File)
            .WithMany(f => f.AwardedFirms)
            .HasForeignKey(c => c.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.Suppliers.Supplier>()
            .WithMany()
            .HasForeignKey(c => c.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Accounts)
            .WithOne(a => a.AwardedFirm)
            .HasForeignKey(a => a.ProcurementAwardedFirmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.DirectProcurementFileId);
        builder.HasIndex(c => c.SupplierId);
    }

    // ---------- Hesap bazlı detay ----------

    public void Configure(EntityTypeBuilder<ProcurementAwardedFirmAccount> builder)
    {
        builder.ToTable("ProcurementAwardedFirmAccounts");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.AccountName).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(c => c.AccountNo).HasColumnType("nvarchar(100)").HasMaxLength(100);

        builder.HasIndex(c => c.ProcurementAwardedFirmId);
    }

    // ---------- Görevli ----------

    public void Configure(EntityTypeBuilder<ProcurementTeamMember> builder)
    {
        builder.ToTable("ProcurementTeamMembers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PersonnelName).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(c => c.Title).HasColumnType("nvarchar(200)").HasMaxLength(200);

        builder.HasOne(c => c.File)
            .WithMany(f => f.TeamMembers)
            .HasForeignKey(c => c.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.DirectProcurementFileId, c.Role, c.Sequence });
    }

    // ---------- Bütçe seçimi ----------

    public void Configure(EntityTypeBuilder<DirectProcurementBudgetSelection> builder)
    {
        builder.ToTable("DirectProcurementBudgetSelections");
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.File)
            .WithMany(f => f.BudgetSelections)
            .HasForeignKey(c => c.DirectProcurementFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.ChartOfAccounts.ChartOfAccount>()
            .WithMany()
            .HasForeignKey(c => c.ChartOfAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.DirectProcurementFileId, c.Year });
        builder.HasIndex(c => c.ChartOfAccountId);
    }
}