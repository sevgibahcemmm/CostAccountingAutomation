using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cost.Accounting.Automation.Domain.Roles;

namespace Cost.Accounting.Automation.Infrastructure.Configurations;
internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);
        builder.OwnsOne(i => i.Name);
        builder.OwnsMany(i => i.Permissions);
    }
}
