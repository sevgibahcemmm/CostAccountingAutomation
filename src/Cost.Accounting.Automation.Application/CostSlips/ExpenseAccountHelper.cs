using Cost.Accounting.Automation.Application.ChartOfAccounts;
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

    /// <summary>
    /// Gider tipinin hesap planındaki (7xx) karşılığını çözer.
    ///
    /// DisplayName'in "..." öncesi kod kısmı alınıp normalleştirilir:
    /// "740-02 - Üretimle..." → "740.02", "710 - Direkt..." → "710".
    /// Planın kodları DisplayName koduyla örtüşmüyorsa
    /// <see cref="_planCodeOverrides"/> kullanılır.
    ///
    /// Yarı mamul (151) bu akışta stok hesabından karşılığı olmadığı için
    /// buradan geçmez; gider eşlemesine konu olmaz.
    /// </summary>
    public static string GetChartOfAccountCode(ExpenseAccountType account)
    {
        if (_planCodeOverrides.TryGetValue(account, out string? overrideCode))
        {
            return overrideCode;
        }

        string displayName = Helpers.EnumDisplay.GetDisplayName(account);

        string codePart = displayName
            .Split(" - ", StringSplitOptions.TrimEntries)
            .First();

        return ChartOfAccountCodeHelper.NormalizeCode(codePart);
    }

    /// <summary>
    /// Hesap planındaki fiili kodu, DisplayName ön ekinden farklı olan giderler.
    /// DisplayName "720-02" (SGK Devlet Primi) derken hesap planında aynı gider
    /// "720.01.02" altında açılmıştır.
    /// </summary>
    private static readonly IReadOnlyDictionary<ExpenseAccountType, string> _planCodeOverrides =
        new Dictionary<ExpenseAccountType, string>
        {
            [ExpenseAccountType.Account720_2] = "720.01.02",
        };
}