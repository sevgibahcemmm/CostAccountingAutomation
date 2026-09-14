using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductUnitTypesListForm : CrudListFormBase<ProductUnitTypeGetAllQuery, ProductUnitTypeDto, ProductUnitTypeEditForm>
    {
        public ProductUnitTypesListForm() : base("Birim Cinsleri")
        {
            InitializeComponent();
        }

        protected override SvgImage ModuleIcon => SvgIcons.TagIcon;

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