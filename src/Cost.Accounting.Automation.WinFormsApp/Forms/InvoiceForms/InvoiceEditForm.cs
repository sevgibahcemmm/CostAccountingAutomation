using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Application.Invoices;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
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
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Data;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.InvoiceForms
{
    public partial class InvoiceEditForm : XtraForm
    {
        private readonly InvoiceDto? _editing;
        private readonly BindingList<InvoiceLineDto> _lines = [];
        private List<CustomerDto> _customers = [];
        private List<SupplierDto> _suppliers = [];
        private List<ProductDto> _products = [];
        private List<ChartOfAccountLookUpDto> _warehouses = [];
        private RepositoryItemSearchLookUpEdit _riProductLookUp = default!;

        public InvoiceEditForm() : this(null)
        {
        }

        public InvoiceEditForm(InvoiceDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            IconOptions.SvgImage = SvgIcons.Modules[4];
            lblHeaderIcon.ImageOptions.SvgImage = SvgIcons.Modules[4];

            Text = _editing is null ? "Yeni Fatura" : "Fatura Ä°ncele";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Soldan Ã¼rÃ¼n seÃ§in, sadece fiyat ve miktarÄ± girin"
                : _editing.Status == InvoiceStatus.Draft
                    ? "TaslaÄŸÄ± dÃ¼zenleyip kaydedebilir veya onaylayabilirsiniz"
                    : "Fatura ve kalem detaylarÄ±";

            InitControls();
            WireEvents();
        }

        private void InitControls()
        {
            cmbInvoiceType.Properties.Items.Clear();
            cmbInvoiceType.Properties.Items.Add("SatÄ±ÅŸ FaturasÄ±");
            cmbInvoiceType.Properties.Items.Add("SatÄ±n Alma FaturasÄ±");
            cmbInvoiceType.SelectedIndex = 1;

            dtDate.DateTime = DateTime.Today;
            txtInvoiceNumber.Text = $"FAT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
            gridLines.DataSource = _lines;
            ConfigureGrid();
            ConfigureCatalogGrid();

            btnApprove.Visible = false;
            lblStatusValue.Text = "";

            if (_editing is not null)
            {
                bool isDraft = _editing.Status == InvoiceStatus.Draft;

                btnSave.Visible = false;
                btnSaveDraft.Visible = isDraft;
                btnApprove.Visible = isDraft;
                btnAddLine.Enabled = isDraft;
                btnDeleteLine.Enabled = isDraft;
                btnAddProduct.Enabled = isDraft;
                gridLinesView.OptionsBehavior.Editable = isDraft;

                if (!isDraft)
                {
                    cmbInvoiceType.ReadOnly = true;
                    txtInvoiceNumber.ReadOnly = true;
                    dtDate.ReadOnly = true;
                    lookUpAccount.ReadOnly = true;
                    txtDescription.ReadOnly = true;
                }
            }
        }

        private void ConfigureGrid()
        {
            gridLinesView.RowHeight = 28;
            GridColumnFactory.ConfigureFromAttributes(gridLinesView, typeof(InvoiceLineDto));

            _riProductLookUp = new RepositoryItemSearchLookUpEdit
            {
                ValueMember = nameof(ProductDto.Id),
                DisplayMember = nameof(ProductDto.Name),
                NullText = "ÃœrÃ¼n SeÃ§iniz...",
                PopupFilterMode = PopupFilterMode.Contains
            };
            _riProductLookUp.View.Columns.AddField(nameof(ProductDto.ProductCode)).Caption = "ÃœrÃ¼n Kodu";
            _riProductLookUp.View.Columns.AddField(nameof(ProductDto.Name)).Caption = "ÃœrÃ¼n AdÄ±";
            _riProductLookUp.View.Columns.AddField(nameof(ProductDto.StockQuantity)).Caption = "Stok";
            _riProductLookUp.View.Columns[0].Visible = true;
            _riProductLookUp.View.Columns[1].Visible = true;
            _riProductLookUp.View.Columns[2].Visible = true;
            _riProductLookUp.EditValueChanged += RiProductLookUp_EditValueChanged;

            RepositoryItemSpinEdit riQuantity = new() { MinValue = 0.0001m, MaxValue = 999999999, Increment = 1 };
            RepositoryItemSpinEdit riPrice = new() { MinValue = 0, MaxValue = 999999999, Increment = 10, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemSpinEdit riDiscountRate = new() { MinValue = 0, MaxValue = 100, Increment = 1, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n0" } };
            RepositoryItemSpinEdit riTaxRate = new() { MinValue = 0, MaxValue = 100, Increment = 1, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n0" } };
            RepositoryItemSpinEdit riReadOnlyMoney = new() { ReadOnly = true, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemTextEdit riDesc = new();

            gridLines.RepositoryItems.AddRange([_riProductLookUp, riQuantity, riPrice, riDiscountRate, riTaxRate, riReadOnlyMoney, riDesc]);

            foreach (GridColumn column in gridLinesView.Columns)
            {
                column.ColumnEdit = column.FieldName switch
                {
                    nameof(InvoiceLineDto.ProductId) => _riProductLookUp,
                    nameof(InvoiceLineDto.Quantity) => riQuantity,
                    nameof(InvoiceLineDto.UnitPrice) => riPrice,
                    nameof(InvoiceLineDto.DiscountRate) => riDiscountRate,
                    nameof(InvoiceLineDto.DiscountAmount) => riReadOnlyMoney,
                    nameof(InvoiceLineDto.TaxRateRate) => riTaxRate,
                    nameof(InvoiceLineDto.TaxAmount) => riReadOnlyMoney,
                    nameof(InvoiceLineDto.TotalAmount) => riReadOnlyMoney,
                    nameof(InvoiceLineDto.Description) => riDesc,
                    _ => column.ColumnEdit
                };
            }
        }

        private void ConfigureCatalogGrid()
        {
            gridCatalogView.Columns.Clear();
            gridCatalogView.OptionsBehavior.AutoPopulateColumns = false;

            GridColumn[] columns =
            [
                new() { Caption = "ÃœrÃ¼n AdÄ±", FieldName = nameof(ProductDto.Name), Visible = true, Width = 160 },
                new() { Caption = "ÃœrÃ¼n Kodu", FieldName = nameof(ProductDto.ProductCode), Visible = true, Width = 90 },
                new() { Caption = "Depo", FieldName = nameof(ProductDto.WarehouseName), Visible = true, Width = 90 },
                new() { Caption = "Birim", FieldName = nameof(ProductDto.ProductUnitTypeName), Visible = true, Width = 55 },
                new() { Caption = "KDV %", FieldName = nameof(ProductDto.TaxRateRate), Visible = true, Width = 60, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "p0" } },
                new() { Caption = "Stok", FieldName = nameof(ProductDto.StockQuantity), Visible = true, Width = 70, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" } },
                new() { Caption = "AlÄ±ÅŸ FiyatÄ±", FieldName = "PurchasePriceUnbound", UnboundDataType = typeof(decimal), Visible = true, Width = 85, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "SatÄ±ÅŸ FiyatÄ±", FieldName = "SalePriceUnbound", UnboundDataType = typeof(decimal), Visible = true, Width = 85, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } }
            ];

            gridCatalogView.Columns.AddRange(columns);
            gridCatalogView.CustomUnboundColumnData += GridCatalogView_CustomUnboundColumnData;
        }

        private void GridCatalogView_CustomUnboundColumnData(object? sender, CustomColumnDataEventArgs e)
        {
            if (!e.IsGetData || e.ListSourceRowIndex < 0)
            {
                return;
            }

            if (gridCatalogView.GetRow(e.ListSourceRowIndex) is not ProductDto product)
            {
                return;
            }

            e.Value = e.Column.FieldName switch
            {
                "PurchasePriceUnbound" => product.Prices
                    .FirstOrDefault(p => p.PriceType == ProductPriceType.Purchase)?.UnitPrice ?? 0m,
                "SalePriceUnbound" => product.Prices
                    .FirstOrDefault(p => p.PriceType == ProductPriceType.Sale)?.UnitPrice ?? 0m,
                _ => null
            };
        }

        private void WireEvents()
        {
            Load += InvoiceEditForm_Load;
            cmbInvoiceType.SelectedIndexChanged += CmbInvoiceType_SelectedIndexChanged;
            btnAddLine.Click += (_, _) => AddEmptyLine();
            btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
            btnAddProduct.Click += (_, _) => AddSelectedCatalogProduct();
            btnSave.Click += BtnSave_Click;
            btnSaveDraft.Click += BtnSaveDraft_Click;
            btnApprove.Click += BtnApprove_Click;
            btnCancel.Click += (_, _) => Close();
            gridLinesView.CellValueChanged += GridLinesView_CellValueChanged;
            gridLinesView.ValidatingEditor += GridLinesView_ValidatingEditor;
            gridCatalogView.DoubleClick += GridCatalogView_DoubleClick;
            cmbCatalogWarehouse.EditValueChanged += CmbCatalogWarehouse_EditValueChanged;
        }

        private async void InvoiceEditForm_Load(object? sender, EventArgs e)
        {
            await LoadLookUpsAsync();

            if (_editing is not null)
            {
                PopulateExisting(_editing);
            }
            else
            {
                AddEmptyLine();
            }
        }

        private async Task LoadLookUpsAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                _customers = (await mediator.Send(new CustomerGetAllQuery())).ToList();
                _suppliers = (await mediator.Send(new SupplierGetAllQuery())).ToList();
                _products = (await mediator.Send(new ProductGetAllQuery())).ToList();
                _warehouses = ((await mediator.Send(new ChartOfAccountLookUpQuery())).Data ?? [])
                    .Where(w => w.Type == ChartOfAccountType.Warehouse)
                    .ToList();

                _riProductLookUp.DataSource = _products;
                UpdateAccountDataSource();

                cmbCatalogWarehouse.Properties.DataSource = _warehouses;
                cmbCatalogWarehouse.Properties.ValueMember = nameof(ChartOfAccountLookUpDto.Id);
                cmbCatalogWarehouse.Properties.DisplayMember = nameof(ChartOfAccountLookUpDto.Display);
                cmbCatalogWarehouse.Properties.PopupFilterMode = PopupFilterMode.Contains;
                cmbCatalogWarehouse.Properties.BestFitMode = BestFitMode.BestFit;
                cmbCatalogWarehouseView.Columns.Clear();
                GridColumn whColumn = cmbCatalogWarehouseView.Columns.AddField(nameof(ChartOfAccountLookUpDto.Display));
                whColumn.Caption = "Depo";
                whColumn.VisibleIndex = 0;
                whColumn.Width = 300;
                cmbCatalogWarehouseView.BestFitColumns();

                gridCatalog.DataSource = _products;
                gridCatalogView.BestFitColumns();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Veriler yÃ¼klenirken hata oluÅŸtu: " + ex.Message, ToastType.Error);
            }
        }

        private void CmbInvoiceType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateAccountDataSource();
        }

        private void UpdateAccountDataSource()
        {
            bool isSales = cmbInvoiceType.SelectedIndex == 0;
            lblAccountLabel.Text = isSales ? "MÃ¼ÅŸteri:" : "TedarikÃ§i:";

            lookUpAccount.Properties.DataSource = null;
            lookUpAccountView.Columns.Clear();
            lookUpAccountView.OptionsBehavior.AutoPopulateColumns = false;

            if (isSales)
            {
                lookUpAccount.Properties.DataSource = _customers;
                lookUpAccount.Properties.ValueMember = nameof(CustomerDto.Id);
                lookUpAccount.Properties.DisplayMember = nameof(CustomerDto.Name);

                lookUpAccountView.Columns.AddField(nameof(CustomerDto.Name)).Caption = "MÃ¼ÅŸteri AdÄ±";
                lookUpAccountView.Columns.AddField(nameof(CustomerDto.TaxNumber)).Caption = "Vergi No";
                lookUpAccountView.Columns.AddField(nameof(CustomerDto.City)).Caption = "Åehir";
            }
            else
            {
                lookUpAccount.Properties.DataSource = _suppliers;
                lookUpAccount.Properties.ValueMember = nameof(SupplierDto.Id);
                lookUpAccount.Properties.DisplayMember = nameof(SupplierDto.Name);

                lookUpAccountView.Columns.AddField(nameof(SupplierDto.Name)).Caption = "TedarikÃ§i AdÄ±";
                lookUpAccountView.Columns.AddField(nameof(SupplierDto.TaxNumber)).Caption = "Vergi No";
                lookUpAccountView.Columns.AddField(nameof(SupplierDto.City)).Caption = "Åehir";
            }

            foreach (GridColumn col in lookUpAccountView.Columns)
            {
                col.Visible = true;
            }

            lookUpAccount.Properties.BestFitMode = BestFitMode.BestFit;
        }

        private void PopulateExisting(InvoiceDto invoice)
        {
            cmbInvoiceType.SelectedIndex = invoice.InvoiceType == InvoiceType.Sales ? 0 : 1;
            txtInvoiceNumber.Text = invoice.InvoiceNumber;
            dtDate.DateTime = invoice.Date.ToDateTime(TimeOnly.MinValue);
            lookUpAccount.EditValue = invoice.InvoiceType == InvoiceType.Sales ? invoice.CustomerId : invoice.SupplierId;
            txtDescription.Text = invoice.Description;

            lblStatusValue.Text = "Durum: " + invoice.StatusName;
            lblStatusValue.Appearance.ForeColor = invoice.Status == InvoiceStatus.Approved
                ? SkinTheme.Success
                : SkinTheme.Warning;

            _lines.Clear();
            foreach (var l in invoice.Lines)
            {
                _lines.Add(new InvoiceLineDto
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountRate = l.DiscountRate,
                    TaxRateRate = l.TaxRateRate,
                    Description = l.Description
                });
            }

            RecalculateTotals();
        }

        private void AddEmptyLine()
        {
            _lines.Add(new InvoiceLineDto
            {
                Quantity = 1,
                UnitPrice = 0,
                TaxRateRate = 20
            });
            gridLinesView.FocusedRowHandle = _lines.Count - 1;
        }

        private void DeleteSelectedLine()
        {
            int rowHandle = gridLinesView.FocusedRowHandle;
            if (rowHandle >= 0 && rowHandle < _lines.Count)
            {
                _lines.RemoveAt(rowHandle);
                RecalculateTotals();
            }
        }

        private void CmbCatalogWarehouse_EditValueChanged(object? sender, EventArgs e)
        {
            if (gridCatalogView.GridControl == null)
            {
                return;
            }

            if (cmbCatalogWarehouse.EditValue is Guid warehouseId)
            {
                gridCatalogView.ActiveFilterString = $"[{nameof(ProductDto.WarehouseId)}] = '{warehouseId}'";
            }
            else
            {
                gridCatalogView.ActiveFilterString = "";
            }
        }

        private void GridCatalogView_DoubleClick(object? sender, EventArgs e)
        {
            AddSelectedCatalogProduct();
        }

        private void AddSelectedCatalogProduct()
        {
            if (gridCatalogView.GetFocusedRow() is not ProductDto product)
            {
                ToastHelper.Show("LÃ¼tfen listeden bir Ã¼rÃ¼n seÃ§in.", ToastType.Warning);
                return;
            }

            int existingRow = _lines.ToList().FindIndex(l => l.ProductId == product.Id);
            if (existingRow >= 0)
            {
                ToastHelper.Show($"'{product.Name}' faturaya zaten eklenmiÅŸ.", ToastType.Warning);
                gridLinesView.FocusedRowHandle = existingRow;
                return;
            }

            bool isSales = cmbInvoiceType.SelectedIndex == 0;
            ProductPriceDto? price = product.Prices
                .Where(p => p.PriceType == (isSales ? ProductPriceType.Sale : ProductPriceType.Purchase))
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            _lines.Add(new InvoiceLineDto
            {
                ProductId = product.Id,
                Quantity = 1,
                UnitPrice = price?.UnitPrice ?? 0,
                TaxRateRate = product.TaxRateRate > 0 && product.TaxRateRate <= 1 ? product.TaxRateRate * 100 : product.TaxRateRate,
                Description = product.ProductCode
            });

            gridLinesView.FocusedRowHandle = _lines.Count - 1;
            RecalculateTotals();
        }

        private void RiProductLookUp_EditValueChanged(object? sender, EventArgs e)
        {
            if (sender is SearchLookUpEdit edit && edit.EditValue is Guid productId)
            {
                ProductDto? prod = _products.FirstOrDefault(p => p.Id == productId);
                if (prod is not null)
                {
                    int rowHandle = gridLinesView.FocusedRowHandle;
                    if (rowHandle >= 0 && rowHandle < _lines.Count)
                    {
                        InvoiceLineDto line = _lines[rowHandle];

                        InvoiceLineDto? duplicate = _lines.FirstOrDefault(l => l != line && l.ProductId == productId);
                        if (duplicate is not null)
                        {
                            ToastHelper.Show($"'{prod.Name}' faturaya zaten eklenmiÅŸ.", ToastType.Warning);
                            edit.EditValue = line.ProductId == Guid.Empty ? null : line.ProductId;
                            return;
                        }

                        line.ProductId = prod.Id;
                        line.TaxRateRate = prod.TaxRateRate > 0 && prod.TaxRateRate <= 1 ? prod.TaxRateRate * 100 : prod.TaxRateRate;

                        bool isSales = cmbInvoiceType.SelectedIndex == 0;
                        var priceObj = prod.Prices
                            .Where(p => p.PriceType == (isSales ? ProductPriceType.Sale : ProductPriceType.Purchase))
                            .OrderByDescending(p => p.StartDate)
                            .FirstOrDefault();

                        line.UnitPrice = priceObj?.UnitPrice ?? 0;

                        gridLinesView.RefreshRow(rowHandle);
                        RecalculateTotals();
                    }
                }
            }
        }

        private void GridLinesView_ValidatingEditor(object? sender, BaseContainerValidateEditorEventArgs e)
        {
            string? columnName = gridLinesView.FocusedColumn?.FieldName;

            if (columnName == nameof(InvoiceLineDto.UnitPrice) && e.Value is decimal price && price <= 0)
            {
                e.Valid = false;
                e.ErrorText = "Birim fiyat sÄ±fÄ±rdan bÃ¼yÃ¼k olmalÄ±dÄ±r.";
                return;
            }

            if (columnName == nameof(InvoiceLineDto.Quantity) && e.Value is decimal qty && qty <= 0)
            {
                e.Valid = false;
                e.ErrorText = "Miktar sÄ±fÄ±rdan bÃ¼yÃ¼k olmalÄ±dÄ±r.";
                return;
            }

            if (columnName == nameof(InvoiceLineDto.DiscountRate) && e.Value is decimal discount && (discount < 0 || discount > 100))
            {
                e.Valid = false;
                e.ErrorText = "Ä°skonto oranÄ± %0 ile %100 arasÄ±nda olmalÄ±dÄ±r.";
                return;
            }

            if (columnName != nameof(InvoiceLineDto.ProductId)
                || e.Value is not Guid newProductId
                || newProductId == Guid.Empty)
            {
                return;
            }

            int dataRow = gridLinesView.GetDataSourceRowIndex(gridLinesView.FocusedRowHandle);
            if (dataRow < 0 || gridLinesView.GetRow(gridLinesView.FocusedRowHandle) is not InvoiceLineDto current)
            {
                return;
            }

            bool duplicate = _lines.Any(l => l != current && l.ProductId == newProductId);
            if (!duplicate)
            {
                return;
            }

            ToastHelper.Show($"'{_products.FirstOrDefault(p => p.Id == newProductId)?.Name ?? "ÃœrÃ¼n"}' faturaya zaten eklenmiÅŸ.", ToastType.Warning);
            e.Valid = false;
            e.ErrorText = "Bu Ã¼rÃ¼n faturada zaten mevcut.";
        }

        private void GridLinesView_CellValueChanged(object? sender, CellValueChangedEventArgs e)
        {
            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            decimal subTotal = 0;
            decimal discountTotal = 0;
            decimal taxTotal = 0;
            Dictionary<decimal, (decimal Matrah, decimal Kdv)> byRate = [];

            foreach (var line in _lines)
            {
                decimal lineSub = line.Quantity * line.UnitPrice;
                decimal discountRate = line.DiscountRate > 1 && line.DiscountRate <= 100 ? line.DiscountRate / 100m : line.DiscountRate;
                decimal discountAmount = Math.Round(lineSub * discountRate, 2);
                decimal netAmount = lineSub - discountAmount;
                decimal rate = line.TaxRateRate > 1 ? line.TaxRateRate / 100m : line.TaxRateRate;
                decimal tax = Math.Round(netAmount * rate, 2);

                line.TaxAmount = tax;
                line.TotalAmount = netAmount + tax;

                subTotal += lineSub;
                discountTotal += discountAmount;
                taxTotal += tax;

                if (!byRate.TryGetValue(line.TaxRateRate, out (decimal Matrah, decimal Kdv) acc))
                {
                    acc = (0, 0);
                }
                acc.Matrah += netAmount;
                acc.Kdv += tax;
                byRate[line.TaxRateRate] = acc;
            }

            lblSubTotalValue.Text = subTotal.ToString("n2") + " â‚º";
            lblDiscountTotalValue.Text = "- " + discountTotal.ToString("n2") + " â‚º";
            lblTaxTotalValue.Text = taxTotal.ToString("n2") + " â‚º";
            lblGrandTotalValue.Text = (subTotal - discountTotal + taxTotal).ToString("n2") + " â‚º";

            var breakdown = byRate
                .OrderByDescending(kv => kv.Key)
                .Select(kv => $"KDV %{kv.Key:n0}  â”‚  Matrah: {kv.Value.Matrah:n2} â‚º  â”‚  KDV: {kv.Value.Kdv:n2} â‚º");

            lblTaxBreakdown.Text = breakdown.Any() ? string.Join(Environment.NewLine, breakdown) : "Kalem ekleyin.";

            gridLinesView.RefreshData();
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            await SaveAsync(approve: true);
        }

        private async void BtnSaveDraft_Click(object? sender, EventArgs e)
        {
            await SaveAsync(approve: false);
        }

        private async Task SaveAsync(bool approve)
        {
            string number = txtInvoiceNumber.Text.Trim();
            if (string.IsNullOrEmpty(number))
            {
                ToastHelper.Show("Fatura numarasÄ± boÅŸ olamaz.", ToastType.Warning);
                txtInvoiceNumber.Focus();
                return;
            }

            if (lookUpAccount.EditValue is not Guid accountId || accountId == Guid.Empty)
            {
                ToastHelper.Show("LÃ¼tfen bir cari (MÃ¼ÅŸteri/TedarikÃ§i) seÃ§iniz.", ToastType.Warning);
                lookUpAccount.Focus();
                return;
            }

            var validLines = _lines
                .Where(l => l.ProductId != Guid.Empty && l.Quantity > 0 && l.UnitPrice > 0)
                .ToList();
            if (validLines.Count == 0 || validLines.Count != _lines.Count(l => l.ProductId != Guid.Empty))
            {
                ToastHelper.Show("Miktar ve fiyat sÄ±fÄ±rdan bÃ¼yÃ¼k olmalÄ±dÄ±r. Eksik kalemleri tamamlayÄ±n.", ToastType.Warning);
                return;
            }

            foreach (var line in validLines)
            {
                if (line.DiscountRate < 0 || line.DiscountRate > 100)
                {
                    ToastHelper.Show("Ä°skonto oranÄ± %0 ile %100 arasÄ±nda olmalÄ±dÄ±r.", ToastType.Warning);
                    return;
                }
            }

            InvoiceType invoiceType = cmbInvoiceType.SelectedIndex == 0 ? InvoiceType.Sales : InvoiceType.Purchase;
            DateOnly date = DateOnly.FromDateTime(dtDate.DateTime);

            List<InvoiceCreateLineModel> lineModels = validLines.Select(l => new InvoiceCreateLineModel(
                l.ProductId,
                l.Quantity,
                l.UnitPrice,
                l.TaxRateRate,
                l.Description,
                l.DiscountRate)).ToList();

            btnSave.Enabled = false;
            btnSaveDraft.Enabled = false;
            try
            {
                bool ok;
                if (_editing is { Status: InvoiceStatus.Draft } editing)
                {
                    ok = await CrudExecutor.ExecuteAsync(new InvoiceUpdateCommand(
                        Id: editing.Id,
                        InvoiceNumber: number,
                        InvoiceType: invoiceType,
                        Date: date,
                        CustomerId: invoiceType == InvoiceType.Sales ? accountId : null,
                        SupplierId: invoiceType == InvoiceType.Purchase ? accountId : null,
                        Description: txtDescription.Text.Trim(),
                        Lines: lineModels));
                }
                else
                {
                    InvoiceCreateCommand command = new(
                        InvoiceNumber: number,
                        InvoiceType: invoiceType,
                        Date: date,
                        CustomerId: invoiceType == InvoiceType.Sales ? accountId : null,
                        SupplierId: invoiceType == InvoiceType.Purchase ? accountId : null,
                        Description: txtDescription.Text.Trim(),
                        Lines: lineModels,
                        IsApproved: approve);

                    ok = await CrudExecutor.ExecuteAsync(command);
                }

                if (ok)
                {
                    ToastHelper.Show(_editing is { Status: InvoiceStatus.Draft }
                        ? "Fatura taslaÄŸÄ± gÃ¼ncellendi."
                        : approve
                            ? "Fatura onaylandÄ±; stok ve cari hareketleri oluÅŸturuldu."
                            : "Fatura taslak olarak kaydedildi.", ToastType.Success);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            finally
            {
                btnSave.Enabled = true;
                btnSaveDraft.Enabled = true;
            }
        }

        private async void BtnApprove_Click(object? sender, EventArgs e)
        {
            if (_editing is null)
            {
                return;
            }

            btnApprove.Enabled = false;
            try
            {
                bool ok = await CrudExecutor.ExecuteAsync(new InvoiceApproveCommand(_editing.Id));
                if (ok)
                {
                    ToastHelper.Show("Fatura onaylandÄ±; stok ve cari hareketleri oluÅŸturuldu.", ToastType.Success);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            finally
            {
                btnApprove.Enabled = true;
            }
        }
    }
}
