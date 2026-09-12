using System.Drawing;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using FluentValidation.Results;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CustomerForms
{
    public partial class CustomerEditForm : XtraForm
    {
        private readonly CustomerDto? _editing;

        public CustomerEditForm() : this(null)
        {
        }

        public CustomerEditForm(CustomerDto? existing)
        {
            InitializeComponent();
            _editing = existing;
            IconOptions.SvgImage = SvgIcons.HeaderUserIcon;

            Text = _editing is null ? "Yeni Müşteri" : "Müşteri Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Yeni müşteri tanımlamak için bilgileri doldurun"
                : "Müşteri bilgilerini güncelleyin";
            chkActive.Checked = _editing?.IsActive ?? true;

            lblHeaderIcon.ImageOptions.SvgImage = SvgIcons.HeaderUserIcon;
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(32, 32);

            StyleTabs();
            StyleButtons();

            AddFieldIcon(tabBasic, txtName, SvgIcons.UserIcon);
            AddFieldIcon(tabBasic, txtTaxOffice, SvgIcons.StarIcon);
            AddFieldIcon(tabBasic, txtTaxNumber, SvgIcons.StarIcon);
            AddFieldIcon(tabContact, txtPhone1, SvgIcons.MailIcon);
            AddFieldIcon(tabContact, txtEmail, SvgIcons.AtIcon);

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            Load += CustomerEditForm_Load;
        }

        private static void AddFieldIcon(Control parent, TextEdit editor, SvgImage icon)
        {
            editor.Properties.Padding = new Padding(26, 2, 2, 2);
            var label = new LabelControl
            {
                Parent = parent,
                Name = "Icon_" + editor.Name,
                Location = new Point(editor.Left + 4, editor.Top + 4),
                Size = new Size(18, 18),
                Cursor = Cursors.Default,
            };
            label.ImageOptions.SvgImage = icon;
            label.ImageOptions.SvgImageSize = new Size(18, 18);
            label.MouseDown += (_, _) => editor.Focus();
        }

        private void StyleTabs()
        {
            tabBasic.ImageOptions.SvgImage = SvgIcons.UserIcon;
            tabBasic.ImageOptions.SvgImageSize = new Size(16, 16);
            tabContact.ImageOptions.SvgImage = SvgIcons.AtIcon;
            tabContact.ImageOptions.SvgImageSize = new Size(16, 16);
        }

        private void StyleButtons()
        {
            btnSave.ImageOptions.SvgImage = SvgIcons.CheckIcon;
            btnSave.ImageOptions.SvgImageSize = new Size(20, 20);
            btnSave.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnCancel.ImageOptions.SvgImage = SvgIcons.CloseIcon;
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
        }

        private void CustomerEditForm_Load(object? sender, EventArgs e)
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
                ToastHelper.Show("Müşteri bilgileri yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private void Populate(CustomerDto customer)
        {
            txtName.Text = customer.Name;
            txtTaxOffice.Text = customer.TaxOffice;
            txtTaxNumber.Text = customer.TaxNumber;
            memoDescription.Text = customer.Description;
            txtCity.Text = customer.City;
            txtDistrict.Text = customer.District;
            memoAddress.Text = customer.FullAddress;
            txtPhone1.Text = customer.PhoneNumber1;
            txtPhone2.Text = customer.PhoneNumber2 ?? string.Empty;
            txtEmail.Text = customer.Email ?? string.Empty;
            chkActive.Checked = customer.IsActive;
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
                ? new CustomerCreateCommand(name, taxOffice, taxNumber, address, contact, description, isActive)
                : new CustomerUpdateCommand(_editing.Id, name, taxOffice, taxNumber, address, contact, description, isActive);

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
                CustomerCreateCommand create => new CustomerCreateCommandValidator().Validate(create),
                CustomerUpdateCommand update => new CustomerUpdateCommandValidator().Validate(update),
                _ => null,
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            (string Property, BaseEdit Editor)[] map =
            [
                (CustomerCommandFields.Name, txtName),
                (CustomerCommandFields.City, txtCity),
                (CustomerCommandFields.District, txtDistrict),
                (CustomerCommandFields.FullAddress, memoAddress),
                (CustomerCommandFields.PhoneNumber1, txtPhone1),
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

        private static class CustomerCommandFields
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