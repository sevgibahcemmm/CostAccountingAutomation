using Cost.Accounting.Automation.WinFormsApp.Forms.CostSlips;
using Cost.Accounting.Automation.WinFormsApp.Forms.Reports;
using Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable;
using Cost.Accounting.Automation.WinFormsApp.Reports.StockCountListReports;
using Cost.Accounting.Automation.WinFormsApp.Reports.StockMovementsListReports;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.Utils.Svg;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Rapor sınıflarının MDI sekmesinde görünecek Türkçe adları ve ikonları.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Neden gerekiyor: sekme başlığı, rapor nesnesinin <c>Text</c> değerinden
    /// okunur. Raporda bu değer çalışma zamanında bilinmez (şablon dosyasından
    /// gelir, veri kaynağına bağlıdır). Bu yüzden başlık, raporun hangi belgeyi
    /// gösterdiğini bilen <b>katalogdan</b> okunur.
    /// </para>
    /// <para>
    /// Alternatif olan "sınıf adından türet" yaklaşımı
    /// (<c>CostSlipProductReport</c> → <c>CostSlipProduct</c>) kullanıcıya
    /// hiçbir şey anlatmıyor ve Türkçe değil. Yeni rapor eklenirken buraya bir
    /// satır eklenmesi yeterlidir; kayıtsız raporlar sınıf adıyla gösterilir ve
    /// günlüğe uyarı düşer.
    /// </para>
    /// </remarks>
    internal static class ReportCatalog
    {
        private sealed record Entry(string Caption, Func<SvgImage> Icon);

        private static readonly Dictionary<Type, Entry> Entries = new()
        {
            [typeof(CostSlipProductReport)] =
                new("Mamül Maliyet Baskı Önizleme", () => DxIcon.PrintPreview),

            [typeof(CostSlipServiceReport)] =
                new("Hizmet Maliyet Baskı Önizleme", () => DxIcon.PrintPreview),

            [typeof(MovableAssetTransactionSlipReport)] =
                new("Taşınır Varlık Fişi Baskı Önizleme", () => DxIcon.PrintPreview),

            [typeof(StockCountListReport)] =
                new("Stok Sayım Listesi Baskı Önizleme", () => DxIcon.PrintPreview),

            [typeof(StockMovementsListReport)] =
                new("Stok Hareket Listesi Baskı Önizleme", () => DxIcon.PrintPreview),

            [typeof(ProductCostAllocationTableReport)] =
                new("Mamül Maliyet Dağıtım Tablosu Baskı Önizleme", () => DxIcon.PrintPreview),

            [typeof(ServiceCostAllocationTable)] =
                new("Hizmet Maliyet Dağıtım Tablosu Baskı Önizleme", () => DxIcon.PrintPreview),

            [typeof(ProductDeclarationReport)] =
                new("Üretim Beyannamesi Baskı Önizleme", () => DxIcon.PrintPreview),
        };

        /// <summary>Rapor sınıfının MDI sekmesinde görünecek Türkçe adı.</summary>
        public static string CaptionOf(Type reportType)
        {
            ArgumentNullException.ThrowIfNull(reportType);

            if (Entries.TryGetValue(reportType, out Entry? entry))
            {
                return entry.Caption;
            }

            // Kayıtsız rapor: geliştiriciyi uyarmak için günlüğe düşer.
            CrashLog.Write(
                "ReportCatalog",
                $"'{reportType.Name}' icin Turkce baslik tanimli degil; " +
                "sekme sinif adi ile gosterilecek. ReportCatalog'a satır ekleyin.");

            return reportType.Name;
        }

        /// <summary>Rapor sınıfının sekme ikonu.</summary>
        public static SvgImage IconOf(Type reportType)
        {
            ArgumentNullException.ThrowIfNull(reportType);

            return Entries.TryGetValue(reportType, out Entry? entry)
                ? entry.Icon()
                : DxIcon.PrintPreview;
        }
    }
}
