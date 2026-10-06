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
    /// Alıcının mesajı gördüğü an. Henüz görülmediyse <c>null</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// WhatsApp'taki teslim/okunma ayrımının karşılığıdır. Üç durum vardır:
    /// </para>
    /// <list type="bullet">
    /// <item><b>Gönderildi</b>: kayıt yazıldı (<see cref="Entity.CreatedAt"/>).</item>
    /// <item><b>Teslim edildi</b>: alıcının istemcisi mesajı ekranda gösterdi
    /// (<see cref="DeliveredAt"/>).</item>
    /// <item><b>Okundu</b>: alıcı konuşmayı açtı (<see cref="ReadAt"/>).</item>
    /// </list>
    /// <para>
    /// Gönderen için "teslim" anlamı vardır, gönderilmemiş bir mesajda
    /// <c>null</c> kalır.
    /// </para>
    /// </remarks>
    public DateTimeOffset? DeliveredAt { get; private set; }

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
        // Okunan mesaj aynı zamanda teslim edilmiş sayılır. Aksi hâlde alıcı
        /// mesajı okumuş ama gönderende hâlâ "teslim edilmedi" görünürdü.
        MarkDelivered(now);

        if (ReadState.Value)
        {
            return;
        }

        ReadState = new MessageReadState(true);
        ReadAt = now;
    }

    /// <summary>
    /// Mesajı teslim edilmiş olarak işaretler.
    /// </summary>
    /// <remarks>
    /// Zaten teslim edilmişse hiçbir şey değiştirmez. Zaten okunmuşsa da
    /// değiştirmez: okunma teslimin üstündedir ve okunma zamanı teslim zamanını
    /// da belirler.
    /// </remarks>
    public void MarkDelivered(DateTimeOffset now)
    {
        if (DeliveredAt is not null || ReadAt is not null)
        {
            return;
        }

        DeliveredAt = now;
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
