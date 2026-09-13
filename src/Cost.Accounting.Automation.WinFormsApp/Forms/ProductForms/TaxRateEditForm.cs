using System;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.XtraEditors;
using FluentValidation.Results;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public partial class TaxRateEditForm : XtraForm
    {
        private readonly TaxRateDto? _editing;

        public TaxRateEditForm() : this(null)
        {
        }

        public TaxRateEditForm(TaxRateDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            Text = _editing is null ? "Yeni KDV Oranı" : "KDV Oranı Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Ürün fiyatlarında kullanılacak yeni KDV oranını tanımlayın."
                : "Mevcut KDV oranı bilgilerini güncelleyin.";
            chkActive.Checked = _editing?.IsActive ?? true;

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            Load += Form_Load;
        }

        private void Form_Load(object? sender, EventArgs e)
        {
            btnSave.Enabled = false;
            try
            {
                if (_editing is not null)
                {
                    txtName.Text = _editing.Name;
                    spinRate.Value = _editing.Rate * 100m;
                }
                txtName.Focus();
                txtName.SelectAll();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            decimal rate = spinRate.Value / 100m;
            bool isActive = chkActive.Checked;

            IRequest<Result<string>> command = _editing is null
                ? new TaxRateCreateCommand(name, rate, isActive)
                : new TaxRateUpdateCommand(_editing.Id, name, rate, isActive);

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
            txtName.ErrorText = string.Empty;
            spinRate.ErrorText = string.Empty;

            ValidationResult? result = command switch
            {
                TaxRateCreateCommand create => new TaxRateCreateCommandValidator().Validate(create),
                TaxRateUpdateCommand update => new TaxRateUpdateCommandValidator().Validate(update),
                _ => null,
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            foreach (ValidationFailure failure in result.Errors)
            {
                if (failure.PropertyName == nameof(TaxRateCreateCommand.Name))
                {
                    txtName.ErrorText = failure.ErrorMessage;
                }

                if (failure.PropertyName == nameof(TaxRateCreateCommand.Rate))
                {
                    spinRate.ErrorText = failure.ErrorMessage;
                }
            }

            var message = string.Join(Environment.NewLine, result.Errors.Select(e => e.ErrorMessage));
            ToastHelper.Show(message, ToastType.Warning, 5000);
            txtName.Focus();
            return false;
        }
    }
}