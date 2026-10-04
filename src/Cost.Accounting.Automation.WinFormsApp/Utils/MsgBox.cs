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

        /// <summary>
        /// Grid içindeki satırlar silinmeden önce ortak onay penceresi.
        /// Düzenleme formlarındaki "Satır Sil" düğmeleri bu metodu kullanır;
        /// böylece onay metni program genelinde aynıdır.
        /// </summary>
        public static DialogResult ConfirmRowDelete(int count, string itemLabel)
        {
            string message = count == 1
                ? $"Seçili {itemLabel} satırı silinecek."
                : $"{count} {itemLabel} satırı silinecek.";

            return Confirm($"{message}\n\nEmin misiniz?", "Silme Onayı");
        }

        /// <summary>
        /// Yalnızca bilgi veren pencere. Onay bekleyen kayıt gibi "farkında olun"
        /// mesajları için kullanılır; Evet/Hayır sormaz.
        /// </summary>
        public static void Notice(IWin32Window? owner, string message, string title = "Bilgi")
        {
            XtraMessageBoxArgs args = new()
            {
                Owner = owner,
                Caption = title,
                Text = message,
                Buttons = new DialogResult[] { DialogResult.OK },
                Icon = SystemIcons.Information
            };

            args.Showing += (s, e) =>
            {
                e.Buttons[DialogResult.OK].Text = "Tamam";
                ForceTopMost(e.Form);
            };

            XtraMessageBox.Show(args);
        }

        public static void Notice(string message, string title = "Bilgi")
            => Notice(null, message, title);

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