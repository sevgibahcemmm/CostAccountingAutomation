using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    public sealed partial class DateRangePromptForm : XtraForm
    {
        private readonly DateEdit _startEdit = new();
        private readonly DateEdit _endEdit = new();

        public DateOnly StartDate { get; private set; }
        public DateOnly EndDate { get; private set; }

        public DateRangePromptForm(DateOnly? defaultStart = null, DateOnly? defaultEnd = null)
        {
            InitializeComponent();

            Text = "Tarih Aralığı Seçin";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(340, 160);

            var lblStart = new LabelControl
            {
                Text = "Başlangıç Tarihi:",
                Location = new Point(20, 28),
                AutoSize = true
            };

            _startEdit.Location = new Point(160, 23);
            _startEdit.Width = 150;
            _startEdit.Properties.Mask.EditMask = "dd.MM.yyyy";
            _startEdit.Properties.Mask.UseMaskAsDisplayFormat = true;
            _startEdit.DateTime = (defaultStart ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)))
                .ToDateTime(TimeOnly.MinValue);

            var lblEnd = new LabelControl
            {
                Text = "Bitiş Tarihi:",
                Location = new Point(20, 68),
                AutoSize = true
            };

            _endEdit.Location = new Point(160, 63);
            _endEdit.Width = 150;
            _endEdit.Properties.Mask.EditMask = "dd.MM.yyyy";
            _endEdit.Properties.Mask.UseMaskAsDisplayFormat = true;
            _endEdit.DateTime = (defaultEnd ?? DateOnly.FromDateTime(DateTime.Today))
                .ToDateTime(TimeOnly.MinValue);

            var btnOk = new SimpleButton
            {
                Text = "Tamam",
                Location = new Point(130, 115),
                Width = 90,
                DialogResult = DialogResult.OK
            };

            var btnCancel = new SimpleButton
            {
                Text = "İptal",
                Location = new Point(230, 115),
                Width = 90,
                DialogResult = DialogResult.Cancel
            };

            btnOk.Click += BtnOk_Click;

            Controls.AddRange(new Control[] { lblStart, _startEdit, lblEnd, _endEdit, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            DateOnly start = DateOnly.FromDateTime(_startEdit.DateTime);
            DateOnly end = DateOnly.FromDateTime(_endEdit.DateTime);

            if (start > end)
            {
                XtraMessageBox.Show(
                    this,
                    "Başlangıç tarihi bitiş tarihinden büyük olamaz.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                DialogResult = DialogResult.None;
                return;
            }

            StartDate = start;
            EndDate = end;
        }
    }
}