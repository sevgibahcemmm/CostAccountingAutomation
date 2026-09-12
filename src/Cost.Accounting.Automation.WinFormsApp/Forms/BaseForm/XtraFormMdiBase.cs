using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public partial class XtraFormMdiBase : XtraForm
    {
        protected virtual Color HeaderColor => DXSkinColors.FillColors.Primary;

        protected XtraFormMdiBase(string formTitle)
        {
            Text = formTitle;
            StartPosition = FormStartPosition.CenterScreen;
        }
        protected XtraFormMdiBase() : this(string.Empty)
        {
        }

        protected void PnlHeader_Paint(object? sender, PaintEventArgs e)
        {
            var control = (Control)sender!;
            using var brush = new SolidBrush(HeaderColor);
            e.Graphics.FillRectangle(brush, control.ClientRectangle);
        }
    }
}