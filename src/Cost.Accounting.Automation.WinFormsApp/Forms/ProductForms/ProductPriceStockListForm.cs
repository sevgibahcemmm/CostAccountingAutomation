using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
using System;
using System.Linq;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductPriceStockListForm : CrudListFormBase<ProductGetAllQuery, ProductDto, ProductEditForm>
    {
        public ProductPriceStockListForm() : base("Fiyat & Stok Listesi")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.PriceStock;

        protected override bool AllowDelete => false;

        protected override string[] SearchFieldNames =>
        [
            nameof(ProductDto.Name),
            nameof(ProductDto.ProductCode),
            nameof(ProductDto.CategoryName),
            nameof(ProductDto.WarehouseName),
            nameof(ProductDto.ProductUnitTypeName)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();

            View.Columns[nameof(ProductDto.TaxRateRate)]!.Visible = false;
            View.Columns[nameof(ProductDto.ChartOfAccountCode)]!.Visible = false;

            GridColumn purchasePrice = new()
            {
                Caption = "Alış Fiyatı",
                FieldName = "PurchasePriceUnbound",
                UnboundDataType = typeof(decimal),
                Visible = true,
                Width = 110,
                DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" }
            };
            purchasePrice.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            purchasePrice.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Far;

            GridColumn salePrice = new()
            {
                Caption = "Satış Fiyatı",
                FieldName = "SalePriceUnbound",
                UnboundDataType = typeof(decimal),
                Visible = true,
                Width = 110,
                DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" }
            };
            salePrice.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            salePrice.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Far;
            salePrice.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            salePrice.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Far;

            int insertIndex = View.Columns[nameof(ProductDto.StockQuantity)]!.VisibleIndex + 1;
            View.Columns.Add(purchasePrice);
            View.Columns.Add(salePrice);
            purchasePrice.VisibleIndex = insertIndex;
            salePrice.VisibleIndex = insertIndex + 1;

            View.CustomUnboundColumnData += View_CustomUnboundColumnData;
        }

        private void View_CustomUnboundColumnData(object? sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (!e.IsGetData || e.ListSourceRowIndex < 0)
            {
                return;
            }

            if (View.GetRow(e.ListSourceRowIndex) is not ProductDto product)
            {
                return;
            }

            switch (e.Column.FieldName)
            {
                case "PurchasePriceUnbound":
                    e.Value = GetLatestPrice(product, ProductPriceType.Purchase);
                    break;
                case "SalePriceUnbound":
                    e.Value = GetLatestPrice(product, ProductPriceType.Sale);
                    break;
            }
        }

        private static decimal? GetLatestPrice(ProductDto product, ProductPriceType priceType)
        {
            ProductPriceDto? latest = product.Prices
                .Where(p => p.PriceType == priceType)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            return latest?.UnitPrice;
        }

        protected override ProductGetAllQuery BuildListQuery()
            => new(WarehouseId: SelectedFilterGuid, OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(ProductDto item)
            => throw new NotSupportedException("Fiyat & Stok Listesi salt okunurdur.");
    }
}