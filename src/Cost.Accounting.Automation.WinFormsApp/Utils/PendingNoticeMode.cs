namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Onay bekleyen kayıt bulunduğunda sayfa açılışında nasıl bilgilendirileceği.
    /// </summary>
    public enum PendingNoticeMode
    {
        /// <summary>Bilgilendirme gösterilmez.</summary>
        None = 0,

        /// <summary>Kullanıcıdan onay isteyen kalıcı pencere (MsgBox) açılır.</summary>
        MessageBox = 1,

        /// <summary>Ekranın köşesinde kaybolan toast gösterilir.</summary>
        Toast = 2
    }
}