using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    public static class MsgBox
    {
        public static DialogResult Confirm(IWin32Window? owner, string message, string title = "Onay")
        {
            XtraMessageBoxArgs args = new XtraMessageBoxArgs
            {
                Owner = owner,
                Caption = title,
                Text = message,
                Buttons = new DialogResult[] { DialogResult.Yes, DialogResult.No },
                Icon = SystemIcons.Question,
                DefaultButtonIndex = 1
            };

            args.Showing += (s, e) =>
            {
                e.Buttons[DialogResult.Yes].Text = "Evet";
                e.Buttons[DialogResult.No].Text = "Hayır";
                ForceTopMost(e.Form);
            };

            return XtraMessageBox.Show(args);
        }

        public static DialogResult Confirm(string message, string title = "Onay")
            => Confirm(null, message, title);

        private static void ForceTopMost(Form? form)
        {
            if (form is null)
            {
                return;
            }

            form.TopMost = true;
            form.Activate();
            form.BringToFront();
        }
    }
}