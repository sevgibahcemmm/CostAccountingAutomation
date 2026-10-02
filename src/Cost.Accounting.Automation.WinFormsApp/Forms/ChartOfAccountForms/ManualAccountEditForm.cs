using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ChartOfAccountForms
{
    public sealed partial class ManualAccountEditForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly Guid? _preselectedParentId;
        private bool _loading;

        /// <summary>
        /// Yalnızca Visual Studio tasarım yüzeyi içindir; gerçek açılışta
        /// <see cref="ManualAccountEditForm(Guid?)"/> kullanılır.
        /// </summary>
        public ManualAccountEditForm()
        {
            InitializeComponent();
            DesignTime.Guard(typeof(ManualAccountEditForm));
        }

        public ManualAccountEditForm(Guid? preselectedParentId = null)
        {
            _preselectedParentId = preselectedParentId;

            InitializeComponent();
            WireEvents();
        }

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
                    chkActive.IsOn));

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