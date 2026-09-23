using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Application.Recipes;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.StockIssues;
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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Data;
using TS.MediatR;
using ReportItemDto = Cost.Accounting.Automation.WinFormsApp.Reports.CostSlipReport.CostSlipItemDto;
using AppCostSlip = Cost.Accounting.Automation.Application.CostSlips.CostSlipDto;
using Cost.Accounting.Automation.WinFormsApp.Reports.CostSlipReport;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public partial class CostSlipEditForm : XtraForm
    {
        private CostSlipListDto? _editing;
        private readonly BindingList<CostSlipItemEditDto> _lines = [];
        private List<ProductDto> _products = [];
        private readonly Dictionary<Guid, List<AtelierTransferProductDto>> _transferredByWorkshop = [];
        private Dictionary<Guid, WorkshopLink> _workshopLinks = [];
        private readonly Dictionary<ExpenseAccountType, decimal> _accountAmounts = [];
        private readonly Dictionary<ExpenseAccountType, TextEdit> _accountInputs = [];
        private readonly List<TextEdit> _accountInputList = [];
        private bool _syncingTotals;
        private decimal _grandTotal;
        private decimal _unitCost;
        private ExpenseAccountType? _roundingTargetAccount;
        private decimal _roundingDiff;
        private List<ChartOfAccountLookUpDto> _workshops = [];
        private RepositoryItemSearchLookUpEdit _riProductLookUp = default!;
        private string _workshopName = string.Empty;
        private bool _saved;
        private string _autoDescription = string.Empty;
        private Guid? _semiFinishedProductId;
        private decimal _semiFinishedBalance;

        private sealed record WorkshopLink(string Code, Guid? SemiFinishedAccountId, Guid? FinishedAccountId);

        private sealed record ProductLookUpItem(Guid Id, string Name, string Code, string UnitType, decimal Balance, decimal Draft)
        {
            public string DisplayName => $"{Name}  [Bakiye: {Balance:n2}]";
        }

        public CostSlipEditForm() : this(null)
        {
        }

        public CostSlipEditForm(CostSlipListDto? existing)
        {
            InitializeComponent();
            _editing = existing;

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

            RebuildAccountPanel();

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
                    txtQuantity.ReadOnly = true;
                    txtDescription.ReadOnly = true;

                    foreach (TextEdit input in _accountInputList)
                    {
                        input.ReadOnly = true;
                    }
                }
            }
        }

        private void ConfigureGrid()
        {
            gridLinesView.RowHeight = 28;
            gridLinesView.Columns.Clear();

            _riProductLookUp = new RepositoryItemSearchLookUpEdit
            {
                ValueMember = nameof(ProductLookUpItem.Id),
                DisplayMember = nameof(ProductLookUpItem.Name),
                NullText = "Ürün / Masraf Seçiniz...",
                PopupFilterMode = PopupFilterMode.Contains
            };
            _riProductLookUp.View.OptionsBehavior.AutoPopulateColumns = false;
            _riProductLookUp.View.Columns.AddField(nameof(ProductLookUpItem.Name)).Caption = "Ürün Adı";
            _riProductLookUp.View.Columns[0].Visible = true;
            _riProductLookUp.View.Columns[0].Width = 240;
            GridColumn colBalance = _riProductLookUp.View.Columns.AddField(nameof(ProductLookUpItem.Balance));
            colBalance.Caption = "Bakiye";
            colBalance.Visible = true;
            colBalance.Width = 80;
            colBalance.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colBalance.DisplayFormat.FormatString = "n2";
            colBalance.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            GridColumn colDraft = _riProductLookUp.View.Columns.AddField(nameof(ProductLookUpItem.Draft));
            colDraft.Caption = "Taslak";
            colDraft.Visible = true;
            colDraft.Width = 80;
            colDraft.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colDraft.DisplayFormat.FormatString = "n2";
            colDraft.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            _riProductLookUp.EditValueChanged += RiProductLookUp_EditValueChanged;

            RepositoryItemSpinEdit riQuantity = new() { MinValue = 0.0001m, MaxValue = 999999999, Increment = 1, Mask = { MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric, EditMask = "n2", UseMaskAsDisplayFormat = true } };
            RepositoryItemSpinEdit riPrice = new() { MinValue = 0, ReadOnly = true, Mask = { MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric, EditMask = "n2", UseMaskAsDisplayFormat = true } };
            RepositoryItemSpinEdit riReadOnlyMoney = new() { ReadOnly = true, Mask = { MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric, EditMask = "n2", UseMaskAsDisplayFormat = true } };
            RepositoryItemTextEdit riDesc = new();

            gridLines.RepositoryItems.AddRange([_riProductLookUp, riQuantity, riPrice, riReadOnlyMoney, riDesc]);

            GridColumn[] columns =
            [
                new() { Caption = "Ürün / Masraf", FieldName = nameof(CostSlipItemEditDto.ProductId), Visible = true, Width = 280, ColumnEdit = _riProductLookUp },
                new() { Caption = "Birim", FieldName = nameof(CostSlipItemEditDto.ProductUnitTypeName), Visible = true, Width = 60, OptionsColumn = { AllowEdit = false }, AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Center } } },
                new() { Caption = "Miktar", FieldName = nameof(CostSlipItemEditDto.Quantity), Visible = true, Width = 90, ColumnEdit = riQuantity, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Birim Fiyat", FieldName = nameof(CostSlipItemEditDto.UnitPrice), Visible = true, Width = 100, ColumnEdit = riPrice, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Tutar", FieldName = nameof(CostSlipItemEditDto.TotalAmount), Visible = true, Width = 110, ColumnEdit = riReadOnlyMoney, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Gelen", FieldName = nameof(CostSlipItemEditDto.TransferredQuantity), Visible = true, Width = 90, ColumnEdit = riReadOnlyMoney, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Kalan", FieldName = nameof(CostSlipItemEditDto.AvailableQuantity), Visible = true, Width = 90, ColumnEdit = riReadOnlyMoney, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Açıklama", FieldName = nameof(CostSlipItemEditDto.Description), Visible = true, MinWidth = 160, Width = 400, ColumnEdit = riDesc }
            ];

            foreach (GridColumn column in columns)
            {
                bool isNumeric = column.FieldName is nameof(CostSlipItemEditDto.Quantity)
                    or nameof(CostSlipItemEditDto.UnitPrice)
                    or nameof(CostSlipItemEditDto.TotalAmount)
                    or nameof(CostSlipItemEditDto.TransferredQuantity)
                    or nameof(CostSlipItemEditDto.AvailableQuantity);

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

            gridLinesView.OptionsView.ShowFooter = true;
            GridColumn totalColumn = columns.First(c => c.FieldName == nameof(CostSlipItemEditDto.TotalAmount));
            totalColumn.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            totalColumn.SummaryItem.DisplayFormat = "{0:n2} ₺";

            GridColumn priceColumn = columns.First(c => c.FieldName == nameof(CostSlipItemEditDto.UnitPrice));
            priceColumn.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            priceColumn.SummaryItem.DisplayFormat = "";

            GridColumn descriptionColumn = columns.First(c => c.FieldName == nameof(CostSlipItemEditDto.Description));
            descriptionColumn.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            descriptionColumn.SummaryItem.DisplayFormat = "";
        }

        private void WireEvents()
        {
            Load += CostSlipEditForm_Load;
            cmbCostSlipType.SelectedIndexChanged += CmbCostSlipType_SelectedIndexChanged;
            lookUpWorkshop.EditValueChanged += LookUpWorkshop_EditValueChanged;
            btnAddLine.Click += (_, _) => AddEmptyLine();
            btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
            btnSave.Click += BtnSave_Click;
            btnSaveDraft.Click += BtnSaveDraft_Click;
            btnApprove.Click += BtnApprove_Click;
            btnPrintSlip.Click += (_, _) => ShowPreviewAsync();
            btnCancel.Click += (_, _) => Close();
            gridLinesView.CellValueChanged += GridLinesView_CellValueChanged;
            gridLinesView.ValidatingEditor += GridLinesView_ValidatingEditor;
            gridLinesView.CustomDrawFooterCell += GridLinesView_CustomDrawFooterCell;
            lookUpProducedProduct.EditValueChanged += (_, _) =>
            {
                UpdateAutoDescription();
                _ = CheckSemiFinishedBalanceAsync();
            };
            dtCostDate.EditValueChanged += (_, _) => UpdateAutoDescription();
            txtQuantity.EditValueChanged += (_, _) =>
            {
                RecalculateGrandTotal();
                UpdateAutoDescription();
            };
        }

        private async void CostSlipEditForm_Load(object? sender, EventArgs e)
        {
            Task loadLookUps = LoadLookUpsAsync();
            Task<AppCostSlip?> fetchSlip = _editing is null
                ? Task.FromResult<AppCostSlip?>(null)
                : FetchSlipAsync(_editing.Id);

            await Task.WhenAll(loadLookUps, fetchSlip);

            AppCostSlip? slip = await fetchSlip;
            if (slip is not null)
            {
                PopulateExisting(slip);
            }

            RebuildAccountPanel();
            SyncAccountAmountsToInputs();
            RecalculateTotals();
            UpdateProducedProductAvailability();
            await LoadMaterialProductsAsync();

            if (_editing is null)
            {
                await AutoAssignNumberAsync();
            }
        }

        private static async Task<AppCostSlip?> FetchSlipAsync(Guid slipId)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(new CostSlipGetByIdQuery(slipId), CancellationToken.None);
                return result.IsSuccessful ? result.Data : null;
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Pusula detayları yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
                return null;
            }
        }

        private async Task LoadLookUpsAsync()
        {
            try
            {
                Task<List<ProductDto>> productsTask = LoadProductsAsync();
                Task<List<ChartOfAccountLookUpDto>> accountsTask = LoadAccountLookUpsAsync();

                _products = await productsTask;
                List<ChartOfAccountLookUpDto> accountLookUps = await accountsTask;

                _workshops = accountLookUps
                    .Where(w => w.Type == ChartOfAccountType.Workshop)
                    .ToList();

                _workshopLinks = _workshops.ToDictionary(
                    w => w.Id,
                    w => new WorkshopLink(w.Code, w.SemiFinishedAccountId, w.FinishedAccountId));

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

                _riProductLookUp.DataSource = Array.Empty<ProductLookUpItem>();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Veriler yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private static async Task<List<ProductDto>> LoadProductsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            IQueryable<ProductDto> query = await mediator.Send(new ProductGetAllQuery(), CancellationToken.None);
            return await Task.Run(() => query.ToList());
        }

        private static async Task<List<ChartOfAccountLookUpDto>> LoadAccountLookUpsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            return (await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None)).Data ?? [];
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
            txtQuantity.EditValue = slip.Quantity;
            txtDescription.Text = slip.Description;

            lblStatusValue.Text = "Durum: " + (slip.Status == CostSlipStatus.Approved ? "Onaylı" : "Taslak");
            lblStatusValue.Appearance.ForeColor = slip.Status == CostSlipStatus.Approved
                ? SkinTheme.Success
                : SkinTheme.Warning;

            _lines.Clear();
            foreach (var item in slip.CostSlipItems)
            {
                if (item.ProductId is null)
                {
                    _accountAmounts[item.ExpenseAccountType] = Math.Round(
                        _accountAmounts.GetValueOrDefault(item.ExpenseAccountType) + item.TotalAmount,
                        2);
                    continue;
                }

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

            ProductDto? produced = _products.FirstOrDefault(p => p.Id == slip.ProducedProductId);
            if (produced?.SemiFinishedProductId is Guid semiId)
            {
                _semiFinishedProductId = semiId;
                _semiFinishedBalance = _lines
                    .Where(l => l.ProductId == semiId)
                    .Sum(l => l.Quantity);
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
                bool removedSemi = _lines[rowHandle].ProductId == _semiFinishedProductId;
                _lines.RemoveAt(rowHandle);
                if (removedSemi)
                {
                    _semiFinishedProductId = null;
                    _semiFinishedBalance = 0m;
                }
                RefreshAvailableQuantities();
                RecalculateTotals();
            }
        }

        private CostSlipType CurrentType => cmbCostSlipType.SelectedIndex switch
        {
            1 => CostSlipType.Service,
            2 => CostSlipType.SemiFinishedProduct,
            _ => CostSlipType.Product
        };

        private Guid? SelectedWorkshopId
            => lookUpWorkshop.EditValue is Guid id && id != Guid.Empty ? id : null;

        private ExpenseAccountType DerivedAccount => CurrentType == CostSlipType.Service
            ? ExpenseAccountType.Account740_1
            : ExpenseAccountType.Account710;

        private async void CmbCostSlipType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RebuildAccountPanel();
            SyncAccountAmountsToInputs();
            RecalculateTotals();
            UpdateProducedProductAvailability();
            await LoadMaterialProductsAsync();

            if (_editing is null)
            {
                await AutoAssignNumberAsync();
            }

            UpdateAutoDescription();
        }

        private void LookUpWorkshop_EditValueChanged(object? sender, EventArgs e)
        {
            UpdateProducedProductAvailability();
            _ = LoadMaterialProductsAsync();
            UpdateAutoDescription();
            _ = CheckSemiFinishedBalanceAsync();
        }

        private async Task CheckSemiFinishedBalanceAsync()
        {
            if (_editing is not null)
            {
                return;
            }

            if (CurrentType != CostSlipType.Product)
            {
                return;
            }

            Guid? workshopId = SelectedWorkshopId;
            if (workshopId is null || lookUpProducedProduct.EditValue is not Guid productId)
            {
                return;
            }

            CostSlipSemiFinishedBalanceDto? balanceDto = await GetSemiFinishedBalanceAsync(workshopId.Value, productId);
            if (balanceDto is null)
            {
                return;
            }

            decimal balance = balanceDto.Balance;
            if (balance <= 0)
            {
                return;
            }

            Guid semiProductId = balanceDto.SemiProductId ?? productId;
            string productName = _products.FirstOrDefault(p => p.Id == productId)?.Name ?? "Seçilen ürün";
            string workshopName = _workshops.FirstOrDefault(w => w.Id == workshopId)?.Display ?? string.Empty;
            decimal totalAmount = balance * balanceDto.UnitPrice;

            bool included = _lines.Any(l => l.ProductId == semiProductId && l.Quantity > 0);
            if (included)
            {
                return;
            }

            DialogResult answer = MsgBox.Confirm(
                $"Seçilen mamül ({productName}) için {balance:0.##} adet, {totalAmount:n2} tutarında yarımamül mevcut.\n\n" +
                "Maliyete eklensin mi?",
                "Yarı Mamul Bakiyesi Uyarısı");

            if (answer == DialogResult.Yes)
            {
                AddSemiFinishedLine(semiProductId, balance, balanceDto.UnitPrice);
                lookUpProducedProduct.EditValue = productId;
            }
            else if (answer == DialogResult.No)
            {
                ToastHelper.Show("Yarı mamul maliyeti eklenmeden devam ediliyor.", ToastType.Warning);
            }
        }

        private async Task<bool> ConfirmSemiFinishedBalanceAsync()
        {
            if (CurrentType != CostSlipType.Product)
            {
                return true;
            }

            Guid? workshopId = SelectedWorkshopId;
            Guid? producedProductId = lookUpProducedProduct.EditValue is Guid pid && pid != Guid.Empty ? pid : null;
            if (workshopId is null || producedProductId is null)
            {
                return true;
            }

            CostSlipSemiFinishedBalanceDto? balanceDto = await GetSemiFinishedBalanceAsync(workshopId.Value, producedProductId.Value);
            if (balanceDto is null)
            {
                return true;
            }

            decimal balance = balanceDto.Balance;
            if (balance <= 0)
            {
                return true;
            }

            Guid semiProductId = balanceDto.SemiProductId ?? producedProductId.Value;
            string productName = _products.FirstOrDefault(p => p.Id == producedProductId)?.Name ?? "Seçilen ürün";
            string workshopName = _workshops.FirstOrDefault(w => w.Id == workshopId)?.Display ?? string.Empty;
            decimal totalAmount = balance * balanceDto.UnitPrice;

            bool included = _lines.Any(l => l.ProductId == semiProductId && l.Quantity > 0);
            if (included)
            {
                return true;
            }

            DialogResult answer = MsgBox.Confirm(
                $"Seçilen mamül ({productName}) için {balance:0.##} adet, {totalAmount:n2} tutarında yarımamül mevcut.\n\n" +
                "Maliyete eklensin mi?",
                "Yarı Mamul Bakiyesi Uyarısı");

            if (answer == DialogResult.Yes)
            {
                AddSemiFinishedLine(semiProductId, balance, balanceDto.UnitPrice);
            }

            RefreshAvailableQuantities();

            return true;
        }

        private void AddSemiFinishedLine(Guid productId, decimal quantity, decimal unitPrice)
        {
            ProductDto? prod = _products.FirstOrDefault(p => p.Id == productId);

            _lines.Add(new CostSlipItemEditDto
            {
                ProductId = productId,
                ProductName = prod?.Name ?? string.Empty,
                ProductUnitTypeId = prod?.ProductUnitTypeId,
                ProductUnitTypeName = prod?.ProductUnitTypeName ?? string.Empty,
                ExpenseAccountType = ExpenseAccountType.Account710,
                Quantity = quantity,
                UnitPrice = unitPrice
            });

            _semiFinishedProductId = productId;
            _semiFinishedBalance = quantity;
            UpdateMaterialProductDataSource();

            gridLinesView.RefreshData();
            RecalculateTotals();
        }

        private async Task<CostSlipSemiFinishedBalanceDto?> GetSemiFinishedBalanceAsync(Guid workshopId, Guid productId)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(
                    new CostSlipSemiFinishedBalanceQuery(workshopId, productId),
                    CancellationToken.None);

                return result.IsSuccessful ? result.Data : null;
            }
            catch
            {
                return null;
            }
        }

        private void UpdateAutoDescription()
        {
            string generated = BuildAutoDescription();
            if (string.IsNullOrWhiteSpace(generated))
            {
                return;
            }

            bool currentIsAuto = !string.IsNullOrEmpty(_autoDescription)
                && txtDescription.Text.Trim() == _autoDescription.Trim();
            _autoDescription = generated;

            if (string.IsNullOrWhiteSpace(txtDescription.Text) || currentIsAuto)
            {
                txtDescription.Text = generated;
            }
        }

        private string BuildAutoDescription()
        {
            Guid? workshopId = SelectedWorkshopId;
            if (workshopId is null)
            {
                return string.Empty;
            }

            string workshop = _workshops.FirstOrDefault(w => w.Id == workshopId)?.Display ?? string.Empty;
            if (string.IsNullOrWhiteSpace(workshop))
            {
                return string.Empty;
            }

            int qty = int.TryParse(txtQuantity.Text.Trim(), out int q) ? q : 0;
            string date = dtCostDate.DateTime.ToString("dd.MM.yyyy");

            if (CurrentType == CostSlipType.Service)
            {
                return $"{date} tarihinde {workshop} atölyesinde verilen {qty} adet hizmetin maliyeti";
            }

            if (lookUpProducedProduct.EditValue is not Guid pid)
            {
                return string.Empty;
            }

            string product = _products.FirstOrDefault(p => p.Id == pid)?.Name ?? string.Empty;
            if (string.IsNullOrWhiteSpace(product))
            {
                return string.Empty;
            }

            return $"{date} tarihinde {workshop} atölyesinde üretilen {qty} adet {product} adlı mamülün maliyeti";
        }

        private async Task LoadMaterialProductsAsync()
        {
            try
            {
                if (SelectedWorkshopId is Guid workshopId && !_transferredByWorkshop.ContainsKey(workshopId))
                {
                    await LoadTransferredProductsAsync(workshopId);
                }

                UpdateMaterialProductDataSource();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CostSlip.MaterialProducts", ex);
                ToastHelper.Show("Atölyeye ait ürünler yüklenirken bir hata oluştu.", ToastType.Error);
            }
        }

        private async Task LoadTransferredProductsAsync(Guid workshopId)
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            List<AtelierTransferProductDto> transferred =
                (await mediator.Send(new AtelierTransferProductsQuery(workshopId))).Data ?? [];

            _transferredByWorkshop[workshopId] = transferred;
        }

        private void UpdateProducedProductAvailability()
        {
            CostSlipType type = CurrentType;
            bool isService = type == CostSlipType.Service;

            lookUpProducedProduct.Enabled = !isService;

            string warehouseCode = type == CostSlipType.SemiFinishedProduct ? "151" : "152";
            Guid? targetAccountId = isService ? null : GetWorkshopProducedAccountId(SelectedWorkshopId, type);
            string? workshopName = GetSelectedWorkshopName();

            bool hasSelector = targetAccountId is not null || !string.IsNullOrWhiteSpace(workshopName);

            List<ProductDto> filtered = isService
                ? []
                : !hasSelector
                    ? []
                    : _products
                        .Where(p => MatchesWarehouse(p, warehouseCode) &&
                                    (targetAccountId is Guid tid
                                        ? p.CategoryId == tid
                                        : string.Equals(p.CategoryName.Trim(), workshopName!.Trim(), StringComparison.OrdinalIgnoreCase)))
                        .ToList();

            if (lookUpProducedProduct.EditValue is Guid producedId && producedId != Guid.Empty
                && !filtered.Any(p => p.Id == producedId))
            {
                lookUpProducedProduct.EditValue = null;
            }

            lookUpProducedProduct.Properties.DataSource = filtered;
            lookUpProducedProduct.Properties.NullText = isService
                ? "Hizmet pusulasında gerekmez"
                : SelectedWorkshopId is null
                    ? "Önce atölye seçin"
                    : filtered.Count == 0
                        ? "Bu atölye için üretilen ürün (151/152 bağlantısı veya aynı adlı kategori) tanımlı değil"
                        : "Üretilen Ürün Seçiniz...";
        }

        private string? GetSelectedWorkshopName()
        {
            if (SelectedWorkshopId is not Guid workshopId)
            {
                return null;
            }

            string? name = _workshops.FirstOrDefault(w => w.Id == workshopId)?.Name?.Trim();
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }

        private Guid? GetWorkshopProducedAccountId(Guid? workshopId, CostSlipType type)
        {
            if (workshopId is not Guid id || !_workshopLinks.TryGetValue(id, out WorkshopLink? link))
            {
                return null;
            }

            return type == CostSlipType.SemiFinishedProduct
                ? link.SemiFinishedAccountId
                : link.FinishedAccountId;
        }

        private void UpdateMaterialProductDataSource()
        {
            Guid? workshopId = SelectedWorkshopId;

            List<ProductDto> filtered;
            List<ProductLookUpItem> lookupItems;
            if (workshopId is Guid wid
                && _transferredByWorkshop.TryGetValue(wid, out List<AtelierTransferProductDto>? transferred))
            {
                filtered = _products
                    .Where(p => transferred.Any(t => t.ProductId == p.Id && t.AvailableQuantity > 0m))
                    .ToList();

                lookupItems = filtered.Select(p => new ProductLookUpItem(
                    p.Id,
                    p.Name,
                    p.ProductCode,
                    p.ProductUnitTypeName,
                    transferred.FirstOrDefault(t => t.ProductId == p.Id)?.AvailableQuantity ?? 0m,
                    transferred.FirstOrDefault(t => t.ProductId == p.Id)?.DraftQuantity ?? 0m)).ToList();
            }
            else
            {
                filtered = [];
                lookupItems = [];
            }

            if (_semiFinishedProductId is Guid semiId)
            {
                ProductDto? semi = _products.FirstOrDefault(p => p.Id == semiId);
                if (semi is not null && !lookupItems.Any(x => x.Id == semiId))
                {
                    lookupItems.Add(new ProductLookUpItem(
                        semi.Id,
                        semi.Name,
                        semi.ProductCode,
                        semi.ProductUnitTypeName,
                        _semiFinishedBalance,
                        0m));
                }
            }

            _riProductLookUp.DataSource = lookupItems;
            _riProductLookUp.NullText = workshopId is null
                ? "Önce atölye seçin"
                : _transferredByWorkshop.ContainsKey(workshopId.Value)
                    ? filtered.Count == 0
                        ? "Bu atölyeye transfer edilen ürün bulunamadı"
                        : "Ürün / Masraf Seçiniz..."
                    : "Atölyeye transfer edilen ürünler yükleniyor...";

            foreach (var line in _lines)
            {
                if (line.ProductId is Guid pid
                    && pid != _semiFinishedProductId
                    && !filtered.Any(p => p.Id == pid))
                {
                    line.ProductId = null;
                    line.ProductName = string.Empty;
                    line.ProductUnitTypeId = null;
                    line.ProductUnitTypeName = string.Empty;
                    line.UnitPrice = 0;
                    line.TransferredQuantity = 0;
                    line.AvailableQuantity = 0;
                }
            }

            RefreshAvailableQuantities();
            gridLinesView.RefreshData();
        }

        private void RefreshAvailableQuantities()
        {
            foreach (var line in _lines)
            {
                line.AvailableQuantity = GetDisplayedAvailable(line);
            }
        }

        private decimal GetDisplayedAvailable(CostSlipItemEditDto line)
        {
            if (line.ProductId is not Guid pid || SelectedWorkshopId is not Guid workshopId
                || !_transferredByWorkshop.TryGetValue(workshopId, out List<AtelierTransferProductDto>? transferred))
            {
                return 0m;
            }

            if (pid == _semiFinishedProductId)
            {
                decimal semiConsumed = _lines
                    .Where(l => !ReferenceEquals(l, line) && l.ProductId == pid)
                    .Sum(l => l.Quantity);

                return Math.Max(0m, _semiFinishedBalance - semiConsumed);
            }

            decimal baseAvailable = transferred
                .FirstOrDefault(t => t.ProductId == pid)?.AvailableQuantity ?? 0m;

            decimal reservedByDrafts = GetReservedByOtherDrafts(pid);
            decimal netAvailable = Math.Max(0m, baseAvailable - reservedByDrafts);

            decimal consumedByOtherRows = _lines
                .Where(l => !ReferenceEquals(l, line) && l.ProductId == pid)
                .Sum(l => l.Quantity);

            return Math.Max(0m, netAvailable - consumedByOtherRows);
        }

        private decimal GetReservedByOtherDrafts(Guid productId)
        {
            AtelierTransferProductDto? info = GetTransferInfo(productId);
            if (info is null)
            {
                return 0m;
            }

            decimal drafted = info.DraftQuantity;

            if (_editing is { Status: CostSlipStatus.Draft })
            {
                decimal ownQuantity = _editing.CostSlipItems
                    .Where(i => i.ProductId == productId)
                    .Sum(i => i.Quantity);

                drafted = Math.Max(0m, drafted - ownQuantity);
            }

            return drafted;
        }

        private AtelierTransferProductDto? GetTransferInfo(Guid productId)
        {
            if (SelectedWorkshopId is not Guid workshopId
                || !_transferredByWorkshop.TryGetValue(workshopId, out List<AtelierTransferProductDto>? transferred))
            {
                return null;
            }

            return transferred.FirstOrDefault(t => t.ProductId == productId);
        }

        private static bool MatchesWarehouse(ProductDto product, string warehouseCode)
            => product.WarehouseCode.Equals(warehouseCode, StringComparison.OrdinalIgnoreCase)
               || product.WarehouseCode.StartsWith(warehouseCode + ".", StringComparison.OrdinalIgnoreCase);

        private void RebuildAccountPanel()
        {
            flpAccounts.Controls.Clear();
            _accountInputs.Clear();
            _accountInputList.Clear();

            CostSlipType type = CurrentType;

            foreach (ExpenseAccountType account in ExpenseAccountHelper.GetFilteredAccounts(type))
            {
                _accountAmounts.TryAdd(account, 0m);

                bool isDerived = account == DerivedAccount;
                string caption = AccountDisplayName(account) + (isDerived ? "  (Grid Toplamı)" : string.Empty);

                int rowWidth = Math.Max(430, (flpAccounts.ClientSize.Width - 16) / 2);
                int labelWidth = (int)(rowWidth * 0.66);

                Panel row = new()
                {
                    Width = rowWidth,
                    Height = 26,
                    Margin = new Padding(0, 1, 0, 1)
                };

                Label lbl = new()
                {
                    Text = caption,
                    Location = new Point(0, 3),
                    Size = new Size(labelWidth, 20),
                    AutoSize = false,
                    AutoEllipsis = true,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 8.25F)
                };

                TextEdit input = new()
                {
                    Location = new Point(labelWidth + 2, 1),
                    Size = new Size(rowWidth - labelWidth - 4, 24),
                    ReadOnly = isDerived
                };
                input.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
                input.Properties.Mask.EditMask = "n2";
                input.Properties.Mask.UseMaskAsDisplayFormat = true;
                input.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                input.Properties.DisplayFormat.FormatString = "n2";
                input.Properties.NullValuePrompt = "0,00";
                input.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                input.EditValue = 0m;

                row.Controls.Add(lbl);
                row.Controls.Add(input);
                flpAccounts.Controls.Add(row);

                _accountInputs[account] = input;
                _accountInputList.Add(input);

                ExpenseAccountType captured = account;
                input.EditValueChanged += (_, _) =>
                {
                    if (_syncingTotals)
                    {
                        return;
                    }

                    _accountAmounts[captured] = ReadAmount(input);
                    RecalculateGrandTotal();
                };
            }
        }

        private void SyncAccountAmountsToInputs()
        {
            if (_editing is null)
            {
                return;
            }

            _syncingTotals = true;
            try
            {
                foreach ((ExpenseAccountType account, TextEdit input) in _accountInputs)
                {
                    input.EditValue = _accountAmounts.GetValueOrDefault(account);
                }
            }
            finally
            {
                _syncingTotals = false;
            }
        }

        private static decimal ReadAmount(TextEdit input)
        {
            if (input.EditValue is decimal d)
            {
                return d;
            }

            if (decimal.TryParse(
                input.EditValue?.ToString(),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal parsed))
            {
                return parsed;
            }

            return 0m;
        }

        private static string AccountDisplayName(ExpenseAccountType account)
            => Cost.Accounting.Automation.Application.Helpers.EnumDisplay.GetDisplayName(account);

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

                        AtelierTransferProductDto? transferInfo = GetTransferInfo(productId);
                        decimal newPrice = transferInfo?.UnitPrice ?? 0m;

                        CostSlipItemEditDto? duplicate = _lines.FirstOrDefault(l =>
                            !ReferenceEquals(l, line)
                            && l.ProductId == productId
                            && l.UnitPrice == newPrice);

                        if (duplicate is not null)
                        {
                            ToastHelper.Show($"'{prod.Name}' bu pusulada {newPrice:n2} birim fiyatıyla zaten mevcut.", ToastType.Warning);
                            edit.EditValue = line.ProductId ?? (Guid?)null;
                            return;
                        }

                        line.ProductId = prod.Id;
                        line.ProductName = prod.Name;
                        line.ProductUnitTypeId = prod.ProductUnitTypeId;
                        line.ProductUnitTypeName = prod.ProductUnitTypeName;

                        line.TransferredQuantity = transferInfo?.TransferredQuantity ?? 0m;
                        line.UnitPrice = newPrice;

                        RefreshAvailableQuantities();
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

            if (columnName == nameof(CostSlipItemEditDto.UnitPrice) && e.Value is decimal newUnitPrice)
            {
                int rowHandle = gridLinesView.FocusedRowHandle;
                if (rowHandle >= 0 && rowHandle < _lines.Count)
                {
                    CostSlipItemEditDto line = _lines[rowHandle];
                    if (line.ProductId is Guid pid)
                    {
                        bool priceConflict = _lines.Any(l =>
                            !ReferenceEquals(l, line)
                            && l.ProductId == pid
                            && l.UnitPrice == newUnitPrice);

                        if (priceConflict)
                        {
                            e.Valid = false;
                            e.ErrorText = "Bu ürün aynı birim fiyatıyla zaten listede mevcut. Farklı fiyatla giriş yapabilirsiniz.";
                            return;
                        }
                    }
                }
            }

            if (columnName == nameof(CostSlipItemEditDto.Quantity) && e.Value is decimal qty)
            {
                if (qty <= 0)
                {
                    e.Valid = false;
                    e.ErrorText = "Miktar sıfırdan büyük olmalıdır.";
                    return;
                }

                int rowHandle = gridLinesView.FocusedRowHandle;
                if (rowHandle >= 0 && rowHandle < _lines.Count)
                {
                    CostSlipItemEditDto line = _lines[rowHandle];
                    if (line.ProductId is Guid pid && GetTransferInfo(pid) is not null)
                    {
                        decimal displayed = GetDisplayedAvailable(line);
                        if (qty > displayed)
                        {
                            e.Valid = false;
                            e.ErrorText = $"Yetersiz stok. Bu üründen atölyede kalan: {displayed:n2}.";
                        }
                    }
                }
            }
        }

        private void GridLinesView_CellValueChanged(object? sender, CellValueChangedEventArgs e)
        {
            RefreshAvailableQuantities();
            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            foreach (var line in _lines)
            {
                line.TotalAmount = Math.Round(line.Quantity * line.UnitPrice, 2);
            }

            decimal gridTotal = _lines.Sum(l => l.TotalAmount);

            if (_accountInputs.TryGetValue(DerivedAccount, out TextEdit? derivedInput))
            {
                _accountAmounts[DerivedAccount] = gridTotal;

                _syncingTotals = true;
                try
                {
                    derivedInput.EditValue = gridTotal;
                }
                finally
                {
                    _syncingTotals = false;
                }
            }

            RecalculateGrandTotal();
            gridLinesView.RefreshData();
        }

        private void RecalculateGrandTotal()
        {
            int qty = ParseQuantity();
            decimal total = Math.Round(_accountAmounts.Values.Sum(), 2);

            decimal unitCost = 0m;
            decimal reconciled = total;
            decimal diff = 0m;
            ExpenseAccountType? target = null;

            if (qty > 0 && total > 0)
            {
                // Birim maliyet virgülden sonra iki rakam olacak şekilde hep YUKARI yuvarlanır
                // (en yakına yuvarlama aşağı değer üretip amortisman alanına eksi giriş ekleyebiliyor).
                unitCost = Math.Ceiling(total / qty * 100m) / 100m;
                reconciled = Math.Round(unitCost * qty, 2);
                diff = Math.Round(reconciled - total, 2);
                if (diff < 0m)
                {
                    diff = 0m;
                }

                target = CurrentType == CostSlipType.Service
                    ? ExpenseAccountType.Account740_7
                    : ExpenseAccountType.Account730_07;
            }

            _unitCost = unitCost;
            _roundingDiff = diff;
            _roundingTargetAccount = target;

            if (target is not null && _accountInputs.TryGetValue(target.Value, out TextEdit? targetInput))
            {
                decimal displayed = Math.Round(_accountAmounts.GetValueOrDefault(target.Value) + diff, 2);
                if (displayed < 0m)
                {
                    displayed = 0m;
                }

                _syncingTotals = true;
                try
                {
                    targetInput.EditValue = displayed;
                }
                finally
                {
                    _syncingTotals = false;
                }
            }

            _grandTotal = reconciled;
            gridLinesView.RefreshData();
        }

        private int ParseQuantity()
        {
            if (int.TryParse(txtQuantity.Text.Trim(), out int qty) && qty > 0)
            {
                return qty;
            }

            return 0;
        }

        private void ApplyRoundingAdjustmentToAccounts()
        {
            if (_roundingTargetAccount is not ExpenseAccountType target
                || _roundingDiff == 0m
                || !_accountAmounts.ContainsKey(target))
            {
                return;
            }

            _accountAmounts[target] = Math.Round(_accountAmounts[target] + _roundingDiff, 2);
        }

        private void GridLinesView_CustomDrawFooterCell(object? sender, FooterCellCustomDrawEventArgs e)
        {
            if (e.Column?.FieldName == nameof(CostSlipItemEditDto.TotalAmount))
            {
                e.Info.DisplayText = _grandTotal.ToString("n2") + " ₺";
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                return;
            }

            if (e.Column?.FieldName == nameof(CostSlipItemEditDto.Description))
            {
                GridColumn? rightTotalColumn = gridLinesView.Columns[nameof(CostSlipItemEditDto.TotalAmount)];
                if (rightTotalColumn is null)
                {
                    return;
                }

                int rightSpanWidth = gridLinesView.Columns
                    .Cast<GridColumn>()
                    .Where(c => c.Visible && c.VisibleIndex >= 0 && c.VisibleIndex > rightTotalColumn.VisibleIndex)
                    .Sum(c => c.VisibleWidth);

                Rectangle rightSpan = new(e.Bounds.Right - rightSpanWidth, e.Bounds.Top, rightSpanWidth, e.Bounds.Height);
                Rectangle rightTextRect = new(rightSpan.X + 12, rightSpan.Y, rightSpan.Width - 12, rightSpan.Height);

                e.Appearance.FillRectangle(e.Cache, rightSpan);
                using (StringFormat format = new()
                {
                    Alignment = StringAlignment.Far,
                    LineAlignment = StringAlignment.Center
                })
                {
                    e.Appearance.DrawString(e.Cache, $"Birim Maliyet: {_unitCost:n2} ₺", rightTextRect, format);
                }

                e.Handled = true;
                return;
            }

            if (e.Column?.FieldName != nameof(CostSlipItemEditDto.UnitPrice))
            {
                return;
            }

            GridColumn? totalColumn = gridLinesView.Columns[nameof(CostSlipItemEditDto.TotalAmount)];
            if (totalColumn is null)
            {
                return;
            }

            int spanWidth = gridLinesView.Columns
                .Cast<GridColumn>()
                .Where(c => c.Visible && c.VisibleIndex >= 0 && c.VisibleIndex < totalColumn.VisibleIndex)
                .Sum(c => c.VisibleWidth);

            Rectangle span = new(e.Bounds.Right - spanWidth, e.Bounds.Top, spanWidth, e.Bounds.Height);
            Rectangle textRect = new(span.X + 12, span.Y, span.Width - 12, span.Height);

            e.Appearance.FillRectangle(e.Cache, span);
            using (StringFormat format = new()
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            })
            {
                e.Appearance.DrawString(e.Cache, "Maliyet Genel Toplamı", textRect, format);
            }

            e.Handled = true;
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

            var materialLines = _lines.Where(l => l.Quantity > 0).ToList();
            if (materialLines.Any(l => l.UnitPrice < 0))
            {
                ToastHelper.Show("Birim fiyat negatif olamaz.", ToastType.Warning);
                return;
            }

            CostSlipItemEditDto? missingProductLine = materialLines.FirstOrDefault(l => l.ProductId is null);
            if (missingProductLine is not null)
            {
                ToastHelper.Show("Miktarı sıfırdan büyük her satır için bir ürün seçilmelidir.", ToastType.Warning);
                int row = _lines.IndexOf(missingProductLine);
                if (row >= 0)
                {
                    gridLinesView.FocusedRowHandle = row;
                }
                return;
            }

            if (SelectedWorkshopId is Guid saveWorkshopId
                && _transferredByWorkshop.TryGetValue(saveWorkshopId, out List<AtelierTransferProductDto>? saveTransferred))
            {
                foreach (var productGroup in materialLines
                    .Where(l => l.ProductId is not null
                        && l.ProductId != _semiFinishedProductId)
                    .GroupBy(l => l.ProductId!.Value))
                {
                    decimal totalQty = productGroup.Sum(l => l.Quantity);
                    decimal available = saveTransferred
                        .FirstOrDefault(t => t.ProductId == productGroup.Key)?.AvailableQuantity ?? 0m;
                    available = Math.Max(0m, available - GetReservedByOtherDrafts(productGroup.Key));

                    if (totalQty > available)
                    {
                        string name = productGroup.First().ProductName;
                        ToastHelper.Show($"'{name}' için yetersiz stok. Atölyede kalan: {available:n2}.", ToastType.Warning);
                        return;
                    }
                }
            }

            if (!await ConfirmSemiFinishedBalanceAsync())
            {
                return;
            }

            ExpenseAccountType derivedAccount = DerivedAccount;
            RecalculateTotals();
            ApplyRoundingAdjustmentToAccounts();

            List<CostSlipItemModel> itemModels = materialLines.Select(l => new CostSlipItemModel(
                l.ProductId,
                l.ProductUnitTypeId,
                derivedAccount,
                l.Quantity,
                l.UnitPrice,
                l.Description)).ToList();

            foreach ((ExpenseAccountType account, decimal amount) in _accountAmounts)
            {
                if (account == derivedAccount || amount <= 0)
                {
                    continue;
                }

                itemModels.Add(new CostSlipItemModel(null, null, account, 1m, amount, null));
            }

            if (itemModels.Count == 0 || itemModels.Sum(i => i.Quantity * i.UnitPrice) <= 0)
            {
                ToastHelper.Show("Pusula genel toplamı sıfırdan büyük olmalıdır.", ToastType.Warning);
                return;
            }

            DateOnly date = DateOnly.FromDateTime(dtCostDate.DateTime);

            Guid? producedProductId = lookUpProducedProduct.EditValue is Guid pid && pid != Guid.Empty ? pid : null;

            bool ok = false;
            btnSave.Enabled = false;
            btnSaveDraft.Enabled = false;
            try
            {
                if (_editing is { Status: CostSlipStatus.Draft })
                {
                    ok = await CrudExecutor.ExecuteAsync(new CostSlipUpdateCommand(
                        _editing.Id,
                        number,
                        CurrentType,
                        date,
                        workshopId,
                        producedProductId,
                        null,
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
                        CustomerId: null,
                        Quantity: quantity,
                        Description: txtDescription.Text.Trim(),
                        Items: itemModels,
                        IsApproved: approve));
                }
            }
            finally
            {
                btnSave.Enabled = !ok;
                btnSaveDraft.Enabled = !ok;
            }

            if (!ok)
            {
                return;
            }

            bool isEditingDraft = _editing is { Status: CostSlipStatus.Draft };
            bool becomesApproved = approve && !isEditingDraft;

            if (!becomesApproved && _editing is null)
            {
                _editing = await FindCreatedDraftAsync(number, workshopId, CurrentType, date);
            }

