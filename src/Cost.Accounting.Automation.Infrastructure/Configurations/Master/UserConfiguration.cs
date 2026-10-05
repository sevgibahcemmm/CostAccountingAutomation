using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Infrastructure.Configurations.Master;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(i => i.Id);
        builder.OwnsOne(i => i.FirstName);
        builder.OwnsOne(i => i.LastName);
        builder.OwnsOne(i => i.FullName);
        builder.OwnsOne(i => i.Email);
        builder.OwnsOne(i => i.UserName);
        builder.OwnsOne(i => i.Password);
        builder.OwnsOne(i => i.ForgotPasswordCode);
        builder.OwnsOne(i => i.ForgotPasswordDate);
        builder.OwnsOne(i => i.IsForgotPasswordCompleted);
        builder.OwnsOne(i => i.TRIdentityNumber, tr =>
        {
            tr.Property(v => v.Value).HasColumnName("TcNo_Value");
        });

builder.Property(i => i.AvatarPath).HasMaxLength(500);

        // Sicil numarası mesajlaşmada alıcı bulmak için kullanılır. Uzunluğu
        // sınırlıdır ve üzerinde indeks vardır; arama bu alan üzerinden yapılır.
        builder.Property(i => i.RegistryNumber).HasMaxLength(50);

        // Benzersizlik uygulama katmanında (UserCreateCommand) denetlenir ve
        // DuplicateKey üzerinden indekslenir. Burada ikinci bir benzersizlik
        // kısıtı eklenmez: numarası olmayan kullanıcılar için SQL Server birden çok
        // NULL'a izin verir ama "sicil numarası girilmişse benzersizdir" kuralı
        // yalnızca uygulama tarafında tutulur (Employee kaydıyla aynı yaklaşım).
    }
}
