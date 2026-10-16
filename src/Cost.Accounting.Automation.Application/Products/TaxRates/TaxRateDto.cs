using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products.TaxRates;

namespace Cost.Accounting.Automation.Application.Products.TaxRates;

public sealed class TaxRateDto : EntityDto
{
    [Column("KDV Adı", Order = 10, Width = 200)]
    public string Name { get; set; } = default!;

    [Column("Oran (%)", Order = 20, Width = 90, Format = "p1", Alignment = "Right")]
    public decimal Rate { get; set; }

    [Column("Görünen", IsVisible = false)]
    public string Display => $"{Name} (%{Rate * 100:0.###})";
}

public static class TaxRateExtensions
{
    public static IQueryable<TaxRateDto> MapTo(this IQueryable<TaxRate> entity)
    {
        return entity
            .Select(s => new TaxRateDto
            {
                Id = s.Id,
                Name = s.Name.Value,
                Rate = s.Rate,

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value,
            });
    }
}