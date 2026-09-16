using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.StockIssues;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public sealed class AtelierTransferEditForm : StockIssueEditFormBase
    {
        public AtelierTransferEditForm() : base(StockIssueType.AtelierTransfer, null)
        {
        }

        public AtelierTransferEditForm(StockIssueListDto existing) : base(StockIssueType.AtelierTransfer, existing)
        {
        }
    }
}
