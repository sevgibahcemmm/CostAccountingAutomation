using System.Drawing;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductUnitTypesListForm : CrudListFormBase<ProductUnitTypeGetAllQuery, ProductUnitTypeDto, ProductUnitTypeEditForm>
    {
        public ProductUnitTypesListForm() : base("Birim Cinsleri")
        {
            InitializeComponent();
            IncreaseHeaderHeight();
        }

        protected override SvgImage ModuleIcon => SvgIcons.TagIcon;

        protected override string[] SearchFieldNames =>
        [
            nameof(ProductUnitTypeDto.Name)
        ];

        private void IncreaseHeaderHeight()
        {
            // Sadece üst başlık alanının yüksekliği artırıldı, başka hiçbir ayara dokunulmadı.
            if (Controls.Find("pnlHeader", true).FirstOrDefault() is Control headerPanel)
            {
                headerPanel.Height = 88;
            }
        }

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();

            GridColumn[] columns =
            [
                new() { Caption = "Birim Cinsi", FieldName = nameof(ProductUnitTypeDto.Name), Visible = true, Width = 240 }
            ];

            View.Columns.AddRange(columns);
            AddColumnsFromAttributes();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(ProductUnitTypeDto item)
            => new ProductUnitTypeDeleteCommand(item.Id);

        protected override string GetDeleteSummary(ProductUnitTypeDto item) => item.Name;

        protected override bool SupportsRestore => true;

        protected override ProductUnitTypeGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(ProductUnitTypeDto item)
            => new ProductUnitTypeRestoreCommand(item.Id);
    }
}