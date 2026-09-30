using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Application.Users;
public sealed class UserDto : EntityDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    [Column("Ad Soyad", Width = 160, Order = 10)]
    public string FullName { get; set; } = default!;

    [Column("Kullanıcı Adı", Width = 130, Order = 20)]
    public string UserName { get; set; } = default!;

    [Column("TC Kimlik No", Width = 150, Order = 30, Alignment = "Center", Format = "###-####-####")]
    public string? TRIdentityNumber { get; set; }

    [Column("E-Posta", Width = 180, Order = 40)]
    public string Email { get; set; } = default!;

    [Column("Şirket", Width = 150, Order = 50)]
    public string CompanyName { get; set; } = default!;

    [Column("Rol", Width = 130, Order = 60)]
    public string RoleName { get; set; } = default!;

    [Column("Durum", Width = 75, Order = 70, TrueText = "Aktif", FalseText = "Pasif")]
    public new bool IsActive { get; set; }

    [Column("Kayıt Tarihi", Width = 120, Order = 80, Alignment = "Center", Format = "dd MMM yyyy")]
    public new DateTimeOffset CreatedAt { get; set; }

    public Guid CompanyId { get; set; }
    public Guid RoleId { get; set; }
    public int PhotoCount { get; set; }
    public string? DefaultPhotoPath { get; set; }
}

public static class UserExtensions
{
    public static IQueryable<UserDto> MapTo(
        this IQueryable<EntityWithAuditDto<User>> entities,
        IQueryable<Role> roles,
        IQueryable<Company> Companyes
        )
    {
        var res = entities
            .Join(roles, m => m.Entity.RoleId, m => m.Id, (e, role)
                => new { e.Entity, e.CreatedUser, e.UpdatedUser, Role = role })
            .Join(Companyes, m => m.Entity.CompanyId, m => m.Id, (entity, Company)
                => new { entity.Entity, entity.CreatedUser, entity.UpdatedUser, entity.Role, Company = Company })
            .Select(s => new UserDto
            {
                Id = s.Entity.Id,
                FirstName = s.Entity.FirstName.Value,
                LastName = s.Entity.LastName.Value,
                FullName = s.Entity.FullName.Value,
                Email = s.Entity.Email.Value,
                UserName = s.Entity.UserName.Value,
                RoleId = s.Entity.RoleId,
                RoleName = s.Role.Name.Value,
                CompanyId = s.Entity.CompanyId,
                CompanyName = s.Company.Name.Value,
                IsActive = s.Entity.IsActive,
                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy.Value,
                CreatedFullName = s.CreatedUser.FullName.Value,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy != null ? s.Entity.UpdatedBy.Value : null,
                UpdatedFullName = s.UpdatedUser != null ? s.UpdatedUser.FullName.Value : null,
                TRIdentityNumber = s.Entity.TRIdentityNumber != null ? s.Entity.TRIdentityNumber.Value : null,
                PhotoCount = s.Entity.AvatarPath == null ? 0 : 1,
                DefaultPhotoPath = s.Entity.AvatarPath,
            });

        return res;
    }
}