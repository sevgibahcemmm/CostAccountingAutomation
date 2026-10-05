using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Application.Recipes;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.CostSlips;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.Reports;
using Cost.Accounting.Automation.WinFormsApp.Forms.RecipeForms;
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
using TS.Result;
using ReportItemDto = Cost.Accounting.Automation.WinFormsApp.Reports.CostSlipReport.CostSlipItemDto;
using AppCostSlip = Cost.Accounting.Automation.Application.CostSlips.CostSlipDto;
using Cost.Accounting.Automation.WinFormsApp.Reports.CostSlipReport;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public partial class CostSlipEditForm : XtraForm
    {
        private CostSlipListDto? _editing;
        private readonly BindingList<CostSlipItemEditDto> _lines = [];
        private List<ProductCatalogDto> _products = [];
        // Hesap satırları bilerek sıkı: giriş kutusu 24px yüksekliğinde
        // olduğu için satır yüksekliği 28px yeterlidir. Daha önce 38px +
        // 2px kenar boşluğu kullanılıyordu ve dikey boşluk malzeme grid'ine
        // ayrılamıyordu. Kazanılan yükseklik pnlItemsPanel'e (grid) aktarılır.
        private const int AccountRowHeight = 28;
        private const int AccountsTitleHeight = 30;
        private const int AccountsRowGap = 2;
        private const int AccountsPanelGap = 4;
        private const int AccountsBottomMargin = 6;
        private const int MinItemsPanelHeight = 150;

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

        /// <summary>
        /// Kullanıcının "İstenen Birim Maliyet" kutusuna girdiği değer.
        /// Sıfır (veya boş kutu) otomatik yuvarlamanın kullanılacağı anlamına
        /// gelir. Hesap paneli yeniden kurulduğunda kutunun değeri bu alandan
        /// geri yüklenir, böylece pusula tipi değişince kaybolmaz.
        /// </summary>
        private decimal _desiredUnitCost;

        /// <summary>
        /// "İstenen Birim Maliyet" giriş kutusu. Hesap paneli
        /// <see cref="RebuildAccountPanel"/> tarafından kurulur; 730-07 gibi
        /// bir hesap KUTUSU DEĞİLDİR, bu yüzden <see cref="_accountInputs"/>
        /// sözlüğüne eklenmez. Yalnızca <see cref="_accountInputList"/>'e
        /// girer ki "İncele" modunda salt okunur olsun.
        /// </summary>
        private TextEdit _desiredUnitCostInput = default!;

        /// <summary>
        /// Girilen istenen birim maliyet, pusulanın mevcut toplam maliyetinden
        /// düşük olduğunda true olur. Bu durumda amortisman giderine yazılacak
        /// fark negatif olacağı için kayıt <see cref="SaveAsync"/> tarafından
        /// engellenir.
        /// </summary>
        private bool _desiredUnitCostInvalid;

        /// <summary>
        /// <see cref="_desiredUnitCostInput"/> için skin varsayılan renkleri.
        /// Geçersiz değerden sonra eski görünüme dönmek için saklanır.
        /// </summary>
        private Color _desiredUnitCostNormalBorder;

        private Color _desiredUnitCostNormalBack;

        private Color _desiredUnitCostNormalText;
        private List<ChartOfAccountLookUpDto> _workshops = [];
        private RepositoryItemSearchLookUpEdit _riProductLookUp = default!;
        private string _workshopName = string.Empty;
        private bool _saved;
        private string _autoDescription = string.Empty;
        private Guid? _semiFinishedProductId;

        /// <summary>
        /// Yarımamülün stoktaki MEVCUT miktar bakiyesi (bu pusulanın tükettiği
        /// miktar dahil değildir). "Kalan" sütunu bunu kullanır.
        /// </summary>
        private decimal _semiFinishedBalance;

        /// <summary>
        /// Yarımamülün tüketilmemiş giriş katmanları (FIFO sırasıyla). Kullanıcı
        /// 151 kutusuna tutar girdiğinde miktar bu katmanlardan türetilir.
        /// </summary>
        private List<CostingLayer> _semiFinishedLayers = [];

        /// <summary>
        /// Yarımamül katmanlarının sıralanacağı yöntem. Bakiye sorgusundaki
        /// yöntemle AYNI olmalıdır; aksi halde ekranda bulunan miktar onay
        /// anındaki FIFO kırılımını vermez.
        /// </summary>
        private StockCostingMethod _semiFinishedCostingMethod = StockCostingMethod.Fifo;

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
                    ? "Taslağı düzenleyip kaydedebilirsiniz; onayı listeden yaparsınız"
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

            // Onay işlemi bu ekranda yok: yeni kayıt formlarında onay verilmez.
            // Pusula daima TASLAK kaydedilir; onay CostSlipsListForm'daki
            // satır onayı ya da toplu onay ile yapılır.
            btnPrintSlip.Enabled = false;
            lblStatusValue.Text = "";

            if (_editing is not null)
            {
                bool isDraft = _editing.Status == CostSlipStatus.Draft;

                btnSaveDraft.Visible = isDraft;
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
                }
            }
        }

        /// <summary>
        /// Yarı mamul maliyet pusulasında miktar her zaman 1'dir ve
        /// kullanıcı tarafından değiştirilemez; mamul/hizmet pusulalarında
        /// miktar serbesttir.
        /// </summary>
        private void ApplySemiFinishedQuantityRule()
        {
            bool isSemiFinished = CurrentType == CostSlipType.SemiFinishedProduct;

            if (isSemiFinished && !Equals(txtQuantity.EditValue, 1))
            {
                txtQuantity.EditValue = 1;
            }

            txtQuantity.ReadOnly = isSemiFinished
                || (_editing is not null && _editing.Status != CostSlipStatus.Draft);
        }

        /// <summary>
        /// Hesap girişlerini salt okunur yapar. Panel her yeniden kurulduğunda
        /// yeniden uygulanır; aksi halde "İncele" modunda bir gider kutusu
        /// düzenlenebilir kalırdı.
        /// </summary>
        private void ApplyAccountInputReadOnly()
        {
            bool readOnly = _editing is not null && _editing.Status != CostSlipStatus.Draft;

            foreach (TextEdit input in _accountInputList)
            {
                input.ReadOnly = readOnly;
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
            GridColumnFactory.RegisterManualNumericColumns(gridLinesView);
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
            cmbCostSlipType.SelectedIndexChanged += CmbCostSlipType_SelectedIndexChanged;
            lookUpWorkshop.EditValueChanged += LookUpWorkshop_EditValueChanged;
            btnAddLine.Click += (_, _) => AddEmptyLine();
            btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
            btnSaveDraft.Click += BtnSaveDraft_Click;
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

        /// <summary>
        /// Yapısal kurulum burada biter: hesap satırları kurulur, yerleşimi
        /// hesaplanır ve formun boyutu belli olur. Böylece form ilk çiziminde
        /// eksiksiz görünür; hesap kutuları veriden önce ekranda belirip formu
        /// yarım bırakmaz.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RebuildAccountPanel();
        }

        /// <summary>
        /// Sorgular form ilk kez çizildikten SONRA başlar. Böylece veri
        /// yüklemesi hiçbir zaman yapısal bir değişikliğin ortasında görünmez.
        /// </summary>
        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            try
            {
                await LoadFormDataAsync();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CostSlipEditForm.Load", ex);
                ToastHelper.Show("Pusula verileri yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private async Task LoadFormDataAsync()
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

            // Hesap satırları OnLoad'da kuruldu ve slip tipi değiştiğinde
            // CmbCostSlipType_SelectedIndexChanged tarafından yeniden kuruluyor.
            SyncAccountAmountsToInputs();
            RecalculateTotals();
            UpdateProducedProductAvailability();
            await LoadMaterialProductsAsync();

            // Kaydedilmiş bir pusula açıldığında uyarı gösterilmez ama yarımamül
            // katmanları yine yüklenir; aksi halde 151 kutusundaki tutar
            // FIFO kırılımı olmadan miktara çevrilemez.
            await LoadSemiFinishedLayersAsync();

            if (_editing is null)
            {
                await AutoAssignNumberAsync();
            }
        }

        /// <summary>
        /// Yarımamülün tüketilmemiş giriş katmanlarını bakiye sorgusundan alır.
        /// Uyarı penceresi göstermez; yalnızca katman listesini hazırlar.
        /// </summary>
        private async Task LoadSemiFinishedLayersAsync()
        {
            Guid? semiId = ActiveSemiFinishedProductId;
            if (semiId is null)
            {
                return;
            }

            if (SelectedWorkshopId is not Guid workshopId)
            {
                return;
            }

            CostSlipSemiFinishedBalanceDto? balance =
                await GetSemiFinishedBalanceAsync(workshopId, semiId.Value);

            if (balance is null)
            {
                return;
            }

            _semiFinishedLayers = balance.ToCostingLayers();

            // "Kalan" sütunu bu pusulanın tükettiği miktarı kendisi düşsün diye
            // mevcut bakiye tutulur; tüketilen miktar GetDisplayedAvailable
            // içinde ayrıca hesaplanır.
            _semiFinishedBalance = balance.Balance
                + _lines.Where(l => l.ProductId == semiId).Sum(l => l.Quantity);
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
                Task<List<ProductCatalogDto>> productsTask = LoadProductsAsync();
                Task<List<ChartOfAccountLookUpDto>> accountsTask = LoadAccountLookUpsAsync();

                List<ChartOfAccountLookUpDto> accountLookUps = await accountsTask;

                _workshops = accountLookUps
                    .Where(w => w.Type == ChartOfAccountType.Workshop)
                    .ToList();

                _workshopLinks = _workshops.ToDictionary(
                    w => w.Id,
                    w => new WorkshopLink(w.Code, w.SemiFinishedAccountId, w.FinishedAccountId));

                // Atölye kutusu, ürün sorgusu beklenmeden bağlanır; aksi halde combo
                // ürünler yüklenene dek boş kalıp geç doluyordu.
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

                _products = await productsTask;

                lookUpProducedProduct.Properties.DataSource = _products;
                lookUpProducedProduct.Properties.ValueMember = nameof(ProductCatalogDto.Id);
                lookUpProducedProduct.Properties.DisplayMember = nameof(ProductCatalogDto.Name);
                lookUpProducedProduct.Properties.BestFitMode = BestFitMode.BestFit;
                lookUpProducedProductView.Columns.Clear();
                lookUpProducedProductView.OptionsBehavior.AutoPopulateColumns = false;
                GridColumn productColumn = lookUpProducedProductView.Columns.AddField(nameof(ProductCatalogDto.Name));
                productColumn.Caption = "Ürün Adı";
                productColumn.VisibleIndex = 0;
                productColumn.Width = 240;
                lookUpProducedProductView.Columns.AddField(nameof(ProductCatalogDto.ProductCode)).Caption = "Ürün Kodu";
                lookUpProducedProductView.Columns.AddField(nameof(ProductCatalogDto.ProductUnitTypeName)).Caption = "Birim";
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

        private static async Task<List<ProductCatalogDto>> LoadProductsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            return await mediator.Send(new ProductCatalogGetAllQuery(), CancellationToken.None);
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
            ApplySemiFinishedQuantityRule();
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
                    Description = item.Description ?? string.Empty
                });
            }

            ProductCatalogDto? produced = _products.FirstOrDefault(p => p.Id == slip.ProducedProductId);
            if (produced?.SemiFinishedProductId is Guid semiId)
            {
                // Yalnızca kimlik burada set edilir. Mevcut bakiye ve katman
                // listesi LoadFormDataAsync sonunda stok hareketlerinden yüklenir;
                // kayıttan türetmek taslakları da sayar ve stokla uyuşmazdı.
                _semiFinishedProductId = semiId;
            }

            MigrateLegacySemiFinishedAccount();
            RestoreDesiredUnitCost(slip);
            RecalculateTotals();
        }

        /// <summary>
        /// Kaydedilmiş pusulanın birim maliyetini "İstenen Birim Maliyet"
        /// kutusuna geri yükler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Amaç, kaydı açıp hiçbir değişiklik yapmadan kaydettiğinizde toplam
        /// maliyetin değişmemesidir. Kayıtlı toplam zaten farkı içerdiği için,
        /// kutu bu değerle doldurulursa yeni fark sıfır çıkar.
        /// </para>
        /// <para>
        /// Bölme tam temiz denk gelmiyorsa (eski veri, yuvarlanmamış toplam)
        /// kutu bilerek BOŞ bırakılır: doldurulursa otomatik yuvarlama devreye
        /// girer ve açılışta küçük bir fark daha eklenirdi.
        /// </para>
        /// </remarks>
        private void RestoreDesiredUnitCost(AppCostSlip slip)
        {
            _desiredUnitCost = 0m;

            if (slip.Quantity > 0 && slip.GrandTotal > 0m)
            {
                decimal candidate = Math.Ceiling(slip.GrandTotal / slip.Quantity * 100m) / 100m;
                if (Math.Abs(Math.Round(candidate * slip.Quantity, 2) - slip.GrandTotal) <= 0.005m)
                {
                    _desiredUnitCost = candidate;
                }
            }

            SyncDesiredUnitCostToInput();
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
                if (rowHandle < 0 || rowHandle >= _lines.Count)
                {
                    return;
                }

                if (MsgBox.ConfirmRowDelete(1, "maliyet pusulası") != DialogResult.Yes)
                {
                    return;
                }

                bool removedSemi = _lines[rowHandle].ProductId == _semiFinishedProductId;
                _lines.RemoveAt(rowHandle);
                if (removedSemi)
                {
                    _semiFinishedProductId = null;

                    // Bakiye sıfırlanmaz; yalnızca katmanlar temizlenir. "Kalan"
                    // sütunu mevcut stok miktarını göstermelidir, bu
                    // pusulanın tükettiği miktarı değil.
                    _semiFinishedLayers = [];
                    _semiFinishedBalance = 0m;
                }

                RefreshAvailableQuantities();
                RecalculateTotals();
            }

        /// <summary>
        /// Yarımamül tüketimi eskiden 710 kalemi olarak kaydediliyordu, artık
        /// 151'e yazılıyor. Eski kayıtlar açıldığında satır 151'e taşınır ve
        /// tutarı "Yarımamülden gelen" kutusuna yansıtılır. Taşınmazsa satır
        /// 710 grid toplamından düşüldüğü için o maliyet toplamdan kaybolur.
        /// </summary>
        private void MigrateLegacySemiFinishedAccount()
        {
            if (ActiveSemiFinishedProductId is null)
            {
                return;
            }

            CostSlipItemEditDto? semiLine = _lines.FirstOrDefault(IsSemiFinishedLine);
            if (semiLine is null)
            {
                return;
            }

            semiLine.ExpenseAccountType = ExpenseAccountHelper.SemiFinishedAccount;
            _accountAmounts[ExpenseAccountHelper.SemiFinishedAccount] =
                Math.Round(semiLine.Quantity * semiLine.UnitPrice, 2);
        }

        private CostSlipType CurrentType => cmbCostSlipType.SelectedIndex switch
        {
            1 => CostSlipType.Service,
            2 => CostSlipType.SemiFinishedProduct,
            _ => CostSlipType.Product
        };

        /// <summary>
        /// Reçete kontrolü yalnızca MAMUL pusulasında yapılır. Yarı mamul
        /// üretiminde reçete tutulmadığı için var/yok kontrolü, malzeme
        /// uyumsuzluk uyarısı ve reçeteye göre satış fiyatı uyarısı çalıştırılmaz.
        /// </summary>
        private bool RequiresRecipeCheck => CurrentType == CostSlipType.Product;

        private Guid? SelectedWorkshopId
            => lookUpWorkshop.EditValue is Guid id && id != Guid.Empty ? id : null;

        private ExpenseAccountType DerivedAccount => CurrentType == CostSlipType.Service
            ? ExpenseAccountType.Account740_1
            : ExpenseAccountType.Account710;

        private async void CmbCostSlipType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplySemiFinishedQuantityRule();
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
                ApplySemiFinishedAmounts(semiProductId, balanceDto);
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
                ApplySemiFinishedAmounts(semiProductId, balanceDto);
            }

            RefreshAvailableQuantities();

            return true;
        }

        /// <summary>
        /// Yarımamülü tek satır olarak malzeme listesine (grid) ekler; böylece
        /// stoğu düşer (151 çıkış), 151'den gelen değer mamülün maliyetine
        /// geçer ve yarımamül bakiyesi sıfırlanır.
        ///
        /// Yarımamülün 720/730 gider kırılımı GİDER olarak yazılmaz: o giderler
        /// zaten yarımamül pusulasında giderleştirildi, tekrar yazılırsa aynı
        /// maliyet iki defa gider olur. Kullanılan 151 tutarları tek satırda
        /// toplandığı için hesap alanlarında asılı kalmaz (sıfırlanır); kırılım
        /// satırın açıklamasında görünür.
        /// </summary>
        private void ApplySemiFinishedAmounts(Guid productId, CostSlipSemiFinishedBalanceDto balance)
        {
            ProductCatalogDto? prod = _products.FirstOrDefault(p => p.Id == productId);

            _semiFinishedProductId = productId;

            // Katmanlar bakiyenin tek doğruluk kaynağıdır. Kullanıcı tutar
            // girdiğinde miktar bu katmanlardan türetilir; ortalama birim
            // maliyet yalnızca tam bakiye için tek satırlı gösterimde kullanılır.
            _semiFinishedLayers = balance.ToCostingLayers();

            // Miktar da atanmalıydı: hem "Kalan" sütunu hem de ürün seçim
            // listesi bu alandan okuyor. Atanmadığında bakiye 0 görünüyor ve
            // kullanıcı yarımamülün stokta olmadığını sanıyordu.
            _semiFinishedBalance = balance.Balance;

            decimal availableAmount = balance.AvailableAmount;

            IReadOnlyList<CostSlipExpenseBreakdownDto> parts = balance.ExpenseBreakdown.Count > 0
                ? balance.ExpenseBreakdown
                : [new CostSlipExpenseBreakdownDto(ExpenseAccountType.Account710, balance.UnitPrice)];

            string breakdown = string.Join(
                " / ",
                parts
                    .Select(p => $"{ShortAccountName(p.AccountType)}: {Math.Round(balance.Balance * p.UnitPrice, 2):n2}")
                    .Where(s => !s.EndsWith(": 0,00", StringComparison.Ordinal)));

            if (!_lines.Any(l => l.ProductId == productId && l.Quantity > 0))
            {
                // "Yarımamülden gelen" tutarı katmanların toplamıdır; miktar da
                // katmanlardan gelir. Böylece iki değer baştan tutarlıdır.
                _lines.Add(new CostSlipItemEditDto
                {
                    ProductId = productId,
                    ProductName = prod?.Name ?? string.Empty,
                    ProductUnitTypeId = prod?.ProductUnitTypeId,
                    ProductUnitTypeName = prod?.ProductUnitTypeName ?? string.Empty,
                    ExpenseAccountType = ExpenseAccountHelper.SemiFinishedAccount,
                    Quantity = balance.Balance,
                    UnitPrice = balance.Balance > 0m
                        ? Math.Round(availableAmount / balance.Balance, 4)
                        : 0m,
                    Description = string.IsNullOrEmpty(breakdown)
                        ? "Yarımamül tüketimi"
                        : $"Yarımamül tüketimi ({breakdown})"
                });
            }

            // "Yarımamülden gelen" kutusu mevcut katman tutarıyla doldurulur;
            // kullanıcı kısmi tüketim için düşürebilir.
            SetSemiFinishedAccountAmount(availableAmount);

            UpdateMaterialProductDataSource();
            gridLinesView.RefreshData();
            RecalculateTotals();
        }

        private static string ShortAccountName(ExpenseAccountType account)
            => AccountDisplayName(account).Split('-')[0].Trim();

        private async Task<CostSlipSemiFinishedBalanceDto?> GetSemiFinishedBalanceAsync(Guid workshopId, Guid productId)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                // Katmanlar pusulanın tarihine kadar olan hareketlerden
                // hesaplanır; onayda kullanılacak kırılımın aynısı böylece
                // ekranda da görünür.
                var result = await mediator.Send(
                    new CostSlipSemiFinishedBalanceQuery(
                        workshopId,
                        productId,
                        CostDate: DateOnly.FromDateTime(dtCostDate.DateTime),
                        CostingMethod: _semiFinishedCostingMethod),
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

            List<ProductCatalogDto> filtered = isService
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

            List<ProductLookUpItem> lookupItems = [];

            if (workshopId is Guid wid
                && _transferredByWorkshop.TryGetValue(wid, out List<AtelierTransferProductDto>? transferred))
            {
                // Liste doğrudan transfer kayıtlarından kurulur. Atölyeye transfer
                // edilmemiş bir ürünün bakiyesi bilinmediği için listelenmez; daha
                // önce 150 deposunun tamamı listeleniyor ve bakiyeleri de transfer
                // kaydı bulunamadığı için 0 görünüyordu.
                lookupItems = transferred
                    .Select(t => new ProductLookUpItem(
                        t.ProductId,
                        t.ProductName,
                        t.ProductCode,
                        t.UnitTypeName,
                        t.AvailableQuantity,
                        t.DraftQuantity))
                    .OrderByDescending(x => x.Balance)
                    .ThenBy(x => x.Code)
                    .ThenBy(x => x.Name)
                    .ToList();
            }

            if (_semiFinishedProductId is Guid semiId)
            {
                ProductCatalogDto? semi = _products.FirstOrDefault(p => p.Id == semiId);
                if (semi is not null && !lookupItems.Any(x => x.Id == semiId))
                {
                    lookupItems.Insert(0, new ProductLookUpItem(
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
                : !_transferredByWorkshop.ContainsKey(workshopId.Value)
                    ? "Atölyeye transfer edilen ürünler yükleniyor..."
                    : lookupItems.Count == 0
                        ? "Bu atölyeye henüz transfer yapılmamış"
                        : "Ürün / Masraf Seçiniz...";

            foreach (var line in _lines)
            {
                if (line.ProductId is Guid pid
                    && pid != _semiFinishedProductId
                    && !lookupItems.Any(x => x.Id == pid))
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

        private static bool MatchesWarehouse(ProductCatalogDto product, string warehouseCode)
            => product.WarehouseCode.Equals(warehouseCode, StringComparison.OrdinalIgnoreCase)
               || product.WarehouseCode.StartsWith(warehouseCode + ".", StringComparison.OrdinalIgnoreCase);

        private void RebuildAccountPanel()
        {
            // Satırlar silinip yeniden kurulduğu için her ekleme ayrı bir
            // yerleşim ve boyama tetikliyordu; akış tek geçişte toplanır.
            flpAccounts.SuspendLayout();
            try
            {
                flpAccounts.Controls.Clear();
                _accountInputs.Clear();
                _accountInputList.Clear();
                flpAccounts.AutoSize = false;
                flpAccounts.WrapContents = true;
                flpAccounts.AutoScroll = false;

                CostSlipType type = CurrentType;

                int rowWidth = CalculateAccountRowWidth();
                int labelWidth = (int)(rowWidth * 0.66);

                foreach (ExpenseAccountType account in ExpenseAccountHelper.GetFilteredAccounts(type))
                {
                    AddAccountRow(account, rowWidth, labelWidth);
                }

                // "Yarımamülden gelen" (151) satırı GetFilteredAccounts içinde
                // daima son kalem olarak eklenir; bu satır da hemen ardından
                // geldiği için kutuyu 151'in yanına denk gelir.
                AddDesiredUnitCostRow(rowWidth, labelWidth);
            }
            finally
            {
                flpAccounts.ResumeLayout(true);

                // Yerleşim, panelin gerçek genişliği oturduktan sonra hesaplanır;
                // askı sürerken çağrılırsa bayat ClientSize kullanılır.
                LayoutAccountsPanel();

                // Yeniden kurulan kutuların salt okunurluğu sıfırlanır; "İncele"
                // modunda panel yeniden kurulursa kutu yine kilitlenir.
                ApplyAccountInputReadOnly();
            }
        }

        /// <summary>
        /// Hesap alanına bir gider satırı (başlık + tutar kutusu) ekler ve
        /// tutar kutusunu hesap toplamına bağlar.
        /// </summary>
        private void AddAccountRow(ExpenseAccountType account, int rowWidth, int labelWidth)
        {
            _accountAmounts.TryAdd(account, 0m);

            bool isDerived = account == DerivedAccount;
            string caption = AccountDisplayName(account) + (isDerived ? "  (Grid Toplamı)" : string.Empty);

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

            Panel row = new()
            {
                Width = rowWidth,
                Height = AccountRowHeight,
                Margin = new Padding(0, 1, 0, 1),
                Controls = { lbl, input }
            };

            flpAccounts.Controls.Add(row);

            _accountInputs[account] = input;
            _accountInputList.Add(input);

            // account her çağrıda ayrı bir parametre olduğu için döngü değişkeni
            // yakalanmış olmaz.
            input.EditValueChanged += (_, _) =>
            {
                if (_syncingTotals)
                {
                    return;
                }

                _accountAmounts[account] = ReadAmount(input);

                if (account == ExpenseAccountHelper.SemiFinishedAccount)
                {
                    SyncSemiFinishedQuantityFromAmount();
                }

                RecalculateGrandTotal();
            };
        }

        /// <summary>
        /// Hesap alanına "İstenen Birim Maliyet" satırı ekler.
        ///
        /// <para>
        /// Bu kutu bir <b>gider hesabı değildir</b>: tutarı hesap toplamına
        /// yazılmaz, yalnızca birim maliyeti belirlemek için okunur. Farkı
        /// <see cref="RecalculateGrandTotal"/> hesaplar ve amortisman gideri
        /// kutusuna ekler.
        /// </para>
        /// <para>
        /// Kutu boş bırakılırsa mevcut otomatik davranış korunur: birim
        /// maliyet iki haneye yukarı yuvarlanır.
        /// </para>
        /// </summary>
        private void AddDesiredUnitCostRow(int rowWidth, int labelWidth)
        {
            Label lbl = new()
            {
                Text = "İstenen Birim Maliyet",
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
                Size = new Size(rowWidth - labelWidth - 4, 24)
            };
            input.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            input.Properties.Mask.EditMask = "n2";
            input.Properties.Mask.UseMaskAsDisplayFormat = true;
            input.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            input.Properties.DisplayFormat.FormatString = "n2";
            input.Properties.NullValuePrompt = "Otomatik";
            input.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;

            // Girilen değer geçerli olduğunda kutu normal görünür; toplam
            // maliyetten düşük bir değer girildiğinde kırmızıya döner.
            // Varsayılan renkler Color.Empty'dir; skin devreye girer. Geçersiz
            // durumdan dönüldüğünde Color.Empty'ye geri yüklenir.
            _desiredUnitCostNormalBorder = input.Properties.Appearance.BorderColor;
            _desiredUnitCostNormalBack = input.Properties.Appearance.BackColor;
            _desiredUnitCostNormalText = input.Properties.Appearance.ForeColor;

            Panel row = new()
            {
                Width = rowWidth,
                Height = AccountRowHeight,
                Margin = new Padding(0, 1, 0, 1),
                Controls = { lbl, input }
            };

            flpAccounts.Controls.Add(row);

            // Hesap sözlüklerine EKLENMEZ; yalnızca salt okunurluk listesine
            // girer, "İncele" modunda da kilitlenebilsin diye.
            _accountInputList.Add(input);
            _desiredUnitCostInput = input;

            SyncDesiredUnitCostToInput();

            input.EditValueChanged += (_, _) =>
            {
                if (_syncingTotals)
                {
                    return;
                }

                _desiredUnitCost = ReadAmount(input);
                RecalculateGrandTotal();
            };

            ToolTip tip = new();
            tip.SetToolTip(
                input,
                "Birim maliyeti kendiniz belirlemek için değer girin. Girilen tutarın "
                + "miktar ile çarpımı, toplam maliyetten farkı kadar amortisman giderine eklenir. "
                + "Boş bırakırsanız birim maliyet iki haneye yukarı yuvarlanır.");
        }

        /// <summary>
        /// <see cref="_desiredUnitCost"/> değerini kutuya yazar. Sıfır değer
        /// "belirtilmedi" demektir ve kutu boş (null) bırakılır.
        /// </summary>
        private void SyncDesiredUnitCostToInput()
        {
            if (_desiredUnitCostInput is null)
            {
                return;
            }

            _syncingTotals = true;
            try
            {
                _desiredUnitCostInput.EditValue = _desiredUnitCost > 0m ? _desiredUnitCost : null;
            }
            finally
            {
                _syncingTotals = false;
            }
        }

        /// <summary>
        /// Kutudan okunan istenen birim maliyet. Kutu boşsa veya sıfırsa 0 döner;
        /// bu durumda otomatik yuvarlama kullanılır.
        /// </summary>
        private decimal DesiredUnitCost
            => _desiredUnitCostInput is null ? 0m : ReadAmount(_desiredUnitCostInput);

        /// <summary>
        /// Hesap alanı içeriğe göre büyür ve gövdenin altına yaslanır; arada
        /// boşluk kalmaz. Tüm kalemler tek alanda görünür, kaydırma çubuğu
        /// gerekmez. Form yetmezse kendini büyütür.
        /// </summary>
        private void LayoutAccountsPanel()
        {
            int rowWidth = CalculateAccountRowWidth();
            if (rowWidth <= 0)
            {
                return;
            }

            int columns = Math.Max(1, flpAccounts.ClientSize.Width / rowWidth);

            // +1: "İstenen Birim Maliyet" satırı _accountInputs'ta yoktur ama
            // panelde yer kaplar; sayılmazsa son satır panelin dışında kalır.
            int rowCount = (int)Math.Ceiling((_accountInputs.Count + 1) / (double)columns);

            int accountsHeight = AccountsTitleHeight
                + (rowCount * (AccountRowHeight + AccountsRowGap))
                + 8;

            int top = pnlItemsPanel.Top;

            // Form yetmiyorsa büyütülür; böylece hiçbir kalem kırpılmaz.
            int requiredClient = pnlHeader.Height
                + top
                + MinItemsPanelHeight
                + AccountsPanelGap
                + accountsHeight
                + AccountsBottomMargin
                + pnlFooter.Height;

            if (requiredClient > ClientSize.Height)
            {
                ClientSize = new Size(ClientSize.Width, requiredClient);
            }

            int accountsTop = pnlBody.Height - accountsHeight - AccountsBottomMargin;
            if (accountsTop < top + MinItemsPanelHeight)
            {
                accountsTop = top + MinItemsPanelHeight;
            }

            // Bu yordam form açılışında birden çok kez çağrılır. Boyut ve konum
            // atamalarının her biri tek tek ekrana yansıyor ve form kademeli
            // büyüyerek titriyordu; tüm geçiş tek boyamada toplanır.
            pnlBody.SuspendLayout();
            flpAccounts.SuspendLayout();
            try
            {
                lblAccountsTitle.Visible = true;
                lblAccountsTitle.BringToFront();

                pnlAccounts.Top = accountsTop;
                pnlAccounts.Height = accountsHeight;

                pnlItemsPanel.Height = accountsTop - top - AccountsPanelGap;

                flpAccounts.Location = new Point(flpAccounts.Left, AccountsTitleHeight);
                flpAccounts.Size = new Size(
                    Math.Max(200, pnlAccounts.ClientSize.Width - 20),
                    accountsHeight - AccountsTitleHeight);
                flpAccounts.AutoScroll = false;
            }
            finally
            {
                flpAccounts.ResumeLayout(true);
                pnlBody.ResumeLayout(true);
            }

            pnlAccounts.PerformLayout();
            flpAccounts.PerformLayout();
        }

        private int CalculateAccountRowWidth()
        {
            int flowWidth = flpAccounts.ClientSize.Width;
            if (flowWidth <= 0)
            {
                flowWidth = pnlAccounts.ClientSize.Width - 20;
            }

            if (flowWidth <= 0)
            {
                return 0;
            }

            return Math.Max(430, (flowWidth - 16) / 2);
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
            => account == ExpenseAccountHelper.SemiFinishedAccount
                ? "Yarımamülden gelen"
                : Cost.Accounting.Automation.Application.Helpers.EnumDisplay.GetDisplayName(account);

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
                ProductCatalogDto? prod = _products.FirstOrDefault(p => p.Id == productId);
                if (prod is not null)
                {
                    int rowHandle = gridLinesView.FocusedRowHandle;
                    if (rowHandle >= 0 && rowHandle < _lines.Count)
                    {
                        CostSlipItemEditDto line = _lines[rowHandle];

                        AtelierTransferProductDto? transferInfo = GetTransferInfo(productId);
                        decimal newPrice = transferInfo?.UnitPrice ?? 0m;

                        // Yarımamül pusulaya zaten "Yarımamülden gelen" olarak
                        // dahil edildiyse ürün seçiminde tekrar sunulamaz.
                        // Genel mükerrer kontrolü bunu yakalayamaz: yarımamül
                        // satırının birim fiyatı transfer kaydından değil FIFO
                        // katmanlarından türetilir, bu yüzden "aynı birim fiyat"
                        // şartı tutmaz ve mükerrer satır açılırdı.
                        if (ActiveSemiFinishedProductId is Guid semiId
                            && productId == semiId
                            && !IsSemiFinishedRow(line)
                            && IsSemiFinishedIncluded())
                        {
                            ToastHelper.Show(
                                $"'{prod.Name}' yarımamülü bu pusulaya zaten dahil edildi. "
                                + "151 'Yarımamülden gelen' olarak maliyete yazıldığı için tekrar seçilemez.",
                                ToastType.Warning);
                            edit.EditValue = line.ProductId ?? (Guid?)null;
                            return;
                        }

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

        /// <summary>
        /// "Yarımamülden gelen" kutusuna girilen TUTARDAN yarımamülün tüketilecek
        /// MİKTARINI hesaplar ve grid satırının miktarını günceller.
        ///
        /// <para>
        /// Miktar ORTALAMA birim maliyetten bölünerek bulunamaz. Ortalama,
        /// tutarın hangi girişten alınacağını söylemez; üstelik onay anında
        /// yevmiye FIFO katman kırılımıyla yazıldığı için iki hesap birbirini
        /// tutmaz. Bu yüzden miktar, <see cref="StockCostingLayers"/> üzerinde
        /// TUTAR üzerinden yürütülerek türetilir: ilk giriş tamamen karşılanır,
        /// kalan tutar sonraki girişten alınır.
        /// </para>
        /// </summary>
        private void SyncSemiFinishedQuantityFromAmount()
        {
            Guid? semiId = ActiveSemiFinishedProductId;
            if (semiId is null)
            {
                return;
            }

            CostSlipItemEditDto? semiLine = _lines.FirstOrDefault(IsSemiFinishedLine);
            if (semiLine is null)
            {
                return;
            }

            decimal amount = _accountAmounts.GetValueOrDefault(ExpenseAccountHelper.SemiFinishedAccount);
            if (amount < 0m)
            {
                return;
            }

            // Katman listesi yoksa FIFO kırılımı yapılamaz. Miktar sıfıra
            // düşürülürse kayıt "Miktar sıfırdan büyük olmalıdır" hatasıyla
            // reddedilir; bu yüzden eski davranışa (ortalama) düşülür ve
            // kullanıcıya durum bildirilir.
            if (_semiFinishedLayers.Count == 0)
            {
                decimal fallback = semiLine.UnitPrice > 0m
                    ? StockCostingLayers.NormalizeQuantity(amount / semiLine.UnitPrice)
                    : 0m;

                if (fallback <= 0m || semiLine.Quantity == fallback)
                {
                    return;
                }

                semiLine.Quantity = fallback;
                semiLine.TotalAmount = Math.Round(fallback * semiLine.UnitPrice, 2);
                WarnSemiFinishedLayersMissing();

                UpdateMaterialProductDataSource();
                RefreshAvailableQuantities();
                gridLinesView.RefreshData();
                return;
            }

            decimal quantity = ResolveSemiFinishedQuantity(amount);
            decimal realizedAmount = StockCostingLayers.TotalAmount(
                StockCostingLayers.TakeByAmount(
                    _semiFinishedLayers,
                    amount,
                    _semiFinishedCostingMethod));

            // Girilen tutar mevcut katmanları aşıyorsa kırılım yalnızca
            // mevcut tutarı karşılar. Fazlası sahte maliyet olurdu; bu yüzden
            // tutar gerçekleşen değere indirilir ve kullanıcı bilgilendirilir.
            if (amount - realizedAmount > 0.005m)
            {
                ToastHelper.Show(
                    $"Yarımamül için kullanılabilecek tutar {realizedAmount:n2} TL. "
                    + "Tutar buna indirildi.",
                    ToastType.Warning,
                    5000);

                SetSemiFinishedAccountAmount(realizedAmount);
                amount = realizedAmount;
                quantity = ResolveSemiFinishedQuantity(amount);
            }

            decimal unitPrice = quantity > 0m
                ? Math.Round(amount / quantity, 4)
                : 0m;

            if (semiLine.Quantity == quantity && semiLine.UnitPrice == unitPrice)
            {
                return;
            }

            semiLine.Quantity = quantity;
            semiLine.UnitPrice = unitPrice;
            semiLine.TotalAmount = Math.Round(amount, 2);

            UpdateMaterialProductDataSource();
            RefreshAvailableQuantities();
            gridLinesView.RefreshData();
        }

        /// <summary>
        /// "Yarımamülden gelen" tutarını hem modelde hem kutuda günceller.
        /// Kutunun <c>EditValueChanged</c> olayının yeniden tetiklenmesi
        /// engellenir; aksi hâlde senkronizasyon özyinelemeli çalışır.
        /// </summary>
        private void SetSemiFinishedAccountAmount(decimal amount)
        {
            _accountAmounts[ExpenseAccountHelper.SemiFinishedAccount] = amount;

            if (_accountInputs.TryGetValue(ExpenseAccountHelper.SemiFinishedAccount, out TextEdit? semiInput))
            {
                _syncingTotals = true;
                try
                {
                    semiInput.EditValue = Math.Round(amount, 2);
                }
                finally
                {
                    _syncingTotals = false;
                }
            }
        }

        private bool _semiFinishedLayerWarningShown;

        /// <summary>
        /// Katman listesi yüklenemediğinde kullanıcıyı bilgilendirir: miktar
        /// ortalamadan türetildiği için giriş bazında kırılım yapılamadı.
        /// </summary>
        private void WarnSemiFinishedLayersMissing()
        {
            if (_semiFinishedLayerWarningShown)
            {
                return;
            }

            _semiFinishedLayerWarningShown = true;
            ToastHelper.Show(
                "Yarımamül giriş katmanları okunamadı; miktar ortalama maliyetten hesaplandı. "
                + "Katman bazlı (FIFO) tüketim için pusulayı yeniden açın.",
                ToastType.Warning,
                6000);
        }

        /// <summary>
        /// Girilen tutarın FIFO/LIFO ile hangi giriş katmanlarından
        /// karşılanacağını bulur ve tüketilecek miktarı döndürür. Mevcut
        /// katmanların toplamından fazlası tüketilemez.
        /// </summary>
        private decimal ResolveSemiFinishedQuantity(decimal amount)
        {
            if (amount <= 0m || _semiFinishedLayers.Count == 0)
            {
                return 0m;
            }

            List<CostingLayer> taken = StockCostingLayers.TakeByAmount(
                _semiFinishedLayers,
                amount,
                _semiFinishedCostingMethod);

            return StockCostingLayers.TotalQuantity(taken);
        }

        /// <summary>
        /// Yarımamülün malzeme satırıdır. Bu satır 151 stok çıkışını
        /// taşıdığı için grid'de TUTULUR, ancak maliyeti artık "Yarımamülden
        /// gelen" gider kutusu (151) taşır; bu yüzden 710 grid toplamına ve
        /// kaydedilen kalemlere GİRMEZ. Aksi halde aynı tutar bir kez 710
        /// içinden bir kez de 151'den iki kez giderleşirdi.
        /// </summary>
        /// <summary>
        /// Etkin yarımamül ürün kimliği; yoksa veya Guid.Empty ise null döner.
        /// _semiFinishedProductId nullable olduğu için Guid.Empty ile doğrudan
        /// karşılaştırmak null durumunu YAKALAMAZ.
        /// </summary>
        private Guid? ActiveSemiFinishedProductId
            => _semiFinishedProductId is Guid id && id != Guid.Empty ? id : null;

        private bool IsSemiFinishedLine(CostSlipItemEditDto line)
            => ActiveSemiFinishedProductId is Guid semiId
                && line.ProductId is Guid productId
                && productId == semiId;

        /// <summary>
        /// Satır, yarımamülün taşıyacağı özel satır mı? Grid'de hesap
        /// sütunu düzenlenemediği için bu işaret yalnızca "Yarımamülden gelen"
        /// satırında 151 olur. Satırın ürünü temizlendiğinde
        /// <see cref="IsSemiFinishedLine"/> yanlış döner; bu yüzden ürün
        /// seçimi kontrolünde bu işaret kullanılır.
        /// </summary>
        private static bool IsSemiFinishedRow(CostSlipItemEditDto line)
            => line.ExpenseAccountType == ExpenseAccountHelper.SemiFinishedAccount;

        /// <summary>
        /// Yarımamül bu pusulanın maliyetine zaten girmiş mi?
        /// </summary>
        /// <remarks>
        /// İki durum da "dahil" sayılır: yarımamülün grid'de tek satırı vardır
        /// (normal akış) veya yalnızca 151 "Yarımamülden gelen" kutusunda tutar
        /// bulunur (satırı taşınmamış eski kayıt). İkisinde de ürün seçiminde
        /// tekrar sunulursa 151 hem satır hem kutu üzerinden iki kez maliyete
        /// girer.
        /// </remarks>
        private bool IsSemiFinishedIncluded()
            => ActiveSemiFinishedProductId is not null
                && (_lines.Any(IsSemiFinishedLine)
                    || Math.Abs(_accountAmounts.GetValueOrDefault(
                        ExpenseAccountHelper.SemiFinishedAccount)) > 0.004m);

        private void RecalculateTotals()
        {
            foreach (var line in _lines)
            {
                line.TotalAmount = Math.Round(line.Quantity * line.UnitPrice, 2);
            }

            decimal gridTotal = _lines.Where(l => !IsSemiFinishedLine(l)).Sum(l => l.TotalAmount);

            // Türetilmiş hesap (mamülde 710, hizmette 740-01) yalnızca grid
            // toplamıdır. Yarımamül satırı buraya yazılmaz; maliyeti 151
            // gider kutusundan gelir.
            _accountAmounts[DerivedAccount] = Math.Round(gridTotal, 2);

            if (_accountInputs.TryGetValue(DerivedAccount, out TextEdit? derivedInput))
            {
                _syncingTotals = true;
                try
                {
                    derivedInput.EditValue = _accountAmounts[DerivedAccount];
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
            bool invalid = false;

            if (qty > 0 && total > 0)
            {
                decimal desired = DesiredUnitCost;

                if (desired > 0m)
                {
                    // Kullanıcı birim maliyeti kendisi belirledi: girilen değer
                    // olduğu gibi kullanılır, otomatik yuvarlama yapılmaz.
                    // Ürün (151/152) bu tutarla değerlenir; gider tarafı ile
                    // ürün değerini eşitlemek için fark amortisman giderine
                    // eklenir.
                    unitCost = desired;
                    reconciled = Math.Round(unitCost * qty, 2);
                    diff = Math.Round(reconciled - total, 2);

                    // Girilen değer toplam maliyetten düşükse fark NEGATİF olur.
                    // Kayıt yolu negatif kalemleri attığı için (amount <= 0
                    // atlanır) böyle bir kayıtta toplam maliyet ile ürün
                    // değeri tutmaz, defter dengesiz kalır. Bu yüzden fark
                    // yazılmaz, kayıt engellenir ve kutu kırmızıya döner.
                    invalid = diff < 0m;
                    if (invalid)
                    {
                        diff = 0m;
                    }
                }
                else
                {
                    // Kutu boş: birim maliyet virgülden sonra iki rakam olacak
                    // şekilde hep YUKARI yuvarlanır (en yakına yuvarlama aşağı
                    // değer üretip amortisman alanına eksi giriş ekleyebiliyor).
                    unitCost = Math.Ceiling(total / qty * 100m) / 100m;
                    reconciled = Math.Round(unitCost * qty, 2);
                    diff = Math.Round(reconciled - total, 2);
                    if (diff < 0m)
                    {
                        diff = 0m;
                    }
                }

                target = CurrentType == CostSlipType.Service
                    ? ExpenseAccountType.Account740_7
                    : ExpenseAccountType.Account730_07;
            }

            _unitCost = unitCost;
            _roundingDiff = diff;
            _roundingTargetAccount = target;
            _desiredUnitCostInvalid = invalid;

            // Geçersiz girişte ürün değeri olarak mutabakatsız bir tutar
            // gösterilmez; toplam olduğu gibi bırakılır.
            _grandTotal = invalid ? total : reconciled;

            if (_desiredUnitCostInput is not null)
            {
                _desiredUnitCostInput.Properties.Appearance.BorderColor = invalid
                    ? Color.FromArgb(180, 35, 24)
                    : _desiredUnitCostNormalBorder;
                _desiredUnitCostInput.Properties.Appearance.BackColor = invalid
                    ? Color.FromArgb(254, 226, 226)
                    : _desiredUnitCostNormalBack;
                _desiredUnitCostInput.Properties.Appearance.ForeColor = invalid
                    ? Color.FromArgb(180, 35, 24)
                    : _desiredUnitCostNormalText;
            }

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

                // Yarı mamul (151) kendi kovasına gider ve YALNIZCA "Maliyet
                // Bedeli" satırına eklenir; 730/740 kovalarına karışmaz.
                (byte)ExpenseAccountHelper.SemiFinishedAccount => "M151",
                _ => string.Empty
            };
        }

        private async void BtnSaveDraft_Click(object? sender, EventArgs e)
        {
            await SaveAsync();
        }

        /// <summary>
        /// Pusulayı kaydeder. Kayıt daima TASLAK olarak yazılır; onay kayıt
        /// formunda yapılmaz, listedeki satır onayı ya da toplu onay ile yapılır.
        /// </summary>
        private async Task SaveAsync()
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

            // Yarımamül yalnızca tek satır olarak maliyete girebilir: hem
            // "Yarımamülden gelen" satırı hem de kutu tutarı 151'e yazıldığı
            // için aynı ürünün ikinci satırı maliyeti iki kez sayar. Ürün
            // seçimi zaten engelleniyor; bu, korumanın atlandığı durumlar
            // (eski taslak, toplu düzenleme) için son savunmadır.
            if (ActiveSemiFinishedProductId is Guid saveSemiId)
            {
                CostSlipItemEditDto[] semiLines = materialLines
                    .Where(l => l.ProductId == saveSemiId)
                    .ToArray();

                if (semiLines.Length > 1)
                {
                    ToastHelper.Show(
                        "Yarımamül maliyete yalnızca bir satır olarak eklenebilir. "
                        + "Mükerrer yarımamül satırını silin.",
                        ToastType.Warning);

                    int semiRow = _lines.IndexOf(semiLines[1]);
                    if (semiRow >= 0)
                    {
                        gridLinesView.FocusedRowHandle = semiRow;
                    }

                    return;
                }
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

            // İstenen birim maliyet toplam maliyetten düşükse amortisman
            // giderine yazılacak fark negatif olur; kayıt yolu negatif kalemleri
            // attığı için pusula toplamı ile ürün değeri tutmaz. Böyle bir
            // kayıt oluşturulmaz.
            if (_desiredUnitCostInvalid)
            {
                ToastHelper.Show(
                    "İstenen birim maliyet, pusulanın toplam maliyetinden düşük olamaz.",
                    ToastType.Warning,
                    5000);
                _desiredUnitCostInput?.Focus();
                return;
            }

            ApplyRoundingAdjustmentToAccounts();

            // Malzeme satırları türetilmiş hesaba (710) yazılır. Hesap bazlı
            // giriş alanındaki tutarlar ürünsüz kalem olarak yazılır.
            //
            // Yarımamül satırı istisnadır: grid'de kalır (CostSlipStockHelper
            // bu satırdan 151 stok çıkışını üretir) ancak 710'a DEĞİL 151'e
            // yazılır. Böylece maliyet hem 710'da hem 151'de sayılmaz.
            List<CostSlipItemModel> itemModels = materialLines.Select(l => new CostSlipItemModel(
                l.ProductId,
                l.ProductUnitTypeId,
                IsSemiFinishedLine(l) ? ExpenseAccountHelper.SemiFinishedAccount : derivedAccount,
                l.Quantity,
                l.UnitPrice,
                l.Description)).ToList();

            bool hasSemiFinishedLine = materialLines.Any(IsSemiFinishedLine);

            foreach ((ExpenseAccountType account, decimal amount) in _accountAmounts)
            {
                if (account == derivedAccount || amount <= 0)
                {
                    continue;
                }

                // Yarımamül satırı varsa 151 tutarı zaten o kalemde yazılı;
                // ayrıca ürünsüz kalem olarak yazılırsa mükerrer olur.
                if (account == ExpenseAccountHelper.SemiFinishedAccount && hasSemiFinishedLine)
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

            if (RequiresRecipeCheck
                && producedProductId is Guid producedId
                && !await ConfirmRecipeCompatibilityAsync(producedId, materialLines, _grandTotal, quantity))
            {
                return;
            }

            bool ok = false;
            Guid? createdSlipId = null;
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
                    Result<CostSlipCreateResult>? createResult = await CrudExecutor.TryExecuteAsync(new CostSlipCreateCommand(
                        SlipNumber: number,
                        CostSlipType: CurrentType,
                        CostDate: date,
                        WorkshopId: workshopId,
                        ProducedProductId: producedProductId,
                        CustomerId: null,
                        Quantity: quantity,
                        Description: txtDescription.Text.Trim(),
                        Items: itemModels));
                    ok = createResult is not null;
                    createdSlipId = createResult?.Data?.SlipId;
                }
            }
            finally
            {
                btnSaveDraft.Enabled = !ok;
            }

            if (!ok)
            {
                return;
            }

            bool isEditingDraft = _editing is { Status: CostSlipStatus.Draft };

            if (_editing is null && createdSlipId is Guid createdId)
            {
                _editing = new CostSlipListDto
                {
                    Id = createdId,
                    SlipNumber = number,
                    CostSlipType = CurrentType,
                    Status = CostSlipStatus.Draft,
                    CostDate = date,
                    WorkshopId = workshopId
                };
            }

            ToastHelper.Show(
                isEditingDraft
                    ? "Maliyet pusulası taslağı güncellendi."
                    : "Maliyet pusulası taslak olarak kaydedildi. Onaylanınca stok hareketleri oluşturulacak.",
                ToastType.Success);

            _saved = true;
            LockAfterSave();
        }

        private async Task<bool> ConfirmRecipeCompatibilityAsync(Guid producedProductId, List<CostSlipItemEditDto> materialLines, decimal grandTotal, int quantity)
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
                return await TryCreateRecipeFromSlipAsync(producedProductId, materialLines, quantity, compare.ProducedProductName);
            }

            if (compare.Mismatches.Count == 0)
            {
                return true;
            }

            System.Text.StringBuilder sb = new();
            sb.AppendLine("Reçete ile malzeme kullanımı uyumsuz (kaydedilmesi onaylandığında düzeltme gerekir):");
            sb.AppendLine();
            foreach (RecipeCompareMismatchDto m in compare.Mismatches)
            {
                sb.AppendLine($"  • {m.ProductName}: reçete {m.Expected:n2}, gerçek {m.Actual:n2} ({m.Reason})");
            }
            sb.AppendLine();
            sb.AppendLine("Yine de kaydetmek istiyor musunuz?");

            return MsgBox.Confirm(sb.ToString(), "Reçete Uyumsuzluk Uyarısı") == DialogResult.Yes;
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("CostSlip.RecipeCompatibilityConfirm", ex);
            return true;
        }
    }

    private async Task<bool> TryCreateRecipeFromSlipAsync(
        Guid producedProductId,
        List<CostSlipItemEditDto> materialLines,
        int quantity,
        string producedProductName)
    {
        try
        {
            DialogResult dr = MsgBox.Confirm(
                $"'{producedProductName}' için reçete henüz tanımlı değil.\nMaliyeti reçete olarak kaydetmek istermisiniz?",
                "Reçete Kaydı");
            if (dr != DialogResult.Yes)
            {
                return true;
            }

            RecipeMaterialCandidate[] candidates = materialLines
                .Where(l => l.ProductId != null)
                .Select(l => new RecipeMaterialCandidate(
                    l.ProductId!.Value,
                    l.ProductName ?? string.Empty,
                    l.ProductUnitTypeName ?? string.Empty,
                    l.Quantity))
                .ToArray();

            if (candidates.Length == 0)
            {
                return true;
            }

            using (var dialog = new RecipeMaterialSelectionForm(producedProductName, quantity, candidates))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return true;
                }

                await CrudExecutor.ExecuteAsync(new RecipeSaveCommand(
                    producedProductId,
                    true,
                    dialog.SelectedItems,
                    SelectedWorkshopId ?? Guid.Empty));
            }

            ToastHelper.Show("Reçete olarak kaydedildi.", ToastType.Success);
            return true;
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("CostSlip.RecipeCreateFromSlip", ex);
            return true;
        }
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

            if (compare.Mismatches.Count > 0)
            {
                System.Text.StringBuilder sb = new();
                sb.AppendLine("Reçete ile malzeme kullanımı uyumsuz:");
                foreach (RecipeCompareMismatchDto m in compare.Mismatches)
                {
                    sb.AppendLine($"  {m.ProductName}: reçete {m.Expected:n2}, gerçek {m.Actual:n2} ({m.Reason})");
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
        catch (Exception ex)
        {
            CrashLog.WriteException("CostSlip.RecipeCheck", ex);
        }
}

        /// <summary>
        /// Kaydettikten sonra formu kilitler. Kayıt daima taslak olduğu için
        /// durum etiketi "Durum: Taslak" olur.
        /// </summary>
        private void LockAfterSave()
        {
            btnPrintSlip.Enabled = true;
            btnSaveDraft.Enabled = false;
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

            lblStatusValue.Text = "Durum: Taslak";
            lblStatusValue.Appearance.ForeColor = SkinTheme.Warning;
        }

        // Hesap alanı, satırları RebuildAccountPanel tarafından kurulduktan sonra
        // LayoutAccountsPanel ile yerleştirilir. Daha önce OnLoad'da bir kez
        // daha çağrılıyordu; o an hesap satırları henüz boş olduğundan form
        // önce sıfır satırlı, sonra dolu hâliyle iki kez boyanıyor ve ekran
        // titriyordu.

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

                SetReportParam(report, "ProductName", lookUpProducedProduct.EditValue is Guid pid && pid != Guid.Empty
                    ? _products.FirstOrDefault(p => p.Id == pid)?.Name ?? string.Empty
                    : string.Empty);
                SetReportParam(report, "Quantity", int.TryParse(txtQuantity.Text.Trim(), out int qty) ? qty : 0);
                SetReportParam(report, "Tarih", DateOnly.FromDateTime(dtCostDate.DateTime));
                SetReportParam(report, "Donem", dtCostDate.DateTime.ToString(
                    "MM'. Ay - 'MMMM'-'yyyy",
                    System.Globalization.CultureInfo.GetCultureInfo("tr-TR")));
                SetReportParam(report, "Isyurdu", company.Name);

                string workshopName = lookUpWorkshop.EditValue is Guid wid && wid != Guid.Empty
                    ? _workshops.FirstOrDefault(w => w.Id == wid)?.Display ?? string.Empty
                    : _workshopName;
                SetReportParam(report, "Workshop", workshopName);
                SetReportParam(report, "Antet", company.Letterhead);

                SetReportParam(report, "CiltNo", dtCostDate.DateTime.Year);
                SetReportParam(report, "SerialNo", txtSlipNumber.Text.Trim());
                SetReportParam(report, "SiparisNo", txtSlipNumber.Text.Trim());

                string[] moneyParams = ["M710", "M720", "M730", "M740", "M750", "M760", "M770", "M780", "M151"];
                foreach (string param in moneyParams)
                {
                    decimal value = totals.TryGetValue(param, out decimal sum) ? Math.Round(sum, 2) : 0;
                    SetReportParam(report, param, value);
                }

                decimal grandTotal = _accountAmounts.Values.Sum();
                SetReportParam(report, "Toplam", Math.Round(grandTotal, 2));

                // Alt bilgi cümlesindeki birim maliyet ifade içinde hesaplanırsa
                // miktar sıfırken bölme hatası boş metne yol açar; burada
                // biçimlendirilmiş metin parametre olarak verilir.
                SetReportParam(
                    report,
                    "BirimFiyat",
                    FormatUnitPrice(int.TryParse(txtQuantity.Text.Trim(), out int previewQty) ? previewQty : 0, grandTotal));

                // İmza kutuları (işyurdu müdürü, atölye şefi, taşınır kayıt
                // yetkilisi) personel görev kayıtlarından çözümlenir. Atölye
                // seçilmemişse atölye şefi kutusu boş basılır.
                Guid? signatoryWorkshopId = lookUpWorkshop.EditValue is Guid swid && swid != Guid.Empty
                    ? swid
                    : null;
                await CostSlipSignatoryHelper.ApplySignatoriesAsync(report, signatoryWorkshopId);

                // Yalnızca belge üretimi bekleme penceresinin kapsamında; önizleme
                // penceresi modal olduğu için bekleme kapandıktan sonra açılır.
await ReportPreviewHelper.PrintAsync(
                report,
                caption: "Pusula hazırlanıyor...",
                description: "Lütfen bekleyin...");
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Maliyet pusulası açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private static void SetReportParam(DevExpress.XtraReports.UI.XtraReport report, string name, object value)
        {
            if (report.Parameters[name] is { } parameter)
            {
                parameter.Value = value;
            }
        }

        private static string FormatUnitPrice(int quantity, decimal total)
        {
            return quantity <= 0
                ? string.Empty
                : (total / quantity).ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
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
