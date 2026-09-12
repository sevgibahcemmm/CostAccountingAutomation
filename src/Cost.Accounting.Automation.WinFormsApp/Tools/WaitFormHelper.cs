using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Microsoft.Extensions.DependencyInjection;

namespace Cost.Accounting.Automation.WinFormsApp.Tools
{
    public static class WaitFormHelper
    {
        public static TForm Show<TForm>(string caption, string description, bool showInTaskbar = false)
            where TForm : Form
        {
            TForm form = Program.Services.GetRequiredService<TForm>();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.ShowInTaskbar = showInTaskbar;

            if (form is WaitForm waitForm)
            {
                waitForm.SetCaption(caption);
                waitForm.SetDescription(description);
            }

            form.Show();
            form.BringToFront();
            form.Refresh();
            return form;
        }
    }
}