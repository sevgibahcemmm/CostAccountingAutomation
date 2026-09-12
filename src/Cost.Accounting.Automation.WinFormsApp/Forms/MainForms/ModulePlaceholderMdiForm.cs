using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;

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

            pnlHeader.Paint += PnlHeader_Paint;
            pnlBody.Resize += (s, e) => CenterContent();
            CenterContent();
            AddCloseButton();
        }

        private void AddCloseButton()
        {
            SimpleButton btnClosePage = new()
            {
                Text = "Kapat",
                Size = new Size(92, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlHeader.ClientSize.Width - 118, 42),
                Appearance = { Font = new Font("Segoe UI", 10F, FontStyle.Bold) }
            };
            btnClosePage.ImageOptions.SvgImage = SvgIcons.CloseIcon;
            btnClosePage.ImageOptions.SvgImageSize = new Size(16, 16);
            btnClosePage.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnClosePage.Click += (_, _) => Close();
            pnlHeader.Controls.Add(btnClosePage);
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