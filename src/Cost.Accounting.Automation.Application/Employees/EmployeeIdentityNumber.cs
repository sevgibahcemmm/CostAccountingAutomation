namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// T.C. Kimlik Numarası doğrulaması.
///
/// Personel kaydında yanlış TC girilmesi, raporda resmî bir belgeye yanlış
/// kişinin adının basılmasına yol açar; bu yüzden 11 haneli algoritma
/// doğrulaması kayıt anında yapılır.
///
/// <para>
/// <b>Doğru algoritma (KTC):</b> ilk 9 hane tek ve çift sıralı toplanır,
/// 10. hane <c>((tekToplam * 7) - çiftToplam) % 10</c>, 11. hane ise
/// <c>(10. hane + ilk 10 hanenin toplamı) % 10</c> ile bulunur.
///
/// <b>Dikkat:</b> "ilk 9 haneyi 1..9 ağırlığıyla toplayıp 11'e bölmek"
/// yaklaşımı YANLIŞTIR ve gerçek kimlik numaralarını reddeder. Bu tuzağa
/// düşülmemesi için testlerde geçerli numaralar bağımsız olarak üretilip
/// doğrulanmalıdır.
/// </para>
/// </summary>
public static class EmployeeIdentityNumber
{
    /// <summary>
    /// Numara 11 karakter, tamamı rakam, ilk hanesi sıfır değil ve kontrol
    /// haneleri yukarıdaki algoritmayla eşleşiyor olmalıdır.
    /// </summary>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();

        if (trimmed.Length != 11 || !trimmed.All(char.IsAsciiDigit))
        {
            return false;
        }

        if (trimmed[0] == '0')
        {
            return false;
        }

        int[] digits = [.. trimmed.Select(c => c - '0')];

        // 1'den 9'a kadar numaralandırıldığında tek sıra 1-3-5-7-9,
        // çift sıra 2-4-6-8 karşılık gelir (0 tabanlı dizide 0-2-4-6-8 / 1-3-5-7).
        int oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        int evenSum = digits[1] + digits[3] + digits[5] + digits[7];

        int tenth = Mod10((oddSum * 7) - evenSum);

        if (tenth != digits[9])
        {
            return false;
        }

        int eleventh = Mod10(tenth + digits.Take(10).Sum());

        return eleventh == digits[10];
    }

    /// <summary>
    /// C#'te negatif sayılarda <c>%</c> negatif sonuç verir; TC algoritmasında
    /// sonuç her zaman 0-9 aralığında olmalıdır.
    /// </summary>
    private static int Mod10(int value)
        => ((value % 10) + 10) % 10;

    /// <summary>
    /// Verilen ilk 9 haneye göre geçerli 10. ve 11. haneleri üretir.
    /// Arayüzde kullanıcıya örnek göstermek ve test verisi hazırlamak için
    /// kullanılır; doğrulama yine <see cref="IsValid"/> ile yapılır.
    /// </summary>
    public static string? Create(string firstNineDigits)
    {
        if (firstNineDigits.Length != 9
            || !firstNineDigits.All(char.IsAsciiDigit)
            || firstNineDigits[0] == '0')
        {
            return null;
        }

        int[] digits = [.. firstNineDigits.Select(c => c - '0')];

        int oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        int evenSum = digits[1] + digits[3] + digits[5] + digits[7];

        int tenth = Mod10((oddSum * 7) - evenSum);

        int eleventh = Mod10(tenth + digits.Sum() + tenth);

        return firstNineDigits + tenth.ToString() + eleventh.ToString();
    }
}