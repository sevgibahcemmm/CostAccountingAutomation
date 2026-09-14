using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public partial class XtraFormMdiBase : XtraForm
    {
        protected XtraFormMdiBase(string formTitle)
        {
            Text = formTitle;
            StartPosition = FormStartPosition.CenterScreen;
        }

        protected XtraFormMdiBase() : this(string.Empty)
        {
        }
    }
}