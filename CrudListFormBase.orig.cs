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
        /// Ba┼şl─▒k ve ay─▒r─▒c─▒ renkleri aktif skinden ├ğ├Âz├╝l├╝r; b├Âylece a├ğ─▒k ve koyu
        /// temalarda ba┼şl─▒k metni zeminin ├╝zerinde okunur kal─▒r. Butonlara renk
        /// atanmaz ÔÇö onlar─▒n g├Âr├╝n├╝m├╝ tamamen DevExpress skin'ine b─▒rak─▒l─▒r.
        ///
        /// <para>
        /// Ara├ğ ├ğubu─şu paneli (<see cref="pnlToolbar"/>) bilerek ┼şeffaft─▒r: buton
        /// band─▒n─▒n arkas─▒nda ayr─▒ bir renk ┼şeridi g├Âr├╝nmesin, butonlar do─şrudan
        /// formun zemininde dursun. Bu y├╝zden panel zemin rengi formun kendi
        /// zeminine b─▒rak─▒l─▒r (<c>Color.Transparent</c>); arama kutusu gibi panel
        /// i├ğindeki d├╝zenleyiciler kendi zeminlerini korur.
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

            // Sat─▒r renkleri skin'in zemininden t├╝retildi─şi i├ğin tema de─şi┼şince
            // yeniden hesaplanmal─▒ ve grid yeniden boyanmal─▒d─▒r; aksi h├ólde eski
            // zemine kar─▒┼ş─▒k tonlar kal─▒r ve koyu temada okunmaz hale gelir.
            _approvedRowColor = null;
            _draftRowColor = null;
            RefreshStatusRowColors();

            if (!gridControl.IsDisposed)
            {
                gridView.RefreshData();
            }
        }

        /// <summary>
        /// Ara├ğ ├ğubu─şu band─▒n─▒n zeminini kald─▒r─▒r. Panel hem DevExpress
        /// <c>Appearance</c> hem de WinForms <c>BackColor</c> de─şerinde ┼şeffaf
        /// yap─▒l─▒r: DevExpress panel boyama i┼şlemi <c>Appearance</c> ├╝zerinden
        /// y├╝r├╝r, alt kontrollerin ise arka plan─▒ WinForms taraf─▒nda okunur.
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

        /// <summary>Liste ekran─▒n─▒n ara├ğ ├ğubu─şuna ├Âzel bir buton ekler.</summary>
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
        /// T├╝retilmi┼ş formun kendi ara├ğ ├ğubu─şu butonlar─▒n─▒ haz─▒rlad─▒─ş─▒ kanca.
        ///
        /// T├╝retilmi┼ş formlar─▒n butonlar─▒ kendi tasar─▒m dosyalar─▒nda tan─▒mlan─▒r ve
        /// <c>flpToolbar</c> i├ğine eklenir; bu y├╝zden butonlar ancak t├╝retilmi┼ş
        /// kurucunun <c>InitializeComponent()</c> ├ğa─şr─▒s─▒ndan sonra var olur.
        /// Kurucu bu metodu kendi <c>InitializeComponent()</c> ├ğa─şr─▒s─▒ndan hemen
        /// sonra ├ğa─ş─▒rmal─▒d─▒r.
        /// </summary>
        protected virtual void RegisterDerivedToolbarButtons()
        {
            // T├╝retilmi┼ş tasar─▒m dosyas─▒n─▒n ekledi─şi butonlar taban geni┼şlik
            // hesab─▒ndan sonra geldi─şi i├ğin yeniden ├Âl├ğ├╝l├╝r.
            AutoSizeToolbarButtons();
        }

        /// <summary>
        /// Tasar─▒m dosyas─▒nda <c>flpToolbar</c> i├ğine eklenmi┼ş bir butonun ara├ğ
        /// ├ğubu─şundaki s─▒ras─▒n─▒ belirler.
        /// </summary>
        protected void MoveToolbarButton(Control button, int index)
        {
            if (index >= 0 && index < flpToolbar.Controls.Count)
            {
                flpToolbar.Controls.SetChildIndex(button, index);
            }
        }

