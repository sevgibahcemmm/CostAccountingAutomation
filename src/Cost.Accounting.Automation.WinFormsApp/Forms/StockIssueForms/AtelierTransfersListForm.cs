using Cost.Accounting.Automation.Domain.StockIssues;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public sealed class AtelierTransfersListForm : StockIssueListFormBase<AtelierTransferEditForm>
    {
        public AtelierTransfersListForm() : base(StockIssueType.AtelierTransfer, "Atölye Transferi")
        {
        }
    }
}
