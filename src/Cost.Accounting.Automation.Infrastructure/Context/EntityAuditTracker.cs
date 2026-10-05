using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Cost.Accounting.Automation.Infrastructure.Context;

public sealed class EntityAuditTracker(IClaimContext claimContext)
{
    private Guid? _seedAdminUserId;

    public void SetSeedAdminUserId(Guid? userId) => _seedAdminUserId = userId;

    public void ClearSeedAdminUserId() => _seedAdminUserId = null;

public void ApplyAudit(ChangeTracker changeTracker)
    {
        Guid? currentUserGuid = _seedAdminUserId ?? claimContext.GetUserIdOrDefault();
        IdentityId? currentUserId = currentUserGuid is null ? null : new IdentityId(currentUserGuid.Value);
        DateTimeOffset now = DateTimeOffset.Now;

        foreach (var entry in changeTracker.Entries<Entity>())
        {
            PromoteIfOwnedValueChanged(entry);

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(p => p.CreatedAt).CurrentValue = now;

                    if (currentUserId is not null)
                        entry.Property(p => p.CreatedBy).CurrentValue = currentUserId;

                    break;

                case EntityState.Modified:
                    ApplyModifiedAudit(entry, now, currentUserId);
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is not IHardDeletable)
                    {
                        throw new ArgumentException(
                            $"'{entry.Entity.GetType().Name}' için Db'den direkt silme işlemi yapamazsınız. " +
                            "Hard delete için entity IHardDeletable interface'ini implemente etmelidir.");
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Kök kaydın sahip olduğu değer nesnelerinden biri değişmişse kök kaydı
    /// <see cref="EntityState.Modified"/> yapar.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sorun şudur. <c>Customer.Description</c>, <c>Customer.Name</c>,
    /// <c>Customer.Address</c> gibi alanlar EF'de tablo-ağılmış (table-splitting)
    /// <i>owned</i> entity'lerdir. Yalnızca bunlardan biri değiştiğinde EF kök
    /// kaydın <b>kendi</b> skaler alanları değişmediği için kök girişi
    /// <see cref="EntityState.Unchanged"/> bırakır.
    /// </para>
    /// <para>
    /// Bunun iki sonucu vardır:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// Denetim alanları yazılmaz. Kayıt "güncellendi" ama <c>UpdatedAt</c> ve
    /// <c>UpdatedBy</c> boş kalır; dolayısıyla kim değiştirdi bilgisi kaybolur.
    /// </item>
    /// <item>
    /// Eşzamanlılık belirteci çakışmayı yine de <i>tespit</i> eder (EF değişen
    /// owned kaydın UPDATE'ini gönderir), ama çakışma bize kimin değiştirdiğini
    /// söyleyemez çünkü o bilgi hiç yazılmamıştır.
    /// </item>
    /// </list>
    /// <para>
    /// Bu yüzden kök kayıt bilinçli olarak <c>Modified</c> durumuna alınır. Bedeli,
    /// UPDATE'in tüm sütunları kapsamasıdır; ayrı tabloda tutulan satırlar
    /// (örneğin <c>InvoiceLine</c>) bu etkiden etkilenmez çünkü onlar kendi
    /// kayıtlarıyla izlenir.
    /// </para>
    /// </remarks>
    private static void PromoteIfOwnedValueChanged(EntityEntry<Entity> entry)
    {
        if (entry.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
        {
            return;
        }

        foreach (var reference in entry.References)
        {
            // TargetEntry, referans verilen kaydın kendi girişidir. Owned
            // değer nesnelerinin durumu değiştiyse kök kayıt da değişmiştir.
            if (reference.TargetEntry is not { State: EntityState.Added or EntityState.Modified or EntityState.Deleted })
            {
                continue;
            }

            entry.State = EntityState.Modified;

            return;
        }
    }

    private static void ApplyModifiedAudit(EntityEntry<Entity> entry, DateTimeOffset now, IdentityId? currentUserId)
    {
        bool isDeletedTouched = entry.Property(p => p.IsDeleted).IsModified;
        bool isDeletedNow = entry.Property(p => p.IsDeleted).CurrentValue;

        if (isDeletedTouched && isDeletedNow)
        {
            entry.Property(p => p.DeletedAt).CurrentValue = now;
            entry.Property(p => p.DeletedBy).CurrentValue = currentUserId;
        }
        else if (isDeletedTouched && !isDeletedNow)
        {
            entry.Property(p => p.DeletedAt).CurrentValue = null;
            entry.Property(p => p.DeletedBy).CurrentValue = null;
            entry.Property(p => p.UpdatedAt).CurrentValue = now;
            entry.Property(p => p.UpdatedBy).CurrentValue = currentUserId;
        }
        else
        {
            entry.Property(p => p.UpdatedAt).CurrentValue = now;
            entry.Property(p => p.UpdatedBy).CurrentValue = currentUserId;
        }
    }
}
