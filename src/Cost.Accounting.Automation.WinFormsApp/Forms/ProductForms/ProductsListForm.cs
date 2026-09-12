using System.Drawing;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductsListForm : CrudListFormBase<ProductGetAllQuery, ProductDto, ProductEditForm>
    {
        public ProductsListForm() : base("Ürünler")
        {
        }

        protected override SvgImage ModuleIcon => SvgIcons.Modules[2];

        protected override string[] SearchFieldNames =>
        [
            nameof(ProductDto.Name),
            nameof(ProductDto.ProductCode),
            nameof(ProductDto.Barcode),
            nameof(ProductDto.QRCode),
            nameof(ProductDto.CategoryCode),
            nameof(ProductDto.CategoryName),
            nameof(ProductDto.WarehouseName),
            nameof(ProductDto.ProductUnitTypeName)
        ];

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();

            GridColumn[] columns =
            [
                new() { Caption = "Ürün Adı", FieldName = nameof(ProductDto.Name), Visible = true, Width = 240 },
                new() { Caption = "Barkod", FieldName = nameof(ProductDto.Barcode), Visible = true, Width = 130 },
                new() { Caption = "Kategori", FieldName = nameof(ProductDto.CategoryName), Visible = true, Width = 150 },
                new() { Caption = "Depo", FieldName = nameof(ProductDto.WarehouseName), Visible = true, Width = 130 },
                new() { Caption = "Birim", FieldName = nameof(ProductDto.ProductUnitTypeName), Visible = true, Width = 70 },
                new() { Caption = "KDV", FieldName = nameof(ProductDto.TaxRate), Visible = true, Width = 70, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Custom, FormatString = "p0" } },
                new() { Caption = "Stok", FieldName = nameof(ProductDto.StockQuantity), Visible = true, Width = 90, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Custom, FormatString = "n2" } },
                new() { Caption = "Hesap Planı No", FieldName = nameof(ProductDto.ChartOfAccountCode), Visible = true, Width = 170 }
            ];

            View.Columns.AddRange(columns);

            GridColumn codeColumn = new()
            {
                Caption = "Ürün Kodu",
                FieldName = nameof(ProductDto.ProductCode),
                Visible = true,
                Width = 170
            };
            codeColumn.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            codeColumn.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            View.Columns.Add(codeColumn);

            AddColumnsFromAttributes();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(ProductDto item)
            => new ProductDeleteCommand(item.Id);

        protected override string GetDeleteSummary(ProductDto item) => item.Name;

        protected override bool SupportsRestore => true;

        protected override ProductGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(ProductDto item)
            => new ProductRestoreCommand(item.Id);
    }
}