using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.StockCounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Reports.StockCountListReports;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductPriceStockListForm : CrudListFormBase<ProductCatalogListQuery, ProductCatalogDto, ProductEditForm>
    {
        public ProductPriceStockListForm() : base("Fiyat & Stok Listesi")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.PriceStock;

        protected override bool AllowDelete => false;

        protected override bool SupportsStockCountListReport => true;

        protected override async Task ShowStockCountListReportAsync(ProductCatalogDto? item)
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var accountsResult = await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None);
            if (!accountsResult.IsSuccessful || accountsResult.Data is null)
            {
                ToastHelper.Show("Atölye / depo bilgileri yüklenemedi.", ToastType.Error);
                return;
            }

            using var form = new StockCountPromptForm(accountsResult.Data, accountsResult.Data);
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            List<StockCountReportRowDto> rows = await mediator.Send(
                new StockCountListReportQuery(form.Mode, form.AllGroups, form.GroupIds, form.AsOfDate),
                CancellationToken.None);

            if (rows.Count == 0)
            {
                ToastHelper.Show("Seçilen kriterlere uygun ürün bulunamadı.", ToastType.Warning);
                return;
            }

            CompanyDto company = await LoadCompanyAsync();

            var report = new StockCountListReport();
            report.SetData(form.Mode, rows, company.Letterhead);
            report.PrintReport();
        }

        private static async Task<CompanyDto> LoadCompanyAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                SessionClaimContext session = Program.Services.GetRequiredService<SessionClaimContext>();

                var result = await mediator.Send(new CompanyGetQuery(session.GetCompanyId()), CancellationToken.None);
                if (result.IsSuccessful && result.Data is not null)
                {
                    return result.Data;
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("StockCountListReport.Company", ex);
            }

            return new CompanyDto();
        }

        protected override string[] SearchFieldNames =>
        [
            nameof(ProductCatalogDto.Name),
            nameof(ProductCatalogDto.ProductCode),
            nameof(ProductCatalogDto.CategoryName),
            nameof(ProductCatalogDto.WarehouseName),
            nameof(ProductCatalogDto.ProductUnitTypeName)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.Columns[nameof(ProductCatalogDto.WarehouseName)]!.Visible = false;
            ConfigureWarehouseGrouping(nameof(ProductCatalogDto.WarehouseGroup));

            View.Columns[nameof(ProductCatalogDto.TaxRateRate)]!.Visible = false;
            View.Columns[nameof(ProductCatalogDto.ChartOfAccountCode)]!.Visible = false;

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

            int insertIndex = View.Columns[nameof(ProductCatalogDto.StockQuantity)]!.VisibleIndex + 1;
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

            if (View.GetRow(e.ListSourceRowIndex) is not ProductCatalogDto product)
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

        private static decimal? GetLatestPrice(ProductCatalogDto product, ProductPriceType priceType)
        {
            ProductPriceDto? latest = product.Prices
                .Where(p => p.PriceType == priceType)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            return latest?.UnitPrice;
        }

        protected override ProductCatalogListQuery BuildListQuery()
            => new(WarehouseId: SelectedFilterGuid, OnlyDeleted: ShowDeleted);

        protected override bool EnrichReplacesBaseQuery => true;

        protected override Task<IReadOnlyList<ProductCatalogDto>> EnrichAsync(List<ProductCatalogDto> items, CancellationToken cancellationToken)
            => Task.Run(async () =>
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                return (IReadOnlyList<ProductCatalogDto>)(await mediator.Send(
                    new ProductCatalogGetAllQuery(WarehouseId: SelectedFilterGuid, OnlyDeleted: ShowDeleted),
                    cancellationToken));
            }, cancellationToken);

        protected override IRequest<Result<string>> BuildDeleteCommand(ProductCatalogDto item)
            => throw new NotSupportedException("Fiyat & Stok Listesi salt okunurdur.");

        protected override ProductEditForm CreateEditEditor(ProductCatalogDto item)
            => new(new ProductDto
            {
                Id = item.Id,
                Name = item.Name,
                ProductCode = item.ProductCode,
                CategoryId = item.CategoryId
            });
    }
}