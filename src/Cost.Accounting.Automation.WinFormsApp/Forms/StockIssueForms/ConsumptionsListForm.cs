using Cost.Accounting.Automation.Domain.StockIssues;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public sealed class ConsumptionsListForm : StockIssueListFormBase<ConsumptionEditForm>
    {
        public ConsumptionsListForm() : base(StockIssueType.Consumption, "Tüketim")
        {
        }
    }
}
