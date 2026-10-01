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
        private List<ProductCatalogDto> _products = [];
        private readonly List<ProductCatalogDto> _catalogProducts = [];
        private Dictionary<Guid, ProductCatalogDto> _productsById = [];
        private readonly Dictionary<Guid, List<ProductMovementDto>> _movementsByProductId = [];
        private List<ChartOfAccountLookUpDto> _accounts = [];
        private List<ChartOfAccountLookUpDto> _warehouses = [];
        private RepositoryItemSearchLookUpEdit _riProductLookUp = default!;
        private string? _lastAutoDescription;
        private bool _saved;

        private InvoiceType SelectedInvoiceType => cmbInvoiceType.SelectedIndex switch
        {
            1 => InvoiceType.PurchaseReturn,
            2 => InvoiceType.Sales,
            3 => InvoiceType.SalesReturn,
            _ => InvoiceType.Purchase
        };

        private static int IndexOf(InvoiceType type) => type switch
        {
            InvoiceType.Purchase => 0,
            InvoiceType.PurchaseReturn => 1,
            InvoiceType.Sales => 2,
            InvoiceType.SalesReturn => 3,
            _ => 0
        };

        private ProductPriceType PriceTypeFor(InvoiceType type)
        => type.IsPurchaseSide() ? ProductPriceType.Purchase : ProductPriceType.Sale;

        private bool IsNewSalesInvoice => _editing is null
            && SelectedInvoiceType == InvoiceType.Sales;

        private async Task<decimal?> AskSalesPriceAsync(ProductCatalogDto product, decimal currentUnitPrice)
        {
            if (!IsNewSalesInvoice || currentUnitPrice > 0)
            {
                return null;
            }

            List<ProductMovementDto> movements = await LoadMovementsAsync(product.Id);
            decimal? costPrice = SalesPriceSuggestionCalculator.CostPriceOf(product.Prices, movements);
            if (costPrice is null or <= 0)
            {
                return null;
            }

            using var form = new SalesPriceSuggestionForm(
                product.Name,
                product.ProductCode,
                costPrice.Value,
                product.TaxRateRate);

            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return null;
            }

            if (form.SaveAsSalePrice)
            {
                await SaveSalePriceAsync(product, form.Price);
                ApplySavedSalePriceToCache(product, form.Price);
            }

            return form.Price;
        }

        private static async Task SaveSalePriceAsync(ProductCatalogDto product, decimal price)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                await mediator.Send(new ProductPriceCreateCommand(
                    product.Id,
                    ProductPriceType.Sale,
                    price,
                    DateOnly.FromDateTime(DateTime.Today)), CancellationToken.None);
            }