protected virtual SvgImage ModuleIcon => DxIcon.Module;

        /// <summary>Liste ekran─▒nda "Yeni" butonunun g├Âsterilip g├Âsterilmeyece─şi.</summary>
        protected virtual bool AllowCreate => true;

        /// <summary>
        /// Liste ekran─▒nda "D├╝zenle" butonunun g├Âsterilip g├Âsterilmeyece─şi.
        /// Onaylama gibi yaln─▒zca mevcut kay─▒t ├╝zerinde i┼şlem yapan listelerde
        /// <c>false</c> verilir; aksi halde <see cref="AllowsEdit"/> her kay─▒t i├ğin
        /// <c>false</c> d├Ând├╝─ş├╝nden buton hep pasif g├Âr├╝n├╝r.
        /// </summary>
        protected virtual bool AllowEdit => true;

        /// <summary>Liste ekran─▒nda "Sil" butonunun g├Âsterilip g├Âsterilmeyece─şi.</summary>
        protected virtual bool AllowDelete => true;

        /// <summary>Belirli bir kayd─▒n d├╝zenlenip d├╝zenlenemeyece─şi.</summary>
        protected virtual bool AllowsEdit(TDto item) => true;

        /// <summary>
        /// ├çift t─▒klamada d├╝zenleme formu a├ğ─▒ls─▒n m─▒? <c>false</c> ise
        /// <see cref="ShowItemDetailAsync"/> ├ğa─şr─▒l─▒r (salt okunur detay ekran─▒ olan listeler i├ğin).
        /// </summary>
        protected virtual bool DoubleClickOpensEditor => true;

        /// <summary>D├╝zenleme/detay butonunun metni.</summary>
        protected virtual string EditButtonCaption => "D├╝zenle";

        /// <summary>D├╝zenleme/detay butonunun ikonu.</summary>
        protected virtual SvgImage EditButtonIcon => DxIcon.Edit;

        /// <summary>
        /// <see cref="DoubleClickOpensEditor"/> <c>false</c> oldu─şunda ├ğift t─▒klamada a├ğ─▒lacak
        /// a├ğ─▒klay─▒c─▒ detay ekran─▒.
        /// </summary>
        protected virtual Task ShowItemDetailAsync(TDto item) => Task.CompletedTask;

        protected virtual bool AllowsDelete(TDto item) => true;

        protected bool ShowDeleted => _showDeleted;

        protected virtual bool SupportsRestore => false;

        protected virtual bool SupportsApprove => false;

        /// <summary>
        /// Sayfa a├ğ─▒l─▒┼ş─▒nda onay bekleyen kay─▒t varsa nas─▒l bilgilendirilece─şi.
        /// Program genelinde tek tip kullan─▒l─▒r (kal─▒c─▒ pencere); "Tamam" ile onay
        /// ekran─▒na ge├ğilir.
        /// </summary>
        protected virtual PendingNoticeMode PendingNotice => PendingNoticeMode.MessageBox;

        /// <summary>
        /// Bilgilendirme metninde kullan─▒lacak kay─▒t ad─▒ (├Ârn. "fatura",
        /// "maliyet pusulas─▒").
        /// </summary>
        protected virtual string PendingItemLabel => "kay─▒t";

        /// <summary>
        /// Listedeki kayd─▒n onay bekleyip beklemedi─şi. Varsay─▒lan olarak
        /// <see cref="AllowsApprove"/> kural─▒ kullan─▒l─▒r; onay edilebilen kay─▒t
        /// onay bekleyen kay─▒tt─▒r. Bildirim yaln─▒zca <see cref="SupportsApprove"/>
        /// true iken g├Âsterildi─şinden bu kural yaln─▒zca onay ekranlar─▒nda anlaml─▒d─▒r.
        /// </summary>
        protected virtual bool IsPendingApproval(TDto item) => AllowsApprove(item);

        /// <summary>
        /// Bildirim penceresindeki "Tamam" sonras─▒nda a├ğ─▒lacak onay ekran─▒n─▒n t├╝r├╝.
        /// Ayr─▒ bir onay ekran─▒ olmayan listeler (maliyet pusulas─▒, stok belgesi)
        /// onay─▒ kendi ├╝zerinde yapt─▒─ş─▒ i├ğin <c>null</c> b─▒rak─▒r.
        /// </summary>
        protected virtual Type? PendingApprovalFormType => null;

        /// <summary>
        /// A├ğ─▒lacak onay ekran─▒n─▒n ba┼şl─▒─ş─▒. <c>null</c> ise formun kendi ba┼şl─▒─ş─▒
        /// kullan─▒l─▒r.
        /// </summary>
        protected virtual string? PendingApprovalFormTitle => null;

        /// <summary>
        /// Ba┼şar─▒l─▒ her y├╝klemeden sonra onay bekleyen kay─▒tlar─▒ bildirir.
        ///
        /// Bildirim yaln─▒zca onay ak─▒┼ş─▒ olan ekranlarda ├ğal─▒┼ş─▒r; onay deste─şi
        /// olmayan listelerde (├Ârn. cari hareketler) g├Âsterilmez. "Tamam" ile
        /// kapat─▒ld─▒─ş─▒nda <see cref="PendingApprovalFormType"/> tan─▒ml─▒ysa onay
        /// ekran─▒ a├ğ─▒l─▒r.
        /// </summary>
        private void NotifyPendingItems()
        {
            // Onay ak─▒┼ş─▒ olmayan ekranlarda "onay bekleyen kay─▒t" kavram─▒ yoktur;
            // SupportsApprove bunun tek g├╝venilir g├Âstergesidir.
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

            MsgBox.Notice(this, message, "Onay Bekleyen Kay─▒tlar");
            OpenPendingApprovalScreen();
        }

        /// <summary>
        /// Bekleyen kay─▒tlar─▒n d├Âk├╝m├╝n├╝ i├ğeren bildirim metnini kurar. Kullan─▒c─▒
        /// neyin bekledi─şini g├Ârmek zorunda; yaln─▒zca say─▒ s├Âylemek hangi kayd─▒n
        /// s─▒k─▒┼şt─▒─ş─▒n─▒ anlatmaz.
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
                lines.Add($"ÔÇó {GetItemSummary(pendingItems[i])}");
            }

            if (pendingItems.Count > maxShown)
            {
                lines.Add($"ÔÇó ... ve {pendingItems.Count - maxShown} kay─▒t daha");
            }

            lines.Add("");
            lines.Add("Onay ekran─▒nda inceleyip onaylayabilirsiniz.");

            return string.Join("\r\n", lines);
        }

        /// <summary>
        /// Bildirimde ve silme onay─▒nda kullan─▒lan k─▒sa kay─▒t tan─▒m─▒.
        /// </summary>
        protected virtual string GetItemSummary(TDto item) => GetDeleteSummary(item);

        /// <summary>
        /// "Tamam" sonras─▒nda onay ekran─▒n─▒ a├ğar. MDI ├ğocuk form, ba┼şka bir MDI
        /// ├ğocu─şu do─şuramad─▒─ş─▒ i├ğin hedef daima as─▒l MDI kapsay─▒c─▒d─▒r.
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
                ToastHelper.Show("Onay ekran─▒ a├ğ─▒lamad─▒: " + ex.Message, ToastType.Error);
            }
        }

        /// <summary>
        /// ├ço─şul durumda kullan─▒lacak kay─▒t ad─▒. T├╝rk├ğede ├ğo─şul ek fiille de─şil
        /// s├Âzc├╝k de─şi┼şimiyle yap─▒ld─▒─ş─▒ i├ğin ayr─▒ tan─▒mlan─▒r; varsay─▒lan tekil
        /// bi├ğimle ayn─▒d─▒r.
        /// </summary>
        protected virtual string PendingItemLabelPlural => PendingItemLabel;

        protected virtual bool AllowsApprove(TDto item) => true;

        /// <summary>
        /// Liste ekran─▒nda "Maliyet Pusulas─▒ Yazd─▒r" gibi bir rapor butonunun g├Âsterilip g├Âsterilmeyece─şi.
        /// </summary>
        protected virtual bool SupportsSlipReport => false;

        /// <summary>
        /// Liste ekran─▒nda "Gider Da─ş─▒t─▒m Tablosu Yazd─▒r" butonunun g├Âsterilip g├Âsterilmeyece─şi.
        /// </summary>
        protected virtual bool SupportsDistributionReport => false;

        /// <summary>
        /// Liste ekran─▒nda "Mam├╝l Beyan Yazd─▒r" butonunun g├Âsterilip g├Âsterilmeyece─şi.
        /// </summary>
        protected virtual bool SupportsProductDeclarationReport => false;

        /// <summary>
        /// Liste ekran─▒nda "Stok Hareket Listesi Yazd─▒r" butonunun g├Âsterilip g├Âsterilmeyece─şi.
        /// </summary>
        protected virtual bool SupportsStockMovementsListReport => false;

        /// <summary>
        /// Liste ekran─▒nda "Stok Say─▒m Listesi Yazd─▒r" butonunun g├Âsterilip g├Âsterilmeyece─şi.
        /// </summary>
        protected virtual bool SupportsStockCountListReport => false;

        /// <summary>
        /// Tek kayd─▒n onay komutunu ├╝retir.
        ///
        /// KURAL: Onay yaln─▒zca liste ekranlar─▒nda yap─▒l─▒r. Kay─▒t (d├╝zenleme /
        /// olu┼şturma) formlar─▒nda onay d├╝─şmesi bulunmaz; kay─▒t formu daima
        /// taslak yazar, onay listeden ya da toplu onaydan yap─▒l─▒r.
        /// </summary>
        protected virtual IRequest<Result<string>>? BuildApproveCommand(TDto item) => null;

        /// <summary>
        /// Se├ğili kay─▒tlar─▒n HEPS─░ i├ğin tek seferde ├ğal─▒┼şan toplu onay komutunu
        /// ├╝retir. Atomic olan komutlar stok/defter tutarl─▒l─▒─ş─▒ i├ğin gereklidir:
        /// kay─▒tlar tek tek onaylan─▒rsa ilki onaylanan kay─▒tlar stoktan d├╝┼şer,
        /// sonrakiler stok yetersizli─şiyle reddedilir ve sistemde YARIM onayl─▒
        /// bir durum kal─▒r. Tek komut ise ya hep birlikte ya hi├ğ onaylar.
        ///
        /// null d├Ânerse geriye d├Ân├╝k uyum i├ğin kay─▒tlar tek tek onaylan─▒r.
        /// </summary>
        protected virtual IRequest<Result<string>>? BuildBulkApproveCommand(IReadOnlyList<TDto> items) => null;

        /// <summary>
        /// Liste ekran─▒nda "TIF Yazd─▒r" butonu ve sa─ş t─▒k men├╝s├╝n├╝n g├Âsterilip g├Âsterilmeyece─şi.
        /// </summary>
        protected virtual bool SupportsSlipPrint => false;

        /// <summary>
        /// Belirli bir kayd─▒n ta┼ş─▒n─▒r i┼şlem fi┼şi olarak yazd─▒r─▒l─▒p yazd─▒r─▒lamayaca─ş─▒.
        /// </summary>
        protected virtual bool CanPrintSlip(TDto item) => SupportsSlipPrint;

        /// <summary>
        /// "TIF Yazd─▒r" butonunun bir kay─▒t SE├ç─░LMEDEN de ├ğal─▒┼ş─▒p ├ğal─▒┼şmayaca─ş─▒.
        ///
        /// <para>
        /// Fi┼şi tek bir kayda ba─şl─▒ formlarda (├Âr. stok ├ğ─▒k─▒┼ş─▒) buton se├ğimsiz
        /// kal─▒r. Fi┼ş bir d├Ânemi kapsayan formlarda (├Âr. maliyet pusulas─▒) ise
        /// se├ğim zorunlu de─şildir: kullan─▒c─▒ d├Ânemi fi┼ş penceresinde se├ğer.
        /// </para>
        /// </summary>
        protected virtual bool AllowsSlipPrintWithoutSelection => false;

        /// <summary>
        /// Kayd─▒n ta┼ş─▒n─▒r i┼şlem fi┼şi verisini ├╝retir. Desteklenmiyorsa null d├Âner.
        /// </summary>
        protected virtual Task<MovableAssetTransactionSlipData?> BuildSlipDataAsync(TDto item)
            => Task.FromResult<MovableAssetTransactionSlipData?>(null);

        /// <summary>
        /// Fi┼ş verisi haz─▒rlanmadan ├ûNCE ├ğal─▒┼ş─▒r; tarih aral─▒─ş─▒ veya at├Âlye
        /// se├ğimi gibi kullan─▒c─▒ sorular─▒ burada sorulur.
        ///
        /// <para>
        /// Sorusu burada sorulmas─▒n─▒n nedeni bekleme penceresidir: veri
        /// haz─▒rlama <see cref="LoadingHelper"/> ile sar─▒l─▒r ve kullan─▒c─▒
        /// sorusu bu pencerenin arkas─▒nda kal─▒rsa ekranda iki kal─▒c─▒ pencere
        /// ├╝st ├╝ste birikir. Bu y├╝zden bu ad─▒m, bekleme penceresi a├ğ─▒lmadan
        /// ├ğa─şr─▒l─▒r.
        /// </para>
        ///
        /// <para>
        /// false d├Ânerse fi┼ş bas─▒lmaz (kullan─▒c─▒ vazge├ğti).
        /// </para>
        ///
        /// <para>
        /// <paramref name="item"/> null ise se├ğim yap─▒lmadan bas─▒lmak isteniyor
        /// demektir; yaln─▒zca <see cref="AllowsSlipPrintWithoutSelection"/>
        /// true olan formlarda olur.
        /// </para>
        /// </summary>
        protected virtual Task<bool> PrepareSlipPrintAsync(TDto? item) => Task.FromResult(true);

        /// <summary>
        /// Bir kay─▒t i├ğin birden ├ğok ta┼ş─▒n─▒r i┼şlem fi┼şi bas─▒labilir (├Âr. at├Âlye
        /// ba┼ş─▒na bir fi┼ş). Varsay─▒lan olarak <see cref="BuildSlipDataAsync"/>
        /// sonucu tek fi┼ş olarak d├Ând├╝r├╝l├╝r; liste bo┼şsa fi┼ş bas─▒lmaz.
        ///
        /// <para>
        /// <paramref name="item"/> null ise (se├ğim yok) varsay─▒lan
        /// uygulama fi┼ş basmaz; se├ğimsiz bas─▒m yaln─▒zca fi┼şi bir kayda
        /// ba─şl─▒ olmayan formlarda anlaml─▒d─▒r.
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
        /// Se├ğili kayd─▒n raporu (├Âr. maliyet pusulas─▒) a├ğ─▒l─▒r. Desteklenmiyorsa hi├ğbir i┼şlem yap─▒lmaz.
        /// </summary>
        protected virtual Task ShowSlipReportAsync(TDto item) => Task.CompletedTask;

        /// <summary>
        /// Se├ğili kayd─▒n gider da─ş─▒t─▒m tablosu raporu a├ğ─▒l─▒r. Desteklenmiyorsa hi├ğbir i┼şlem yap─▒lmaz.
        /// </summary>
        protected virtual Task ShowDistributionReportAsync(TDto item) => Task.CompletedTask;

        /// <summary>
        /// Mam├╝l ├╝retim beyan─▒ raporu a├ğ─▒l─▒r. Desteklenmiyorsa hi├ğbir i┼şlem yap─▒lmaz.
        /// </summary>
        protected virtual Task ShowProductDeclarationReportAsync(TDto? item) => Task.CompletedTask;

        /// <summary>
        /// Stok hareket listesi raporu a├ğ─▒l─▒r. Desteklenmiyorsa hi├ğbir i┼şlem yap─▒lmaz.
        /// </summary>
        protected virtual Task ShowStockMovementsListReportAsync(TDto? item) => Task.CompletedTask;

        /// <summary>
        /// Stok say─▒m listesi raporu a├ğ─▒l─▒r. Desteklenmiyorsa hi├ğbir i┼şlem yap─▒lmaz.
        /// </summary>
        protected virtual Task ShowStockCountListReportAsync(TDto? item) => Task.CompletedTask;

        protected virtual TListQuery BuildListQuery() => new();

        /// <summary>
        /// Liste verisi y├╝klendikten sonra ek zenginle┼ştirme (stok/fiyat gibi toplu hesaplar)
        /// yapmak isteyen formlar bu metodu override eder. Varsay─▒lan davran─▒┼ş veriyi aynen d├Ând├╝r├╝r.
        /// </summary>
        protected virtual Task<IReadOnlyList<TDto>> EnrichAsync(List<TDto> items, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<TDto>>(items);

        /// <summary>
        /// <see cref="EnrichAsync"/> t├╝m listeyi yeniden ├╝retiyorsa (├Âr. Fiyat &amp; Stok Listesi)
        /// base sorgusu tamamen atlan─▒r; b├Âylece bo┼şa ├ğal─▒┼şan ikinci bir sorgu olmaz.
        /// </summary>
        protected virtual bool EnrichReplacesBaseQuery => false;

        protected virtual IRequest<Result<string>>? BuildRestoreCommand(TDto item) => null;

        protected virtual string[] SearchFieldNames => [];

        protected abstract void ConfigureColumns();

        /// <summary>
        /// Tekil silme komutu. <c>null</c> d├Ânerse liste ekran─▒ndan silme yap─▒lamaz;
        /// bu durumda <see cref="BuildBulkDeleteCommand"/> da <c>null</c> d├Ânmelidir.
        /// </summary>
        protected virtual IRequest<Result<string>>? BuildDeleteCommand(TDto item) => null;

        /// <summary>
        /// Se├ğili kay─▒tlar─▒n tamam─▒ i├ğin TEK transaction'da ├ğal─▒┼şan toplu silme
        /// komutunu d├Ând├╝r├╝r.
        ///
        /// <para>
        /// Sa─şland─▒─ş─▒nda silme ak─▒┼ş─▒ kay─▒t ba┼ş─▒na ayr─▒ komut g├Ândermek yerine bu
        /// komutu kullan─▒r: hareket denetimi tek sorguda yap─▒l─▒r ve ya b├╝t├╝n
        /// kay─▒tlar silinir ya hi├ğbiri. Bu y├╝zden sa─şlayan listelerde silme
        /// d├╝─şmesi yaln─▒zca engellenen kay─▒tlar varsa pasifle┼şir.
        /// </para>
        /// </summary>
        protected virtual IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<TDto> items) => null;

        /// <summary>
        /// Silme mesajlar─▒nda kullan─▒lan kay─▒t ad─▒ (├Ârn. "m├╝┼şteri", "hesap").
        /// </summary>
        protected virtual string DeleteItemLabel => "kay─▒t";

        protected virtual string GetDeleteSummary(TDto item) => item.Id.ToString();

        /// <summary>
        /// Hareket g├Âr├╝ld├╝─ş├╝ vb. nedenle silinemeyen kay─▒tlar─▒ d├Ând├╝r├╝r. Bo┼ş de─şilse silme
        /// onay─▒ hi├ğ sorulmaz; yaln─▒zca bir bilgi toast'─▒ g├Âsterilir ve silme ├ğal─▒┼şt─▒r─▒lmaz.
        /// </summary>
        protected virtual Task<List<TDto>> GetUndeletableAsync(List<TDto> selected, CancellationToken cancellationToken)
            => Task.FromResult(new List<TDto>());

        protected virtual string GetSubtitle(int count)
            => _showDeleted ? $"{count} silinen kay─▒t" : $"{count} kay─▒t listeleniyor";

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

            // Buton sat─▒r─▒, arama kutusuna girmeyecek kadar daralt─▒l─▒r; s─▒─şmayan
            // butonlar kayd─▒rma ├ğubu─şuyla eri┼şilebilir kal─▒r.
            Resize += (_, _) => ClampToolbarWidth();
            ClampToolbarWidth();

            // T├╝m listelerde say─▒sal kolonlar 0,00 bi├ğiminde ve bo┼ş de─şerlerde de
            // 0,00 g├Âr├╝n├╝r (kod i├ğinde elle eklenen kolonlar d├óhil).
            GridColumnFactory.RegisterManualNumericColumns(View);

            _ = ReloadAsync();
        }

        /// <summary>
        /// Buton ├ğubu─şunun geni┼şli─şini arama/filtre alan─▒na kadar s─▒n─▒rlar.
        /// S─▒n─▒rlanmazsa ├ğubuk, ├╝st ├╝ste binen butonlar─▒n arama kutusunu
        /// kapatmas─▒na yol a├ğar.
        /// </summary>
        private void ClampToolbarWidth()
        {
            Control[] rightSide = [lblFilter, cmbFilter, txtSearch];

            int limit = rightSide
                .Where(c => c.Visible)
                .Select(c => c.Left)
                .DefaultIfEmpty(int.MaxValue)
                .Min();

            if (limit == int.MaxValue)
            {
                return;
            }

            flpToolbar.Width = Math.Max(200, limit - 8 - flpToolbar.Left);
        }

        private void WireEvents()
        {
            btnClosePage.Click += (_, _) => Close();
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
                        ToastHelper.Show("Bu kay─▒t d├╝zenlenemez.", ToastType.Warning);
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
            txtSearch.EditValueChanged += TxtSearch_EditValueChanged;
            cmbFilter.EditValueChanged += CmbFilter_EditValueChanged;
            gridView.DoubleClick += GridView_DoubleClick;
            gridView.SelectionChanged += GridView_SelectionChanged;
        }

        private void SetupGrid()
        {
            gridView.OptionsBehavior.AutoPopulateColumns = false;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsSelection.MultiSelect = true;
            gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsView.ColumnAutoWidth = false;

            // DevExpress'te sat─▒r ┼şeridi (even/odd) g├Âr├╝n├╝m├╝ a├ğ─▒kken ├ğizimde
            // Appearance.EvenRow/OddRow rengi, RowStyle'da atanan rengin ├£ST├£NE
            // bilyor; taslak/onayl─▒ boyamas─▒ bu y├╝zden hi├ğ g├Âr├╝nmez. Bu, ger├ğek
            // ekran ├ğizimi ├╝zerinden piksel okunarak do─şrulanm─▒┼şt─▒r. Durum
            // renklendirmesi olan listelerde ┼şerit kapat─▒l─▒r, renk tamamen
            // RowStyle'dan gelir. Durumu olmayan listelerde ┼şerit korunur.
            bool hasStatusRows = typeof(IApprovalStatusDto).IsAssignableFrom(typeof(TDto));
            gridView.OptionsView.EnableAppearanceEvenRow = !hasStatusRows;
            gridView.OptionsView.EnableAppearanceOddRow = !hasStatusRows;

            // Sat─▒r boyama tek olay ├╝zerinden y├╝r├╝r: hem depo grup paleti hem de
            // taslak/onayl─▒ durum rengi burada karar verilir. ─░ki ayr─▒ i┼şleyici
            // ba─şlan─▒rsa son eklenen di─şerini ezer.
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
            SetButtonIcon(btnClosePage, DxIcon.Close, 16);
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
        /// Sa─ş ├╝st arama ├ğubu─şunun soluna depo/cari gibi filtre ama├ğl─▒ bir lookup ekler.
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
                ToastHelper.Show("Depo filtresi y├╝klenemedi: " + ex.Message, ToastType.Warning);
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
        /// Depo gruplar─▒ i├ğin kullan─▒lan canl─▒ pastel zemin paleti. Her grup de─şeri
        /// hash ile bu paletten bir renge sabitlenir; b├Âylece ayn─▒ depo her
        /// listelemede ayn─▒ rengi al─▒r, s─▒ralama de─şi┼şse de kar─▒┼şmaz.
        /// </summary>
        private static readonly Color[] WarehouseGroupPalette =
        [
            Color.FromArgb(255, 224, 130),  // pastel sar─▒
            Color.FromArgb(255, 183, 183),  // pastel k─▒rm─▒z─▒
            Color.FromArgb(168, 208, 255),  // pastel mavi
            Color.FromArgb(168, 235, 178),  // pastel ye┼şil
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

            // Zemin rengi grup ba┼ş─▒na RowStyle ile belirlendi─şi i├ğin Appearance
            // yaln─▒zca font ve varsay─▒lan metin rengini ta┼ş─▒r.
            View.Appearance.GroupRow.Font = new Font("Segoe UI Semibold", 9.5F);
            View.Appearance.GroupRow.Options.UseFont = true;
            View.Appearance.GroupRow.ForeColor = Color.FromArgb(24, 30, 45);
            View.Appearance.GroupRow.Options.UseForeColor = true;
        }

        /// <summary>
        /// Sat─▒r boyamay─▒ TEK yerden y├Ânetir.
        ///
        /// <para>
        /// ├ûncelik s─▒ras─▒: grup ba┼şl─▒─ş─▒ sat─▒r─▒ ÔåÆ depo paleti; kay─▒t sat─▒r─▒ ÔåÆ
        /// durum rengi (taslak/onayl─▒). ─░kisi ayn─▒ anda devrede olamaz; bir
        /// liste ya gruplar ya durum g├Âsterir. <c>RowStyle</c> tek olay oldu─şu
        /// i├ğin iki ayr─▒ i┼şleyici ba─şlanmamal─▒d─▒r.
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

        /// <summary>Grup ba┼şl─▒k sat─▒r─▒n─▒ depo paletinden bir renge boyar.</summary>
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

            // Options bayraklar─▒ olmadan atanan renk/font ├ğizimde yok say─▒l─▒r.
            e.Appearance.Options.UseBackColor = true;
            e.Appearance.Options.UseForeColor = true;
            e.Appearance.Options.UseFont = true;

            e.Appearance.ForeColor = SkinTheme.GetContrastText(backColor);
            e.Appearance.Font = GroupRowFont;
            return true;
        }

        /// <summary>
        /// Taslak/onayl─▒ kay─▒t sat─▒r─▒n─▒ renklendirir.
        ///
        /// <para>
        /// Yaln─▒zca <see cref="IApprovalStatusDto"/> uygulayan DTO'lar boyan─▒r;
        /// b├Âylece durumu olmayan listeler (├╝r├╝n, m├╝┼şteri, hesap plan─▒ ...)
        /// hi├ğ etkilenmez. Renk aktif skin'in zeminiyle harmanlan─▒r: tam doygun
        /// bir sar─▒/ye┼şil zemin hem a├ğ─▒k hem koyu temada okunmaz hale gelir.
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
        /// Durum renklerini aktif skin'in zeminine g├Âre hesaplar.
        ///
        /// <para>
        /// <c>RowStyle</c> her boyamada her sat─▒r i├ğin ├ğa─şr─▒ld─▒─ş─▒ ve skin
        /// ├ğ├Âz├╝mlemesi DevExpress paletine bak─▒yor; bu y├╝zden iki renk bir kez
        /// hesaplan─▒p saklan─▒r. Skin de─şi┼şince <see cref="ApplySkin"/>
        /// de─şerleri ge├ğersiz k─▒lar.
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

            // Se├ğim zorunlu olmayan fi┼şlerde buton her zaman a├ğ─▒kt─▒r: kay─▒t
            // se├ğimi yaln─▒zca varsay─▒lan d├Ânemi belirler, zorunlu de─şildir.
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

            // Se├ğim zorunlu olmayan fi┼şlerde (├Âr. d├Ânem bazl─▒ ├╝retim fi┼şi)
            // buton se├ğimsiz de ├ğal─▒┼ş─▒r; d├Ânem sorusu fi┼ş penceresinde sorulur.
            if (dto is null && !AllowsSlipPrintWithoutSelection)
            {
                ToastHelper.Show("Ta┼ş─▒n─▒r i┼şlem fi┼şi i├ğin tek bir kay─▒t se├ğin.", ToastType.Warning);
                return;
            }

            await PrintSlipAsync(dto);
        }

        private async Task ShowSelectedSlipReportAsync()
        {
            int[] rows = gridView.GetSelectedRows();
            if (rows.Length != 1 || gridView.GetRow(rows[0]) is not TDto dto)
            {
                ToastHelper.Show("Rapor i├ğin tek bir kay─▒t se├ğin.", ToastType.Warning);
                return;
            }

            try
            {
                await ShowSlipReportAsync(dto);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("SlipReport", ex);
                ToastHelper.Show("Rapor a├ğ─▒lamad─▒: " + ex.Message, ToastType.Error, 6000);
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
                ToastHelper.Show("Rapor a├ğ─▒lamad─▒: " + ex.Message, ToastType.Error, 6000);
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
                ToastHelper.Show("Rapor a├ğ─▒lamad─▒: " + ex.Message, ToastType.Error, 6000);
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
                ToastHelper.Show("Rapor a├ğ─▒lamad─▒: " + ex.Message, ToastType.Error, 6000);
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
                ToastHelper.Show("Rapor a├ğ─▒lamad─▒: " + ex.Message, ToastType.Error, 6000);
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
            // Se├ğim yoksa (se├ğimsiz bas─▒ma izin veren form) kayda ├Âzel kontrol
            // yap─▒lamaz; kapsam d├Ânem/at├Âlye sorular─▒yla belirlenir.
            if (item is not null && !CanPrintSlip(item))
            {
                ToastHelper.Show("Bu kay─▒t i├ğin ta┼ş─▒n─▒r i┼şlem fi┼şi olu┼şturulamaz.", ToastType.Warning);
                return;
            }

            try
            {
                // Tarih/at├Âlye gibi sorular bekleme penceresi a├ğ─▒lmadan sorulur.
                if (!await PrepareSlipPrintAsync(item))
                {
                    return;
                }

                // Fi┼ş verisi haz─▒rlan─▒rken ekran─▒n bir saniye boyunca
                // donmamas─▒ i├ğin bekleme penceresiyle sar─▒l─▒r. Belge
                // ├╝retimi kendi penceresini a├ğar; bu y├╝zden iki a┼şama
                // ayr─▒ ayr─▒ g├Âsterilir.
                IReadOnlyList<MovableAssetTransactionSlipData> slips = await LoadingHelper.RunAsync(
                    () => BuildSlipDataListAsync(item),
                    caption: "Fi┼ş verileri haz─▒rlan─▒yor...",
                    description: "L├╝tfen bekleyin...");

                foreach (MovableAssetTransactionSlipData data in slips)
                {
                    await MovableAssetTransactionSlipPresenter.ShowAsync(data);
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("SlipPrint", ex);
                ToastHelper.Show("Ta┼ş─▒n─▒r i┼şlem fi┼şi a├ğ─▒lamad─▒: " + ex.Message, ToastType.Error, 6000);
            }
        }

        /// <summary>
        /// Listeyi bekleme penceresi e┼şli─şinde yeniler. T├╝m liste formlar─▒
        /// verisini bu metottan y├╝kledi─şi i├ğin bekleme davran─▒┼ş─▒ tek yerden
        /// y├Ânetilir.
        ///
        /// Onay bekleyen kay─▒t bildirimi bilerek y├╝kleme i┼şinin ─░├ç─░NDE
        /// g├Âsterilmez: i├ğeride ├ğa─şr─▒l─▒rsa "L├╝tfen Bekleyin" penceresi
        /// ekrandayken MsgBox a├ğ─▒l─▒r ve kullan─▒c─▒ iki kal─▒c─▒ pencereyi
        /// ├╝st ├╝ste g├Âr├╝r. Bunun yerine y├╝kleme bitip bekleme penceresi
        /// kapand─▒ktan SONRA bildirim g├Âsterilir.
        /// </summary>
        protected virtual async Task ReloadAsync()
        {
            bool loaded = await LoadingHelper.RunAsync(
                ReloadCoreAsync,
                caption: "Kay─▒tlar y├╝kleniyor...",
                description: "L├╝tfen bekleyin...");

            // Y├╝kleme ba┼şar─▒s─▒zsa uyar─▒ zaten Toast ile verildi; onay
            // bildirimi yaln─▒zca ger├ğekten y├╝klenmi┼ş veri i├ğin anlaml─▒d─▒r.
            if (loaded)
            {
                NotifyPendingItems();
            }
        }

        /// <summary>
        /// Liste sat─▒rlar─▒n─▒n y├╝klendikten sonraki s─▒ras─▒n─▒ belirler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Varsay─▒lan s─▒ra <b>kay─▒t tarihine g├Âre en son kaydedilen ├╝stte</b>dir.
        /// T├╝m liste formlar─▒ bu kural─▒ payla┼ş─▒r; hi├ğbiri kendi s─▒ras─▒n─▒
        /// de─şi┼ştirmez. Ge├ğmi┼ş tarihli bir belge bug├╝n kaydedildi─şi i├ğin
        /// listede en ├╝stte g├Âr├╝n├╝r ÔÇö listenin s─▒ras─▒ "kim ne zaman kaydetti"
        /// sorusunu yan─▒tlar, belge tarihine g├Âre de─şil.
        /// </para>
        /// <para>
        /// Tek istisna <b>stok hareketleri</b> listesidir: FIFO'da t├╝ketim
        /// s─▒ras─▒ hareket tarihine g├Âre kuruldu─şu i├ğin liste s─▒ras─▒ da
        /// hareket tarihine g├Âre tutulur
        /// (<see cref="Utils.ListOrder.NewestDocumentFirst{T}"/>).
        /// </para>
        /// <para>
        /// Sorgular ├ğo─şunlukla s─▒ras─▒z d├Ând├╝─ş├╝ i├ğin s─▒ralama veri
        /// materialize edildikten sonra burada uygulan─▒r; her liste formu
        /// ayr─▒ ayr─▒ s─▒ralama yazmak yerine tek bir kural payla┼ş─▒r.
        /// </para>
        /// </remarks>
        protected virtual IEnumerable<TDto> ApplyDefaultOrder(IEnumerable<TDto> items)
            => items
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id);

        /// <summary>
        /// Veriyi y├╝kler ve listeye ba─şlar. Bekleme penceresi bu metottan
        /// sonra kapan─▒r; bu y├╝zden burada hi├ğbir kal─▒c─▒ pencere a├ğ─▒lmaz.
        /// </summary>
        /// <returns>Liste ba┼şar─▒yla y├╝klendiyse <c>true</c>.</returns>
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
                ToastHelper.Show("Liste y├╝klenemedi: " + ex.Message, ToastType.Error, 4000);
                lblSub.Text = "Y├╝kleme hatas─▒";
                return false;
            }
            finally
            {
                UpdateButtonStates();
            }
        }

        /// <summary>
        /// Kolon geni┼şliklerini i├ğeriklere g├Âre ayarlar; metin kolonlar─▒ devasa b├╝y├╝mesin diye
        /// tavan de─şer uygulan─▒r. Sabit geni┼şlikli (g├Ârsel) kolonlar etkilenmez.
        /// </summary>
        private void FitColumnsToContent()
        {
            if (_columnsFitted)
            {
                return;
            }

            // ├çok sat─▒rl─▒ listelerde (├Âr. stok hareketleri) BestFit t├╝m sat─▒rlar─▒ tarad─▒─ş─▒ i├ğin
            // aray├╝z birka├ğ saniye kilitlenebilir; b├╝y├╝k listelerde kolon geni┼şlikleri korunur.
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
                ToastHelper.Show("Form a├ğ─▒lamad─▒: " + ex.Message, ToastType.Error, 8000);
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
                    ToastHelper.Show("Form hatas─▒: " + ex.Message, ToastType.Error, 8000);
                }
            }
        }

        private void TxtSearch_EditValueChanged(object? sender, EventArgs e)
        {
            string term = (txtSearch.Text ?? string.Empty).Trim();
            if (term.Length == 0 || SearchFieldNames.Length == 0)
            {
                gridView.ActiveFilterString = "";
                return;
            }

            string safe = term.Replace("'", "''");
            string filter = string.Join(" OR ", SearchFieldNames.Select(field => $"[{field}] LIKE '%{safe}%'"));
            gridView.ActiveFilterString = filter;
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
                    ToastHelper.Show("Bu kay─▒t d├╝zenlenemez.", ToastType.Warning);
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
        /// Se├ğili kay─▒tlar─▒ d├Ând├╝r├╝r. Silme, geri y├╝kleme ve onaylama ak─▒┼şlar─▒
        /// ayn─▒ se├ğim kural─▒n─▒ payla┼ş─▒r.
        /// </summary>
        private List<TDto> GetSelectedItems()
            => gridView.GetSelectedRows()
                .Select(i => gridView.GetRow(i) as TDto)
                .Where(x => x is not null)
                .Cast<TDto>()
                .ToList();

        /// <summary>
        /// Silme ak─▒┼ş─▒n─▒n tek uygulamas─▒.
        ///
        /// <para>
        /// S─▒ra ┼ş├Âyledir: se├ğim al ÔåÆ hareket denetimi ÔåÆ onay ÔåÆ atomik toplu silme.
        /// Denetim <c>GetUndeletableAsync</c> ile tek sorguda yap─▒l─▒r; sorgu
        /// say─▒s─▒ se├ğim b├╝y├╝kl├╝─ş├╝ne ba─şl─▒ de─şildir.
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
                ToastHelper.Show("Silinemeyen kay─▒t(lar) se├ğildi. ─░┼şlem iptal edildi.", ToastType.Warning);
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
                ToastHelper.Show("Silme kontrol├╝ yap─▒lamad─▒: " + ex.Message, ToastType.Error, 6000);
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
            if (MsgBox.Confirm(BuildDeleteConfirmText(selected, bulkCommand is not null), $"{Text} - Silme Onay─▒")
                != DialogResult.Yes)
            {
                return;
            }

            btnDelete.Enabled = false;
            try
            {
                // ├ûncelikli yol: atomik toplu silme. Tek komut, tek transaction.
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
                        ToastHelper.Show($"{typeof(TDto).Name} i├ğin silme tan─▒ml─▒ de─şil.", ToastType.Warning);
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

        /// <summary>Silme onay penceresinin metnini ├╝retir.</summary>
        private string BuildDeleteConfirmText(List<TDto> selected, bool atomic)
        {
            string preview = DeletionMessages.BuildPreview(selected.Select(GetDeleteSummary));
            string atomicNote = atomic
                ? "\n\nSe├ğilen kay─▒tlar birlikte silinecek; biri silinemezse hi├ğbiri silinmez."
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
                ToastHelper.Show("Onaylanacak kay─▒tlar─▒ i┼şaretleyin", ToastType.Warning);
                return;
            }

            btnApprove.Enabled = false;
            try
            {
                // ├ûncelikli yol: atomik toplu onay. Tek komut t├╝m se├ğimi tek
                // transaction'da onaylar; biri d├╝┼şerse hi├ğbiri onaylanmaz.
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
                        ToastHelper.Show($"'{GetDeleteSummary(item)}' zaten onaylanm─▒┼ş durumda.", ToastType.Warning);
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
                ToastHelper.Show("Geri y├╝klenecek kay─▒tlar─▒ i┼şaretleyin", ToastType.Warning);
                return;
            }

            string preview = DeletionMessages.BuildPreview(selected.Select(GetDeleteSummary));

            if (MsgBox.Confirm($"{selected.Count} kay─▒t geri y├╝klenecek.\n{preview}\n\nEmin misiniz?", $"{Text} - Geri Y├╝kleme Onay─▒") != DialogResult.Yes)
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
                        ToastHelper.Show($"{typeof(TDto).Name} i├ğin geri y├╝kleme desteklenmiyor", ToastType.Warning);
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
