using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.StockIssues;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

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
            ConfigureItemsDetail();
        }

        private void ConfigureMasterSummary()
        {
            View.OptionsView.ShowFooter = true;

            GridColumn quantityColumn = View.Columns[nameof(StockIssueListDto.LineCount)];
            quantityColumn.Summary.Add(SummaryItemType.Sum, nameof(StockIssueListDto.LineCount), "Kalem: {0:n0}");

            GridColumn totalColumn = View.Columns[nameof(StockIssueListDto.TotalAmount)];
            totalColumn.Summary.Add(SummaryItemType.Sum, nameof(StockIssueListDto.TotalAmount), "Toplam: {0:n2}");
        }

        private void ConfigureItemsDetail()
        {
            View.OptionsDetail.EnableMasterViewMode = true;
            View.OptionsDetail.ShowDetailTabs = false;
            View.OptionsDetail.AllowOnlyOneMasterRowExpanded = false;

            GridView linesView = new(BaseGrid)
            {
                Name = "StockIssueLinesView"
            };
            linesView.OptionsBehavior.Editable = false;
            linesView.OptionsView.ShowGroupPanel = false;
            linesView.OptionsView.EnableAppearanceEvenRow = true;
            linesView.OptionsView.EnableAppearanceOddRow = true;

            AddDetailColumn(linesView, nameof(StockIssueLineDto.ProductCode), "Ürün Kodu", 110, alignment: "Right");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.ProductName), "Ürün Adı", 220);
            AddDetailColumn(linesView, nameof(StockIssueLineDto.UnitTypeName), "Birim", 70, alignment: "Center");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.Quantity), "Miktar", 90, "n2", "Right");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.UnitCost), "Birim Maliyet", 110, "n2", "Right");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.TotalAmount), "Toplam Tutar", 120, "n2", "Right");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.Description), "Açıklama", 200);

            linesView.OptionsView.ShowFooter = true;
            linesView.Columns[nameof(StockIssueLineDto.Quantity)].Summary.Add(
                SummaryItemType.Sum, nameof(StockIssueLineDto.Quantity), "Toplam: {0:n2}");
            linesView.Columns[nameof(StockIssueLineDto.TotalAmount)].Summary.Add(
                SummaryItemType.Sum, nameof(StockIssueLineDto.TotalAmount), "Toplam: {0:n2}");

            BaseGrid.LevelTree.Nodes.Add(new GridLevelNode
            {
                RelationName = "StockIssueLines",
                LevelTemplate = linesView
            });

            View.MasterRowGetRelationName += (_, e) => e.RelationName = "StockIssueLines";
            View.MasterRowGetChildList += (_, e) =>
                e.ChildList = (View.GetRow(e.RowHandle) as StockIssueListDto)?.Lines;
        }

        private static void AddDetailColumn(GridView view, string fieldName, string caption, int width,
            string? format = null, string? alignment = null)
        {
            GridColumn column = new()
            {
                Caption = caption,
                FieldName = fieldName,
                Width = width,
                Visible = true,
                OptionsColumn = { AllowEdit = false }
            };

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DisplayFormat.FormatType = FormatType.Numeric;
                column.DisplayFormat.FormatString = format;
            }

            if (alignment == "Right")
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            }
            else if (alignment == "Center")
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            }

            view.Columns.Add(column);
        }
    }
}