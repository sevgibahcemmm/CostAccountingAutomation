using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.StockIssues;

public sealed class StockIssueLine : Entity, IHardDeletable
{
    private StockIssueLine()
    {
    }

    public StockIssueLine(
        IdentityId stockIssueId,
        IdentityId productId,
        decimal quantity,
        Price unitCost,
        Description description)
    {
        StockIssueId = stockIssueId;
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
        Description = description;
    }

    public IdentityId StockIssueId { get; private set; } = default!;
    public IdentityId ProductId { get; private set; } = default!;
    public Product? Product { get; private set; }

    public decimal Quantity { get; private set; }
    public Price UnitCost { get; private set; } = default!;
    public Description Description { get; private set; } = default!;
}
