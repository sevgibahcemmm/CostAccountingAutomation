using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

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

    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }

    public decimal DebitBalance => DebitAmount > CreditAmount ? DebitAmount - CreditAmount : 0;
    public decimal CreditBalance => CreditAmount > DebitAmount ? CreditAmount - DebitAmount : 0;

    public string TypeText => Type switch
    {
        ChartOfAccountType.Warehouse => "Depo",
        ChartOfAccountType.Category => "Kategori",
        ChartOfAccountType.Workshop => "Atölye",
        ChartOfAccountType.Stok => "Stok",
        ChartOfAccountType.ConsumptionUnit => "Tüketim Birimi",
        _ => "Anagrup"
    };
}

public static class ChartOfAccountExtensions
{
    /// <summary>
    /// Hesap planını veritabanında projekte eder. Hesap sayısı binlerce olduğu
    /// için filtreleme ve alan seçimi SQL'de yapılmalıdır; belleğe alınan bir
    /// liste üzerinde çalışmak ekran açılış maliyetini üstel olarak artırır.
    /// </summary>
    public static IQueryable<ChartOfAccountDto> MapTo(this IQueryable<ChartOfAccount> entity)
    {
        return entity
            .Select(s => new ChartOfAccountDto
            {
                Id = s.Id,
                Code = s.Code.Value,
                Name = s.Name.Value,
                ParentId = s.ParentId == null ? null : s.ParentId.Value,
                SemiFinishedAccountId = s.SemiFinishedAccountId == null ? null : s.SemiFinishedAccountId.Value,
                FinishedAccountId = s.FinishedAccountId == null ? null : s.FinishedAccountId.Value,
                Level = s.Level,
                Type = s.Type,

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value
            });
    }
}
