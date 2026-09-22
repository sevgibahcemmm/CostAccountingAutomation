using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Application.Invoices;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraReports.UI;
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
        private Dictionary<Guid, ProductDto> _productsById = [];
        private List<ChartOfAccountLookUpDto> _accounts = [];
        private List<ChartOfAccountLookUpDto> _warehouses = [];
        private RepositoryItemSearchLookUpEdit _riProductLookUp = default!;
        private string? _lastAutoDescription;
        private bool _saved;

        public InvoiceEditForm() : this(null)
        {
        }

        public InvoiceEditForm(InvoiceDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            Text = _editing is null ? "Yeni Fatura" : "Fatura İncele";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Soldan ürün seçin, sadece fiyat ve miktarı girin"
                : _editing.Status == InvoiceStatus.Draft
                    ? "Taslağı düzenleyip kaydedebilir veya onaylayabilirsiniz"
                    : "Fatura ve kalem detayları";

            InitControls();
            WireEvents();
            UpdateAutoDescription();
        }

        private void InitControls()
        {
            cmbInvoiceType.Properties.Items.Clear();
            cmbInvoiceType.Properties.Items.Add("Satış Faturası");
            cmbInvoiceType.Properties.Items.Add("Satın Alma Faturası");
            cmbInvoiceType.SelectedIndex = 1;

            dtDate.DateTime = DateTime.Today;
            txtInvoiceNumber.Text = $"FAT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
            gridLines.DataSource = _lines;
            ConfigureGrid();
            ConfigureCatalogGrid();

            btnApprove.Visible = false;
            btnPrintSlip.Visible = cmbInvoiceType.SelectedIndex == 1;
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
                    btnPrintSlip.Enabled = true;
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
                NullText = "Ürün Seçiniz...",
                PopupFilterMode = PopupFilterMode.Contains
            };
            // Popup"ta yalnızca ürün görünsün: gereksiz alt-detail kolonları (Ürün Kodu, Stok) listelenmesin.
            _riProductLookUp.View.OptionsBehavior.AutoPopulateColumns = false;
            _riProductLookUp.View.Columns.AddField(nameof(ProductDto.Name)).Caption = "Ürün Adı";
            _riProductLookUp.View.Columns[0].Visible = true;
            _riProductLookUp.EditValueChanged += RiProductLookUp_EditValueChanged;

            RepositoryItemSpinEdit riQuantity = new() { MinValue = 0.0001m, MaxValue = 999999999, Increment = 1 };
            RepositoryItemSpinEdit riPrice = new() { MinValue = 0, MaxValue = 999999999, Increment = 10, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemSpinEdit riDiscountRate = new() { MinValue = 0, MaxValue = 100, Increment = 1, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n0" } };
            RepositoryItemSpinEdit riTaxRate = new() { MinValue = 0, MaxValue = 100, Increment = 1, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n0" } };
            RepositoryItemSpinEdit riReadOnlyMoney = new() { ReadOnly = true, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemTextEdit riDesc = new();

            gridLines.RepositoryItems.AddRange([_riProductLookUp, riQuantity, riPrice, riDiscountRate, riTaxRate, riReadOnlyMoney, riDesc]);

            // ConfigureFromAttributes DevExpress'in dar varsayılan sütun genişliklerini
            // kullanır; okunabilirlik için burada gerçekçi genişlikler veriyoruz.
            Dictionary<string, int> columnWidths = new()
            {
                [nameof(InvoiceLineDto.ProductId)] = 260,
                [nameof(InvoiceLineDto.Quantity)] = 90,
                [nameof(InvoiceLineDto.UnitPrice)] = 110,
                [nameof(InvoiceLineDto.DiscountRate)] = 90,
                [nameof(InvoiceLineDto.DiscountAmount)] = 110,
                [nameof(InvoiceLineDto.TaxRateRate)] = 80,
                [nameof(InvoiceLineDto.TaxAmount)] = 110,
                [nameof(InvoiceLineDto.TotalAmount)] = 130,
            };

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

                if (columnWidths.TryGetValue(column.FieldName, out int width))
                {
                    column.Width = width;
                    column.OptionsColumn.FixedWidth = true;
                }
                else if (column.FieldName == nameof(InvoiceLineDto.Description))
                {
                    // Açıklama sütunu kalan tüm boşluğu doldursun.
                    column.MinWidth = 220;
                    column.OptionsColumn.FixedWidth = false;
                }

                bool isMoneyColumn = column.FieldName is nameof(InvoiceLineDto.UnitPrice)
                    or nameof(InvoiceLineDto.DiscountAmount)
                    or nameof(InvoiceLineDto.TaxAmount)
                    or nameof(InvoiceLineDto.TotalAmount);
                bool isNumericColumn = isMoneyColumn || column.FieldName is nameof(InvoiceLineDto.Quantity)
                    or nameof(InvoiceLineDto.DiscountRate)
                    or nameof(InvoiceLineDto.TaxRateRate);

                if (isNumericColumn)
                {
                    column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                }
                column.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }

            // Sütunlar arttıkça grid'in tamamı kullanılsın, Açıklama sütunu esnesin.
            gridLinesView.OptionsView.ColumnAutoWidth = true;
            gridLinesView.OptionsCustomization.AllowColumnResizing = true;
        }

        private void ConfigureCatalogGrid()
        {
            gridCatalogView.Columns.Clear();
            gridCatalogView.OptionsBehavior.AutoPopulateColumns = false;
            // ProductDto içindeki liste tipi alanlar (ör. Prices) DevExpress tarafından
            // otomatik master-detail olarak algılanıp satırlara "+" ekleyebiliyor; kapatıyoruz.
            gridCatalogView.OptionsDetail.EnableMasterViewMode = false;

            GridColumn[] columns =
            [
                new() { Caption = "Ürün Adı", FieldName = nameof(ProductDto.Name), Visible = true, Width = 220 },
                new() { Caption = "Ürün Kodu", FieldName = nameof(ProductDto.ProductCode), Visible = true, Width = 110 },
                new() { Caption = "Depo", FieldName = nameof(ProductDto.WarehouseName), Visible = true, Width = 110 },
                new() { FieldName = nameof(ProductDto.WarehouseId), Visible = false }, // filtre panelinde depo adını göstermek için gizli kolon
                new() { Caption = "Birim", FieldName = nameof(ProductDto.ProductUnitTypeName), Visible = true, Width = 75 },
                new() { Caption = "KDV %", FieldName = nameof(ProductDto.TaxRateRate), Visible = true, Width = 75, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "p0" }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } } },
                new() { Caption = "Stok", FieldName = nameof(ProductDto.StockQuantity), Visible = true, Width = 90, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } } },
                new() { Caption = "Alış Fiyatı", FieldName = "PurchasePriceUnbound", UnboundDataType = typeof(decimal), Visible = true, Width = 110, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } } },
                new() { Caption = "Satış Fiyatı", FieldName = "SalePriceUnbound", UnboundDataType = typeof(decimal), Visible = true, Width = 110, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } } }
            ];

            foreach (GridColumn column in columns)
            {
                column.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }

            gridCatalogView.Columns.AddRange(columns);
            gridCatalogView.CustomUnboundColumnData += GridCatalogView_CustomUnboundColumnData;
            gridCatalogView.OptionsView.ColumnAutoWidth = false;
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
            btnPrintSlip.Click += (_, _) => ShowSlipPreviewAsync();
            btnCancel.Click += (_, _) => Close();
            gridLinesView.CellValueChanged += GridLinesView_CellValueChanged;
            gridLinesView.ValidatingEditor += GridLinesView_ValidatingEditor;
            gridCatalogView.DoubleClick += GridCatalogView_DoubleClick;
            cmbCatalogWarehouse.EditValueChanged += CmbCatalogWarehouse_EditValueChanged;

            // Açıklama alanını otomatik oluşturmak için ilgili alanlardaki değişiklikleri izle.
            cmbInvoiceType.SelectedIndexChanged += (_, _) => UpdateAutoDescription();
            dtDate.EditValueChanged += (_, _) => UpdateAutoDescription();
            txtInvoiceNumber.EditValueChanged += (_, _) => UpdateAutoDescription();
            lookUpAccount.EditValueChanged += (_, _) => UpdateAutoDescription();
            _lines.ListChanged += (_, _) => UpdateAutoDescription();
        }

        /// <summary>
        /// Fatura tarihi, cari, fatura numarası ve kalemlerdeki ürünlere göre
        /// "... tarihinde ... firmasından/firmasına ... fatura numarası ile ... alınmıştır/satılmıştır."
        /// biçiminde bir açıklama metni üretip Açıklama alanına yazar.
        /// Kullanıcı açıklamayı elle değiştirmişse üzerine yazmaz.
        /// </summary>
        private void UpdateAutoDescription()
        {
            // Onaylanmış faturalarda veya salt-okunur durumda otomatik açıklama üretilmez.
            if (_editing is not null && _editing.Status != InvoiceStatus.Draft)
            {
                return;
            }

            // Kullanıcı, otomatik üretilenden farklı bir metin yazdıysa dokunma.
            if (!string.IsNullOrEmpty(txtDescription.Text) && txtDescription.Text != _lastAutoDescription)
            {
                return;
            }

            bool isSales = cmbInvoiceType.SelectedIndex == 0;
            string accountName = GetSelectedAccountName();
            string invoiceNumber = txtInvoiceNumber.Text.Trim();
            string dateText = dtDate.DateTime == DateTime.MinValue
                ? DateTime.Today.ToString("dd.MM.yyyy")
                : dtDate.DateTime.ToString("dd.MM.yyyy");
            string materialText = GetLineMaterialsText();

            if (string.IsNullOrEmpty(accountName) && string.IsNullOrEmpty(invoiceNumber) && string.IsNullOrEmpty(materialText))
            {
                return;
            }

            string accountPart = string.IsNullOrEmpty(accountName)
                ? (isSales ? "müşteri" : "tedarikçi")
                : accountName;
            string numberPart = string.IsNullOrEmpty(invoiceNumber) ? "..." : invoiceNumber;
            string materialPart = string.IsNullOrEmpty(materialText) ? "malzeme" : materialText;

            string description = isSales
                ? $"{dateText} tarihinde {accountPart} firmasına {numberPart} fatura numarası ile {materialPart} satılmıştır."
                : $"{dateText} tarihinde {accountPart} firmasından {numberPart} fatura numarası ile {materialPart} alınmıştır.";

            txtDescription.Text = description;
            _lastAutoDescription = description;
        }

        private string GetSelectedAccountName()
        {
            if (lookUpAccount.EditValue is not Guid accountId || accountId == Guid.Empty)
            {
                return string.Empty;
            }

            bool isSales = cmbInvoiceType.SelectedIndex == 0;
            return isSales
                ? _customers.FirstOrDefault(c => c.Id == accountId)?.Name ?? string.Empty
                : _suppliers.FirstOrDefault(s => s.Id == accountId)?.Name ?? string.Empty;
        }

        private string GetLineMaterialsText()
        {
            List<string> names = _lines
                .Where(l => l.ProductId != Guid.Empty)
                .Select(l => _productsById.GetValueOrDefault(l.ProductId)?.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .Distinct()
                .ToList();

            const int maxNames = 3;
            if (names.Count == 0)
            {
                return string.Empty;
            }
            if (names.Count <= maxNames)
            {
                return string.Join(", ", names);
            }

            int remaining = names.Count - maxNames;
            return string.Join(", ", names.Take(maxNames)) + $" ve {remaining} kalem daha (vb.)";
        }

        private async void InvoiceEditForm_Load(object? sender, EventArgs e)
        {
            await LoadLookUpsAsync();

            if (_editing is not null)
            {
                PopulateExisting(_editing);
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
                _productsById = _products.ToDictionary(p => p.Id);
                _accounts = (await mediator.Send(new ChartOfAccountLookUpQuery())).Data ?? [];
                _warehouses = _accounts
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
                ToastHelper.Show("Veriler yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private void CmbInvoiceType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateAccountDataSource();
        }

        private void UpdateAccountDataSource()
        {
            bool isSales = cmbInvoiceType.SelectedIndex == 0;
            lblAccountLabel.Text = isSales ? "Müşteri:" : "Tedarikçi:";

            lookUpAccount.Properties.DataSource = null;
            lookUpAccountView.Columns.Clear();
            lookUpAccountView.OptionsBehavior.AutoPopulateColumns = false;

            if (isSales)
            {
                lookUpAccount.Properties.DataSource = _customers;
                lookUpAccount.Properties.ValueMember = nameof(CustomerDto.Id);
                lookUpAccount.Properties.DisplayMember = nameof(CustomerDto.Name);

                lookUpAccountView.Columns.AddField(nameof(CustomerDto.Name)).Caption = "Müşteri Adı";
                lookUpAccountView.Columns.AddField(nameof(CustomerDto.TaxNumber)).Caption = "Vergi No";
                lookUpAccountView.Columns.AddField(nameof(CustomerDto.City)).Caption = "Şehir";
            }
            else
            {
                lookUpAccount.Properties.DataSource = _suppliers;
                lookUpAccount.Properties.ValueMember = nameof(SupplierDto.Id);
                lookUpAccount.Properties.DisplayMember = nameof(SupplierDto.Name);

                lookUpAccountView.Columns.AddField(nameof(SupplierDto.Name)).Caption = "Tedarikçi Adı";
                lookUpAccountView.Columns.AddField(nameof(SupplierDto.TaxNumber)).Caption = "Vergi No";
                lookUpAccountView.Columns.AddField(nameof(SupplierDto.City)).Caption = "Şehir";
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
            UpdateAutoDescription();
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
                UpdateAutoDescription();
            }
        }

        private void CmbCatalogWarehouse_EditValueChanged(object? sender, EventArgs e)
        {
            if (gridCatalogView.GridControl == null)
            {
                return;
            }

            GridColumn warehouseIdColumn = gridCatalogView.Columns[nameof(ProductDto.WarehouseId)];

            if (cmbCatalogWarehouse.EditValue is Guid warehouseId)
            {
                // Filtre panelinde ham GUID yerine seçilen deponun adı gösterilsin.
                string warehouseName = _warehouses.FirstOrDefault(w => w.Id == warehouseId)?.Display ?? string.Empty;
                warehouseIdColumn.FilterInfo = new ColumnFilterInfo(warehouseIdColumn, warehouseId, $"Depo = {warehouseName}");
            }
            else
            {
                warehouseIdColumn.FilterInfo = new ColumnFilterInfo();
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
                ToastHelper.Show("Lütfen listeden bir ürün seçin.", ToastType.Warning);
                return;
            }

            int existingRow = _lines.ToList().FindIndex(l => l.ProductId == product.Id);
            if (existingRow >= 0)
            {
                ToastHelper.Show($"'{product.Name}' faturaya zaten eklenmiş.", ToastType.Warning);
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
            UpdateAutoDescription();
        }

        private void RiProductLookUp_EditValueChanged(object? sender, EventArgs e)
        {
            if (sender is SearchLookUpEdit edit && edit.EditValue is Guid productId)
            {
                ProductDto? prod = _productsById.GetValueOrDefault(productId);
                if (prod is not null)
                {
                    int rowHandle = gridLinesView.FocusedRowHandle;
                    if (rowHandle >= 0 && rowHandle < _lines.Count)
                    {
                        InvoiceLineDto line = _lines[rowHandle];

                        InvoiceLineDto? duplicate = _lines.FirstOrDefault(l => l != line && l.ProductId == productId);
                        if (duplicate is not null)
                        {
                            ToastHelper.Show($"'{prod.Name}' faturaya zaten eklenmiş.", ToastType.Warning);
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
                        UpdateAutoDescription();
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
                e.ErrorText = "Birim fiyat sıfırdan büyük olmalıdır.";
                return;
            }

            if (columnName == nameof(InvoiceLineDto.Quantity) && e.Value is decimal qty && qty <= 0)
            {
                e.Valid = false;
                e.ErrorText = "Miktar sıfırdan büyük olmalıdır.";
                return;
            }

            if (columnName == nameof(InvoiceLineDto.DiscountRate) && e.Value is decimal discount && (discount < 0 || discount > 100))
            {
                e.Valid = false;
                e.ErrorText = "İskonto oranı %0 ile %100 arasında olmalıdır.";
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

            ToastHelper.Show($"'{_productsById.GetValueOrDefault(newProductId)?.Name ?? "Ürün"}' faturaya zaten eklenmiş.", ToastType.Warning);
            e.Valid = false;
            e.ErrorText = "Bu ürün faturada zaten mevcut.";
        }

        private void GridLinesView_CellValueChanged(object? sender, CellValueChangedEventArgs e)
        {
            RecalculateTotals();
            UpdateAutoDescription();
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
                decimal discountRate = line.DiscountRate / 100m;
                decimal discountAmount = Math.Round(lineSub * discountRate, 2);
                decimal netAmount = lineSub - discountAmount;
                decimal rate = line.TaxRateRate / 100m;
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

            decimal netTotal = subTotal - discountTotal;
            lblSubTotalValue.Text = netTotal.ToString("n2") + " ₺";
            lblDiscountTotalValue.Text = "- " + discountTotal.ToString("n2") + " ₺";
            lblTaxTotalValue.Text = taxTotal.ToString("n2") + " ₺";
            lblGrandTotalValue.Text = (netTotal + taxTotal).ToString("n2") + " ₺";

            var breakdown = byRate
                .OrderByDescending(kv => kv.Key)
                .Select(kv => $"KDV %{kv.Key:n0}  │  Matrah: {kv.Value.Matrah:n2} ₺  │  KDV: {kv.Value.Kdv:n2} ₺");

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
                ToastHelper.Show("Fatura numarası boş olamaz.", ToastType.Warning);
                txtInvoiceNumber.Focus();
                return;
            }

            if (lookUpAccount.EditValue is not Guid accountId || accountId == Guid.Empty)
            {
                ToastHelper.Show("Lütfen bir cari (Müşteri/Tedarikçi) seçiniz.", ToastType.Warning);
                lookUpAccount.Focus();
                return;
            }

            var validLines = _lines
                .Where(l => l.ProductId != Guid.Empty && l.Quantity > 0 && l.UnitPrice > 0)
                .ToList();
            if (validLines.Count == 0 || validLines.Count != _lines.Count(l => l.ProductId != Guid.Empty))
            {
                ToastHelper.Show("Miktar ve fiyat sıfırdan büyük olmalıdır. Eksik kalemleri tamamlayın.", ToastType.Warning);
                return;
            }

            foreach (var line in validLines)
            {
                if (line.DiscountRate < 0 || line.DiscountRate > 100)
                {
                    ToastHelper.Show("İskonto oranı %0 ile %100 arasında olmalıdır.", ToastType.Warning);
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
                    string message = _editing is { Status: InvoiceStatus.Draft }
                        ? "Fatura taslağı güncellendi."
                        : approve
                            ? "Fatura onaylandı; stok ve cari hareketleri oluşturuldu."
                            : "Fatura taslak olarak kaydedildi.";
                    ToastHelper.Show(message, ToastType.Success);

                    if (approve && invoiceType == InvoiceType.Purchase)
                    {
                        _saved = true;
                        LockAfterApproval();
                        return;
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            finally
            {
                if (!_saved)
                {
                    btnSave.Enabled = true;
                    btnSaveDraft.Enabled = true;
                }
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
                    ToastHelper.Show("Fatura onaylandı; stok ve cari hareketleri oluşturuldu.", ToastType.Success);

                    if (_editing.InvoiceType == InvoiceType.Purchase)
                    {
                        _saved = true;
                        LockAfterApproval();
                        return;
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            finally
            {
                btnApprove.Enabled = true;
            }
        }

        private void LockAfterApproval()
        {
            btnPrintSlip.Enabled = true;
            btnSave.Enabled = false;
            btnSaveDraft.Enabled = false;
            btnApprove.Enabled = false;
            btnApprove.Visible = false;
            btnAddLine.Enabled = false;
            btnDeleteLine.Enabled = false;
            btnAddProduct.Enabled = false;
            cmbInvoiceType.ReadOnly = true;
            txtInvoiceNumber.ReadOnly = true;
            dtDate.ReadOnly = true;
            lookUpAccount.ReadOnly = true;
            txtDescription.ReadOnly = true;
            gridLinesView.OptionsBehavior.Editable = false;
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

        private async void ShowSlipPreviewAsync()
        {
            await MovableAssetTransactionSlipPresenter.ShowAsync(await BuildSlipDataAsync());
        }

        private async Task<MovableAssetTransactionSlipData> BuildSlipDataAsync()
        {
            CompanyDto company = await LoadCompanyAsync();

            string supplierName = GetSelectedAccountName();

            string city = string.IsNullOrWhiteSpace(company.City) ? string.Empty : company.City.Trim();
            string district = string.IsNullOrWhiteSpace(company.District) ? string.Empty : company.District.Trim();
            string ilIlce = city.Length > 0 && district.Length > 0 ? $"{city} / {district}" : city + district;

            MovableAssetTransactionSlipData data = new()
            {
                DocumentNumber = txtInvoiceNumber.Text.Trim(),
                Date = dtDate.DateTime,
                IslemCesidi = "Giriş",
                NeredenGeldigi = supplierName,
                KimeVerildigi = string.Empty,
                NereyeVerildigi = string.Empty,
                IlIlceAdi = ilIlce,
                IlIlceKodu = string.Empty,
                HarcamaBirimiAdi = company.ExpenditureUnitName ?? string.Empty,
                HarcamaBirimiKodu = company.ExpenditureUnitCode ?? string.Empty,
                AmbarAdi = string.Empty,
                AmbarKodu = string.Empty,
                MuhasebeBirimiAdi = company.AccountingUnitName ?? string.Empty,
                MuhasebeBirimiKodu = company.AccountingUnitCode ?? string.Empty,
                DayanakTarihi = dtDate.DateTime,
                DayanakKodu = txtInvoiceNumber.Text.Trim(),
                AccountNames = MovableAssetTransactionSlipPresenter.BuildAccountNameMap(_accounts)
            };

            int order = 0;
            foreach (InvoiceLineDto line in _lines.Where(l => l.ProductId != Guid.Empty && l.Quantity > 0))
            {
                ProductDto? product = _productsById.GetValueOrDefault(line.ProductId);
                order++;
                data.Rows.Add(new MovableAssetTransactionSlipRow
                {
                    SiraNo = order,
                    Kodu = MovableAssetTransactionSlipPresenter.ResolveItemCode(product, line.ProductCode),
                    DepoKodu = product?.WarehouseCode ?? string.Empty,
                    DepoAdi = product?.WarehouseName ?? string.Empty,
                    BarkodNo = product?.Barcode ?? string.Empty,
                    Adi = product?.Name ?? string.Empty,
                    OlcuBirimi = product?.ProductUnitTypeName ?? string.Empty,
                    Miktari = line.Quantity,
                    BirimFiyati = line.UnitPrice,
                    Tutari = line.Quantity * line.UnitPrice
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
                CrashLog.WriteException("InvoiceSlip.Company", ex);
            }

            return new CompanyDto();
        }
    }
}