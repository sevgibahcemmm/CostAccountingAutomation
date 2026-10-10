using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;

namespace Cost.Accounting.Automation.Domain.Users;
public sealed class User : Entity
{
    public User(
        FirstName firstName,
        LastName lastName,
        Email email,
        UserName userName,
        Password password,
        IdentityId companyId,
        IdentityId roleId,
        bool isActive,
        TRIdentityNumber? tRIdentityNumber = null
        )
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetUserName(userName);
        SetPassword(password);
        SetFullName();
        SetIsForgotPasswordCompleted(new(true));
        SetCompanyId(companyId);
        SetRoleId(roleId);
        SetStatus(isActive);
        SetTRIdentityNumber(tRIdentityNumber);
        ResolveDuplicateKey();
    }

    private User()
    {
        ForgotPasswordCode = null;
        ForgotPasswordDate = null;
        IsForgotPasswordCompleted = new(true);
    }
    public FirstName FirstName { get; private set; } = default!;
    public LastName LastName { get; private set; } = default!;
    public FullName FullName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public UserName UserName { get; private set; } = default!;
    public Password Password { get; private set; } = default!;
    public ForgotPasswordCode? ForgotPasswordCode { get; private set; }
    public ForgotPasswordDate? ForgotPasswordDate { get; private set; }
    public IsForgotPasswordCompleted IsForgotPasswordCompleted { get; private set; } = default!;
    public IdentityId CompanyId { get; private set; } = default!;
    public IdentityId RoleId { get; private set; } = default!;
    public TRIdentityNumber? TRIdentityNumber { get; private set; }

    /// <summary>
    /// Kurum sicil numarası.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Personel kaydında <c>Employee.RegistryNumber</c> olarak da tutulur. Buraya
    /// eklenmesinin nedeni: mesajlaşma <b>kullanıcılar</b> arasında olur ve
    /// alıcının giriş ekranında bir kimlikle bulunabilmesi gerekir. Sicil
    /// numarası, sistem kullanıcısı ile personel kaydının eşleştirilmesini sağlayan
    /// doğal anahtardır.
    /// </para>
    /// <para>Zorunlu değildir; numarası olmayan kullanıcılar TC kimlik numarası ya da kullanıcı adıyla bulunur.</para>
    /// </remarks>
    public string? RegistryNumber { get; private set; }

    /// <summary>Şifre sıfırlama kodunu kimin ürettiği (denetim).</summary>
    /// <remarks>
    /// <para>
    /// <see cref="UpdatedBy"/> değil: sıfırlama, giriş ekranından oturum açılmadan
    /// yapılır; kullanıcı kimliği yokken <c>EntityAuditTracker</c> tarafından
    /// <c>UpdatedBy</c> NULL'e ezilir ve "hangi yönetici kodu üretti" bilgisi
    /// kaybolur. Bu alan yalnızca kod üretiminde yazılır ve hiçbir sonraki işlemde
    /// silinmez.
    /// </para>
    /// </remarks>
    public IdentityId? PasswordResetIssuedBy { get; private set; }

    /// <summary>Şifre sıfırlama kodunun üretildiği zaman (denetim).</summary>
    public DateTimeOffset? PasswordResetIssuedAt { get; private set; }

    /// <summary>Şifre sıfırlama kodunun kullanılıp sıfırlamanın tamamlandığı zaman (denetim).</summary>
    public DateTimeOffset? PasswordResetCompletedAt { get; private set; }

    /// <summary>Sicil numarasını kırpıp atar; boşsa <c>null</c> yapar.</summary>
    public void SetRegistryNumber(string? registryNumber)
    {
        RegistryNumber = string.IsNullOrWhiteSpace(registryNumber)
            ? null
            : registryNumber.Trim();
    }

    /// <summary>
    /// Kullanıcı avatarının dosya depolama köküne göreli yolu.
    /// Kullanıcı verileri master veritabanında tutulduğundan avatar da
    /// <c>Photo</c> tablosunda değil, doğrudan bu alanda saklanır.
    /// </summary>
    public string? AvatarPath { get; private set; }

    public static string? BuildDuplicateKey(string userName)
        => DuplicateKeyRule.From(userName);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(UserName.Value));

    #region Behaviors
    /// <summary>
    /// Sıfırlama kodunun geçerli kalacağı süre.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Daha önce 24 saat idi ve bu çok uzun bir pencere: ele geçirilen bir kod
    /// bir gün boyunca kullanılabiliyordu. Kurtarma kodları kısa ömürlü olmalıdır;
    /// kullanıcı kodu yöneticiden alır ve hemen girer.
    /// </para>
    /// <para>
    /// Tek bir kaynaktan okunur. Üç ayrı yerde <c>AddDays(1)</c> yazılıydı ve
    /// birinin değiştirilip diğerlerinin unutulması kolaydı.
    /// </para>
    /// </remarks>
    public static readonly TimeSpan PasswordResetCodeValidity = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Parolayı doğrular ve gerekiyorsa hash'i güncel biçime yükseltir.
    /// </summary>
    public PasswordVerification VerifyPassword(string password)
        => Password.VerifyAndUpgradeIfNeeded(password);

    /// <summary>
    /// Sıfırlama talebinin sahibi olan kullanıcı için yeni kod üretir.
    /// </summary>
    public void CreatePasswordResetRequest()
    {
        ForgotPasswordCode = new(Guid.CreateVersion7());
        ForgotPasswordDate = new(DateTimeOffset.Now);
        IsForgotPasswordCompleted = new(false);

        // Denetim: kod üretildi. IssuedBy ayrıca atanır (yönetici kimliği
        // yalnızca provider katmanından bilinir).
        PasswordResetIssuedAt = DateTimeOffset.Now;
    }

    /// <summary>
    /// Kod üreten yöneticinin kimliğini denetim alanına yazar. Yalnızca ilk atamada
    /// geçerli olur; yeniden kod üretilse bile önceki denetim no kaydı korunur
    /// (üst üste yazmaz), böylece "kodu kim üretti" sorusunun genel cevabı değişmez.
    /// </summary>
    public void SetPasswordResetIssuedBy(IdentityId adminId)
    {
        PasswordResetIssuedBy = PasswordResetIssuedBy ?? adminId;
    }

    /// <summary>
    /// Üretilmiş sıfırlama kodunu geçersiz kılar.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Bu çağrı zorunludur.</b> Kod üretildikten sonra işaretlenmiyorsa
    /// <see cref="IsForgotPasswordCompleted"/> <c>false</c> kalmaya devam eder ve
    /// aynı kod 24 saat boyunca sınırsız kez yeniden kullanılabilir. Sıfırlama
    /// başarılı olduğunda kodun geçersizleştirilmesi, tek kullanımlılığı sağlar.
    /// </para>
    /// <para>
    /// Kod alanı da temizlenir: artık geçersiz olan bir değer veritabanında
    /// gereksiz yere durmasın.
    /// </para>
    /// </remarks>
    public void MarkPasswordResetCompleted()
    {
        IsForgotPasswordCompleted = new(true);
        ForgotPasswordCode = null;
        ForgotPasswordDate = null;

        // Denetim: sıfırlama tamamlandığı an. Kod artık temizlendiği için
        // "ne zaman tamamlandı" bilgisi burada korunur.
        PasswordResetCompletedAt = DateTimeOffset.Now;
    }

    /// <summary>
    /// Sıfırlama kodu hâlâ geçerli mi?
    /// </summary>
    public bool IsPasswordResetCodeUsable() =>
        ForgotPasswordCode is not null
        && IsForgotPasswordCompleted.Value == false
        && ForgotPasswordDate is not null
        && DateTimeOffset.Now < ForgotPasswordDate.Value.Add(PasswordResetCodeValidity);

    public void SetFirstName(FirstName firstName)
    {
        FirstName = firstName;
    }

    public void SetLastName(LastName lastName)
    {
        LastName = lastName;
    }

    public void SetEmail(Email email)
    {
        Email = email;
    }

    public void SetUserName(UserName userName)
    {
        UserName = userName;
        ResolveDuplicateKey();
    }

    public void SetFullName()
    {
        FullName = new(FirstName.Value + " " + LastName.Value + " (" + Email.Value + ")");
    }

    public void SetPassword(Password password)
    {
        Password = password;
    }

    public void SetIsForgotPasswordCompleted(IsForgotPasswordCompleted isForgotPasswordCompleted)
    {
        IsForgotPasswordCompleted = isForgotPasswordCompleted;
    }

    public void SetCompanyId(IdentityId companyId)
    {
        CompanyId = companyId;
    }

    public void SetRoleId(IdentityId roleId)
    {
        RoleId = roleId;
    }

    public void SetTRIdentityNumber(TRIdentityNumber? tRIdentityNumber)
    {
        TRIdentityNumber = tRIdentityNumber;
    }

    public void SetAvatarPath(string? avatarPath)
    {
        AvatarPath = avatarPath;
    }
    #endregion
}