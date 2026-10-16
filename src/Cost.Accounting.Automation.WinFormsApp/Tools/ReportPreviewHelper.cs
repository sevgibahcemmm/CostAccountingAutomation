using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting.Preview;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Tools
{

    public static class ReportPreviewHelper
    {

        public static async Task PrintAsync(
            XtraReport report,
            string caption,
            string description = "Lütfen bekleyin...")
        {
            ArgumentNullException.ThrowIfNull(report);

            string name = report.GetType().Name;
            CrashLog.Write("ReportPreview", $"{name} belge üretimi başlıyor.");

            await LoadingHelper.RunAsync(
                () => report.CreateDocumentAsync(),
                caption: caption,
                description: description);

            CrashLog.Write("ReportPreview", $"{name} belge üretildi, sekme açılıyor.");

            OpenAsMdiTab(report);
        }

        private static void OpenAsMdiTab(XtraReport report)
        {
            XtraForm container = ResolveMdiContainer();

            ReportPrintTool tool = new(report);

            PrintPreviewRibbonFormEx preview = tool.PreviewRibbonForm;

            preview.PrintControl.UseDirectXPaint = DefaultBoolean.True;

            preview.IconOptions.SvgImage = ReportCatalog.IconOf(report.GetType());

            preview.FormClosed += (_, _) => ReleaseQuietly(tool, report);


            string caption = ReportCatalog.CaptionOf(report.GetType());

            MdiFormManager.Instance.OpenExistingForm(
                preview,
                container,
                MdiFormManager.NextTabTitle(container, caption));
        }


        private static void ReleaseQuietly(ReportPrintTool tool, XtraReport report)
        {
            try
            {
                tool.Dispose();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("ReportPreview.DisposeTool", ex);
            }

            try
            {
                report.Dispose();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("ReportPreview.DisposeReport", ex);
            }
        }

        private static XtraForm ResolveMdiContainer()
        {
            List<XtraForm> candidates = [.. System.Windows.Forms.Application.OpenForms
                .OfType<XtraForm>()
                .Where(f => f.TopLevel && f.Visible && !f.IsDisposed)];

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException(
                    "Rapor sekmesi açılamadı: upgulama penceresi bulunamadı.");
            }

            return candidates.FirstOrDefault(f => f.MdiChildren.Length > 0)
                ?? candidates[^1];
        }
    }
}
