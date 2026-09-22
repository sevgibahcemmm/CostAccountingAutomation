using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Suppliers;

public sealed class Supplier : Entity
{
    private Supplier() { }

    public Supplier(
        Name name,
        TaxOffice taxOffice,
        TaxNumber taxNumber,
        Contact contact,
        Address address,
        Description description,
        bool isActive)
    {
        SetName(name);
        SetTaxOffice(taxOffice);
        SetTaxNumber(taxNumber);
        SetContact(contact);
        SetAddress(address);
        SetDescription(description);
        SetStatus(isActive);
        ResolveDuplicateKey();
    }

    public Name Name { get; private set; } = default!;
    public TaxOffice TaxOffice { get; private set; } = default!;
    public TaxNumber TaxNumber { get; private set; } = default!;
    public Contact Contact { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public Description Description { get; private set; } = default!;

    public static string? BuildDuplicateKey(string name)
        => DuplicateKeyRule.From(name);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(Name.Value));

    public void SetName(Name name)
    {
        Name = name;
        ResolveDuplicateKey();
    }
    public void SetTaxOffice(TaxOffice taxOffice) => TaxOffice = taxOffice;
    public void SetTaxNumber(TaxNumber taxNumber) => TaxNumber = taxNumber;
    public void SetContact(Contact contact) => Contact = contact;
    public void SetAddress(Address address) => Address = address;
    public void SetDescription(Description description) => Description = description;
}