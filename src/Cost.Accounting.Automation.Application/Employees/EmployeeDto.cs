using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// Personel liste satırı. Görevler liste ekranında tek satırda özet olarak
/// gösterilir; ayrıntılı görev-atölye eşleşmesi düzenleme formundadır.
/// </summary>
public sealed class EmployeeDto : EntityDto
{
    [Column("TC Kimlik No", Order = 10, Width = 150, Format = "### ### ### ##")]
    public string IdentityNumber { get; set; } = default!;

    [Column("Adı", Order = 20, Width = 140)]
    public string FirstName { get; set; } = default!;

    [Column("Soyadı", Order = 25, Width = 160)]
    public string LastName { get; set; } = default!;

    [Column("Ad Soyad", Order = 30, Width = 200)]
    public string FullName { get; set; } = default!;

    [Column("Ünvanı", Order = 35, Width = 200)]
    public string Title { get; set; } = default!;

    [Column("Görevleri", Order = 40, Width = 320)]
    public string SigningRoles { get; set; } = string.Empty;

    [Column("Atölyeler", Order = 45, Width = 240)]
    public string Workshops { get; set; } = string.Empty;

    [Column("Telefon 1", Order = 50, Width = 130)]
    public string PhoneNumber1 { get; set; } = default!;

    [Column("Telefon 2", Order = 55, Width = 130, IsVisible = false)]
    public string PhoneNumber2 { get; set; } = string.Empty;

    [Column("E-Posta", Order = 60, Width = 200)]
    public string Email { get; set; } = string.Empty;

    [Column("Fotoğraf", IsVisible = false)]
    public string? PhotoPath { get; set; }

    /// <summary>Liste satırındaki görev sayısı; düzenleme formunda sekme başlığı için kullanılır.</summary>
    public int DutyCount { get; set; }

    /// <summary>Düzenleme formunda görev tablosunu dolduran kayıtlar.</summary>
    public List<EmployeeDutyDto> Duties { get; set; } = [];
}

/// <summary>Tek bir görevlendirme satırı (görev + bağlı atölye).</summary>
public sealed class EmployeeDutyDto
{
    public Guid Id { get; set; }

    /// <summary>Yetkili görev tanımının kimliği (<c>EmployeeSigningRoles</c>).</summary>
    public Guid? SigningRoleId { get; set; }

    /// <summary>Görev başlığının okunabilir karşılığı (arayüzde seçim kutusunda görünür).</summary>
    public string SigningRoleName { get; set; } = string.Empty;

    public Guid? WorkshopId { get; set; }

    /// <summary>Atölye kodu + adı; kurum geneli görevlerde boştur.</summary>
    public string? WorkshopName { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Atölye seçimi zorunlu mu? Yalnızca atölyeye bağlı görevlerde true.</summary>
    public bool RequiresWorkshop { get; set; }
}

public static class EmployeeExtensions
{
    public static IQueryable<EmployeeDto> MapTo(this IQueryable<EntityWithAuditDto<Employee>> entity)
    {
        return entity
            .Select(e => new EmployeeDto
            {
                Id = e.Entity.Id,
                IdentityNumber = e.Entity.IdentityNumber.Value,
                FirstName = e.Entity.FirstName.Value,
                LastName = e.Entity.LastName.Value,
                FullName = e.Entity.FirstName.Value + " " + e.Entity.LastName.Value,
                Title = e.Entity.Title.Value,
                PhoneNumber1 = e.Entity.PhoneNumber1,
                PhoneNumber2 = e.Entity.PhoneNumber2,
                Email = e.Entity.Email,
                PhotoPath = e.Entity.PhotoPath,
                IsActive = e.Entity.IsActive,

                SigningRoles = string.Join(", ", e.Entity.Duties
                    .Where(d => !d.IsDeleted && d.SigningRole != null)
                    .Select(d => d.SigningRole!.Name.Value)
                    .OrderBy(n => n)),

                Workshops = string.Join(", ", e.Entity.Duties
                    .Where(d => !d.IsDeleted && d.Workshop != null)
                    .Select(d => d.Workshop!.Name.Value)
                    .OrderBy(n => n)),

                DutyCount = e.Entity.Duties.Count(d => !d.IsDeleted),

                CreatedAt = e.Entity.CreatedAt,
                CreatedBy = e.Entity.CreatedBy,
                UpdatedAt = e.Entity.UpdatedAt,
                UpdatedBy = e.Entity.UpdatedBy == null ? null : e.Entity.UpdatedBy.Value,
                CreatedFullName = e.CreatedUser.FullName.Value,
                UpdatedFullName = e.UpdatedUser == null ? null : e.UpdatedUser.FullName.Value,

                // Alt grid (master-detail) bu koleksiyondan besleniyor; önceden
                // dolmazsa satır genişletildiğinde görev listesi boş görünür.
                // Projeksiyon aynı zamanda SigningRoles/Workshops özet
                // metinlerini de üretiyor; ikisi aynı Include zincirini paylaşır.
                Duties = e.Entity.Duties
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.SigningRole!.SortOrder)
                    .ThenBy(d => d.SigningRole!.Name.Value)
                    .Select(d => new EmployeeDutyDto
                    {
                        Id = d.Id.Value,
                        SigningRoleId = d.SigningRoleId != null ? d.SigningRoleId.Value : (Guid?)null,
                        SigningRoleName = d.SigningRole != null
                            ? d.SigningRole.Name.Value
                            : "(tanımsız görev)",
                        WorkshopId = d.WorkshopId != null ? d.WorkshopId.Value : (Guid?)null,
                        WorkshopName = d.Workshop != null ? d.Workshop.Name.Value : null,
                        IsActive = d.IsActive,
                        RequiresWorkshop = d.SigningRole != null && d.SigningRole.RequiresWorkshop
                    })
                    .ToList()
            })
            .AsQueryable();
    }

