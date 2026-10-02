using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Employees;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Employees;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.EmployeeForms
{
    /// <summary>
    /// Personel kaydı ve yetkili görevlerinin düzenlendiği form.
    ///
    /// Görevler ayrı bir sekmede yönetilir çünkü rapor imza blokları bu
    /// görevlere göre çözülür: aynı kişi birden fazla görev taşıyabilir ve
    /// "Atölye Şefi" görevi için atölye seçimi zorunludur.
    /// </summary>
    public partial class EmployeeEditForm : XtraForm
    {
private readonly EmployeeDto? _editing;

        /// <summary>
        /// Görev satırları <see cref="BindingList{T}"/> olarak tutulur; sıradan
        /// <c>List</c> kullanılsaydı "Görev Ekle" düğmesi listeye ekleyecek ama
        /// grid satırı göstermeyecek ve kullanıcı boş formu kaydetmeye
        /// çalışacaktı. CostSlipEditForm'deki malzeme satırları da aynı
        /// nedenle BindingList kullanır.
        /// </summary>
        private readonly BindingList<EmployeeDutyDto> _duties = [];

        private List<ChartOfAccountLookUpDto> _workshops = [];
        private PhotoInput? _photo;

        public EmployeeEditForm() : this(null)
        {
        }

        public EmployeeEditForm(EmployeeDto? existing)
        {
            InitializeComponent();
            _editing = existing;

            Text = _editing is null ? "Yeni Personel" : "Personel Düzenle";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Rapor imza bloklarında kullanılacak personeli tanımlayın"
                : $"{_editing.FullName} bilgilerini güncelleyin";

            chkActive.Checked = _editing?.IsActive ?? true;

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            btnAddPhoto.Click += BtnAddPhoto_Click;
            btnRemovePhoto.Click += BtnRemovePhoto_Click;
            btnAddDutyShortcut.Click += BtnAddDutyShortcut_Click;
            btnAddDuty.Click += BtnAddDuty_Click;
            btnRemoveDuty.Click += BtnRemoveDuty_Click;

            Load += EmployeeEditForm_Load;
        }

        private void FieldIcon_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is LabelControl icon && icon.Tag is Control editor)
            {
                editor.Focus();
            }
        }

        private async void EmployeeEditForm_Load(object? sender, EventArgs e)
        {
            btnSave.Enabled = false;

            try
            {
                await LoadLookupsAsync();

                if (_editing is not null)
                {
                    Populate(_editing);

                    if (!string.IsNullOrWhiteSpace(_editing.PhotoPath))
                    {
                        LoadPhotoFromPath(_editing.PhotoPath);
                    }
                }
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Atölye listesi yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private async Task LoadLookupsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None);

            _workshops = result.IsSuccessful && result.Data is not null
                ? [.. result.Data.Where(a => a.Type == ChartOfAccountType.Workshop)]
                : [];

            ConfigureDutyGrid();
        }

        /// <summary>
        /// Görev ızgarası düzenlenebilirdir: kullanıcı görevi ve varsa atölyeyi
        /// doğrudan satır üzerinden seçer. Kurum geneli görevlerde atölye
        /// sütunu boş bırakılabilir.
        /// </summary>
        private void ConfigureDutyGrid()
        {
            viewDuties.Columns.Clear();

            // Enum değerlerini [Display] adlarıyla gösteren yardımcı kayıt.
            List<SigningRoleOption> roles = [.. EmployeeSigningRoleRules.ReportRoles
                .Select(r => new SigningRoleOption(r, EnumDisplay.GetDisplayName(r)))];

            RepositoryItemSearchLookUpEdit roleLookUp = new()
            {
                DataSource = roles,
                DisplayMember = nameof(SigningRoleOption.Display),
                NullText = "Görev seçiniz...",
                PopupFilterMode = PopupFilterMode.Contains,
                ValueMember = nameof(SigningRoleOption.Value)
            };

            roleLookUp.View.OptionsBehavior.AutoPopulateColumns = false;
            roleLookUp.View.Columns.Clear();
            roleLookUp.View.Columns.AddField(nameof(SigningRoleOption.Display)).Caption = "Görev";
            roleLookUp.View.Columns[0].Visible = true;
            roleLookUp.View.Columns[0].Width = 240;

            RepositoryItemSearchLookUpEdit workshopLookUp = new()
            {
                DataSource = _workshops,
                DisplayMember = nameof(ChartOfAccountLookUpDto.Display),
                NullText = "(kurum geneli)",
                PopupFilterMode = PopupFilterMode.Contains,
                ValueMember = nameof(ChartOfAccountLookUpDto.Id)
            };

            workshopLookUp.View.OptionsBehavior.AutoPopulateColumns = false;
            workshopLookUp.View.Columns.Clear();
            workshopLookUp.View.Columns.AddField(nameof(ChartOfAccountLookUpDto.Display)).Caption = "Atölye";
            workshopLookUp.View.Columns[0].Visible = true;
            workshopLookUp.View.Columns[0].Width = 220;

            gridDuties.RepositoryItems.AddRange([roleLookUp, workshopLookUp]);

            viewDuties.Columns.Clear();

            viewDuties.Columns.Add(new GridColumn
            {
                Caption = "Yetkili Görevi",
                ColumnEdit = roleLookUp,
                FieldName = nameof(EmployeeDutyDto.SigningRole),
                Visible = true,
                Width = 220
            });

            viewDuties.Columns.Add(new GridColumn
            {
                Caption = "Atölye",
                ColumnEdit = workshopLookUp,
                FieldName = nameof(EmployeeDutyDto.WorkshopId),
                Visible = true,
                Width = 180
            });

            viewDuties.Columns.Add(new GridColumn
            {
                Caption = "Aktif",
                FieldName = nameof(EmployeeDutyDto.IsActive),
                OptionsColumn = { FixedWidth = true },
                Visible = true,
                Width = 70
            });

gridDuties.DataSource = _duties;

            UpdateDutyStatus();
        }

        private void Populate(EmployeeDto employee)
        {
            txtIdentityNumber.Text = employee.IdentityNumber;
            txtFirstName.Text = employee.FirstName;
            txtLastName.Text = employee.LastName;
            txtTitle.Text = employee.Title;
            txtPhone1.Text = employee.PhoneNumber1;
            txtPhone2.Text = employee.PhoneNumber2;
            txtEmail.Text = employee.Email;
            chkActive.Checked = employee.IsActive;

            // _duties alan readonly BindingList; mevcut görevler aynı kapsama
            // kopyalanır, böylece grid veri kaynağı değişmeden kalır.
            _duties.RaiseListChangedEvents = false;
            _duties.Clear();

            foreach (EmployeeDutyDto duty in employee.Duties)
            {
                _duties.Add(duty);
            }

            _duties.RaiseListChangedEvents = true;
            gridDuties.DataSource = _duties;

            UpdateDutyStatus();
        }

        private void BtnAddDutyShortcut_Click(object? sender, EventArgs e)
        {
            tabMain.SelectedTabPage = tabDuties;

            // Kullanıcı ilk sekmeden geldiği için satır eklemeyi de biz
            // tetikleriz; aksi hâlde "Görev Ekle" düğmesine tekrar basması
            // gerekirdi.
            BtnAddDuty_Click(sender, e);

viewDuties.Focus();
        }

        private void BtnAddDuty_Click(object? sender, EventArgs e)
        {
            _duties.Add(new EmployeeDutyDto { SigningRole = EmployeeSigningRole.AccountingOfficer, IsActive = true });
            UpdateDutyStatus();
        }

        private void BtnRemoveDuty_Click(object? sender, EventArgs e)
        {
            if (viewDuties.FocusedRowHandle < 0
                || viewDuties.GetRow(viewDuties.FocusedRowHandle) is not EmployeeDutyDto duty)
            {
                ToastHelper.Show("Silinecek görev satırını seçin.", ToastType.Warning);
                return;
            }

            _duties.Remove(duty);
            UpdateDutyStatus();
        }

        /// <summary>
        /// Kişisel Bilgiler sekmesindeki canlı görev durumunu, sekme başlığını
        /// ve ilerleme metnini tazeler. Görev zorunlu olduğu için kullanıcı
        /// hatanın kaynağını ilk sekmeden görebilmelidir.
        /// </summary>
        private void UpdateDutyStatus()
        {
            int count = _duties.Count;

            tabDuties.Text = count > 0
                ? $"Yetkili Görevler ({count})"
                : "Yetkili Görevler *";

            if (count > 0)
            {
                string names = string.Join(", ", _duties
                    .Select(d => EnumDisplay.GetDisplayName(d.SigningRole))
                    .Distinct());

                lblNote.Text = $"Yetkili görev tanımlandı: {names}.";
                lblNote.Appearance.ForeColor = SkinTheme.MutedText(_surfaceMuted());
                btnAddDutyShortcut.Text = "Görev Ekle";
            }
            else
            {
                lblNote.Text =
                    "ZORUNLU: En az bir yetkili görev tanımlayın. "
                    + "Görevi olmayan personel rapor imza bloklarında yer alamaz.";
                lblNote.Appearance.ForeColor = SkinTheme.Danger;
                btnAddDutyShortcut.Text = "Görev Ekle";
                btnAddDutyShortcut.Focus();
            }

            lblDutyNote.Text = count > 0
                ? $"{count} görev tanımlı."
                : "Raporlar bu görevlere göre imza yetkilisi bulur.";
        }

        private static Color _surfaceMuted() => SkinTheme.SurfaceMuted(SkinTheme.HighContrastSurface);

        private void BtnAddPhoto_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Görseller (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Multiselect = false,
                Title = "Personel Fotoğrafı Seç"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string file = dialog.FileName;
            string fileName = Path.GetFileName(file);

            _photo = new PhotoInput(fileName, GetContentType(fileName), File.ReadAllBytes(file), true);

            using var memory = new MemoryStream(_photo.Data);
            picPhoto.Image = Image.FromStream(memory);
        }

        private void BtnRemovePhoto_Click(object? sender, EventArgs e)
        {
            if (_photo is null && string.IsNullOrWhiteSpace(_editing?.PhotoPath))
            {
                ToastHelper.Show("Kaldırılacak fotoğraf yok.", ToastType.Warning);
                return;
            }

            _photo = null;
            picPhoto.Image = null;
        }

        private void LoadPhotoFromPath(string relativePath)
        {
            string fullPath = StorageRoot.Resolve(relativePath);

            if (!File.Exists(fullPath))
            {
                return;
            }

            string fileName = Path.GetFileName(relativePath);
            byte[] data = File.ReadAllBytes(fullPath);

            _photo = new PhotoInput(fileName, GetContentType(fileName), data, true);

            using var memory = new MemoryStream(data);
            picPhoto.Image = Image.FromStream(memory);
        }

        private static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".jpeg" or ".jpg" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            // Kaldırılan fotoğrafın baytları null olur; handler "Photo yoksa
            // mevcut yolu koru" davranışını uygular.
            PhotoInput? photo = _photo;

            List<EmployeeDutyInput> duties =
            [
                .. _duties.Select(d => new EmployeeDutyInput(
                    d.SigningRole,
                    d.WorkshopId,
                    d.IsActive))
            ];

            string identityNumber = txtIdentityNumber.Text.Replace(" ", string.Empty).Replace("-", string.Empty).Trim();

            IRequest<Result<Guid>> command = _editing is null
                ? new EmployeeCreateCommand(
                    txtFirstName.Text.Trim(),
                    txtLastName.Text.Trim(),
                    identityNumber,
                    txtTitle.Text.Trim(),
                    txtPhone1.Text.Trim(),
                    txtPhone2.Text.Trim(),
                    txtEmail.Text.Trim(),
                    photo,
                    chkActive.Checked,
                    duties)
                : new EmployeeUpdateCommand(
                    _editing.Id,
                    txtFirstName.Text.Trim(),
                    txtLastName.Text.Trim(),
                    identityNumber,
                    txtTitle.Text.Trim(),
                    txtPhone1.Text.Trim(),
                    txtPhone2.Text.Trim(),
                    txtEmail.Text.Trim(),
                    photo,
                    chkActive.Checked,
                    duties);