catch (Exception ex)
            {
                CrashLog.WriteException("Invoice.LoadLookUps", ex);
                ToastHelper.Show("Veriler yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private void ApplySavedSalePriceToCache(ProductCatalogDto product, decimal price)
        {
            product.Prices.Insert(0, new ProductPriceDto
            {
                Id = Guid.Empty,
                PriceType = ProductPriceType.Sale,
                UnitPrice = price,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = null
            });

            gridCatalogView.RefreshData();
        }

        private bool IsOutputInvoice => SelectedInvoiceType == InvoiceType.Sales
            || SelectedInvoiceType == InvoiceType.PurchaseReturn;

        private decimal AvailableQuantity(Guid productId, DateOnly asOfDate)
        {
            if (!_movementsByProductId.TryGetValue(productId, out List<ProductMovementDto>? movements))
            {
                return 0;
            }

            decimal balance = 0;

            foreach (ProductMovementDto movement in movements.Where(m => m.Date <= asOfDate))
            {
                balance += movement.MovementType == ProductMovementType.Input
                    ? movement.Quantity
                    : -movement.Quantity;
            }

            return balance;
        }

        private async Task<List<ProductMovementDto>> LoadMovementsAsync(Guid productId)
        {
            if (_movementsByProductId.TryGetValue(productId, out List<ProductMovementDto>? cached))
            {
                return cached;
            }

            List<ProductMovementDto> movements = [];
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                ProductDto? full = (await mediator.Send(new ProductGetQuery(productId), CancellationToken.None)).Data;
                movements = full?.Movements ?? [];
            }
catch (Exception ex)
             {
                 CrashLog.WriteException("Invoice.AskAndReload", ex);
                 ToastHelper.Show("Veriler yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
             }

            _movementsByProductId[productId] = movements;
            return movements;
        }

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
            cmbInvoiceType.Properties.Items.AddRange([ "Alış Faturası", "Alışlardan İade Faturası", "Satış Faturası", "Satışlardan İade Faturası" ]);
            cmbInvoiceType.SelectedIndex = 0;

            dtDate.DateTime = DateTime.Today;
            txtInvoiceNumber.Text = "";

            gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
            gridLines.DataSource = _lines;
            ConfigureGrid();
            ConfigureCatalogGrid();

            // Kaydetme her zaman TASLAK olarak yapar; onay giriş ekranından
            // YAPILMAZ, liste ekranından yapılır. Böylece fatura girişi ile
            // onayı ayrı adımlardır.
            btnPrintSlip.Visible = SelectedInvoiceType == InvoiceType.Purchase;
            lblStatusValue.Text = "";

            if (_editing is not null)
            {
                bool isDraft = _editing.Status == InvoiceStatus.Draft;

                btnSaveDraft.Visible = isDraft;
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
                ValueMember = nameof(ProductCatalogDto.Id),
                DisplayMember = nameof(ProductCatalogDto.Name),
                NullText = "Ürün Seçiniz...",
                PopupFilterMode = PopupFilterMode.Contains
            };
            // Popup"ta yalnızca ürün görünsün: gereksiz alt-detail kolonları (Ürün Kodu, Stok) listelenmesin.
            _riProductLookUp.View.OptionsBehavior.AutoPopulateColumns = false;
            _riProductLookUp.View.Columns.AddField(nameof(ProductCatalogDto.Name)).Caption = "Ürün Adı";
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
                new() { Caption = "Ürün Adı", FieldName = nameof(ProductCatalogDto.Name), Visible = true, Width = 220 },
                new() { Caption = "Ürün Kodu", FieldName = nameof(ProductCatalogDto.ProductCode), Visible = true, Width = 110 },
                new() { Caption = "Depo", FieldName = nameof(ProductCatalogDto.WarehouseName), Visible = true, Width = 110 },
                new() { FieldName = nameof(ProductCatalogDto.WarehouseId), Visible = false }, // filtre panelinde depo adını göstermek için gizli kolon
                new() { Caption = "Birim", FieldName = nameof(ProductCatalogDto.ProductUnitTypeName), Visible = true, Width = 75 },
                new() { Caption = "KDV %", FieldName = nameof(ProductCatalogDto.TaxRateRate), Visible = true, Width = 75, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "p0" }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } } },
                new() { Caption = "Stok", FieldName = nameof(ProductCatalogDto.StockQuantity), Visible = true, Width = 90, DisplayFormat = { FormatType = FormatType.Custom, FormatString = "n2" }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } } },
                new() { Caption = "Alış Fiyatı", FieldName = "PurchasePriceUnbound", UnboundDataType = typeof(decimal), Visible = true, Width = 110, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } } },
                new() { Caption = "Satış Fiyatı", FieldName = "SalePriceUnbound", UnboundDataType = typeof(decimal), Visible = true, Width = 110, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } } }
            ];

            foreach (GridColumn column in columns)
            {
                column.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }

            gridCatalogView.Columns.AddRange(columns);
            // Elle kurulan kolonlar da 0,00 kuralına tabi olsun.
            GridColumnFactory.RegisterManualNumericColumns(gridCatalogView);
            gridCatalogView.CustomUnboundColumnData += GridCatalogView_CustomUnboundColumnData;
            gridCatalogView.OptionsView.ColumnAutoWidth = false;
        }

        private void GridCatalogView_CustomUnboundColumnData(object? sender, CustomColumnDataEventArgs e)
        {
            if (!e.IsGetData || e.ListSourceRowIndex < 0)
            {
                return;
            }

            if (gridCatalogView.GetRow(e.ListSourceRowIndex) is not ProductCatalogDto product)
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
            cmbInvoiceType.SelectedIndexChanged += (_, _) => RefreshForInvoiceTypeChange();
            btnAddLine.Click += (_, _) => AddEmptyLine();
            btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
            btnAddProduct.Click += (_, _) => AddSelectedCatalogProduct();
            btnSaveDraft.Click += BtnSaveDraft_Click;
            btnPrintSlip.Click += (_, _) => ShowSlipPreviewAsync();
            btnCancel.Click += (_, _) => Close();
            gridLinesView.CellValueChanged += GridLinesView_CellValueChanged;
            gridLinesView.ValidatingEditor += GridLinesView_ValidatingEditor;
            gridCatalogView.DoubleClick += GridCatalogView_DoubleClick;
            cmbCatalogWarehouse.CloseUp += CmbCatalogWarehouse_CloseUp;
            txtCatalogProductSearch.EditValueChanged += TxtCatalogProductSearch_EditValueChanged;

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

            InvoiceType type = SelectedInvoiceType;
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
                ? (type.IsSalesSide() ? "müşteri" : "tedarikçi")
                : accountName;
            string numberPart = string.IsNullOrEmpty(invoiceNumber) ? "..." : invoiceNumber;
            string materialPart = string.IsNullOrEmpty(materialText) ? "malzeme" : materialText;

            string description = type switch
            {
                InvoiceType.PurchaseReturn =>
                    $"{dateText} tarihinde {accountPart} firmasından {numberPart} fatura numarası ile {materialPart} iade edilmiştir.",
                InvoiceType.SalesReturn =>
                    $"{dateText} tarihinde {accountPart} firmasına {numberPart} fatura numarası ile {materialPart} iadesi alınmıştır.",
                _ when type.IsSalesSide() =>
                    $"{dateText} tarihinde {accountPart} firmasına {numberPart} fatura numarası ile {materialPart} satılmıştır.",
                _ =>
                    $"{dateText} tarihinde {accountPart} firmasından {numberPart} fatura numarası ile {materialPart} alınmıştır."
            };

            txtDescription.Text = description;
            _lastAutoDescription = description;
        }

        private string GetSelectedAccountName()
        {
            if (lookUpAccount.EditValue is not Guid accountId || accountId == Guid.Empty)
            {
                return string.Empty;
            }

            return SelectedInvoiceType.IsSalesSide()
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
            try
            {
                await LoadLookUpsAsync();
                if (_editing is not null)
                {
                    PopulateExisting(_editing);
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Invoice.Load", ex);
                ToastHelper.Show("Hata: " + ex.Message, ToastType.Error);
            }
        }

        private async Task LoadLookUpsAsync()
        {
            try
            {
                // Dört sorguyu ayrı scope'larla paralel çalıştırıyoruz: DbContext tek scope'ta
                // thread-safe değildir, bu yüzden her sorgu kendi scope'unu kullanır. Böylece
                // katalog (en yavaş veri) hazır olana kadar firma ve depo listeleri bağlanır.
                Task<List<CustomerDto>> customersTask = LoadCustomersAsync();
                Task<List<SupplierDto>> suppliersTask = LoadSuppliersAsync();
                Task<List<ChartOfAccountLookUpDto>> accountsTask = LoadAccountsAsync();
                Task<List<ProductCatalogDto>> productsTask = LoadProductsAsync();

                _customers = await customersTask;
                _suppliers = await suppliersTask;
                _accounts = (await accountsTask) ?? [];
                _warehouses = _accounts
                    .Where(w => w.Type == ChartOfAccountType.Warehouse)
                    .ToList();

                // Firma ve depo listeleri katalogu beklemeden hazır; önce bunları bağlıyoruz.
                UpdateAccountDataSource();
                BindWarehouseCombo();

                if (_editing is { Lines.Count: > 0 })
                {
                    foreach (Guid productId in _editing.Lines.Select(l => l.ProductId).Distinct())
                    {
                        if (productId != Guid.Empty)
                        {
                            await LoadMovementsAsync(productId);
                        }
                    }
                }

                _products = await productsTask;
                _productsById = _products.ToDictionary(p => p.Id);

                _riProductLookUp.DataSource = _products;

                _catalogProducts.Clear();
                _catalogProducts.AddRange(_products);
                gridCatalog.DataSource = _catalogProducts;
                gridCatalogView.BestFitColumns();
                UpdateCatalogFeedback();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Veriler yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private async Task<List<CustomerDto>> LoadCustomersAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            return (await mediator.Send(new CustomerGetAllQuery())).ToList();
        }

        private async Task<List<SupplierDto>> LoadSuppliersAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            return (await mediator.Send(new SupplierGetAllQuery())).ToList();
        }

        private async Task<List<ChartOfAccountLookUpDto>> LoadAccountsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            return (await mediator.Send(new ChartOfAccountLookUpQuery())).Data ?? [];
        }

        private async Task<List<ProductCatalogDto>> LoadProductsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            return (await mediator.Send(new ProductCatalogGetAllQuery())).ToList();
        }

        private void BindWarehouseCombo()
        {
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
        }

        private void CmbInvoiceType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateAccountDataSource();

            if (_editing is null
                && SelectedInvoiceType is InvoiceType.Sales or InvoiceType.SalesReturn
                && string.IsNullOrWhiteSpace(txtInvoiceNumber.Text))
            {
                _ = TrySetNextSalesNumberAsync(SelectedInvoiceType);
            }
        }

        private void RefreshForInvoiceTypeChange()
        {
            if (_editing is not null || !HasUnsavedEntry())
            {
                return;
            }

            _ = AskAndReloadLookUpsAsync(SelectedInvoiceType);
        }

        private bool HasUnsavedEntry()
        {
            if (_lines.Any(l => l.ProductId != Guid.Empty))
            {
                return true;
            }

            if (lookUpAccount.EditValue is Guid accountId && accountId != Guid.Empty)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(txtInvoiceNumber.Text);
        }

        private async Task AskAndReloadLookUpsAsync(InvoiceType newType)
        {
            DialogResult result = MsgBox.Confirm(
                this,
                "Fatura tipi seçildiği için sayfa yenilenecektir. Girilen veriler temizlenecektir. Devam edilsin mi?",
                "Fatura Tipi Değişikliği");

            if (result != DialogResult.Yes)
            {
                return;
            }

            txtDescription.Text = string.Empty;
            _lastAutoDescription = null;
            _lines.Clear();
            lookUpAccount.EditValue = null;
            RecalculateTotals();

            try
            {
                Task<List<CustomerDto>> customersTask = LoadCustomersAsync();
                Task<List<SupplierDto>> suppliersTask = LoadSuppliersAsync();
                Task<List<ChartOfAccountLookUpDto>> accountsTask = LoadAccountsAsync();
                Task<List<ProductCatalogDto>> productsTask = LoadProductsAsync();

                _customers = await customersTask;
                _suppliers = await suppliersTask;
                _accounts = (await accountsTask) ?? [];
                _warehouses = _accounts
                    .Where(w => w.Type == ChartOfAccountType.Warehouse)
                    .ToList();

                UpdateAccountDataSource();
                BindWarehouseCombo();

                _products = await productsTask;
                _productsById = _products.ToDictionary(p => p.Id);
                _riProductLookUp.DataSource = _products;

                _catalogProducts.Clear();
                _catalogProducts.AddRange(_products);
                gridCatalog.DataSource = _catalogProducts;
                gridCatalogView.BestFitColumns();
                UpdateCatalogFeedback();

                if (newType is InvoiceType.Sales or InvoiceType.SalesReturn
                    && string.IsNullOrWhiteSpace(txtInvoiceNumber.Text))
                {
                    await TrySetNextSalesNumberAsync(newType);
                }

                UpdateAutoDescription();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Veriler yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private async Task TrySetNextSalesNumberAsync(InvoiceType type)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await mediator.Send(new InvoiceGetNextNumberQuery(type), CancellationToken.None);

                if (result.IsSuccessful && !string.IsNullOrWhiteSpace(result.Data))
                {
                    txtInvoiceNumber.Text = result.Data;
                }
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Fatura numarası alınamadı: " + ex.Message, ToastType.Warning);
            }
        }

        private void UpdateAccountDataSource()
        {
            bool isSales = SelectedInvoiceType.IsSalesSide();
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
            cmbInvoiceType.SelectedIndex = IndexOf(invoice.InvoiceType);
            txtInvoiceNumber.Text = invoice.InvoiceNumber;
            dtDate.DateTime = invoice.Date.ToDateTime(TimeOnly.MinValue);
            lookUpAccount.EditValue = invoice.InvoiceType.IsSalesSide() ? invoice.CustomerId : invoice.SupplierId;
            txtDescription.Text = invoice.Description;

            if (invoice.Status != InvoiceStatus.Draft && invoice.InvoiceType == InvoiceType.Purchase)
            {
                btnPrintSlip.Visible = true;
            }

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

        private void CmbCatalogWarehouse_CloseUp(object? sender, EventArgs e)
        {
            // Katalog grid'i popup kapanış işleminin içindeyken filtre uygulamak DevExpress'in
            // kendi update akışına denk gelir ve ilk seferde listeyi boş bırakır (EditValueChanged
            // ile de aynısı yaşanmıştı). Arama kutusu gibi popup dışı aksiyonlarda aynı kod doğru
            // çalıştığı için kapanış tamamlanana dek filtreyi erteliyoruz.
            BeginInvoke((Action)ApplyCatalogFilter);
        }

        private void TxtCatalogProductSearch_EditValueChanged(object? sender, EventArgs e)
        {
            ApplyCatalogFilter();
        }

        private void ApplyCatalogFilter()
        {
            IEnumerable<ProductCatalogDto> filtered = _products;

            if (cmbCatalogWarehouse.EditValue is Guid warehouseId)
            {
                filtered = filtered.Where(p => p.WarehouseId == warehouseId);
            }

            // EditValue'dan okunur - boş kutu iken Text, NullText watermark'ını ("Ürün ara...")
            // döndürür ve bu gerçek bir arama terimi gibi filtrelenerek listeyi boşaltırdı.
            string term = (txtCatalogProductSearch.EditValue as string)?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(term))
            {
                filtered = filtered.Where(p =>
                    (p.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (p.ProductCode?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (p.Barcode?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            List<ProductCatalogDto> result = filtered.ToList();
            _catalogProducts.Clear();
            _catalogProducts.AddRange(result);

            gridCatalog.DataSource = null;
            gridCatalog.DataSource = result;
            UpdateCatalogFeedback();
        }

        private void UpdateCatalogFeedback()
        {
            int count = _catalogProducts.Count;

            if (count == 0)
            {
                lblCatalogTitle.Text = "Ürün bulunamadı";
                lblCatalogTitle.Appearance.ForeColor = Color.FromArgb(200, 60, 60);
                return;
            }

            lblCatalogTitle.Text = $"Tanımlı Ürünler ({count})";
            lblCatalogTitle.Appearance.ForeColor = Color.FromArgb(64, 64, 64);
        }

        private void GridCatalogView_DoubleClick(object? sender, EventArgs e)
        {
            AddSelectedCatalogProduct();
        }

        private async void AddSelectedCatalogProduct()
        {
            if (gridCatalogView.GetFocusedRow() is not ProductCatalogDto product)
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

            await LoadMovementsAsync(product.Id);

            ProductPriceDto? price = product.Prices
                .Where(p => p.PriceType == PriceTypeFor(SelectedInvoiceType))
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            decimal unitPrice = price?.UnitPrice ?? 0;
            if (unitPrice <= 0)
            {
                unitPrice = await AskSalesPriceAsync(product, unitPrice) ?? 0m;
            }

            _lines.Add(new InvoiceLineDto
            {
                ProductId = product.Id,
                Quantity = 1,
                UnitPrice = unitPrice,
                TaxRateRate = product.TaxRateRate > 0 && product.TaxRateRate <= 1 ? product.TaxRateRate * 100 : product.TaxRateRate,
                Description = product.ProductCode
            });

            gridLinesView.FocusedRowHandle = _lines.Count - 1;
            RecalculateTotals();
            UpdateAutoDescription();
        }

private async void RiProductLookUp_EditValueChanged(object? sender, EventArgs e)
        {
            if (sender is SearchLookUpEdit edit && edit.EditValue is Guid productId)
            {
                ProductCatalogDto? prod = _productsById.GetValueOrDefault(productId);
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

                        await LoadMovementsAsync(prod.Id);

                        var priceObj = prod.Prices
                            .Where(p => p.PriceType == PriceTypeFor(SelectedInvoiceType))
                            .OrderByDescending(p => p.StartDate)
                            .FirstOrDefault();

                        decimal inGridUnitPrice = priceObj?.UnitPrice ?? 0;
                        if (inGridUnitPrice <= 0)
                        {
                            inGridUnitPrice = await AskSalesPriceAsync(prod, inGridUnitPrice) ?? 0m;
                        }

                        line.UnitPrice = inGridUnitPrice;

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

            if (columnName == nameof(InvoiceLineDto.Quantity)
                && IsOutputInvoice
                && e.Value is decimal quantity)
            {
                int dataRowForQty = gridLinesView.GetDataSourceRowIndex(gridLinesView.FocusedRowHandle);
                if (dataRowForQty >= 0
                    && gridLinesView.GetRow(gridLinesView.FocusedRowHandle) is InvoiceLineDto currentForQty
                    && currentForQty.ProductId != Guid.Empty
                    && _productsById.TryGetValue(currentForQty.ProductId, out ProductCatalogDto? productForQty)
                    && _movementsByProductId.ContainsKey(currentForQty.ProductId))
                {
                    DateOnly asOf = DateOnly.FromDateTime(dtDate.DateTime);
                    decimal available = AvailableQuantity(currentForQty.ProductId, asOf);

                    if (quantity > available)
                    {
                        e.Valid = false;
                        e.ErrorText = $"'{productForQty.Name}' için yeterli stok yok. Mevcut: {available:n2}, istenen: {quantity:n2}.";
                        return;
                    }
                }
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

        private async void BtnSaveDraft_Click(object? sender, EventArgs e)
        {
            await SaveAsync();
        }

        private async Task SaveAsync()
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

            InvoiceType invoiceType = SelectedInvoiceType;
            DateOnly date = DateOnly.FromDateTime(dtDate.DateTime);


            List<InvoiceCreateLineModel> lineModels = validLines.Select(l => new InvoiceCreateLineModel(
                l.ProductId,
                l.Quantity,
                l.UnitPrice,
                l.TaxRateRate,
                l.Description,
                l.DiscountRate)).ToList();
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
                        CustomerId: invoiceType.IsSalesSide() ? accountId : null,
                        SupplierId: invoiceType.IsPurchaseSide() ? accountId : null,
                        Description: txtDescription.Text.Trim(),
                        Lines: lineModels));
                }
                else
                {
                    InvoiceCreateCommand command = new(
                        InvoiceNumber: number,
                        InvoiceType: invoiceType,
                        Date: date,
                        CustomerId: invoiceType.IsSalesSide() ? accountId : null,
                        SupplierId: invoiceType.IsPurchaseSide() ? accountId : null,
                        Description: txtDescription.Text.Trim(),
                        Lines: lineModels,
                        IsApproved: false);

                    ok = await CrudExecutor.ExecuteAsync(command);
                }

                if (ok)
                {
                    string message = _editing is { Status: InvoiceStatus.Draft }
                        ? "Fatura taslağı güncellendi."
                        : "Fatura taslak olarak kaydedildi. Onaylamak için listeden onaylayın.";
                    ToastHelper.Show(message, ToastType.Success);

                    _saved = true;
                    LockAfterSave();
                    return;
                }
            }
            finally
            {
                if (!_saved)
                {
                    btnSaveDraft.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Fatura kaydedildikten sonra formu düzenlenemez hale getirir.
        /// Onay artık giriş ekranından YAPILMAZ; liste ekranından yapılır.
        /// </summary>
        private void LockAfterSave()
        {
            btnPrintSlip.Enabled = true;
            btnSaveDraft.Enabled = false;
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
                ProductCatalogDto? product = _productsById.GetValueOrDefault(line.ProductId);
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
