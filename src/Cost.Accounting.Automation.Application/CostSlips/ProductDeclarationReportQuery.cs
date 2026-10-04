using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.CostSlips;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:view")]
public sealed record ProductDeclarationReportQuery(
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<List<ProductDeclarationRowDto>>;

public sealed class ProductDeclarationRowDto
{
    public Guid? WorkshopId { get; init; }
    public string WorkshopName { get; init; } = string.Empty;

    /// <summary>
    /// Atölyenin şefi. Beyan atölyeye göre gruplandığı için rapor bu alanı
    /// veri alanı olarak okur; parametre grup bazlı değişemez.
    /// </summary>
    public string WorkshopChiefName { get; set; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string ProductUnitTypeName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Total { get; init; }
}