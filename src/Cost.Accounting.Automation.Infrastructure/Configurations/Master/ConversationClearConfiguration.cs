using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cost.Accounting.Automation.Infrastructure.Configurations.Master;

/// <summary>
/// Kullanıcı başına konuşma temizleme (görünüm gizleme) tablosu; master'da tutulur.
/// </summary>
/// <remarks>
/// <para>
/// Her satır "şu kullanıcı bu konuşmayı kendi tarafından temizledi" bilgisidir;
/// mesajlara dokunmaz. Özindek (kullanıcı + karşı taraf + kanal) satır sayısını
/// kullanıcı başına tek satırda sınırlar.
/// </para>
/// <para>
/// Bu tablo mesajlar gibi master'dadır: kimlikler orada yaşar ve temizleme
/// durumu yıl veritabanı seçilmeden önce de okunabilir olmalıdır.
/// </para>
/// </remarks>
internal sealed class ConversationClearConfiguration : IEntityTypeConfiguration<ConversationClear>
{
    public void Configure(EntityTypeBuilder<ConversationClear> builder)
    {
        builder.ToTable("ConversationClears");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId);
        builder.Property(c => c.CounterpartId);
        builder.Property(c => c.IsAnnouncementChannel);
        builder.Property(c => c.ClearedAt);

        builder.HasIndex(c => new { c.UserId, c.CounterpartId, c.IsAnnouncementChannel })
            .IsUnique()
            .HasDatabaseName("IX_ConversationClears_User_Counterpart_Channel");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.CounterpartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
