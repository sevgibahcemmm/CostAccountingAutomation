using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.WinFormsApp.Forms.AccountingYearForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CompanyForms
{
    public sealed partial class CompaniesListForm : CrudListFormBase<CompanyGetAllQuery, CompanyDto, CompanyEditForm>
    {
        private readonly SimpleButton _btnAccountingYear = new();

        public CompaniesListForm() : base("Şirketler")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.Company;

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
            foreach (Control ctrl in Controls)
            {
                if (ctrl is Panel panel && panel.Dock == DockStyle.Top)
                {
                    panel.Height = 100;
                    break;
                }
            }

            _btnAccountingYear.Text = "Mali Yıl Aç";
            _btnAccountingYear.ImageOptions.SvgImage = DxIcon.Recipe;
            _btnAccountingYear.ImageOptions.SvgImageSize = new Size(18, 18);
            _btnAccountingYear.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            _btnAccountingYear.Click += BtnAccountingYear_Click;
            AddToolbarButton(_btnAccountingYear);

            base.OnLoad(e);
        }

        /// <summary>
        /// Seçili kurum için yeni mali yıl açar. Yıl veritabanı yalnızca tek bir
        /// kurum seçiliyken açılabilir.
        /// </summary>
        private void BtnAccountingYear_Click(object? sender, EventArgs e)
        {
            int[] rows = View.GetSelectedRows();

            if (rows.Length != 1 || View.GetRow(rows[0]) is not CompanyDto company)
            {
                ToastHelper.Show("Mali yıl açmak için tek bir kurum seçin.", ToastType.Warning);
                return;
            }

            using var form = new AccountingYearCreateForm(company);
            form.ShowDialog(this);
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
