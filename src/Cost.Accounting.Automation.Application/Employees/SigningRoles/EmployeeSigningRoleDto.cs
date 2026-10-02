using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees;

namespace Cost.Accounting.Automation.Application.Employees.SigningRoles;

/// <summary>Yetkili görev tanımı liste satırı.</summary>
public sealed class EmployeeSigningRoleDto : EntityDto
{
    [Column("Görev", Order = 10, Width = 260)]
    public string Name { get; set; } = default!;

    [Column("Açıklama", Order = 20, Width = 420)]
    public string Description { get; set; } = string.Empty;

    [Column("Atölye Zorunlu", Order = 30, Width = 130, Alignment = "Center")]
    public bool RequiresWorkshop { get; set; }

    [Column("Sıra", Order = 40, Width = 80, Alignment = "Center")]
    public int SortOrder { get; set; }
}

public static class EmployeeSigningRoleExtensions
{
    public static IQueryable<EmployeeSigningRoleDto> MapTo(
        this IQueryable<EntityWithAuditDto<EmployeeSigningRole>> entity)
    {
        return entity
            .Select(e => new EmployeeSigningRoleDto
            {
                Id = e.Entity.Id,
                Name = e.Entity.Name.Value,
                Description = e.Entity.Description,
                RequiresWorkshop = e.Entity.RequiresWorkshop,
                SortOrder = e.Entity.SortOrder,

                CreatedAt = e.Entity.CreatedAt,
                CreatedBy = e.Entity.CreatedBy,
                IsActive = e.Entity.IsActive,
                UpdatedAt = e.Entity.UpdatedAt,
                UpdatedBy = e.Entity.UpdatedBy == null ? null : e.Entity.UpdatedBy.Value,
                CreatedFullName = e.CreatedUser.FullName.Value,
                UpdatedFullName = e.UpdatedUser == null ? null : e.UpdatedUser.FullName.Value
            })
            .AsQueryable();
    }
}

/// <summary>Görev seçim kutusu satırı.</summary>
public sealed class EmployeeSigningRoleOption
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string Description { get; set; } = string.Empty;

    public bool RequiresWorkshop { get; set; }

    public int SortOrder { get; set; }
}
