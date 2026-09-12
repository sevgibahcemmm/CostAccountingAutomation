using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;

using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public partial class ProductEditForm : XtraForm
    {
        private readonly ProductDto? _editing;

        private readonly BindingList<PriceRowVm> _prices = [];
        private readonly BindingList<ImageRowVm> _images = [];
        private readonly List<ProductMovementDto> _movements = [];
        private List<ProductUnitTypeDto> _unitTypes = [];
        private List<ChartOfAccountLookUpDto> _accounts = [];
        private string _nextProductCode = "";
        private bool _isPopulating;

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
            IconOptions.SvgImage = SvgIcons.Modules[2];

            gridPrices.DataSource = _prices;
            gridImages.DataSource = _images;

            ApplyIcons();
            WireEvents();
            FitToWorkArea();
            fixFooterButtons();
        }

        private static void SetButtonImage(SimpleButton btn, SvgImage icon, int size)
        {
            btn.ImageOptions.SvgImage = icon;
            btn.ImageOptions.SvgImageSize = new Size(size, size);
            btn.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
        }

        private void ApplyIcons()
        {
            lblHeaderIcon.ImageOptions.SvgImage = SvgIcons.BarcodeIcon;
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(32, 32);
            picBarcode.SvgImage = SvgIcons.BarcodeIcon;
            picBarcode.SvgImageSize = new Size(56, 22);
            picQR.SvgImage = SvgIcons.QRIcon;
            picQR.SvgImageSize = new Size(32, 22);

            SetButtonImage(btnSave, SvgIcons.CheckIcon, 20);
            SetButtonImage(btnCancel, SvgIcons.CloseIcon, 16);
            SetButtonImage(btnAddUnitType, SvgIcons.PlusIcon, 14);
            SetButtonImage(btnAddPrice, SvgIcons.PlusIcon, 18);
            SetButtonImage(btnRemovePrice, SvgIcons.TrashIcon, 18);
            SetButtonImage(btnAddImage, SvgIcons.PlusIcon, 18);
            SetButtonImage(btnRemoveImage, SvgIcons.TrashIcon, 18);
            SetButtonImage(btnSetPrimary, SvgIcons.CheckIcon, 18);

            tabBasic.ImageOptions.SvgImage = SvgIcons.Modules[2];
            tabBasic.ImageOptions.SvgImageSize = new Size(16, 16);
            tabPrices.ImageOptions.SvgImage = SvgIcons.TagIcon;
            tabPrices.ImageOptions.SvgImageSize = new Size(16, 16);
            tabMovements.ImageOptions.SvgImage = SvgIcons.TrendBlueIcon;
            tabMovements.ImageOptions.SvgImageSize = new Size(16, 16);
            tabImages.ImageOptions.SvgImage = SvgIcons.PhotoIcon;
            tabImages.ImageOptions.SvgImageSize = new Size(16, 16);
        }

        private void WireEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            btnAddUnitType.Click += BtnAddUnitType_Click;
            cmbWarehouse.EditValueChanged += CmbWarehouse_EditValueChanged;
            cmbCategory.EditValueChanged += CmbCategory_EditValueChanged;
            btnAddPrice.Click += BtnAddPrice_Click;
            btnRemovePrice.Click += (_, _) => { if (gridPriceView.FocusedRowHandle >= 0) _prices.RemoveAt(gridPriceView.FocusedRowHandle); };
            btnAddImage.Click += BtnAddImage_Click;
            btnRemoveImage.Click += (_, _) => { if (gridImageView.FocusedRowHandle >= 0) _images.RemoveAt(gridImageView.FocusedRowHandle); };
            btnSetPrimary.Click += BtnSetPrimary_Click;
            gridMovementView.CustomColumnDisplayText += GridMovementView_CustomColumnDisplayText;
            Load += ProductEditForm_Load;
        }

        private void FitToWorkArea()
        {
            Rectangle workArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            int fitW = Math.Min(ClientSize.Width, workArea.Width - 16);
            int fitH = Math.Min(ClientSize.Height, workArea.Height - 24);
            if (fitW < ClientSize.Width || fitH < ClientSize.Height)
            {
                ClientSize = new Size(fitW, fitH);
            }
        }

        private void fixFooterButtons()
        {
            pnlFooter.Size = new Size(ClientSize.Width, 64);
            btnSave.Location = new Point(ClientSize.Width - 14 - btnSave.Width, 15);
            btnCancel.Location = new Point(btnSave.Left - 6 - btnCancel.Width, 15);
        }

        private async void ProductEditForm_Load(object? sender, EventArgs e)
        {
            try
            {
                cmbWarehouse.Properties.View.Columns.Clear();
                cmbCategory.Properties.View.Columns.Clear();
                cmbUnitType.Properties.View.Columns.Clear();

                await LoadLookupsAsync();

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
            _accounts = (await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None)).Data ?? [];

            List<ChartOfAccountLookUpDto> warehouses = _accounts.Where(a => a.Type == ChartOfAccountType.Warehouse).ToList();

            ConfigureLookUp(cmbWarehouse, warehouses, nameof(ChartOfAccountLookUpDto.Id), nameof(ChartOfAccountLookUpDto.Display), "Depo", 440);
            ConfigureLookUp(cmbCategory, new List<ChartOfAccountLookUpDto>(), nameof(ChartOfAccountLookUpDto.Id), nameof(ChartOfAccountLookUpDto.Display), "Kategori", 200);
            cmbCategory.Enabled = false;
            ConfigureLookUp(cmbUnitType, _unitTypes, nameof(ProductUnitTypeDto.Id), nameof(ProductUnitTypeDto.Name), "Birim Cinsi", 120);
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

            txtName.Text = full.Name;
            txtProductCode.Text = full.ProductCode;
            _nextProductCode = full.ProductCode;
            picBarcode.ToolTip = string.IsNullOrWhiteSpace(full.Barcode) ? "Barkod" : "Barkod: " + full.Barcode;
            picQR.ToolTip = string.IsNullOrWhiteSpace(full.QRCode) ? "Karekod" : "Karekod: " + full.QRCode;
            spinTaxRate.EditValue = full.TaxRate * 100;
            spinMinLevel.EditValue = full.MinimumProductLevel;
            _isPopulating = true;
            try
            {
                cmbWarehouse.EditValue = full.WarehouseId;
                LoadCategoriesForWarehouse(full.WarehouseId);
                cmbCategory.EditValue = full.CategoryId;
            }
            finally
            {
                _isPopulating = false;
            }
            cmbUnitType.EditValue = full.ProductUnitTypeId;
            memoDescription.Text = full.Description;
            chkActive.Checked = full.IsActive;

            _movements.AddRange(full.Movements);
            gridMovements.DataSource = _movements;
            gridMovementView.BestFitColumns();

            foreach (ProductPriceDto p in full.Prices)
            {
                _prices.Add(new PriceRowVm
                {
                    PriceTypeName = p.PriceType == ProductPriceType.Sale ? "Satış" : "Alış",
                    UnitPrice = p.UnitPrice,
                    StartDate = p.StartDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = p.EndDate?.ToDateTime(TimeOnly.MinValue)
                });
            }

            foreach (ProductImageDto img in full.Images)
            {
                _images.Add(new ImageRowVm { Path = img.Path, IsPrimary = img.IsPrimary });
            }

            gridPriceView.BestFitColumns();
            gridImageView.BestFitColumns();
        }

        private async void CmbCategory_EditValueChanged(object? sender, EventArgs e)
        {
            if (cmbCategory.EditValue is not Guid catId)
            {
                return;
            }

            if (_editing is not null && catId == _editing.CategoryId)
            {
                txtProductCode.Text = _editing.ProductCode;
                return;
            }

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                string? nextCode = (await mediator.Send(new ProductGetNextProductCodeQuery(catId), CancellationToken.None)).Data;
                if (nextCode is not null)
                {
                    _nextProductCode = nextCode;
                    txtProductCode.Text = _nextProductCode;
                }
            }
            catch
            {
                txtProductCode.Text = "";
            }
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
            _prices.Add(new PriceRowVm
            {
                PriceTypeName = "Satış",
                UnitPrice = 0,
                StartDate = DateTime.Today,
                EndDate = null
            });

            gridPriceView.Focus();
            gridPriceView.FocusedRowHandle = _prices.Count - 1;
        }

        private void BtnAddImage_Click(object? sender, EventArgs e)
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

            string imagesDir = Path.Combine(AppContext.BaseDirectory, "ProductImages");
            Directory.CreateDirectory(imagesDir);

            foreach (string file in dialog.FileNames)
            {
                string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file);
                string dest = Path.Combine(imagesDir, fileName);
                File.Copy(file, dest, true);
                _images.Add(new ImageRowVm
                {
                    Path = "ProductImages\\" + fileName,
                    IsPrimary = _images.Count == 0
                });
            }

            gridImageView.BestFitColumns();
        }

        private void BtnSetPrimary_Click(object? sender, EventArgs e)
        {
            if (gridImageView.FocusedRowHandle < 0 || gridImageView.GetFocusedRow() is not ImageRowVm vm)
            {
                ToastHelper.Show("Önce bir resim seçin", ToastType.Warning);
                return;
            }

            for (int i = 0; i < _images.Count; i++)
            {
                _images[i].IsPrimary = (i == gridImageView.FocusedRowHandle);
            }

            gridImages.RefreshDataSource();
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string description = memoDescription.Text.Trim();
            decimal taxRate = decimal.Round(Convert.ToDecimal(spinTaxRate.EditValue ?? 0m) / 100m, 4);
            decimal? minLevel = spinMinLevel.EditValue is null ? null : Convert.ToDecimal(spinMinLevel.EditValue);
            Guid? warehouseId = cmbWarehouse.EditValue as Guid?;
            Guid? categoryId = cmbCategory.EditValue as Guid?;
            Guid? unitTypeId = cmbUnitType.EditValue as Guid?;
            bool isActive = chkActive.Checked;

            List<ProductPriceRow> priceRows = _prices.Select(p => new ProductPriceRow(
                null,
                p.PriceTypeName == "Satış" ? ProductPriceType.Sale : ProductPriceType.Purchase,
                p.UnitPrice,
                DateOnly.FromDateTime(p.StartDate),
                p.EndDate is { } end ? DateOnly.FromDateTime(end) : null)).ToList();

            List<ProductImageRow> imageRows = _images.Select(i => new ProductImageRow(null, i.Path, i.IsPrimary)).ToList();

            IRequest<Result<string>> command = _editing is null
                ? new ProductCreateCommand(name, null, null, taxRate, minLevel, warehouseId!.Value, categoryId!.Value, unitTypeId!.Value, description, isActive, priceRows, _images.Select(i => i.Path).ToList())
                : new ProductUpdateCommand(_editing.Id, name, _editing.Barcode, _editing.QRCode, taxRate, minLevel, warehouseId!.Value, categoryId!.Value, unitTypeId!.Value, description, isActive, priceRows, imageRows);

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
                (nameof(ProductCreateCommand.TaxRate), spinTaxRate),
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
            spinTaxRate.ErrorText = string.Empty;
            cmbWarehouse.ErrorText = string.Empty;
            cmbCategory.ErrorText = string.Empty;
            cmbUnitType.ErrorText = string.Empty;
        }

        private sealed class PriceRowVm
        {
            public string PriceTypeName { get; set; } = "Satış";
            public decimal UnitPrice { get; set; }
            public DateTime StartDate { get; set; } = DateTime.Today;
            public DateTime? EndDate { get; set; }
        }

        private sealed class ImageRowVm
        {
            public string Path { get; set; } = default!;
            public bool IsPrimary { get; set; }
        }
    }
}