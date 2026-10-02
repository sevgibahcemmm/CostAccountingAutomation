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

AttachOwner(form);
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
        /// Bekleme penceresini sahipli pencereye bağlayıp z-sırasını güvenceye alır.
        ///
        /// Sorun: pencere sahipsiz (<c>Show()</c> ile açılıyordu) açıldığı için
        /// ana pencere etkin olduğunda arkada kalabiliyordu. Kullanıcı "Lütfen
        /// Bekleyin" penceresini düğmenin arkasında görüyor, işlemin nerede
        /// takıldığını anlayamıyordu.
        ///
        /// <c>TopMost</c> yerine DevExpress'in <c>ShowOnTopMode</c> kullanılır;
        /// <c>TopMost</c> bu sürümde kullanımdan kaldırılmıştır ve tüm
        /// pencerelerin üstüne çıktığı için zaten fazla güçlüdür.
        ///
        /// Sahip seçilirken MDI alt pencereleri dışlanır (bunlar sahiplik
        /// alamaz); sahip bulunamazsa pencere yine de ekranın üstünde kalır.
        /// </summary>
        private static void AttachOwner(Form form)
        {
            if (form is not WaitForm waitForm)
            {
                return;
            }

            Form? owner = Form.ActiveForm;

            bool canOwn = owner is not null
                && !ReferenceEquals(owner, form)
                && owner is { TopLevel: true, Visible: true };

            if (canOwn)
            {
                waitForm.Owner = owner!;
                waitForm.ShowOnTopMode = DevExpress.XtraWaitForm.ShowFormOnTopMode.AboveParent;
            }
            else
            {
                waitForm.ShowOnTopMode = DevExpress.XtraWaitForm.ShowFormOnTopMode.AboveAll;
            }
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