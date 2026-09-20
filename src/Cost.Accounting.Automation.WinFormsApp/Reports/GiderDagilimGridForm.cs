using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraWaitForm;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Reports
{
    public sealed partial class GiderDagilimGridForm : XtraForm
    {
        private readonly ISender _mediator;
        private GridView _gridView = null!;
        private GridControl _gridControl = null!;
        private ComboBoxEdit _cmbPeriod = null!;
        private ComboBoxEdit _cmbReportType = null!;
        private SimpleButton _btnRefresh = null!;
        private SimpleButton _btnPrint = null!;

        public GiderDagilimGridForm() : this(null, null, null) { }

        public GiderDagilimGridForm(DateOnly? startDate, DateOnly? endDate, CostSlipType? type)
        {
            _mediator = Program.Services.GetRequiredService<ISender>();
            _preStartDate = startDate;
            _preEndDate = endDate;
            _preType = type;
            InitializeComponentManual();
            Load += async (_, _) => await LoadReportAsync();
        }

        private DateOnly? _preStartDate;
        private DateOnly? _preEndDate;
        private CostSlipType? _preType;

        private void InitializeComponentManual()
        {
            Text = "Gider Dağıtım Tablosu";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1400, 800);
            MinimumSize = new Size(1000, 600);

            var topPanel = new PanelControl { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
            Controls.Add(topPanel);

            var lblPeriod = new LabelControl { Text = "Dönem:", Location = new Point(10, 18), AutoSizeMode = LabelAutoSizeMode.None, Size = new Size(80, 20) };
            _cmbPeriod = new ComboBoxEdit { Location = new Point(95, 16), Width = 180 };
            _cmbPeriod.Properties.Items.AddRange(GeneratePeriodItems());
            _cmbPeriod.SelectedIndex = 0;
            _cmbPeriod.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

            var lblType = new LabelControl { Text = "Tür:", Location = new Point(290, 18), AutoSizeMode = LabelAutoSizeMode.None, Size = new Size(40, 20) };
            _cmbReportType = new ComboBoxEdit { Location = new Point(335, 16), Width = 200 };
            _cmbReportType.Properties.Items.AddRange(new[] { "Tümü", "Hizmet", "Mamül", "Yarımamül" });
            _cmbReportType.SelectedIndex = 0;

            _btnRefresh = new SimpleButton { Text = "Yenile", Location = new Point(550, 14), Width = 100, Height = 26 };
            _btnRefresh.ImageOptions.SvgImage = DxIcon.Refresh;
            _btnRefresh.Click += async (_, _) => await LoadReportAsync();

            _btnPrint = new SimpleButton { Text = "Yazdır", Location = new Point(660, 14), Width = 100, Height = 26 };
            _btnPrint.ImageOptions.SvgImage = DxIcon.Print;
            _btnPrint.Click += (_, _) => _gridControl.ShowRibbonPrintPreview();
            _btnPrint.Enabled = true;

            topPanel.Controls.AddRange(new Control[] { lblPeriod, _cmbPeriod, lblType, _cmbReportType, _btnRefresh, _btnPrint });

            _gridControl = new GridControl { Dock = DockStyle.Fill };
            Controls.Add(_gridControl);

            _gridView = new GridView(_gridControl)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false, ShowFooter = true, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true },
                OptionsSelection = { EnableAppearanceFocusedCell = true },
                OptionsCustomization = { AllowColumnMoving = false, AllowColumnResizing = true },
                OptionsPrint = { PrintHeader = true, PrintFooter = true }
            };
            _gridControl.MainView = _gridView;

            var workshopCol = new GridColumn
            {
                Caption = "Atölye",
                FieldName = "WorkshopName",
                Width = 200,
                Visible = true,
                OptionsColumn = { AllowEdit = false, FixedWidth = true },
                Summary = { new GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Count, "WorkshopName", "Toplam: {0}") }
            };
            _gridView.Columns.Add(workshopCol);

            var expenseTypes = Enum.GetValues<ExpenseAccountType>().OrderBy(e => (int)e).ToList();
            foreach (var expType in expenseTypes)
            {
                var displayName = CostSlipDto.GetDisplayName(expType);
                var col = new GridColumn
                {
                    Caption = displayName,
                    FieldName = $"Values[{expType}]",
                    Width = 120,
                    Visible = true,
                    DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "n2" },
                    AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } },
                    AppearanceHeader = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center, WordWrap = DevExpress.Utils.WordWrap.Wrap } },
                    OptionsColumn = { AllowEdit = false },
                    Summary = { new GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, $"Values[{expType}]", "{0:n2}") }
                };
                _gridView.Columns.Add(col);
            }

            var totalCol = new GridColumn
            {
                Caption = "Genel Toplam",
                FieldName = "Total",
                Width = 130,
                Visible = true,
                DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "n2" },
                AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far }, FontStyleDelta = FontStyle.Bold },
                AppearanceHeader = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center }, FontStyleDelta = FontStyle.Bold },
                OptionsColumn = { AllowEdit = false },
                Summary = { new GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "Total", "{0:n2}") }
            };
            _gridView.Columns.Add(totalCol);

            _gridView.BestFitColumns();
            _gridView.OptionsView.ColumnAutoWidth = false;
        }

        private async Task LoadReportAsync()
        {
            _btnRefresh.Enabled = false;
            try
            {
                using var wait = WaitFormHelper.Show<WaitForm>("Rapor hazırlanıyor...", "Lütfen bekleyin...");

                DateOnly startDate, endDate;
                CostSlipType? type;

                if (_preStartDate.HasValue && _preEndDate.HasValue)
                {
                    startDate = _preStartDate.Value;
                    endDate = _preEndDate.Value;
                    type = _preType;
                    // Disable the period/type controls since we're using pre-set values
                    _cmbPeriod.Enabled = false;
                    _cmbReportType.Enabled = false;
                    _btnRefresh.Enabled = false;
                    _btnPrint.Enabled = true;
                    Text += $" ({startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy})";
                }
                else
                {
                    var (s, e) = GetPeriodFromSelection();
                    startDate = s;
                    endDate = e;
                    type = _cmbReportType.SelectedIndex switch
                    {
                        1 => CostSlipType.Service,
                        2 => CostSlipType.Product,
                        3 => CostSlipType.SemiFinishedProduct,
                        _ => (CostSlipType?)null
                    };
                }

                var result = await _mediator.Send(new GiderDagilimReportQuery(startDate, endDate, type), CancellationToken.None);
                BuildGridData(result);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Rapor yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnRefresh.Enabled = true;
            }
        }

        private (DateOnly start, DateOnly end) GetPeriodFromSelection()
        {
            var today = DateTime.Today;
            var monthsBack = _cmbPeriod.SelectedIndex;
            var target = today.AddMonths(-monthsBack);
            var start = new DateOnly(target.Year, target.Month, 1);
            var end = new DateOnly(target.Year, target.Month, DateTime.DaysInMonth(target.Year, target.Month));
            return (start, end);
        }

        private List<string> GeneratePeriodItems()
        {
            var items = new List<string>();
            var today = DateTime.Today;
            for (int i = 0; i < 12; i++)
            {
                var d = today.AddMonths(-i);
                items.Add($"{d:MMMM yyyy} ({d:yyyy-MM})");
            }
            return items;
        }

        private void BuildGridData(GiderDagilimReportResult result)
        {
            var bindingList = new BindingList<GiderDagilimRowWrapper>(result.Rows.Select(r => new GiderDagilimRowWrapper(r)).ToList());
            _gridControl.DataSource = bindingList;
            _gridView.BestFitColumns();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gridControl?.Dispose();
            }
            base.Dispose(disposing);
        }

        private sealed class GiderDagilimRowWrapper
        {
            private readonly GiderDagilimRow _row;
            public GiderDagilimRowWrapper(GiderDagilimRow row) => _row = row;

            public string WorkshopName => _row.WorkshopName;
            public decimal Total => _row.Total;

            public decimal this[ExpenseAccountType key] => _row.Values.TryGetValue(key, out var v) ? v : 0m;
        }
    }
}