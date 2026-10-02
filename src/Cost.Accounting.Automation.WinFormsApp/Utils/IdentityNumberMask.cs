namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// TC kimlik no alanı için rakam sınırlı, yazarken biçimlenen giriş.
    ///
    /// <para>
    /// Alan yalnızca rakam kabul eder ve 11 haneyi geçmez. Hane sayısı sabit
    /// olduğu için biçimlendirme ilerlemeli değildir: 3 - 3 - 3 - 2 gruplanır
    /// ("123 456 789 01"). Kayıtta düz rakam saklanır; biçim yalnızca ekranda
    /// görünür, böylece arama ve rapor çıktıları değişmez.
    /// </para>
    /// </summary>
    public static class IdentityNumberMask
    {
        /// <summary>
        /// TC kimlik no hane sayısı. Alan sınırı ve doğrulama aynı kaynaktan
        /// beslenir; iki ayrı sabit tutulursa alan ile doğrulama birbirinden
        /// ayrılabilir.
        /// </summary>
        public const int MaxDigits = Application.Employees.EmployeeIdentityNumber.Length;

        /// <summary>Görüntülemede kullanılan hane grupları.</summary>
        private static readonly int[] Groups = [3, 3, 3, 2];

        /// <summary>Metni TC kimlik no biçimine göre yeniden yazar.</summary>
        public static string Format(string? value)
        {
            string digits = DigitMaskInput.Digits(value, MaxDigits);

            if (digits.Length == 0)
            {
                return string.Empty;
            }

            var parts = new System.Collections.Generic.List<string>(Groups.Length);
            int index = 0;

            foreach (int size in Groups)
            {
                if (index >= digits.Length)
                {
                    break;
                }

                parts.Add(digits.Substring(index, Math.Min(size, digits.Length - index)));
                index += size;
            }

            return string.Join(" ", parts);
        }

        /// <summary>Verilen alana rakam sınırlı, biçimlenen giriş davranışını bağlar.</summary>
        public static void Attach(DevExpress.XtraEditors.TextEdit editor)
            => DigitMaskInput.Attach(editor, MaxDigits, Format);
    }
}
