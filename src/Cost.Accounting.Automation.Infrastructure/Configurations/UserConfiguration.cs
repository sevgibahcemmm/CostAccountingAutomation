using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cost.Accounting.Automation.Domain.Photos;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;
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

        builder.HasMany(i => i.Photos)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
