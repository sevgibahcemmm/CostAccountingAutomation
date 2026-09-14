using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Roles;

namespace Cost.Accounting.Automation.Application.Roles;
public sealed class RoleDto : EntityDto
{
    [Column("Rol Adı", Order = 10, Width = 220)]
    public string Name { get; set; } = default!;

    [Column("Yetki Sayısı", Order = 20, Width = 90, Alignment = "Center")]
    public int PermissionCount { get; set; }

    [Column("Yetkiler", IsVisible = false)]
    public List<string> Permissions { get; set; } = new();

    [Column("Yetki Detayları", IsVisible = false)]
    public List<RolePermissionDto> PermissionDetails { get; set; } = new();
}

public static class RoleExtensions
{
    public static IQueryable<RoleDto> MapTo(this IQueryable<EntityWithAuditDto<Role>> entites)
    {
        var res = entites.Select(s => new RoleDto
        {
            Id = s.Entity.Id,
            Name = s.Entity.Name.Value,
            PermissionCount = s.Entity.Permissions.Count,
            PermissionDetails = s.Entity.Permissions.Select(p => new RolePermissionDto { Key = p.Value }).ToList(),
            IsActive = s.Entity.IsActive,
            CreatedAt = s.Entity.CreatedAt,
            CreatedBy = s.Entity.CreatedBy,
            CreatedFullName = s.CreatedUser.FullName.Value,
            UpdatedAt = s.Entity.UpdatedAt,
            UpdatedBy = s.Entity.UpdatedBy != null ? s.Entity.UpdatedBy.Value : null,
            UpdatedFullName = s.UpdatedUser != null ? s.UpdatedUser.FullName.Value : null,
        }).AsQueryable();

        return res;
    }

    public static IQueryable<RoleDto> MapToGet(this IQueryable<EntityWithAuditDto<Role>> entites)
    {
        var res = entites.Select(s => new RoleDto
        {
            Id = s.Entity.Id,
            Name = s.Entity.Name.Value,
            PermissionCount = s.Entity.Permissions.Count,
            Permissions = s.Entity.Permissions.Select(s => s.Value).ToList(),
            PermissionDetails = s.Entity.Permissions.Select(p => new RolePermissionDto { Key = p.Value }).ToList(),
            IsActive = s.Entity.IsActive,
            CreatedAt = s.Entity.CreatedAt,
            CreatedBy = s.Entity.CreatedBy,
            CreatedFullName = s.CreatedUser.FullName.Value,
            UpdatedAt = s.Entity.UpdatedAt,
            UpdatedBy = s.Entity.UpdatedBy != null ? s.Entity.UpdatedBy.Value : null,
            UpdatedFullName = s.UpdatedUser != null ? s.UpdatedUser.FullName.Value : null,
        }).AsQueryable();

        return res;
    }
}
