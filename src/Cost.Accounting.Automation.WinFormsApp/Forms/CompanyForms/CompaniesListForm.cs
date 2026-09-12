using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CompanyForms
{
    public sealed partial class CompaniesListForm : CrudListFormBase<CompanyGetAllQuery, CompanyDto, CompanyEditForm>
    {
        public CompaniesListForm() : base("Şirketler")
        {
        }

        protected override SvgImage ModuleIcon => SvgIcons.BuildingWhiteIcon;

        protected override string[] SearchFieldNames =>
        [
            nameof(CompanyDto.Name),
            nameof(CompanyDto.TaxOffice),
            nameof(CompanyDto.TaxNumber),
            nameof(CompanyDto.City),
            nameof(CompanyDto.District),
            nameof(CompanyDto.PhoneNumber1),
            nameof(CompanyDto.Email)
        ];

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();

            GridColumn[] columns =
            [
                new() { Caption = "Şirket Adı", FieldName = nameof(CompanyDto.Name), Visible = true, Width = 220 },
                new() { Caption = "Ön Ek", FieldName = nameof(CompanyDto.CompanyPrefix), Visible = true, Width = 70 },
                new() { Caption = "Vergi Dairesi", FieldName = nameof(CompanyDto.TaxOffice), Visible = true, Width = 130 },
                new() { Caption = "Vergi No", FieldName = nameof(CompanyDto.TaxNumber), Visible = true, Width = 115 },
                new() { Caption = "Şehir", FieldName = nameof(CompanyDto.City), Visible = true, Width = 95 },
                new() { Caption = "İlçe", FieldName = nameof(CompanyDto.District), Visible = true, Width = 95 },
                new() { Caption = "Telefon 1", FieldName = nameof(CompanyDto.PhoneNumber1), Visible = true, Width = 125 },
                new() { Caption = "E-Posta", FieldName = nameof(CompanyDto.Email), Visible = true, Width = 170 },
                new() { Caption = "Harcama Birimi", FieldName = nameof(CompanyDto.ExpenditureUnitName), Visible = true, Width = 120 },
                new() { Caption = "Muhasebe Birimi", FieldName = nameof(CompanyDto.AccountingUnitName), Visible = true, Width = 120 }
            ];

            View.Columns.AddRange(columns);
            AddColumnsFromAttributes();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(CompanyDto item)
            => new CompanyDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CompanyDto item) => item.Name;

        protected override bool SupportsRestore => true;

        protected override CompanyGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(CompanyDto item)
            => new CompanyRestoreCommand(item.Id);
    }
}