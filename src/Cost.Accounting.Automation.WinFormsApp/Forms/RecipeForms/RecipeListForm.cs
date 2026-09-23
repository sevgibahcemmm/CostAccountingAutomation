using Cost.Accounting.Automation.Application.Recipes;
using Cost.Accounting.Automation.Domain.Recipes;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using Microsoft.Extensions.DependencyInjection;
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
    }

    protected override RecipeGetAllQuery BuildListQuery()
        => new(OnlyDeleted: ShowDeleted);

    protected override IRequest<Result<string>> BuildDeleteCommand(RecipeListDto item)
        => new RecipeDeleteCommand(item.Id);

    protected override string GetDeleteSummary(RecipeListDto item)
        => item.ProductName;

    protected override bool SupportsRestore => true;

    protected override IRequest<Result<string>>? BuildRestoreCommand(RecipeListDto item)
        => new RecipeRestoreCommand(item.Id);

    protected override bool AllowsDelete(RecipeListDto item) => true;
}