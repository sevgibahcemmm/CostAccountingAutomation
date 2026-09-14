

using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Application.Companies;

public sealed class CompanyDto : EntityDto
{
    [Column("Şirket Adı", Order = 10, Width = 220)]
    public string Name { get; set; } = default!;

    [Column("Vergi Dairesi", Order = 25, Width = 140)]
    public string TaxOffice { get; set; } = default!;

    [Column("Vergi No", Order = 30, Width = 110)]
    public string TaxNumber { get; set; } = default!;

    [Column("Açıklama", IsVisible = false)]
    public string Description { get; set; } = default!;

    [Column("Fatura Bilgisi", IsVisible = false)]
    public string Invoiceinformation { get; set; } = default!;

    [Column("Kaşe", IsVisible = false)]
    public string Letterhead { get; set; } = default!;

    [Column("Ön Ek", Order = 15, Width = 70, Alignment = "Center")]
    public string CompanyPrefix { get; set; } = default!;

    [Column("Adres", IsVisible = false)]
    public Address Address { get; set; } = default!;

    [Column("İletişim", IsVisible = false)]
    public Contact Contact { get; set; } = default!;

    // --- Yeni Eklenen Rapor Alanları ---
    [Column("Harcama Birimi", Order = 75, Width = 150)]
    public string ExpenditureUnitName { get; set; } = default!;

    [Column("Harcama Birimi Kodu", IsVisible = false)]
    public string ExpenditureUnitCode { get; set; } = default!;

    [Column("Muhasebe Birimi", Order = 80, Width = 150)]
    public string AccountingUnitName { get; set; } = default!;

    [Column("Muhasebe Birimi Kodu", IsVisible = false)]
    public string AccountingUnitCode { get; set; } = default!;

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