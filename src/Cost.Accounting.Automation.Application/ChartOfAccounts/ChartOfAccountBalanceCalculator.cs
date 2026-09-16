namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

public static class ChartOfAccountBalanceCalculator
{
    public static void RollUp(List<ChartOfAccountDto> items)
    {
        Dictionary<Guid, List<ChartOfAccountDto>> childrenByParent = items
            .Where(x => x.ParentId is not null)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (ChartOfAccountDto root in items.Where(x => x.ParentId is null))
        {
            Accumulate(root, childrenByParent);
        }
    }

    private static (decimal Debit, decimal Credit) Accumulate(
        ChartOfAccountDto node,
        Dictionary<Guid, List<ChartOfAccountDto>> childrenByParent)
    {
        decimal debit = node.DebitAmount;
        decimal credit = node.CreditAmount;

        if (childrenByParent.TryGetValue(node.Id, out List<ChartOfAccountDto>? children))
        {
            foreach (ChartOfAccountDto child in children)
            {
                (decimal cd, decimal cc) = Accumulate(child, childrenByParent);
                debit += cd;
                credit += cc;
            }
        }

        node.DebitAmount = debit;
        node.CreditAmount = credit;
        return (debit, credit);
    }
}