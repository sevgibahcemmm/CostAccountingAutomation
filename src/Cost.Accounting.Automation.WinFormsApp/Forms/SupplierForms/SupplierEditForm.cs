using System.Windows.Forms;
using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using FluentValidation.Results;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.SupplierForms
{
    public partial class SupplierEditForm : XtraForm
    {
        private readonly SupplierDto? _editing;

        public SupplierEditForm() : this(null)
        {
        }

        public SupplierEditForm(SupplierDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            Text = _editing is null ? "Yeni Tedarikçi" : "Tedarikçi Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Yeni tedarikçi tanımlamak için bilgileri doldurun"
                : "Tedarikçi bilgilerini güncelleyin";
            chkActive.Checked = _editing?.IsActive ?? true;

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            Load += SupplierEditForm_Load;
        }

        private void FieldIcon_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is LabelControl icon && icon.Tag is Control editor)
            {
                editor.Focus();
            }
        }

        private void SupplierEditForm_Load(object? sender, EventArgs e)
        {
            btnSave.Enabled = false;
            try
            {
                if (_editing is not null)
                {
                    Populate(_editing);
                }
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Tedarikçi bilgileri yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private void Populate(SupplierDto supplier)
        {
            txtName.Text = supplier.Name;
            txtTaxOffice.Text = supplier.TaxOffice;
            txtTaxNumber.Text = supplier.TaxNumber;
            memoDescription.Text = supplier.Description;
            txtCity.Text = supplier.City;
            txtDistrict.Text = supplier.District;
            memoAddress.Text = supplier.FullAddress;
            txtPhone1.Text = supplier.PhoneNumber1;
            txtPhone2.Text = supplier.PhoneNumber2 ?? string.Empty;
            txtEmail.Text = supplier.Email ?? string.Empty;
            chkActive.Checked = supplier.IsActive;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string taxOffice = txtTaxOffice.Text.Trim();
            string taxNumber = txtTaxNumber.Text.Trim();
            string description = memoDescription.Text.Trim();

            Address address = new(
                txtCity.Text.Trim(),
                txtDistrict.Text.Trim(),
                memoAddress.Text.Trim());

            Contact contact = new(
                txtPhone1.Text.Trim(),
                txtPhone2.Text.Trim(),
                txtEmail.Text.Trim());

            bool isActive = chkActive.Checked;

            IRequest<Result<string>> command = _editing is null
                ? new SupplierCreateCommand(name, taxOffice, taxNumber, address, contact, description, isActive)
                : new SupplierUpdateCommand(_editing.Id, name, taxOffice, taxNumber, address, contact, description, isActive);

            if (!RunApplicationValidator(command))
            {
                return;
            }

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

        private bool RunApplicationValidator(object command)
        {
            ClearFieldErrors();

            ValidationResult? result = command switch
            {
                SupplierCreateCommand create => new SupplierCreateCommandValidator().Validate(create),
                SupplierUpdateCommand update => new SupplierUpdateCommandValidator().Validate(update),
                _ => null,
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            (string Property, BaseEdit Editor)[] map =
            [
                (SupplierCommandFields.Name, txtName),
                (SupplierCommandFields.City, txtCity),
                (SupplierCommandFields.District, txtDistrict),
                (SupplierCommandFields.FullAddress, memoAddress),
                (SupplierCommandFields.PhoneNumber1, txtPhone1),
            ];

            var messages = new List<string>();
            foreach (ValidationFailure failure in result.Errors)
            {
                (string, BaseEdit) entry = map.FirstOrDefault(m => m.Property == failure.PropertyName);
                if (entry.Item2 is not null)
                {
                    entry.Item2.ErrorText = failure.ErrorMessage;
                    messages.Add(failure.ErrorMessage);
                }
            }

            if (messages.Count == 0)
            {
                messages.AddRange(result.Errors.Select(e => e.ErrorMessage));
            }

            (string, BaseEdit) firstInvalid = map.FirstOrDefault(m => result.Errors.Any(e => e.PropertyName == m.Property));
            firstInvalid.Item2?.Focus();

            ToastHelper.Show(string.Join(Environment.NewLine, messages), ToastType.Warning, 6000);
            return false;
        }

        private static class SupplierCommandFields
        {
            public const string Name = "Name";
            public const string City = "Address.City";
            public const string District = "Address.District";
            public const string FullAddress = "Address.FullAddress";
            public const string PhoneNumber1 = "Contact.PhoneNumber1";
        }

        private void ClearFieldErrors()
        {
            txtName.ErrorText = string.Empty;
            txtTaxOffice.ErrorText = string.Empty;
            txtTaxNumber.ErrorText = string.Empty;
            memoDescription.ErrorText = string.Empty;
            txtCity.ErrorText = string.Empty;
            txtDistrict.ErrorText = string.Empty;
            memoAddress.ErrorText = string.Empty;
            txtPhone1.ErrorText = string.Empty;
            txtEmail.ErrorText = string.Empty;
        }
    }
}