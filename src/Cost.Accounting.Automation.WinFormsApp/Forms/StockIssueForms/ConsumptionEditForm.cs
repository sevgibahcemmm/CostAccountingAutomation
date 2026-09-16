using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.StockIssues;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public sealed class ConsumptionEditForm : StockIssueEditFormBase
    {
        public ConsumptionEditForm() : base(StockIssueType.Consumption, null)
        {
        }

        public ConsumptionEditForm(StockIssueListDto existing) : base(StockIssueType.Consumption, existing)
        {
        }
    }
}
