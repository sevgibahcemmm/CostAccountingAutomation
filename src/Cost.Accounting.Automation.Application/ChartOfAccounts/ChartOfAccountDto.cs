using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

public sealed class ChartOfAccountDto : EntityDto
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public Guid? ParentId { get; set; }
    public Guid? SemiFinishedAccountId { get; set; }
    public Guid? FinishedAccountId { get; set; }
    public int Level { get; set; }
    public ChartOfAccountType Type { get; set; }

    public string? SemiFinishedCode { get; set; }
    public string? FinishedCode { get; set; }

    public string TypeText => Type switch
    {
        ChartOfAccountType.Warehouse => "Depo",
        ChartOfAccountType.Category => "Kategori",
        ChartOfAccountType.Workshop => "Atölye",
        _ => "Anagrup"
    };
}

public static class ChartOfAccountExtensions
{
    public static IQueryable<ChartOfAccountDto> MapTo(this IQueryable<EntityWithAuditDto<ChartOfAccount>> entity)
    {
        return entity
            .Select(s => new ChartOfAccountDto
            {
                Id = s.Entity.Id,
                Code = s.Entity.Code.Value,
                Name = s.Entity.Name.Value,
                ParentId = s.Entity.ParentId == null ? null : s.Entity.ParentId.Value,
                SemiFinishedAccountId = s.Entity.SemiFinishedAccountId == null ? null : s.Entity.SemiFinishedAccountId.Value,
                FinishedAccountId = s.Entity.FinishedAccountId == null ? null : s.Entity.FinishedAccountId.Value,
                Level = s.Entity.Level,
                Type = s.Entity.Type,

                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy,
                IsActive = s.Entity.IsActive,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy == null ? null : s.Entity.UpdatedBy.Value,
                CreatedFullName = s.CreatedUser.FullName.Value,
                UpdatedFullName = s.UpdatedUser == null ? null : s.UpdatedUser.FullName.Value
            })
            .AsQueryable();
    }
}