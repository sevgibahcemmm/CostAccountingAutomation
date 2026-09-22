using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    public sealed partial class WorkshopSelectionPromptForm : XtraForm
    {
        private static readonly Color AccentColor = Color.FromArgb(64, 120, 200);
        private static readonly Color DangerColor = Color.FromArgb(200, 60, 60);

        public List<string> SelectedWorkshops { get; private set; } = [];

        public WorkshopSelectionPromptForm(List<string> workshops, string headerTitle = "Atölye Seçimi")
        {
            InitializeComponent();

            checkedList.Items.AddRange(workshops.ToArray());
            for (int i = 0; i < checkedList.Items.Count; i++)
            {
                checkedList.SetItemChecked(i, true);
            }

            lblHeaderTitle.Text = headerTitle;
            Text = headerTitle;
            lblHeaderSub.Text = "Yazdırmak istediğiniz atölyeleri seçin";

            checkedList.ItemCheck += (_, _) => UpdateBadge();
            btnSelectAll.Click += (_, _) => SetAllChecked(true);
            btnClear.Click += (_, _) => SetAllChecked(false);
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (_, _) => Close();

            UpdateBadge();
        }

        private void SetAllChecked(bool value)
        {
            for (int i = 0; i < checkedList.Items.Count; i++)
            {
                checkedList.SetItemChecked(i, value);
            }

            UpdateBadge();
        }

        private void UpdateBadge()
        {
            int count = GetSelected().Count;

            lblBadge.Text = count == 0
                ? "Hiçbir atölye seçilmedi"
                : $"{count} atölye seçildi";

            badgeAccentStrip.Appearance.BackColor = count == 0 ? DangerColor : AccentColor;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            List<string> selected = GetSelected();

            if (selected.Count == 0)
            {
                ToastHelper.Show("En az bir atölye seçmelisiniz.", ToastType.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            SelectedWorkshops = selected;
            DialogResult = DialogResult.OK;
        }

        private List<string> GetSelected()
        {
            List<string> result = [];

            for (int i = 0; i < checkedList.Items.Count; i++)
            {
                if (checkedList.GetItemChecked(i) && checkedList.Items[i].Value is string name)
                {
                    result.Add(name);
                }
            }

            return result;
        }
    }
}