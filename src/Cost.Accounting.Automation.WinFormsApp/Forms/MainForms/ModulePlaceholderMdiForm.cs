using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using DevExpress.Utils.Svg;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class ModulePlaceholderMdiForm : XtraFormMdiBase
    {
        public ModulePlaceholderMdiForm()
            : this("Sistem Modülü", "Sistem", null)
        {
        }

        public ModulePlaceholderMdiForm(string moduleTitle, string moduleName, SvgImage? icon = null)
            : base(moduleTitle)
        {
            InitializeComponent();
            lblTitle.Text = moduleTitle;
            lblSub.Text = moduleName;

            if (icon is not null)
            {
                picIcon.SvgImage = icon;
                IconOptions.SvgImage = icon;
            }

            pnlBody.Resize += (s, e) => CenterContent();

            CenterContent();
        }

        private void CenterContent()
        {
            int titleWidth = lblDesc.Width;
            int titleHeight = lblDesc.Height;
            int noteHeight = lblNote.Height;

            lblDesc.Location = new Point(
                Math.Max(20, (pnlBody.ClientSize.Width - titleWidth) / 2),
                Math.Max(20, (pnlBody.ClientSize.Height - noteHeight - titleHeight) / 2 - 20));

            lblNote.Location = new Point(
                Math.Max(20, (pnlBody.ClientSize.Width - lblNote.Width) / 2),
                lblDesc.Bottom + 14);
        }
    }
}