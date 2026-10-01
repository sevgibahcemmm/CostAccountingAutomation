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

            // Pencere henüz boyanmadan iş başlayınca (örn. OnLoad içinden tetiklenen
            // ilk yükleme) kullanıcı hiçbir geri bildirim görmüyordu. Mesaj
            // döngüsü bir kez işletilerek boyama ve animasyon başlamadan işe
            // başlanmaz.
            FlushPaintQueue(form);

            return form;
        }

        /// <summary>
        /// Bekleme penceresinin gerçekten ekrana çizilmesini sağlar.
        /// </summary>
        private static void FlushPaintQueue(Form form)
        {
            try
            {
                form.Refresh();
                // Projede Cost.Accounting.Automation.Application adlı bir namespace
                // bulunduğu için System.Windows.Forms.Application tam adıyla
                // belirtilmelidir.
                System.Windows.Forms.Application.DoEvents();
            }
            catch
            {
                // Pencere kapanmış olabilir; yükleme akışı bu yüzden bozulmamalı.
            }
        }
    }
}