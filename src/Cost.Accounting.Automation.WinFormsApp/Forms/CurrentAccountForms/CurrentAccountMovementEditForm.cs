using Cost.Accounting.Automation.Application.CurrentAccountMovements;
using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraGrid.Columns;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    public partial class CurrentAccountMovementEditForm : SkinSensitiveForm
    {
        private readonly CurrentAccountMovementDto? _editing;
        private readonly CurrentAccountType? _preselectedType;
        private List<CustomerDto> _customers = [];
        private List<SupplierDto> _suppliers = [];

        public CurrentAccountMovementEditForm() : this(null, null)
        {
        }

        public CurrentAccountMovementEditForm(CurrentAccountType? accountType) : this(null, accountType)
        {
        }

        public CurrentAccountMovementEditForm(CurrentAccountMovementDto? existing, CurrentAccountType? accountType = null)
        {
            InitializeComponent();
            _editing = existing;
            _preselectedType = accountType ?? existing?.CurrentAccountType;

            IconOptions.SvgImage = SvgIcons.Modules[5];
            lblHeaderIcon.ImageOptions.SvgImage = SvgIcons.Modules[5];

            Text = _editing is null ? "Yeni Cari Hareket" : "Cari Hareket İncele";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Yeni tahsilat, ödeme veya dekont işlemi kaydedin"
                : "Cari hareket detayları";

            InitControls();
            WireEvents();
        }

        private void InitControls()
        {
            cmbAccountType.Properties.Items.Clear();
            cmbAccountType.Properties.Items.Add("Müşteri");
            cmbAccountType.Properties.Items.Add("Tedarikçi");
            cmbAccountType.SelectedIndex = _preselectedType == CurrentAccountType.Supplier ? 1 : 0;

            cmbMovementType.Properties.Items.Clear();
            cmbMovementType.Properties.Items.Add("Tahsilat");
            cmbMovementType.Properties.Items.Add("Ödeme");
            cmbMovementType.Properties.Items.Add("Devir / Açılış");
            cmbMovementType.Properties.Items.Add("Borç Dekontu");
            cmbMovementType.Properties.Items.Add("Alacak Dekontu");
            cmbMovementType.SelectedIndex = 0;

            dtDate.DateTime = DateTime.Today;

            if (_editing is not null)
            {
                btnSave.Visible = false;
                cmbAccountType.ReadOnly = true;
                lookUpAccount.ReadOnly = true;
                cmbMovementType.ReadOnly = true;
                dtDate.ReadOnly = true;
                txtDocNo.ReadOnly = true;
                spinDebit.ReadOnly = true;
                spinCredit.ReadOnly = true;
                txtDescription.ReadOnly = true;
            }
        }

        private void WireEvents()
        {
            Load += CurrentAccountMovementEditForm_Load;
            cmbAccountType.SelectedIndexChanged += (_, _) => UpdateAccountDataSource();
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
        }

        private async void CurrentAccountMovementEditForm_Load(object? sender, EventArgs e)
        {
            await LoadLookUpsAsync();

            if (_editing is not null)
            {
                cmbAccountType.SelectedIndex = _editing.CurrentAccountType == CurrentAccountType.Customer ? 0 : 1;
                lookUpAccount.EditValue = _editing.CurrentAccountType == CurrentAccountType.Customer ? _editing.CustomerId : _editing.SupplierId;
                dtDate.DateTime = _editing.Date.ToDateTime(TimeOnly.MinValue);
                txtDocNo.Text = _editing.DocumentNo ?? string.Empty;
                spinDebit.Value = _editing.Debit;
                spinCredit.Value = _editing.Credit;
                txtDescription.Text = _editing.Description;
            }
        }

        private async Task LoadLookUpsAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                _customers = (await mediator.Send(new CustomerGetAllQuery())).ToList();
                _suppliers = (await mediator.Send(new SupplierGetAllQuery())).ToList();

                UpdateAccountDataSource();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Cari listesi yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private void UpdateAccountDataSource()
        {
            bool isCustomer = cmbAccountType.SelectedIndex == 0;
            lblAccountLabel.Text = isCustomer ? "Müşteri:" : "Tedarikçi:";

            lookUpAccount.Properties.DataSource = null;
            lookUpAccountView.Columns.Clear();

            if (isCustomer)
            {
                lookUpAccount.Properties.DataSource = _customers;
                lookUpAccount.Properties.ValueMember = nameof(CustomerDto.Id);
                lookUpAccount.Properties.DisplayMember = nameof(CustomerDto.Name);

                lookUpAccountView.Columns.AddField(nameof(CustomerDto.Name)).Caption = "Müşteri Adı";
                lookUpAccountView.Columns.AddField(nameof(CustomerDto.TaxNumber)).Caption = "Vergi No";
                lookUpAccountView.Columns.AddField(nameof(CustomerDto.City)).Caption = "Şehir";
            }
            else
            {
                lookUpAccount.Properties.DataSource = _suppliers;
                lookUpAccount.Properties.ValueMember = nameof(SupplierDto.Id);
                lookUpAccount.Properties.DisplayMember = nameof(SupplierDto.Name);

                lookUpAccountView.Columns.AddField(nameof(SupplierDto.Name)).Caption = "Tedarikçi Adı";
                lookUpAccountView.Columns.AddField(nameof(SupplierDto.TaxNumber)).Caption = "Vergi No";
                lookUpAccountView.Columns.AddField(nameof(SupplierDto.City)).Caption = "Şehir";
            }

            foreach (GridColumn col in lookUpAccountView.Columns)
            {
                col.Visible = true;
            }

            lookUpAccount.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFit;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (lookUpAccount.EditValue is not Guid accountId || accountId == Guid.Empty)
            {
                ToastHelper.Show("Lütfen bir cari seçiniz.", ToastType.Warning);
                lookUpAccount.Focus();
                return;
            }

            if (spinDebit.Value == 0 && spinCredit.Value == 0)
            {
                ToastHelper.Show("Borç veya alacak tutarından en az birini giriniz.", ToastType.Warning);
                spinDebit.Focus();
                return;
            }

            CurrentAccountType accountType = cmbAccountType.SelectedIndex == 0 ? CurrentAccountType.Customer : CurrentAccountType.Supplier;
            CurrentAccountMovementType movementType = cmbMovementType.SelectedIndex switch
            {
                0 => CurrentAccountMovementType.Collection,
                1 => CurrentAccountMovementType.Payment,
                2 => CurrentAccountMovementType.OpeningBalance,
                3 => CurrentAccountMovementType.DebitVoucher,
                4 => CurrentAccountMovementType.CreditVoucher,
                _ => CurrentAccountMovementType.Collection
            };

            DateOnly date = DateOnly.FromDateTime(dtDate.DateTime);

            CurrentAccountMovementCreateCommand command = new(
                CurrentAccountType: accountType,
                CustomerId: accountType == CurrentAccountType.Customer ? accountId : null,
                SupplierId: accountType == CurrentAccountType.Supplier ? accountId : null,
                Date: date,
                MovementType: movementType,
                DocumentNo: txtDocNo.Text.Trim(),
                Debit: spinDebit.Value,
                Credit: spinCredit.Value,
                Description: txtDescription.Text.Trim());

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
