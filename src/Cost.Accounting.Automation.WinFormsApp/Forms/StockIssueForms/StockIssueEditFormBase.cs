using System.ComponentModel;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public abstract class StockIssueEditFormBase : XtraForm
    {
        private readonly StockIssueType _issueType;
        private readonly StockIssueListDto? _editing;
        private readonly BindingList<LineRow> _lines = [];

        private List<ProductDto> _allProducts = [];
        private List<ProductDto> _filteredProducts = [];
        private List<ChartOfAccountLookUpDto> _accounts = [];
        private List<ChartOfAccountLookUpDto> _targetAccounts = [];
        private Guid _sourceWarehouseId;
        private string _sourceWarehouseDisplay = string.Empty;

        private Label lblTitle = default!;
        private Label lblSubtitle = default!;
        private Label lblDate = default!;
        private Label lblDocumentNumber = default!;
        private Label lblCosting = default!;
        private Label lblWarehouse = default!;
        private Label lblTarget = default!;
        private Label lblDescription = default!;
        private Label lblLines = default!;
        private Label lblTotalCaption = default!;
        private Label lblTotalValue = default!;
        private DateEdit dtDate = default!;
        private TextEdit txtDocumentNumber = default!;
        private TextEdit txtWarehouse = default!;
        private ComboBoxEdit cmbCosting = default!;
        private SearchLookUpEdit lookUpTarget = default!;
        private MemoEdit memoDescription = default!;
        private GridControl gridLinesControl = default!;
        private GridView gridLinesView = default!;
        private SimpleButton btnAddLine = default!;
        private SimpleButton btnDeleteLine = default!;
        private SimpleButton btnSave = default!;
        private SimpleButton btnCancel = default!;
        private RepositoryItemSearchLookUpEdit riProduct = default!;

        protected StockIssueEditFormBase(StockIssueType issueType, StockIssueListDto? existing)
        {
            _issueType = issueType;
            _editing = existing;

            BuildLayout();
            WireEvents();
        }

        private bool IsConsumption => _issueType == StockIssueType.Consumption;

        private string ExpectedWarehouseCode => IsConsumption ? "150" : "150.98";

        private string FormTitle => IsConsumption ? "Tüketim" : "Atölye Transferi";

        private void BuildLayout()
        {
            SuspendLayout();

            Text = _editing is null ? $"Yeni {FormTitle}" : $"{FormTitle} İncele";
            IconOptions.SvgImage = DxIcon.StockIssue;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(960, 660);
            Font = new Font("Segoe UI", 9F);

            lblTitle = new Label
            {
                Text = Text,
                Location = new Point(24, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            };

            lblSubtitle = new Label
            {
                Text = IsConsumption
                    ? "150 deposundan 900 (Tüketimler) altındaki bir tüketim birimine kayıt oluşturun"
                    : "150.98 deposundan 150.55 altındaki atölyeye transfer kaydı oluşturun",
                Location = new Point(26, 50),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            lblDate = MakeLabel("Tarih:", 24, 92);
            dtDate = new DateEdit { Location = new Point(96, 89), Size = new Size(140, 24) };

            lblDocumentNumber = MakeLabel("Belge No:", 260, 92);
            txtDocumentNumber = new TextEdit { Location = new Point(336, 89), Size = new Size(190, 24) };

            lblCosting = MakeLabel("Değerleme:", 550, 92);
            cmbCosting = new ComboBoxEdit { Location = new Point(640, 89), Size = new Size(140, 24) };
            cmbCosting.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;

            lblWarehouse = MakeLabel("Kaynak Depo:", 24, 132);
            txtWarehouse = new TextEdit { Location = new Point(140, 129), Size = new Size(300, 24), ReadOnly = true };

            lblTarget = MakeLabel("Hedef Hesap:", 470, 132);
            lookUpTarget = new SearchLookUpEdit { Location = new Point(580, 129), Size = new Size(356, 24) };

            lblDescription = MakeLabel("Açıklama:", 24, 172);
            memoDescription = new MemoEdit { Location = new Point(140, 169), Size = new Size(796, 46) };

            lblLines = MakeLabel("Kalemler:", 24, 228);

            gridLinesControl = new GridControl
            {
                Location = new Point(24, 252),
                Size = new Size(912, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            gridLinesView = new GridView();
            gridLinesControl.MainView = gridLinesView;
            gridLinesControl.ViewCollection.Add(gridLinesView);

            btnAddLine = new SimpleButton
            {
                Text = "Satır Ekle",
                Location = new Point(24, 562),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            btnDeleteLine = new SimpleButton
            {
                Text = "Satır Sil",
                Location = new Point(142, 562),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };

            lblTotalCaption = new Label
            {
                Text = "Toplam Tutar:",
                Location = new Point(640, 568),
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            lblTotalValue = new Label
            {
                Text = "0,00",
                Location = new Point(740, 568),
                Size = new Size(196, 20),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };

            btnSave = new SimpleButton
            {
                Text = "Kaydet",
                Location = new Point(744, 612),
                Size = new Size(92, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnCancel = new SimpleButton
            {
                Text = "Kapat",
                Location = new Point(844, 612),
                Size = new Size(92, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };

            Controls.AddRange([
                lblTitle, lblSubtitle, lblDate, dtDate, lblDocumentNumber, txtDocumentNumber,
                lblCosting, cmbCosting, lblWarehouse, txtWarehouse, lblTarget, lookUpTarget,
                lblDescription, memoDescription, lblLines, gridLinesControl,
                btnAddLine, btnDeleteLine, lblTotalCaption, lblTotalValue, btnSave, btnCancel
            ]);

            ConfigureGrid();

            dtDate.DateTime = DateTime.Today;
            cmbCosting.Properties.Items.Add("FIFO");
            cmbCosting.Properties.Items.Add("LIFO");
            cmbCosting.SelectedIndex = 0;

            if (_editing is not null)
            {
                btnSave.Visible = false;
                dtDate.ReadOnly = true;
                txtDocumentNumber.ReadOnly = true;
                cmbCosting.ReadOnly = true;
                lookUpTarget.ReadOnly = true;
                memoDescription.ReadOnly = true;
                btnAddLine.Enabled = false;
                btnDeleteLine.Enabled = false;
                gridLinesView.OptionsBehavior.Editable = false;
            }

            ResumeLayout(false);
        }

        private static Label MakeLabel(string text, int x, int y)
            => new() { Text = text, Location = new Point(x, y), AutoSize = true };

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

            gridLinesView.Columns.AddRange([productColumn, quantityColumn, unitCostColumn, stockColumn, descriptionColumn]);

            foreach (GridColumn col in gridLinesView.Columns)
            {
                col.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }
        }

        private void WireEvents()
        {
            Load += StockIssueEditForm_Load;
            btnAddLine.Click += (_, _) => AddEmptyLine();
            btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
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

        private void UpdateTotal()
        {
            decimal total = _lines.Sum(l => l.Quantity * l.UnitCost);
            lblTotalValue.Text = total.ToString("n2");
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
                ProductDto? selectedProduct = _filteredProducts.FirstOrDefault(p => p.Id == row.ProductId);
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
            ProductDto? product = _filteredProducts.FirstOrDefault(p => p.Id == productId);
            if (product is null)
            {
                WarnAndClearProductLine(rowHandle, edit, productId);
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

            ProductDto? product = _filteredProducts.FirstOrDefault(p => p.Id == productId);
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

            return _filteredProducts.FirstOrDefault(p => p.Id == productId)?.Name ?? string.Empty;
        }

        private void WarnAndClearProductLine(int rowHandle, BaseEdit editor, Guid productId)
        {
            _lines[rowHandle].ProductId = Guid.Empty;
            _lines[rowHandle].UnitCost = 0m;
            _lines[rowHandle].AvailableStock = 0m;
            _lines[rowHandle].Description = string.Empty;

            editor.EditValue = null;
            gridLinesView.RefreshRow(rowHandle);

            string message = _allProducts.Any(p => p.Id == productId)
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
            ProductDto? product = _filteredProducts.FirstOrDefault(p => p.Id == row.ProductId);
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

        private sealed class LineRow
        {
            public Guid ProductId { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitCost { get; set; }
            public decimal AvailableStock { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}
