using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Tools
{
    /// <summary>
    /// Rapor önizlemesini bekleme penceresi eşliğinde açar.
    ///
    /// <para>
    /// <b>Neden gerekli?</b> DevExpress, <c>ShowRibbonPreviewDialog()</c>
    /// çağrısında belgeyi <b>arayüz iş parçacığında ve eşzamanlı</b> olarak
    /// üretir. Bu yüzden belge üretimi doğrudan çağrıldığında ekran donar ve
    /// <c>LoadingHelper</c> araya giremez: bekleyen iş parçacığında bekleme
    /// penceresini boyacak zaman kalmaz.
    /// </para>
    ///
    /// <para>
    /// Çözüm: belge <c>Task.Run</c> ile arka plana alınır. Böylece iş parçacığı
    /// boş kalır, <see cref="LoadingHelper"/> bekleme penceresini gösterip
    /// kapatabilir, önizleme ise yine arayüz iş parçacığında açılır.
    /// </para>
    /// </summary>
    public static class ReportPreviewHelper
    {
        /// <summary>
        /// Raporu arka planda üretir ve ribbon önizlemesini açar.
        /// </summary>
        /// <param name="report">Verisi doldurulmuş, henüz üretilmemiş rapor.</param>
        /// <param name="caption">Bekleme penceresi başlığı.</param>
        /// <param name="description">Bekleme penceresi açıklaması.</param>
        public static async Task PrintAsync(
            XtraReport report,
            string caption = "Rapor hazırlanıyor...",
            string description = "Lütfen bekleyin...")
        {
            ArgumentNullException.ThrowIfNull(report);

            await LoadingHelper.RunAsync(
                () => Task.Run(report.CreateDocument),
                caption: caption,
                description: description);

            using ReportPrintTool tool = new(report);

            tool.PreviewRibbonForm.PrintControl.UseDirectXPaint = DefaultBoolean.True;
            tool.ShowRibbonPreviewDialog();
        }
    }
}