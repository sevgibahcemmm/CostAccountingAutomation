using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Rakam sınırlı, yazarken biçimlenen metin alanı bağlantısı.
    ///
    /// <para>
    /// DevExpress <c>InputMask</c> ve <c>MaskedTextEdit</c> maskeleri sabit
    /// karakterleri (boşluk, parantez) maskenin bir parçası sayar; veritabanında
    /// düz rakam olarak saklanan bir değer ("02161234501") alana yüklenirken
    /// maskeden geçmeyebilir. Bu yüzden maske yerine metin her değiştiğinde
    /// yeniden biçimlendirilir: kullanıcı yalnızca rakam yazar, ayraçları
    /// uygulama kendisi ekler.
    /// </para>
    ///
    /// <para>
    /// <b>Neden <c>TextChanged</c>:</b> DevExpress <c>EditValueChanged</c> olayı
    /// yalnızca düzenleyici değeri commit edildiğinde tetiklendiği için yazarki
    /// her tuşta güvence vermez. <c>TextChanged</c> ise WinForms sözleşmesi
    /// gereği hem her tuşta hem de programatik atamada tetiklenir.
    /// </para>
    ///
    /// <para>
    /// Rakam sınırı iki yerden korunur: <c>KeyPress</c> sınır dolduğunda tuşu
    /// reddeder, <see cref="Digits"/> ise yapıştırılan metni kırpar.
    /// </para>
    /// </summary>
    internal static class DigitMaskInput
    {
        /// <summary>
        /// Verilen düzenleyiciye rakam sınırlı, yazarken biçimlenen giriş
        /// davranışı bağlar.
        /// </summary>
        /// <param name="editor">Biçimlendirilecek metin alanı.</param>
        /// <param name="maxDigits">Kabul edilecek en uzun rakam sayısı.</param>
        /// <param name="format">Rakamları biçimli metne çeviren fonksiyon.</param>
        public static void Attach(TextEdit editor, int maxDigits, Func<string, string> format)
        {
            ArgumentNullException.ThrowIfNull(editor);
            ArgumentNullException.ThrowIfNull(format);

            // Karakter sınırı, biçimlenmiş TAM değerin uzunluğundan hesaplanır.
            // Ham rakam sayısı (11) kullanılırsa "123 456 789 55" gibi 14
            // karakterlik biçimli metin kesilir ve alanda hane kaybolur; bu
            // yüzden sınır biçimleyicinin çıktısından türetilir.
            editor.Properties.MaxLength = format(new string('1', maxDigits)).Length;

            editor.KeyPress += (_, e) =>
            {
                if (char.IsControl(e.KeyChar))
                {
                    return;
                }

                // Yalnızca rakam kabul edilir; ayraçları uygulama kendisi yazar.
                if (!char.IsAsciiDigit(e.KeyChar))
                {
                    e.Handled = true;
                    return;
                }

                // Sınır dolduysa yeni rakam alınmaz: alan "sınırsız" kalmaz.
                if (CountDigits(editor.Text, maxDigits) >= maxDigits)
                {
                    e.Handled = true;
                }
            };

            bool applying = false;

            editor.TextChanged += (_, _) =>
            {
                if (applying)
                {
                    return;
                }

                string formatted = format(editor.Text);

                if (string.Equals(editor.Text, formatted, StringComparison.Ordinal))
                {
                    return;
                }

                applying = true;

                try
                {
                    editor.Text = formatted;
                    editor.Select(formatted.Length, 0);
                }
                finally
                {
                    applying = false;
                }
            };
        }

        /// <summary>
        /// Metinden rakamları sırayla alır; en fazla <paramref name="maxDigits"/>
        /// hane tutulur, kalanı atılır.
        /// </summary>
        public static string Digits(string? value, int maxDigits)
        {
            if (string.IsNullOrEmpty(value) || maxDigits <= 0)
            {
                return string.Empty;
            }

            var digits = new System.Text.StringBuilder(maxDigits);

            foreach (char c in value)
            {
                if (!char.IsAsciiDigit(c))
                {
                    continue;
                }

                digits.Append(c);

                if (digits.Length == maxDigits)
                {
                    break;
                }
            }

            return digits.ToString();
        }

        /// <summary>Metindeki rakam sayısını (en fazla <paramref name="maxDigits"/>) sayar.</summary>
        private static int CountDigits(string? value, int maxDigits)
            => Digits(value, maxDigits).Length;
    }
}
