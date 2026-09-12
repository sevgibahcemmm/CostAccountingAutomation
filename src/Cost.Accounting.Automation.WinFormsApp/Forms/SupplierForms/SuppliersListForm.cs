using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
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
            View.Columns.Clear();

            GridColumn[] columns =
            [
                new() { Caption = "Firma / Kişi Adı", FieldName = nameof(SupplierDto.Name), Visible = true, Width = 220 },
                new() { Caption = "Vergi Dairesi", FieldName = nameof(SupplierDto.TaxOffice), Visible = true, Width = 130 },
                new() { Caption = "Vergi No", FieldName = nameof(SupplierDto.TaxNumber), Visible = true, Width = 115 },
                new() { Caption = "Şehir", FieldName = nameof(SupplierDto.City), Visible = true, Width = 95 },
                new() { Caption = "İlçe", FieldName = nameof(SupplierDto.District), Visible = true, Width = 95 },
                new() { Caption = "Telefon 1", FieldName = nameof(SupplierDto.PhoneNumber1), Visible = true, Width = 125 },
                new() { Caption = "E-Posta", FieldName = nameof(SupplierDto.Email), Visible = true, Width = 170 }
            ];

            View.Columns.AddRange(columns);
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