using Cost.Accounting.Automation.Domain.Employees;
using Cost.Accounting.Automation.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;

/// <summary>
/// Personel kaydı yıl veritabanında tutulur: görevlendirmeler atölyelere
/// (<see cref="Domain.ChartOfAccounts.ChartOfAccount"/>) bağlıdır ve atölyeler
/// yıl veritabanındadır.
/// </summary>
internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.OwnsOne(e => e.FirstName, first =>
            first.Property(x => x.Value).HasColumnName("FirstName").HasMaxLength(100).IsRequired());

        builder.OwnsOne(e => e.LastName, last =>
            last.Property(x => x.Value).HasColumnName("LastName").HasMaxLength(100).IsRequired());

        // TC kimlik no benzersizdir: aynı kişi iki kez kaydedilemez.
        // Value converter yerine OwnsOne kullanılmadı çünkü benzersiz indeks
        // üzerinde sorgulanabilir bir sütun gerekiyor.
        builder.Property(e => e.IdentityNumber)
            .HasConversion(
                v => v.Value,
                v => new TRIdentityNumber(v))
            .HasColumnName("IdentityNumber")
            .HasMaxLength(11)
            .IsRequired();

        builder.OwnsOne(e => e.Title, title =>
            title.Property(x => x.Value).HasColumnName("Title").HasMaxLength(200).IsRequired());

        builder.Property(e => e.PhoneNumber1).HasMaxLength(50).IsRequired();
        builder.Property(e => e.PhoneNumber2).HasMaxLength(50);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.PhotoPath).HasMaxLength(500);

        builder.HasIndex(e => e.IdentityNumber).IsUnique();
        builder.HasIndex(e => e.CreatedBy);
        builder.HasIndex(e => e.UpdatedBy);

        builder.HasMany(e => e.Duties)
            .WithOne(d => d.Employee)
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Duties).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// Yetkili görev tanımı. Görevler enum değil tablo olduğu için kullanıcı
/// yeni bir imza görevi ekleyebilir; kayıtlar
/// <see cref="YearDatabaseProvisioner"/> tarafından tohumlanır.
/// </summary>
internal sealed class EmployeeSigningRoleConfiguration : IEntityTypeConfiguration<EmployeeSigningRole>
{
    public void Configure(EntityTypeBuilder<EmployeeSigningRole> builder)
    {
        builder.ToTable("EmployeeSigningRoles");

        builder.HasKey(r => r.Id);

        builder.OwnsOne(r => r.Name, name =>
            name.Property(x => x.Value).HasColumnName("Name").HasMaxLength(120).IsRequired());

        builder.Property(r => r.Description)
            .HasColumnName("Description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.RequiresWorkshop)
            .HasColumnName("RequiresWorkshop")
            .IsRequired();

        builder.Property(r => r.SortOrder)
            .HasColumnName("SortOrder")
            .IsRequired();
    }
}

/// <summary>
/// Görevlendirme kaydı. Aynı personelin aynı görevi tekrar tanımlanmasını
/// benzersiz indeks ile engeller.
/// </summary>
internal sealed class EmployeeDutyConfiguration : IEntityTypeConfiguration<EmployeeDuty>
{
    public void Configure(EntityTypeBuilder<EmployeeDuty> builder)
    {
        builder.ToTable("EmployeeDuties");

        builder.HasKey(d => d.Id);

        // EmployeeId ve SigningRoleId / WorkshopId için EF Core foreign key
        // başlığı üzerinde zaten indeks oluşturur; ayrıca HasIndex vermek aynı
        // alanlarda ikinci bir indeks denemesine yol açar.

        // DuplicateKey üzerine benzersiz indeks BURADA verilemez:
        // AuditedDbContext.ApplySharedModelConfiguration her entity için
        // DuplicateKey alanına zaten (benzersiz olmayan) bir indeks ekliyor.
        // Aynı görevin iki kez tanımlanması komut işleyicisinde engellenir.
        builder.HasOne(d => d.SigningRole)
            .WithMany()
            .HasForeignKey(d => d.SigningRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Workshop)
            .WithMany()
            .HasForeignKey(d => d.WorkshopId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
