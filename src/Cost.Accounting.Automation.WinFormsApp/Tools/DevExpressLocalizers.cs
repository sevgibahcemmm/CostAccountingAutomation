using System.Globalization;
using System.IO;
using System.Reflection;
using DevExpress.XtraBars.Localization;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Localization;
using DevExpress.XtraPrinting.Localization;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

/// <summary>
/// DevExpress arayüzünü Türkçeleştirmek için kullanılan yardımcı sınıf.
/// Tüm localizer'ları kaydeder ve tr-TR kültürünü + uydu derlemelerini yükler.
/// </summary>
public static class DevExpressLocalizers
{
    private const string TargetCultureName = "tr-TR";

    private static readonly object SyncLock = new();
    private static readonly List<Assembly> LoadedSatelliteAssembliesList = [];
    private static bool _registered;

    /// <summary>
    /// DevExpress kontrollerinin Türkçe açılması için gereken tüm kurulumu yapar.
    /// Uygulamanın <c>Main()</c> metodunun en başında çağrılmalıdır.
    /// </summary>
    public static void Register()
    {
        lock (SyncLock)
        {
            if (_registered) return;
            _registered = true;

            ApplyCulture();
            LoadSatelliteAssemblies();
            RegisterLocalizers();
        }
    }

    private static void RegisterLocalizers()
    {
        GridLocalizer.Active = new TurkishGridLocalizer();
        PreviewLocalizer.Active = new TurkishPreviewLocalizer();
        Localizer.Active = new TurkishXtraMessageBoxLocalizer();
        BarLocalizer.Active = new TurkishBarLocalizer();
    }

    /// <summary>
    /// Grid filtre popup'ında <see cref="GridLocalizer"/> tarafından kapsanmayan
    /// metinleri ("(All)", "Today" vb.) Türkçeleştirir.
    /// </summary>
    public static void SetupFilterPopupTranslation(GridControl gridControl)
    {
        gridControl.HandleCreated -= OnGridHandleCreated;
        gridControl.HandleCreated += OnGridHandleCreated;

        void OnGridHandleCreated(object? sender, EventArgs e)
        {
            foreach (Control ctrl in gridControl.Controls)
                TranslateControlRecursive(ctrl);
        }
    }

    private static readonly Dictionary<string, string> _filterPopupTranslations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Values"] = "Değerler",
        ["Date Filters"] = "Tarih Filtreleri",
        ["Clear Filter"] = "Filtreyi Temizle",
        ["Close"] = "Kapat",
        ["(All)"] = "(Tümü)",
        ["(Blanks)"] = "(Boş)",
        ["Today"] = "Bugün",
        ["Yesterday"] = "Dün",
        ["Tomorrow"] = "Yarın",
        ["This Week"] = "Bu Hafta",
        ["Last Week"] = "Geçen Hafta",
        ["This Month"] = "Bu Ay",
        ["Last Month"] = "Geçen Ay",
        ["This Year"] = "Bu Yıl",
        ["Last Year"] = "Geçen Yıl",
        ["Past"] = "Geçmiş",
        ["Future"] = "Gelecek",
    };

    private static void TranslateControlRecursive(Control ctrl)
    {
        if (_filterPopupTranslations.TryGetValue(ctrl.Text?.Trim() ?? "", out var tr))
            ctrl.Text = tr;
        foreach (Control child in ctrl.Controls)
            TranslateControlRecursive(child);
    }

    private static void ApplyCulture()
    {
        var culture = CultureInfo.GetCultureInfo(TargetCultureName);
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private static void LoadSatelliteAssemblies()
    {
        var appDirectory = AppContext.BaseDirectory;
        var candidateFolders = new[]
        {
            Path.Combine(appDirectory, "tr-TR"),
            Path.Combine(appDirectory, "tr")
        };

        foreach (var folder in candidateFolders)
        {
            if (!Directory.Exists(folder)) continue;
            foreach (var filePath in Directory.EnumerateFiles(folder, "*.resources.dll", SearchOption.TopDirectoryOnly))
            {
                LoadAssembly(filePath);
            }
        }
    }

    private static void LoadAssembly(string filePath)
    {
        try
        {
            var assembly = Assembly.LoadFrom(filePath);
            if (!LoadedSatelliteAssembliesList.Contains(assembly))
                LoadedSatelliteAssembliesList.Add(assembly);
        }
        catch { }
    }
}

