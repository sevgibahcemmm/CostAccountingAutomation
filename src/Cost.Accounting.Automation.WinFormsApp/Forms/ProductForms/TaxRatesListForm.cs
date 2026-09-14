using System.Drawing;
using System.Linq;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class TaxRatesListForm : CrudListFormBase<TaxRateGetAllQuery, TaxRateDto, TaxRateEditForm>
    {
        public TaxRatesListForm() : base("KDV Oranları")
        {
            InitializeComponent();
            IncreaseHeaderHeight();
        }

        protected override SvgImage ModuleIcon => SvgIcons.TagIcon;

        protected override string[] SearchFieldNames =>
        [
            nameof(TaxRateDto.Name)
        ];

        private void IncreaseHeaderHeight()
        {
            if (Controls.Find("pnlHeader", true).FirstOrDefault() is Control headerPanel)
            {
                headerPanel.Height = 88;
            }
        }

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(TaxRateDto item)
            => new TaxRateDeleteCommand(item.Id);

        protected override string GetDeleteSummary(TaxRateDto item)
            => $"{item.Name} (%{item.Rate * 100:0.###})";

        protected override bool SupportsRestore => true;

        protected override TaxRateGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(TaxRateDto item)
            => new TaxRateRestoreCommand(item.Id);
    }
}