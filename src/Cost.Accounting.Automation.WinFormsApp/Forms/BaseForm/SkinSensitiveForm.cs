using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    /// <summary>
    /// Modal edit formları için taban. Tüm görünüm skin tarafından yönetilir;
    /// yalnızca Oracle tarzı edit formlarının ortak davranışlarını barındırır.
    /// </summary>
    public abstract class SkinSensitiveForm : XtraForm
    {
        protected SkinSensitiveForm()
        {
        }
    }
}