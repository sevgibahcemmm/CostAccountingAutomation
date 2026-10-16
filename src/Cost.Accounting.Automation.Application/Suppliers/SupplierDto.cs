using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Suppliers;

namespace Cost.Accounting.Automation.Application.Suppliers;

public sealed class SupplierDto : EntityDto
{
    [Column("Tedarikçi Adı", Order = 10, Width = 220)]
    public string Name { get; set; } = default!;

    [Column("Vergi Dairesi", Order = 25, Width = 140)]
    public string TaxOffice { get; set; } = default!;

    [Column("Vergi No", Order = 30, Width = 110)]
    public string TaxNumber { get; set; } = default!;

    [Column("Açıklama", IsVisible = false)]
    public string Description { get; set; } = default!;

    [Column("Şehir", Order = 40, Width = 120)]
    public string City { get; set; } = default!;

    [Column("İlçe", Order = 45, Width = 120)]
    public string District { get; set; } = default!;

    [Column("Açık Adres", IsVisible = false)]
    public string FullAddress { get; set; } = default!;

    [Column("Telefon 1", Order = 50, Width = 120)]
    public string PhoneNumber1 { get; set; } = default!;

    [Column("Telefon 2", IsVisible = false)]
    public string? PhoneNumber2 { get; set; }

    [Column("E-Posta", Order = 55, Width = 160)]
    public string? Email { get; set; }
}

public static class SupplierExtensions
{
    public static IQueryable<SupplierDto> MapTo(this IQueryable<Supplier> entity)
    {
        return entity
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name.Value,
                TaxOffice = s.TaxOffice.Value,
                TaxNumber = s.TaxNumber.Value,
                Description = s.Description.Value,
                City = s.Address.City,
                District = s.Address.District,
                FullAddress = s.Address.FullAddress,
                PhoneNumber1 = s.Contact.PhoneNumber1,
                PhoneNumber2 = s.Contact.PhoneNumber2,
                Email = s.Contact.Email,

                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy == null ? null : s.UpdatedBy.Value,
            });
    }
}