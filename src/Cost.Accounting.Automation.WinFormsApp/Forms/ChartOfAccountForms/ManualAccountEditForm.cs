using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ChartOfAccountForms
{
    public sealed class ManualAccountEditForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly Guid? _preselectedParentId;
        private bool _loading;

        private Label lblTitle = default!;
        private Label lblSubtitle = default!;
        private Label lblParent = default!;
        private Label lblCode = default!;
        private Label lblName = default!;
        private LookUpEdit cmbParent = default!;
        private TextEdit txtCode = default!;
        private TextEdit txtName = default!;
        private CheckEdit chkActive = default!;
        private SimpleButton btnSave = default!;
        private SimpleButton btnCancel = default!;

        public ManualAccountEditForm(Guid? preselectedParentId = null)
        {
            _preselectedParentId = preselectedParentId;

            BuildLayout();
            WireEvents();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            Text = "Yeni Hesap Kaydı";
            IconOptions.SvgImage = DxIcon.ChartAccounts;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 340);
            Font = new Font("Segoe UI", 9F);

            lblTitle = new Label
            {
                Text = "Manüel Hesap / Alt Hesap Ekle",
                Location = new Point(24, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold)
            };

            lblSubtitle = new Label
            {
                Text = "Üst hesabı seçerek alt kod ekleyin. 900 (Tüketimler) dahil tüm hesap planı kapsanır.",
                Location = new Point(26, 48),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            lblParent = MakeLabel("Üst Hesap:", 24, 92);
            cmbParent = new LookUpEdit { Location = new Point(120, 88), Size = new Size(400, 24) };

            lblCode = MakeLabel("Kod:", 24, 128);
            txtCode = new TextEdit { Location = new Point(120, 124), Size = new Size(400, 24) };

            lblName = MakeLabel("Hesap Adı:", 24, 164);
            txtName = new TextEdit { Location = new Point(120, 160), Size = new Size(400, 24) };

            chkActive = new CheckEdit
            {
                Text = "Aktif",
                Location = new Point(120, 200),
                Size = new Size(120, 24),
                Checked = true
            };

            btnSave = new SimpleButton
            {
                Text = "Kaydet",
                Location = new Point(336, 272),
                Size = new Size(84, 34)
            };
            btnCancel = new SimpleButton
            {
                Text = "Kapat",
                Location = new Point(428, 272),
                Size = new Size(84, 34)
            };

            Controls.AddRange([
                lblTitle, lblSubtitle, lblParent, cmbParent, lblCode, txtCode,
                lblName, txtName, chkActive, btnSave, btnCancel
            ]);

            ResumeLayout(false);
        }

        private static Label MakeLabel(string text, int x, int y)
            => new() { Text = text, Location = new Point(x, y), AutoSize = true };

        private void WireEvents()
        {
            Load += ManualAccountEditForm_Load;
            cmbParent.EditValueChanged += CmbParent_EditValueChanged;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
        }

        private async void ManualAccountEditForm_Load(object? sender, EventArgs e)
        {
            _loading = true;
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None);
                List<ChartOfAccountLookUpDto> accounts = result.Data ?? [];

                var items = new List<ParentOption>();
                items.Add(new ParentOption { Id = null, Display = "— Kök Hesap (Anagrup) —" });
                items.AddRange(accounts.Select(a => new ParentOption
                {
                    Id = a.Id,
                    Display = $"{a.Code} - {a.Name}"
                }));

                cmbParent.Properties.DataSource = items;
                cmbParent.Properties.ValueMember = nameof(ParentOption.Id);
                cmbParent.Properties.DisplayMember = nameof(ParentOption.Display);
                cmbParent.Properties.NullText = "Üst hesap seçin";

                _loading = false;

                if (_preselectedParentId is Guid parentId
                    && items.Any(i => i.Id == parentId))
                {
                    cmbParent.EditValue = parentId;
                }
                else if (accounts.Count > 0)
                {
                    cmbParent.EditValue = null;
                }

                await SuggestCodeAsync();
            }
            catch (Exception ex)
            {
                _loading = false;
                ToastHelper.Show("Hesap planı yüklenemedi: " + ex.Message, ToastType.Warning);
            }
        }

        private async void CmbParent_EditValueChanged(object? sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            await SuggestCodeAsync();
        }

        private async Task SuggestCodeAsync()
        {
            Guid? parentId = cmbParent.EditValue is Guid id && id != Guid.Empty ? id : null;

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(new ChartOfAccountGetNextCodeQuery(parentId), CancellationToken.None);
                if (result.IsSuccessful)
                {
                    txtCode.Text = result.Data ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("ChartOfAccount.NextCode", ex);
            }

            txtCode.Focus();
            txtCode.SelectAll();
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ToastHelper.Show("Hesap adı zorunludur.", ToastType.Warning);
                txtName.Focus();
                return;
            }

            string code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                ToastHelper.Show("Hesap kodu zorunludur. Otomatik önerilen kodu kullanabilirsiniz.", ToastType.Warning);
                txtCode.Focus();
                return;
            }

            Guid? parentId = cmbParent.EditValue is Guid id && id != Guid.Empty ? id : null;

            btnSave.Enabled = false;
            try
            {
                bool ok = await CrudExecutor.ExecuteAsync(new ChartOfAccountManualCreateCommand(
                    parentId,
                    code,
                    name,
                    chkActive.Checked));

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

        private sealed class ParentOption
        {
            public Guid? Id { get; set; }
            public string Display { get; set; } = default!;
        }
    }
}