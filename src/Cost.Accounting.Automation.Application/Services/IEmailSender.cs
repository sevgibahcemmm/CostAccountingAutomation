namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// E-posta gönderim kanalı. Şifre sıfırlama kodunun kullanıcının kendi
/// adresine iletilmesini sağlar.
/// </summary>
/// <remarks>
/// <para>
/// Gönderim <b>en iyi çaba</b> ilkesiyle çalışır: hata oluşursa (SMTP erişilemez,
/// kimlik doğrulama başarısız vb.) uygulamaya/komuta istisna yansıtılmaz;
/// gönderim sağlayıcısı hatayı günlüğe yazar ve sessizce döner. Böylece şifre
/// sıfırlama akışı bir posta sunucusuna bağımlı kılınmaz ve e-posta kapalıyken
/// yönetici kod üretme akışı (AdminIssuePasswordResetCommand) geri planda kalır.
/// </para>
/// </remarks>
public interface IEmailSender
{
    /// <summary>
    /// E-posta gönderimi yapılandırılmış ve açık mı?
    /// </summary>
    /// <remarks>
    /// Kapalıysa çağıran, kod üretmeden yönetici kod üretme akışını izlemelidir.
    /// </remarks>
    bool IsEnabled { get; }

    /// <summary>
    /// Şifre sıfırlama kodunu <paramref name="toEmail"/> adresine gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı e-posta adresi.</param>
    /// <param name="recipientName">Alıcının adı (mesajda hitap için).</param>
    /// <param name="code">Tek kullanımlık sıfırlama kodu.</param>
    /// <param name="expiresAt">Kodun geçerlilik bitişi.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <remarks>
    /// Hata durumunda istisna <b>fırlatılmaz</b>; durum sağlayıcı tarafından
    /// günlüğe yazılır. Yalnızca iptal (CancellationToken) üst katmana akabilir.
    /// </remarks>
    Task SendPasswordResetCodeAsync(
        string toEmail,
        string recipientName,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default);
}