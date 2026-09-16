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
    public static IQueryable<TaxRateDto> MapTo(this IQueryable<EntityWithAuditDto<TaxRate>> entity)
    {
        return entity
            .Select(s => new TaxRateDto
            {
                Id = s.Entity.Id,
                Name = s.Entity.Name.Value,
                Rate = s.Entity.Rate,

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