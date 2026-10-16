using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.DirectProcurement;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using FluentValidation.Results;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.DirectProcurementForms
{
    /// <summary>
    /// Kaynak sistemdeki <c>TFrmYeniIhale</c> ("Yeni Doğrudan Temin Girişi")
    /// penceresinin birebir düzenini taşıyan giriş/düzenleme penceresi.
    /// </summary>
    /// <remarks>
    /// Yerleşim ve etiket metinleri <c>TFRMYENIIHALE.dfm</c>'den alınmıştır. Alan
    /// karşılıkları: <c>EDDOSYANO→FileNumber</c>, <c>EDISINADI→WorkName</c>,
    /// <c>EDIHALETIPI→FileType</c>, <c>EDBIRIM→CompanyId</c>,
    /// <c>EDCALISMAYILI→WorkingYear</c>, <c>EDTEMINSEKLI→Flow</c>,
    /// <c>EDYMVARMI→HasApproximateCost</c>, <c>EDYMHESAPSEKLI→CostEstimationMethod</c>,
    /// <c>EDGECMISALIM→UseHistoryPurchases</c>, <c>EDSONTEKLIF→RequireFinalOffer</c>,
    /// <c>EDSOZLESME→HasContract</c>, <c>cxDBSpinEdit1→DecimalPlaces</c>,
    /// <c>cxDBSpinEdit2→UnitPriceDecimalPlaces</c>. Dört "Temin Türü" düğmesi
    /// <c>Category</c> alanını belirler.
    /// </remarks>
    public partial class DirectProcurementFileEditForm : XtraForm
    {
        private readonly DirectProcurementFileDto? _editing;
        private readonly IMasterCompanyNameLookup? _companyLookup;

        private ProcurementCategoryType _category = ProcurementCategoryType.Goods;

        public DirectProcurementFileEditForm() : this(null, null)
        {
        }

        public DirectProcurementFileEditForm(DirectProcurementFileDto? existing, IMasterCompanyNameLookup? companyLookup = null)
        {
            InitializeComponent();
            _editing = existing;
            _companyLookup = companyLookup;

            bool isNew = _editing is null;
            Text = "Doğrudan Temin";
            lblHeaderTitle.Text = isNew ? "Yeni Doğrudan Temin Girişi" : "Doğrudan Temin Dosyası";

            LoadImages();

            FillTeminKindOptions();
            FillMaterialProcurementOptions();
            FillWorkingYears();

            WireEvents();
            Load += DirectProcurementFileEditForm_Load;
        }

        /// <summary>Kaynak DFM'den çıkarılan gömülü görselleri yükler.</summary>
        private void LoadImages()
        {
            picLogo.Image = EmbeddedImageLoader.Load("direct-procurement.png");
            btnMalAlimi.ImageOptions.Image = EmbeddedImageLoader.Load("temin-type-mal-alimi.png");
            btnHizmet.ImageOptions.Image = EmbeddedImageLoader.Load("temin-type-hizmet.png");
            btnYapim.ImageOptions.Image = EmbeddedImageLoader.Load("temin-type-yapim.png");
            btnDanismanlik.ImageOptions.Image = EmbeddedImageLoader.Load("temin-type-danismanlik.png");
        }

        private void WireEvents()
        {
            btnMalAlimi.Click += (_, _) => SelectCategory(ProcurementCategoryType.Goods);
            btnHizmet.Click += (_, _) => SelectCategory(ProcurementCategoryType.Service);
            btnYapim.Click += (_, _) => SelectCategory(ProcurementCategoryType.Construction);
            btnDanismanlik.Click += (_, _) => SelectCategory(ProcurementCategoryType.Consultancy);

            cmbTeminKind.EditValueChanged += (_, _) => UpdateTeminKindDescription();
            cmbUnit.EditValueChanged += (_, _) => lblUnitWarning.Visible = ReadOptionValue(cmbUnit) is null;

            btnGerekce.Click += (_, _) => ToastHelper.Show(
                "Doğrudan Temin Gerekçe Tutanağı ikinci fazda eklenecek.",
                ToastType.Info,
                3000);

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
        }

        private async void DirectProcurementFileEditForm_Load(object? sender, EventArgs e)
        {
            btnSave.Enabled = false;
            try
            {
                await LoadCompaniesAsync();

                if (_editing is not null)
                {
                    Populate(_editing);
                }
                else
                {
                    // Kaynak ekranda çalışma yılı seçili gelir; bugünün yılı ön doldurulur.
                    SetOptionValue(cmbWorkingYear, DateTime.Today.Year);
                    deEditDate.EditValue = DateTime.Today;
                    rgHasApproximateCost.EditValue = true;
                    rgUseHistoryPurchases.EditValue = true;
                    rgRequireFinalOffer.EditValue = true;
                    rgHasContract.EditValue = false;
                    rgCostEstimationMethod.EditValue = CostEstimationMethodType.CommonUnitPrice;
                    seDecimalPlaces.Value = 2;
                    seUnitPriceDecimalPlaces.Value = 2;
                    SelectCategory(ProcurementCategoryType.Goods);
                    txtFileNumber.Focus();
                }
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Dosya bilgileri yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private async Task LoadCompaniesAsync()
        {
            List<Option> options = [];

            if (_companyLookup is not null)
            {
                List<MasterCompanyInfo> companies = await _companyLookup.GetAllAsync();
                options.AddRange(companies.Select(c => new Option(c.Id, c.Name)));
            }

            FillOptions(cmbUnit, options);
            lblUnitWarning.Visible = true;
        }

        private void Populate(DirectProcurementFileDto file)
        {
            txtFileNumber.Text = file.FileNumber;
            memoWorkName.Text = file.WorkName;
            SetOptionValue(cmbUnit, file.CompanyId);
            SetOptionValue(cmbWorkingYear, file.WorkingYear);
            deEditDate.EditValue = file.Date?.ToDateTime(TimeOnly.MinValue);
            SetOptionValue(cmbTeminKind, file.FileType);
            SetOptionValue(cmbMaterialProcurement, file.Flow);
            SelectCategory(file.Category);
            UpdateTeminKindDescription();

            rgHasApproximateCost.EditValue = file.HasApproximateCost;
            rgCostEstimationMethod.EditValue = file.CostEstimationMethod;
            rgUseHistoryPurchases.EditValue = file.UseHistoryPurchases;
            rgRequireFinalOffer.EditValue = file.RequireFinalOffer;
            rgHasContract.EditValue = file.HasContract;
            seDecimalPlaces.Value = file.DecimalPlaces;
            seUnitPriceDecimalPlaces.Value = file.UnitPriceDecimalPlaces;

            lblUnitWarning.Visible = file.CompanyId is null;
        }

        private void SelectCategory(ProcurementCategoryType category)
        {
            _category = category;

            Highlight(btnMalAlimi, category == ProcurementCategoryType.Goods);
            Highlight(btnHizmet, category == ProcurementCategoryType.Service);
            Highlight(btnYapim, category == ProcurementCategoryType.Construction);
            Highlight(btnDanismanlik, category == ProcurementCategoryType.Consultancy);
        }

        /// <summary>Seçili "Temin Türü" düğmesini vurgular.</summary>
        private static void Highlight(SimpleButton button, bool selected)
        {
            button.Appearance.BackColor = selected
                ? System.Drawing.Color.FromArgb(198, 220, 245)
                : System.Drawing.Color.FromArgb(240, 244, 249);
            button.Appearance.BorderColor = selected
                ? System.Drawing.Color.FromArgb(90, 130, 190)
                : System.Drawing.Color.FromArgb(198, 214, 235);
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseBorderColor = true;
        }

        /// <summary>
        /// Kaynaktaki <c>EDTipi</c> (Temin Tipi Açıklama) memo'sunu seçili
        /// <see cref="ProcurementFileType"/> değerinin açıklamasıyla doldurur.
        /// Kaynakta bu alan salt okunurdur ve seçime göre değişir.
        /// </summary>
        private void UpdateTeminKindDescription()
        {
            if (ReadOptionValue(cmbTeminKind) is ProcurementFileType kind)
            {
                memoTeminKindDesc.Text = EnumDisplay.GetDisplayName(kind);
            }
            else
            {
                memoTeminKindDesc.Text = string.Empty;
            }
        }

        private void FillTeminKindOptions()
        {
            List<Option> options = [.. Enum.GetValues<ProcurementFileType>()
                .Select(v => new Option(v, EnumDisplay.GetDisplayName(v)))];

            FillOptions(cmbTeminKind, options);
        }

        private void FillMaterialProcurementOptions()
        {
            List<Option> options = [.. Enum.GetValues<ProcurementFlowType>()
                .Select(v => new Option(v, EnumDisplay.GetDisplayName(v)))];

            FillOptions(cmbMaterialProcurement, options);
        }

        /// <summary>
        /// Çalışma yılı listesi kaynak sistemdeki <c>DSLQCalismaYili</c>
        /// listesinden gelir; burada mevcut yılın çevresindeki 11 yıl üretilir.
        /// </summary>
        private void FillWorkingYears()
        {
            int current = DateTime.Today.Year;
            List<Option> options = [.. Enumerable.Range(current - 5, 11)
                .Select(y => new Option(y, y.ToString(System.Globalization.CultureInfo.CurrentCulture)))];

            FillOptions(cmbWorkingYear, options);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            _ = SaveAsync();
        }

        private async Task SaveAsync()
        {
            DirectProcurementFileCreateCommand create = new(
                txtFileNumber.Text.Trim(),
                memoWorkName.Text.Trim(),
                _category,
                ReadOptionValue(cmbTeminKind) as ProcurementFileType? ?? ProcurementFileType.DirectProcurement,
                Convert.ToInt32(ReadOptionValue(cmbWorkingYear) ?? DateTime.Today.Year, System.Globalization.CultureInfo.InvariantCulture),
                AsDate(deEditDate.EditValue),
                ReadOptionValue(cmbMaterialProcurement) as ProcurementFlowType? ?? ProcurementFlowType.Article22D1,
                ReadOptionValue(cmbUnit) as Guid?,
                _editing?.SupplierId,
                _editing?.ChartOfAccountId,
                _editing?.ProjectNumber ?? string.Empty,
                _editing?.DecisionNumber ?? string.Empty,
                _editing?.DecisionDate,
                _editing?.ApproximateCost,
                _editing?.Appropriation,
                _editing?.WorkNature ?? string.Empty,
                _editing?.WorkQuantity ?? string.Empty,
                _editing?.PurchasingUnit ?? string.Empty,
                _editing?.RelatedPerson ?? string.Empty,
                ReadBool(rgHasContract),
                _editing?.HasSpecification ?? false,
                _editing?.HasCommission ?? false,
                ReadCostEstimationMethod(),
                ReadBool(rgHasApproximateCost),
                ReadBool(rgUseHistoryPurchases),
                ReadBool(rgRequireFinalOffer),
                ReadDecimalPlaces(seDecimalPlaces),
                ReadDecimalPlaces(seUnitPriceDecimalPlaces),
                _editing?.IsActive ?? true);

            IRequest<Result<string>> command = _editing is null
                ? create
                : new DirectProcurementFileUpdateCommand(
                        _editing.Id,
                        create.FileNumber,
                        create.WorkName,
                        create.Category,
                        create.FileType,
                        create.WorkingYear,
                        create.Date,
                        create.Flow,
                        create.CompanyId,
                        create.SupplierId,
                        create.ChartOfAccountId,
                        create.ProjectNumber,
                        create.DecisionNumber,
                        create.DecisionDate,
                        create.ApproximateCost,
                        create.Appropriation,
                        create.WorkNature,
                        create.WorkQuantity,
                        create.PurchasingUnit,
                        create.RelatedPerson,
                        create.HasContract,
                        create.HasSpecification,
                        create.HasCommission,
                        create.CostEstimationMethod,
                        create.HasApproximateCost,
                        create.UseHistoryPurchases,
                        create.RequireFinalOffer,
                        create.DecimalPlaces,
                        create.UnitPriceDecimalPlaces,
                        create.IsActive);

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

        /// <summary>Kaynaktaki "Evet"/"Hayır" radyo düğmelerini okur.</summary>
        private static bool ReadBool(RadioGroup group) => group.EditValue is true;

        private CostEstimationMethodType ReadCostEstimationMethod()
            => rgCostEstimationMethod.EditValue is CostEstimationMethodType method
                ? method
                : CostEstimationMethodType.CommonUnitPrice;

        private static int ReadDecimalPlaces(SpinEdit edit)
            => Convert.ToInt32(edit.Value, System.Globalization.CultureInfo.InvariantCulture);

        private static DateOnly? AsDate(object? value)
            => value switch
            {
                DateTime dt => DateOnly.FromDateTime(dt),
                DateOnly d => d,
                _ => null
            };

        private static void FillOptions(ComboBoxEdit combo, IEnumerable<Option> options)
        {
            combo.Properties.Items.Clear();
            combo.Properties.Items.AddRange(options.Cast<object>().ToArray());
            combo.EditValue = null;
        }

        private static void SetOptionValue(ComboBoxEdit combo, object? value)
        {
            combo.EditValue = value is null
                ? null
                : combo.Properties.Items.Cast<Option>().FirstOrDefault(o => Equals(o.Value, value));
        }

        private static object? ReadOptionValue(ComboBoxEdit combo)
            => (combo.EditValue as Option)?.Value;

        /// <summary>
        /// Combo kutusu öğesi. <see cref="EditValue"/> öğenin kendisi olduğu için
        /// gerçek değer <see cref="Value"/> üzerinden okunur; görünen metin
        /// <see cref="Text"/> olur.
        /// </summary>
        private sealed record Option(object Value, string Text)
        {
            public override string ToString() => Text;
        }

        private bool RunApplicationValidator(object command)
        {
            ClearFieldErrors();

            ValidationResult? result = command switch
            {
                DirectProcurementFileCreateCommand create =>
                    new DirectProcurementFileCreateCommandValidator().Validate(create),
                DirectProcurementFileUpdateCommand update =>
                    new DirectProcurementFileUpdateCommandValidator().Validate(update),
                _ => null
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            (string Property, BaseEdit Editor)[] map =
            [
                (DirectProcurementCommandFields.FileNumber, txtFileNumber),
                (DirectProcurementCommandFields.WorkName, memoWorkName),
                (DirectProcurementCommandFields.WorkingYear, cmbWorkingYear),
                (DirectProcurementCommandFields.DecimalPlaces, seDecimalPlaces),
                (DirectProcurementCommandFields.UnitPriceDecimalPlaces, seUnitPriceDecimalPlaces)
            ];

            List<string> messages = [];
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

        private void ClearFieldErrors()
        {
            txtFileNumber.ErrorText = string.Empty;
            memoWorkName.ErrorText = string.Empty;
            cmbWorkingYear.ErrorText = string.Empty;
            seDecimalPlaces.ErrorText = string.Empty;
            seUnitPriceDecimalPlaces.ErrorText = string.Empty;
        }

        private void btnDanismanlik_Click(object sender, EventArgs e)
        {

        }
    }
}
