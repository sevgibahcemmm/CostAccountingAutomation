using System.Windows.Forms;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using FluentValidation.Results;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CompanyForms
{
    public partial class CompanyEditForm : XtraForm
    {
        private readonly CompanyDto? _editing;

        public CompanyEditForm() : this(null)
        {
        }

        public CompanyEditForm(CompanyDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            Text = _editing is null ? "Yeni Şirket" : "Şirket Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Yeni şirket tanımlamak için bilgileri doldurun"
                : "Şirket bilgilerini güncelleyin";
            chkActive.IsOn = _editing?.IsActive ?? true;

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            Load += CompanyEditForm_Load;
        }

        private void FieldIcon_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is LabelControl icon && icon.Tag is Control editor)
            {
                editor.Focus();
            }
        }

        private void CompanyEditForm_Load(object? sender, EventArgs e)
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
                ToastHelper.Show("Şirket bilgileri yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private void Populate(CompanyDto company)
        {
            txtName.Text = company.Name;
            txtTaxOffice.Text = company.TaxOffice;
            txtTaxNumber.Text = company.TaxNumber;
            txtPrefix.Text = company.CompanyPrefix;
            memoDescription.Text = company.Description;
            memoInvoice.Text = company.Invoiceinformation;
            memoLetterhead.Text = company.Letterhead;
            txtCity.Text = company.City;
            txtDistrict.Text = company.District;
            memoAddress.Text = company.FullAddress;
            txtPhone1.Text = company.PhoneNumber1;
            txtPhone2.Text = company.PhoneNumber2 ?? string.Empty;
            txtEmail.Text = company.Email ?? string.Empty;
            txtExpName.Text = company.ExpenditureUnitName;
            txtExpCode.Text = company.ExpenditureUnitCode;
            txtAccName.Text = company.AccountingUnitName;
            txtAccCode.Text = company.AccountingUnitCode;
            chkActive.IsOn = company.IsActive;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            Address address = new(
                txtCity.Text.Trim(),
                txtDistrict.Text.Trim(),
                memoAddress.Text.Trim());

            Contact contact = new(
                txtPhone1.Text.Trim(),
                txtPhone2.Text.Trim(),
                txtEmail.Text.Trim());

            string name = txtName.Text.Trim();
            string taxOffice = txtTaxOffice.Text.Trim();
            string taxNumber = txtTaxNumber.Text.Trim();
            string description = memoDescription.Text.Trim();
            string invoiceInformation = memoInvoice.Text.Trim();
            string letterhead = memoLetterhead.Text.Trim();
            string prefix = txtPrefix.Text.Trim();
            string expenditureName = txtExpName.Text.Trim();
            string expenditureCode = txtExpCode.Text.Trim();
            string accountingName = txtAccName.Text.Trim();
            string accountingCode = txtAccCode.Text.Trim();
            bool isActive = chkActive.IsOn;

            IRequest<Result<string>> command = _editing is null
                ? new CompanyCreateCommand(name, taxOffice, taxNumber, description, invoiceInformation,
                    letterhead, prefix, address, contact, expenditureName, expenditureCode,
                    accountingName, accountingCode, isActive)
                : new CompanyUpdateCommand(_editing.Id, name, taxOffice, taxNumber, description, invoiceInformation,
                    letterhead, prefix, address, contact, expenditureName, expenditureCode,
                    accountingName, accountingCode, isActive);

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
                CompanyCreateCommand create => new CompanyCreateCommandValidator().Validate(create),
                CompanyUpdateCommand update => new CompanyUpdateCommandValidator().Validate(update),
                _ => null,
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            (string Property, BaseEdit Editor)[] map =
            [
                (CompanyCommandFields.Name, txtName),
                (CompanyCommandFields.TaxOffice, txtTaxOffice),
                (CompanyCommandFields.TaxNumber, txtTaxNumber),
                (CompanyCommandFields.Invoiceinformation, memoInvoice),
                (CompanyCommandFields.CompanyPrefix, txtPrefix),
                (CompanyCommandFields.ExpenditureUnitName, txtExpName),
                (CompanyCommandFields.AccountingUnitName, txtAccName),
                (CompanyCommandFields.City, txtCity),
                (CompanyCommandFields.District, txtDistrict),
                (CompanyCommandFields.FullAddress, memoAddress),
                (CompanyCommandFields.PhoneNumber1, txtPhone1),
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

        private static class CompanyCommandFields
        {
            public const string Name = "Name";
            public const string TaxOffice = "TaxOffice";
            public const string TaxNumber = "TaxNumber";
            public const string Invoiceinformation = "Invoiceinformation";
            public const string CompanyPrefix = "CompanyPrefix";
            public const string ExpenditureUnitName = "ExpenditureUnitName";
            public const string AccountingUnitName = "AccountingUnitName";
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
            txtPrefix.ErrorText = string.Empty;
            memoDescription.ErrorText = string.Empty;
            memoInvoice.ErrorText = string.Empty;
            memoLetterhead.ErrorText = string.Empty;
            txtCity.ErrorText = string.Empty;
            txtDistrict.ErrorText = string.Empty;
            memoAddress.ErrorText = string.Empty;
            txtPhone1.ErrorText = string.Empty;
            txtEmail.ErrorText = string.Empty;
            txtExpName.ErrorText = string.Empty;
            txtAccName.ErrorText = string.Empty;
        }
    }
}