using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;

namespace Cost.Accounting.Automation.Application.StockIssues;

public sealed class StockIssueLineDto
{
    [Column("Id", IsVisible = false)]
    public Guid Id { get; set; }

    [Column("Ürün Id", IsVisible = false)]
    public Guid ProductId { get; set; }

    [Column("Ürün Kodu", Order = 10, Width = 110, Alignment = "Right")]
    public string ProductCode { get; set; } = default!;

    [Column("Ürün Adı", Order = 20, Width = 220)]
    public string ProductName { get; set; } = default!;

    [Column("Birim", Order = 30, Width = 70, Alignment = "Center")]
    public string UnitTypeName { get; set; } = default!;

    [Column("Miktar", Order = 40, Width = 90, Format = "n2", Alignment = "Right")]
    public decimal Quantity { get; set; }

    [Column("Birim Maliyet", Order = 50, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal UnitCost { get; set; }

    [Column("Toplam Tutar", Order = 60, Width = 120, Format = "n2", Alignment = "Right")]
    public decimal TotalAmount { get; set; }

    [Column("Açıklama", Order = 70, Width = 200)]
    public string Description { get; set; } = default!;
}

public sealed class StockIssueListDto : EntityDto
{
    [Column("Belge No", Order = 10, Width = 140)]
    public string DocumentNumber { get; set; } = default!;

    [Column("Tarih", Order = 20, Width = 100, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly Date { get; set; }

    [Column("Kaynak Depo", Order = 30, Width = 160)]
    public string SourceWarehouseName { get; set; } = default!;

    [Column("Hedef Hesap Kodu", Order = 40, Width = 120, Alignment = "Right")]
    public string TargetAccountCode { get; set; } = default!;

    [Column("Hedef Hesap", Order = 50, Width = 220)]
    public string TargetAccountName { get; set; } = default!;

    [Column("Kalem", Order = 60, Width = 70, Alignment = "Center")]
    public int LineCount { get; set; }

    [Column("Toplam Tutar", Order = 70, Width = 130, Format = "n2", Alignment = "Right")]
    public decimal TotalAmount { get; set; }

    [Column("Açıklama", Order = 80, Width = 220)]
    public string Description { get; set; } = default!;

    [Column("Belge Türü", IsVisible = false)]
    public StockIssueType IssueType { get; set; }

    [Column("Depo Id", IsVisible = false)]
    public Guid SourceWarehouseId { get; set; }

    [Column("Hesap Id", IsVisible = false)]
    public Guid TargetAccountId { get; set; }
}

public sealed class StockIssueDto : EntityDto
{
    public string DocumentNumber { get; set; } = default!;
    public DateOnly Date { get; set; }
    public StockIssueType IssueType { get; set; }
    public StockCostingMethod CostingMethod { get; set; }
    public Guid SourceWarehouseId { get; set; }
    public string SourceWarehouseName { get; set; } = default!;
    public Guid TargetAccountId { get; set; }
    public string TargetAccountCode { get; set; } = default!;
    public string TargetAccountName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public List<StockIssueLineDto> Lines { get; set; } = [];
}

public static class StockIssueExtensions
{
    public static IQueryable<StockIssueListDto> MapTo(this IQueryable<EntityWithAuditDto<StockIssue>> entity)
    {
        return entity
            .Select(s => new StockIssueListDto
            {
                Id = s.Entity.Id,
                DocumentNumber = s.Entity.DocumentNumber,
                Date = s.Entity.Date,
                IssueType = s.Entity.IssueType,
                SourceWarehouseId = s.Entity.SourceWarehouseId,
                SourceWarehouseName = s.Entity.SourceWarehouse == null ? string.Empty : s.Entity.SourceWarehouse.Name.Value,
                TargetAccountId = s.Entity.TargetAccountId,
                TargetAccountCode = s.Entity.TargetAccount == null ? string.Empty : s.Entity.TargetAccount.Code.Value,
                TargetAccountName = s.Entity.TargetAccount == null ? string.Empty : s.Entity.TargetAccount.Name.Value,
                LineCount = s.Entity.Lines.Count(),
                TotalAmount = s.Entity.Lines.Sum(l => l.Quantity * l.UnitCost.Value),
                Description = s.Entity.Description.Value,

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

    public static StockIssueDto ToDto(this StockIssue issue)
    {
        return new StockIssueDto
        {
            Id = issue.Id,
            DocumentNumber = issue.DocumentNumber,
            Date = issue.Date,
            IssueType = issue.IssueType,
            CostingMethod = issue.CostingMethod,
            SourceWarehouseId = issue.SourceWarehouseId,
            SourceWarehouseName = issue.SourceWarehouse?.Name.Value ?? string.Empty,
            TargetAccountId = issue.TargetAccountId,
            TargetAccountCode = issue.TargetAccount?.Code.Value ?? string.Empty,
            TargetAccountName = issue.TargetAccount?.Name.Value ?? string.Empty,
            Description = issue.Description.Value,
            IsActive = issue.IsActive,
            CreatedAt = issue.CreatedAt,
            CreatedBy = issue.CreatedBy,
            UpdatedAt = issue.UpdatedAt,
            UpdatedBy = issue.UpdatedBy == null ? null : issue.UpdatedBy.Value,
            Lines = issue.Lines.Select(l => new StockIssueLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductCode = l.Product?.ProductCode.Value ?? string.Empty,
                ProductName = l.Product?.Name.Value ?? string.Empty,
                UnitTypeName = l.Product?.ProductUnitType?.Name.Value ?? string.Empty,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost.Value,
                TotalAmount = l.Quantity * l.UnitCost.Value,
                Description = l.Description.Value
            }).ToList()
        };
    }
}
