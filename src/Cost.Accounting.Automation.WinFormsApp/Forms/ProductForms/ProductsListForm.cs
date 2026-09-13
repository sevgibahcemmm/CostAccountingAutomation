using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
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
        private IBarcodeGeneratorService? _barcodeService;

        public ProductsListForm() : base("Ürünler")
        {
            Load += ProductsListForm_Load;
        }

        private void ProductsListForm_Load(object? sender, EventArgs e)
        {
            foreach (Control ctrl in Controls)
            {
                if (ctrl.Height < 100 && (ctrl.BackColor == Color.Black || ctrl.Controls.Cast<Control>().Any(c => c.Text.Contains("listeleniyor") || c.Text.Contains("Ürünler"))))
                {
                    ctrl.Height = Math.Max(ctrl.Height, 82);
                    foreach (Control subCtrl in ctrl.Controls)
                    {
                        if (subCtrl is LabelControl lbl && lbl.Text.Contains("listeleniyor"))
                        {
                            lbl.Top = ctrl.Height - lbl.Height - 8;
                        }
                    }
                }
            }
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
                Visible = true,
                Width = 45,
                ColumnEdit = riPicture
            };
            colImage.OptionsColumn.FixedWidth = true;

            GridColumn colBarcodeImage = new()
            {
                Caption = "Barkod",
                FieldName = "BarcodeImageUnbound",
                UnboundDataType = typeof(Image),
                Visible = true,
                Width = 90,
                ColumnEdit = riBarcodePicture
            };
            colBarcodeImage.OptionsColumn.FixedWidth = true;

            GridColumn colQrImage = new()
            {
                Caption = "Karekod",
                FieldName = "QrImageUnbound",
                UnboundDataType = typeof(Image),
                Visible = true,
                Width = 70,
                ColumnEdit = riQrPicture
            };
            colQrImage.OptionsColumn.FixedWidth = true;

            GridColumn[] columns =
            [
                colImage,
                new() { Caption = "Ürün Adı", FieldName = nameof(ProductDto.Name), Visible = true, Width = 240 },
                colBarcodeImage,
                colQrImage,
                new() { Caption = "Kategori", FieldName = nameof(ProductDto.CategoryName), Visible = true, Width = 150 },
                new() { Caption = "Depo", FieldName = nameof(ProductDto.WarehouseName), Visible = true, Width = 130 },
                new() { Caption = "Birim", FieldName = nameof(ProductDto.ProductUnitTypeName), Visible = true, Width = 70 },
                new() { Caption = "KDV", FieldName = nameof(ProductDto.TaxRateRate), Visible = true, Width = 70, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "p0" } },
                new() { Caption = "Stok", FieldName = nameof(ProductDto.StockQuantity), Visible = true, Width = 90, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" } },
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
            codeColumn.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            codeColumn.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Far;
            View.Columns.Add(codeColumn);

            View.RowHeight = 55;
            View.CustomUnboundColumnData += View_CustomUnboundColumnData;
            View.MasterRowEmpty += View_MasterRowEmpty;

            ToolTipController toolTipController = new();
            toolTipController.GetActiveObjectInfo += ToolTipController_GetActiveObjectInfo;
            View.GridControl.ToolTipController = toolTipController;

            ConfigureImagesDetailView();

            AddColumnsFromAttributes();
        }

        private void View_MasterRowEmpty(object? sender, MasterRowEmptyEventArgs e)
        {
            int rowHandle = View.GetRowHandle(e.RowHandle);
            if (rowHandle >= 0 && View.GetRow(rowHandle) is ProductDto product)
            {
                e.IsEmpty = product.Images == null || product.Images.Count == 0;
            }
        }

        private void ConfigureImagesDetailView()
        {
            GridView detailView = new(View.GridControl) { Name = "ImagesDetailView" };
            detailView.OptionsBehavior.Editable = false;
            detailView.RowHeight = 35;

            RepositoryItemPictureEdit riDetailPicture = new()
            {
                SizeMode = PictureSizeMode.Zoom,
                NullText = "Yok"
            };

            GridColumn colDetailImage = new()
            {
                Caption = "Resim",
                FieldName = "DetailImageUnbound",
                UnboundDataType = typeof(Image),
                Visible = true,
                Width = 45,
                ColumnEdit = riDetailPicture
            };
            colDetailImage.OptionsColumn.FixedWidth = true;

            GridColumn colDetailIsPrimary = new()
            {
                Caption = "Ana Resim",
                FieldName = nameof(ProductImageDto.IsPrimary),
                Visible = true,
                Width = 100
            };

            detailView.Columns.AddRange([colDetailImage, colDetailIsPrimary]);

            // Detay view klonlandığında (açıldığında) un-bound veri olayını yakala
            View.GridControl.ViewRegistered += (sender, e) =>
            {
                if (e.View is GridView targetDetailView && targetDetailView.Name == "ImagesDetailView")
                {
                    targetDetailView.CustomUnboundColumnData += (s, ev) =>
                    {
                        if (ev.Column.FieldName == "DetailImageUnbound" && ev.IsGetData)
                        {
                            int rowHandle = targetDetailView.GetRowHandle(ev.ListSourceRowIndex);
                            if (rowHandle >= 0 && targetDetailView.GetRow(rowHandle) is ProductImageDto imgDto && !string.IsNullOrWhiteSpace(imgDto.Path))
                            {
                                ev.Value = GetOrLoadImage(imgDto.Path);
                            }
                        }
                    };
                }
            };

            View.GridControl.LevelTree.Nodes.Add("Images", detailView);
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
                IBarcodeGeneratorService svc = _barcodeService ??= Program.Services.CreateScope().ServiceProvider.GetRequiredService<IBarcodeGeneratorService>();
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
                IBarcodeGeneratorService svc = _barcodeService ??= Program.Services.CreateScope().ServiceProvider.GetRequiredService<IBarcodeGeneratorService>();
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
            else if (hitView.Name == "ImagesDetailView" && hitInfo.Column.FieldName == "DetailImageUnbound"
                     && hitView.GetRow(rowHandle) is ProductImageDto imgDto)
            {
                SuperToolTip stp = new();
                ToolTipItem item = new();
                item.Text = imgDto.IsPrimary ? "Ana Resim" : "Ürün Resmi";
                stp.Items.Add(item);

                e.Info = new ToolTipControlInfo(new CellToolTipInfo(hitInfo.RowHandle, hitInfo.Column, "cell"), "")
                {
                    SuperTip = stp
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

        protected override string GetDeleteSummary(ProductDto item) => item.Name;

        protected override bool SupportsRestore => true;

        protected override ProductGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(ProductDto item)
            => new ProductRestoreCommand(item.Id);
    }
}