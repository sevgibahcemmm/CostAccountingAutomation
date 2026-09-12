namespace Cost.Accounting.Automation.Domain.Companies.ValueObjects;

public sealed record TaxOffice(string Value)
{
    public override string ToString() => Value;
}
