using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.SupplierForms
{
    public sealed partial class SuppliersListForm : CrudListFormBase<SupplierGetAllQuery, SupplierDto, SupplierEditForm>
    {
        public SuppliersListForm() : base("Tedarikçiler")
        {
        }

        protected override SvgImage ModuleIcon => SvgIcons.Modules[2];

        protected override string[] SearchFieldNames =>
        [
            nameof(SupplierDto.Name),
            nameof(SupplierDto.TaxOffice),
            nameof(SupplierDto.TaxNumber),
            nameof(SupplierDto.City),
            nameof(SupplierDto.District),
            nameof(SupplierDto.PhoneNumber1),
            nameof(SupplierDto.Email)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(SupplierDto item)
            => new SupplierDeleteCommand(item.Id);

        protected override string GetDeleteSummary(SupplierDto item) => item.Name;

        protected override bool SupportsRestore => true;

        protected override SupplierGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(SupplierDto item)
            => new SupplierRestoreCommand(item.Id);
    }
}