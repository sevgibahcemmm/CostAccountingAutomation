using System.Drawing;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using DomainEntityDto = Cost.Accounting.Automation.Domain.Abstractions.EntityDto;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public abstract partial class CrudListFormBase<TListQuery, TDto, TEditForm> : XtraFormMdiBase
        where TListQuery : class, IRequest<IQueryable<TDto>>, new()
        where TDto : DomainEntityDto
        where TEditForm : XtraForm
    {
        protected List<TDto> _allItems = [];

        private GridColumn? _warehouseGroupColumn;
        private bool _showDeleted;
        private bool _isFilterSetting;
        private int _reloadVersion;
        private bool _columnsFitted;
        private readonly IDisposable? _skinBinding;

        protected CrudListFormBase(string formTitle) : base(formTitle)
        {
            InitializeComponent();
            lblTitle.Text = formTitle;
            IconOptions.SvgImage = ModuleIcon;
            WireEvents();
            SetupGrid();
            SetupButtonIcons();
            picModuleIcon.SvgImage = ModuleIcon;
            ApplySkin();
            _skinBinding = SkinTheme.Bind(ApplySkin);
        }

        /// <summary>
        /// Başlık ve ayırıcı renkleri aktif skinden çözülür; böylece açık ve koyu
        /// temalarda başlık metni zeminin üzerinde okunur kalır. Butonlara renk
        /// atanmaz — onların görünümü tamamen DevExpress skin'ine bırakılır.
        ///
        /// <para>
        /// Araç çubuğu paneli (<see cref="pnlToolbar"/>) bilerek şeffaftır: buton
        /// bandının arkasında ayrı bir renk şeridi görünmesin, butonlar doğrudan
        /// formun zemininde dursun. Bu yüzden panel zemin rengi formun kendi
        /// zeminine bırakılır (<c>Color.Transparent</c>); arama kutusu gibi panel
        /// içindeki düzenleyiciler kendi zeminlerini korur.
        /// </para>
        /// </summary>
        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(pnlHeader);
            Color primary = SkinTheme.Text;
            Color secondary = SkinTheme.Blend(primary, surface, 0.22F);

            pnlHeader.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
            ApplyTransparentToolbar();
            headerAccent.Appearance.BackColor = SkinTheme.Primary;
            headerDivider.Appearance.BackColor = SkinTheme.BorderMuted(surface);

            headerAccent.Appearance.Options.UseBackColor = true;
            headerDivider.Appearance.Options.UseBackColor = true;

            lblTitle.Appearance.ForeColor = primary;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblSub.Appearance.ForeColor = secondary;
            lblSub.Appearance.Options.UseForeColor = true;
            lblFilter.Appearance.ForeColor = secondary;
            lblFilter.Appearance.Options.UseForeColor = true;

            // Satır renkleri skin'in zemininden türetildiği için tema değişince
            // yeniden hesaplanmalı ve grid yeniden boyanmalıdır; aksi hâlde eski
            // zemine karışık tonlar kalır ve koyu temada okunmaz hale gelir.
            _approvedRowColor = null;
            _draftRowColor = null;
            RefreshStatusRowColors();

            if (!gridControl.IsDisposed)
            {
                gridView.RefreshData();
            }
        }

        /// <summary>
        /// Araç çubuğu bandının zeminini kaldırır. Panel hem DevExpress
        /// <c>Appearance</c> hem de WinForms <c>BackColor</c> değerinde şeffaf
        /// yapılır: DevExpress panel boyama işlemi <c>Appearance</c> üzerinden
        /// yürür, alt kontrollerin ise arka planı WinForms tarafında okunur.
        /// </summary>
        private void ApplyTransparentToolbar()
        {
            pnlToolbar.Appearance.BackColor = Color.Transparent;
            pnlToolbar.Appearance.Options.UseBackColor = true;
            pnlToolbar.BackColor = Color.Transparent;
        }

        protected GridView View => gridView;

        protected DevExpress.XtraGrid.GridControl BaseGrid => gridControl;

        protected DevExpress.XtraEditors.PanelControl HeaderPanel => pnlHeader;

        protected DevExpress.XtraEditors.PanelControl ToolbarPanel => pnlToolbar;

        /// <summary>Liste ekranının araç çubuğuna özel bir buton ekler.</summary>
        protected void AddToolbarButton(DevExpress.XtraEditors.SimpleButton button, int? index = null)
        {
            button.Margin = new Padding(0, 0, 6, 0);
            button.Height = 36;
            button.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            button.Appearance.Options.UseFont = true;
            flpToolbar.Controls.Add(button);
            button.Width = MeasureButtonWidth(button);

            if (index is >= 0)
            {
                flpToolbar.Controls.SetChildIndex(button, index.Value);
            }
        }

        /// <summary>
        /// Türetilmiş formun kendi araç çubuğu butonlarını hazırladığı kanca.
        ///
        /// Türetilmiş formların butonları kendi tasarım dosyalarında tanımlanır ve
        /// <c>flpToolbar</c> içine eklenir; bu yüzden butonlar ancak türetilmiş
        /// kurucunun <c>InitializeComponent()</c> çağrısından sonra var olur.
        /// Kurucu bu metodu kendi <c>InitializeComponent()</c> çağrısından hemen
        /// sonra çağırmalıdır.
        /// </summary>
        protected virtual void RegisterDerivedToolbarButtons()
        {
            // Türetilmiş tasarım dosyasının eklediği butonlar taban genişlik
            // hesabından sonra geldiği için yeniden ölçülür.
            AutoSizeToolbarButtons();
        }

        /// <summary>
        /// Tasarım dosyasında <c>flpToolbar</c> içine eklenmiş bir butonun araç
        /// çubuğundaki sırasını belirler.
        /// </summary>
        protected void MoveToolbarButton(Control button, int index)
        {
            if (index >= 0 && index < flpToolbar.Controls.Count)
            {
                flpToolbar.Controls.SetChildIndex(button, index);
            }
        }

        protected virtual SvgImage ModuleIcon => DxIcon.Module;

        /// <summary>Liste ekranında "Yeni" butonunun gösterilip gösterilmeyeceği.</summary>
        protected virtual bool AllowCreate => true;

        /// <summary>
        /// Liste ekranında "Düzenle" butonunun gösterilip gösterilmeyeceği.
        /// Onaylama gibi yalnızca mevcut kayıt üzerinde işlem yapan listelerde
        /// <c>false</c> verilir; aksi halde <see cref="AllowsEdit"/> her kayıt için
        /// <c>false</c> döndüğünden buton hep pasif görünür.
        /// </summary>
        protected virtual bool AllowEdit => true;

        /// <summary>Liste ekranında "Sil" butonunun gösterilip gösterilmeyeceği.</summary>
        protected virtual bool AllowDelete => true;

        /// <summary>Belirli bir kaydın düzenlenip düzenlenemeyeceği.</summary>
        protected virtual bool AllowsEdit(TDto item) => true;

        /// <summary>
        /// Çift tıklamada düzenleme formu açılsın mı? <c>false</c> ise
        /// <see cref="ShowItemDetailAsync"/> çağrılır (salt okunur detay ekranı olan listeler için).
        /// </summary>
        protected virtual bool DoubleClickOpensEditor => true;

        /// <summary>Düzenleme/detay butonunun metni.</summary>
        protected virtual string EditButtonCaption => "Düzenle";

        /// <summary>Düzenleme/detay butonunun ikonu.</summary>
        protected virtual SvgImage EditButtonIcon => DxIcon.Edit;

        /// <summary>
        /// <see cref="DoubleClickOpensEditor"/> <c>false</c> olduğunda çift tıklamada açılacak
        /// açıklayıcı detay ekranı.
        /// </summary>
        protected virtual Task ShowItemDetailAsync(TDto item) => Task.CompletedTask;

        protected virtual bool AllowsDelete(TDto item) => true;

        protected bool ShowDeleted => _showDeleted;

        protected virtual bool SupportsRestore => false;

        protected virtual bool SupportsApprove => false;

        /// <summary>
        /// Sayfa açılışında onay bekleyen kayıt varsa nasıl bilgilendirileceği.
        /// Program genelinde tek tip kullanılır (kalıcı pencere); "Tamam" ile onay
        /// ekranına geçilir.
        /// </summary>
        protected virtual PendingNoticeMode PendingNotice => PendingNoticeMode.MessageBox;

        /// <summary>
        /// Bilgilendirme metninde kullanılacak kayıt adı (örn. "fatura",
        /// "maliyet pusulası").
        /// </summary>
        protected virtual string PendingItemLabel => "kayıt";

        /// <summary>
        /// Listedeki kaydın onay bekleyip beklemediği. Varsayılan olarak
        /// <see cref="AllowsApprove"/> kuralı kullanılır; onay edilebilen kayıt
        /// onay bekleyen kayıttır. Bildirim yalnızca <see cref="SupportsApprove"/>
        /// true iken gösterildiğinden bu kural yalnızca onay ekranlarında anlamlıdır.
        /// </summary>
        protected virtual bool IsPendingApproval(TDto item) => AllowsApprove(item);

        /// <summary>
        /// Bildirim penceresindeki "Tamam" sonrasında açılacak onay ekranının türü.
        /// Ayrı bir onay ekranı olmayan listeler (maliyet pusulası, stok belgesi)
        /// onayı kendi üzerinde yaptığı için <c>null</c> bırakır.
        /// </summary>
        protected virtual Type? PendingApprovalFormType => null;

        /// <summary>
        /// Açılacak onay ekranının başlığı. <c>null</c> ise formun kendi başlığı
        /// kullanılır.
        /// </summary>
        protected virtual string? PendingApprovalFormTitle => null;

        /// <summary>
        /// Başarılı her yüklemeden sonra onay bekleyen kayıtları bildirir.
        ///
        /// Bildirim yalnızca onay akışı olan ekranlarda çalışır; onay desteği
        /// olmayan listelerde (örn. cari hareketler) gösterilmez. "Tamam" ile
        /// kapatıldığında <see cref="PendingApprovalFormType"/> tanımlıysa onay
        /// ekranı açılır.
        /// </summary>
        private void NotifyPendingItems()
        {
            // Onay akışı olmayan ekranlarda "onay bekleyen kayıt" kavramı yoktur;
            // SupportsApprove bunun tek güvenilir göstergesidir.
            if (PendingNotice == PendingNoticeMode.None || !SupportsApprove)
            {
                return;
            }

            List<TDto> pendingItems = _allItems.Where(IsPendingApproval).ToList();
            if (pendingItems.Count == 0)
            {
                return;
            }

            string label = pendingItems.Count == 1 ? PendingItemLabel : PendingItemLabelPlural;
            string message = BuildPendingMessage(pendingItems, label);

            if (PendingNotice == PendingNoticeMode.Toast)
            {
                ToastHelper.Show(message, ToastType.Warning, 5000);
                return;
            }

            MsgBox.Notice(this, message, "Onay Bekleyen Kayıtlar");
            OpenPendingApprovalScreen();
        }

        /// <summary>
        /// Bekleyen kayıtların dökümünü içeren bildirim metnini kurar. Kullanıcı
        /// neyin beklediğini görmek zorunda; yalnızca sayı söylemek hangi kaydın
        /// sıkıştığını anlatmaz.
        /// </summary>
        private string BuildPendingMessage(List<TDto> pendingItems, string label)
        {
            const int maxShown = 5;

            string header = pendingItems.Count == 1
                ? $"1 {label} onay bekliyor:"
                : $"{pendingItems.Count} {label} onay bekliyor:";

            List<string> lines = [header, ""];

            for (int i = 0; i < Math.Min(maxShown, pendingItems.Count); i++)
            {
                lines.Add($"• {GetItemSummary(pendingItems[i])}");
            }

            if (pendingItems.Count > maxShown)
            {
                lines.Add($"• ... ve {pendingItems.Count - maxShown} kayıt daha");
            }

            lines.Add("");
            lines.Add("Onay ekranında inceleyip onaylayabilirsiniz.");

            return string.Join("\r\n", lines);
        }

        /// <summary>
        /// Bildirimde ve silme onayında kullanılır olan kısa kayıt tanımı.
        /// </summary>
        protected virtual string GetItemSummary(TDto item) => GetDeleteSummary(item);

        /// <summary>
        /// "Tamam" sonrasında onay ekranını açar. MDI çocuk form, başka bir MDI
        /// çocuğu doğuramadığı için hedef daima asıl MDI kapsayıcıdır.
        /// </summary>
        private void OpenPendingApprovalScreen()
        {
            if (PendingApprovalFormType is not Type target || MdiParent is not XtraForm container)
            {
                return;
            }

            try
            {
                MdiFormManager.Instance.OpenForm(container, target, PendingApprovalFormTitle);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("PendingApproval.Open", ex);
                ToastHelper.Show("Onay ekranında açılamadı: " + ex.Message, ToastType.Error);
            }
        }

        /// <summary>
        /// Çoğul durumda kullanılacak kayıt adı. Türkçede çoğul ek fiille değil
        /// sözcük değişimiyle yapıldığı için ayrı tanımlanır; varsayılan tekil
        /// biçimle aynıdır.
        /// </summary>
        protected virtual string PendingItemLabelPlural => PendingItemLabel;

        protected virtual bool AllowsApprove(TDto item) => true;

        /// <summary>
        /// Liste ekranında "Maliyet Pusulası Yazdır" gibi bir rapor butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsSlipReport => false;

        /// <summary>
        /// Liste ekranında "Gider Dağıtım Tablosu Yazdır" butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsDistributionReport => false;

        /// <summary>
        /// Liste ekranında "Mamül Beyan Yazdır" butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsProductDeclarationReport => false;

        /// <summary>
        /// Liste ekranında "Stok Hareket Listesi Yazdır" butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsStockMovementsListReport => false;

        /// <summary>
        /// Liste ekranında "Stok Sayım Listesi Yazdır" butonunun gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsStockCountListReport => false;

        /// <summary>
        /// Tek kaydın onay komutunu üretir.
        ///
        /// KURAL: Onay yalnızca liste ekranlarında yapılır. Kayıt (düzenleme /
        /// oluşturma) formlarında onay düğmesi bulunmaz; kayıt formu daima
        /// taslak yazar, onay listeden ya da toplu onaydan yapılır.
        /// </summary>
        protected virtual IRequest<Result<string>>? BuildApproveCommand(TDto item) => null;

        /// <summary>
        /// Seçili kayıtların HEPSİ için tek seferde çalışan toplu onay komutunu
        /// üretir. Atomic olan komutlar stok/defter tutarlılığı için gereklidir:
        /// kayıtlar tek tek onaylanırsa ilki onaylanan kayıtlar stoktan düşer,
        /// sonrakiler stok yetersizliğiyle reddedilir ve sistemde YARIM onaylı
        /// bir durum kalır. Tek komut ise ya hep birlikte ya hiç onaylar.
        ///
        /// null dönerse geriye dönük uyum için kayıtlar tek tek onaylanır.
        /// </summary>
        protected virtual IRequest<Result<string>>? BuildBulkApproveCommand(IReadOnlyList<TDto> items) => null;

        /// <summary>
        /// Liste ekranında "TİF Yazdır" butonu ve sağ tık menüsünün gösterilip gösterilmeyeceği.
        /// </summary>
        protected virtual bool SupportsSlipPrint => false;

        /// <summary>
        /// Belirli bir kaydın taşınır işlem fişi olarak yazdırılıp yazdırılamayacağı.
        /// </summary>
        protected virtual bool CanPrintSlip(TDto item) => SupportsSlipPrint;

        /// <summary>
        /// "TİF Yazdır" butonunun bir kayıt SEÇİLMEDEN de çalışıp çalışmayacağı.
        ///
        /// <para>
        /// Fişi tek bir kayda bağlı formlarda (ör. stok çıkışı) buton seçimsiz
        /// kalır. Fiş bir dönemi kapsayan formlarda (ör. maliyet pusulası) ise
        /// seçim zorunlu değildir: kullanıcı dönemi fiş penceresinde seçer.
        /// </para>
        /// </summary>
        protected virtual bool AllowsSlipPrintWithoutSelection => false;

        /// <summary>
        /// Kaydın taşınır işlem fişi verisini üretir. Desteklenmiyorsa null döner.
        /// </summary>
        protected virtual Task<MovableAssetTransactionSlipData?> BuildSlipDataAsync(TDto item)
            => Task.FromResult<MovableAssetTransactionSlipData?>(null);

        /// <summary>
        /// Fiş verisi hazırlanmadan ÖNCE çalışır; tarih aralığı veya atölye
        /// seçimi gibi kullanıcı soruları burada sorulur.
        ///
        /// <para>
        /// Sorunun burada sorulmasının nedeni bekleme penceresidir: veri
        /// hazırlama <see cref="LoadingHelper"/> ile sarılır ve kullanıcı
        /// sorusu bu pencerenin arkasında kalırsa ekranda iki kalıcı pencere
        /// üst üste birikir. Bu yüzden bu adım, bekleme penceresi açılmadan
        /// çağrılır.
        /// </para>
        ///
        /// <para>
        /// false dönerse fiş basılmaz (kullanıcı vazgeçti).
        /// </para>
        ///
        /// <para>
        /// <paramref name="item"/> null ise seçim yapılmadan basılmak isteniyor
        /// demektir; yalnızca <see cref="AllowsSlipPrintWithoutSelection"/>
        /// true olan formlarda olur.
        /// </para>
        /// </summary>
        protected virtual Task<bool> PrepareSlipPrintAsync(TDto? item) => Task.FromResult(true);

        /// <summary>
        /// Bir kayıt için birden çok taşınır işlem fişi basılabilir (ör. atölye
        /// başına bir fiş). Varsayılan olarak <see cref="BuildSlipDataAsync"/>
        /// sonucu tek fiş olarak döndürülür; liste boşsa fiş basılmaz.
        ///
        /// <para>
        /// <paramref name="item"/> null ise (seçim yok) varsayılan
        /// uygulama fiş basmaz; seçimsiz basım yalnızca fişi bir kayda
        /// bağlı olmayan formlarda anlamlıdır.
        /// </para>
        /// </summary>
        protected virtual async Task<IReadOnlyList<MovableAssetTransactionSlipData>> BuildSlipDataListAsync(TDto? item)
        {
            if (item is null)
            {
                return [];
            }

            MovableAssetTransactionSlipData? data = await BuildSlipDataAsync(item);

            return data is null
                ? []
                : [data];
        }

        /// <summary>
        /// Seçili kaydın raporu (ör. maliyet pusulası) açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowSlipReportAsync(TDto item) => Task.CompletedTask;

        /// <summary>
        /// Seçili kaydın gider dağıtım tablosu raporu açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowDistributionReportAsync(TDto item) => Task.CompletedTask;

        /// <summary>
        /// Mamül üretim beyanı raporu açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowProductDeclarationReportAsync(TDto? item) => Task.CompletedTask;

        /// <summary>
        /// Stok hareket listesi raporu açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowStockMovementsListReportAsync(TDto? item) => Task.CompletedTask;

        /// <summary>
        /// Stok sayım listesi raporu açılır. Desteklenmiyorsa hiçbir işlem yapılmaz.
        /// </summary>
        protected virtual Task ShowStockCountListReportAsync(TDto? item) => Task.CompletedTask;

        protected virtual TListQuery BuildListQuery() => new();

        /// <summary>
        /// Liste verisi yüklendikten sonra ek zenginleştirme (stok/fiyat gibi toplu hesaplar)
        /// yapmak isteyen formlar bu metodu override eder. Varsayılan davranış veriyi aynen döndürür.
        /// </summary>
        protected virtual Task<IReadOnlyList<TDto>> EnrichAsync(List<TDto> items, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<TDto>>(items);

        /// <summary>
        /// <see cref="EnrichAsync"/> tüm listeyi yeniden üretiyorsa (ör. Fiyat &amp; Stok Listesi)
        /// base sorgusu tamamen atlanır; böylece boşa çalışan ikinci bir sorgu olmaz.
        /// </summary>
        protected virtual bool EnrichReplacesBaseQuery => false;

        protected virtual IRequest<Result<string>>? BuildRestoreCommand(TDto item) => null;

        protected virtual string[] SearchFieldNames => [];

        protected abstract void ConfigureColumns();

        /// <summary>
        /// Tekil silme komutu. <c>null</c> dönerse liste ekranından silme yapılamaz;
        /// bu durumda <see cref="BuildBulkDeleteCommand"/> da <c>null</c> dönmelidir.
        /// </summary>
        protected virtual IRequest<Result<string>>? BuildDeleteCommand(TDto item) => null;

        /// <summary>
        /// Seçili kayıtların tamamı için TEK transaction'da çalışan toplu silme
        /// komutunu döndürür.
        ///
        /// <para>
        /// Sağlandığında silme akışı kayıt başına ayrı komut göndermek yerine bu
        /// komutu kullanır: hareket denetimi tek sorguda yapılır ve ya bütün
        /// kayıtlar silinir ya hiçbiri. Bu yüzden sağlayan listelerde silme
        /// düğmesi yalnızca engellenen kayıtlar varsa pasifleşir.
        /// </para>
        /// </summary>
        protected virtual IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<TDto> items) => null;

        /// <summary>
        /// Silme mesajlarında kullanılan kayıt adı (örn. "müşteri", "hesap").
        /// </summary>
        protected virtual string DeleteItemLabel => "kayıt";

        protected virtual string GetDeleteSummary(TDto item) => item.Id.ToString();

        /// <summary>
        /// Hareket görüldüğü vb. nedenle silinemeyen kayıtları döndürür. Boş değilse silme
        /// onayı hiç sorulmaz; yalnızca bir bilgi toast'ı gösterilir ve silme çalıştırılmaz.
        /// </summary>
        protected virtual Task<List<TDto>> GetUndeletableAsync(List<TDto> selected, CancellationToken cancellationToken)
            => Task.FromResult(new List<TDto>());

        protected virtual string GetSubtitle(int count)
            => _showDeleted ? $"{count} silinen kayıt" : $"{count} kayıt listeleniyor";

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            IconOptions.SvgImage = ModuleIcon;
            btnNew.Visible = AllowCreate;
            btnEdit.Visible = AllowEdit;
            btnDeleted.Visible = SupportsRestore;
            if (!AllowDelete)
            {
                btnDelete.Visible = false;
            }
            btnApprove.Visible = SupportsApprove;
            btnSlipPrint.Visible = SupportsSlipPrint;
            btnSlipReport.Visible = SupportsSlipReport;
            btnDistributionReport.Visible = SupportsDistributionReport;
            btnProductDeclaration.Visible = SupportsProductDeclarationReport;
            btnStockMovementsList.Visible = SupportsStockMovementsListReport;
            btnStockCountList.Visible = SupportsStockCountListReport;
            SetupSlipContextMenu();
            ConfigureColumns();

            // Buton satırı, arama kutusuna girmeyecek kadar daraltılır; sığmayan
            // butonlar kaydırma çubuğuyla erişilebilir kalır.
            Resize += (_, _) => ClampToolbarWidth();
            ClampToolbarWidth();

            // Tüm listelerde sayısal kolonlar 0,00 biçiminde ve boş değerlerde de
            // 0,00 görünür (kod içinde elle eklenen kolonlar dâhil).
            GridColumnFactory.RegisterManualNumericColumns(View);

            _ = ReloadAsync();
        }

        /// <summary>
        /// Buton çubuğunun genişliğini arama/filtre alanına kadar sınırlar.
        /// Sınırlanmazsa çubuk, üst üste binen butonların arama kutusunu
        /// kapatmasına yol açar.
        /// </summary>
        private void ClampToolbarWidth()
        {
            // Sağ taraftaki filtre kontrolleri görünürse çubuk onların soluna
            // kadar kısalır; görünmüyorsa panelin kullanılabilir genişliğini
            // tamamen kaplar. Böylece butonlar sıkışıp üst üste binmez.
            Control[] rightSide = [lblFilter, cmbFilter];

            int panelRight = pnlToolbar.ClientSize.Width - pnlToolbar.Padding.Right;
            int limit = rightSide
                .Where(c => c.Visible)
                .Select(c => c.Left)
                .DefaultIfEmpty(panelRight)
                .Min();

            flpToolbar.Width = Math.Max(200, limit - 8 - flpToolbar.Left);
        }

        private void WireEvents()
        {
            btnNew.Click += async (_, _) => await RunEditorAsync(null);
            btnEdit.Click += async (_, _) =>
            {
                if (!AllowEdit)
                {
                    return;
                }

                int[] rows = gridView.GetSelectedRows();
                if (rows.Length == 1 && gridView.GetRow(rows[0]) is TDto dto)
                {
                    if (!AllowsEdit(dto))
                    {
                        ToastHelper.Show("Bu kayıt düzenlenemez.", ToastType.Warning);
                        return;
                    }

                    if (!DoubleClickOpensEditor)
                    {
                        await ShowItemDetailAsync(dto);
                        return;
                    }

                    await RunEditorAsync(dto);
                }
            };
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (_, _) => await ReloadAsync();
            btnSlipPrint.Click += async (_, _) => await ShowSelectedSlipAsync();
            btnSlipReport.Click += async (_, _) => await ShowSelectedSlipReportAsync();
            btnDistributionReport.Click += async (_, _) => await ShowSelectedDistributionReportAsync();
            btnProductDeclaration.Click += async (_, _) => await ShowSelectedProductDeclarationReportAsync();
            btnStockMovementsList.Click += async (_, _) => await ShowSelectedStockMovementsListReportAsync();
            btnStockCountList.Click += async (_, _) => await ShowSelectedStockCountListReportAsync();
            btnApprove.Click += BtnApprove_Click;
            btnDeleted.CheckedChanged += BtnDeleted_CheckedChanged;
            btnRestore.Click += BtnRestore_Click;
            cmbFilter.EditValueChanged += CmbFilter_EditValueChanged;
            gridView.DoubleClick += GridView_DoubleClick;
            gridView.SelectionChanged += GridView_SelectionChanged;
        }

        private void SetupGrid()
        {
            gridView.OptionsBehavior.AutoPopulateColumns = false;

            // Grup paneli her listede görünür. Arama kutusu araç çubuğundan
            // kaldırıldı; DevExpress'in modern Find paneli doğrudan grup
            // panelinin içine yerleştirilir ve tüm kolonlarda anlık arama yapar.
            gridView.OptionsView.ShowGroupPanel = true;
            gridView.OptionsFind.AlwaysVisible = true;
            gridView.OptionsFind.FindPanelLocation = GridFindPanelLocation.GroupPanel;
            gridView.OptionsFind.FindNullPrompt = "Ara...";
            gridView.OptionsFind.ShowFindButton = false;
            gridView.OptionsFind.ShowFindButton = false;
            gridView.OptionsFind.ShowClearButton = true;

            // Liste, aranacak alanları kısıtlıyorsa Find paneli yalnızca bu
            // alanlarda arar; kısıt yoksa tüm kolonlarda arar.
            if (SearchFieldNames.Length > 0)
            {
                gridView.OptionsFind.FindFilterColumns = string.Join(";", SearchFieldNames);
            }

            gridView.OptionsSelection.MultiSelect = true;
            gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsView.ColumnAutoWidth = false;

            // DevExpress'te satır şeridi (even/odd) görünümü açıkken çizimde
            // Appearance.EvenRow/OddRow rengi, RowStyle'da atanan rengin ÜSTÜNE
            // biniyor; taslak/onaylı boyaması bu yüzden hiç görünmez. Bu, gerçek
            // ekran çizimi üzerinden piksel okunarak doğrulanmıştır. Durum
            // renklendirmesi olan listelerde şerit kapatılır, renk tamamen
            // RowStyle'dan gelir. Durumu olmayan listelerde şerit korunur.
            bool hasStatusRows = typeof(IApprovalStatusDto).IsAssignableFrom(typeof(TDto));
            gridView.OptionsView.EnableAppearanceEvenRow = !hasStatusRows;
            gridView.OptionsView.EnableAppearanceOddRow = !hasStatusRows;

            // Satır boyama tek olay üzerinden yürür: hem depo grup paleti hem de
            // taslak/onaylı durum rengi burada karar verilir. İki ayrı işleyici
            // bağlanırsa son eklenen diğerini ezer.
            gridView.RowStyle -= GridView_RowStyle;
            gridView.RowStyle += GridView_RowStyle;
        }

        private void SetupButtonIcons()
        {
            SetButtonIcon(btnNew, DxIcon.Add, 18);
            SetButtonIcon(btnEdit, EditButtonIcon, 18);
            btnEdit.Text = EditButtonCaption;
            SetButtonIcon(btnDelete, DxIcon.Delete, 18);
            SetButtonIcon(btnRefresh, DxIcon.Refresh, 18);
            SetButtonIcon(btnSlipPrint, DxIcon.Receipt, 18);
            SetButtonIcon(btnSlipReport, DxIcon.Receipt, 18);
            SetButtonIcon(btnDistributionReport, DxIcon.Receipt, 18);
            SetButtonIcon(btnProductDeclaration, DxIcon.Receipt, 18);
            SetButtonIcon(btnStockMovementsList, DxIcon.Receipt, 18);
            SetButtonIcon(btnStockCountList, DxIcon.StockBox, 18);
            SetButtonIcon(btnDeleted, DxIcon.Delete, 18);
            SetButtonIcon(btnRestore, DxIcon.Restore, 18);
            SetButtonIcon(btnApprove, DxIcon.Check, 18);
            AutoSizeToolbarButtons();
        }

        private void AutoSizeToolbarButtons()
        {
            foreach (SimpleButton button in flpToolbar.Controls.OfType<SimpleButton>())
            {
                button.Width = MeasureButtonWidth(button);
            }
        }

        private static int MeasureButtonWidth(SimpleButton button)
        {
            bool hasIcon = button.ImageOptions.SvgImage is not null;
            int textWidth = string.IsNullOrEmpty(button.Text)
                ? 0
                : System.Windows.Forms.TextRenderer.MeasureText(button.Text, button.Appearance.Font).Width;
            int iconWidth = hasIcon ? button.ImageOptions.SvgImageSize.Width + 6 : 0;
            int padding = string.IsNullOrEmpty(button.Text) ? 16 : 56;
            return textWidth + iconWidth + padding;
        }

        private static void SetButtonIcon(SimpleButton button, SvgImage icon, int size)
        {
            button.ImageOptions.SvgImage = icon;
            button.ImageOptions.SvgImageSize = new Size(size, size);
            button.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
        }

        /// <summary>
        /// Sağ üst arama çubuğunun soluna depo/cari gibi filtre amaçlı bir lookup ekler.
        /// </summary>
        protected void ConfigureFilter(object? dataSource, string valueMember, string displayMember, string caption)
        {
            bool hasItems = dataSource is System.Collections.IEnumerable { } items && items.Cast<object?>().Any();

            if (!hasItems)
            {
                lblFilter.Visible = false;
                cmbFilter.Visible = false;
                return;
            }

            _isFilterSetting = true;
            try
            {
                cmbFilter.Properties.DataSource = dataSource;
                cmbFilter.Properties.ValueMember = valueMember;
                cmbFilter.Properties.DisplayMember = displayMember;
                cmbFilter.Properties.PopupFilterMode = PopupFilterMode.Contains;
                cmbFilter.Properties.BestFitMode = BestFitMode.BestFit;

                cmbFilterView.Columns.Clear();
                GridColumn column = cmbFilterView.Columns.AddField(displayMember);
                column.Caption = caption;
                column.VisibleIndex = 0;
                column.Width = 160;
                cmbFilterView.BestFitColumns();

                lblFilter.Text = caption + ":";
                lblFilter.Visible = true;
                cmbFilter.Visible = true;
            }
            finally
            {
                _isFilterSetting = false;
            }
        }

        protected Guid? SelectedFilterGuid => cmbFilter.EditValue is Guid filterId ? filterId : null;

        protected async Task LoadWarehouseFilterAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                List<ChartOfAccountLookUpDto> warehouses = ((await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None)).Data ?? [])
                    .Where(w => w.Type == ChartOfAccountType.Warehouse)
                    .ToList();

                ConfigureFilter(warehouses, nameof(ChartOfAccountLookUpDto.Id), nameof(ChartOfAccountLookUpDto.Display), "Depo");
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Depo filtresi yüklenemedi: " + ex.Message, ToastType.Warning);
            }
        }

        private static void SetButtonIcon(CheckButton button, SvgImage icon, int size)
        {
            button.ImageOptions.SvgImage = icon;
            button.ImageOptions.SvgImageSize = new Size(size, size);
            button.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
        }

        protected void AddColumnsFromAttributes()
            => GridColumnFactory.ConfigureFromAttributes(View, typeof(TDto));

        /// <summary>
        /// Depo grupları için kullanılan canlı pastel zemin paleti. Her grup değeri
        /// hash ile bu paletten bir renge sabitlenir; böylece aynı depo her
        /// listelemede aynı rengi alır, sıralama değişse de karışmaz.
        /// </summary>
        private static readonly Color[] WarehouseGroupPalette =
        [
            Color.FromArgb(255, 224, 130),  // pastel sarı
            Color.FromArgb(255, 183, 183),  // pastel kırmızı
            Color.FromArgb(168, 208, 255),  // pastel mavi
            Color.FromArgb(168, 235, 178),  // pastel yeşil
            Color.FromArgb(214, 176, 255),  // pastel mor
            Color.FromArgb(255, 197, 148),  // pastel turuncu
            Color.FromArgb(150, 224, 224)   // pastel turkuaz
        ];

        private static readonly Font GroupRowFont = new("Segoe UI Semibold", 9.5F);

        private Color? _approvedRowColor;
        private Color? _draftRowColor;

        protected void ConfigureWarehouseGrouping(string fieldName)
        {
            GridColumn? warehouseColumn = View.Columns[fieldName];
            if (warehouseColumn is null)
            {
                return;
            }

            View.OptionsView.ShowGroupPanel = true;
            View.OptionsView.ShowGroupPanelColumnsAsSingleRow = true;
            View.ClearGrouping();
            warehouseColumn.Group();
            _warehouseGroupColumn = warehouseColumn;

            // Zemin rengi grup başına RowStyle ile belirlendiği için Appearance
            // yalnızca font ve varsayılan metin rengini taşır.
            View.Appearance.GroupRow.Font = new Font("Segoe UI Semibold", 9.5F);
            View.Appearance.GroupRow.Options.UseFont = true;
            View.Appearance.GroupRow.ForeColor = Color.FromArgb(24, 30, 45);
            View.Appearance.GroupRow.Options.UseForeColor = true;
        }

        /// <summary>
        /// Satır boyamayı TEK yerden yönetir.
        ///
        /// <para>
        /// Öncelik sırası: grup başlığı satırı → depo paleti; kayıt satırı →
        /// durum rengi (taslak/onaylı). İkisi aynı anda devrede olamaz; bir
        /// liste ya gruplar ya durum gösterir. <c>RowStyle</c> tek olay olduğu
        /// için iki ayrı işleyici bağlanmamalıdır.
        /// </para>
        /// </summary>
        private void GridView_RowStyle(object? sender, RowStyleEventArgs e)
        {
            if (TryStyleGroupRow(e))
            {
                return;
            }

            TryStyleStatusRow(e);
        }

        /// <summary>Grup başlık satırını depo paletinden bir renge boyar.</summary>
        private bool TryStyleGroupRow(RowStyleEventArgs e)
        {
            if (_warehouseGroupColumn is null || !View.IsGroupRow(e.RowHandle))
            {
                return false;
            }

            object? groupValue = View.GetGroupRowValue(e.RowHandle, _warehouseGroupColumn);
            if (groupValue is null)
            {
                return false;
            }

            int hash = StringComparer.Ordinal.GetHashCode(groupValue.ToString() ?? string.Empty);
            Color backColor = WarehouseGroupPalette[(hash & int.MaxValue) % WarehouseGroupPalette.Length];

            e.Appearance.BackColor = backColor;
            e.Appearance.BackColor2 = backColor;

            // Options bayrakları olmadan atanan renk/font çizimde yok sayılır.
            e.Appearance.Options.UseBackColor = true;
            e.Appearance.Options.UseForeColor = true;
            e.Appearance.Options.UseFont = true;

            e.Appearance.ForeColor = SkinTheme.GetContrastText(backColor);
            e.Appearance.Font = GroupRowFont;
            return true;
        }

        /// <summary>
        /// Taslak/onaylı kayıt satırını renklendirir.
        ///
        /// <para>
        /// Yalnızca <see cref="IApprovalStatusDto"/> uygulayan DTO'lar boyanır;
        /// böylece durumu olmayan listeler (ürün, müşteri, hesap planı ...)
        /// hiç etkilenmez. Renk aktif skin'in zeminiyle harmanlanır: tam doygun
        /// bir sarı/yeşil zemin hem açık hem koyu temada okunmaz hale gelir.
        /// </para>
        /// </summary>
        private void TryStyleStatusRow(RowStyleEventArgs e)
        {
            if (View.IsGroupRow(e.RowHandle)
                || View.GetRow(e.RowHandle) is not IApprovalStatusDto status)
            {
                return;
            }

            if (_approvedRowColor is null || _draftRowColor is null)
            {
                RefreshStatusRowColors();
            }

            StatusRowPainter.Apply(
                e,
                status,
                _approvedRowColor ?? Color.Empty,
                _draftRowColor ?? Color.Empty);
        }

        /// <summary>
        /// Durum renklerini aktif skin'in zeminine göre hesaplar.
        ///
        /// <para>
        /// <c>RowStyle</c> her boyamada her satır için çağrıldığı ve skin
        /// çözümlemesi DevExpress paletine baktığı için iki renk bir kez
        /// hesaplanıp saklanır. Skin değişince <see cref="ApplySkin"/>
        /// değerleri geçersiz kılar.
        /// </para>
        /// </summary>
        private void RefreshStatusRowColors()
        {
            if (gridControl.IsDisposed)
            {
                return;
            }

            Color surface = SkinTheme.SurfaceOf(gridControl);
            _approvedRowColor = StatusRowPainter.ApprovedRowColor(surface);
            _draftRowColor = StatusRowPainter.DraftRowColor(surface);
        }

        private void GridView_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            int selected = gridView.GetSelectedRows().Length;
            bool showDeleted = _showDeleted;
            bool allEditable = true;
            bool allDeletable = true;
            bool allPrintable = true;
            bool allApprovable = true;

            foreach (int row in gridView.GetSelectedRows())
            {
                if (gridView.GetRow(row) is not TDto dto)
                {
                    continue;
                }

                if (!AllowsEdit(dto))
                {
                    allEditable = false;
                }

                if (!AllowsDelete(dto))
                {
                    allDeletable = false;
                }

                if (!CanPrintSlip(dto))
                {
                    allPrintable = false;
                }

                if (!AllowsApprove(dto))
                {
                    allApprovable = false;
                }
            }

            btnNew.Enabled = AllowCreate && !showDeleted;
            btnNew.Visible = AllowCreate;
            btnEdit.Visible = AllowEdit;
            btnEdit.Enabled = AllowEdit && !showDeleted && selected == 1 && allEditable;
            btnDelete.Enabled = !showDeleted && selected >= 1 && allDeletable;
            btnDelete.Visible = AllowDelete && !showDeleted;
            btnSlipPrint.Visible = SupportsSlipPrint;

            // Seçim zorunlu olmayan fişlerde buton her zaman açıktır: kayıt
            // seçimi yalnızca varsayılan dönemi belirler, zorunlu değildir.
            btnSlipPrint.Enabled = !showDeleted
                && (AllowsSlipPrintWithoutSelection || selected == 1 && allPrintable);
            btnApprove.Enabled = SupportsApprove && !showDeleted && selected >= 1 && allApprovable;
            btnApprove.Visible = SupportsApprove && !showDeleted;
            btnSlipReport.Visible = SupportsSlipReport;
            btnSlipReport.Enabled = !showDeleted && selected == 1;
            btnDistributionReport.Visible = SupportsDistributionReport;
            btnDistributionReport.Enabled = !showDeleted;
            btnProductDeclaration.Visible = SupportsProductDeclarationReport;
            btnProductDeclaration.Enabled = !showDeleted;
            btnStockMovementsList.Visible = SupportsStockMovementsListReport;
            btnStockMovementsList.Enabled = !showDeleted;
            btnStockCountList.Visible = SupportsStockCountListReport;
            btnStockCountList.Enabled = !showDeleted;
            btnRestore.Enabled = showDeleted && selected >= 1;
            btnRestore.Visible = showDeleted && SupportsRestore;
            btnDeleted.Checked = showDeleted;
        }

        private void SetupSlipContextMenu()
        {
            if (!SupportsSlipPrint)
            {
                gridControl.ContextMenuStrip = null;
                return;
            }

            slipMenuItem.Click += async (_, _) => await ShowFocusedSlipAsync();
            slipMenu.Opening += (_, _) =>
                slipMenuItem.Enabled = CanPrintSlipItem(gridView.GetFocusedRow() as TDto);
        }

        private bool CanPrintSlipItem(TDto? item)
            => !_showDeleted && item is not null && CanPrintSlip(item);

        private async Task ShowSelectedSlipAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            // Seçim zorunlu olmayan fişlerde (ör. dönem bazlı üretim fişi)
            // buton seçimsiz de çalışır; dönem sorusu fiş penceresinde sorulur.
            if (dto is null && !AllowsSlipPrintWithoutSelection)
            {
                ToastHelper.Show("Taşınır işlem fişi için tek bir kayıt seçin.", ToastType.Warning);
                return;
            }

            await PrintSlipAsync(dto);
        }

        private async Task ShowSelectedSlipReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            if (rows.Length != 1 || gridView.GetRow(rows[0]) is not TDto dto)
            {
                ToastHelper.Show("Rapor için tek bir kayıt seçin.", ToastType.Warning);
                return;
            }

            try
            {
                await ShowSlipReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("SlipReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowSelectedDistributionReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            try
            {
                await ShowDistributionReportAsync(dto!);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("DistributionReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowSelectedProductDeclarationReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            try
            {
                await ShowProductDeclarationReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("ProductDeclarationReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowSelectedStockMovementsListReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            try
            {
                await ShowStockMovementsListReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("StockMovementsListReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowSelectedStockCountListReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            TDto? dto = rows.Length == 1 ? gridView.GetRow(rows[0]) as TDto : null;

            try
            {
                await ShowStockCountListReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("StockCountListReport", ex);
                ToastHelper.Show("Rapor açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private async Task ShowFocusedSlipAsync()
        {
            if (gridView.GetFocusedRow() is not TDto dto)
            {
                return;
            }

            await PrintSlipAsync(dto);
        }

        private async Task PrintSlipAsync(TDto? item)
        {
            // Seçim yoksa (seçimsiz basıma izin veren form) kayda özel kontrol
            // yapılamaz; kapsam dönem/atölye soruları ile belirlenir.
            if (item is not null && !CanPrintSlip(item))
            {
                ToastHelper.Show("Bu kayıt için taşınır işlem fişi oluşturulamaz.", ToastType.Warning);
                return;
            }

            try
            {
                // Tarih/atölye gibi sorular bekleme penceresi açılmadan sorulur.
                if (!await PrepareSlipPrintAsync(item))
                {
                    return;
                }

                // Fiş verisi hazırlanırken ekranın bir saniye boyunca
                // donmaması için bekleme penceresiyle sarılır. Belge
                // üretimi kendi penceresini açar; bu yüzden iki aşama
                // ayrı ayrı gösterilir.
                IReadOnlyList<MovableAssetTransactionSlipData> slips = await LoadingHelper.RunAsync(
                    () => BuildSlipDataListAsync(item),
                    caption: "Fiş verileri hazırlanıyor...",
                    description: "Lütfen bekleyin...");

                foreach (MovableAssetTransactionSlipData data in slips)
                {
                    await MovableAssetTransactionSlipPresenter.ShowAsync(data);
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("SlipPrint", ex);
                ToastHelper.Show("Taşınır işlem fişi açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }

        /// <summary>
        /// Listeyi bekleme penceresi eşliğinde yeniler. Tüm liste formları
        /// verisini bu metottan yüklediği için bekleme davranışı tek yerden
        /// yönetilir.
        ///
        /// Onay bekleyen kayıt bildirimi bilerek yükleme işinin İÇİNDE
        /// gösterilmez: içeride çağrılırsa "Lütfen Bekleyin" penceresi
        /// ekrandayken MsgBox açılır ve kullanıcı iki kalıcı pencereyi
        /// üst üste görür. Bunun yerine yükleme bitip bekleme penceresi
        /// kapandıktan SONRA bildirim gösterilir.
        /// </summary>
        protected virtual async Task ReloadAsync()
        {
            bool loaded = await LoadingHelper.RunAsync(
                ReloadCoreAsync,
                caption: "Kayıtlar yükleniyor...",
                description: "Lütfen bekleyin...");

            // Yükleme başarısızsa uyarı zaten Toast ile verildi; onay
            // bildirimi yalnızca gerçekten yüklenmiş veri için anlamlıdır.
            if (loaded)
            {
                NotifyPendingItems();
            }
        }

        /// <summary>
        /// Liste satırlarının yüklendikten sonraki sırasını belirler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Varsayılan sıra <b>kayıt tarihine göre en son kaydedilen üstte</b>dir.
        /// Tüm liste formları bu kuralı paylaşır; hiçbiri kendi sırasını
        /// değiştirmez. Geçmiş tarihli bir belge bugün kaydedildiği için
        /// listede en üstte görünür — listenin sırası "kim ne zaman kaydetti"
        /// sorusunu yanıtlar, belge tarihine göre değil.
        /// </para>
        /// <para>
        /// Tek istisna <b>stok hareketleri</b> listesidir: FIFO'da tüketim
        /// sırası hareket tarihine göre kurulduğu için liste sırası da
        /// hareket tarihine göre tutulur
        /// (<see cref="Utils.ListOrder.NewestDocumentFirst{T}"/>).
        /// </para>
        /// <para>
        /// Sorgular çoğunlukla sırasız döndüğü için sıralama veri
        /// materialize edildikten sonra burada uygulanır; her liste formu
        /// ayrı ayrı sıralama yazmak yerine tek bir kural paylaşır.
        /// </para>
        /// </remarks>
        protected virtual IEnumerable<TDto> ApplyDefaultOrder(IEnumerable<TDto> items)
            => items
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id);

        /// <summary>
        /// Veriyi yükler ve listeye bağlar. Bekleme penceresi bu metottan
        /// sonra kapanır; bu yüzden burada hiçbir kalıcı pencere açılmaz.
        /// </summary>
        /// <returns>Liste başarıyla yüklendiyse <c>true</c>.</returns>
        private async Task<bool> ReloadCoreAsync()
        {
            int version = ++_reloadVersion;
            try
            {
                lblSub.Text = "Yenileniyor...";
                TListQuery query = BuildListQuery();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                List<TDto> items = [];

                if (EnrichReplacesBaseQuery)
                {
                    items = ApplyDefaultOrder((await Task.Run(
                        () => EnrichAsync([], CancellationToken.None),
                        CancellationToken.None)).ToList()).ToList();
                    CrashLog.Write("PageLoad", $"{GetType().Name} DB(Enrich Only) {sw.Elapsed.TotalMilliseconds:N0} ms ({items.Count} satir)");
                }
                else
                {
                    List<TDto> fetched = await Task.Run(async () =>
                    {
                        using var scope = Program.Services.CreateScope();
                        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                        IQueryable<TDto> result = await mediator.Send(query, CancellationToken.None);
                        List<TDto> resultItems = result.ToList();
                        return ApplyDefaultOrder(resultItems).ToList();
                    });

                    CrashLog.Write("PageLoad", $"{GetType().Name} DB(Query) {sw.Elapsed.TotalMilliseconds:N0} ms ({fetched.Count} satir)");

                    if (version != _reloadVersion)
                    {
                        return false;
                    }

                    sw.Restart();
                    items = (await Task.Run(
                        () => EnrichAsync(fetched, CancellationToken.None),
                        CancellationToken.None)).ToList();
                    CrashLog.Write("PageLoad", $"{GetType().Name} DB(Enrich) {sw.Elapsed.TotalMilliseconds:N0} ms ({items.Count} satir)");
                }

                if (version != _reloadVersion)
                {
                    return false;
                }

                sw.Restart();
                _allItems = items;
                gridControl.DataSource = null;
                gridControl.DataSource = items;
                FitColumnsToContent();
                lblSub.Text = GetSubtitle(items.Count);
                CrashLog.Write("PageLoad", $"{GetType().Name} Ready Toplam {sw.Elapsed.TotalMilliseconds:N0} ms");

                return true;
            }
            catch (AuthorizationException ex)
            {
                CrashLog.WriteException("Reload.Auth", ex);
                ToastHelper.Show(ex.Message, ToastType.Warning, 4000);
                lblSub.Text = "Yetkiniz yok";
                return false;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Reload", ex);
                ToastHelper.Show("Liste yüklenemedi: " + ex.Message, ToastType.Error, 4000);
                lblSub.Text = "Yükleme hatası";
                return false;
            }
            finally
            {
                UpdateButtonStates();
            }
        }

        /// <summary>
        /// Kolon genişliklerini içeriklere göre ayarlar; metin kolonları devasa büyümesin diye
        /// tavan değer uygulanır. Sabit genişlikli (görsel) kolonlar etkilenmez.
        /// </summary>
        private void FitColumnsToContent()
        {
            if (_columnsFitted)
            {
                return;
            }

            // Çok satırlı listelerde (ör. stok hareketleri) BestFit tüm satırları taradığı için
            // arayüz birkaç saniye kilitlenebilir; büyük listelerde kolon genişlikleri korunur.
            if (_allItems.Count > 25000)
            {
                _columnsFitted = true;
                return;
            }

            gridView.BeginUpdate();
            try
            {
                foreach (GridColumn column in gridView.Columns)
                {
                    if (column.MaxWidth == 0)
                    {
                        column.MaxWidth = Math.Max(280, column.Width * 2);
                    }
                }

                gridView.BestFitColumns();
                _columnsFitted = true;
            }
            finally
            {
                gridView.EndUpdate();
            }
        }

        protected virtual TEditForm CreateEditEditor(TDto item)
            => (TEditForm)Activator.CreateInstance(typeof(TEditForm), item)!;

        protected virtual XtraForm CreateNewEditor()
            => Activator.CreateInstance<TEditForm>();

        private async Task RunEditorAsync(TDto? item)
        {
            if (item is null ? !AllowCreate : !AllowsEdit(item))
            {
                return;
            }

            XtraForm form;
            try
            {
                CrashLog.Write("Editor", $"Creating edit form for {typeof(TDto).Name}. New:{(item is null)}");
                form = item is null ? CreateNewEditor() : CreateEditEditor(item);
                CrashLog.Write("Editor", "Edit form created");
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Editor.Create", ex);
                ToastHelper.Show("Form açılamadı: " + ex.Message, ToastType.Error, 8000);
                return;
            }

            using (form)
            {
                try
                {
                    CrashLog.Write("Editor", "Showing edit dialog");
                    form.ShowDialog(this);
                    await ReloadAsync();
                    CrashLog.Write("Editor", "Edit dialog closed");
                }
                catch (Exception ex)
                {
                    CrashLog.WriteException("Editor.Show", ex);
                    ToastHelper.Show("Form hatası: " + ex.Message, ToastType.Error, 8000);
                }
            }
        }

        private async void GridView_DoubleClick(object? sender, EventArgs e)
        {
            if (_showDeleted || !AllowEdit)
            {
                return;
            }

            if (gridView.GetFocusedRow() is TDto dto)
            {
                if (!AllowsEdit(dto))
                {
                    ToastHelper.Show("Bu kayıt düzenlenemez.", ToastType.Warning);
                    return;
                }

                if (!DoubleClickOpensEditor)
                {
                    await ShowItemDetailAsync(dto);
                    return;
                }

                await RunEditorAsync(dto);
            }
        }

        /// <summary>
        /// Seçili kayıtları döndürür. Silme, geri yükleme ve onaylama akışları
        /// aynı seçim kuralını paylaşır.
        /// </summary>
        private List<TDto> GetSelectedItems()
            => gridView.GetSelectedRows()
                .Select(i => gridView.GetRow(i) as TDto)
                .Where(x => x is not null)
                .Cast<TDto>()
                .ToList();

        /// <summary>
        /// Silme akışının tek uygulaması.
        ///
        /// <para>
        /// Sıra şöyledir: seçim al → hareket denetimi → onay → atomik toplu silme.
        /// Denetim <c>GetUndeletableAsync</c> ile tek sorguda yapılır; sorgu
        /// sayısı seçim büyüklüğüne bağlı değildir.
        /// </para>
        /// </summary>
        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (!AllowDelete)
            {
                return;
            }

            List<TDto> selected = GetSelectedItems();

            if (selected.Count == 0)
            {
                ToastHelper.Show(DeletionMessages.NoSelection(DeleteItemLabel), ToastType.Warning);
                return;
            }

            if (selected.Any(item => !AllowsDelete(item)))
            {
                ToastHelper.Show("Silinemeyen kayıt(lar) seçildi. İşlem iptal edildi.", ToastType.Warning);
                return;
            }

            List<TDto> blocked;
            try
            {
                blocked = await GetUndeletableAsync(selected, CancellationToken.None);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Delete.PreCheck", ex);
                ToastHelper.Show("Silme kontrolü yapılamadı: " + ex.Message, ToastType.Error, 6000);
                return;
            }

            if (blocked.Count > 0)
            {
                ToastHelper.Show(
                    DeletionMessages.MovementBlocked(
                        DeleteItemLabel,
                        blocked.Count,
                        blocked.Select(GetDeleteSummary)),
                    ToastType.Warning,
                    6000);
                return;
            }

            IRequest<Result<string>>? bulkCommand = BuildBulkDeleteCommand(selected);
            if (MsgBox.Confirm(BuildDeleteConfirmText(selected, bulkCommand is not null), $"{Text} - Silme Onayı")
                != DialogResult.Yes)
            {
                return;
            }

            btnDelete.Enabled = false;
            try
            {
                // Öncelikli yol: atomik toplu silme. Tek komut, tek transaction.
                if (bulkCommand is not null)
                {
                    if (await CrudExecutor.ExecuteAsync(bulkCommand))
                    {
                        await ReloadAsync();
                    }

                    return;
                }

                foreach (TDto item in selected)
                {
                    IRequest<Result<string>>? command = BuildDeleteCommand(item);
                    if (command is null)
                    {
                        ToastHelper.Show($"{typeof(TDto).Name} için silme tanımlı değil.", ToastType.Warning);
                        return;
                    }

                    if (!await CrudExecutor.ExecuteAsync(command))
                    {
                        break;
                    }
                }

                await ReloadAsync();
            }
            finally
            {
                btnDelete.Enabled = true;
            }
        }

        /// <summary>Silme onay penceresinin metnini üretir.</summary>
        private string BuildDeleteConfirmText(List<TDto> selected, bool atomic)
        {
            string preview = DeletionMessages.BuildPreview(selected.Select(GetDeleteSummary));
            string atomicNote = atomic
                ? "\n\nSeçilen kayıtlar birlikte silinecek; biri silinemezse hiçbiri silinmez."
                : string.Empty;

            return $"{selected.Count} {DeleteItemLabel} silinecek.\n{preview}{atomicNote}\n\nEmin misiniz?";
        }

        private async void CmbFilter_EditValueChanged(object? sender, EventArgs e)
        {
            if (_isFilterSetting)
            {
                return;
            }

            await ReloadAsync();
        }

        private async void BtnApprove_Click(object? sender, EventArgs e)
        {
            if (!SupportsApprove)
            {
                return;
            }

            List<TDto> selected = GetSelectedItems();

            if (selected.Count == 0)
            {
                ToastHelper.Show("Onaylanacak kayıtları işaretleyin", ToastType.Warning);
                return;
            }

            btnApprove.Enabled = false;
            try
            {
                // Öncelikli yol: atomik toplu onay. Tek komut tüm seçimi tek
                // transaction'da onaylar; biri düşerse hiçbiri onaylanmaz.
                if (BuildBulkApproveCommand(selected) is { } bulkCommand)
                {
                    await CrudExecutor.ExecuteAsync(bulkCommand);
                    await ReloadAsync();
                    return;
                }

                foreach (TDto item in selected)
                {
                    IRequest<Result<string>>? command = BuildApproveCommand(item);
                    if (command is null)
                    {
                        ToastHelper.Show($"'{GetDeleteSummary(item)}' zaten onaylanmış durumda.", ToastType.Warning);
                        continue;
                    }

                    if (!await CrudExecutor.ExecuteAsync(command))
                    {
                        break;
                    }
                }
                await ReloadAsync();
            }
            finally
            {
                btnApprove.Enabled = true;
            }
        }

        private async void BtnDeleted_CheckedChanged(object? sender, EventArgs e)
        {
            _showDeleted = btnDeleted.Checked;
            UpdateButtonStates();
            await ReloadAsync();
        }

        private async void BtnRestore_Click(object? sender, EventArgs e)
        {
            if (!_showDeleted || !SupportsRestore)
            {
                return;
            }

            List<TDto> selected = GetSelectedItems();

            if (selected.Count == 0)
            {
                ToastHelper.Show("Geri yüklenecek kayıtları işaretleyin", ToastType.Warning);
                return;
            }

            string preview = DeletionMessages.BuildPreview(selected.Select(GetDeleteSummary));

            if (MsgBox.Confirm($"{selected.Count} kayıt geri yüklenecek.\n{preview}\n\nEmin misiniz?", $"{Text} - Geri Yükleme Onayı") != DialogResult.Yes)
            {
                return;
            }

            btnRestore.Enabled = false;
            bool restored = false;
            try
            {
                foreach (TDto item in selected)
                {
                    IRequest<Result<string>>? command = BuildRestoreCommand(item);
                    if (command is null)
                    {
                        ToastHelper.Show($"{typeof(TDto).Name} için geri yükleme desteklenmiyor", ToastType.Warning);
                        return;
                    }

                    if (!await CrudExecutor.ExecuteAsync(command))
                    {
                        break;
                    }

                    restored = true;
                }
            }
            finally
            {
                btnRestore.Enabled = true;
            }

            if (restored && btnDeleted.Checked)
            {
                btnDeleted.Checked = false;
                return;
            }

            await ReloadAsync();
        }
    }
}