using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public sealed partial class CostSlipsListForm : CrudListFormBase<CostSlipGetAllQuery, CostSlipListDto, CostSlipEditForm>
    {
        public CostSlipsListForm() : base("Maliyet Pusulası")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.Module;

        protected override string[] SearchFieldNames =>
        [
            nameof(CostSlipListDto.SlipNumber),
            nameof(CostSlipListDto.WorkshopName),
            nameof(CostSlipListDto.ProducedProductName),
            nameof(CostSlipListDto.CustomerName),
            nameof(CostSlipListDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            ConfigureItemsDetail();
        }

        private void ConfigureItemsDetail()
        {
            View.OptionsDetail.EnableMasterViewMode = true;
            View.OptionsDetail.ShowDetailTabs = false;
            View.OptionsDetail.AllowOnlyOneMasterRowExpanded = false;

            GridView itemsView = new(BaseGrid)
            {
                Name = "CostSlipItemsView"
            };
            itemsView.OptionsBehavior.Editable = false;
            itemsView.OptionsView.ShowGroupPanel = false;
            itemsView.OptionsView.EnableAppearanceEvenRow = true;
            itemsView.OptionsView.EnableAppearanceOddRow = true;

            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ProductName), "Ürün / Masraf", 220);
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ProductUnitTypeName), "Birim", 80, alignment: "Center");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ExpenseAccountTypeName), "Hesap", 250);
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.Quantity), "Miktar", 90, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.UnitPrice), "Birim Fiyat", 100, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.TotalAmount), "Tutar", 110, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.Description), "Açıklama", 200);

            BaseGrid.LevelTree.Nodes.Add(new GridLevelNode
            {
                RelationName = "CostSlipItems",
                LevelTemplate = itemsView
            });

            View.MasterRowGetRelationName += (_, e) => e.RelationName = "CostSlipItems";
            View.MasterRowGetChildList += (_, e) =>
                e.ChildList = (View.GetRow(e.RowHandle) as CostSlipListDto)?.CostSlipItems;
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
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = format;
            }

            if (alignment == "Right")
            {
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            }
            else if (alignment == "Center")
            {
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }

            view.Columns.Add(column);
        }

        protected override CostSlipGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(CostSlipListDto item)
            => new CostSlipDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CostSlipListDto item)
            => $"{item.SlipNumber} ({item.CustomerName})";

        protected override bool SupportsRestore => true;

        protected override bool SupportsApprove => true;

        protected override bool SupportsSlipReport => true;

        protected override bool SupportsDistributionReport => true;

        protected override bool AllowsApprove(CostSlipListDto item) => item.Status == CostSlipStatus.Draft;

        protected override bool AllowsEdit(CostSlipListDto item) => item.Status != CostSlipStatus.Approved;

        protected override bool AllowsDelete(CostSlipListDto item) => item.Status != CostSlipStatus.Approved;

        protected override IRequest<Result<string>>? BuildApproveCommand(CostSlipListDto item)
            => item.Status == CostSlipStatus.Draft ? new CostSlipApproveCommand(item.Id) : null;

        protected override IRequest<Result<string>> BuildRestoreCommand(CostSlipListDto item)
            => new CostSlipRestoreCommand(item.Id);

        protected override Task ShowSlipReportAsync(CostSlipListDto item)
            => CostSlipReportPresenter.ShowAsync(item);

        protected override Task ShowDistributionReportAsync(CostSlipListDto item)
            => GiderDagitimReportPresenter.ShowAsync(item);
    }
}