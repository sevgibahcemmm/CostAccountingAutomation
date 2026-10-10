using System.Net;
using System.Net.Mail;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cost.Accounting.Automation.Infrastructure.Services
{
    /// <summary>
    /// SMTP üzerinden e-posta gönderen <see cref="IEmailSender"/> uygulaması.
    /// </summary>
    /// <remarks>
    /// Gönderim en iyi çabadır: SMTP hatası (erişilememe, kimlik doğrulama
    /// başarısızlığı vb.) çağırana istisna olarak yansımaz; günlüğe yazılır ve
    /// sessizce dönülür. İstisna taşmak zorundaysa yalnızca iptal durumudur
    /// (<see cref="OperationCanceledException"/>).
    /// </remarks>
    internal sealed class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public bool IsEnabled => _options.IsFullyConfigured;

        public async Task SendPasswordResetCodeAsync(
            string toEmail,
            string recipientName,
            string code,
            DateTimeOffset expiresAt,
            CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
            {
                return;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_options.FromAddress, _options.FromName),
                    Subject = "Şifre Sıfırlama Kodu",
                    IsBodyHtml = true,
                    Body = BuildBody(recipientName, code, expiresAt)
                };

                message.To.Add(toEmail);

                bool anonymous = string.IsNullOrWhiteSpace(_options.UserName);

                using var client = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = _options.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = anonymous,
                    Credentials = anonymous
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(_options.UserName, _options.Password)
                };

                await client.SendMailAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Şifre sıfırlama kodu gönderildi. Alıcı: {Recipient}, geçerlilik: {ExpiresAt:O}",
                    toEmail,
                    expiresAt);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Şifre sıfırlama e-postası gönderilemedi. Alıcı: {Recipient}",
                    toEmail);
            }
        }

        private static string BuildBody(string recipientName, string code, DateTimeOffset expiresAt)
        {
            string expiresLocal = expiresAt.ToLocalTime().ToString("g");

            return $"""
                <!DOCTYPE html>
                <html lang="tr">
                <head>
                  <meta charset="utf-8" />
                </head>
                <body style="font-family:Segoe UI, Arial, sans-serif; color:#1f2937; line-height:1.6;">
                  <p>Merhaba {WebUtility.HtmlEncode(recipientName)},</p>
                  <p>Şifre sıfırlama talebinde bulundunuz. Aşağıdaki kod ile yeni şifrenizi
                  belirleyebilirsiniz:</p>
                  <p style="font-size:20px; font-weight:600; letter-spacing:2px; background:#f3f4f6;
                            padding:12px 16px; border-radius:8px; display:inline-block;">
                    {WebUtility.HtmlEncode(code)}
                  </p>
                  <p>Kod <b>{WebUtility.HtmlEncode(expiresLocal)}</b> tarihine kadar geçerlidir
                  ve yalnızca bir kez kullanılabilir.</p>
                  <p>Bu isteği siz yapmadıysanız e-postayı silmeniz yeterlidir; hesabınıza
                  herhangi bir işlem yapılmaz.</p>
                  <p style="color:#6b7280; font-size:12px;">
                    Bu mesaj otomatik üretilmiştir; yanıtlamayınız.
                  </p>
                </body>
                </html>
                """;
        }
    }
}