    /// <summary>
    /// Zaten belleğe alınmış bir personel kaydını listeye uygun satıra çevirir.
    /// Liste sorgusu <see cref="MapTo"/> ile SQL üzerinde projeksiyon yapar;
    /// tekil kayıt okuma yolları bu metotu kullanır.
    ///
    /// <para>
    /// Ad/soyad/ünvan ve görev adı owned navigasyonlardan okunur. Bu
    /// nesneler veritabanından <c>Include</c> zincirleriyle birlikte
    /// materyalize edilir; bir zincir eksik kalırsa alan null olabilir.
    /// Dönüşüm ekran açmaya çalarken çökmemeli, bu yüzden okuma null
    /// güvenlidir ve düşen alan "(bilinmiyor)" olarak işaretlenir.
    /// </para>
    /// </summary>
    public static EmployeeDto ToDto(this Employee entity) => new()
    {
        Id = entity.Id,
        IdentityNumber = entity.IdentityNumber?.Value ?? string.Empty,
        FirstName = entity.FirstName?.Value ?? string.Empty,
        LastName = entity.LastName?.Value ?? string.Empty,
        FullName = entity.FullName,
        Title = entity.Title?.Value ?? string.Empty,
        PhoneNumber1 = entity.PhoneNumber1,
        PhoneNumber2 = entity.PhoneNumber2,
        Email = entity.Email,
        PhotoPath = entity.PhotoPath,
        IsActive = entity.IsActive,

        SigningRoles = string.Join(", ", entity.Duties
            .Where(d => !d.IsDeleted && d.SigningRole != null)
            .Select(d => d.SigningRole!.Name?.Value ?? string.Empty)
            .OrderBy(n => n)),

        Workshops = string.Join(", ", entity.Duties
            .Where(d => !d.IsDeleted && d.Workshop != null)
            .Select(d => d.Workshop!.Name?.Value ?? string.Empty)
            .OrderBy(n => n)),

        DutyCount = entity.Duties.Count(d => !d.IsDeleted),

        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy is IdentityId updatedBy ? updatedBy.Value : null,
        CreatedFullName = string.Empty,
        Duties = entity.MapDuties()
    };

    /// <summary>
    /// Ayrıntılı görev listesi; liste sorgusunun <c>IQueryable</c> modeline
    /// taşınmayan kısmı burada materyalize edilerek doldurulur.
    ///
    /// <para>
    /// <c>SigningRoleId</c> değer dönüştürücülü bir referans tip olduğu için
    /// kolon boşsa EF null üretebilir; bu yüzden <c>?.</c> ile okunur. Görev
    /// tanımı yüklenmemişse satır yine de listelenir, adı "(tanımsız görev)"
    /// olur ve kaydetme aşamasında doğrulama reddeder.
    /// </para>
    /// </summary>
    public static List<EmployeeDutyDto> MapDuties(this Employee employee)
        => [.. employee.Duties
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.SigningRole?.SortOrder ?? int.MaxValue)
            .ThenBy(d => d.SigningRole?.Name?.Value ?? string.Empty)
            .Select(d => new EmployeeDutyDto
            {
                Id = d.Id,
                SigningRoleId = d.SigningRoleId?.Value ?? Guid.Empty,
                SigningRoleName = d.SigningRole?.Name?.Value ?? "(tanımsız görev)",
                WorkshopId = d.WorkshopId is IdentityId workshop ? workshop.Value : null,
                WorkshopName = d.Workshop?.Name?.Value,
                IsActive = d.IsActive,
                RequiresWorkshop = d.SigningRole?.RequiresWorkshop ?? false
            })];
}