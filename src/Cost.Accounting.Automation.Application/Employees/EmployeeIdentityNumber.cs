namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// T.C. Kimlik Numarası alanının <b>biçim</b> doğrulaması.
///
/// <para>
/// Sistemde tutulan numaralar gerçek nüfus kaydı değildir; kurum tarafından
/// üretilen/sanallaştırılan kimliklerdir. Bu yüzden KTC kontrol hanesi
/// (10. ve 11. hane) algoritması <b>uygulanmaz</b>: gerçek bir kişiye ait
/// olmayan bir numara kontrol hanesiyle eşleşmez ve doğrulama her kayıtta
/// yanlış hata üretirdi.
/// </para>
///
/// <para>
/// Uygulanan tek kural biçimdir: numara 11 hane olmalı ve yalnızca rakamlardan
/// oluşmalıdır. Ekranda gruplu gösterilir ("123 456 789 55"), kayıtta düz 11
/// rakam saklanır; böylece arama ve rapor çıktıları biçimden etkilenmez.
/// </para>
/// </summary>
public static class EmployeeIdentityNumber
{
    /// <summary>
    /// TC kimlik numarasının hane sayısı. Arayüzdeki alan bu sınırı aşan
    /// girişleri reddeder.
    /// </summary>
    public const int Length = 11;

    /// <summary>
    /// Metinden yalnızca rakamları alır ve <see cref="Length"/> haneye
    /// indirger. Kullanıcının yapıştırdığı "123 456 789 01" gibi gruplu
    /// değerlerin de kabul edilmesini sağlar.
    /// </summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var digits = new System.Text.StringBuilder(Length);

        foreach (char c in value)
        {
            if (!char.IsAsciiDigit(c))
            {
                continue;
            }

            digits.Append(c);

            if (digits.Length == Length)
            {
                break;
            }
        }

        return digits.ToString();
    }

    /// <summary>
    /// Numara yalnızca rakamlardan oluşan tam 11 hane ise geçerlidir. Gruplu
    /// metin de (<c>"123 456 789 55"</c>) kabul edilir.
    /// </summary>
    public static bool IsValid(string? value)
        => Validate(value) is null;

    /// <summary>
    /// Numarayı biçim açısından doğrular ve geçersizse nedenini döner.
    /// </summary>
    /// <returns>
    /// Numara geçerliyse <c>null</c>; aksi hâlde kullanıcıya gösterilecek
    /// açıklama.
    /// </returns>
    public static string? Validate(string? value)
    {
        string digits = Normalize(value);

        if (digits.Length == 0)
        {
            return "TC kimlik numarasını giriniz";
        }

        // Yalnızca uzunluk kontrolü yapılır; kontrol hanesi doğrulanmaz
        // (bkz. sınıf açıklaması).
        return digits.Length == Length
            ? null
            : $"TC kimlik numarası {Length} haneli olmalı "
                + $"(şu an {digits.Length} hane girdiniz)";
    }
}
