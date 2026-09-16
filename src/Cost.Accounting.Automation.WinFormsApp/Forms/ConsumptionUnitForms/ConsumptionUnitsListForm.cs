using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ConsumptionUnitForms
{
    public sealed class ConsumptionUnitsListForm : CrudListFormBase<ConsumptionUnitGetAllQuery, ConsumptionUnitDto, ConsumptionUnitEditForm>
    {
        public ConsumptionUnitsListForm() : base("Tüketim Birimleri")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.StockIssue;

        protected override string[] SearchFieldNames =>
        [
            nameof(ConsumptionUnitDto.Code),
            nameof(ConsumptionUnitDto.Name)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
        }

        protected override ConsumptionUnitGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(ConsumptionUnitDto item)
            => new ConsumptionUnitDeleteCommand(item.Id);

        protected override string GetDeleteSummary(ConsumptionUnitDto item)
            => $"{item.Code} - {item.Name}";

        protected override bool SupportsRestore => false;
    }
}
