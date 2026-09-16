
using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.ChartOfAccounts;

public sealed class ChartOfAccountLedger : Entity
{
    private ChartOfAccountLedger()
    {
    }

    public ChartOfAccountLedger(
        IdentityId chartOfAccountId,
        decimal debitAmount,
        decimal creditAmount,
        string sourceType,
        IdentityId? sourceId)
    {
        if (debitAmount < 0 || creditAmount < 0)
        {
            throw new ArgumentException("Borç/Alacak tutarı negatif olamaz.");
        }

        ChartOfAccountId = chartOfAccountId;
        DebitAmount = debitAmount;
        CreditAmount = creditAmount;
        SourceType = sourceType;
        SourceId = sourceId;
    }

    public IdentityId ChartOfAccountId { get; private set; } = default!;
    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }
    public string SourceType { get; private set; } = default!; // "StokGirisi", "StokCikisi" vb.
    public IdentityId? SourceId { get; private set; }
}