string message = isEditingDraft
                 ? "Maliyet pusulası taslağı güncellendi."
                 : approve
                     ? "Maliyet pusulası onaylandı; stok hareketleri oluşturuldu."
                     : "Maliyet pusulası taslak olarak kaydedildi. Onaylanınca stok hareketleri oluşturulacak.";
             ToastHelper.Show(message, ToastType.Success);

             if (becomesApproved && (CurrentType is CostSlipType.Product or CostSlipType.SemiFinishedProduct) && producedProductId.HasValue)
             {
                 await CheckRecipeAndSalePriceAsync(producedProductId.Value, materialLines, _grandTotal, quantity);
             }

             _saved = true;
             LockAfterSave(becomesApproved);
        }

        private async Task CheckRecipeAndSalePriceAsync(Guid producedProductId, List<CostSlipItemEditDto> materialLines, decimal grandTotal, int quantity)
    {
        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            RecipeCompareResult compare = await mediator.Send(new RecipeCompareQuery(
                producedProductId,
                quantity,
                grandTotal,
                materialLines.Select(l => new RecipeCompareMaterialRow(l.ProductId!.Value, l.ProductName!, l.Quantity)).ToList()),
                CancellationToken.None);

            if (!compare.HasRecipe)
            {
                DialogResult dr = MsgBox.Confirm(
                    "Bu üretim için reçete henüz tanımlı değil.\nMaliyeti reçete olarak kaydetmek istermisiniz?",
                    "Reçete Kaydı");
                if (dr == DialogResult.Yes)
                {
                    var items = materialLines
                        .Where(l => l.ProductId != null)
                        .Select(l => new RecipeItemRow(l.ProductId!.Value, Math.Round(l.Quantity / quantity, 4)))
                        .ToList();
                    await CrudExecutor.ExecuteAsync(new RecipeSaveCommand(producedProductId, true, items, SelectedWorkshopId ?? Guid.Empty));
                    ToastHelper.Show("Reçete olarak kaydedildi.", ToastType.Success);
                }
            }
            else
            {
                if (compare.Mismatches.Count > 0)
                {
                    System.Text.StringBuilder sb = new();
                    sb.AppendLine("Reçete ile malzeme kullanımı uyumsuz:");
                    foreach (RecipeCompareMismatchDto m in compare.Mismatches)
                    {
                        sb.AppendLine($"  {m.ProductName}: reçete {m.Expected:n4}, gerçek {m.Actual:n4} ({m.Reason})");
                    }
                    ToastHelper.Show(sb.ToString(), ToastType.Warning, 10000);
                }
                if (compare.SalePriceExceeded)
                {
                    ToastHelper.Show(
                        $"{compare.ProducedProductName} birim maliyeti ({compare.UnitCost:n2} ₺) satış fiyatını ({compare.SalePrice:n2} ₺) aşmaktadır.",
                        ToastType.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("CostSlip.RecipeCheck", ex);
        }
    }

    private async Task<CostSlipListDto?> FindCreatedDraftAsync(string number, Guid workshopId, CostSlipType type, DateOnly date)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                IQueryable<CostSlipListDto> query = await mediator.Send(
                    new CostSlipGetAllQuery(CostSlipType: type, OnlyDeleted: false, Status: CostSlipStatus.Draft),
                    CancellationToken.None);
                List<CostSlipListDto> drafts = await Task.Run(() => query.ToList());
                return drafts.FirstOrDefault(d =>
                    d.SlipNumber.Equals(number.Trim(), StringComparison.OrdinalIgnoreCase)
                    && d.WorkshopId == workshopId
                    && d.CostDate == date);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CostSlip.FindCreatedDraft", ex);
                return null;
            }
        }

        private async void BtnApprove_Click(object? sender, EventArgs e)
        {
            if (_editing is null)
            {
                ToastHelper.Show("Pusula onaylanamadı. Listeden yeniden açıp onaylayabilirsiniz.", ToastType.Warning);
                return;
            }

            btnApprove.Enabled = false;
            try
            {
                if (!await ConfirmSemiFinishedBalanceAsync())
                {
                    return;
                }

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

        private void LockAfterApproval() => LockAfterSave(approved: true);

        private void LockAfterSave(bool approved)
        {
            btnPrintSlip.Enabled = true;
            btnSave.Enabled = false;
            btnSaveDraft.Enabled = false;
            btnApprove.Enabled = !approved;
            btnApprove.Visible = !approved;
            btnAddLine.Enabled = false;
            btnDeleteLine.Enabled = false;
            cmbCostSlipType.ReadOnly = true;
            txtSlipNumber.ReadOnly = true;
            dtCostDate.ReadOnly = true;
            lookUpWorkshop.ReadOnly = true;
            lookUpProducedProduct.ReadOnly = true;
            txtQuantity.ReadOnly = true;
            txtDescription.ReadOnly = true;
            gridLinesView.OptionsBehavior.Editable = false;

            foreach (TextEdit input in _accountInputList)
            {
                input.ReadOnly = true;
            }

            lblStatusValue.Text = approved ? "Durum: Onaylı" : "Durum: Taslak";
            lblStatusValue.Appearance.ForeColor = approved ? SkinTheme.Success : SkinTheme.Warning;
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

                if (report is CostSlipProductReport productReport)
                {
                    productReport.SlipTypeTitle = CurrentType == CostSlipType.SemiFinishedProduct
                        ? "YARI MAMÜL MALİYET PUSULASI"
                        : "MAMÜL MALİYET PUSULASI";
                }
                else if (report is CostSlipServiceReport serviceReport)
                {
                    serviceReport.SlipTypeTitle = "HİZMET MALİYET PUSULASI";
                }

                report.RequestParameters = false;
                foreach (DevExpress.XtraReports.Parameters.Parameter parameter in report.Parameters)
                {
                    parameter.Visible = false;
                }

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
                        ExpenseAccountType = DerivedAccount
                    })
                    .ToList();

                report.DataSource = items;

                Dictionary<string, decimal> totals = _accountAmounts
                    .GroupBy(kv => GetReportAccountParam(kv.Key))
                    .ToDictionary(
                        g => g.Key,
                        g => Math.Round(g.Sum(kv => kv.Value), 2),
                        StringComparer.Ordinal);

                CompanyDto company = await LoadCompanyAsync();

                SetReportParam(report, "MamulAdi", lookUpProducedProduct.EditValue is Guid pid && pid != Guid.Empty
                    ? _products.FirstOrDefault(p => p.Id == pid)?.Name ?? string.Empty
                    : string.Empty);
                SetReportParam(report, "Miktari", int.TryParse(txtQuantity.Text.Trim(), out int qty) ? qty : 0);
                SetReportParam(report, "Tarih", DateOnly.FromDateTime(dtCostDate.DateTime));
                SetReportParam(report, "Donem", dtCostDate.DateTime.ToString(
                    "MM'. Ay - 'MMMM'-'yyyy",
                    System.Globalization.CultureInfo.GetCultureInfo("tr-TR")));
                SetReportParam(report, "Isyurdu", company.Name);

                string workshopName = lookUpWorkshop.EditValue is Guid wid && wid != Guid.Empty
                    ? _workshops.FirstOrDefault(w => w.Id == wid)?.Display ?? string.Empty
                    : _workshopName;
                SetReportParam(report, "Atolye", workshopName);
                SetReportParam(report, "Antet", company.Name);

                SetReportParam(report, "CiltNo", dtCostDate.DateTime.Year);
                SetReportParam(report, "SeriNo", txtSlipNumber.Text.Trim());
                SetReportParam(report, "SiparisNo", txtSlipNumber.Text.Trim());

                string[] moneyParams = ["M710", "M720", "M730", "M740", "M750", "M760", "M770", "M780"];
                foreach (string param in moneyParams)
                {
                    decimal value = totals.TryGetValue(param, out decimal sum) ? Math.Round(sum, 2) : 0;
                    SetReportParam(report, param, value);
                }

                decimal grandTotal = _accountAmounts.Values.Sum();
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