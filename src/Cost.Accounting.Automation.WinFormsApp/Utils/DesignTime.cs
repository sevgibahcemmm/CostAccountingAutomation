using System.ComponentModel;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Tasarım yüzeyi ile çalışma zamanını ayıran ortak yardımcıdır.
    ///
    /// Visual Studio Windows Forms tasarımcısı bir formu yalnızca
    /// <b>parametresiz</b> yapıcıyla örnekleyebilir. Bu yüzden veri bekleyen
    /// (ör. <c>new UrunEditForm(dto)</c>) formlara, düzenlerini tasarım
    /// yüzeyinde de görülebilmesi için parametresiz bir yapıcı eklenir.
    ///
    /// Bu yapıcı çalışma zamanında yanlışlıkla çağrılırsa form eksik veriyle
    /// açılacağı için sessizce geçilmez; <see cref="Guard"/> ile açık bir hata
    /// üretilir.
    /// </summary>
    internal static class DesignTime
    {
        /// <summary>
        /// <c>true</c> ise kod Visual Studio tasarım yüzeyi içinde
        /// çalıştırılmaktadır.
        /// </summary>
        /// <remarks>
        /// <c>Control.DesignMode</c> yapıcı içinde henüz <c>false</c> döndüğü
        /// için burada kullanılmaz; <see cref="LicenseManager.UsageMode"/> bu
        /// noktada güvenilir sonucu verir.
        /// </remarks>
        public static bool IsDesignTime
            => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>
        /// Yalnızca tasarım yüzeyi için eklenen parametresiz yapıcıları
        /// çalışma zamanında engeller.
        /// </summary>
        /// <param name="formType">Yapıcısı çağrılan formun türü.</param>
        /// <exception cref="InvalidOperationException">
        /// Tasarım yüzeyi dışında çağrıldığında.
        /// </exception>
        public static void Guard(Type formType)
        {
            if (IsDesignTime)
            {
                return;
            }

            throw new InvalidOperationException(
                $"{formType.Name} sınıfının parametresiz yapıcısı yalnızca Visual Studio tasarım " +
                "yüzeyi içindir. Çalışma zamanında formu verisiyle birlikte açan yapıcıyı kullanın.");
        }
    }
}