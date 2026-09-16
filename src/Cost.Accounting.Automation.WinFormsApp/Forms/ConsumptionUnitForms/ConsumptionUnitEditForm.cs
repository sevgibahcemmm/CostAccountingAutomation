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
    public sealed class ConsumptionUnitEditForm : XtraForm
    {
        private readonly ConsumptionUnitDto? _editing;

        private Label lblTitle = default!;
        private Label lblSubtitle = default!;
        private Label lblCode = default!;
        private Label lblName = default!;
        private TextEdit txtCode = default!;
        private TextEdit txtName = default!;
        private CheckEdit chkActive = default!;
        private SimpleButton btnSave = default!;
        private SimpleButton btnCancel = default!;

        public ConsumptionUnitEditForm() : this(null)
        {
        }

        public ConsumptionUnitEditForm(ConsumptionUnitDto? existing)
        {
            _editing = existing;

            BuildLayout();
            WireEvents();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            Text = _editing is null ? "Yeni Tüketim Birimi" : "Tüketim Birimi Düzenle";
            IconOptions.SvgImage = DxIcon.StockIssue;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 300);
            Font = new Font("Segoe UI", 9F);

            lblTitle = new Label
            {
                Text = Text,
                Location = new Point(24, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            };

            lblSubtitle = new Label
            {
                Text = "900 (Tüketimler) altında yeni bir tüketim birimi tanımlayın.",
                Location = new Point(26, 50),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            lblCode = MakeLabel("Kod:", 24, 96);
            txtCode = new TextEdit { Location = new Point(120, 93), Size = new Size(360, 24) };

            lblName = MakeLabel("Birim Adı:", 24, 136);
            txtName = new TextEdit { Location = new Point(120, 133), Size = new Size(360, 24) };

            chkActive = new CheckEdit
            {
                Text = "Aktif",
                Location = new Point(120, 173),
                Size = new Size(120, 24),
                Checked = _editing?.IsActive ?? true
            };

            btnSave = new SimpleButton
            {
                Text = "Kaydet",
                Location = new Point(304, 240),
                Size = new Size(84, 32)
            };
            btnCancel = new SimpleButton
            {
                Text = "Kapat",
                Location = new Point(396, 240),
                Size = new Size(84, 32)
            };

            Controls.AddRange([
                lblTitle, lblSubtitle, lblCode, txtCode, lblName, txtName,
                chkActive, btnSave, btnCancel
            ]);

            ResumeLayout(false);
        }

        private static Label MakeLabel(string text, int x, int y)
            => new() { Text = text, Location = new Point(x, y), AutoSize = true };

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
