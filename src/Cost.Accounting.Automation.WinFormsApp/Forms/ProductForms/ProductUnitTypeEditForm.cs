using System;
using System.Drawing;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using FluentValidation.Results;
using TS.MediatR;
using TS.Result;

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
            IconOptions.SvgImage = SvgIcons.TagIcon;

            Text = _editing is null ? "Yeni Birim Cinsi" : "Birim Cinsi Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Ürünlerde kullanılacak yeni birim cinsini sisteme tanımlayın."
                : "Mevcut birim cinsi bilgilerini güncelleyin.";
            chkActive.Checked = _editing?.IsActive ?? true;

            lblHeaderIcon.ImageOptions.SvgImage = SvgIcons.TagIcon;
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(24, 24);

            StyleButtons();
            ApplyModernStyles();

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            Load += Form_Load;
        }


        private void ApplyModernStyles()
        {
            // Alanlar için modern iç boşluk ve tipografi iyileştirmeleri
            txtName.Properties.Padding = new Padding(8, 4, 8, 4);
            pnlHeader.Appearance.BackColor = Color.FromArgb(248, 249, 250);
            pnlHeader.Appearance.Options.UseBackColor = true;

            pnlFooter.Appearance.BackColor = Color.FromArgb(248, 249, 250);
            pnlFooter.Appearance.Options.UseBackColor = true;
        }

        private void StyleButtons()
        {
            // Kaydet Butonu (Modern Yeşil Vurgu)
            btnSave.ImageOptions.SvgImage = SvgIcons.CheckIcon;
            btnSave.ImageOptions.SvgImageSize = new Size(18, 18);
            btnSave.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnSave.Appearance.BackColor = Color.FromArgb(16, 185, 129);
            btnSave.Appearance.ForeColor = Color.White;
            btnSave.Appearance.Options.UseBackColor = true;
            btnSave.Appearance.Options.UseForeColor = true;

            // Vazgeç Butonu
            btnCancel.ImageOptions.SvgImage = SvgIcons.CloseIcon;
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnCancel.Appearance.ForeColor = Color.FromArgb(75, 85, 99);
            btnCancel.Appearance.Options.UseForeColor = true;
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
            bool isActive = chkActive.Checked;

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