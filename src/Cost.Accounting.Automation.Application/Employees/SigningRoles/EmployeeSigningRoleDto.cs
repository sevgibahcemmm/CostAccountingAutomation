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
    public static IQueryable<EmployeeSigningRoleDto> MapTo(this IQueryable<EmployeeSigningRole> entity)
    {
        return entity
            .Select(e => new EmployeeSigningRoleDto
            {
                Id = e.Id,
                Name = e.Name.Value,
                Description = e.Description,
                RequiresWorkshop = e.RequiresWorkshop,
                SortOrder = e.SortOrder,

                CreatedAt = e.CreatedAt,
                CreatedBy = e.CreatedBy,
                IsActive = e.IsActive,
                UpdatedAt = e.UpdatedAt,
                UpdatedBy = e.UpdatedBy == null ? null : e.UpdatedBy.Value,
            });
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
