using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using DevExpress.LookAndFeel;
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
    /// Belge <c>CreateDocumentAsync()</c> ile üretilir: DevExpress'in
    /// arayüz iş parçacığına bağlı olma kuralını kendi içinde korur ve yine de
    /// bekleme penceresinin boyanmasına izin verir.
    /// </para>
    ///
    /// <para>
    /// <b>Önizlemenin kapanma nedeni (asıl hata).</b> Sahip pencere açıkça
    /// verilmezse DevExpress <c>Form.ActiveForm</c>'u sahip olarak seçer. Rapor
    /// öncesinde gösterilen <c>ToastForm</c> <c>TopMost</c> olduğu için aktif
    /// form o olur, şerit önizleme onun <i>altında</i> açılır ve WinForms kuralı
    /// gereği bildirim kendiliğinden söndüğünde (<c>Hold</c> bitti → <c>Close</c>
    /// + <c>Dispose</c>) sahiplendiği önizleme de kapanır. Kullanıcı önizlemeyi
    /// "açılıp kapanıyor" olarak görür, hata oluşmaz, <c>crash.log</c>'a yalnızca
    /// normal kapanma düşer.
    ///
    /// Ölçülen zaman çizelgesi bunu doğruluyordu: uyarı 17.860, önizleme 18.500,
    /// kapanış 21.782 — yani 180 ms <c>Enter</c> + 3000 ms <c>Hold</c> + 240 ms
    /// <c>Exit</c> ile örtüşen, toast'un ömrüne birebir oturan bir kapanış.
    /// </para>
    ///
    /// <para>
    /// DevExpress, belge üretim hatalarını kendi modal döngüsü içinde yuttuğu
    /// için hata <c>crash.log</c>'a düşmez ve kullanıcı yalnızca kapanan pencereyi
    /// görür. Bu yüzden her aşama burada günlüğe yazılır.
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
        /// <param name="owner">
        /// Önizlemenin sahibi. <c>null</c> ise listedeki en üstteki kalıcı pencere
        /// seçilir. <b>Boş bırakılırsa <c>Form.ActiveForm</c> kullanılır; bu
        /// yanlıştır</b>: <c>ToastForm</c> <c>TopMost</c> olduğu için her
        /// bildirimden sonra aktif form o olur, önizleme onun altında açılır ve
        /// bildirim kendiliğinden söndüğünde sahibi olduğu önizleme de kapanır.
        /// </param>
        public static async Task PrintAsync(
            XtraReport report,
            string caption = "Rapor hazırlanıyor...",
            string description = "Lütfen bekleyin...",
            IWin32Window? owner = null)
        {
            ArgumentNullException.ThrowIfNull(report);

            string name = report.GetType().Name;
            CrashLog.Write("ReportPreview", $"{name} belge üretimi başlıyor.");

            try
            {
                await LoadingHelper.RunAsync(
                    () => report.CreateDocumentAsync(),
                    caption: caption,
                    description: description);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException($"ReportPreview.Create.{name}", ex);
                throw;
            }

            CrashLog.Write("ReportPreview", $"{name} belge üretildi, önizleme açılıyor.");

            IWin32Window target = owner ?? ResolveOwner();

            using ReportPrintTool tool = new(report);

            tool.PreviewRibbonForm.PrintControl.UseDirectXPaint = DefaultBoolean.True;

            try
            {
                // Sahip açıkça veriliyor: DevExpress aksi hâlde Form.ActiveForm'u
                // seçiyor ve TopMost bir bildirim formu önizlemeyi sahipleniyor.
                tool.ShowRibbonPreviewDialog(target, UserLookAndFeel.Default);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException($"ReportPreview.Show.{name}", ex);
                throw;
            }

            CrashLog.Write("ReportPreview", $"{name} önizleme kapandı.");
        }

        /// <summary>
        /// Önizleme sahibi olarak kalıcı bir pencere seçer.
        ///
        /// İki eleme birlikte kritik:
        /// <list type="bullet">
        /// <item><c>TopMost</c> bildirimler (<c>ToastForm</c>) ve bekleme
        /// pencereleri dışlanır — bunlar kendi kapanmalarında sahiplendikleri
        /// önizlemeyi de kapatırdı.</item>
        /// <item><c>TopLevel: true</c> şartı gerekir — liste formları MDI alt
        /// penceresidir ve sahiplik alamaz (<c>WaitFormHelper.AttachOwner</c>
        /// ile aynı kural).</item>
        /// </list>
        /// </summary>
        private static IWin32Window ResolveOwner()
        {
            // Projede Cost.Accounting.Automation.Application adlı bir namespace
            // bulunduğu için System.Windows.Forms.Application tam adıyla yazılır.
            Form? candidate = System.Windows.Forms.Application.OpenForms
                .OfType<Form>()
                .LastOrDefault(f => f.TopLevel
                                   && f.Visible
                                   && !f.TopMost
                                   && !f.IsDisposed
                                   && f is not ToastForm
                                   && f is not WaitForm);

            return candidate ?? new Control();
        }
    }
}