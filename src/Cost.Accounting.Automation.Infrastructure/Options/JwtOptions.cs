namespace Cost.Accounting.Automation.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// HS512 algoritmasının gerektirdiği en kısa anahtar uzunluğu (bayt).
    /// </summary>
    /// <remarks>
    /// HMAC-SHA512'in blok boyutu 128 bayttır. Daha kısa bir anahtar verilirse
    /// kütüphane ya <see cref="ArgumentException"/> fırlatır ya da anahtarı
    /// sessizce sıfırla doldurur. İkisi de kafa karıştırıcıdır; bu yüzden
    /// uzunluk burada açıkça denetlenir.
    /// </remarks>
    public const int MinimumSecretKeyLength = 64;

    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;

    /// <summary>
    /// Token imzalama anahtarı. <b>Gizli değerdir ve dosyada tutulmaz.</b>
    /// </summary>
    /// <remarks>
    /// <c>appsettings.json</c> izlenen (tracked) bir dosyadır; anahtarı oraya
    /// yazmak anahtarı tüm commit geçmişine yaymak demektir. Bu yüzden yalnızca
    /// ortam değişkeninden gelir: <c>Jwt__SecretKey</c>.
    /// </remarks>
    public string SecretKey { get; set; } = default!;

    public int ExpirationMinutes { get; set; } = 480;

    /// <summary>
    /// Gizli değerlerin gerçekten tanımlı olduğunu doğrular.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Anahtar <c>appsettings.json</c>'da bulunmadığı için eksikliği ilk
    /// giriş denemesinde değil, <b>program açılışında</b> bildirilir. Aksi hâlde
    /// kullanıcı giriş ekranında "geçersiz kullanıcı adı" sanıp gerçek nedeni
    /// göremezdi.
    /// </para>
    /// <para>
    /// Tasarım zamanı (migration) araçları bu doğrulamayı çağırmaz; onlar
    /// veritabanı şeması üretir, token imzalamaz.
    /// </para>
    /// </remarks>
    public void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new InvalidOperationException(
                "Jwt:SecretKey tanımlı değil."
                + Environment.NewLine + Environment.NewLine
                + "Anahtar gizli bir değerdir ve appsettings.json dosyasında tutulmaz."
                + " Ortam değişkeni ile verilir:" + Environment.NewLine
                + Environment.NewLine
                + "    setx Jwt__SecretKey \"" + new string('x', 8) + "... (en az "
                + MinimumSecretKeyLength + " karakter)\"" + Environment.NewLine + Environment.NewLine
                + "Rastgele bir anahtar üretip kaydetmek için: src\\Set-LocalSecrets.ps1");
        }

        if (SecretKey.Length < MinimumSecretKeyLength)
        {
            throw new InvalidOperationException(
                $"Jwt:SecretKey en az {MinimumSecretKeyLength} karakter olmalıdır "
                + $"(HS512 gereği). Mevcut uzunluk: {SecretKey.Length}.");
        }
    }
}
