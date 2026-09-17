using Cost.Accounting.Automation.Domain.CostSlips;

namespace Cost.Accounting.Automation.Application.CostSlips;

public static class ExpenseAccountHelper
{
    public static List<ExpenseAccountType> GetFilteredAccounts(CostSlipType costSlipType)
    {
        string targetDescription = (costSlipType == CostSlipType.Service)
            ? "Hizmet"
            : "Mamul";

        List<ExpenseAccountType> filteredList = [];

        foreach (ExpenseAccountType account in Enum.GetValues<ExpenseAccountType>())
        {
            string? description = Helpers.EnumDisplay.GetDisplayDescription(account);
            if (description == targetDescription || description == "Ortak")
            {
                filteredList.Add(account);
            }
        }

        return filteredList;
    }
}