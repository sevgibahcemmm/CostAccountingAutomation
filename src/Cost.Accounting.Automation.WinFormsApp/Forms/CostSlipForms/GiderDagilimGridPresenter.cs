using Cost.Accounting.Automation.WinFormsApp.Reports;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public static class GiderDagilimGridPresenter
    {
        public static void Show()
        {
            var form = new GiderDagilimGridForm
            {
                Text = "Gider Dağıtım Tablosu",
                WindowState = FormWindowState.Maximized,
                StartPosition = FormStartPosition.CenterScreen
            };
            form.Show();
        }
    }
}