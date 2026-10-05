using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Infrastructure.Configurations.Master;

/// <summary>
/// Mesaj tablosu; master (merkezi) veritabanında yer alır.
/// </summary>
/// <remarks>
/// <para>
/// Neden master: mesajlaşma kullanıcılar arasındadır ve kullanıcı kayıtları da
/// master'da tutulur. Kullanıcı giriş ekranından önce yıl veritabanı seçmediği
/// için mesajlar yıl veritabanında olamaz.
/// </para>
/// <para>
/// İndeksler gerçekten kullanılan sorguları karşılar:
/// </para>
/// <list type="bullet">
/// <item>
/// <c>(RecipientId, ReadState)</c> — gelen kutusu ve okunmamış sayacı. En sık sorgu.
/// </item>
/// <item>
/// <c>(SenderId, RecipientId, CreatedAt)</c> — konuşma geçmişi. Karşılıklı yön
/// için iki ayrı indeks gerekir; ikisi de tanımlıdır.
/// </item>
/// </list>
/// </remarks>
internal sealed class MessageConfiguration : IEntityTypeConfiguration<UserMessage>
{
    public void Configure(EntityTypeBuilder<UserMessage> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);

        builder.OwnsOne(m => m.Body, body =>
        {
            body.Property(b => b.Value).HasColumnName("Body_Value");
        });

        builder.OwnsOne(m => m.Subject, subject =>
        {
            subject.Property(s => s.Value).HasColumnName("Subject_Value");
        });

        builder.OwnsOne(m => m.ReadState, read =>
        {
            read.Property(r => r.Value).HasColumnName("ReadState_Value");
        });

        // Gelen kutusu her zaman "bana gelenler" ile baslar; okunmus/okunmamis
        // filtresi bu daraltilmis kume uzerinde uygulanir. Sahip olunan bir
        // navigasyon indeksin parcasi olamaz, bu yuzden ReadState indekse girmez.
        builder.HasIndex(m => m.RecipientId)
            .HasDatabaseName("IX_Messages_RecipientId");

        builder.HasIndex(m => new { m.SenderId, m.RecipientId, m.CreatedAt })
            .HasDatabaseName("IX_Messages_Sender_Recipient_CreatedAt");

        builder.HasIndex(m => new { m.RecipientId, m.SenderId, m.CreatedAt })
            .HasDatabaseName("IX_Messages_Recipient_Sender_CreatedAt");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
