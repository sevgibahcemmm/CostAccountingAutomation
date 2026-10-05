namespace Cost.Accounting.Automation.Domain.Users.ValueObjects;

/// <summary>
/// Bir parolanın doğrulanma sonucu.
/// </summary>
public enum PasswordVerification
{
    /// <summary>Parola eşleşmedi.</summary>
    Failed,

    /// <summary>Parola eşleşti ve hash güncel biçimde.</summary>
    Verified,

    /// <summary>
    /// Parola eşleşti ama hash eski biçimde üretilmiş. Kullanıcı yeni hash'e
    /// yükseltilmelidir.
    /// </summary>
    /// <remarks>
    /// Bu değer "hash'i düzelt" bilgisidir, "güvensiz" bilgisi değil: parolanın
    /// kendisi doğru olduğu için giriş reddedilmez. Yükseltme başarısız olursa
    /// kullanıcı yine girebilir; sadece hash eski kalır.
    /// </remarks>
    VerifiedNeedsUpgrade
}

/// <summary>
/// Parolanın tuzlanmış özeti.
/// </summary>
/// <remarks>
/// <para>
/// <b>Biçim.</b> <see cref="HashIterations"/> sıfırdan büyükse PBKDF2-HMAC-SHA512,
/// sıfırsa eski biçim (tek turlu HMAC-SHA512, rastgele anahtar tuz olarak saklanır)
/// kullanılır. Ayrı bir sürüm kolonu yerine tur sayısı ayrım görevi görür; yeni
/// biçimler ileride eklendiğinde yalnızca burası değişir.
/// </para>
/// <para>
/// <b>Neden değiştirildi.</b> Eski biçim tek turdu ve iş faktörü yoktu. GPU ile
/// saniyede milyarlarca deneme yapılabildiği için veritabanı bir kez sızdığında
/// parolalar pratikte kırılabiliyordu. PBKDF2 tur sayısını kasıtlı olarak
/// pahalı kılar.
/// </para>
/// </remarks>
public sealed record Password
{
    /// <summary>
    /// Yeni parolalar için tur sayısı. OWASP'in HMAC-SHA512 için önerdiği değer.
    /// </summary>
    /// <remarks>
    /// Daha yüksek değer kaba kuvvet saldırısını pahalılaştırır ama **her girişte**
    /// de bu kadar iş yapılır. 210.000 tur, masaüstü donanımda giriş başına
    /// ihmal edilebilir süre ekler ve modern kılavuzlarla uyumludur.
    /// </remarks>
    public const int CurrentHashIterations = 210_000;

    private Password()
    {
    }

    public Password(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        byte[] salt = GenerateSalt();
        byte[] hash = Derive(password, salt, CurrentHashIterations);

        PasswordSalt = salt;
        PasswordHash = hash;
        HashIterations = CurrentHashIterations;
    }

    /// <summary>
    /// Kayıtlı değerlerden yeniden oluşturur (doğrulama için).
    /// </summary>
    private Password(byte[] hash, byte[] salt, int hashIterations)
    {
        PasswordHash = hash;
        PasswordSalt = salt;
        HashIterations = hashIterations;
    }

    public byte[] PasswordHash { get; private set; } = default!;

    /// <summary>
    /// Tuz. Eski biçimde bu alan aslında rastgele üretilen HMAC anahtarıdır;
    /// işlevsel olarak tuzla aynı rolü görür.
    /// </summary>
    public byte[] PasswordSalt { get; private set; } = default!;

    /// <summary>
    /// Türetme tur sayısı. <c>0</c> eski biçimi belirtir.
    /// </summary>
    public int HashIterations { get; private set; }

    public static Password FromStored(byte[] hash, byte[] salt, int hashIterations)
        => new(hash, salt, hashIterations);

    /// <summary>
    /// Düz parolayı doğrular.
    /// </summary>
    public PasswordVerification Verify(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return PasswordVerification.Failed;
        }

        if (HashIterations <= 0)
        {
            return VerifyLegacy(password);
        }

        byte[] computed = Derive(password, PasswordSalt, HashIterations);

        return FixedTimeEquals(computed, PasswordHash)
            ? PasswordVerification.Verified
            : PasswordVerification.Failed;
    }

    /// <summary>
    /// <see cref="Verify"/> sonucunu döndürür ve eski biçimdeyse hash'i
    /// güncel biçime yükseltir.
    /// </summary>
    /// <remarks>
    /// "Tembel geçiş": kullanıcılar şifre değiştirmeden de yükseltilir, dolayısıyla
    /// kimsenin dışarıda kalmaz ve ayrı bir toplu işlem gerekmez.
    /// </remarks>
    public PasswordVerification VerifyAndUpgradeIfNeeded(string password)
    {
        PasswordVerification result = Verify(password);

        if (result != PasswordVerification.VerifiedNeedsUpgrade)
        {
            return result;
        }

        byte[] salt = GenerateSalt();
        byte[] hash = Derive(password, salt, CurrentHashIterations);

        PasswordSalt = salt;
        PasswordHash = hash;
        HashIterations = CurrentHashIterations;

        return PasswordVerification.Verified;
    }

    /// <summary>
    /// Eski biçim: tek turlu HMAC-SHA512, anahtar tuz olarak saklanır.
    /// </summary>
    private PasswordVerification VerifyLegacy(string password)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512(PasswordSalt);
        byte[] computed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

        return FixedTimeEquals(computed, PasswordHash)
            ? PasswordVerification.VerifiedNeedsUpgrade
            : PasswordVerification.Failed;
    }

    /// <summary>
    /// Türetilen özetin uzunluğu (bayt). SHA-512 çıktısıyla aynı: eski biçimle
    /// aynı uzunlukta tutulur, böylece karşılaştırma ve depolama tutarlı kalır.
    /// </summary>
    private const int DerivedHashLength = 64;

    private static byte[] Derive(string password, byte[] salt, int iterations)
        => System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            System.Text.Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            System.Security.Cryptography.HashAlgorithmName.SHA512,
            DerivedHashLength);

    private static byte[] GenerateSalt()
        => System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);

    /// <summary>
    /// Sabit süreli karşılaştırma.
    /// </summary>
    /// <remarks>
    /// <c>SequenceEqual</c> ilk farklı baytta döner ve bu, "parolanın kaçıncı
    /// karakteri yanlış" bilgisini sızdırır. Zamanlama saldırısına karşı
    /// <see cref="CryptographicOperations.FixedTimeEquals(byte[], byte[])"/>
    /// kullanılır.
    /// </remarks>
    private static bool FixedTimeEquals(byte[] left, byte[] right)
        => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(left, right);
}
