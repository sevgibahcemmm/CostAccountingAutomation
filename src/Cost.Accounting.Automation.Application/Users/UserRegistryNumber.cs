namespace Cost.Accounting.Automation.Application.Users;

/// <summary>Sicil numarası alanının ortak kuralları.</summary>
public static class UserRegistryNumber
{
    /// <summary>
    /// Girilen sicil numarasını aranmaya uygun hâle getirir.
    ///
    /// <para>
    /// Kırpma işlemi <see cref="Domain.Users.User.SetRegistryNumber"/> ile
    /// aynıdır; amaç ikisinin de boş string'i <c>null</c> saymasıdır. Aksi
    /// hâlde "tekrar" kontrolü boş değerlerde yanlışlıkla tetiklenir ve
    /// sicil girilmemiş iki kullanıcı birbirini engeller.
    /// </para>
    /// </summary>
    public static string? Normalize(string? registryNumber)
    {
        if (string.IsNullOrWhiteSpace(registryNumber))
        {
            return null;
        }

        string trimmed = registryNumber.Trim();

        return trimmed.Length == 0 ? null : trimmed;
    }

    /// <summary>
    /// Sicil numarasının biçim denetimi.
    ///
    /// <para>
    /// Alan opsiyoneldir; boş bırakılabilir. Boş değilse harf ve rakam (ve kısa
    /// tire) dışında karakter kabul edilmez. Uzunluk için üst sınır konmaz: sicil
    /// numarasının biçimi kurumdan kuruma değişebilir, yalnızca veritabanı
    /// kolonundaki uzunluk sınırı geçerlidir.
    /// </para>
    /// </summary>
    public static bool IsValid(string? registryNumber)
    {
        if (string.IsNullOrWhiteSpace(registryNumber))
        {
            return true;
        }

        string trimmed = registryNumber.Trim();

        return !trimmed.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-');
    }

    /// <summary>Geçersiz sicil numarası için kullanıcıya gösterilen metin.</summary>
    public const string InvalidFormatMessage = "Sicil numarası yalnızca harf, rakam ve tire içerebilir";
}