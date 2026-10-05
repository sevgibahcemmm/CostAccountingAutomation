namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>
/// Mesajlaşma yetkilerinin tek tanım yeri.
/// </summary>
/// <remarks>
/// Komutların <c>[Permission]</c> öznitelikleri ve arayüzün düğme görünürlüğü
/// aynı sabitleri kullanır; böylece isimler ayrışamaz.
/// </remarks>
public static class MessagePermissions
{
    /// <summary>Mesajları ve konuşma geçmişini görme.</summary>
    public const string View = "message:view";

    /// <summary>Kullanıcıya mesaj gönderme.</summary>
    public const string Send = "message:send";

    /// <summary>Birden çok kullanıcıya duyuru gönderme.</summary>
    public const string Announce = "message:announce";
}