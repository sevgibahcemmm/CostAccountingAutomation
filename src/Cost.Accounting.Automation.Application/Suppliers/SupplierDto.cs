using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Suppliers;

namespace Cost.Accounting.Automation.Application.Suppliers;

public sealed class SupplierDto : EntityDto
{
    public string Name { get; set; } = default!;
    public string TaxOffice { get; set; } = default!;
    public string TaxNumber { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string FullAddress { get; set; } = default!;
    public string PhoneNumber1 { get; set; } = default!;
    public string? PhoneNumber2 { get; set; }
    public string? Email { get; set; }
}

public static class SupplierExtensions
{
    public static IQueryable<SupplierDto> MapTo(this IQueryable<EntityWithAuditDto<Supplier>> entity)
    {
        return entity
            .Select(s => new SupplierDto
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