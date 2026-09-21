using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using System.Windows.Controls.Primitives;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    public sealed partial class DateRangePromptForm : XtraForm
    {
        // Height (px) freed up when the type selector section is hidden — must match the
        // vertical gap reserved for lblType/lookupType in the designer layout.
        private const int TypeSelectorSectionHeight = 64;

        private static readonly Color AccentColor = Color.FromArgb(64, 120, 200);
        private static readonly Color AccentColorLight = Color.FromArgb(235, 242, 250);
        private static readonly Color DangerColor = Color.FromArgb(200, 60, 60);
        private static readonly Color DangerColorLight = Color.FromArgb(253, 236, 236);

        private SimpleButton? _activeQuickButton;

        public DateOnly StartDate { get; private set; }
        public DateOnly EndDate { get; private set; }

        public CostSlipType CostSlipType { get; private set; } = CostSlipType.Product;

        public DateRangePromptForm(
            DateOnly? defaultStart = null,
            DateOnly? defaultEnd = null,
            bool showTypeSelector = true,
            string headerTitle = "")
        {
            InitializeComponent();

            picHeader.SvgImage = DxIcon.Receipt;

            dateStart.DateTime = (defaultStart ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)))
                .ToDateTime(TimeOnly.MinValue);
            dateEnd.DateTime = (defaultEnd ?? DateOnly.FromDateTime(DateTime.Today))
                .ToDateTime(TimeOnly.MinValue);

            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (_, _) => Close();
            btnThisMonth.Click += (_, _) =>
            {
                SetRange(
                    new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                    new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1));
                HighlightQuickButton(btnThisMonth);
            };
            btnLastMonth.Click += (_, _) =>
            {
                DateTime first = new(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime firstLastMonth = first.AddMonths(-1);
                SetRange(firstLastMonth, firstLastMonth.AddMonths(1).AddDays(-1));
                HighlightQuickButton(btnLastMonth);
            };
            btnLast30.Click += (_, _) =>
            {
                SetRange(DateTime.Today.AddDays(-29), DateTime.Today);
                HighlightQuickButton(btnLast30);
            };
            btnThisYear.Click += (_, _) =>
            {
                SetRange(new DateTime(DateTime.Today.Year, 1, 1), DateTime.Today);
                HighlightQuickButton(btnThisYear);
            };

            if (!showTypeSelector)
            {
                lblType.Visible = false;
                lookupType.Visible = false;

                footerDivider.Location = new Point(footerDivider.Location.X, footerDivider.Location.Y - TypeSelectorSectionHeight);
                btnCancel.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y - TypeSelectorSectionHeight);
                btnOk.Location = new Point(btnOk.Location.X, btnOk.Location.Y - TypeSelectorSectionHeight);
                ClientSize = new Size(ClientSize.Width, ClientSize.Height - TypeSelectorSectionHeight);
            }
            else
            {
                lookupType.Properties.DataSource = Enum.GetValues<CostSlipType>()
                    .Select(t => new { Value = (byte)t, Name = EnumDisplay.GetDisplayName(t) })
                    .ToList();
                lookupType.Properties.ValueMember = "Value";
                lookupType.Properties.DisplayMember = "Name";
                lookupType.EditValue = (byte)CostSlipType.Product;
                lookupType.EditValueChanged += (_, _) =>
                {
                    if (lookupType.EditValue is byte value)
                    {
                        CostSlipType = (CostSlipType)value;
                        UpdateHeaderTitle();
                    }
                };
            }

            if (!string.IsNullOrWhiteSpace(headerTitle))
            {
                lblHeaderTitle.Text = headerTitle;
                Text = headerTitle;
            }
            else
            {
                UpdateHeaderTitle();
            }

            dateStart.EditValueChanged += (_, _) =>
            {
                ClearQuickHighlight();
                UpdateRangeLabel();
            };
            dateEnd.EditValueChanged += (_, _) =>
            {
                ClearQuickHighlight();
                UpdateRangeLabel();
            };
            UpdateRangeLabel();
        }

        private void SetRange(DateTime start, DateTime end)
        {
            dateStart.DateTime = start;
            dateEnd.DateTime = end;
            UpdateRangeLabel();
        }

        private void HighlightQuickButton(SimpleButton button)
        {
            ClearQuickHighlight();

            button.Appearance.BackColor = AccentColor;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;

            _activeQuickButton = button;
        }

        private void ClearQuickHighlight()
        {
            if (_activeQuickButton is null)
            {
                return;
            }

            _activeQuickButton.Appearance.Options.UseBackColor = false;
            _activeQuickButton.Appearance.Options.UseForeColor = false;
            _activeQuickButton = null;
        }

        private void UpdateHeaderTitle()
        {
            lblHeaderTitle.Text = $"{ShortTypeName(CostSlipType)} Gider Dağıtım Tablosu";
        }

        private static string ShortTypeName(CostSlipType type) => type switch
        {
            CostSlipType.Service => "Hizmet",
            CostSlipType.SemiFinishedProduct => "Yarı Mamül",
            CostSlipType.SemiFinishedService => "Yarı Mamül Hizmet",
            _ => "Mamül",
        };

        private void UpdateRangeLabel()
        {
            DateTime start = dateStart.DateTime.Date;
            DateTime end = dateEnd.DateTime.Date;
            bool isValid = start <= end;

            lblRange.Text = isValid
                ? $"{start:dd.MM.yyyy} - {end:dd.MM.yyyy}   ({end.Subtract(start).Days + 1} gün)"
                : "Başlangıç, bitişten sonra olamaz.";

            lblRange.Appearance.ForeColor = isValid ? AccentColor : DangerColor;
            rangeBadge.Appearance.BackColor = isValid ? AccentColorLight : DangerColorLight;
            rangeAccentStrip.Appearance.BackColor = isValid ? AccentColor : DangerColor;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            DateOnly start = DateOnly.FromDateTime(dateStart.DateTime);
            DateOnly end = DateOnly.FromDateTime(dateEnd.DateTime);

            if (start > end)
            {
                ToastHelper.Show("Başlangıç tarihi bitiş tarihinden sonra olamaz.", ToastType.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            StartDate = start;
            EndDate = end;
            DialogResult = DialogResult.OK;
        }
    }
}