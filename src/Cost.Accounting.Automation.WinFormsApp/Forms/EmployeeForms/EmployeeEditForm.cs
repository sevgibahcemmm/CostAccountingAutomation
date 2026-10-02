using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Employees;
using Cost.Accounting.Automation.Application.Employees.SigningRoles;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.SigningRoleForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
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
    /// <para>
    /// Görevler ayrı bir sekmede yönetilir çünkü rapor imza blokları bu
    /// görevlere göre çözülür: aynı kişi birden fazla görev taşıyabilir ve
    /// atölyeye bağlı görevlerde ("Atölye Şefi") atölye seçimi zorunludur.
    /// </para>
    ///
    /// <para>
    /// Görev tanımları veritabanında tutulur (enum değil). Bu yüzden sekmedeki
    /// "Yeni Görev Tanımla" düğmesi, kullanıcı personel kaydederken gerekirse
    /// listeye yeni bir yetkili görev ekleyebilir; eklenen görev aynı anda
    /// seçim kutusuna düşer.
    /// </para>
    /// </summary>
    public partial class EmployeeEditForm : XtraForm
    {
        private readonly EmployeeDto? _editing;
        private readonly IDisposable? _skinBinding;

        /// <summary>
        /// Görev satırları <see cref="BindingList{T}"/> olarak tutulur; sıradan
        /// <c>List</c> kullanılsaydı "Görev Ekle" düğmesi listeye ekleyecek ama
        /// grid satırı göstermeyecek ve kullanıcı boş formu kaydetmeye
        /// çalışacaktı. CostSlipEditForm'deki malzeme satırları da aynı
        /// nedenle BindingList kullanır.
        /// </summary>
        private readonly BindingList<EmployeeDutyDto> _duties = [];

        private List<ChartOfAccountLookUpDto> _workshops = [];
        private List<EmployeeSigningRoleOption> _signingRoles = [];
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

            chkActive.IsOn = _editing?.IsActive ?? true;

            tabPersonal.ImageOptions.SvgImage = DxIcon.IdCard;
            tabPersonal.ImageOptions.SvgImageSize = new Size(16, 16);
            tabDuties.ImageOptions.SvgImage = DxIcon.Roles;
            tabDuties.ImageOptions.SvgImageSize = new Size(16, 16);
            tabPhotos.ImageOptions.SvgImage = DxIcon.Photo;
            tabPhotos.ImageOptions.SvgImageSize = new Size(16, 16);

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
            btnAddPhoto.Click += BtnAddPhoto_Click;
            btnRemovePhoto.Click += BtnRemovePhoto_Click;
            btnAddDutyShortcut.Click += BtnAddDutyShortcut_Click;
            btnAddDuty.Click += BtnAddDuty_Click;
            btnRemoveDuty.Click += BtnRemoveDuty_Click;
            btnNewSigningRole.Click += BtnNewSigningRole_Click;

            // TC kimlik no yalnızca 11 rakam kabul eder ve 3-3-3-2 gruplanır;
            // telefon alanları yazarken okunur biçime döner ve 11 rakamla sınırlıdır.
            IdentityNumberMask.Attach(txtIdentityNumber);
            PhoneNumberFormatter.Attach(txtPhone1);
            PhoneNumberFormatter.Attach(txtPhone2);

            txtIdentityNumber.TextChanged += (_, _) => UpdateIdentityHint();

            ApplySkin();
            _skinBinding = SkinTheme.Bind(ApplySkin);

            Load += EmployeeEditForm_Load;
        }

        /// <summary>
        /// TC kimlik no alanının altındaki canlı hane sayacını tazeler.
        ///
        /// <para>
        /// Numaralar gerçek nüfus kaydı olmadığı için kontrol hanesi
        /// doğrulanmaz; tek kural 11 hanelik rakam olmaktır. Sayaç, kullanıcı
        /// 11 haneye ulaştığında bunu yeşil renkle bildirir, eksik hane
        /// kaldığında hangi hanenin eksik olduğunu gösterir.
        /// </para>
        /// </summary>
        private void UpdateIdentityHint()
        {
            string digits = EmployeeIdentityNumber.Normalize(txtIdentityNumber.Text);
            int typed = digits.Length;

            lblIdentityHint.Appearance.Options.UseForeColor = true;

            if (typed >= EmployeeIdentityNumber.Length)
            {
                lblIdentityHint.Text = $"✓ {EmployeeIdentityNumber.Length} hane tamam";
                lblIdentityHint.Appearance.ForeColor = SkinTheme.Success;
                return;
            }

            lblIdentityHint.Text = $"{typed} / {EmployeeIdentityNumber.Length} hane";
            lblIdentityHint.Appearance.ForeColor = SkinTheme.MutedText(
                SkinTheme.SurfaceMuted(SkinTheme.HighContrastSurface));
        }

        /// <summary>
        /// Tüm renkler aktif skinden çözülür; skin değişiminde yeniden uygulanır.
        /// Sabit renkler koyu temalarda okunmaz olduğu için başlık, alt bar,
        /// ayırıcılar, etiketler ve fotoğraf zemini burada tek yerden boyanır.
        /// </summary>
        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(this);
            Color mutedSurface = SkinTheme.SurfaceMuted(surface);

            // Alan etiketleri zeminin çok koyu tonuna çekilmez; neredeyse tam
            // metin rengi kullanılır ki koyu temada da okunur kalsın.
            Color labelColor = SkinTheme.Blend(SkinTheme.Text, surface, 0.15F);

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

            LabelControl[] fieldLabels =
            [
                lblIdentityNumber,
                lblTitleField,
                lblFirstName,
                lblLastName,
                lblPhone1,
                lblPhone2,
                lblEmail
            ];

            foreach (LabelControl label in fieldLabels)
            {
                label.Appearance.ForeColor = labelColor;
                label.Appearance.Options.UseForeColor = true;
            }

            lblDutyNote.Appearance.ForeColor = labelColor;
            lblDutyNote.Appearance.Options.UseForeColor = true;

            // Fotoğraf önizlemesi düz zeminden ayırt edilsin.
            picPhoto.Properties.Appearance.BackColor = SkinTheme.SurfaceReadOnly(surface);
            picPhoto.Properties.Appearance.Options.UseBackColor = true;

            // lblIdentityHint rengini UpdateIdentityHint yönetir.
            UpdateIdentityHint();

            // lblNote rengini ApplySkin değil UpdateDutyStatus yönetir.
            UpdateDutyStatus();
        }

        private async void EmployeeEditForm_Load(object? sender, EventArgs e)
        {
            btnSave.Enabled = false;

            try
            {
                await LoadLookupsAsync();

                if (_editing is not null)
                {
                    EmployeeDto record = await LoadEmployeeWithDutiesAsync(_editing);

                    Populate(record);

                    if (!string.IsNullOrWhiteSpace(record.PhotoPath))
                    {
                        LoadPhotoFromPath(record.PhotoPath);
                    }
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("EmployeeEditForm.Load", ex);
                ToastHelper.Show(
                    "Atölye / yetkili görev listesi yüklenemedi: " + ex.Message,
                    ToastType.Error,
                    4000);
                Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        /// <summary>
        /// Düzenleme için kaydı görev detayıyla birlikte getirir.
        ///
        /// <para>
        /// Liste ekranı <see cref="EmployeeExtensions.MapTo"/> projeksiyonunu
        /// kullanır; projeksiyon yalnızca özet sütunlarını (özet görev metni,
        /// atölye metni, görev sayısı) doldurur ve görev satırlarını
        /// (<c>EmployeeDto.Duties</c>) taşımaz. Bu yüzden liste ekranından
        /// gelen kayıtta görevler boştur; ayrıntılı kayıt
        /// <see cref="EmployeeGetQuery"/> ile yeniden okunur.
        /// </para>
        /// </summary>
        private async Task<EmployeeDto> LoadEmployeeWithDutiesAsync(EmployeeDto summary)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(new EmployeeGetQuery(summary.Id), CancellationToken.None);

                if (result.IsSuccessful && result.Data is not null)
                {
                    return result.Data;
                }

                CrashLog.Write("EmployeeEditForm.Load", $"Görevler yüklenemedi (Id={summary.Id})");
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("EmployeeEditForm.LoadDuties", ex);
            }

            // Kayıt okunamazsa liste ekranındaki özetle devam et; kullanıcı
            // kişisel bilgileri kaybedip görevleri elle yeniden girebilsin.
            return summary;
        }


        private async Task LoadLookupsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None);

            _workshops = result.IsSuccessful && result.Data is not null
                ? [.. result.Data.Where(a => a.Type == ChartOfAccountType.Workshop)]
                : [];

            _signingRoles = await LoadSigningRolesAsync(mediator);

            ConfigureDutyGrid();
        }

        /// <summary>Seçim kutusunu besleyen aktif yetkili görev tanımlarını getirir.</summary>
        private static async Task<List<EmployeeSigningRoleOption>> LoadSigningRolesAsync(ISender mediator)
        {
            IQueryable<EmployeeSigningRoleOption> roles =
                await mediator.Send(new EmployeeSigningRoleLookUpQuery(), CancellationToken.None);

            return [.. roles];
        }

        /// <summary>
        /// Görev ızgarası düzenlenebilirdir: kullanıcı görevi ve varsa atölyeyi
        /// doğrudan satır üzerinden seçer. Kurum geneli görevlerde atölye
        /// sütunu boş bırakılabilir.
        ///
        /// <para>
        /// Görev listesi veritabanından gelir; enum değildir. Kullanıcı
        /// "Yeni Görev Tanımla" düğmesiyle liste genişletebilir.
        /// </para>
        /// </summary>
        private void ConfigureDutyGrid()
        {
            viewDuties.Columns.Clear();

            RepositoryItemSearchLookUpEdit roleLookUp = new()
            {
                DataSource = _signingRoles,
                DisplayMember = nameof(EmployeeSigningRoleOption.Name),
                NullText = "Görev seçiniz...",
                PopupFilterMode = PopupFilterMode.Contains,
                ValueMember = nameof(EmployeeSigningRoleOption.Id)
            };

            roleLookUp.View.OptionsBehavior.AutoPopulateColumns = false;
            roleLookUp.View.Columns.Clear();
            roleLookUp.View.Columns.AddField(nameof(EmployeeSigningRoleOption.Name)).Caption = "Görev";
            roleLookUp.View.Columns.AddField(nameof(EmployeeSigningRoleOption.Description)).Caption = "Açıklama";
            roleLookUp.View.Columns[0].Visible = true;
            roleLookUp.View.Columns[0].Width = 200;
            roleLookUp.View.Columns[1].Visible = true;
            roleLookUp.View.Columns[1].Width = 240;

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
                FieldName = nameof(EmployeeDutyDto.SigningRoleId),
                Visible = true,
                Width = 300
            });

            viewDuties.Columns.Add(new GridColumn
            {
                Caption = "Atölye",
                ColumnEdit = workshopLookUp,
                FieldName = nameof(EmployeeDutyDto.WorkshopId),
                Visible = true,
                Width = 280
            });

            viewDuties.Columns.Add(new GridColumn
            {
                Caption = "Aktif",
                FieldName = nameof(EmployeeDutyDto.IsActive),
                OptionsColumn = { FixedWidth = true },
                Visible = true,
                Width = 80
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
            chkActive.IsOn = employee.IsActive;

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

        /// <summary>
        /// Yeni bir görev satırı ekler ve o satırın görev hücresine odaklanır.
        /// Görev bilinçli olarak boş bırakılır: geçerli bir görev seçilmeden
        /// kaydedilirse doğrulama hatası döner; rastgele bir görevi varsayılan
        /// atamak yanlış yetkili atamaya yol açardı.
        /// </summary>
        private void BtnAddDuty_Click(object? sender, EventArgs e)
        {
            _duties.Add(new EmployeeDutyDto { IsActive = true });
            UpdateDutyStatus();

            viewDuties.FocusedRowHandle = viewDuties.RowCount - 1;
            viewDuties.Focus();

            GridColumn? roleColumn =
                viewDuties.Columns.ColumnByFieldName(nameof(EmployeeDutyDto.SigningRoleId));

            if (roleColumn is not null)
            {
                viewDuties.FocusedColumn = roleColumn;
                viewDuties.ShowEditor();
            }
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
        /// Yetkili görev listesine yeni bir tanım ekler. Kullanıcı personel
        /// kaydederken ihtiyaç duyduğu görevi kod değiştirmeden tanımlayabilsin
        /// diye bu kısayol görev sekmesinde durur; kayıt tamamlandığında seçim
        /// kutusu tazelenir.
        /// </summary>
        private async void BtnNewSigningRole_Click(object? sender, EventArgs e)
        {
            btnNewSigningRole.Enabled = false;

            try
            {
                using EmployeeSigningRoleEditForm editor = new();

                if (editor.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                await RefreshSigningRolesAsync();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("EmployeeEditForm.NewSigningRole", ex);
                ToastHelper.Show("Görev tanımlanamadı: " + ex.Message, ToastType.Error, 5000);
            }
            finally
            {
                btnNewSigningRole.Enabled = true;
            }
        }

        /// <summary>
        /// Görev seçim kutusunu veritabanından yeniler. Girilmiş satırlar
        /// korunur; yalnızca arama kaynağı değişir.
        /// </summary>
        private async Task RefreshSigningRolesAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            List<EmployeeSigningRoleOption> roles = await LoadSigningRolesAsync(mediator);

            // Lookup editör veri kaynağına yeni bir liste atanmalı; aynı liste
            // örneğini Clear/Add ile değiştirmek editörü tazelemez.
            _signingRoles = roles;

            if (viewDuties.Columns[nameof(EmployeeDutyDto.SigningRoleId)]?.ColumnEdit
                is RepositoryItemSearchLookUpEdit roleLookUp)
            {
                roleLookUp.DataSource = _signingRoles;
            }

            viewDuties.RefreshData();

            ToastHelper.Show(
                "Yeni yetkili görev tanımlandı; artık görev satırında seçebilirsiniz.",
                ToastType.Success,
                3500);
        }

        /// <summary>
        /// Görev durumunu üç yerde birden tazeler: sekme başlığı (yıldız işareti),
        /// Kişisel Bilgiler sekmesindeki özet not ve görev sekmesindeki sayaç.
        /// Görev zorunlu olduğu için kullanıcı hatanın kaynağını ilk sekmeden
        /// görebilmelidir.
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
                    .Select(d => string.IsNullOrWhiteSpace(d.SigningRoleName)
                        ? "(seçilmedi)"
                        : d.SigningRoleName)
                    .Distinct());

                lblNote.Text = $"Tanımlı görevler: {names}.";
                lblNote.Appearance.ForeColor = SkinTheme.MutedText(SkinTheme.SurfaceMuted(SkinTheme.HighContrastSurface));
            }
            else
            {
                lblNote.Text =
                    "ZORUNLU: En az bir yetkili görev tanımlayın. Görevi olmayan personel "
                    + "rapor imza bloklarında yer alamaz.";
              //  lblNote.Appearance.ForeColor = SkinTheme.Danger;
            }

            lblNote.Appearance.Options.UseForeColor = true;

            lblDutyNote.Text = count > 0
                ? $"{count} görev tanımlı."
                : "Henüz görev yok. En az bir tane ekleyin.";
        }

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
                    d.SigningRoleId,
                    d.WorkshopId,
                    d.IsActive))
            ];

            // TC kimlik no kayıtta düz rakam olarak saklanır; ekrandaki
            // "123 456 789 01" biçimi yalnızca görseldir. Normalize, yapıştırma
            // ve programatik atama yollarındaki ayraçları da temizler.
            string identityNumber = EmployeeIdentityNumber.Normalize(txtIdentityNumber.Text);

            // Telefon okunabilir biçimiyle saklanır ("0 (216) 123 45 01"); aynı
            // biçim kayıt açıldığında da yeniden üretildiği için değişmez.
            string phone1 = txtPhone1.Text.Trim();
            string phone2 = txtPhone2.Text.Trim();

            IRequest<Result<Guid>> command = _editing is null
                ? new EmployeeCreateCommand(
                    txtFirstName.Text.Trim(),
                    txtLastName.Text.Trim(),
                    identityNumber,
                    txtTitle.Text.Trim(),
                    phone1,
                    phone2,
                    txtEmail.Text.Trim(),
                    photo,
                    chkActive.IsOn,
                    duties)
                : new EmployeeUpdateCommand(
                    _editing.Id,
                    txtFirstName.Text.Trim(),
                    txtLastName.Text.Trim(),
                    identityNumber,
                    txtTitle.Text.Trim(),
                    phone1,
                    phone2,
                    txtEmail.Text.Trim(),
                    photo,
                    chkActive.IsOn,
                    duties);

            if (!RunApplicationValidator(command))
            {
                // Görev eksikliği ilk sekmede görünmüyordu; kullanıcıyı doğrudan
                // sorunun olduğu sekmeye alıyoruz.
                if (_duties.Count == 0
                    || _duties.Any(d => d.SigningRoleId is not Guid roleId || roleId == Guid.Empty))
                {
                    tabMain.SelectedTabPage = tabDuties;
                    btnAddDuty.Focus();
                }

                return;
            }

            // Atölye kuralları görev tanımından okunduğu için senkron
            // doğrulamada kontrol edilemez; veritabanından yüklenen görev
            // bilgisiyle burada bir kez daha bakılır. Hem atölye zorunluluğu
            // hem de kurum geneli göreve atölye atanamaması denetlenir.
            string? dutyProblem = EmployeeDutyValidator.FindProblem(
                duties, BuildDutyRoleInfos());

            if (dutyProblem is not null)
            {
                tabMain.SelectedTabPage = tabDuties;
                ToastHelper.Show(dutyProblem, ToastType.Warning, 5000);
                viewDuties.Focus();
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

        /// <summary>
        /// Görev kimliği → görev tanımı eşlemesi. Görev kayıtları veritabanında
        /// olduğu için atölye kuralları (zorunlu mu, seçilebilir mi) bu listeden
        /// okunur.
        /// </summary>
        private Dictionary<Guid, EmployeeDutyRoleInfo> BuildDutyRoleInfos()
            => _signingRoles.ToDictionary(
                r => r.Id,
                r => new EmployeeDutyRoleInfo(r.Id, r.Name, r.RequiresWorkshop));

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