if (!RunApplicationValidator(command))
            {
                // Görev eksikliği ilk sekmede görünmüyordu; kullanıcıyı doğrudan
                // sorunun olduğu sekmeye alıyoruz.
                if (_duties.Count == 0)
                {
                    tabMain.SelectedTabPage = tabDuties;
                    btnAddDuty.Focus();
                }

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

        /// <summary>
        /// Uygulama katmanı doğrulamasını form üzerinde çalıştırır; hatalar
        /// ilgili alanın altında gösterilir. Böylece komut gönderilmeden
        /// önce kullanıcı eksik alanı görür.
        /// </summary>
        private bool RunApplicationValidator(object command)
        {
            ClearFieldErrors();

            ValidationResult? result = command switch
            {
                EmployeeCreateCommand create => new EmployeeCreateCommandValidator().Validate(create),
                EmployeeUpdateCommand update => new EmployeeUpdateCommandValidator().Validate(update),
                _ => null
            };

            if (result is null || result.IsValid)
            {
                return true;
            }

            (string Property, BaseEdit Editor)[] map =
            [
                (EmployeeCommandFields.FirstName, txtFirstName),
                (EmployeeCommandFields.LastName, txtLastName),
                (EmployeeCommandFields.IdentityNumber, txtIdentityNumber),
                (EmployeeCommandFields.Title, txtTitle),
                (EmployeeCommandFields.PhoneNumber1, txtPhone1)
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

            (string, BaseEdit) firstInvalid =
                map.FirstOrDefault(m => result.Errors.Any(e => e.PropertyName == m.Property));

            firstInvalid.Item2?.Focus();

            ToastHelper.Show(string.Join(Environment.NewLine, messages), ToastType.Warning, 6000);

            return false;
        }

        private void ClearFieldErrors()
        {
            txtFirstName.ErrorText = string.Empty;
            txtLastName.ErrorText = string.Empty;
            txtIdentityNumber.ErrorText = string.Empty;
            txtTitle.ErrorText = string.Empty;
            txtPhone1.ErrorText = string.Empty;
            txtPhone2.ErrorText = string.Empty;
            txtEmail.ErrorText = string.Empty;
        }

        /// <summary>Seçim kutusunda enum adını görünür metin olarak gösterir.</summary>
        private sealed record SigningRoleOption(EmployeeSigningRole Value, string Display);

        private static class EmployeeCommandFields
        {
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string IdentityNumber = "IdentityNumber";
            public const string Title = "Title";
            public const string PhoneNumber1 = "PhoneNumber1";
        }
    }
}