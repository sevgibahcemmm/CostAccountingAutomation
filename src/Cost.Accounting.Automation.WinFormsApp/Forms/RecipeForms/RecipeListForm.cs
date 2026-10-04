using Cost.Accounting.Automation.Application.Recipes;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.RecipeForms;

public sealed partial class RecipeListForm : CrudListFormBase<RecipeGetAllQuery, RecipeListDto, RecipeEditForm>
{
    public RecipeListForm() : base("Reçeteler")
    {
    }

    protected override SvgImage ModuleIcon => DxIcon.Module;

    protected override string[] SearchFieldNames =>
    [
        nameof(RecipeListDto.ProductName),
        nameof(RecipeListDto.ProductUnitTypeName)
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
            Name = "RecipeItemsView"
        };
        itemsView.OptionsBehavior.Editable = false;
        itemsView.OptionsView.ShowGroupPanel = false;
        itemsView.OptionsView.EnableAppearanceEvenRow = true;
        itemsView.OptionsView.EnableAppearanceOddRow = true;
        itemsView.OptionsView.ShowFooter = true;

        AddDetailColumn(itemsView, nameof(RecipeItemDto.ProductName), "Malzeme", 300);
        AddDetailColumn(itemsView, nameof(RecipeItemDto.ProductUnitTypeName), "Birim", 80, alignment: "Center");
        AddDetailColumn(itemsView, nameof(RecipeItemDto.Quantity), "Miktar", 120, "n2", "Right");

        itemsView.Columns[nameof(RecipeItemDto.Quantity)].Summary.Add(
            DevExpress.Data.SummaryItemType.Sum, nameof(RecipeItemDto.Quantity), "Toplam: {0:n2}");

        BaseGrid.LevelTree.Nodes.Add(new GridLevelNode
        {
            RelationName = "Items",
            LevelTemplate = itemsView
        });

        View.MasterRowGetRelationName += (_, e) => e.RelationName = "Items";
        View.MasterRowGetChildList += (_, e) =>
            e.ChildList = (View.GetRow(e.RowHandle) as RecipeListDto)?.Items;
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

    protected override RecipeGetAllQuery BuildListQuery()
        => new(OnlyDeleted: ShowDeleted);

protected override string DeleteItemLabel => "reçete";

        protected override IRequest<Result<string>> BuildDeleteCommand(RecipeListDto item)
            => new RecipeDeleteCommand(item.Id);

        /// <summary>Seçili reçeteler tek transaction'da silinir.</summary>
        protected override IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<RecipeListDto> items)
            => new BulkDeleteRecipesCommand(items.Select(item => item.Id).ToList());

    protected override string GetDeleteSummary(RecipeListDto item)
        => item.ProductName;

    protected override bool SupportsRestore => true;

    protected override IRequest<Result<string>>? BuildRestoreCommand(RecipeListDto item)
        => new RecipeRestoreCommand(item.Id);

    protected override bool AllowsDelete(RecipeListDto item) => true;
}