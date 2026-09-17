using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.CostSlips;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.Reports;
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
using ReportItemDto = Cost.Accounting.Automation.WinFormsApp.Forms.CostSlips.CostSlipItemDto;
using AppCostSlip = Cost.Accounting.Automation.Application.CostSlips.CostSlipDto;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public partial class CostSlipEditForm : XtraForm
    {
        private readonly CostSlipListDto? _editing;
        private readonly BindingList<CostSlipItemEditDto> _lines = [];
        private List<ProductDto> _products = [];
        private List<CustomerDto> _customers = [];
        private List<ChartOfAccountLookUpDto> _workshops = [];
        private RepositoryItemSearchLookUpEdit _riProductLookUp = default!;
        private RepositoryItemLookUpEdit _riAccountLookUp = default!;
        private string _workshopName = string.Empty;
        private bool _saved;

        public CostSlipEditForm() : this(null)
        {
        }

        public CostSlipEditForm(CostSlipListDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            IconOptions.SvgImage = DxIcon.Percent;
            lblHeaderIcon.ImageOptions.SvgImage = DxIcon.Percent;

            Text = _editing is null ? "Yeni Maliyet Pusulası" : "Maliyet Pusulası İncele";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Atölyeyi seçin, üretilen ürünü belirleyin ve gider kalemlerini girin"
                : _editing.Status == CostSlipStatus.Draft
                    ? "Taslağı düzenleyip kaydedebilir veya onaylayabilirsiniz"
                    : "Maliyet pusulası ve gider kalemi detayları";

            InitControls();
            WireEvents();
        }

        private void InitControls()
        {
            cmbCostSlipType.Properties.Items.Clear();
            cmbCostSlipType.Properties.Items.Add("Mamul Maliyet Pusulası");
            cmbCostSlipType.Properties.Items.Add("Hizmet Maliyet Pusulası");
            cmbCostSlipType.Properties.Items.Add("Yarı Mamul Maliyet Pusulası");
            cmbCostSlipType.SelectedIndex = 0;

            dtCostDate.DateTime = DateTime.Today;
            txtQuantity.EditValue = 1;

            gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
            gridLines.DataSource = _lines;
            ConfigureGrid();

            btnApprove.Visible = false;
            btnApprove.Text = "Pusulayı Onayla";
            btnPrintSlip.Enabled = false;
            lblStatusValue.Text = "";

            if (_editing is not null)
            {
                bool isDraft = _editing.Status == CostSlipStatus.Draft;

                btnSave.Visible = false;
                btnSaveDraft.Visible = isDraft;
                btnApprove.Visible = isDraft;
                btnAddLine.Enabled = isDraft;
                btnDeleteLine.Enabled = isDraft;
                gridLinesView.OptionsBehavior.Editable = isDraft;

                if (!isDraft)
                {
                    btnPrintSlip.Enabled = true;
                    cmbCostSlipType.ReadOnly = true;
                    txtSlipNumber.ReadOnly = true;
                    dtCostDate.ReadOnly = true;
                    lookUpWorkshop.ReadOnly = true;
                    lookUpProducedProduct.ReadOnly = true;
                    lookUpCustomer.ReadOnly = true;
                    txtQuantity.ReadOnly = true;
                    txtDescription.ReadOnly = true;
                }
            }
        }

        private void ConfigureGrid()
        {
            gridLinesView.RowHeight = 28;
            gridLinesView.Columns.Clear();

            _riProductLookUp = new RepositoryItemSearchLookUpEdit
            {
                ValueMember = nameof(ProductDto.Id),
                DisplayMember = nameof(ProductDto.Name),
                NullText = "Ürün / Masraf Seçiniz...",
                PopupFilterMode = PopupFilterMode.Contains
            };
            _riProductLookUp.View.OptionsBehavior.AutoPopulateColumns = false;
            _riProductLookUp.View.Columns.AddField(nameof(ProductDto.Name)).Caption = "Ürün Adı";
            _riProductLookUp.View.Columns[0].Visible = true;
            _riProductLookUp.EditValueChanged += RiProductLookUp_EditValueChanged;

            _riAccountLookUp = new RepositoryItemLookUpEdit
            {
                PopupFilterMode = PopupFilterMode.Contains
            };

            RepositoryItemSpinEdit riQuantity = new() { MinValue = 0.0001m, MaxValue = 999999999, Increment = 1, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemSpinEdit riPrice = new() { MinValue = 0, MaxValue = 999999999, Increment = 10, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemSpinEdit riReadOnlyMoney = new() { ReadOnly = true, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } };
            RepositoryItemTextEdit riDesc = new();

            gridLines.RepositoryItems.AddRange([_riProductLookUp, _riAccountLookUp, riQuantity, riPrice, riReadOnlyMoney, riDesc]);

            GridColumn[] columns =
            [
                new() { Caption = "Ürün / Masraf", FieldName = nameof(CostSlipItemEditDto.ProductId), Visible = true, Width = 300, ColumnEdit = _riProductLookUp },
                new() { Caption = "Hesap", FieldName = nameof(CostSlipItemEditDto.ExpenseAccountType), Visible = true, Width = 280, ColumnEdit = _riAccountLookUp },
                new() { Caption = "Miktar", FieldName = nameof(CostSlipItemEditDto.Quantity), Visible = true, Width = 90, ColumnEdit = riQuantity, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Birim Fiyat", FieldName = nameof(CostSlipItemEditDto.UnitPrice), Visible = true, Width = 110, ColumnEdit = riPrice, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Tutar", FieldName = nameof(CostSlipItemEditDto.TotalAmount), Visible = true, Width = 120, ColumnEdit = riReadOnlyMoney, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Açıklama", FieldName = nameof(CostSlipItemEditDto.Description), Visible = true, MinWidth = 200, Width = 460, ColumnEdit = riDesc }
            ];

            foreach (GridColumn column in columns)
            {
                bool isNumeric = column.FieldName is nameof(CostSlipItemEditDto.Quantity)
                    or nameof(CostSlipItemEditDto.UnitPrice)
                    or nameof(CostSlipItemEditDto.TotalAmount);

                if (isNumeric)
                {
                    column.OptionsColumn.FixedWidth = true;
                    column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                }

                column.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }

            gridLinesView.Columns.AddRange(columns);
            gridLinesView.OptionsView.ColumnAutoWidth = true;
            gridLinesView.OptionsCustomization.AllowColumnResizing = true;
        }

        private void WireEvents()
        {
            Load += CostSlipEditForm_Load;
            cmbCostSlipType.SelectedIndexChanged += CmbCostSlipType_SelectedIndexChanged;
            btnAddLine.Click += (_, _) => AddEmptyLine();
            btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
            btnSave.Click += BtnSave_Click;
            btnSaveDraft.Click += BtnSaveDraft_Click;
            btnApprove.Click += BtnApprove_Click;
            btnPrintSlip.Click += (_, _) => ShowPreviewAsync();
            btnCancel.Click += (_, _) => Close();
            gridLinesView.CellValueChanged += GridLinesView_CellValueChanged;
            gridLinesView.ValidatingEditor += GridLinesView_ValidatingEditor;
        }

        private async void CostSlipEditForm_Load(object? sender, EventArgs e)
        {
            await LoadLookUpsAsync();

            if (_editing is not null)
            {
                await LoadDetailsAndPopulateAsync();
            }

            UpdateProducedProductAvailability();
            UpdateAccountDataSource();

            if (_editing is null)
            {
                await AutoAssignNumberAsync();
            }
        }

        private async Task LoadLookUpsAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                _products = (await mediator.Send(new ProductGetAllQuery())).ToList();
                _customers = (await mediator.Send(new CustomerGetAllQuery())).ToList();
                _workshops = ((await mediator.Send(new ChartOfAccountLookUpQuery())).Data ?? [])
                    .Where(w => w.Type == ChartOfAccountType.Workshop)
                    .ToList();

                lookUpWorkshop.Properties.DataSource = _workshops;
                lookUpWorkshop.Properties.ValueMember = nameof(ChartOfAccountLookUpDto.Id);
                lookUpWorkshop.Properties.DisplayMember = nameof(ChartOfAccountLookUpDto.Display);
                lookUpWorkshop.Properties.BestFitMode = BestFitMode.BestFit;
                lookUpWorkshopView.Columns.Clear();
                lookUpWorkshopView.OptionsBehavior.AutoPopulateColumns = false;
                GridColumn workshopColumn = lookUpWorkshopView.Columns.AddField(nameof(ChartOfAccountLookUpDto.Display));
                workshopColumn.Caption = "Atölye";
                workshopColumn.VisibleIndex = 0;
                workshopColumn.Width = 300;
                lookUpWorkshopView.BestFitColumns();

                lookUpCustomer.Properties.DataSource = _customers;
                lookUpCustomer.Properties.ValueMember = nameof(CustomerDto.Id);
                lookUpCustomer.Properties.DisplayMember = nameof(CustomerDto.Name);
                lookUpCustomer.Properties.BestFitMode = BestFitMode.BestFit;
                lookUpCustomerView.Columns.Clear();
                lookUpCustomerView.OptionsBehavior.AutoPopulateColumns = false;
                GridColumn customerColumn = lookUpCustomerView.Columns.AddField(nameof(CustomerDto.Name));
                customerColumn.Caption = "Müşteri Adı";
                customerColumn.VisibleIndex = 0;
                customerColumn.Width = 220;
                lookUpCustomerView.Columns.AddField(nameof(CustomerDto.City)).Caption = "Şehir";
                foreach (GridColumn col in lookUpCustomerView.Columns)
                {
                    col.Visible = true;
                }
                lookUpCustomerView.BestFitColumns();

                lookUpProducedProduct.Properties.DataSource = _products;
                lookUpProducedProduct.Properties.ValueMember = nameof(ProductDto.Id);
                lookUpProducedProduct.Properties.DisplayMember = nameof(ProductDto.Name);
                lookUpProducedProduct.Properties.BestFitMode = BestFitMode.BestFit;
                lookUpProducedProductView.Columns.Clear();
                lookUpProducedProductView.OptionsBehavior.AutoPopulateColumns = false;
                GridColumn productColumn = lookUpProducedProductView.Columns.AddField(nameof(ProductDto.Name));
                productColumn.Caption = "Ürün Adı";
                productColumn.VisibleIndex = 0;
                productColumn.Width = 240;
                lookUpProducedProductView.Columns.AddField(nameof(ProductDto.ProductCode)).Caption = "Ürün Kodu";
                lookUpProducedProductView.Columns.AddField(nameof(ProductDto.ProductUnitTypeName)).Caption = "Birim";
                foreach (GridColumn col in lookUpProducedProductView.Columns)
                {
                    col.Visible = true;
                }
                lookUpProducedProductView.BestFitColumns();

                _riProductLookUp.DataSource = _products;
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Veriler yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private async Task LoadDetailsAndPopulateAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(new CostSlipGetByIdQuery(_editing!.Id), CancellationToken.None);
                if (result.IsSuccessful && result.Data is not null)
                {
                    PopulateExisting(result.Data);
                }
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Pusula detayları yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private void PopulateExisting(AppCostSlip slip)
        {
            cmbCostSlipType.SelectedIndex = slip.CostSlipType switch
            {
                CostSlipType.Service => 1,
                CostSlipType.SemiFinishedProduct => 2,
                _ => 0
            };
            txtSlipNumber.Text = slip.SlipNumber;
            dtCostDate.DateTime = slip.CostDate.ToDateTime(TimeOnly.MinValue);
            lookUpWorkshop.EditValue = slip.WorkshopId;
            _workshopName = slip.WorkshopName;
            lookUpProducedProduct.EditValue = slip.ProducedProductId;
            lookUpCustomer.EditValue = slip.CustomerId;
            txtQuantity.EditValue = slip.Quantity;
            txtDescription.Text = slip.Description;

            lblStatusValue.Text = "Durum: " + (slip.Status == CostSlipStatus.Approved ? "Onaylı" : "Taslak");
            lblStatusValue.Appearance.ForeColor = slip.Status == CostSlipStatus.Approved
                ? SkinTheme.Success
                : SkinTheme.Warning;

            _lines.Clear();
            foreach (var item in slip.CostSlipItems)
            {
                _lines.Add(new CostSlipItemEditDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductUnitTypeId = item.ProductUnitTypeId,
                    ProductUnitTypeName = item.ProductUnitTypeName,
                    ExpenseAccountType = item.ExpenseAccountType,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalAmount = item.TotalAmount,
                    Description = item.Description
                });
            }

            RecalculateTotals();
        }

        private void AddEmptyLine()
        {
            _lines.Add(new CostSlipItemEditDto
            {
                ExpenseAccountType = CurrentType == CostSlipType.Service
                    ? ExpenseAccountType.Account740_1
                    : ExpenseAccountType.Account710,
                Quantity = 1,
                UnitPrice = 0
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

        private CostSlipType CurrentType => cmbCostSlipType.SelectedIndex switch
        {
            1 => CostSlipType.Service,
            2 => CostSlipType.SemiFinishedProduct,
            _ => CostSlipType.Product
        };

        private async void CmbCostSlipType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateProducedProductAvailability();
            UpdateAccountDataSource();

            if (_editing is null)
            {
                await AutoAssignNumberAsync();
            }
        }

        private void UpdateProducedProductAvailability()
        {
            bool isService = CurrentType == CostSlipType.Service;
            lookUpProducedProduct.Enabled = !isService;
            lookUpProducedProduct.Properties.NullText = isService ? "Hizmet pusulasında gerekmez" : "Üretilen Ürün Seçiniz...";
        }

        private void UpdateAccountDataSource()
        {
            List<ExpenseAccountType> accounts = ExpenseAccountHelper.GetFilteredAccounts(CurrentType);

            _riAccountLookUp.DataSource = accounts
                .Select(a => new
                {
                    Value = a,
                    Name = Cost.Accounting.Automation.Application.Helpers.EnumDisplay.GetDisplayName(a)
                })
                .ToList();
            _riAccountLookUp.ValueMember = "Value";
            _riAccountLookUp.DisplayMember = "Name";
            _riAccountLookUp.BestFitMode = BestFitMode.BestFit;
        }

        private async Task AutoAssignNumberAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(new CostSlipGetNextNumberQuery(CurrentType), CancellationToken.None);
                if (result.IsSuccessful && !string.IsNullOrWhiteSpace(result.Data))
                {
                    txtSlipNumber.Text = result.Data;
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CostSlip.NextNumber", ex);
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
                        CostSlipItemEditDto line = _lines[rowHandle];
                        line.ProductId = prod.Id;
                        line.ProductName = prod.Name;
                        line.ProductUnitTypeId = prod.ProductUnitTypeId;
                        line.ProductUnitTypeName = prod.ProductUnitTypeName;

                        gridLinesView.RefreshRow(rowHandle);
                        RecalculateTotals();
                    }
                }
            }
        }

        private void GridLinesView_ValidatingEditor(object? sender, BaseContainerValidateEditorEventArgs e)
        {
            string? columnName = gridLinesView.FocusedColumn?.FieldName;

            if (columnName == nameof(CostSlipItemEditDto.UnitPrice) && e.Value is decimal price && price < 0)
            {
                e.Valid = false;
                e.ErrorText = "Birim fiyat negatif olamaz.";
                return;
            }

            if (columnName == nameof(CostSlipItemEditDto.Quantity) && e.Value is decimal qty && qty <= 0)
            {
                e.Valid = false;
                e.ErrorText = "Miktar sıfırdan büyük olmalıdır.";
            }
        }

        private void GridLinesView_CellValueChanged(object? sender, CellValueChangedEventArgs e)
        {
            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            foreach (var line in _lines)
            {
                line.TotalAmount = Math.Round(line.Quantity * line.UnitPrice, 2);
            }

            Dictionary<string, decimal> byGroup = [];
            foreach (var line in _lines)
            {
                string param = GetReportAccountParam(line.ExpenseAccountType);
                byGroup.TryGetValue(param, out decimal current);
                byGroup[param] = current + line.TotalAmount;
            }

            CostSlipType type = CurrentType;
            bool isService = type == CostSlipType.Service;
            decimal direct = 0;
            decimal other = 0;

            foreach (var (param, amount) in byGroup)
            {
                if (isService ? param == "M740" : param is "M710" or "M720" or "M730")
                {
                    direct += amount;
                }
                else if (param is "M750" or "M760" or "M770" or "M780")
                {
                    other += amount;
                }
            }

            decimal grand = direct + other;

            lblDirectValue.Text = direct.ToString("n2") + " ₺";
            lblOtherValue.Text = other.ToString("n2") + " ₺";
            lblGrandTotalValue.Text = grand.ToString("n2") + " ₺";

            var breakdown = _lines
                .GroupBy(l => l.ExpenseAccountType)
                .OrderBy(g => (int)g.Key)
                .Select(g => $"{Cost.Accounting.Automation.Application.Helpers.EnumDisplay.GetDisplayName(g.Key)}: {g.Sum(l => l.TotalAmount):n2} ₺");

            lblBreakdownLabel.Text = breakdown.Any()
                ? string.Join(Environment.NewLine, breakdown)
                : "Gider kalemi ekleyin.";

            gridLinesView.RefreshData();
        }

        private static string GetReportAccountParam(ExpenseAccountType account)
        {
            byte value = (byte)account;
            return value switch
            {
                1 => "M710",
                2 or 3 => "M720",
                >= 4 and <= 10 => "M730",
                >= 11 and <= 18 => "M740",
                19 => "M750",
                20 => "M760",
                21 => "M770",
                22 => "M780",
                _ => string.Empty
            };
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
            string number = txtSlipNumber.Text.Trim();
            if (string.IsNullOrEmpty(number))
            {
                ToastHelper.Show("Pusula numarası boş olamaz.", ToastType.Warning);
                txtSlipNumber.Focus();
                return;
            }

            if (lookUpWorkshop.EditValue is not Guid workshopId || workshopId == Guid.Empty)
            {
                ToastHelper.Show("Lütfen bir atölye seçiniz.", ToastType.Warning);
                lookUpWorkshop.Focus();
                return;
            }

            if (CurrentType is CostSlipType.Product or CostSlipType.SemiFinishedProduct)
            {
                if (lookUpProducedProduct.EditValue is not Guid producedGuid || producedGuid == Guid.Empty)
                {
                    ToastHelper.Show("Mamul / yarı mamul pusulası için üretilen ürün seçilmelidir.", ToastType.Warning);
                    lookUpProducedProduct.Focus();
                    return;
                }
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
            {
                ToastHelper.Show("Miktar sıfırdan büyük bir tam sayı olmalıdır.", ToastType.Warning);
                txtQuantity.Focus();
                return;
            }

            var validLines = _lines.Where(l => l.Quantity > 0).ToList();
            if (validLines.Count == 0 || validLines.Any(l => l.UnitPrice < 0))
            {
                ToastHelper.Show("En az bir geçerli gider kalemi (miktarı sıfırdan büyük) olmalıdır.", ToastType.Warning);
                return;
            }

            decimal grandTotal = validLines.Sum(l => l.Quantity * l.UnitPrice);
            if (grandTotal <= 0)
            {
                ToastHelper.Show("Pusula genel toplamı sıfırdan büyük olmalıdır.", ToastType.Warning);
                return;
            }

            DateOnly date = DateOnly.FromDateTime(dtCostDate.DateTime);

            List<CostSlipItemModel> itemModels = validLines.Select(l => new CostSlipItemModel(
                l.ProductId,
                l.ProductUnitTypeId,
                l.ExpenseAccountType,
                l.Quantity,
                l.UnitPrice,
                l.Description)).ToList();

            Guid? producedProductId = lookUpProducedProduct.EditValue is Guid pid && pid != Guid.Empty ? pid : null;
            Guid? customerId = lookUpCustomer.EditValue is Guid cid && cid != Guid.Empty ? cid : null;

            btnSave.Enabled = false;
            btnSaveDraft.Enabled = false;
            try
            {
                bool ok;
                if (_editing is { Status: CostSlipStatus.Draft })
                {
                    ok = await CrudExecutor.ExecuteAsync(new CostSlipUpdateCommand(
                        _editing.Id,
                        number,
                        CurrentType,
                        date,
                        workshopId,
                        producedProductId,
                        customerId,
                        quantity,
                        txtDescription.Text.Trim(),
                        itemModels));
                }
                else
                {
                    ok = await CrudExecutor.ExecuteAsync(new CostSlipCreateCommand(
                        SlipNumber: number,
                        CostSlipType: CurrentType,
                        CostDate: date,
                        WorkshopId: workshopId,
                        ProducedProductId: producedProductId,
                        CustomerId: customerId,
                        Quantity: quantity,
                        Description: txtDescription.Text.Trim(),
                        Items: itemModels,
                        IsApproved: approve));
                }

                if (ok)
                {
                    string message = _editing is { Status: CostSlipStatus.Draft }
                        ? "Maliyet pusulası taslağı güncellendi."
                        : approve
                            ? "Maliyet pusulası onaylandı; stok hareketleri oluşturuldu."
                            : "Maliyet pusulası taslak olarak kaydedildi. Onaylanınca stok hareketleri oluşturulacak.";
                    ToastHelper.Show(message, ToastType.Success);

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
                bool ok = await CrudExecutor.ExecuteAsync(new CostSlipApproveCommand(_editing.Id));
                if (ok)
                {
                    ToastHelper.Show("Maliyet pusulası onaylandı; stok hareketleri oluşturuldu.", ToastType.Success);
                    _saved = true;
                    LockAfterApproval();
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
            cmbCostSlipType.ReadOnly = true;
            txtSlipNumber.ReadOnly = true;
            dtCostDate.ReadOnly = true;
            lookUpWorkshop.ReadOnly = true;
            lookUpProducedProduct.ReadOnly = true;
            lookUpCustomer.ReadOnly = true;
            txtQuantity.ReadOnly = true;
            txtDescription.ReadOnly = true;
            gridLinesView.OptionsBehavior.Editable = false;

            lblStatusValue.Text = "Durum: Onaylı";
            lblStatusValue.Appearance.ForeColor = SkinTheme.Success;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_saved && DialogResult == DialogResult.None)
            {
                DialogResult = DialogResult.OK;
            }

            base.OnFormClosing(e);
        }

        private async void ShowPreviewAsync()
        {
            WaitForm? waitForm = null;
            try
            {
                bool isService = CurrentType == CostSlipType.Service;
                DevExpress.XtraReports.UI.XtraReport report = isService
                    ? new CostSlipServiceReport()
                    : new CostSlipProductReport();

                report.RequestParameters = false;

                List<ReportItemDto> items = _lines
                    .Where(l => l.Quantity > 0)
                    .Select(l => new ReportItemDto
                    {
                        ProductName = string.IsNullOrWhiteSpace(l.ProductName)
                            ? l.Description
                            : l.ProductName,
                        ProductUnitTypeName = l.ProductUnitTypeName,
                        UnitPrice = l.UnitPrice,
                        Quantity = l.Quantity,
                        TotalAmount = l.TotalAmount,
                        ExpenseAccountType = l.ExpenseAccountType
                    })
                    .ToList();

                report.DataSource = items;

                var totals = _lines.Where(l => l.Quantity > 0).ToDictionary(
                    l => GetReportAccountParam(l.ExpenseAccountType),
                    l => l.Quantity * l.UnitPrice,
                    StringComparer.Ordinal);

                CompanyDto company = await LoadCompanyAsync();

                SetReportParam(report, "Musteri", _customers.FirstOrDefault(c => c.Id == (lookUpCustomer.EditValue is Guid g1 && g1 != Guid.Empty ? g1 : Guid.Empty))?.Name ?? string.Empty);
                SetReportParam(report, "MamulAdi", lookUpProducedProduct.EditValue is Guid pid && pid != Guid.Empty
                    ? _products.FirstOrDefault(p => p.Id == pid)?.Name ?? string.Empty
                    : string.Empty);
                SetReportParam(report, "Miktari", int.TryParse(txtQuantity.Text.Trim(), out int qty) ? qty : 0);
                SetReportParam(report, "Tarih", DateOnly.FromDateTime(dtCostDate.DateTime));
                SetReportParam(report, "Isyurdu", company.Name);

                string workshopName = lookUpWorkshop.EditValue is Guid wid && wid != Guid.Empty
                    ? _workshops.FirstOrDefault(w => w.Id == wid)?.Display ?? string.Empty
                    : _workshopName;
                SetReportParam(report, "Atolye", workshopName);
                SetReportParam(report, "Antet", company.Name);

                string[] moneyParams = ["M710", "M720", "M730", "M740", "M750", "M760", "M770", "M780"];
                foreach (string param in moneyParams)
                {
                    decimal value = totals.TryGetValue(param, out decimal sum) ? Math.Round(sum, 2) : 0;
                    SetReportParam(report, param, value);
                }

                decimal grandTotal = _lines.Where(l => l.Quantity > 0).Sum(l => l.Quantity * l.UnitPrice);
                SetReportParam(report, "Toplam", Math.Round(grandTotal, 2));

                waitForm = WaitFormHelper.Show<WaitForm>("Pusula hazırlanıyor...", "Lütfen bekleyin...");
                await report.CreateDocumentAsync(CancellationToken.None);

                waitForm.Close();
                waitForm.Dispose();
                waitForm = null;

                ReportPrintTool tool = new(report);
                tool.ShowRibbonPreviewDialog();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Maliyet pusulası açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
            finally
            {
                waitForm?.Close();
                waitForm?.Dispose();
            }
        }

        private static void SetReportParam(DevExpress.XtraReports.UI.XtraReport report, string name, object value)
        {
            if (report.Parameters[name] is { } parameter)
            {
                parameter.Value = value;
            }
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
                CrashLog.WriteException("CostSlip.Company", ex);
            }

            return new CompanyDto();
        }
    }
}