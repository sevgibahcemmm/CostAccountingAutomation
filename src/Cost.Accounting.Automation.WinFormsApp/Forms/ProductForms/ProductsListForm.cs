using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
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

            GridColumn[] columns =
            [
                colImage,
                new() { Caption = "Ürün Adı", FieldName = nameof(ProductDto.Name), Visible = true, Width = 240 },
                new() { Caption = "Barkod", FieldName = nameof(ProductDto.Barcode), Visible = true, Width = 130 },
                new() { Caption = "Kategori", FieldName = nameof(ProductDto.CategoryName), Visible = true, Width = 150 },
                new() { Caption = "Depo", FieldName = nameof(ProductDto.WarehouseName), Visible = true, Width = 130 },
                new() { Caption = "Birim", FieldName = nameof(ProductDto.ProductUnitTypeName), Visible = true, Width = 70 },
                new() { Caption = "KDV", FieldName = nameof(ProductDto.TaxRate), Visible = true, Width = 70, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "p0" } },
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

            View.RowHeight = 38;
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
            if (e.Column.FieldName == "PrimaryImageUnbound" && e.IsGetData)
            {
                ProductDto? product = null;
                if (View.DataSource is System.Collections.IList list && e.ListSourceRowIndex >= 0 && e.ListSourceRowIndex < list.Count)
                {
                    product = list[e.ListSourceRowIndex] as ProductDto;
                }
                else
                {
                    int rowHandle = View.GetRowHandle(e.ListSourceRowIndex);
                    if (rowHandle >= 0)
                    {
                        product = View.GetRow(rowHandle) as ProductDto;
                    }
                }

                if (product != null)
                {
                    string? imagePath = GetPrimaryImagePath(product);
                    if (!string.IsNullOrWhiteSpace(imagePath))
                    {
                        e.Value = GetOrLoadImage(imagePath);
                    }
                }
            }
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
                string fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
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
            if (e.SelectedControl == View.GridControl)
            {
                if (View.GridControl.GetViewAt(e.ControlMousePosition) is GridView hitView)
                {
                    var hitInfo = hitView.CalcHitInfo(e.ControlMousePosition);
                    if (hitInfo.InRowCell && hitInfo.Column != null)
                    {
                        if (hitView == View && hitInfo.Column.FieldName == "PrimaryImageUnbound")
                        {
                            int rowHandle = hitView.GetRowHandle(hitInfo.RowHandle);
                            if (rowHandle >= 0 && hitView.GetRow(rowHandle) is ProductDto product)
                            {
                                string? imagePath = GetPrimaryImagePath(product);
                                Image? img = imagePath != null ? GetOrLoadImage(imagePath) : null;
                                if (img != null)
                                {
                                    Image largeImg = new Bitmap(img, new Size(200, 200));
                                    SuperToolTip stp = new();
                                    ToolTipItem item = new();
                                    item.ImageOptions.Image = largeImg;
                                    item.Text = product.Name;
                                    stp.Items.Add(item);

                                    e.Info = new ToolTipControlInfo(new CellToolTipInfo(hitInfo.RowHandle, hitInfo.Column, "cell"), "")
                                    {
                                        SuperTip = stp
                                    };
                                }
                            }
                        }
                        else if (hitView.Name == "ImagesDetailView" && hitInfo.Column.FieldName == "DetailImageUnbound")
                        {
                            int rowHandle = hitView.GetRowHandle(hitInfo.RowHandle);
                            if (rowHandle >= 0 && hitView.GetRow(rowHandle) is ProductImageDto imgDto)
                            {
                                string? imagePath = imgDto.Path;
                                Image? img = !string.IsNullOrWhiteSpace(imagePath) ? GetOrLoadImage(imagePath) : null;
                                if (img != null)
                                {
                                    Image largeImg = new Bitmap(img, new Size(200, 200));
                                    SuperToolTip stp = new();
                                    ToolTipItem item = new();
                                    item.ImageOptions.Image = largeImg;
                                    item.Text = imgDto.IsPrimary ? "Ana Resim" : "Ürün Resmi";
                                    stp.Items.Add(item);

                                    e.Info = new ToolTipControlInfo(new CellToolTipInfo(hitInfo.RowHandle, hitInfo.Column, "cell"), "")
                                    {
                                        SuperTip = stp
                                    };
                                }
                            }
                        }
                    }
                }
            }
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