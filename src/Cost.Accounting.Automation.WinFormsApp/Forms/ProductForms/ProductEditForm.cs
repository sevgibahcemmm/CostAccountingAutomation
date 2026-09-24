using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Products.ProductUnitTypes;
using Cost.Accounting.Automation.Application.Products.TaxRates;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.IO;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public partial class ProductEditForm : XtraForm
    {
        private sealed record SemiFinishedOption(Guid Id, string Display);
        private readonly ProductDto? _editing;
        private readonly BindingList<ProductPriceDto> _prices = [];
        private readonly BindingList<ProductImageDto> _images = [];
        private readonly List<ProductMovementDto> _movements = [];
        private readonly Dictionary<string, Image?> _imageCache = [];
        private List<ProductUnitTypeDto> _unitTypes = [];
        private List<TaxRateDto> _taxRates = [];
        private List<ChartOfAccountLookUpDto> _accounts = [];
        private List<SemiFinishedOption> _semiFinishedOptions = [];
        private string _productCode = string.Empty;

        private bool _isPopulating;
        private readonly System.Windows.Forms.Timer _previewDebounce = new() { Interval = 500 };

        public ProductEditForm() : this(null)
        {
        }

        public ProductEditForm(ProductDto? existing)
        {
            _editing = existing;
            InitializeComponent();

            Text = _editing is null ? "Yeni Ürün" : "Ürün Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null ? "Yeni ürün kartı oluşturmak için bilgileri doldurun" : "Ürün bilgilerini güncelleyin";

            ConfigurePriceGrid();
            ConfigureMovementGrid();
            ConfigureImageColumns();
            gridPrices.DataSource = _prices;
            gridImages.DataSource = _images;

            WireEvents();
        }

        private void ConfigurePriceGrid()
        {
            GridColumnFactory.ConfigureFromAttributes(gridPriceView, typeof(ProductPriceDto));
            gridPriceView.Columns[nameof(ProductPriceDto.PriceTypeName)]!.Caption = "Fiyat Türü";
            gridPriceView.Columns[nameof(ProductPriceDto.UnitPrice)]!.Caption = "Birim Fiyat";
            gridPriceView.Columns[nameof(ProductPriceDto.StartDate)]!.Caption = "Başlangıç";
            gridPriceView.Columns[nameof(ProductPriceDto.EndDate)]!.Caption = "Bitiş";
            gridPriceView.OptionsView.ShowColumnHeaders = true;

            foreach (GridColumn column in gridPriceView.Columns)
            {
                column.ColumnEdit = column.FieldName switch
                {
                    nameof(ProductPriceDto.PriceTypeName) => riCombo,
                    nameof(ProductPriceDto.UnitPrice) => riSpin,
                    nameof(ProductPriceDto.StartDate) => riDate,
                    nameof(ProductPriceDto.EndDate) => riDateNull,
                    _ => column.ColumnEdit
                };
            }
        }

        private void ConfigureMovementGrid()
        {
            GridColumnFactory.ConfigureFromAttributes(gridMovementView, typeof(ProductMovementDto));
            gridMovementView.Columns[nameof(ProductMovementDto.Date)]!.Caption = "Tarih";
            gridMovementView.Columns[nameof(ProductMovementDto.MovementType)]!.Caption = "Hareket Türü";
            gridMovementView.Columns[nameof(ProductMovementDto.Quantity)]!.Caption = "Miktar";
            gridMovementView.Columns[nameof(ProductMovementDto.UnitPrice)]!.Caption = "Birim Fiyat";
            gridMovementView.Columns[nameof(ProductMovementDto.ReferenceNo)]!.Caption = "Belge / Ref No";
            gridMovementView.Columns[nameof(ProductMovementDto.Description)]!.Caption = "Açıklama";
            gridMovementView.OptionsView.ShowColumnHeaders = true;
            gridMovementView.OptionsBehavior.ReadOnly = true;
        }

        private void ConfigureImageColumns()
        {
            gridImageView.Columns.Clear();
            gridImageView.CustomUnboundColumnData += GridImageView_CustomUnboundColumnData;

            RepositoryItemPictureEdit riPicture = new()
            {
                SizeMode = PictureSizeMode.Zoom,
                AllowZoom = DefaultBoolean.True,
                ShowZoomSubMenu = DefaultBoolean.True
            };

            GridColumn colImage = new()
            {
                Caption = "Resim",
                FieldName = "ImageUnbound",
                UnboundDataType = typeof(Image),
                Visible = true,
                Width = 80,
                ColumnEdit = riPicture
            };
            colImage.OptionsColumn.FixedWidth = true;

            GridColumn colPath = new()
            {
                Caption = "Resim Yolu",
                FieldName = nameof(ProductImageDto.Path),
                Visible = true,
                Width = 280
            };

            GridColumn colIsPrimary = new()
            {
                Caption = "Ana Resim",
                FieldName = nameof(ProductImageDto.IsPrimary),
                Visible = true,
                Width = 90,
                ColumnEdit = riCheck
            };

            gridImageView.Columns.AddRange([colImage, colPath, colIsPrimary]);
            gridImageView.RowHeight = 65;
        }

        private void GridImageView_CustomUnboundColumnData(object? sender, CustomColumnDataEventArgs e)
        {
            if (e.IsGetData && e.Column.FieldName == "ImageUnbound" && e.Row is ProductImageDto img)
            {
                e.Value = GetOrLoadImage(img.Path);
            }
        }

        private void WireEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            btnAddUnitType.Click += BtnAddUnitType_Click;
            cmbWarehouse.EditValueChanged += CmbWarehouse_EditValueChanged;
            cmbCategory.EditValueChanged += CmbCategory_EditValueChanged;
            txtName.EditValueChanged += (_, _) => RestartPreviewDebounce();
            lookUpTaxRate.EditValueChanged += (_, _) => RestartPreviewDebounce();
            cmbUnitType.EditValueChanged += (_, _) => RestartPreviewDebounce();
            _previewDebounce.Tick += async (_, _) =>
            {
                _previewDebounce.Stop();
                await GenerateBarcodePreviewAsync();
            };
            btnAddPrice.Click += BtnAddPrice_Click;
            btnRemovePrice.Click += (_, _) => { if (gridPriceView.FocusedRowHandle >= 0) _prices.RemoveAt(gridPriceView.FocusedRowHandle); };
            btnAddImage.Click += BtnAddImage_Click;
            btnRemoveImage.Click += (_, _) =>
            {
                if (gridImageView.FocusedRowHandle >= 0)
                {
                    _images.RemoveAt(gridImageView.FocusedRowHandle);
                    EnsureSinglePrimary();
                }
            };
            btnSetPrimary.Click += BtnSetPrimary_Click;
            gridMovementView.CustomColumnDisplayText += GridMovementView_CustomColumnDisplayText;
            gridImageView.CellValueChanging += GridImageView_CellValueChanging;
            Load += ProductEditForm_Load;
        }

        private async void ProductEditForm_Load(object? sender, EventArgs e)
        {
            try
            {
                cmbWarehouse.Properties.View.Columns.Clear();
                cmbCategory.Properties.View.Columns.Clear();
                cmbUnitType.Properties.View.Columns.Clear();
                lookUpTaxRate.Properties.View.Columns.Clear();

                await LoadLookupsAsync();

                if (_editing is null)
                {
                    // Create mode: show pair creation if warehouse is 151 or 152
                    if (cmbWarehouse.EditValue is Guid whId)
                    {
                        var wh = _accounts.FirstOrDefault(a => a.Id == whId);
if (wh != null && (wh.Code == "151" || wh.Code == "152" || wh.Code.StartsWith("151.") || wh.Code.StartsWith("152.")))
                        {
                            chkCreatePair.Visible = true;
                            chkCreatePair.Checked = true;
                        }
                        else
                        {
                            chkCreatePair.Visible = false;
                        }
                    }
                    else
                    {
                        chkCreatePair.Visible = false;
                    }
                }

                if (_editing is not null)
                {
                    await PopulateAsync(_editing);
                }
                else
                {
                    chkActive.Checked = true;
                }
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Ürün bilgileri yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                Close();
            }
        }

        private async Task LoadLookupsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            _unitTypes = (await mediator.Send(new ProductUnitTypeGetAllQuery(), CancellationToken.None)).ToList();
            _taxRates = (await mediator.Send(new TaxRateGetAllQuery(), CancellationToken.None)).ToList();
            _accounts = (await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None)).Data ?? [];

            IQueryable<ProductDto> productsQuery = await mediator.Send(new ProductGetAllQuery(), CancellationToken.None);
            List<ChartOfAccountLookUpDto> warehouses = _accounts.Where(a => a.Type == ChartOfAccountType.Warehouse).ToList();

            ConfigureLookUp(cmbWarehouse, warehouses, nameof(ChartOfAccountLookUpDto.Id), nameof(ChartOfAccountLookUpDto.Display), "Depo", 440);
            ConfigureLookUp(cmbCategory, new List<ChartOfAccountLookUpDto>(), nameof(ChartOfAccountLookUpDto.Id), nameof(ChartOfAccountLookUpDto.Display), "Kategori", 200);
            cmbCategory.Enabled = false;
            ConfigureLookUp(cmbUnitType, _unitTypes, nameof(ProductUnitTypeDto.Id), nameof(ProductUnitTypeDto.Name), "Birim Cinsi", 120);
            ConfigureLookUp(lookUpTaxRate, _taxRates, nameof(TaxRateDto.Id), nameof(TaxRateDto.Display), "KDV Oranı", 120);
        }

        private static void ConfigureLookUp(SearchLookUpEdit editor, object dataSource, string valueMember, string displayMember, string caption, int width)
        {
            editor.Properties.DataSource = dataSource;
            editor.Properties.ValueMember = valueMember;
            editor.Properties.DisplayMember = displayMember;
            editor.Properties.PopupFilterMode = PopupFilterMode.Contains;
            editor.Properties.BestFitMode = BestFitMode.BestFit;

            GridView view = editor.Properties.View;
            view.Columns.Clear();
            GridColumn column = view.Columns.AddField(displayMember);
            column.Caption = caption;
            column.VisibleIndex = 0;
            column.Width = width;
            view.BestFitColumns();
        }

        private void CmbWarehouse_EditValueChanged(object? sender, EventArgs e)
        {
            if (_isPopulating)
            {
                return;
            }

            cmbCategory.Enabled = true;
            cmbCategory.EditValue = null;

            if (cmbWarehouse.EditValue is not Guid warehouseId)
            {
                cmbCategory.Enabled = false;
                return;
            }

            LoadCategoriesForWarehouse(warehouseId);

            if (_editing is null) // create mode only
            {
                var wh = _accounts.FirstOrDefault(a => a.Id == warehouseId);
                if (wh != null && (wh.Code == "151" || wh.Code == "152" || wh.Code.StartsWith("151.") || wh.Code.StartsWith("152.")))
                {
                    chkCreatePair.Visible = true;
                    chkCreatePair.Checked = true;
                }
                else
                {
                    chkCreatePair.Visible = false;
                    chkCreatePair.Checked = false;
                }
            }
        }

        private void LoadCategoriesForWarehouse(Guid warehouseId)
        {
            HashSet<Guid> levelNodes = _accounts
                .Where(a => a.ParentId == warehouseId)
                .Select(a => a.Id)
                .ToHashSet();

            List<ChartOfAccountLookUpDto> categories = _accounts
                .Where(a => a.Type == ChartOfAccountType.Category &&
                            a.ParentId is Guid pid &&
                            (pid == warehouseId || levelNodes.Contains(pid)))
                .ToList();

            if (_editing is not null && !categories.Any(c => c.Id == _editing.CategoryId))
            {
                ChartOfAccountLookUpDto? savedCategory = _accounts
                    .FirstOrDefault(a => a.Id == _editing.CategoryId && a.Type == ChartOfAccountType.Category);
                if (savedCategory is not null)
                {
                    categories.Add(savedCategory);
                }
            }

            ConfigureLookUp(cmbCategory, categories, nameof(ChartOfAccountLookUpDto.Id), nameof(ChartOfAccountLookUpDto.Display), "Kategori", 200);
            cmbCategory.Enabled = categories.Count > 0;
            cmbCategory.EditValue = null;

            if (categories.Count == 0)
            {
                ToastHelper.Show("Seçilen depoya bağlı kategori bulunamadı. Hesap Planı'nda depo altında kategori düğümü oluşturun.", ToastType.Warning, 4000);
            }
        }

        private async void BtnAddUnitType_Click(object? sender, EventArgs e)
        {
            using var form = new ProductUnitTypeEditForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await ReloadUnitTypesAsync();
            }
        }

        private async Task ReloadUnitTypesAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            _unitTypes = (await mediator.Send(new ProductUnitTypeGetAllQuery(), CancellationToken.None)).ToList();

            object? current = cmbUnitType.EditValue;
            ConfigureLookUp(cmbUnitType, _unitTypes, nameof(ProductUnitTypeDto.Id), nameof(ProductUnitTypeDto.Name), "Birim Cinsi", 120);
            if (current is Guid id && _unitTypes.Any(u => u.Id == id))
            {
                cmbUnitType.EditValue = id;
            }
        }

        private async Task PopulateAsync(ProductDto dto)
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            ProductDto? full = (await mediator.Send(new ProductGetQuery(dto.Id), CancellationToken.None)).Data;
            if (full is null)
            {
                ToastHelper.Show("Ürün detayı yüklenemedi", ToastType.Error, 4000);
                Close();
                return;
            }

            txtProductCode.Text = full.ProductCode;
            _productCode = full.ProductCode;
            picBarcode.ToolTip = string.IsNullOrWhiteSpace(full.Barcode) ? "Barkod" : "Barkod: " + full.Barcode;
            picQR.ToolTip = string.IsNullOrWhiteSpace(full.QRCode) ? "Karekod" : full.QRCode;
            _isPopulating = true;
            try
            {
                txtName.Text = full.Name;
                lookUpTaxRate.EditValue = full.TaxRateId;
                spinMinLevel.EditValue = full.MinimumProductLevel;
                cmbUnitType.EditValue = full.ProductUnitTypeId;
                cmbWarehouse.EditValue = full.WarehouseId;
                LoadCategoriesForWarehouse(full.WarehouseId);
                cmbCategory.EditValue = full.CategoryId;
                
            }
            finally
            {
                _isPopulating = false;
            }
            memoDescription.Text = full.Description;
            chkActive.Checked = full.IsActive;

            _movements.AddRange(full.Movements);
            gridMovements.DataSource = _movements;
            gridMovementView.BestFitColumns();

            foreach (ProductPriceDto p in full.Prices)
            {
                _prices.Add(new ProductPriceDto
                {
                    Id = p.Id,
                    PriceType = p.PriceType,
                    UnitPrice = p.UnitPrice,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                });
            }

            foreach (ProductImageDto img in full.Images)
            {
                _images.Add(new ProductImageDto
                {
                    Path = img.Path,
                    IsPrimary = img.IsPrimary
                });
            }

            EnsureSinglePrimary();
            gridPriceView.BestFitColumns();
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
                    img = SafeBitmap(ms);
                }
            }
            catch { }

            _imageCache[relativePath] = img;
            return img;
        }

        private static Image? SafeBitmap(MemoryStream ms)
        {
            try { return new Bitmap(ms); } catch { return null; }
        }

        private async void CmbCategory_EditValueChanged(object? sender, EventArgs e)
        {
            if (cmbCategory.EditValue is not Guid catId)
            {
                return;
            }

            if (_editing is not null && catId == _editing.CategoryId)
            {
                _productCode = _editing.ProductCode;
                txtProductCode.Text = _productCode;
                await GenerateBarcodePreviewAsync();
                return;
            }

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                string? nextCode = (await mediator.Send(new ProductGetNextProductCodeQuery(catId), CancellationToken.None)).Data;
                if (nextCode is not null)
                {
                    _productCode = nextCode;
                    txtProductCode.Text = _productCode;
                    await GenerateBarcodePreviewAsync();
                }
                else
                {
                    _productCode = "";
                    txtProductCode.Text = "";
                    ClearBarcodePreview();
                }
            }
            catch
            {
                _productCode = "";
                txtProductCode.Text = "";
                ClearBarcodePreview();
            }
        }

        private void RestartPreviewDebounce()
        {
            if (_isPopulating)
            {
                return;
            }

            _previewDebounce.Stop();
            _previewDebounce.Start();
        }

        private async Task GenerateBarcodePreviewAsync()
        {
            string? productCode = _productCode;
            if (string.IsNullOrWhiteSpace(productCode))
            {
                ClearBarcodePreview();
                return;
            }

            string productName = txtName.Text?.Trim() ?? string.Empty;
            decimal taxRate = lookUpTaxRate.EditValue is Guid taxRateId
                ? _taxRates.FirstOrDefault(t => t.Id == taxRateId)?.Rate ?? 0m
                : 0m;
            string warehouseName = cmbWarehouse.EditValue is Guid whId
                ? _accounts.FirstOrDefault(a => a.Id == whId)?.Display ?? string.Empty
                : string.Empty;
            string categoryName = cmbCategory.EditValue is Guid catId
                ? _accounts.FirstOrDefault(a => a.Id == catId)?.Display ?? string.Empty
                : string.Empty;
            string unitTypeName = cmbUnitType.EditValue is Guid unitId
                ? _unitTypes.FirstOrDefault(u => u.Id == unitId)?.Name ?? string.Empty
                : string.Empty;

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                ProductBarcodePreviewDto? preview = (await mediator.Send(
                    new ProductBarcodePreviewQuery(productCode, productName, taxRate, warehouseName, categoryName, unitTypeName),
                    CancellationToken.None)).Data;

                if (preview is not null)
                {
                    SetPictureSafely(picBarcode, ByteArrayToImage(preview.BarcodeImage));
                    SetPictureSafely(picQR, ByteArrayToImage(preview.QrImage));
                    picBarcode.ToolTip = "Barkod: " + preview.Gtin;
                    picQR.ToolTip = preview.QrContent;
                    SetBarcodeValue(preview.Gtin);
                    SetQrValue(preview.QrContent);
                }
                else
                {
                    ClearBarcodePreview();
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("ProductEditForm.BarcodePreview", ex);
                ClearBarcodePreview();
            }
        }

        private void ClearBarcodePreview()
        {
            SetPictureSafely(picBarcode, null);
            SetPictureSafely(picQR, null);
            picBarcode.ToolTip = "Barkod";
            picQR.ToolTip = "Karekod";
            SetBarcodeValue(null);
            SetQrValue(null);
        }

        private void SetBarcodeValue(string? gtin)
        {
            lblBarcodeValue.Text = string.IsNullOrWhiteSpace(gtin) ? "--" : gtin;
            lblBarcodeValue.Appearance.Options.UseTextOptions = true;
            lblBarcodeValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        }

        private void SetQrValue(string? qrContent)
        {
            if (string.IsNullOrWhiteSpace(qrContent))
            {
                lblQrValue.Text = "--";
                return;
            }

            string singleLine = qrContent.Replace('\n', ' ').Replace('\r', ' ').Trim();
            lblQrValue.Text = singleLine.Length > 32 ? singleLine[..32] + "…" : singleLine;
        }

        private static void SetPictureSafely(PictureEdit pictureEdit, Image? newImage)
        {
            Image? oldImage = pictureEdit.Image;
            pictureEdit.Image = newImage;
            oldImage?.Dispose();
        }

        private static Image? ByteArrayToImage(byte[]? bytes)
        {
            if (bytes is null || bytes.Length == 0)
            {
                return null;
            }

            using var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        private void GridMovementView_CustomColumnDisplayText(object? sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column is null)
            {
                return;
            }

            if (e.Column.FieldName == nameof(ProductMovementDto.MovementType) && e.Value is ProductMovementType mt)
            {
                e.DisplayText = mt == ProductMovementType.Input ? "Giriş" : "Çıkış";
            }
        }

        private void BtnAddPrice_Click(object? sender, EventArgs e)
        {
            _prices.Add(new ProductPriceDto
            {
                PriceType = ProductPriceType.Sale,
                UnitPrice = 0,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = null
            });

            gridPriceView.Focus();
            gridPriceView.FocusedRowHandle = _prices.Count - 1;
        }

        private async void BtnAddImage_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Görseller (*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp)|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp",
                Multiselect = true,
                Title = "Resim Seç"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using var scope = Program.Services.CreateScope();
            IFileStorageService storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

            foreach (string file in dialog.FileNames)
            {
                string relativePath = await storage.SaveAsync(
                    await File.ReadAllBytesAsync(file),
                    Path.GetFileName(file),
                    "ProductImages");

                _images.Add(new ProductImageDto
                {
                    Path = relativePath,
                    IsPrimary = false
                });
            }

            EnsureSinglePrimary();
        }

        private void BtnSetPrimary_Click(object? sender, EventArgs e)
        {
            int focusedRow = gridImageView.FocusedRowHandle;
            if (focusedRow < 0)
            {
                ToastHelper.Show("Önce bir resim seçin", ToastType.Warning);
                return;
            }

            gridImageView.CloseEditor();

            for (int i = 0; i < _images.Count; i++)
            {
                _images[i].IsPrimary = (i == focusedRow);
            }

            EnsureSinglePrimary();
        }

        private void GridImageView_CellValueChanging(object? sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == nameof(ProductImageDto.IsPrimary))
            {
                gridImageView.CloseEditor();
                bool newValue = Convert.ToBoolean(e.Value);

                if (newValue)
                {
                    int focusedRow = e.RowHandle;
                    for (int i = 0; i < _images.Count; i++)
                    {
                        _images[i].IsPrimary = (i == focusedRow);
                    }
                }
                else
                {
                    _images[e.RowHandle].IsPrimary = true;
                }

                gridImages.RefreshDataSource();
            }
        }

        private void EnsureSinglePrimary()
        {
            if (_images.Count == 0)
            {
                return;
            }

            int primaryCount = _images.Count(x => x.IsPrimary);

            if (primaryCount == 0)
            {
                _images[0].IsPrimary = true;
            }
            else if (primaryCount > 1)
            {
                bool firstFound = false;
                foreach (var img in _images)
                {
                    if (img.IsPrimary)
                    {
                        if (!firstFound)
                        {
                            firstFound = true;
                        }
                        else
                        {
                            img.IsPrimary = false;
                        }
                    }
                }
            }

            gridImages.RefreshDataSource();
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                await BtnSaveCoreAsync();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("ProductEditForm.Save", ex);
                ToastHelper.Show("Kaydetme sırasında bir hata oluştu: " + ex.Message, ToastType.Error, 5000);
            }
        }

        private async Task BtnSaveCoreAsync()
        {
            string name = txtName.Text.Trim();
            string description = memoDescription.Text.Trim();
            Guid? taxRateId = lookUpTaxRate.EditValue as Guid?;
            decimal? minLevel = spinMinLevel.EditValue is null ? null : Convert.ToDecimal(spinMinLevel.EditValue);
            Guid? warehouseId = cmbWarehouse.EditValue as Guid?;
            Guid? categoryId = cmbCategory.EditValue as Guid?;
            Guid? unitTypeId = cmbUnitType.EditValue as Guid?;
            bool createPair = _editing is null && chkCreatePair.Visible && chkCreatePair.Checked;
            Guid? semiFinishedProductId = _editing?.SemiFinishedProductId;
            bool isActive = chkActive.Checked;

            List<ProductPriceRow> priceRows = _prices.Select(p => new ProductPriceRow(
                p.Id == Guid.Empty ? null : p.Id,
                p.PriceType,
                p.UnitPrice,
                p.StartDate,
                p.EndDate)).ToList();

            List<ProductImageRow> imageRows = _images.Select(i => new ProductImageRow(null, i.Path, i.IsPrimary)).ToList();

            IRequest<Result<string>> command = _editing is null
                ? new ProductCreateCommand(name, taxRateId.GetValueOrDefault(), minLevel, warehouseId.GetValueOrDefault(), categoryId.GetValueOrDefault(), unitTypeId.GetValueOrDefault(), description, isActive, createPair, priceRows, _images.Select(i => i.Path).ToList())
                : new ProductUpdateCommand(_editing.Id, name, taxRateId.GetValueOrDefault(), minLevel, warehouseId.GetValueOrDefault(), categoryId.GetValueOrDefault(), unitTypeId.GetValueOrDefault(), description, isActive, semiFinishedProductId, priceRows, imageRows);

            if (!RunApplicationValidator(command))
            {
                return;
            }

            btnSave.Enabled = false;
            try
            {
                bool ok = await CrudExecutor.ExecuteAsync(command);
                if (ok)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private bool RunApplicationValidator(object command)
        {
            ClearFieldErrors();

            ValidationResult? result = command switch
            {
                ProductCreateCommand create => new ProductCreateCommandValidator().Validate(create),
                ProductUpdateCommand update => new ProductUpdateCommandValidator().Validate(update),
                _ => null
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            List<string> messages = result.Errors.Select(f => f.ErrorMessage).Distinct().ToList();
            ToastHelper.Show(string.Join(Environment.NewLine, messages), ToastType.Warning, 6000);

            (string Property, BaseEdit Editor)[] map =
            [
                (nameof(ProductCreateCommand.Name), txtName),
                (nameof(ProductCreateCommand.TaxRateId), lookUpTaxRate),
                (nameof(ProductCreateCommand.WarehouseId), cmbWarehouse),
                (nameof(ProductCreateCommand.CategoryId), cmbCategory),
                (nameof(ProductCreateCommand.ProductUnitTypeId), cmbUnitType)
            ];

            var first = map.FirstOrDefault(m => result.Errors.Any(f => f.PropertyName == m.Property));
            first.Editor?.Focus();

            return false;
        }

        private void ClearFieldErrors()
        {
            txtName.ErrorText = string.Empty;
            lookUpTaxRate.ErrorText = string.Empty;
            cmbWarehouse.ErrorText = string.Empty;
            cmbCategory.ErrorText = string.Empty;
            cmbUnitType.ErrorText = string.Empty;
        }
    }
}