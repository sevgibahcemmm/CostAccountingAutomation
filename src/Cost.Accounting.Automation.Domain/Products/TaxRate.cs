using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Products;

public sealed class TaxRate : Entity
{
    private TaxRate()
    {
    }

    public TaxRate(Name name, decimal rate, bool isActive)
    {
        SetName(name);
        SetRate(rate);
        SetStatus(isActive);
    }

    public Name Name { get; private set; } = default!;
    public decimal Rate { get; private set; }

    public void SetName(Name name) => Name = name;

    public void SetRate(decimal rate) => Rate = rate;
}