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
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
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

            // Modern grup satırı görünümü (renkler skin'e göre çözülür)
            View.Appearance.GroupRow.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            View.Appearance.GroupRow.ForeColor = SkinTheme.Text;
            View.Appearance.GroupRow.BackColor = SkinTheme.SurfaceMuted(SkinTheme.SurfaceOf(this));
            View.Appearance.GroupRow.BackColor2 = SkinTheme.SurfaceMuted(SkinTheme.SurfaceOf(this));
            View.Appearance.GroupRow.Options.UseFont = true;
            View.Appearance.GroupRow.Options.UseForeColor = true;
            View.Appearance.GroupRow.Options.UseBackColor = true;

            View.Columns[nameof(ProductCatalogDto.TaxRateRate)]!.Visible = false;
            View.Columns[nameof(ProductCatalogDto.ChartOfAccountCode)]!.Visible = false;

            GridColumn purchasePrice = new()
            {
                Caption = "Alış Fiyatı",
                FieldName = "PurchasePriceUnbound",
                UnboundDataType = typeof(decimal?),
                Visible = true,
                Width = 110,
                DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" }
            };
            AlignRight(purchasePrice);

            GridColumn salePrice = new()
            {
                Caption = "Satış Fiyatı",
                FieldName = "SalePriceUnbound",
                UnboundDataType = typeof(decimal?),
                Visible = true,
                Width = 110,
                DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" }
            };
            AlignRight(salePrice);

            GridColumn costPrice = new()
            {
                Caption = "Maliyet Fiyatı",
                FieldName = "CostPriceUnbound",
                UnboundDataType = typeof(decimal?),
                Visible = true,
                Width = 110,
                DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" }
            };
            AlignRight(costPrice);

            int insertIndex = View.Columns[nameof(ProductCatalogDto.StockQuantity)]!.VisibleIndex + 1;
            View.Columns.Add(purchasePrice);
            View.Columns.Add(salePrice);
            View.Columns.Add(costPrice);
            purchasePrice.VisibleIndex = insertIndex;
            salePrice.VisibleIndex = insertIndex + 1;
            costPrice.VisibleIndex = insertIndex + 2;

            View.CustomUnboundColumnData += View_CustomUnboundColumnData;
            ConfigureMovementDetailView();
        }

        private static void AlignRight(GridColumn column)
        {
            column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            column.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Far;
        }

        /// <summary>
        /// Ürün satırının altında belgeli stok hareketlerini gösterir: hareket ve belge tarihi,
        /// belge numarası/türü, neden, miktar, birim fiyat, tutar ve açıklama. Fiyat geçmişi ise
        /// çift tıklamada açılan ürün detay formundadır.
        /// </summary>
        private void ConfigureMovementDetailView()
        {
            const string relationName = "Belgeli Stok Hareketleri";

            GridView movementView = new(BaseGrid)
            {
                Name = "BelgeliStokHareketleriView",
                ViewCaption = relationName
            };
            movementView.OptionsBehavior.Editable = false;
            movementView.OptionsDetail.AllowOnlyOneMasterRowExpanded = true;
            movementView.OptionsView.ShowGroupPanel = false;
            movementView.OptionsView.ShowHorizontalLines = DefaultBoolean.False;
            movementView.RowHeight = 26;
            movementView.Appearance.HeaderPanel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            movementView.Appearance.HeaderPanel.Options.UseFont = true;

            GridColumnFactory.ConfigureFromAttributes(movementView, typeof(ProductStockMovementDetailDto));

            View.MasterRowGetRelationCount += (_, e) => e.RelationCount = 1;
            View.MasterRowGetRelationName += (_, e) => e.RelationName = relationName;
            View.MasterRowGetChildList += (_, e) =>
                e.ChildList = (View.GetRow(e.RowHandle) as ProductCatalogDto)?.Movements;
            View.MasterRowEmpty += (_, e) =>
            {
                if (View.GetRow(e.RowHandle) is ProductCatalogDto product)
                {
                    e.IsEmpty = product.Movements is null || product.Movements.Count == 0;
                }
            };

            BaseGrid.LevelTree.Nodes.Add(new GridLevelNode
            {
                RelationName = relationName,
                LevelTemplate = movementView
            });

            // Eski sürümden kalan "Prices" / "Fiyatlar" detay düğümleri varsa temizlenir,
            // aksi halde detay bandında eski başlık görünür.
            foreach (GridLevelNode stale in BaseGrid.LevelTree.Nodes
                         .Where(n => !ReferenceEquals(n, BaseGrid.LevelTree.Nodes[^1]))
                         .Where(n => n.RelationName is "Prices" or "Fiyatlar" or "Fiyat Geçmişi")
                         .ToList())
            {
                BaseGrid.LevelTree.Nodes.Remove(stale);
            }

            CrashLog.Write(
                "ProductPriceStockList.MasterDetail",
                $"dugum={BaseGrid.LevelTree.Nodes.Count} sonDugum='{BaseGrid.LevelTree.Nodes[^1].RelationName}' " +
                $"view='{BaseGrid.LevelTree.Nodes[^1].LevelTemplate?.Name}' " +
                $"caption='{BaseGrid.LevelTree.Nodes[^1].LevelTemplate?.ViewCaption}' " +
                $"hareketSayisi={GetDataRowCount(View)}");

            View.RefreshData();
        }

        private int GetDataRowCount(GridView view)
        {
            try
            {
                return view.DataSource is System.Collections.ICollection collection ? collection.Count : view.DataRowCount;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("ProductPriceStockList.RowCount", ex);
                return -1;
            }
        }

        private void View_CustomUnboundColumnData(object? sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (!e.IsGetData || e.ListSourceRowIndex < 0)
            {
                return;
            }

            if (GetProductFromRow(e.ListSourceRowIndex) is not ProductCatalogDto product)
            {
                return;
            }

            switch (e.Column.FieldName)
            {
                case "PurchasePriceUnbound":
                    // Fiyat kartı girilmemişse (ör. 151/152) maliyet fiyatı gösterilir.
                    e.Value = GetLatestPrice(product, ProductPriceType.Purchase) ?? product.CostPrice;
                    break;
                case "SalePriceUnbound":
                    // Fiyat kartı girilmemişse boş satır yerine 0,00 gösterilir.
                    e.Value = GetLatestPrice(product, ProductPriceType.Sale) ?? 0m;
                    break;
                case "CostPriceUnbound":
                    e.Value = product.CostPrice;
                    break;
            }
        }

        /// <summary>
        /// Liste satır indisinden ilgili ürünü bulur. Görünüm depoya göre gruplandığı için
        /// liste indeksi doğrudan row handle değildir; aksi halde başka ürünün fiyatı okunur.
        /// </summary>
        private ProductCatalogDto? GetProductFromRow(int listSourceRowIndex)
        {
            if (View.DataSource is System.Collections.IList list && listSourceRowIndex < list.Count)
            {
                return list[listSourceRowIndex] as ProductCatalogDto;
            }

            int rowHandle = View.GetRowHandle(listSourceRowIndex);
            return rowHandle >= 0 ? View.GetRow(rowHandle) as ProductCatalogDto : null;
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
                    new ProductCatalogGetAllQuery(
                        WarehouseId: SelectedFilterGuid,
                        OnlyDeleted: ShowDeleted,
                        IncludeMovements: true,
                        MovementLimit: MasterDetailMovementLimit),
                    cancellationToken));
            }, cancellationToken);

        /// <summary>Satır altı detayda ürün başına gösterilecek en yeni hareket sayısı.</summary>
        private const int MasterDetailMovementLimit = 100;

        private IDisposable? skinBinding;

        /// <summary>Çift tıklama ve araç çubuğu butonu stok kartını değil, açıklayıcı salt okunur detayı açar.</summary>
        protected override bool DoubleClickOpensEditor => false;

        protected override string EditButtonCaption => "Detay";

        protected override SvgImage EditButtonIcon => DxIcon.Eye;

        protected override async Task ShowItemDetailAsync(ProductCatalogDto item)
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await mediator.Send(new ProductDetailQuery(item.Id), CancellationToken.None);
            if (!result.IsSuccessful || result.Data is null)
            {
                ToastHelper.Show(AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error);
                return;
            }

            using var form = new ProductDetailForm(result.Data);
            form.ShowDialog(this);
        }

        /// <summary>
        /// Stok kartı bu listeden yalnızca satış fiyatı ve fotoğraf için açılır; ürün künyesi
        /// (ad, depo, kategori, birim, KDV, açıklama) kilitlidir.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = $"{Text} — {ProductDetailForm.BuildStamp()}";
            skinBinding = SkinTheme.Bind(ApplySkin);

            var btnPrice = new SimpleButton
            {
                Text = "Satış Fiyatı / Fotoğraf",
                Width = 190,
                ImageOptions =
                {
                    SvgImage = DxIcon.Tag,
                    SvgImageSize = new Size(18, 18),
                    ImageToTextAlignment = ImageAlignToText.LeftCenter
                }
            };
            btnPrice.Click += BtnPrice_Click;
            AddToolbarButton(btnPrice, index: 2);
        }

        /// <summary>Liste görünümünün renklerini aktif skine göre yeniler.</summary>
        private void ApplySkin()
        {
            Color muted = SkinTheme.SurfaceMuted(SkinTheme.SurfaceOf(this));

            View.Appearance.GroupRow.ForeColor = SkinTheme.Text;
            View.Appearance.GroupRow.BackColor = muted;
            View.Appearance.GroupRow.BackColor2 = muted;
            View.RefreshData();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                skinBinding?.Dispose();
            }

            base.Dispose(disposing);
        }

        private async void BtnPrice_Click(object? sender, EventArgs e)
        {
            if (View.GetFocusedRow() is not ProductCatalogDto item)
            {
                ToastHelper.Show("Önce bir ürün satırı seçin.", ToastType.Warning);
                return;
            }

            using var editor = CreateEditEditor(item, priceAndPhotosOnly: true);
            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                await ReloadAsync();
            }
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(ProductCatalogDto item)
            => throw new NotSupportedException("Fiyat & Stok Listesi salt okunurdur.");

        protected override ProductEditForm CreateEditEditor(ProductCatalogDto item)
            => CreateEditEditor(item, priceAndPhotosOnly: false);

        private static ProductEditForm CreateEditEditor(ProductCatalogDto item, bool priceAndPhotosOnly)
            => new(new ProductDto
            {
                Id = item.Id,
                Name = item.Name,
                ProductCode = item.ProductCode,
                CategoryId = item.CategoryId
            }, priceAndPhotosOnly);
    }
}