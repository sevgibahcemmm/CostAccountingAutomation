using Cost.Accounting.Automation.Domain.CostSlips;

namespace Cost.Accounting.Automation.Application.CostSlips;

public static class ExpenseAccountHelper
{
    /// <summary>
    /// Yarı mamul stoktan (151) tüketilen malzemenin gider hesabıdır.
    /// Slip tipinden bağımsız olarak HER pusulada görünür.
    /// </summary>
    public const ExpenseAccountType SemiFinishedAccount = ExpenseAccountType.Account151_SemiFinished;

    public static List<ExpenseAccountType> GetFilteredAccounts(CostSlipType costSlipType)
    {
        string targetDescription = (costSlipType == CostSlipType.Service)
            ? "Hizmet"
            : "Mamul";

        List<ExpenseAccountType> filteredList = [];

        foreach (ExpenseAccountType account in Enum.GetValues<ExpenseAccountType>())
        {
            if (account == SemiFinishedAccount)
            {
                continue;
            }

            string? description = Helpers.EnumDisplay.GetDisplayDescription(account);
            if (description == targetDescription || description == "Ortak")
            {
                filteredList.Add(account);
            }
        }

        filteredList.Add(SemiFinishedAccount);

        return filteredList;
    }
}