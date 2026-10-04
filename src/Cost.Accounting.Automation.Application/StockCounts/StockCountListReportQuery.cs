using Cost.Accounting.Automation.Application.Behaviors;
using System.ComponentModel.DataAnnotations;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.StockCounts;

public enum StockCountGroupMode
{
    [Display(Name = "Atölye Bazında")]
    Workshop = 1,

    [Display(Name = "Depo Bazında")]
    Warehouse = 2
}

[Permission("stockmovement:view")]
public sealed record StockCountListReportQuery(
    StockCountGroupMode GroupMode,
    bool AllGroups,
    List<Guid>? GroupIds,
    DateOnly AsOfDate) : IRequest<List<StockCountReportRowDto>>;

public sealed class StockCountReportRowDto
{
    public int RowNumber { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string UnitTypeName { get; set; } = default!;
    public decimal SystemQuantity { get; set; }
    public decimal GroupTotalQuantity { get; set; }
    public decimal GrandTotalQuantity { get; set; }
    public Guid GroupId { get; set; }

    /// <summary>
    /// Grubun hesap planı kodu (depo kodu veya atölye kodu).
    /// Grup başlığında kod ve ad birlikte basılır; kod bulunamazsa boş kalır
    /// ve yalnızca ad gösterilir.
    /// </summary>
    public string GroupCode { get; set; } = default!;

    /// <summary>Grubun adı: depo bazında depo adı, atölye bazında atölye adı.</summary>
    public string GroupName { get; set; } = default!;
}