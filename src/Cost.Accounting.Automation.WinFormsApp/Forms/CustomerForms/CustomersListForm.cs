using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CustomerForms
{
    public sealed partial class CustomersListForm : CrudListFormBase<CustomerGetAllQuery, CustomerDto, CustomerEditForm>
    {
        public CustomersListForm() : base("Müşteriler")
        {
        }

        protected override SvgImage ModuleIcon => SvgIcons.Modules[5];

        protected override string[] SearchFieldNames =>
        [
            nameof(CustomerDto.Name),
            nameof(CustomerDto.TaxOffice),
            nameof(CustomerDto.TaxNumber),
            nameof(CustomerDto.City),
            nameof(CustomerDto.District),
            nameof(CustomerDto.PhoneNumber1),
            nameof(CustomerDto.Email)
        ];

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();

            GridColumn[] columns =
            [
                new() { Caption = "Firma / Kişi Adı", FieldName = nameof(CustomerDto.Name), Visible = true, Width = 220 },
                new() { Caption = "Vergi Dairesi", FieldName = nameof(CustomerDto.TaxOffice), Visible = true, Width = 130 },
                new() { Caption = "Vergi No", FieldName = nameof(CustomerDto.TaxNumber), Visible = true, Width = 115 },
                new() { Caption = "Şehir", FieldName = nameof(CustomerDto.City), Visible = true, Width = 95 },
                new() { Caption = "İlçe", FieldName = nameof(CustomerDto.District), Visible = true, Width = 95 },
                new() { Caption = "Telefon 1", FieldName = nameof(CustomerDto.PhoneNumber1), Visible = true, Width = 125 },
                new() { Caption = "E-Posta", FieldName = nameof(CustomerDto.Email), Visible = true, Width = 170 }
            ];

            View.Columns.AddRange(columns);
            AddColumnsFromAttributes();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(CustomerDto item)
            => new CustomerDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CustomerDto item) => item.Name;

        protected override bool SupportsRestore => true;

        protected override CustomerGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(CustomerDto item)
            => new CustomerRestoreCommand(item.Id);
    }
}