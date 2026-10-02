using System;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.XtraEditors;
using FluentValidation.Results;
using TS.MediatR;
using TS.Result;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using Cost.Accounting.Automation.Application.Products.ProductUnitTypes;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public partial class ProductUnitTypeEditForm : XtraForm
    {
        private readonly ProductUnitTypeDto? _editing;

        public ProductUnitTypeEditForm() : this(null)
        {
        }

        public ProductUnitTypeEditForm(ProductUnitTypeDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            Text = _editing is null ? "Yeni Birim Cinsi" : "Birim Cinsi Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Ürünlerde kullanılacak yeni birim cinsini sisteme tanımlayın."
                : "Mevcut birim cinsi bilgilerini güncelleyin.";
            chkActive.IsOn = _editing?.IsActive ?? true;

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
            bool isActive = chkActive.IsOn;

            IRequest<Result<string>> command = _editing is null
                ? new ProductUnitTypeCreateCommand(name, isActive)
                : new ProductUnitTypeUpdateCommand(_editing.Id, name, isActive);

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

            ValidationResult? result = command switch
            {
                ProductUnitTypeCreateCommand create => new ProductUnitTypeCreateCommandValidator().Validate(create),
                ProductUnitTypeUpdateCommand update => new ProductUnitTypeUpdateCommandValidator().Validate(update),
                _ => null,
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            foreach (ValidationFailure failure in result.Errors)
            {
                if (failure.PropertyName == nameof(ProductUnitTypeCreateCommand.Name))
                {
                    txtName.ErrorText = failure.ErrorMessage;
                }
            }

            var message = string.Join(Environment.NewLine, result.Errors.Select(e => e.ErrorMessage));
            ToastHelper.Show(message, ToastType.Warning, 5000);
            txtName.Focus();
            return false;
        }
    }
}