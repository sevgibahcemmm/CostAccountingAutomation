using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Telefon numarası alanları için yazarken biçimlendirme.
    ///
    /// <para>
    /// Kullanıcı yalnızca rakam yazar; ayraçları uygulama kendisi ekler.
    /// Biçimlendirme ilerlemeli (progressive) çalışır: "0" → "0 (2" →
    /// "0 (216)" → "0 (216) 1" → "0 (216) 123 45 01".
    /// </para>
    ///
    /// <para>
    /// Alan en fazla 11 rakam kabul eder (ülke kodu + 10 hane). Kayıtta düz
    /// rakam saklanır; biçim yalnızca ekranda görünür.
    /// </para>
    /// </summary>
    public static class PhoneNumberFormatter
    {
        /// <summary>Alan kabul ettiği en uzun rakam sayısı (ülke kodu + 10 hane).</summary>
        public const int MaxDigits = 11;

        /// <summary>Verilen metni Türkçe telefon biçimine göre yeniden yazar.</summary>
        public static string Format(string? value)
        {
            string digits = DigitMaskInput.Digits(value, MaxDigits);

            if (digits.Length == 0)
            {
                return string.Empty;
            }

            // Kısa girdilerde parantez açmak için yeterli hane yoktur; rakamlar
            // olduğu gibi bırakılır.
            if (digits.Length <= 4)
            {
                return digits;
            }

            bool hasCountryCode = digits[0] == '0';

            string areaCode = hasCountryCode ? digits[1..4] : digits[..3];
            string subscriber = hasCountryCode ? digits[4..] : digits[3..];

            return $"0 ({areaCode}) {GroupSubscriber(subscriber)}";
        }

        /// <summary>
        /// Abone numarasını 3-2-2 gruplarına ayırır. Eksik haneler son grupta
        /// birleştirilir.
        /// </summary>
        private static string GroupSubscriber(string subscriber)
        {
            if (subscriber.Length <= 3)
            {
                return subscriber;
            }

            if (subscriber.Length <= 5)
            {
                return $"{subscriber[..3]} {subscriber[3..]}";
            }

            if (subscriber.Length <= 7)
            {
                return $"{subscriber[..3]} {subscriber[3..5]} {subscriber[5..]}";
            }

            return $"{subscriber[..3]} {subscriber[3..5]} {subscriber[5..]} {subscriber[7..]}";
        }

        /// <summary>Verilen alana rakam sınırlı, biçimlenen giriş davranışını bağlar.</summary>
        public static void Attach(TextEdit editor)
            => DigitMaskInput.Attach(editor, MaxDigits, Format);
    }
}
