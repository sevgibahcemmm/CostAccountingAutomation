using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Application.Customers;

public sealed class CustomerDto : EntityDto
{
    [Column("Müşteri Adı", Order = 10, Width = 220)]
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

public static class CustomerExtensions
{
    public static IQueryable<CustomerDto> MapTo(this IQueryable<EntityWithAuditDto<Customer>> entity)
    {
        return entity
            .Select(s => new CustomerDto
            {
                Id = s.Entity.Id,
                Name = s.Entity.Name.Value,
                TaxOffice = s.Entity.TaxOffice.Value,
                TaxNumber = s.Entity.TaxNumber.Value,
                Description = s.Entity.Description.Value,
                City = s.Entity.Address.City,
                District = s.Entity.Address.District,
                FullAddress = s.Entity.Address.FullAddress,
                PhoneNumber1 = s.Entity.Contact.PhoneNumber1,
                PhoneNumber2 = s.Entity.Contact.PhoneNumber2,
                Email = s.Entity.Contact.Email,

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