using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Domain.Companies;

public sealed class Company : Entity
{
    private Company() { }

    public Company(
        Name name,
        TaxOffice taxOffice,
        TaxNumber taxNumber,
        Description description,
        Invoiceinformation invoiceinformation,
        Letterhead letterhead,
        CompanyPrefix companyPrefix,
        Address address,
        Contact contact,
        ExpenditureUnit expenditureUnit,
        AccountingUnit accountingUnit,
        bool isActive
    )
    {
        SetName(name);
        SetTaxOffice(taxOffice);
        SetTaxNumber(taxNumber);
        SetDescription(description);
        SetInvoiceinformation(invoiceinformation);
        SetLetterhead(letterhead);
        SetCompanyPrefix(companyPrefix);
        SetAddress(address);
        SetContact(contact);
        SetExpenditureUnit(expenditureUnit);
        SetAccountingUnit(accountingUnit);
        SetStatus(isActive);
        ResolveDuplicateKey();
    }

    public Name Name { get; private set; } = default!;
    public TaxOffice TaxOffice { get; private set; } = default!;
    public TaxNumber TaxNumber { get; private set; } = default!;
    public Description Description { get; private set; } = default!;
    public Invoiceinformation Invoiceinformation { get; private set; } = default!;
    public Letterhead Letterhead { get; private set; } = default!;
    public CompanyPrefix CompanyPrefix { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public Contact Contact { get; private set; } = default!;
    public ExpenditureUnit ExpenditureUnit { get; private set; } = default!;
    public AccountingUnit AccountingUnit { get; private set; } = default!;

    public ICollection<User> Users { get; private set; } = new List<User>();

    public static string? BuildDuplicateKey(string name)
        => DuplicateKeyRule.From(name);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(Name.Value));

    #region Behaviors
    public void SetName(Name name)
    {
        Name = name;
        ResolveDuplicateKey();
    }
    public void SetTaxOffice(TaxOffice taxOffice) => TaxOffice = taxOffice;
    public void SetTaxNumber(TaxNumber taxNumber) => TaxNumber = taxNumber;
    public void SetDescription(Description description) => Description = description;
    public void SetInvoiceinformation(Invoiceinformation invoiceinformation) => Invoiceinformation = invoiceinformation;
    public void SetLetterhead(Letterhead letterhead) => Letterhead = letterhead;
    public void SetCompanyPrefix(CompanyPrefix companyPrefix) => CompanyPrefix = companyPrefix;
    public void SetAddress(Address address) => Address = address;
    public void SetContact(Contact contact) => Contact = contact;
    public void SetExpenditureUnit(ExpenditureUnit expenditureUnit) => ExpenditureUnit = expenditureUnit;
    public void SetAccountingUnit(AccountingUnit accountingUnit) => AccountingUnit = accountingUnit;
    #endregion
}