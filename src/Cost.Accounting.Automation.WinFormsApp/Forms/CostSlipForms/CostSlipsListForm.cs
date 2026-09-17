using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
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
        }

        protected override CostSlipGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(CostSlipListDto item)
            => new CostSlipDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CostSlipListDto item)
            => $"{item.SlipNumber} ({item.CustomerName})";

        protected override bool SupportsRestore => true;

        protected override bool SupportsApprove => true;

        protected override bool AllowsEdit(CostSlipListDto item) => item.Status != CostSlipStatus.Approved;

        protected override bool AllowsDelete(CostSlipListDto item) => item.Status != CostSlipStatus.Approved;

        protected override IRequest<Result<string>>? BuildApproveCommand(CostSlipListDto item)
            => item.Status == CostSlipStatus.Draft ? new CostSlipApproveCommand(item.Id) : null;

        protected override IRequest<Result<string>> BuildRestoreCommand(CostSlipListDto item)
            => new CostSlipRestoreCommand(item.Id);
    }
}