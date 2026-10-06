using Cost.Accounting.Automation.Domain.Presence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations.Master;

/// <summary>
/// Çevrimiçi kullanıcı tablosunun master veritabanındaki karşılığı.
/// </summary>
/// <remarks>
/// <para>
/// Başlık <see cref="UserPresence.UserId"/>'dir: bir kullanıcının satırı
/// tektir, böylece kalp atışı "varsa güncelle" mantığıyla güvenli olur.
/// </para>
/// <para>
/// Tablo <see cref="UserPresence.OnlineWindow"/> kadar süredir güncellenmemiş
/// satırları sık sık okur (<see cref="LastHeartbeatAt"/> üzerinde indeks vardır),
/// çünkü en sık çalışan sorgu budur.
/// </para>
/// </remarks>
internal sealed class UserPresenceConfiguration : IEntityTypeConfiguration<UserPresence>
{
    public void Configure(EntityTypeBuilder<UserPresence> builder)
    {
        builder.ToTable("UserPresences");
        builder.HasKey(p => p.UserId);

        // Makine adı kısa bir metindir. Sadece HasMaxLength vermek projedeki
        // varsayılan "string -> nvarchar(MAX)" kuralı yüzünden sütunu yine
        // sınırsız yapar; bu yüzden tip açıkça belirtilir.
        builder.Property(p => p.MachineName)
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");

        builder.HasIndex(p => p.LastHeartbeatAt)
            .HasDatabaseName("IX_UserPresences_LastHeartbeatAt");

        // Kullanıcı silinirse (soft delete) çevrimiçi kaydı da silinmez: kayıt
        // zaten zaman damgası taşıdığı için zararsızdır ve "kullanıcı yoktu"
        // hatasını önler. Bu yüzden FK tanımlanmaz; MasterDbContext'te
        // yalnızca Users ve Messages arasında kısıt vardır.
    }
}