#region ─── Grid Localizer ──────────────────────────────────────────────────────

public sealed class TurkishGridLocalizer : GridLocalizer
{
    public override string GetLocalizedString(GridStringId id) => id switch
    {
        GridStringId.FindControlFindButton => "Ara",
        GridStringId.FindControlClearButton => "Temizle",
        GridStringId.MenuColumnSortAscending => "Artan Sırada Sırala",
        GridStringId.MenuColumnSortDescending => "Azalan Sırada Sırala",
        GridStringId.MenuColumnClearSorting => "Sıralamayı Temizle",
        GridStringId.MenuColumnFilterEditor => "Filtre Oluşturucu...",
        GridStringId.CustomFilterDialogCaption => "Özel Filtreleme",
        GridStringId.MenuColumnBestFit => "En İyi Genişliği Ayarla",
        GridStringId.MenuColumnBestFitAllColumns => "Tüm Sütunlara En İyi Genişliği Uygula",
        GridStringId.MenuColumnGroup => "Bu Sütuna Göre Grupla",
        GridStringId.MenuColumnUnGroup => "Grubu Kaldır",
        GridStringId.GridGroupPanelText => "Gruplamak için sütun başlığını buraya sürükleyin.",
        GridStringId.FindNullPrompt => "Aranacak kelimeyi buraya yazın...",
        GridStringId.CustomizationCaption => "Gizli Sütunlar",
        _ => base.GetLocalizedString(id)
    };
}

#endregion

#region ─── Preview (Baskı Önizleme) Localizer ──────────────────────────────────

