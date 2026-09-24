using System.Diagnostics;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public partial class XtraFormMdiBase : XtraForm
    {
        private readonly Stopwatch _pageStopwatch = Stopwatch.StartNew();

        protected XtraFormMdiBase(string formTitle)
        {
            _pageStopwatch.Restart();
            Text = formTitle;
            StartPosition = FormStartPosition.CenterScreen;
        }

        protected override void OnLoad(EventArgs e)
        {
            CrashLog.Write("PageLoad", $"{GetType().Name} Load ({_pageStopwatch.Elapsed.TotalMilliseconds:N0} ms)");
            base.OnLoad(e);
        }

        protected override void OnShown(EventArgs e)
        {
            CrashLog.Write("PageLoad", $"{GetType().Name} Shown Toplam ({_pageStopwatch.Elapsed.TotalMilliseconds:N0} ms)");
            base.OnShown(e);
        }
    }
}