using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Application.Invoices;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Suppliers;
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
        private readonly BindingList<InvoiceLineVm> _lines = [];
        private List<CustomerDto> _customers = [];
        private List<SupplierDto> _suppliers = [];
        private List<ProductDto> _products = [];
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

            Text = _editing is null ? "Yeni Fatura" : "Fatura İncele";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Yeni fatura düzenlemek için bilgileri doldurun"
                : "Fatura ve kalem detayları";

            InitControls();
            WireEvents();
        }

        private void InitControls()
        {
            cmbInvoiceType.Properties.Items.Clear();
            cmbInvoiceType.Properties.Items.Add("Satış Faturası");
            cmbInvoiceType.Properties.Items.Add("Satın Alma Faturası");
            cmbInvoiceType.SelectedIndex = 0;

            dtDate.DateTime = DateTime.Today;
            txtInvoiceNumber.Text = $"FAT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            gridLines.DataSource = _lines;
            ConfigureGrid();

            if (_editing is not null)
            {
                btnSave.Visible = false;
                btnAddLine.Enabled = false;
                btnDeleteLine.Enabled = false;
                cmbInvoiceType.ReadOnly = true;
                txtInvoiceNumber.ReadOnly = true;
                dtDate.ReadOnly = true;
                lookUpAccount.ReadOnly = true;
                txtDescription.ReadOnly = true;
                gridLinesView.OptionsBehavior.Editable = false;
            }
        }

        private void ConfigureGrid()
        {
            gridLinesView.Columns.Clear();
            gridLinesView.RowHeight = 28;

            _riProductLookUp = new RepositoryItemSearchLookUpEdit
            {
                ValueMember = nameof(ProductDto.Id),
                DisplayMember = nameof(ProductDto.Name),
                NullText = "Ürün Seçiniz...",
                PopupFilterMode = PopupFilterMode.Contains
            };
            _riProductLookUp.View.Columns.AddField(nameof(ProductDto.ProductCode)).Caption = "Ürün Kodu";
            _riProductLookUp.View.Columns.AddField(nameof(ProductDto.Name)).Caption = "Ürün Adı";
            _riProductLookUp.View.Columns.AddField(nameof(ProductDto.StockQuantity)).Caption = "Stok";
            _riProductLookUp.View.Columns[0].Visible = true;
            _riProductLookUp.View.Columns[1].Visible = true;
            _riProductLookUp.View.Columns[2].Visible = true;
            _riProductLookUp.EditValueChanged += RiProductLookUp_EditValueChanged;

            RepositoryItemSpinEdit riQuantity = new() { MinValue = 0.0001m, MaxValue = 999999999, Increment = 1 };
            RepositoryItemSpinEdit riPrice = new() { MinValue = 0, MaxValue = 999999999, Increment = 10, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemSpinEdit riTaxRate = new() { MinValue = 0, MaxValue = 100, Increment = 1, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n0" } };
            RepositoryItemSpinEdit riReadOnlyMoney = new() { ReadOnly = true, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemTextEdit riDesc = new();

            gridLines.RepositoryItems.AddRange([_riProductLookUp, riQuantity, riPrice, riTaxRate, riReadOnlyMoney, riDesc]);

            GridColumn colProduct = new() { Caption = "Ürün", FieldName = nameof(InvoiceLineVm.ProductId), Visible = true, Width = 230, ColumnEdit = _riProductLookUp };
            GridColumn colQty = new() { Caption = "Miktar", FieldName = nameof(InvoiceLineVm.Quantity), Visible = true, Width = 80, ColumnEdit = riQuantity };
            GridColumn colPrice = new() { Caption = "Birim Fiyat", FieldName = nameof(InvoiceLineVm.UnitPrice), Visible = true, Width = 100, ColumnEdit = riPrice };
            GridColumn colTaxRate = new() { Caption = "KDV %", FieldName = nameof(InvoiceLineVm.TaxRateRate), Visible = true, Width = 70, ColumnEdit = riTaxRate };
            GridColumn colTaxAmt = new() { Caption = "KDV Tutarı", FieldName = nameof(InvoiceLineVm.TaxAmount), Visible = true, Width = 95, ColumnEdit = riReadOnlyMoney };
            GridColumn colTotal = new() { Caption = "Toplam Tutar", FieldName = nameof(InvoiceLineVm.TotalAmount), Visible = true, Width = 110, ColumnEdit = riReadOnlyMoney };
            GridColumn colDesc = new() { Caption = "Satır Açıklaması", FieldName = nameof(InvoiceLineVm.Description), Visible = true, Width = 150, ColumnEdit = riDesc };

            gridLinesView.Columns.AddRange([colProduct, colQty, colPrice, colTaxRate, colTaxAmt, colTotal, colDesc]);
        }

        private void WireEvents()
        {
            Load += InvoiceEditForm_Load;
            cmbInvoiceType.SelectedIndexChanged += CmbInvoiceType_SelectedIndexChanged;
            btnAddLine.Click += (_, _) => AddEmptyLine();
            btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            gridLinesView.CellValueChanged += GridLinesView_CellValueChanged;
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

                _riProductLookUp.DataSource = _products;
                UpdateAccountDataSource();
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

            _lines.Clear();
            foreach (var l in invoice.Lines)
            {
                _lines.Add(new InvoiceLineVm
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    TaxRateRate = l.TaxRateRate,
                    Description = l.Description
                });
            }

            RecalculateTotals();
        }

        private void AddEmptyLine()
        {
            _lines.Add(new InvoiceLineVm
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
                        InvoiceLineVm line = _lines[rowHandle];
                        line.ProductId = prod.Id;
                        line.TaxRateRate = prod.TaxRateRate > 0 && prod.TaxRateRate <= 1 ? prod.TaxRateRate * 100 : prod.TaxRateRate;

                        bool isSales = cmbInvoiceType.SelectedIndex == 0;
                        var priceObj = prod.Prices.FirstOrDefault(p => p.PriceType == (isSales ? ProductPriceType.Sale : ProductPriceType.Purchase));
                        if (priceObj != null)
                        {
                            line.UnitPrice = priceObj.UnitPrice;
                        }

                        gridLinesView.RefreshRow(rowHandle);
                        RecalculateTotals();
                    }
                }
            }
        }

        private void GridLinesView_CellValueChanged(object? sender, CellValueChangedEventArgs e)
        {
            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            decimal subTotal = 0;
            decimal taxTotal = 0;

            foreach (var line in _lines)
            {
                decimal lineSub = line.Quantity * line.UnitPrice;
                decimal rate = line.TaxRateRate > 1 ? line.TaxRateRate / 100m : line.TaxRateRate;
                decimal tax = Math.Round(lineSub * rate, 2);

                line.TaxAmount = tax;
                line.TotalAmount = lineSub + tax;

                subTotal += lineSub;
                taxTotal += tax;
            }

            lblSubTotalValue.Text = subTotal.ToString("n2") + " ₺";
            lblTaxTotalValue.Text = taxTotal.ToString("n2") + " ₺";
            lblGrandTotalValue.Text = (subTotal + taxTotal).ToString("n2") + " ₺";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
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

            var validLines = _lines.Where(l => l.ProductId != Guid.Empty && l.Quantity > 0).ToList();
            if (validLines.Count == 0)
            {
                ToastHelper.Show("Faturada en az bir geçerli ürün kalemi bulunmalıdır.", ToastType.Warning);
                return;
            }

            InvoiceType invoiceType = cmbInvoiceType.SelectedIndex == 0 ? InvoiceType.Sales : InvoiceType.Purchase;
            DateOnly date = DateOnly.FromDateTime(dtDate.DateTime);

            List<InvoiceCreateLineModel> lineModels = validLines.Select(l => new InvoiceCreateLineModel(
                l.ProductId,
                l.Quantity,
                l.UnitPrice,
                l.TaxRateRate,
                l.Description)).ToList();

            InvoiceCreateCommand command = new(
                InvoiceNumber: number,
                InvoiceType: invoiceType,
                Date: date,
                CustomerId: invoiceType == InvoiceType.Sales ? accountId : null,
                SupplierId: invoiceType == InvoiceType.Purchase ? accountId : null,
                Description: txtDescription.Text.Trim(),
                Lines: lineModels);

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
    }

    public sealed class InvoiceLineVm : INotifyPropertyChanged
    {
        private Guid _productId;
        private decimal _quantity;
        private decimal _unitPrice;
        private decimal _taxRateRate;
        private decimal _taxAmount;
        private decimal _totalAmount;
        private string? _description;

        public Guid ProductId { get => _productId; set { _productId = value; OnPropertyChanged(nameof(ProductId)); } }
        public decimal Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(nameof(Quantity)); } }
        public decimal UnitPrice { get => _unitPrice; set { _unitPrice = value; OnPropertyChanged(nameof(UnitPrice)); } }
        public decimal TaxRateRate { get => _taxRateRate; set { _taxRateRate = value; OnPropertyChanged(nameof(TaxRateRate)); } }
        public decimal TaxAmount { get => _taxAmount; set { _taxAmount = value; OnPropertyChanged(nameof(TaxAmount)); } }
        public decimal TotalAmount { get => _totalAmount; set { _totalAmount = value; OnPropertyChanged(nameof(TotalAmount)); } }
        public string? Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
