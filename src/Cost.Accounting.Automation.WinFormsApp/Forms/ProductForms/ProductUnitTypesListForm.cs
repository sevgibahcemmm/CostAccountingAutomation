using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using DevExpress.Utils.Svg;
using TS.MediatR;
using TS.Result;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductUnitTypesListForm : CrudListFormBase<ProductUnitTypeGetAllQuery, ProductUnitTypeDto, ProductUnitTypeEditForm>
    {
        public ProductUnitTypesListForm() : base("Birim Cinsleri")
        {
            InitializeComponent();
        }

        protected override SvgImage ModuleIcon => DxIcon.Tag;

        protected override string[] SearchFieldNames =>
        [
            nameof(ProductUnitTypeDto.Name)
        ];

        protected override void ConfigureColumns()
        {
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