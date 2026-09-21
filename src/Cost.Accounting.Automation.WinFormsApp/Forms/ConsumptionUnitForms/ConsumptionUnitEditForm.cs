using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ConsumptionUnitForms
{
    public sealed partial class ConsumptionUnitEditForm : XtraForm
    {
        private readonly ConsumptionUnitDto? _editing;

        public ConsumptionUnitEditForm() : this(null)
        {
        }

        public ConsumptionUnitEditForm(ConsumptionUnitDto? existing)
        {
            _editing = existing;

            InitializeComponent();

            IconOptions.SvgImage = DxIcon.StockIssue;
            Text = _editing is null ? "Yeni Tüketim Birimi" : "Tüketim Birimi Düzenle";
            lblTitle.Text = Text;
            chkActive.Checked = _editing?.IsActive ?? true;

            WireEvents();
        }

        private void WireEvents()
        {
            Load += ConsumptionUnitEditForm_Load;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
        }

        private async void ConsumptionUnitEditForm_Load(object? sender, EventArgs e)
        {
            if (_editing is not null)
            {
                txtCode.Text = _editing.Code;
                txtName.Text = _editing.Name;
                chkActive.Checked = _editing.IsActive;
                txtCode.ReadOnly = true;
                txtName.Focus();
                txtName.SelectAll();
                return;
            }

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await mediator.Send(new ConsumptionUnitGetNextCodeQuery(), CancellationToken.None);
                txtCode.Text = result.Data ?? string.Empty;
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Kod üretilemedi: " + ex.Message, ToastType.Warning);
            }

            txtName.Focus();
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ToastHelper.Show("Tüketim birimi adı zorunludur.", ToastType.Warning);
                txtName.Focus();
                return;
            }

            IRequest<Result<string>> command = _editing is null
                ? new ConsumptionUnitCreateCommand(name, txtCode.Text.Trim(), chkActive.Checked)
                : new ConsumptionUnitUpdateCommand(_editing.Id, name, chkActive.Checked);

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
}