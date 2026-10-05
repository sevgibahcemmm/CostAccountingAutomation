using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Messages;

/// <summary>
/// Kullanıcılar arası tek yönlü bir mesaj veya duyuru kaydı.
/// </summary>
/// <remarks>
/// <para>
/// <b>Mesajlaşma modeli.</b> Kayıt "içerik + gönderen + alıcı" üçlüsüdür. Konuşma
/// (thread) ayrı bir nesne değildir; iki kullanıcı arasındaki mesajların kümesidir
/// ve sorgu tarafından hesaplanır. Bu, ayrı bir tabloya ve senkronizasyon
/// mantığına gerek kalmadan geçmiş sunar.
/// </para>
/// <para>
/// <b>Neden duyuru da ayrı satırlarla tutuluyor?</b> Bir duyuru N kullanıcıya
/// gönderildiğinde her alıcı için bir satır yazılır. Böylece okunmamış/okundu
/// durumu kullanıcı başına ayrı tutulur ve gelen kutusu tek bir sorguyla çalışır;
/// ayrı bir "duyuru alıcıları" tablosuna gerek kalmaz. N kullanıcıya gönderilen bir
/// duyuru N satırdır; bu, N kişilik bir kurumda önemsiz bir maliyettir.
/// </para>
/// <para>
/// <b>Konum.</b> Kayıt master (merkezi) veritabanında tutulur: kullanıcılar orada
/// yaşar, mesajlar kurumlar arasıdır ve kullanıcı giriş ekranından önce yıl
/// veritabanı seçmeden erişilmelidir.
/// </para>
/// </remarks>
public sealed class UserMessage : Entity
{
    private UserMessage() { }

    public UserMessage(
        IdentityId senderId,
        IdentityId recipientId,
        MessageBody body,
        MessageSubject? subject = null,
        bool isAnnouncement = false)
    {
        SenderId = senderId;
        RecipientId = recipientId;
        Body = body;
        Subject = subject;
        IsAnnouncement = isAnnouncement;
        ReadState = new MessageReadState(false);
    }

    /// <summary>Gönderen kullanıcı.</summary>
    public IdentityId SenderId { get; private set; } = default!;

    /// <summary>Alıcı kullanıcı.</summary>
    public IdentityId RecipientId { get; private set; } = default!;

    public MessageSubject? Subject { get; private set; }

    public MessageBody Body { get; private set; } = default!;

    /// <summary>
    /// Bu kayıt bir duyuru mu? Duyurular aynı gönderenden gelen tek bir konuşma
    /// görünümünde toplanır.
    /// </summary>
    public bool IsAnnouncement { get; private set; }

    public MessageReadState ReadState { get; private set; } = default!;

    /// <summary>Okunduğu an. Okunmadıysa <c>null</c>.</summary>
    public DateTimeOffset? ReadAt { get; private set; }

    /// <summary>
    /// Mesajı okundu olarak işaretler.
    /// </summary>
    /// <remarks>
    /// Zaten okunmuşsa <b>hiçbir şey değiştirmez</b>. Bu bilinçlidir: gelen kutusu
    /// her açıldığında tüm konuşmalar taranır ve her seferinde <c>ReadAt</c> yenilenseydi
    /// denetim izi anlamsız biçimde güncellenirdi.
    /// </remarks>
    public void MarkAsRead(DateTimeOffset now)
    {
        if (ReadState.Value)
        {
            return;
        }

        ReadState = new MessageReadState(true);
        ReadAt = now;
    }

    /// <summary>
    /// Mesajı siler (yumuşak silme).
    /// </summary>
    /// <remarks>
    /// Gönderen ve alıcı tarafından aynı işlem kullanılır: kayıt iki taraf için de
    /// ortaktır. Bu yüzden ayrı bir metin gerekmez; taban sınıftaki
    /// <see cref="Entity.Delete"/> yeterlidir.
    /// </remarks>
    public void MarkAsDeleted() => base.Delete();
}
