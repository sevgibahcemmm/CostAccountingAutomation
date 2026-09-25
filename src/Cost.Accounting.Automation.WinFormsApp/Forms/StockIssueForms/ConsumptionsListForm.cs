using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.StockIssues;
using DevExpress.Data;
using DevExpress.XtraGrid.Columns;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public sealed class ConsumptionsListForm : StockIssueListFormBase<ConsumptionEditForm>
    {
        public ConsumptionsListForm() : base(StockIssueType.Consumption, "Tüketim")
        {
        }

        protected override void ConfigureColumns()
        {
            base.ConfigureColumns();
            ConfigureMasterSummary();
        }

        private void ConfigureMasterSummary()
        {
            View.OptionsView.ShowFooter = true;

            GridColumn quantityColumn = View.Columns[nameof(StockIssueListDto.LineCount)];
            quantityColumn.Summary.Add(SummaryItemType.Sum, nameof(StockIssueListDto.LineCount), "Kalem: {0:n0}");

            GridColumn totalColumn = View.Columns[nameof(StockIssueListDto.TotalAmount)];
            totalColumn.Summary.Add(SummaryItemType.Sum, nameof(StockIssueListDto.TotalAmount), "Toplam: {0:n2}");
        }
    }
}