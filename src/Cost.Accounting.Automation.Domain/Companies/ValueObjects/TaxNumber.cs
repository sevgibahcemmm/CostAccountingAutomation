namespace Cost.Accounting.Automation.Domain.Companies.ValueObjects;


public sealed record TaxNumber(string Value)
{
    public override string ToString() => Value;
}