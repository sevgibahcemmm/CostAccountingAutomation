using GenericRepository;
using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Abstractions;

public interface IAuditableRepository<TEntity> : IRepository<TEntity>
    where TEntity : Entity
{
    /// <summary>
    /// Yalnızca silinmemiş kayıtlar. Global sorgu filtresi zaten
    /// <c>IsDeleted = false</c> uyguladığı için bu, <c>GetAll()</c> ile
    /// aynıdır; silinmiş kayıtları da görmek isteyen listeler için ayrı bir
    /// metot bulunur.
    /// </summary>
    IQueryable<TEntity> GetAllActive();

    /// <summary>
    /// Silinmiş kayıtlar dahil tüm kayıtlar. Yalnızca "silinmişler" sekmesi
    /// ve geri yükleme akışı kullanır.
    /// </summary>
    IQueryable<TEntity> GetAllIncludingDeleted();

    IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAudit();

    IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAuditIncludingDeleted();

    Task<TEntity?> GetByIdIncludingDeletedAsync(IdentityId id, CancellationToken cancellationToken = default);

    void UpdateRange(IEnumerable<TEntity> entities);

    // Soft delete
    void SoftDelete(TEntity entity);
    Task SoftDeleteAsync(IdentityId id, CancellationToken cancellationToken = default);
    void SoftDeleteRange(IEnumerable<TEntity> entities);
    Task SoftDeleteRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken = default);

    // Restore
    void Restore(TEntity entity);
    Task RestoreAsync(IdentityId id, CancellationToken cancellationToken = default);
    void RestoreRange(IEnumerable<TEntity> entities);
    Task RestoreRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken = default);

    // Hard delete (sadece IHardDeletable entity'ler için başarılı olur)
    void HardDelete(TEntity entity);
    Task HardDeleteAsync(IdentityId id, CancellationToken cancellationToken = default);
    void HardDeleteRange(IEnumerable<TEntity> entities);
    Task HardDeleteRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken = default);
}

public sealed class EntityWithAuditDto<TEntity>
    where TEntity : Entity
{
    public TEntity Entity { get; set; } = default!;
    public AuditUser CreatedUser { get; set; } = default!;
    public AuditUser? UpdatedUser { get; set; }
}