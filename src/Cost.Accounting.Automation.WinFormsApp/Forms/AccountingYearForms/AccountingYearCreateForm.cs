using Cost.Accounting.Automation.Application.AccountingYears;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.AccountingYearForms
{
    /// <summary>
    /// Seçili kurum için yeni mali yıl açar. Veritabanı adı kurum adı ve yıl
    /// üzerinden önerilir; kullanıcı öneriyi değiştirebilir. Kaydetme sırasında
    /// veritabanı oluşturulur, şeması kurulur ve master'a yıl kaydı eklenir.
    /// </summary>
    public sealed partial class AccountingYearCreateForm : XtraForm
    {
        private Guid _companyId;
        private bool _nameEditedByUser;
        private bool _previewInProgress;

        /// <summary>
        /// Yalnızca Visual Studio tasarım yüzeyi içindir; gerçek açılışta
        /// <see cref="AccountingYearCreateForm(CompanyDto)"/> kullanılır.
        /// </summary>
        public AccountingYearCreateForm()
        {
            InitializeComponent();
            DesignTime.Guard(typeof(AccountingYearCreateForm));
        }

        public AccountingYearCreateForm(CompanyDto company)
        {
            ArgumentNullException.ThrowIfNull(company);

            _companyId = company.Id;

            InitializeComponent();

            Text = $"Mali Yıl Aç - {company.Name}";
            lblTitle.Text = "Mali Yıl Aç";
            lblSubtitle.Text = $"{company.Name} için yeni bir mali yıl veritabanı oluşturur.";
            txtCompany.Text = company.Name;

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int currentYear = DateTime.Now.Year;
            spnYear.Properties.MinValue = Year.MinSupported;
            spnYear.Properties.MaxValue = Year.MaxSupported;
            spnYear.EditValue = currentYear;
            dtOpeningDate.DateTime = new DateTime(currentYear, 1, 1);

            spnYear.ValueChanged += (_, _) => OnYearChanged();
            txtDatabaseName.EditValueChanged += (_, _) =>
            {
                if (_previewInProgress || _nameEditedByUser)
                {
                    return;
                }

                _ = RefreshSuggestedNameAsync();
            };

            _ = RefreshSuggestedNameAsync();
            spnYear.Focus();
        }

        private void OnYearChanged()
        {
            if (spnYear.EditValue is not int year)
            {
                return;
            }

            // Yıl değişince açılış tarihi de yılın ilk gününe döner.
            _previewInProgress = true;
            dtOpeningDate.DateTime = new DateTime(year, 1, 1);
            _previewInProgress = false;

            _ = RefreshSuggestedNameAsync();
        }

        /// <summary>
        /// Kurum adı ve yıldan önerilen veritabanı adını üretip forma yazar.
        /// Kullanıcı alanı elle düzenlediyse öneri yazılmaz.
        /// </summary>
        private async Task RefreshSuggestedNameAsync()
        {
            if (_nameEditedByUser || spnYear.EditValue is not int year)
            {
                return;
            }

            string companyName = txtCompany.Text.Trim();
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return;
            }

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(
                    new AccountingYearDatabaseNamePreviewQuery(companyName, year),
                    CancellationToken.None);

                if (!result.IsSuccessful || result.Data is null)
                {
                    return;
                }

                _previewInProgress = true;
                txtDatabaseName.Text = result.Data.SuggestedDatabaseName;

                lblPreview.ForeColor = result.Data.IsAvailable
                    ? Color.FromArgb(107, 114, 128)
                    : Color.FromArgb(220, 38, 38);

                lblPreview.Text = result.Data.Warning
                    ?? $"Önerilen ad: {result.Data.SuggestedDatabaseName}";

                if (!result.Data.IsAvailable)
                {
                    _nameEditedByUser = true;
                }
            }
            catch (Exception ex)
            {
                lblPreview.ForeColor = Color.FromArgb(220, 38, 38);
                lblPreview.Text = "Ad önerisi alınamadı: " + ex.Message;
            }
            finally
            {
                _previewInProgress = false;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (spnYear.EditValue is not int year)
            {
                ToastHelper.Show("Mali yıl seçilmelidir.", ToastType.Warning);
                spnYear.Focus();
                return;
            }

            string databaseName = txtDatabaseName.Text.Trim();
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                ToastHelper.Show("Veritabanı adı boş olamaz.", ToastType.Warning);
                txtDatabaseName.Focus();
                return;
            }

            // Aynı kural sunucu tarafında da FluentValidation ile uygulanır; burada
            // önden doğrulayarak kullanıcıya ham "Validation failed" yerine anlaşılır
            // bir mesaj gösteriyoruz.
            if (!DatabaseName.TryCreate(databaseName, out _, out string? nameError))
            {
                ToastHelper.Show(nameError ?? "Veritabanı adı geçersiz.", ToastType.Warning);
                txtDatabaseName.Focus();
                return;
            }

            DateTimeOffset? openingDate = dtOpeningDate.DateTime == DateTime.MinValue
                ? null
                : new DateTimeOffset(dtOpeningDate.DateTime);

            var command = new AccountingYearCreateCommand(_companyId, year, databaseName, openingDate);

            btnSave.Enabled = false;
            try
            {
                // Veritabanı oluşturma ve migration uzun sürebilir.
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(command, CancellationToken.None);

                if (!result.IsSuccessful || result.Data is null)
                {
                    ToastHelper.Show(
                        AuthFormStyles.GetErrorText(result.ErrorMessages),
                        ToastType.Error);
                    return;
                }

                string message = result.Data.DatabaseCreated
                    ? $"{year} mali yılı açıldı. '{result.Data.DatabaseName}' veritabanı oluşturuldu " +
                      $"({result.Data.AppliedMigrationCount} migration uygulandı)."
                    : $"{year} mali yılı açıldı. '{result.Data.DatabaseName}' veritabanı hazırlandı.";

                ToastHelper.Show(message, ToastType.Success);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ValidationException ex)
            {
                string message = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
                ToastHelper.Show(message, ToastType.Warning);
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Mali yıl açılamadı: " + ex.Message, ToastType.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
