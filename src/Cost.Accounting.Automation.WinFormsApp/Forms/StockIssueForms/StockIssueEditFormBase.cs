using System.ComponentModel;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public abstract partial class StockIssueEditFormBase : XtraForm
    {
        private readonly StockIssueType _issueType;
        private readonly StockIssueListDto? _editing;
        private readonly BindingList<LineRow> _lines = [];

        private List<ProductDto> _allProducts = [];
        private List<ProductDto> _filteredProducts = [];
        private Dictionary<Guid, ProductDto> _allProductsById = [];
        private Dictionary<Guid, ProductDto> _filteredProductsById = [];
        private List<ChartOfAccountLookUpDto> _accounts = [];
        private List<ChartOfAccountLookUpDto> _targetAccounts = [];
        private Guid _sourceWarehouseId;
        private string _sourceWarehouseDisplay = string.Empty;

        private RepositoryItemSearchLookUpEdit riProduct = default!;

        private bool _saved;

        protected StockIssueEditFormBase(StockIssueType issueType, StockIssueListDto? existing)
        {
            _issueType = issueType;
            _editing = existing;

            InitializeComponent();

            Text = _editing is null ? $"Yeni {FormTitle}" : $"{FormTitle} İncele";
            lblTitle.Text = Text;
            lblSubtitle.Text = IsConsumption
                ? "150 deposundan 900 (Tüketimler) altındaki bir tüketim birimine kayıt oluşturun"
                : "150.98 deposundan 150.55 altındaki atölyeye transfer kaydı oluşturun";

            ConfigureGrid();

            dtDate.DateTime = DateTime.Today;
            cmbCosting.Properties.Items.Add("FIFO");
            cmbCosting.Properties.Items.Add("LIFO");
            cmbCosting.SelectedIndex = 0;

            if (_editing is not null)
            {
                bool isDraft = _editing.Status == StockIssueStatus.Draft;

                // Onaylı belge düzenlenemez. Onay giriş ekranından YAPILMAZ;
                // liste ekranından (tekil veya toplu) yapılır.
                btnSave.Enabled = isDraft;
                dtDate.ReadOnly = true;
                txtDocumentNumber.ReadOnly = true;
                cmbCosting.ReadOnly = true;
                lookUpTarget.ReadOnly = true;
                memoDescription.ReadOnly = true;
                btnAddLine.Enabled = isDraft;
                btnDeleteLine.Enabled = isDraft;
                gridLinesView.OptionsBehavior.Editable = isDraft;
            }

            WireEvents();
        }

        private bool IsConsumption => _issueType == StockIssueType.Consumption;

        private string ExpectedWarehouseCode => IsConsumption ? "150" : "150.98";

        private string FormTitle => IsConsumption ? "Tüketim" : "Atölye Transferi";

        private void ConfigureGrid()
        {
            gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
            gridLinesView.OptionsView.ColumnAutoWidth = false;
            gridLinesView.OptionsView.ShowGroupPanel = false;
            gridLinesView.RowHeight = 26;
            gridLinesControl.DataSource = _lines;

            riProduct = new RepositoryItemSearchLookUpEdit
            {
                ValueMember = nameof(ProductDto.Id),
                DisplayMember = nameof(ProductDto.Name),
                NullText = "Ürün seçiniz...",
                PopupFilterMode = PopupFilterMode.Contains
            };
            riProduct.View.OptionsBehavior.AutoPopulateColumns = false;
            riProduct.View.Columns.AddField(nameof(ProductDto.Name)).Caption = "Ürün Adı";
            riProduct.View.Columns.AddField(nameof(ProductDto.ProductCode)).Caption = "Ürün Kodu";
            riProduct.View.Columns.AddField(nameof(ProductDto.ProductUnitTypeName)).Caption = "Birim";
            riProduct.View.Columns.AddField(nameof(ProductDto.StockQuantity)).Caption = "Stok";
            riProduct.View.Columns[nameof(ProductDto.StockQuantity)].DisplayFormat.FormatType = FormatType.Numeric;
            riProduct.View.Columns[nameof(ProductDto.StockQuantity)].DisplayFormat.FormatString = "n2";
            foreach (GridColumn col in riProduct.View.Columns)
            {
                col.Visible = true;
            }
            riProduct.EditValueChanged += RiProduct_EditValueChanged;

            RepositoryItemSpinEdit riQuantity = new()
            {
                MinValue = 0.0001m,
                MaxValue = 999999999,
                Increment = 1,
                DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" }
            };
            RepositoryItemSpinEdit riMoney = new()
            {
                ReadOnly = true,
                DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" }
            };
            RepositoryItemTextEdit riDesc = new();

            gridLinesControl.RepositoryItems.AddRange([riProduct, riQuantity, riMoney, riDesc]);

            GridColumn productColumn = new()
            {
                Caption = "Ürün",
                FieldName = nameof(LineRow.ProductId),
                ColumnEdit = riProduct,
                Visible = true,
                Width = 360
            };
            GridColumn quantityColumn = new()
            {
                Caption = "Miktar",
                FieldName = nameof(LineRow.Quantity),
                ColumnEdit = riQuantity,
                Visible = true,
                Width = 110,
                AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } }
            };
            GridColumn unitCostColumn = new()
            {
                Caption = "Birim Maliyet",
                FieldName = nameof(LineRow.UnitCost),
                ColumnEdit = riMoney,
                Visible = true,
                Width = 130,
                OptionsColumn = { AllowEdit = false },
                AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } }
            };
            GridColumn stockColumn = new()
            {
                Caption = "Mevcut Stok",
                FieldName = nameof(LineRow.AvailableStock),
                ColumnEdit = riMoney,
                Visible = true,
                Width = 110,
                OptionsColumn = { AllowEdit = false },
                AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } }
            };
            GridColumn descriptionColumn = new()
            {
                Caption = "Açıklama",
                FieldName = nameof(LineRow.Description),
                ColumnEdit = riDesc,
                Visible = true,
                Width = 280,
                MinWidth = 200
            };

            GridColumn totalColumn = new()
            {
                Caption = "Tutar",
                FieldName = nameof(LineRow.TotalAmount),
                ColumnEdit = riMoney,
                Visible = true,
                Width = 120,
                OptionsColumn = { AllowEdit = false, ReadOnly = true },
                AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } }
            };

            gridLinesView.Columns.AddRange(
                [productColumn, quantityColumn, unitCostColumn, stockColumn, descriptionColumn, totalColumn]);
            GridColumnFactory.RegisterManualNumericColumns(gridLinesView);

            foreach (GridColumn col in gridLinesView.Columns)
            {
                col.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }

            // Alt toplam grid'in kendi footer'ında gösterilir; formun altındaki
            // ayrı "Toplam Tutar" etiketleri kaldırıldı.
            gridLinesView.OptionsView.ShowFooter = true;
            quantityColumn.SummaryItem.SummaryType = SummaryItemType.Sum;
            quantityColumn.SummaryItem.DisplayFormat = "{0:n2}";
            totalColumn.SummaryItem.SummaryType = SummaryItemType.Sum;
            totalColumn.SummaryItem.DisplayFormat = "{0:n2} ₺";
        }

        private void WireEvents()
        {
            Load += StockIssueEditForm_Load;
            btnAddLine.Click += (_, _) => AddEmptyLine();
            btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            btnPrintSlip.Click += async (_, _) => await ShowSlipPreviewAsync();
            gridLinesView.CellValueChanged += GridLinesView_CellValueChanged;
            WireDateAndTargetEvents();
        }

        private void WireDateAndTargetEvents()
        {
            dtDate.EditValueChanged += (_, _) => ReapplyAutoDescriptions();
            lookUpTarget.EditValueChanged += (_, _) => ReapplyAutoDescriptions();
        }

        private async void StockIssueEditForm_Load(object? sender, EventArgs e)
        {
            await LoadLookUpsAsync();

            if (_editing is not null)
            {
                await PopulateExistingAsync();
            }
            else
            {
                await LoadNextNumberAsync();
            }
        }

        private async Task LoadLookUpsAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                _accounts = (await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None)).Data ?? [];
                _allProducts = (await mediator.Send(new ProductGetAllQuery(), CancellationToken.None)).ToList();
                _allProductsById = _allProducts.ToDictionary(p => p.Id);

                ChartOfAccountLookUpDto? warehouse = _accounts.FirstOrDefault(a =>
                    a.Type == ChartOfAccountType.Warehouse &&
                    string.Equals(a.Code, ExpectedWarehouseCode, StringComparison.OrdinalIgnoreCase));

                if (warehouse is null)
                {
                    ToastHelper.Show($"{ExpectedWarehouseCode} deposu hesap planında bulunamadı.", ToastType.Error, 6000);
                }
                else
                {
                    _sourceWarehouseId = warehouse.Id;
                    _sourceWarehouseDisplay = warehouse.Display;

                    _filteredProducts = _allProducts
                        .Where(p => p.WarehouseId == _sourceWarehouseId && p.StockQuantity > 0)
                        .ToList();
                    _filteredProductsById = _filteredProducts.ToDictionary(p => p.Id);
                    riProduct.DataSource = _filteredProducts;

                    if (_filteredProducts.Count == 0)
                    {
                        ToastHelper.Show(
                            "Seçilen depoda stoklu ürün bulunamadı. Tüketim/transfer için stokta ürün olmalı.",
                            ToastType.Warning,
                            6000);
                    }
                }

                txtWarehouse.Text = _sourceWarehouseDisplay;

                _targetAccounts = IsConsumption
                    ? _accounts.Where(a => a.Type == ChartOfAccountType.ConsumptionUnit).ToList()
                    : _accounts.Where(a => a.Type == ChartOfAccountType.Workshop).ToList();

                if (_targetAccounts.Count == 0)
                {
                    ToastHelper.Show(
                        IsConsumption
                            ? "900 (Tüketimler) altında tüketim birimi bulunamadı. Lütfen hesap planına birim ekleyin."
                            : "150.55 altında atölye hesabı bulunamadı. Lütfen hesap planını kontrol edin.",
                        ToastType.Warning,
                        6000);
                }

                lookUpTarget.Properties.DataSource = _targetAccounts;
                lookUpTarget.Properties.ValueMember = nameof(ChartOfAccountLookUpDto.Id);
                lookUpTarget.Properties.DisplayMember = nameof(ChartOfAccountLookUpDto.Display);
                lookUpTarget.Properties.NullText = "Hesap arayın veya seçin...";
                lookUpTarget.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;

                GridView targetView = lookUpTarget.Properties.View;
                targetView.OptionsBehavior.AutoPopulateColumns = false;
                targetView.OptionsFind.AlwaysVisible = true;
                targetView.OptionsFind.FindNullPrompt = "Hesap ara...";
                targetView.Columns.Clear();
                targetView.Columns.AddField(nameof(ChartOfAccountLookUpDto.Code)).Caption = "Kod";
                targetView.Columns.AddField(nameof(ChartOfAccountLookUpDto.Name)).Caption = "Hesap Adı";
                foreach (GridColumn col in targetView.Columns)
                {
                    col.Visible = true;
                }

                targetView.BestFitColumns();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Veriler yüklenirken hata oluştu: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task LoadNextNumberAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await mediator.Send(new StockIssueGetNextNumberQuery(_issueType), CancellationToken.None);
                txtDocumentNumber.Text = result.Data ?? string.Empty;
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Belge numarası alınamadı: " + ex.Message, ToastType.Warning);
            }
        }

        private async Task PopulateExistingAsync()
        {
            if (_editing is null)
            {
                return;
            }

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await mediator.Send(new StockIssueGetByIdQuery(_editing.Id), CancellationToken.None);

                StockIssueDto? issue = result.Data;
                if (issue is null)
                {
                    ToastHelper.Show("Belge yüklenemedi.", ToastType.Error);
                    return;
                }

                dtDate.DateTime = issue.Date.ToDateTime(TimeOnly.MinValue);
                txtDocumentNumber.Text = issue.DocumentNumber;
                cmbCosting.SelectedIndex = issue.CostingMethod == StockCostingMethod.Lifo ? 1 : 0;
                memoDescription.Text = issue.Description;

                ChartOfAccountLookUpDto? warehouse = _accounts.FirstOrDefault(a => a.Id == issue.SourceWarehouseId);
                _sourceWarehouseId = issue.SourceWarehouseId;
                txtWarehouse.Text = warehouse?.Display ?? _sourceWarehouseDisplay;

                lookUpTarget.EditValue = issue.TargetAccountId;

                _lines.Clear();
                foreach (StockIssueLineDto line in issue.Lines)
                {
                    _lines.Add(new LineRow
                    {
                        ProductId = line.ProductId,
                        Quantity = line.Quantity,
                        UnitCost = line.UnitCost,
                        Description = line.Description
                    });
                }

                UpdateTotal();
                gridLinesView.RefreshData();
                btnPrintSlip.Enabled = true;
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Belge açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private void AddEmptyLine()
        {
            _lines.Add(new LineRow { Quantity = 1 });
            gridLinesView.RefreshData();
            gridLinesView.FocusedRowHandle = _lines.Count - 1;
        }

        private void DeleteSelectedLine()
        {
            int handle = gridLinesView.FocusedRowHandle;
            if (handle >= 0 && handle < _lines.Count)
            {
                _lines.RemoveAt(handle);
                UpdateTotal();
                UpdateGeneralDescription();
            }
        }

        /// <summary>
        /// Aynı ürün aynı transfer belgesinde yalnızca bir satırda kullanılabilir.
        /// Ürün zaten eklenmişse yeni seçim reddedilir, satır boşaltılır.
        /// </summary>
        private bool RejectDuplicateProduct(int rowHandle, Guid productId, BaseEdit? editor = null)
        {
            if (productId == Guid.Empty)
            {
                return false;
            }

            for (int i = 0; i < _lines.Count; i++)
            {
                if (i == rowHandle || _lines[i].ProductId != productId)
                {
                    continue;
                }

                _lines[rowHandle].ProductId = Guid.Empty;
                _lines[rowHandle].UnitCost = 0m;
                _lines[rowHandle].AvailableStock = 0m;

                if (editor is not null)
                {
                    editor.EditValue = null;
                }
                else if (gridLinesView.Columns[nameof(LineRow.ProductId)] is { } productColumn)
                {
                    gridLinesView.SetRowCellValue(rowHandle, productColumn, null);
                }

                gridLinesView.RefreshRow(rowHandle);
                UpdateTotal();

                ToastHelper.Show(
                    $"'{GetProductName(productId)}' bu belgede zaten eklenmiş. Aynı ürün yalnızca bir satırda kullanılabilir.",
                    ToastType.Warning,
                    4500);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Kayıtlı satırlar için grid'i yeniler. Toplam artık grid footer'ında
        /// otomatik hesaplandığı için ayrı hesaplama yapılmaz.
        /// </summary>
        private void UpdateTotal()
        {
            gridLinesControl.RefreshDataSource();
        }

        private void GridLinesView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.RowHandle < 0 || e.RowHandle >= _lines.Count)
            {
                return;
            }

            LineRow row = _lines[e.RowHandle];

            if (e.Column.FieldName == nameof(LineRow.ProductId) && row.ProductId != Guid.Empty)
            {
                if (RejectDuplicateProduct(
                        e.RowHandle,
                        row.ProductId,
                        gridLinesView.ActiveEditor as BaseEdit))
                {
                    return;
                }

                ProductDto? selectedProduct = _filteredProductsById.GetValueOrDefault(row.ProductId);
                if (selectedProduct is null)
                {
                    WarnAndClearProductLine(
                        e.RowHandle,
                        gridLinesView.ActiveEditor ?? new SearchLookUpEdit(),
                        row.ProductId);
                    return;
                }

                decimal cost = ResolveProductUnitPrice(row.ProductId);
                row.UnitCost = cost;
                gridLinesView.SetRowCellValue(
                    e.RowHandle,
                    gridLinesView.Columns[nameof(LineRow.UnitCost)],
                    cost);

                row.AvailableStock = selectedProduct.StockQuantity;

                gridLinesView.RefreshRow(e.RowHandle);
                UpdateAutoDescription(e.RowHandle);
            }

            if (e.Column.FieldName == nameof(LineRow.Quantity)
                && row.ProductId != Guid.Empty
                && row.Quantity > row.AvailableStock
                && row.AvailableStock > 0)
            {
                ToastHelper.Show(
                    $"'{GetProductName(row.ProductId)}' için istenen miktar ({row.Quantity:n2}) mevcut stoktan ({row.AvailableStock:n2}) fazla.",
                    ToastType.Warning);
            }

            if (e.Column.FieldName == nameof(LineRow.Quantity) && row.ProductId != Guid.Empty)
            {
                UpdateAutoDescription(e.RowHandle);
            }

            UpdateTotal();
        }

        private void RiProduct_EditValueChanged(object? sender, EventArgs e)
        {
            if (sender is not SearchLookUpEdit edit || edit.EditValue is not Guid productId)
            {
                return;
            }

            int rowHandle = gridLinesView.FocusedRowHandle;
            if (rowHandle < 0 || rowHandle >= _lines.Count)
            {
                return;
            }

            LineRow row = _lines[rowHandle];
            ProductDto? product = _filteredProductsById.GetValueOrDefault(productId);
            if (product is null)
            {
                WarnAndClearProductLine(rowHandle, edit, productId);
                return;
            }

            if (RejectDuplicateProduct(rowHandle, product.Id, edit))
            {
                return;
            }

            row.ProductId = product.Id;
            row.UnitCost = ResolveProductUnitPrice(row.ProductId);
            row.AvailableStock = product.StockQuantity;

            gridLinesView.RefreshRow(rowHandle);
            UpdateAutoDescription(rowHandle);
            UpdateTotal();
        }

        private decimal ResolveProductUnitPrice(Guid productId)
        {
            if (productId == Guid.Empty)
            {
                return 0m;
            }

            ProductDto? product = _filteredProductsById.GetValueOrDefault(productId);
            if (product is null)
            {
                return 0m;
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            bool IsActive(ProductPriceDto p) =>
                p.StartDate <= today && (p.EndDate is null || p.EndDate.Value >= today);

            ProductPriceDto? price = product.Prices
                .Where(p => p.PriceType == ProductPriceType.Purchase && IsActive(p))
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault()
                ?? product.Prices
                    .Where(p => p.PriceType == ProductPriceType.Purchase)
                    .OrderByDescending(p => p.StartDate)
                    .FirstOrDefault()
                ?? product.Prices
                    .Where(p => p.PriceType == ProductPriceType.Sale && IsActive(p))
                    .OrderByDescending(p => p.StartDate)
                    .FirstOrDefault()
                ?? product.Prices
                    .Where(p => p.PriceType == ProductPriceType.Sale)
                    .OrderByDescending(p => p.StartDate)
                    .FirstOrDefault();

            if (price is not null && price.UnitPrice > 0)
            {
                return price.UnitPrice;
            }

            return ComputeMovementUnitCost(product) ?? 0m;
        }

        private static decimal? ComputeMovementUnitCost(ProductDto product)
        {
            List<(decimal Quantity, decimal UnitPrice)> layers = product.Movements
                .Where(m => m.MovementType == ProductMovementType.Input && m.UnitPrice is > 0)
                .OrderBy(m => m.Date)
                .ThenBy(m => m.Id)
                .Select(m => (m.Quantity, m.UnitPrice!.Value))
                .ToList();

            if (layers.Count == 0)
            {
                return null;
            }

            decimal priorOutputs = product.Movements
                .Where(m => m.MovementType == ProductMovementType.Output)
                .Sum(m => m.Quantity);

            if (priorOutputs > 0)
            {
                decimal remaining = priorOutputs;
                while (remaining > 0 && layers.Count > 0)
                {
                    (decimal qty, decimal unitPrice) = layers[0];
                    decimal consumed = Math.Min(qty, remaining);
                    remaining -= consumed;

                    if (qty - consumed <= 0)
                    {
                        layers.RemoveAt(0);
                    }
                    else
                    {
                        layers[0] = (qty - consumed, unitPrice);
                    }
                }
            }

            decimal stockQuantity = layers.Sum(l => l.Quantity);
            if (stockQuantity <= 0)
            {
                return null;
            }

            decimal totalCost = layers.Sum(l => l.Quantity * l.UnitPrice);
            return Math.Round(totalCost / stockQuantity, 4);
        }

        private string GetProductName(Guid productId)
        {
            if (productId == Guid.Empty)
            {
                return string.Empty;
            }

            return _filteredProductsById.GetValueOrDefault(productId)?.Name ?? string.Empty;
        }

        private void WarnAndClearProductLine(int rowHandle, BaseEdit editor, Guid productId)
        {
            _lines[rowHandle].ProductId = Guid.Empty;
            _lines[rowHandle].UnitCost = 0m;
            _lines[rowHandle].AvailableStock = 0m;
            _lines[rowHandle].Description = string.Empty;

            editor.EditValue = null;
            gridLinesView.RefreshRow(rowHandle);

            string message = _allProductsById.ContainsKey(productId)
                ? "Ürün stokta bulunamadı (mevcut stok 0). Yalnızca stoklu ürünler seçilebilir."
                : "Ürün bulunamadı.";
            ToastHelper.Show(message, ToastType.Warning, 4000);
        }

        private void ReapplyAutoDescriptions()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                if (_lines[i].ProductId != Guid.Empty)
                {
                    UpdateAutoDescription(i);
                }
            }

            UpdateGeneralDescription();
        }

        private void UpdateAutoDescription(int rowHandle)
        {
            if (rowHandle < 0 || rowHandle >= _lines.Count)
            {
                return;
            }

            LineRow row = _lines[rowHandle];
            ProductDto? product = _filteredProductsById.GetValueOrDefault(row.ProductId);
            if (product is null)
            {
                return;
            }

            string targetName = GetSelectedTargetName();
            string action = IsConsumption ? "malzeme düşümü" : "malzeme transferi";
            string dateText = dtDate.DateTime.ToString("dd.MM.yyyy");
            string unit = string.IsNullOrWhiteSpace(product.ProductUnitTypeName)
                ? string.Empty
                : product.ProductUnitTypeName + " ";

            row.Description = targetName.Length == 0
                ? $"{row.Quantity:N0} {unit}{product.Name} - {dateText} tarihinde {action}"
                : $"{row.Quantity:N0} {unit}{product.Name} - {dateText} tarihinde {targetName} için {action}";

            gridLinesView.RefreshRow(rowHandle);
            UpdateGeneralDescription();
        }

        private void UpdateGeneralDescription()
        {
            if (_editing is not null)
            {
                return;
            }

            List<string> descriptions = _lines
                .Where(l => l.ProductId != Guid.Empty && !string.IsNullOrWhiteSpace(l.Description))
                .Select(l => l.Description)
                .ToList();

            memoDescription.Text = string.Join(" ; ", descriptions);
        }

        private string GetSelectedTargetName()
        {
            if (lookUpTarget.EditValue is not Guid targetId)
            {
                return string.Empty;
            }

            return _targetAccounts.FirstOrDefault(a => a.Id == targetId)?.Name ?? string.Empty;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_sourceWarehouseId == Guid.Empty)
            {
                ToastHelper.Show("Kaynak depo hesap planında bulunamadı.", ToastType.Warning);
                return;
            }

            if (lookUpTarget.EditValue is not Guid targetAccountId || targetAccountId == Guid.Empty)
            {
                ToastHelper.Show("Lütfen bir hedef hesap seçiniz.", ToastType.Warning);
                lookUpTarget.Focus();
                return;
            }

            string documentNumber = txtDocumentNumber.Text.Trim();
            if (string.IsNullOrEmpty(documentNumber))
            {
                ToastHelper.Show("Belge numarası boş olamaz.", ToastType.Warning);
                txtDocumentNumber.Focus();
                return;
            }

            List<LineRow> validLines = _lines
                .Where(l => l.ProductId != Guid.Empty && l.Quantity > 0)
                .ToList();

            if (validLines.Count == 0)
            {
                ToastHelper.Show("En az bir ürün satırı eklemelisiniz.", ToastType.Warning);
                return;
            }

            if (validLines.Count != _lines.Count)
            {
                ToastHelper.Show("Bazı satırlarda ürün veya miktar eksik. Lütfen tamamlayın.", ToastType.Warning);
                return;
            }

            StockCostingMethod costingMethod = cmbCosting.SelectedIndex == 1
                ? StockCostingMethod.Lifo
                : StockCostingMethod.Fifo;

            StockIssueCreateCommand command = new(
                IssueType: _issueType,
                SourceWarehouseId: _sourceWarehouseId,
                TargetAccountId: targetAccountId,
                Date: DateOnly.FromDateTime(dtDate.DateTime),
                DocumentNumber: documentNumber,
                CostingMethod: costingMethod,
                Description: memoDescription.Text.Trim(),
                Lines: validLines
                    .Select(l => new StockIssueCreateLine(l.ProductId, l.Quantity, l.Description))
                    .ToList());

            btnSave.Enabled = false;
            try
            {
                // Kaydetme TASLAK olarak yapar; stok hareketleri oluşmaz.
                // Onay ayrı adımdır ve liste üzerinden yapılır.
                bool ok = await CrudExecutor.ExecuteAsync(command);
                if (ok)
                {
                    _saved = true;
                    btnPrintSlip.Enabled = true;
                    btnAddLine.Enabled = false;
                    btnDeleteLine.Enabled = false;
                    dtDate.ReadOnly = true;
                    txtDocumentNumber.ReadOnly = true;
                    cmbCosting.ReadOnly = true;
                    lookUpTarget.ReadOnly = true;
                    memoDescription.ReadOnly = true;
                    gridLinesView.OptionsBehavior.Editable = false;
                }
            }
            finally
            {
                if (!_saved)
                {
                    btnSave.Enabled = true;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // TIF önizlemesi gibi iç içe modal pencereler, formun DialogResult değerini
            // None dışında bir değere çevirebilir. Kaydedilmiş bir form her durumda OK
            // döndürmeli ki liste kendini yenilesin.
            if (_saved)
            {
                DialogResult = DialogResult.OK;
            }

            base.OnFormClosing(e);
        }

        private async Task ShowSlipPreviewAsync()
        {
            await MovableAssetTransactionSlipPresenter.ShowAsync(await BuildSlipDataAsync());
        }

        private async Task<MovableAssetTransactionSlipData> BuildSlipDataAsync()
        {
            CompanyDto company = await LoadCompanyAsync();

            string targetName = GetSelectedTargetName();
            string targetCode = string.Empty;
            if (lookUpTarget.EditValue is Guid targetId)
            {
                ChartOfAccountLookUpDto? target = _targetAccounts.FirstOrDefault(a => a.Id == targetId);
                if (target is not null)
                {
                    targetCode = target.Code;
                    targetName = target.Name;
                }
            }

            ChartOfAccountLookUpDto? warehouse = _accounts.FirstOrDefault(a => a.Id == _sourceWarehouseId);
            string warehouseName = warehouse?.Name ?? _sourceWarehouseDisplay;
            string warehouseCode = warehouse?.Code ?? string.Empty;

            string city = string.IsNullOrWhiteSpace(company.City) ? string.Empty : company.City.Trim();
            string district = string.IsNullOrWhiteSpace(company.District) ? string.Empty : company.District.Trim();
            string ilIlce = city.Length > 0 && district.Length > 0 ? $"{city} / {district}" : city + district;

            MovableAssetTransactionSlipData data = new()
            {
                DocumentNumber = txtDocumentNumber.Text.Trim(),
                Date = dtDate.DateTime,
                OperationType = IsConsumption ? "Tüketim" : "Atölye Transferi",
                SourceParty = warehouseName,
                RecipientParty = targetName,
                DestinationParty = string.IsNullOrWhiteSpace(targetCode) ? targetName : $"{targetCode} - {targetName}",
                ProvinceDistrictName = ilIlce,
                ProvinceDistrictCode = string.Empty,
                ExpenditureUnitName = company.ExpenditureUnitName ?? string.Empty,
                ExpenditureUnitCode = company.ExpenditureUnitCode ?? string.Empty,
                StoreName = warehouseName,
                StoreCode = warehouseCode,
                AccountingUnitName = company.AccountingUnitName ?? string.Empty,
                AccountingUnitCode = company.AccountingUnitCode ?? string.Empty,
                ReferenceDate = dtDate.DateTime,
                ReferenceCode = txtDocumentNumber.Text.Trim(),
                AccountNames = MovableAssetTransactionSlipPresenter.BuildAccountNameMap(_accounts)
            };

            int order = 0;
            foreach (LineRow line in _lines.Where(l => l.ProductId != Guid.Empty && l.Quantity > 0))
            {
                ProductDto? product = _allProductsById.GetValueOrDefault(line.ProductId);
                order++;
                data.Rows.Add(new MovableAssetTransactionSlipRow
                {
                    RowNumber = order,
                    Code = MovableAssetTransactionSlipPresenter.ResolveItemCode(product, string.Empty),
                    WarehouseCode = product?.WarehouseCode ?? string.Empty,
                    WarehouseName = product?.WarehouseName ?? string.Empty,
                    Barcode = product?.Barcode ?? string.Empty,
                    Adi = product?.Name ?? string.Empty,
                    UnitOfMeasure = product?.ProductUnitTypeName ?? string.Empty,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitCost,
                    Amount = line.Quantity * line.UnitCost
                });
            }

            data.Prepare();

            return data;
        }

        private async Task<CompanyDto> LoadCompanyAsync()
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
                CrashLog.WriteException("SlipReport.Company", ex);
            }

            return new CompanyDto();
        }

private sealed class LineRow
            {
                public Guid ProductId { get; set; }
                public decimal Quantity { get; set; }
                public decimal UnitCost { get; set; }
                public decimal AvailableStock { get; set; }

                /// <summary>Satır tutarı; grid footer toplamı bu sütundan okunur.</summary>
                public decimal TotalAmount => Quantity * UnitCost;

                public string Description { get; set; } = string.Empty;
            }
    }
}
