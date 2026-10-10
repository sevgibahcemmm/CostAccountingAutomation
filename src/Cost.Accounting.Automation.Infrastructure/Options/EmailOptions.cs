namespace Cost.Accounting.Automation.Infrastructure.Options
{
    /// <summary>
    /// Şifre sıfırlama kodu e-postalarının gönderim ayarları.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Varsayılan olarak kapalıdır (<see cref="Enabled"/> = false). Kapalıyken
    /// sıfırlama kodu üretimi sistem yöneticisine aittir (Kullanıcılar ekranından
    /// "Şifre Sıfırlama Kodu Üret"). Açıldığında "Şifremi unuttum" akışı kodu
    /// üretir ve kayıtlı e-posta adresine gönderir.
    /// </para>
    /// <para>
    /// Parolalar (UserName/Password) gizlidir ve appsettings.json'a yazılmaz;
    /// makineye özel appsettings.Local.json ya da ortam değişkeni ile verilir
    /// (ör. <c>Email__Password</c>).
    /// </para>
    /// </remarks>
    public sealed class EmailOptions
    {
        public const string SectionName = "Email";

        /// <summary>
        /// E-posta gönderimi açık mı? Açık kabul edilmesi için ayrıca
        /// <see cref="Host"/> ve <see cref="FromAddress"/> dolu olmalıdır.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>SMTP sunucu adresi.</summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>SMTP portu. SSL kullanılmayan yaygın gönderim 25/587'dir.</summary>
        public int Port { get; set; } = 587;

        /// <summary>Bağlantıda TLS/SSL kullanılsın mı?</summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>SMTP kimlik doğrulama kullanıcı adı. Boşsa anonim denenir.</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>SMTP kimlik doğrulama parolası (uygulama parolası önerilir).</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gönderen e-posta adresi.</summary>
        public string FromAddress { get; set; } = string.Empty;

        /// <summary>Gönderen görünen adı.</summary>
        public string FromName { get; set; } = "Cost Accounting Automation";

        public bool IsFullyConfigured =>
            Enabled
            && !string.IsNullOrWhiteSpace(Host)
            && !string.IsNullOrWhiteSpace(FromAddress);
    }
}