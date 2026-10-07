using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Messages;

/// <summary>
/// Bir kullanıcının bir konuşmayı kendi görünümünden temizleme durumu.
/// </summary>
/// <remarks>
/// <para>
/// Temizleme <b>yönlü ve geri alınabilirdir</b>: satır yalnızca çağıran
/// kullanıcıya aittir ve o kullanıcının konuşma sorgusunda eski mesajları
/// gizler. Mesajların kendisi silinmez; karşı tarafın kopyasına hiç dokunulmaz.
/// "Geçmişi Göster" düğmesi bu satırı kaldırarak görünümü geri yükler.
/// </para>
/// <para>
/// Kullanıcı başına tek satır tutulur (özindeks: kullanıcı + karşı taraf +
/// kanal). Bu bir <b>görünüm tercihidir</b>, denetlenecek bir iş kaydı değil;
/// bu yüzden satır fiziksel olarak silinir (yumuşak silme değil), aksi hâlde
/// özindeks eski satırla çakışırdı. Bu yüzden sınıf <see cref="IHardDeletable"/>
/// uygular: <c>EntityAuditTracker</c> arayüzsüz kayıtlarda doğrudan silmeyi
/// engeller.
/// </para>
/// <para>
/// Mesajlar gibi master (merkezi) veritabanında tutulur: kimlikler orada
/// yaşar ve kullanıcı giriş ekranından önce yıl veritabanı seçmez.
/// </para>
/// </remarks>
public sealed class ConversationClear : Entity, IHardDeletable
{
    private ConversationClear()
    {
    }

    public ConversationClear(
        IdentityId userId,
        IdentityId counterpartId,
        bool isAnnouncementChannel,
        DateTimeOffset clearedAt)
    {
        UserId = userId;
        CounterpartId = counterpartId;
        IsAnnouncementChannel = isAnnouncementChannel;
        ClearedAt = clearedAt;
    }

    /// <summary>Temizlemeyi yapan kullanıcı (satırın sahibi).</summary>
    public IdentityId UserId { get; private set; } = default!;

    /// <summary>Karşı taraf (konuşmanın diğer ucundaki kullanıcı).</summary>
    public IdentityId CounterpartId { get; private set; } = default!;

    /// <summary>
    /// Duyuru kanalı mı? Duyuru kanalı ile birebir sohbet ayrı görünüm
    /// tercihleridir; birinin temizliği diğerini etkilemez.
    /// </summary>
    public bool IsAnnouncementChannel { get; private set; }

    /// <summary>Temizleme anı. Bu andan önceki mesajlar görünmez.</summary>
    public DateTimeOffset ClearedAt { get; private set; }

    /// <summary>Temizleme anını günceller (ikinci kez temizleme).</summary>
    public void SetClearedAt(DateTimeOffset clearedAt) => ClearedAt = clearedAt;
}
