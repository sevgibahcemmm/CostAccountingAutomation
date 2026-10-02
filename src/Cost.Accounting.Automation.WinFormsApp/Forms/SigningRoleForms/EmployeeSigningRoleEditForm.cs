using System;
using Cost.Accounting.Automation.Application.Employees.SigningRoles;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using FluentValidation.Results;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.SigningRoleForms
{
    /// <summary>
    /// Yetkili görev tanımı ekleme / düzenleme formu. Bu form hem "Yetkili
    /// Görevler" liste ekranından hem de personel kaydındaki görev sekmesinden
    /// açılabilir; bu yüzden kaydettikten sonra çağıran tarafın listesini
    /// yenilemesi yeterlidir.
    /// </summary>
    public partial class EmployeeSigningRoleEditForm : XtraForm
    {
        private readonly EmployeeSigningRoleDto? _editing;
        private readonly IDisposable? _skinBinding;

        public EmployeeSigningRoleEditForm() : this(null)
        {
        }

        public EmployeeSigningRoleEditForm(EmployeeSigningRoleDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            Text = _editing is null ? "Yeni Yetkili Görev" : "Yetkili Görev Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Rapor imza bloklarında kullanılacak yeni bir görev tanımlayın."
                : "Mevcut görev tanımını güncelleyin.";

            chkActive.IsOn = _editing?.IsActive ?? true;

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();

            ApplySkin();
            _skinBinding = SkinTheme.Bind(ApplySkin);

            Load += Form_Load;
        }

        /// <summary>
        /// Başlık, alt bar, ayırıcılar ve etiketler aktif skinden çözülür. Sabit
        /// renkler koyu temalarda okunmaz olduğu için hiçbir yerde sabitlenmez.
        /// </summary>
        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(this);
            Color mutedSurface = SkinTheme.SurfaceMuted(surface);
            Color labelColor = SkinTheme.Blend(SkinTheme.Text, surface, 0.22F);

            pnlHeader.Appearance.BackColor = mutedSurface;
            pnlFooter.Appearance.BackColor = mutedSurface;
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlFooter.Appearance.Options.UseBackColor = true;

            pnlHeaderLine.Appearance.BackColor = SkinTheme.BorderMuted(surface);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlFooterLine.Appearance.BackColor = SkinTheme.BorderMuted(surface);
            pnlFooterLine.Appearance.Options.UseBackColor = true;

            lblTitle.Appearance.ForeColor = SkinTheme.Text;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Appearance.ForeColor = SkinTheme.SecondaryText;
            lblSubtitle.Appearance.Options.UseForeColor = true;

            foreach (LabelControl label in FieldLabels)
            {
                label.Appearance.ForeColor = labelColor;
                label.Appearance.Options.UseForeColor = true;
            }

            lblWorkshopHint.Appearance.ForeColor = SkinTheme.MutedText(surface);
            lblWorkshopHint.Appearance.Options.UseForeColor = true;
        }

        /// <summary>Gövdedeki alan etiketleri ve ipucu metni.</summary>
        private LabelControl[] FieldLabels =>
        [
            lblNameLabel,
            lblDescriptionLabel,
            lblSortLabel,
            lblWorkshopHint
        ];

        private void Form_Load(object? sender, EventArgs e)
        {
            if (_editing is null)
            {
                spinSortOrder.EditValue = 100;
            }
            else
            {
                txtName.Text = _editing.Name;
                txtDescription.Text = _editing.Description;
                chkRequiresWorkshop.IsOn = _editing.RequiresWorkshop;
                spinSortOrder.EditValue = _editing.SortOrder;
                chkActive.IsOn = _editing.IsActive;
            }

            txtName.Focus();
            txtName.SelectAll();
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string description = txtDescription.Text.Trim();
            bool requiresWorkshop = chkRequiresWorkshop.IsOn;
            int sortOrder = Convert.ToInt32(spinSortOrder.EditValue ?? 0);
            bool isActive = chkActive.IsOn;

            IRequest<Result<string>> command = _editing is null
                ? new EmployeeSigningRoleCreateCommand(
                    name, description, requiresWorkshop, sortOrder, isActive)
                : new EmployeeSigningRoleUpdateCommand(
                    _editing.Id, name, description, requiresWorkshop, sortOrder, isActive);

            if (!RunApplicationValidator(command))
            {
                return;
            }

            btnSave.Enabled = false;

            try
            {
                if (await CrudExecutor.ExecuteAsync(command))
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
                EmployeeSigningRoleCreateCommand create =>
                    new EmployeeSigningRoleCreateCommandValidator().Validate(create),
                EmployeeSigningRoleUpdateCommand update =>
                    new EmployeeSigningRoleUpdateCommandValidator().Validate(update),
                _ => null
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            foreach (ValidationFailure failure in result.Errors)
            {
                if (failure.PropertyName == nameof(EmployeeSigningRoleCreateCommand.Name))
                {
                    txtName.ErrorText = failure.ErrorMessage;
                }
                else if (failure.PropertyName == nameof(EmployeeSigningRoleCreateCommand.Description))
                {
                    txtDescription.ErrorText = failure.ErrorMessage;
                }
                else if (failure.PropertyName == nameof(EmployeeSigningRoleCreateCommand.SortOrder))
                {
                    spinSortOrder.ErrorText = failure.ErrorMessage;
                }
            }

            ToastHelper.Show(
                string.Join(Environment.NewLine, result.Errors.Select(e => e.ErrorMessage)),
                ToastType.Warning,
                5000);

            if (!string.IsNullOrEmpty(txtName.ErrorText))
            {
                txtName.Focus();
            }
            else if (!string.IsNullOrEmpty(txtDescription.ErrorText))
            {
                txtDescription.Focus();
            }

            return false;
        }

        private void ClearFieldErrors()
        {
            txtName.ErrorText = string.Empty;
            txtDescription.ErrorText = string.Empty;
            spinSortOrder.ErrorText = string.Empty;
        }
    }
}
