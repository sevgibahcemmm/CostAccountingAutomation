using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.ChartOfAccounts;

public enum ChartOfAccountType
{
    MainGroup = 0,
    Warehouse = 1,
    Category = 2,
    Workshop = 3,
    Stok = 4,   
}

public sealed class ChartOfAccount : Entity, IHardDeletable
{
    private ChartOfAccount()
    {
    }

    public ChartOfAccount(
        AccountCode code,
        Name name,
        int level,
        ChartOfAccountType type)
    {
        Code = code;
        Name = name;
        Level = level;
        Type = type;
    }

    public AccountCode Code { get; private set; } = default!;
    public Name Name { get; private set; } = default!;
    public IdentityId? ParentId { get; private set; }
    public int Level { get; private set; }
    public ChartOfAccountType Type { get; private set; }

    public IdentityId? SemiFinishedAccountId { get; private set; }
    public IdentityId? FinishedAccountId { get; private set; }

    public void SetName(Name name) => Name = name;

    public void SetCode(AccountCode code) => Code = code;

    public void SetLevel(int level) => Level = level;

    public void SetType(ChartOfAccountType type) => Type = type;

    public void SetParent(IdentityId? parentId) => ParentId = parentId;

    public void SetProductionLinks(IdentityId? semiFinishedAccountId, IdentityId? finishedAccountId)
    {
        SemiFinishedAccountId = semiFinishedAccountId;
        FinishedAccountId = finishedAccountId;
    }
}