public sealed class TurkishPreviewLocalizer : PreviewLocalizer
{
    private static readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["RibbonPreview_Page"] = "Sayfa",
        ["RibbonPreview_Navigation"] = "Gezinti",
        ["RibbonPreview_Zoom"] = "Yakınlaştır",
        ["RibbonPreview_Print"] = "Yazdır",
        ["RibbonPreview_Export"] = "Dışa Aktar",
        ["RibbonPreview_Send"] = "Gönder",
        ["RibbonPreview_Document"] = "Belge",
        ["Button_Ok"] = "Tamam",
        ["Button_Cancel"] = "İptal",
        ["Button_Apply"] = "Uygula",
        ["PreviewCaptions_None"] = "(Yok)",
        ["PreviewCaptions_All"] = "(Hepsi)",
        ["File"] = "Dosya",
        ["View"] = "Görünüm",
        ["Background"] = "Arka Plan",
        ["Preview"] = "Baskı Önizleme",
        ["PrintDirect"] = "Hızlı Yazdır",
        ["Print"] = "Yazdır",
        ["Close"] = "Kapat",
        ["PageSetup"] = "Sayfa Ayarları",
        ["Watermark"] = "Filigran / Arka Plan",
        ["MultiplePages"] = "Çoklu Sayfalar",
        ["FirstPage"] = "İlk Sayfa",
        ["LastPage"] = "Son Sayfa",
        ["NextPage"] = "Sonraki Sayfa",
        ["PrevPage"] = "Önceki Sayfa",
        ["ExportFile"] = "Dosyayı Dışa Aktar...",
        ["SendFile"] = "Dosyayı E-Posta ile Gönder...",
        ["ExportPdf"] = "PDF Belgesi",
        ["ExportXlsx"] = "Excel Belgesi (XLSX)",
        ["ExportXls"] = "Excel Belgesi (XLS)",
        ["ExportDocx"] = "Word Belgesi (DOCX)",
        ["ExportRtf"] = "Zengin Metin (RTF)",
        ["ExportTxt"] = "Metin Dosyası (TXT)",
        ["ExportCsv"] = "Virgülle Ayrılmış Değerler (CSV)",
        ["ExportGraphic"] = "Resim Dosyası",
        ["ExportOption_Pdf"] = "PDF Ayarları",
        ["ExportOption_Xlsx"] = "Excel Ayarları",
        ["ExportOption_Xls"] = "Excel Ayarları",
        ["ExportOption_Docx"] = "Word/Metin Ayarları",
        ["ExportOption_Rtf"] = "Word/Metin Ayarları",
        ["ExportOption_Image"] = "Resim Ayarları",
        ["ExportOption_Csv"] = "CSV/Metin Ayarları",
        ["MenuItem_PdfDocument"] = "PDF Belgesi",
        ["MenuItem_ImageFile"] = "Resim Dosyası",
        ["SearchInDocument"] = "Belge İçinde Ara...",
        ["NavigationPane"] = "Gezinti Bölmesi",
        ["Thumbnails"] = "Küçük Resimler",
        ["Bookmarks"] = "Yer İşaretleri",
        ["Find"] = "Ara",
        ["Search"] = "Ara",
        ["Zoom"] = "Yakınlaştır",
        ["Msg_Searching"] = "Aranıyor...",
        ["Msg_NoMatches"] = "Eşleşme bulunamadı.",
        ["Msg_SendFileAs"] = "Dosyayı şu formatta gönder:",
        ["PageSetup_Paper"] = "Kağıt",
        ["PageSetup_Margins"] = "Kenar Boşlukları",
        ["PageSetup_Orientation"] = "Yönlendirme",
        ["PageSetup_Portrait"] = "Dikey",
        ["PageSetup_Landscape"] = "Yatay",
        ["PageSetup_Size"] = "Boyut",
        ["Scaling_AdjustTo"] = "Ölçekle:",
        ["Scaling_FitTo"] = "Sığdır:",
        ["Scaling_PagesWide"] = "Sayfa Genişliği",
        ["Watermark_Text"] = "Metin",
        ["Watermark_Image"] = "Resim",
        ["Watermark_Direction"] = "Yön",
        ["Watermark_Font"] = "Yazı Tipi",
        ["Watermark_Color"] = "Renk",
        ["PageInfo_PageNumber"] = "Sayfa",
        ["PageInfo_PageNumberOfTotal"] = " / ",
        ["WM_Warning"] = "Uyarı",
        ["Msg_EmptyDocument"] = "Bu belge hiç sayfa içermiyor.",
        ["Scrolling_"] = "Kaydırma",
    };

    public override string GetLocalizedString(PreviewStringId id)
    {
        var key = id.ToString();
        if (_map.TryGetValue(key, out var exact)) return exact;
        foreach (var entry in _map)
        {
            if (key.Contains(entry.Key, StringComparison.OrdinalIgnoreCase))
                return entry.Value;
        }
        return base.GetLocalizedString(id);
    }
}

#endregion

#region ─── MessageBox & Bar Localizer ─────────────────────────────────────────

public sealed class TurkishXtraMessageBoxLocalizer : Localizer
{
    public override string GetLocalizedString(StringId id) => id switch
    {
        StringId.XtraMessageBoxOkButtonText => "Tamam",
        StringId.XtraMessageBoxCancelButtonText => "İptal",
        StringId.XtraMessageBoxYesButtonText => "Evet",
        StringId.XtraMessageBoxNoButtonText => "Hayır",
        StringId.XtraMessageBoxAbortButtonText => "Durdur",
        StringId.XtraMessageBoxRetryButtonText => "Yeniden Dene",
        StringId.XtraMessageBoxIgnoreButtonText => "Yoksay",
        StringId.SearchControlNullValuePrompt => "Ara...",
        _ => base.GetLocalizedString(id)
    };
}

public sealed class TurkishBarLocalizer : BarLocalizer
{
    public override string GetLocalizedString(BarString id) => id switch
    {
        BarString.RibbonToolbarBelow => "Hızlı Erişim Araç Çubuğunu Şeridin Altında Göster",
        BarString.RibbonToolbarAbove => "Hızlı Erişim Araç Çubuğunu Şeridin Üstünde Göster",
        BarString.RibbonSearchItemNullText => "Ara...",
        BarString.RibbonSearchItemNoMatchesFound => "Sonuç bulunamadı",
        _ => base.GetLocalizedString(id)
    };
}

#endregion
