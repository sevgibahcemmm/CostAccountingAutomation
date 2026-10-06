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

        /// <summary>
        /// Formu MDI alanının ortasına yerleştirir.
        /// </summary>
        /// <remarks>
        /// MDI çocuklarında <see cref="Form.StartPosition"/> dikkate alınmaz;
        /// özellikle çerçevesiz dar pencereler (sohbet ekranı gibi) sol üst
        /// köşeye yapışır. Orta hizalama, pencere bir açılış gibi hissettirir.
        /// </remarks>
        protected void CenterInMdiClient()
        {
            if (MdiParent is not { } parent)
            {
                return;
            }

            Point location = new(
                Math.Max(0, (parent.ClientSize.Width - Width) / 2),
                Math.Max(0, (parent.ClientSize.Height - Height) / 2));

            if (!location.Equals(Location))
            {
                Location = location;
            }
        }
    }
}