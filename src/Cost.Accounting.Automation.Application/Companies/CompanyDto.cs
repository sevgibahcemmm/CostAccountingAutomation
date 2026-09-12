

using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Application.Companies;

public sealed class CompanyDto : EntityDto
{
    public string Name { get; set; } = default!;
    public string TaxOffice { get; set; } = default!;
    public string TaxNumber { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Invoiceinformation { get; set; } = default!;
    public string Letterhead { get; set; } = default!;
    public string CompanyPrefix { get; set; } = default!;
    public Address Address { get; set; } = default!;
    public Contact Contact { get; set; } = default!;

    // --- Yeni Eklenen Rapor Alanları ---
    public string ExpenditureUnitName { get; set; } = default!;
    public string ExpenditureUnitCode { get; set; } = default!;
    public string AccountingUnitName { get; set; } = default!;
    public string AccountingUnitCode { get; set; } = default!;
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public string FullAddress { get; set; } = default!;
    public string PhoneNumber1 { get; set; } = default!;
    public string? PhoneNumber2 { get; set; }
    public string? Email { get; set; }
}


public static class CompanyExtensions
{
    public static IQueryable<CompanyDto> MapTo(this IQueryable<EntityWithAuditDto<Company>> entity)
    {
        return entity
            .Select(s => new CompanyDto
            {
                Id = s.Entity.Id,
                Name = s.Entity.Name.Value,
                TaxOffice = s.Entity.TaxOffice.Value,
                TaxNumber = s.Entity.TaxNumber.Value,
                Description = s.Entity.Description.Value,
                Invoiceinformation = s.Entity.Invoiceinformation.Value,
                Letterhead = s.Entity.Letterhead.Value,
                CompanyPrefix = s.Entity.CompanyPrefix.Value,

                // --- Yeni Alanların Mapping İşlemi ---
                // ExpenditureUnit.Value ve AccountingUnit.Name kullanılarak eşlendi
                ExpenditureUnitName = s.Entity.ExpenditureUnit.Value,
                ExpenditureUnitCode = s.Entity.ExpenditureUnit.Code,
                AccountingUnitName = s.Entity.AccountingUnit.Name,
                AccountingUnitCode = s.Entity.AccountingUnit.Code,

                Address = s.Entity.Address,
                Contact = s.Entity.Contact,

                // --- Flat alanların doldurulması (client için) ---
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