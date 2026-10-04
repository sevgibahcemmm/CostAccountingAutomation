using System.Drawing;
using System.Linq;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using DevExpress.Utils.Svg;
using TS.MediatR;
using TS.Result;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using Cost.Accounting.Automation.Application.Products.TaxRates;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class TaxRatesListForm : CrudListFormBase<TaxRateGetAllQuery, TaxRateDto, TaxRateEditForm>
    {
        public TaxRatesListForm() : base("KDV Oranları")
        {
InitializeComponent();
        }

        protected override SvgImage ModuleIcon => DxIcon.Tag;

        protected override string[] SearchFieldNames =>
        [
            nameof(TaxRateDto.Name)
        ];

protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
        }

protected override string DeleteItemLabel => "KDV oranı";

        protected override IRequest<Result<string>> BuildDeleteCommand(TaxRateDto item)
            => new TaxRateDeleteCommand(item.Id);

        /// <summary>Seçili KDV oranları tek transaction'da silinir.</summary>
        protected override IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<TaxRateDto> items)
            => new BulkDeleteTaxRatesCommand(items.Select(item => item.Id).ToList());

        protected override string GetDeleteSummary(TaxRateDto item)
            => $"{item.Name} (%{item.Rate * 100:0.###})";

        protected override bool SupportsRestore => true;

        protected override TaxRateGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(TaxRateDto item)
            => new TaxRateRestoreCommand(item.Id);
    }
}