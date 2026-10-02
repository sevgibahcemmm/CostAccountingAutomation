using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Cost.Accounting.Automation.Application.Employees;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.EmployeeForms
{
    /// <summary>
    /// Personel listesi. Rapor imza bloklarındaki yetkiler buradan belirlendiği
    /// için "Görevleri" ve "Atölyeler" sütunları liste ekranında görünür durumdadır.
    /// </summary>
    public sealed partial class EmployeesListForm : CrudListFormBase<EmployeeGetAllQuery, EmployeeDto, EmployeeEditForm>
    {
        public EmployeesListForm() : base("Personel")
        {
            InitializeComponent();
            View.CustomUnboundColumnData += EmployeesListForm_CustomUnboundColumnData;
        }

        protected override SvgImage ModuleIcon => DxIcon.Employees;

        protected override string[] SearchFieldNames =>
        [
            nameof(EmployeeDto.IdentityNumber),
            nameof(EmployeeDto.FullName),
            nameof(EmployeeDto.Title),
            nameof(EmployeeDto.SigningRoles),
            nameof(EmployeeDto.Workshops),
            nameof(EmployeeDto.PhoneNumber1),
            nameof(EmployeeDto.Email)
        ];

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();
            AddColumnsFromAttributes();

            // Fotoğraf sütunu, listede avatar olarak gösterilir. Dosya yolu
            // değil, yüklenmiş görsel nesnesi gerektiği için unbound kolon
            // ve CustomUnboundColumnData kullanılır.
            GridColumn photoColumn = new()
            {
                Caption = "Foto",
                ColumnEdit = new RepositoryItemPictureEdit
                {
                    SizeMode = PictureSizeMode.Zoom
                },
                FieldName = "PhotoPreview",
                OptionsColumn = { FixedWidth = true },
                UnboundType = UnboundColumnType.Object,
                Visible = true,
                VisibleIndex = 0,
                Width = 60
            };

View.Columns.Add(photoColumn);

            ConfigureDutyMasterDetail();
        }

        /// <summary>
        /// Üst gridde seçili personelin görev-atölye eşleşmesini gösteren alt grid.
        ///
        /// <para>
        /// Rapor imza bloklarındaki yetkiler liste ekranından okunduğu için
        /// "hangi görev, hangi atölyede" bilgisi satır açılmadan görülemiyordu.
        /// Görev kayıtları zaten <see cref="EmployeeDto.Duties"/> içinde geldiği
        /// için ek sorgu yapılmaz; alt grid veriyi doğrudan üst satırdan alır.
        /// </para>
        ///
        /// <para>
        /// Bu DevExpress sürümünde master-detail <c>MasterDetails</c>
        /// koleksiyonu yerine olaylarla kurulur: kaç alt görünüm olacağı, adı
        /// ve satır içeriği ayrı ayrı bildirilir. Görevi olmayan personelde
        /// ilişki sayısı sıfır döndürülür; böylece o satırda "+" düğmesi
        /// hiç görünmez.
        /// </para>
        /// </summary>
        private void ConfigureDutyMasterDetail()
        {
            View.OptionsDetail.AllowOnlyOneMasterRowExpanded = true;
            View.OptionsDetail.SmartDetailExpandButtonMode = DetailExpandButtonMode.AlwaysEnabled;

            View.MasterRowGetRelationCount += (_, e) =>
                e.RelationCount = View.GetRow(e.RowHandle) is EmployeeDto { Duties.Count: > 0 }
                    ? 1
                    : 0;

            View.MasterRowGetRelationName += (_, e) => e.RelationName = "Görevler";

            View.MasterRowGetChildList += (_, e) =>
            {
                if (View.GetRow(e.RowHandle) is EmployeeDto employee)
                {
                    e.ChildList = new List<EmployeeDutyDto>(employee.Duties);
                }
            };

            View.MasterRowExpanded += (_, e) => ConfigureDutyDetailView(e.RowHandle);
        }

        /// <summary>
        /// Alt grid DevExpress tarafından ilk genişletmede üretilir; sütunlar
        /// bir kez burada kurulur.
        /// </summary>
        private void ConfigureDutyDetailView(int rowHandle)
        {
            if (_dutyDetailView is not null
                || View.GetVisibleDetailView(rowHandle) is not GridView detailView)
            {
                return;
            }

            _dutyDetailView = detailView;

            detailView.OptionsBehavior.AutoPopulateColumns = false;
            detailView.OptionsBehavior.Editable = false;
            detailView.OptionsView.ShowGroupPanel = false;
            detailView.OptionsView.ShowIndicator = false;
            detailView.OptionsView.EnableAppearanceEvenRow = true;
            detailView.OptionsView.EnableAppearanceOddRow = true;

            detailView.Columns.Clear();

            GridColumn roleColumn = detailView.Columns.AddField(nameof(EmployeeDutyDto.SigningRoleName));
            roleColumn.Caption = "Görev";
            roleColumn.Width = 260;

            GridColumn workshopColumn = detailView.Columns.AddField(nameof(EmployeeDutyDto.WorkshopName));
            workshopColumn.Caption = "Atölye";
            workshopColumn.Width = 240;

            GridColumn statusColumn = detailView.Columns.AddField(nameof(EmployeeDutyDto.IsActive));
            statusColumn.Caption = "Durum";
            statusColumn.Width = 90;

            detailView.CustomUnboundColumnData += EmployeesListForm_CustomDutyStatusData;
        }

        /// <summary>
        /// Alt gridde bool alanı checkbox olarak görünmesin diye metne çevirir.
        /// </summary>
        private void EmployeesListForm_CustomDutyStatusData(object? sender, CustomColumnDataEventArgs e)
        {
            if (e.Column.FieldName != nameof(EmployeeDutyDto.IsActive) || !e.IsGetData)
            {
                return;
            }

            if (sender is not GridView dutyGrid
                || e.ListSourceRowIndex < 0
                || dutyGrid.GetRow(e.ListSourceRowIndex) is not EmployeeDutyDto duty)
            {
                return;
            }

            e.Value = duty.IsActive ? "Aktif" : "Pasif";
        }

private static readonly Dictionary<Guid, Image> _imageCache = [];
        private static readonly object _imageCacheLock = new();

        /// <summary>Alt gridin sütunları yalnızca bir kez kurulur.</summary>
        private GridView? _dutyDetailView;

        private void EmployeesListForm_CustomUnboundColumnData(object? sender, CustomColumnDataEventArgs e)
        {
            if (e.Column.FieldName != "PhotoPreview" || !e.IsGetData)
            {
                return;
            }

            if (e.ListSourceRowIndex < 0 || View.GetRow(e.ListSourceRowIndex) is not EmployeeDto employee)
            {
                return;
            }

            e.Value = BuildEmployeeImage(employee);
        }

        private static Image BuildEmployeeImage(EmployeeDto employee)
        {
            lock (_imageCacheLock)
            {
                if (_imageCache.TryGetValue(employee.Id, out Image? cached))
                {
                    return cached;
                }

                Image image = TryLoadPhoto(employee.PhotoPath) ?? CreateAvatar(employee);
                _imageCache[employee.Id] = image;

                return image;
            }
        }

        private static Image? TryLoadPhoto(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            try
            {
                string fullPath = StorageRoot.Resolve(relativePath);

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                using var memory = new MemoryStream();
                stream.CopyTo(memory);

                return new Bitmap(memory);
            }
            catch
            {
                // Bozuk veya erişilemeyen görsel listede bozuk görünmemeli.
                return null;
            }
        }

        /// <summary>Fotoğrafı olmayan personel için baş harflerden avatar üretir.</summary>
        private static Bitmap CreateAvatar(EmployeeDto employee)
        {
            string initials = GetInitials(employee);

            Bitmap bitmap = new(60, 60);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using var brush = new SolidBrush(SkinTheme.Primary);
                graphics.FillEllipse(brush, 0, 0, 60, 60);

                using var textBrush = new SolidBrush(SkinTheme.GetContrastText(SkinTheme.Primary));
                using var font = new Font("Segoe UI", 20, FontStyle.Bold);

                SizeF size = graphics.MeasureString(initials, font);
                graphics.DrawString(initials, font, textBrush, (60 - size.Width) / 2, (60 - size.Height) / 2);
            }

            return bitmap;
        }

        private static string GetInitials(EmployeeDto employee)
        {
            string first = employee.FirstName.Length > 0 ? employee.FirstName[..1].ToUpperInvariant() : string.Empty;
            string last = employee.LastName.Length > 0 ? employee.LastName[..1].ToUpperInvariant() : string.Empty;

            string initials = first + last;

            return initials.Length > 0 ? initials : "?";
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(EmployeeDto item)
            => new EmployeeDeleteCommand(item.Id);

        protected override string GetDeleteSummary(EmployeeDto item)
            => $"{item.FullName} ({item.IdentityNumber})";

        protected override bool SupportsRestore => true;

        protected override EmployeeGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(EmployeeDto item)
            => new EmployeeRestoreCommand(item.Id);

        /// <summary>
        /// Fotoğraf nesneleri hemen serbest bırakılmaz; grid satırları yeniden
        /// boyanırken kullanılıyor olabilir. Bu yüzden liste her yenilendiğinde
        /// önbellek temizlenir ve form kapanırken de serbest bırakılır.
        /// </summary>
        protected override async Task ReloadAsync()
        {
            ClearImageCache();
            await base.ReloadAsync();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ClearImageCache();
            base.OnFormClosed(e);
        }

        private static void ClearImageCache()
        {
            lock (_imageCacheLock)
            {
                foreach (Image image in _imageCache.Values)
                {
                    image.Dispose();
                }

                _imageCache.Clear();
            }
        }
    }
}