using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (Control ctrl in Controls)
            {
                if (ctrl is Panel panel && panel.Dock == DockStyle.Top)
                {
                    panel.Height = 100;
                    break;
                }
            }
        }

        protected override void ConfigureColumns()
        {
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