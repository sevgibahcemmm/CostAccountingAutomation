using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductsListForm : CrudListFormBase<ProductGetAllQuery, ProductDto, ProductEditForm>
    {
        private readonly Dictionary<string, Image?> _imageCache = [];
        private readonly Dictionary<string, Image?> _barcodeImageCache = [];
        private readonly Dictionary<string, Image?> _qrImageCache = [];
        private readonly Dictionary<string, Image?> _slidePreviewCache = [];
        private readonly Dictionary<Guid, int> _slideIndexes = [];
        private IServiceScope? _barcodeScope;

        private sealed class ProductSlideContext(Guid productId)
        {
            public Guid ProductId { get; } = productId;
        }

        public ProductsListForm() : base("Ürünler")
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ = LoadWarehouseFilterAsync();
        }

        protected override SvgImage ModuleIcon => DxIcon.Products;

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
            AddColumnsFromAttributes();

            View.Columns[nameof(ProductDto.MinimumProductLevel)]!.Visible = false;

            RepositoryItemPictureEdit riPicture = new()
            {
                SizeMode = PictureSizeMode.Zoom,
                AllowZoom = DefaultBoolean.True,
                ShowZoomSubMenu = DefaultBoolean.True,
                NullText = "Yok"
            };

            RepositoryItemPictureEdit riBarcodePicture = new()
            {
                SizeMode = PictureSizeMode.Zoom,
                AllowZoom = DefaultBoolean.True,
                ShowZoomSubMenu = DefaultBoolean.True,
                NullText = "Yok"
            };

            RepositoryItemPictureEdit riQrPicture = new()
            {
                SizeMode = PictureSizeMode.Zoom,
                AllowZoom = DefaultBoolean.True,
                ShowZoomSubMenu = DefaultBoolean.True,
                NullText = "Yok"
            };

            GridColumn colImage = new()
            {
                Caption = "Resim",
                FieldName = "PrimaryImageUnbound",
                UnboundDataType = typeof(Image),
                VisibleIndex = 0,
                Width = 45,
                ColumnEdit = riPicture
            };
            colImage.OptionsColumn.FixedWidth = true;

            GridColumn colBarcodeImage = new()
            {
                Caption = "Barkod",
                FieldName = "BarcodeImageUnbound",
                UnboundDataType = typeof(Image),
                VisibleIndex = 2,
                Width = 90,
                ColumnEdit = riBarcodePicture
            };
            colBarcodeImage.OptionsColumn.FixedWidth = true;

            GridColumn colQrImage = new()
            {
                Caption = "Karekod",
                FieldName = "QrImageUnbound",
                UnboundDataType = typeof(Image),
                VisibleIndex = 3,
                Width = 70,
                ColumnEdit = riQrPicture
            };
            colQrImage.OptionsColumn.FixedWidth = true;

            View.Columns.AddRange([colImage, colBarcodeImage, colQrImage]);

            View.RowHeight = 55;
            View.CustomUnboundColumnData += View_CustomUnboundColumnData;
            View.MasterRowEmpty += View_MasterRowEmpty;
            View.MasterRowGetRelationCount += View_MasterRowGetRelationCount;
            View.MasterRowGetRelationName += View_MasterRowGetRelationName;
            View.MasterRowGetChildList += View_MasterRowGetChildList;

            ToolTipController toolTipController = new();
            toolTipController.GetActiveObjectInfo += ToolTipController_GetActiveObjectInfo;
            View.GridControl.ToolTipController = toolTipController;

            ConfigureImagesDetailView();
        }

        private void View_MasterRowEmpty(object? sender, MasterRowEmptyEventArgs e)
        {
            int rowHandle = View.GetRowHandle(e.RowHandle);
            if (rowHandle >= 0 && View.GetRow(rowHandle) is ProductDto product)
            {
                e.IsEmpty = product.Images == null || product.Images.Count == 0;
            }
        }

        private void View_MasterRowGetRelationCount(object? sender, MasterRowGetRelationCountEventArgs e)
        {
            int rowHandle = View.GetRowHandle(e.RowHandle);
            if (rowHandle >= 0 && View.GetRow(rowHandle) is ProductDto product)
            {
                e.RelationCount = product.Images is { Count: > 0 } ? 1 : 0;
            }
        }

        private void View_MasterRowGetRelationName(object? sender, MasterRowGetRelationNameEventArgs e)
        {
            e.RelationName = "Slideshow";
        }

        private void View_MasterRowGetChildList(object? sender, MasterRowGetChildListEventArgs e)
        {
            int rowHandle = View.GetRowHandle(e.RowHandle);
            if (rowHandle < 0 || View.GetRow(rowHandle) is not ProductDto product)
            {
                return;
            }

            if (product.Images is not { Count: > 0 } images)
            {
                return;
            }

            if (!_slideIndexes.ContainsKey(product.Id))
            {
                int primaryIndex = images.FindIndex(i => i.IsPrimary);
                _slideIndexes[product.Id] = primaryIndex >= 0 ? primaryIndex : 0;
            }

            System.Collections.IList childList = new System.Collections.ArrayList();
            childList.Add(new ProductSlideContext(product.Id));
            e.ChildList = childList;
        }

        private void ConfigureImagesDetailView()
        {
            GridView detailView = new(View.GridControl) { Name = "ImagesDetailView" };
            detailView.OptionsBehavior.Editable = false;
            detailView.OptionsBehavior.AutoPopulateColumns = false;
            detailView.OptionsView.ShowColumnHeaders = false;
            detailView.OptionsView.ShowGroupPanel = false;
            detailView.OptionsView.ShowVerticalLines = DefaultBoolean.False;
            detailView.OptionsView.ShowHorizontalLines = DefaultBoolean.False;
            detailView.OptionsSelection.EnableAppearanceFocusedCell = false;
            detailView.OptionsSelection.EnableAppearanceFocusedRow = false;
            detailView.RowHeight = 170;

            RepositoryItemPictureEdit riSlidePicture = new()
            {
                SizeMode = PictureSizeMode.Zoom,
                AllowZoom = DefaultBoolean.True,
                ShowZoomSubMenu = DefaultBoolean.True,
                NullText = "Yok"
            };

            RepositoryItemButtonEdit riPrev = CreateSlideButton(DxIcon.Previous, "Önceki Resim");
            RepositoryItemButtonEdit riNext = CreateSlideButton(DxIcon.Forward, "Sonraki Resim");

            GridColumn colSlidePrev = new()
            {
                Caption = "Önceki",
                FieldName = "SlidePrevUnbound",
                UnboundDataType = typeof(string),
                Visible = true,
                Width = 60,
                ColumnEdit = riPrev
            };
            colSlidePrev.OptionsColumn.FixedWidth = true;
            colSlidePrev.AppearanceCell.TextOptions.VAlignment = VertAlignment.Center;

            GridColumn colSlideImage = new()
            {
                Caption = "Resim",
                FieldName = "SlideImageUnbound",
                UnboundDataType = typeof(Image),
                Visible = true,
                Width = 260,
                ColumnEdit = riSlidePicture
            };

            GridColumn colSlideNext = new()
            {
                Caption = "Sonraki",
                FieldName = "SlideNextUnbound",
                UnboundDataType = typeof(string),
                Visible = true,
                Width = 60,
                ColumnEdit = riNext
            };
            colSlideNext.OptionsColumn.FixedWidth = true;
            colSlideNext.AppearanceCell.TextOptions.VAlignment = VertAlignment.Center;

            GridColumn colSlideInfo = new()
            {
                Caption = "Bilgi",
                FieldName = "SlideInfoUnbound",
                UnboundDataType = typeof(string),
                Visible = true,
                Width = 160
            };
            colSlideInfo.AppearanceCell.TextOptions.VAlignment = VertAlignment.Center;

            detailView.Columns.AddRange([colSlidePrev, colSlideImage, colSlideNext, colSlideInfo]);

            // Detay view klonlandığında (açıldığında) un-bound veri ve tıklama olaylarını yakala
            View.GridControl.ViewRegistered += (sender, e) =>
            {
                if (e.View is GridView targetDetailView && targetDetailView.Name == "ImagesDetailView")
                {
                    targetDetailView.CustomUnboundColumnData -= SlideshowDetailView_CustomUnboundColumnData;
                    targetDetailView.CustomUnboundColumnData += SlideshowDetailView_CustomUnboundColumnData;
                    targetDetailView.RowCellClick -= SlideshowDetailView_OnRowCellClick;
                    targetDetailView.RowCellClick += SlideshowDetailView_OnRowCellClick;
                }
            };

            View.GridControl.LevelTree.Nodes.Add("Slideshow", detailView);
        }

        private RepositoryItemButtonEdit CreateSlideButton(SvgImage icon, string toolTip)
        {
            RepositoryItemButtonEdit button = new()
            {
                TextEditStyle = TextEditStyles.HideTextEditor,
                AutoHeight = false
            };
            button.Buttons[0].Kind = ButtonPredefines.Glyph;
            button.Buttons[0].ToolTip = toolTip;
            button.Buttons[0].ImageOptions.SvgImage = icon;

            return button;
        }

        private void SlideshowDetailView_OnRowCellClick(object? sender, RowCellClickEventArgs e)
        {
            if (sender is not GridView detailView || e.Column is null)
            {
                return;
            }

            switch (e.Column.FieldName)
            {
                case "SlidePrevUnbound":
                    NavigateSlide(detailView, e.RowHandle, -1);
                    break;
                case "SlideNextUnbound":
                    NavigateSlide(detailView, e.RowHandle, +1);
                    break;
            }
        }

        private void NavigateSlide(GridView detailView, int rowHandle, int direction)
        {
            if (detailView.GetRow(rowHandle) is not ProductSlideContext context)
            {
                return;
            }

            ProductDto? product = _allItems.FirstOrDefault(p => p.Id == context.ProductId);
            if (product?.Images is not { Count: > 0 } images)
            {
                return;
            }

            int count = images.Count;
            int next = (GetSlideIndex(context.ProductId, count) + direction + count) % count;
            _slideIndexes[context.ProductId] = next;
            detailView.RefreshData();
        }

        private int GetSlideIndex(Guid productId, int count)
        {
            int index = _slideIndexes.TryGetValue(productId, out int current) ? current : 0;
            return Math.Clamp(index, 0, count - 1);
        }

        private void SlideshowDetailView_CustomUnboundColumnData(object? sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (sender is not GridView detailView || !e.IsGetData)
            {
                return;
            }

            int rowHandle = detailView.GetRowHandle(e.ListSourceRowIndex);
            if (rowHandle < 0 || detailView.GetRow(rowHandle) is not ProductSlideContext context)
            {
                return;
            }

            ProductDto? product = _allItems.FirstOrDefault(p => p.Id == context.ProductId);
            if (product?.Images is not { Count: > 0 } images)
            {
                return;
            }

            int index = GetSlideIndex(context.ProductId, images.Count);
            ProductImageDto slide = images[index];

            switch (e.Column.FieldName)
            {
                case "SlideImageUnbound":
                    if (!string.IsNullOrWhiteSpace(slide.Path))
                    {
                        e.Value = GetOrLoadImage(slide.Path);
                    }
                    break;

                case "SlideInfoUnbound":
                    e.Value = $"{index + 1} / {images.Count}" + (slide.IsPrimary ? "   •   Ana Resim" : "");
                    break;
            }
        }

        private void View_CustomUnboundColumnData(object? sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (!e.IsGetData)
            {
                return;
            }

            ProductDto? product = GetProductFromRow(e.ListSourceRowIndex);
            if (product is null)
            {
                return;
            }

            switch (e.Column.FieldName)
            {
                case "PrimaryImageUnbound":
                    string? imagePath = GetPrimaryImagePath(product);
                    if (!string.IsNullOrWhiteSpace(imagePath))
                    {
                        e.Value = GetOrLoadImage(imagePath);
                    }
                    break;

                case "BarcodeImageUnbound":
                    if (!string.IsNullOrWhiteSpace(product.Barcode))
                    {
                        e.Value = GetOrGenerateBarcodeImage(product.Barcode);
                    }
                    break;

                case "QrImageUnbound":
                    if (!string.IsNullOrWhiteSpace(product.QRCode))
                    {
                        e.Value = GetOrGenerateQrImage(product.QRCode);
                    }
                    break;
            }
        }

        private ProductDto? GetProductFromRow(int listSourceRowIndex)
        {
            if (View.DataSource is System.Collections.IList list && listSourceRowIndex >= 0 && listSourceRowIndex < list.Count)
            {
                return list[listSourceRowIndex] as ProductDto;
            }

            int rowHandle = View.GetRowHandle(listSourceRowIndex);
            return rowHandle >= 0 ? View.GetRow(rowHandle) as ProductDto : null;
        }

        private Image? GetOrGenerateBarcodeImage(string gtin)
        {
            if (_barcodeImageCache.TryGetValue(gtin, out Image? cached))
            {
                return cached;
            }

            Image? img = null;
            try
            {
                _barcodeScope ??= Program.Services.CreateScope();
                IBarcodeGeneratorService svc = _barcodeScope.ServiceProvider.GetRequiredService<IBarcodeGeneratorService>();
                byte[] bytes = svc.GenerateEan13Barcode(gtin);
                using var ms = new MemoryStream(bytes);
                img = new Bitmap(ms);
            }
            catch { }

            _barcodeImageCache[gtin] = img;
            return img;
        }

        private Image? GetOrGenerateQrImage(string qrContent)
        {
            if (_qrImageCache.TryGetValue(qrContent, out Image? cached))
            {
                return cached;
            }

            Image? img = null;
            try
            {
                _barcodeScope ??= Program.Services.CreateScope();
                IBarcodeGeneratorService svc = _barcodeScope.ServiceProvider.GetRequiredService<IBarcodeGeneratorService>();
                byte[] bytes = svc.GenerateQrCode(qrContent);
                using var ms = new MemoryStream(bytes);
                img = new Bitmap(ms);
            }
            catch { }

            _qrImageCache[qrContent] = img;
            return img;
        }

        private static string? GetPrimaryImagePath(ProductDto product)
        {
            if (product.Images is { Count: > 0 } images)
            {
                var primary = images.FirstOrDefault(i => i.IsPrimary);
                if (primary != null && !string.IsNullOrWhiteSpace(primary.Path))
                {
                    return primary.Path;
                }
            }
            return null;
        }

        private Image? GetSlideImagePreview(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            const string cacheKeyPrefix = "preview|";
            if (_slidePreviewCache.TryGetValue(cacheKeyPrefix + relativePath, out Image? cached))
            {
                return cached;
            }

            Image? full = GetOrLoadImage(relativePath);
            if (full is null)
            {
                return null;
            }

            const int maxWidth = 480;
            const int maxHeight = 400;
            Image? preview = full;
            double ratio = Math.Min((double)maxWidth / full.Width, (double)maxHeight / full.Height);
            if (ratio < 1.0)
            {
                int w = Math.Max(1, (int)(full.Width * ratio));
                int h = Math.Max(1, (int)(full.Height * ratio));
                Bitmap scaled = new(w, h);
                using (Graphics g = Graphics.FromImage(scaled))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(full, 0, 0, w, h);
                }
                preview = scaled;
            }

            _slidePreviewCache[cacheKeyPrefix + relativePath] = preview;
            return preview;
        }

        private Image? GetOrLoadImage(string relativePath)
        {
            if (_imageCache.TryGetValue(relativePath, out Image? cached))
            {
                return cached;
            }

            Image? img = null;
            try
            {
                string fullPath = StorageRoot.Resolve(relativePath);
                if (File.Exists(fullPath))
                {
                    using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    using var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    img = new Bitmap(ms);
                }
            }
            catch { }

            _imageCache[relativePath] = img;
            return img;
        }

        private void ToolTipController_GetActiveObjectInfo(object? sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            if (e.SelectedControl != View.GridControl)
            {
                return;
            }

            if (View.GridControl.GetViewAt(e.ControlMousePosition) is not GridView hitView)
            {
                return;
            }

            var hitInfo = hitView.CalcHitInfo(e.ControlMousePosition);
            if (!hitInfo.InRowCell || hitInfo.Column is null)
            {
                return;
            }

            int rowHandle = hitView.GetRowHandle(hitInfo.RowHandle);
            if (rowHandle < 0)
            {
                return;
            }

            if (hitView == View && hitView.GetRow(rowHandle) is ProductDto product)
            {
                SuperToolTip? stp = hitInfo.Column.FieldName switch
                {
                    "PrimaryImageUnbound" => BuildImageToolTip(product),
                    "BarcodeImageUnbound" => BuildBarcodeToolTip(product),
                    "QrImageUnbound" => BuildQrToolTip(product),
                    _ => null
                };

                if (stp is not null)
                {
                    e.Info = new ToolTipControlInfo(new CellToolTipInfo(hitInfo.RowHandle, hitInfo.Column, "cell"), "")
                    {
                        SuperTip = stp
                    };
                }
            }
            else if (hitView.Name == "ImagesDetailView" && hitInfo.Column.FieldName == "SlideImageUnbound"
                     && hitView.GetRow(rowHandle) is ProductSlideContext slideContext
                     && _allItems.FirstOrDefault(p => p.Id == slideContext.ProductId) is ProductDto slideProduct
                     && slideProduct.Images is { Count: > 0 } slideImages)
            {
                int slideIndex = GetSlideIndex(slideContext.ProductId, slideImages.Count);
                ProductImageDto slide = slideImages[slideIndex];
                Image? preview = GetSlideImagePreview(slide.Path);
                if (preview is null)
                {
                    return;
                }

                string title = $"{slideIndex + 1} / {slideImages.Count}" + (slide.IsPrimary ? "   •   Ana Resim" : "   •   Ürün Resmi");
                e.Info = new ToolTipControlInfo(new CellToolTipInfo(hitInfo.RowHandle, hitInfo.Column, "cell"), "")
                {
                    Title = title,
                    ToolTipImage = preview
                };
            }
        }

        private static SuperToolTip BuildImageToolTip(ProductDto product)
        {
            SuperToolTip stp = new();
            ToolTipItem item = new();
            item.Text = product.Name;
            stp.Items.Add(item);
            return stp;
        }

        private SuperToolTip BuildBarcodeToolTip(ProductDto product)
        {
            SuperToolTip stp = new();

            ToolTipItem textItem = new();
            textItem.Text = "Barkod: " + (product.Barcode ?? "-");
            stp.Items.Add(textItem);

            return stp;
        }

        private SuperToolTip BuildQrToolTip(ProductDto product)
        {
            SuperToolTip stp = new();

            ToolTipItem textItem = new();
            textItem.Text = product.QRCode ?? "-";
            stp.Items.Add(textItem);

            return stp;
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(ProductDto item)
            => new ProductDeleteCommand(item.Id);

        /// <summary>Seçili ürünler tek transaction'da silinir.</summary>
        protected override IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<ProductDto> items)
            => new BulkDeleteProductsCommand(items.Select(item => item.Id).ToList());

        protected override string DeleteItemLabel => "ürün";

        /// <summary>
        /// Stok hareketi denetimi tek sorguda yapılır: seçili ürünler için
        /// hareket görenler ürün repository'sinin silme denetim metoduyla bir
        /// kerede çekilir.
        /// </summary>
        protected override async Task<List<ProductDto>> GetUndeletableAsync(
            List<ProductDto> selected, CancellationToken cancellationToken)
        {
            if (selected.Count == 0)
            {
                return [];
            }

            HashSet<Guid> ids = selected.Select(item => item.Id).ToHashSet();

            using var scope = Program.Services.CreateScope();
            IProductRepository repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            DeletionCheck check = await repository.GetDeletionCheckAsync(ids, cancellationToken);

            if (check.MovementIds.Count == 0)
            {
                return [];
            }

            HashSet<Guid> blocked = check.MovementIds.ToHashSet();
            return selected.Where(item => blocked.Contains(item.Id)).ToList();
        }

        protected override string GetDeleteSummary(ProductDto item) => item.Name;

        protected override bool SupportsRestore => true;

        protected override ProductGetAllQuery BuildListQuery()
            => new(WarehouseId: SelectedFilterGuid, OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(ProductDto item)
            => new ProductRestoreCommand(item.Id);
    }
}