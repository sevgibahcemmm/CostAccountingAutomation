using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.StockCounts;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.StockCountListReports
{
    public sealed partial class StockCountPromptForm : XtraForm
    {
        private static readonly Color AccentColor = Color.FromArgb(64, 120, 200);
        private static readonly Color DangerColor = Color.FromArgb(200, 60, 60);

        private sealed class StockCountPromptItem
        {
            public StockCountPromptItem(Guid id, string display)
            {
                Id = id;
                Display = display;
            }

            public Guid Id { get; }
            public string Display { get; }

            public override string ToString() => Display;
        }

        private readonly List<StockCountPromptItem> _workshops;
        private readonly List<StockCountPromptItem> _warehouses;
        private DateEdit? dateEdit;

        public StockCountGroupMode Mode { get; private set; }
        public bool AllGroups { get; private set; }
        public List<Guid> GroupIds { get; private set; } = [];
        public DateOnly AsOfDate { get; private set; }

        public StockCountPromptForm(
            IReadOnlyList<ChartOfAccountLookUpDto> workshops,
            IReadOnlyList<ChartOfAccountLookUpDto> warehouses)
        {
            InitializeComponent();

            _workshops = workshops
                .Where(w => w.Type == ChartOfAccountType.Workshop)
                .OrderBy(w => w.Display)
                .Select(w => new StockCountPromptItem(w.Id, w.Display))
                .ToList();

            _warehouses = warehouses
                .Where(w => w.Type == ChartOfAccountType.Warehouse)
                .OrderBy(w => w.Display)
                .Select(w => new StockCountPromptItem(w.Id, w.Display))
                .ToList();

            radioGroup.Properties.Items.AddRange(new[]
            {
                new DevExpress.XtraEditors.Controls.RadioGroupItem
                {
                    Value = (byte)StockCountGroupMode.Workshop,
                    Description = "Atölye Bazında"
                },
                new DevExpress.XtraEditors.Controls.RadioGroupItem
                {
                    Value = (byte)StockCountGroupMode.Warehouse,
                    Description = "Depo Bazında"
                }
            });
            radioGroup.EditValue = (byte)StockCountGroupMode.Workshop;

            lblHeaderTitle.Text = "Stok Sayım Listesi";
            Text = "Stok Sayım Listesi";
            lblHeaderSub.Text = "Raporu hangi kırılımda hazırlamak istiyorsunuz?";

            // Tarih seçici
            var lblDate = new LabelControl
            {
                Text = "Sayım Tarihi:",
                Location = new Point(24, 376),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(100, 18),
                Appearance = { Font = new Font("Segoe UI", 9F) }
            };

            dateEdit = new DateEdit
            {
                Location = new Point(130, 373),
                Size = new Size(160, 24),
                Properties =
                {
                    DisplayFormat = { FormatString = "dd.MM.yyyy", FormatType = DevExpress.Utils.FormatType.DateTime },
                    EditFormat = { FormatString = "dd.MM.yyyy", FormatType = DevExpress.Utils.FormatType.DateTime },
                    UseMaskAsDisplayFormat = true,
                    CalendarTimeEditing = DevExpress.Utils.DefaultBoolean.False
                },
                EditValue = DateTime.Today
            };

            Controls.Add(lblDate);
            Controls.Add(dateEdit);

            // Badge ve butonları aşağı kaydır
            badge.Location = new Point(24, 410);
            lblInfo.Location = new Point(24, 462);
            footerDivider.Location = new Point(24, 492);
            btnOk.Location = new Point(296, 514);
            btnCancel.Location = new Point(191, 514);
            ClientSize = new Size(470, 566);

            FillList(checkedWorkshops, _workshops);
            FillList(checkedWarehouses, _warehouses);

            radioGroup.EditValueChanged += (_, _) => RefreshListVisibility();
            checkedWorkshops.ItemCheck += (_, _) => UpdateBadge();
            checkedWarehouses.ItemCheck += (_, _) => UpdateBadge();
            btnSelectAll.Click += (_, _) => SetAllChecked(true);
            btnClear.Click += (_, _) => SetAllChecked(false);
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (_, _) => Close();

            RefreshListVisibility();
            UpdateBadge();
        }

        private StockCountGroupMode CurrentMode => radioGroup.EditValue is byte mode
            ? (StockCountGroupMode)mode
            : StockCountGroupMode.Workshop;

        private CheckedListBoxControl CurrentList => CurrentMode == StockCountGroupMode.Warehouse
            ? checkedWarehouses
            : checkedWorkshops;

        private List<StockCountPromptItem> CurrentItems => CurrentMode == StockCountGroupMode.Warehouse
            ? _warehouses
            : _workshops;

        private static void FillList(CheckedListBoxControl list, List<StockCountPromptItem> items)
        {
            list.Items.Clear();
            foreach (StockCountPromptItem item in items)
            {
                list.Items.Add(item, true);
            }
        }

        private void RefreshListVisibility()
        {
            bool byWarehouse = CurrentMode == StockCountGroupMode.Warehouse;

            checkedWarehouses.Visible = byWarehouse;
            checkedWorkshops.Visible = !byWarehouse;
            lblSection.Text = byWarehouse ? "DEPO SEÇİMİ" : "ATÖLYE SEÇİMİ";

            UpdateBadge();
        }

        private void SetAllChecked(bool value)
        {
            CheckedListBoxControl list = CurrentList;
            for (int i = 0; i < list.Items.Count; i++)
            {
                list.SetItemChecked(i, value);
            }

            UpdateBadge();
        }

        private void UpdateBadge()
        {
            List<StockCountPromptItem> selected = GetSelected(CurrentList);
            int total = CurrentItems.Count;

            lblBadge.Text = total == 0
                ? "Seçilebilir kayıt bulunamadı"
                : selected.Count == 0
                    ? "Hiçbiri seçilmedi"
                    : selected.Count == total
                        ? $"Tümü seçildi ({selected.Count})"
                        : $"{selected.Count} / {total} seçildi";

            badgeAccentStrip.Appearance.BackColor = selected.Count == 0 ? DangerColor : AccentColor;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            List<StockCountPromptItem> selected = GetSelected(CurrentList);

            if (selected.Count == 0)
            {
                ToastHelper.Show(CurrentMode == StockCountGroupMode.Warehouse
                    ? "En az bir depo seçmelisiniz."
                    : "En az bir atölye seçmelisiniz.", ToastType.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            Mode = CurrentMode;
            AllGroups = selected.Count == CurrentItems.Count;
            GroupIds = selected.Select(s => s.Id).ToList();
            AsOfDate = DateOnly.FromDateTime((dateEdit?.EditValue as DateTime?) ?? DateTime.Today);
            DialogResult = DialogResult.OK;
        }

        private static List<StockCountPromptItem> GetSelected(CheckedListBoxControl list)
        {
            List<StockCountPromptItem> result = [];

            for (int i = 0; i < list.Items.Count; i++)
            {
                if (list.GetItemChecked(i) && list.Items[i].Value is StockCountPromptItem item)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}