using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Abstractions;

internal class AuditableRepository<TEntity, TContext> : Repository<TEntity, TContext>, IAuditableRepository<TEntity>
    where TEntity : Entity
    where TContext : DbContext
{
    private readonly TContext _yearContext;
    private readonly MasterDbContext _masterContext;

    public AuditableRepository(TContext yearContext, MasterDbContext masterContext) : base(yearContext)
    {
        _yearContext = yearContext;
        _masterContext = masterContext;
    }

    protected TContext Context => _yearContext;

    public IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAudit()
        => BuildGetAllWithAudit(includeDeleted: false);

    public IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAuditIncludingDeleted()
        => BuildGetAllWithAudit(includeDeleted: true);

    /// <summary>
    /// Liste (denetimli) sorgusundan önce uygulanacak Include zinciri.
    ///
    /// <see cref="BuildGetAllWithAudit"/> sonucu belleğe materyalize edilip
    /// LINQ-to-Objects olarak döner; bu nedenle veritabanında <c>Include</c>
    /// uygulanmazsa navigasyon özellikleri (birim, KDV, depo, kategori, görsel
    /// vb.) null gelir ve DTO eşlemesi sessizce boş sütunlar üretir. Alt sınıflar
    /// ihtiyaç duydukları navigasyonları burada bildirir.
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyDetailIncludes(IQueryable<TEntity> query) => query;

    private IQueryable<EntityWithAuditDto<TEntity>> BuildGetAllWithAudit(bool includeDeleted)
    {
        var entities = includeDeleted
            ? _yearContext.Set<TEntity>().IgnoreQueryFilters().AsNoTrackingWithIdentityResolution()
            : _yearContext.Set<TEntity>().AsNoTrackingWithIdentityResolution();

        entities = ApplyDetailIncludes(entities);

        var entityList = entities.ToList();

        if (entityList.Count == 0)
        {
            return new MaterializedAsyncQueryable<EntityWithAuditDto<TEntity>>(Array.Empty<EntityWithAuditDto<TEntity>>());
        }

        var userIds = new HashSet<Guid>();
        foreach (var entity in entityList)
        {
            userIds.Add(entity.CreatedBy.Value);
            if (entity.UpdatedBy is IdentityId ub)
            {
                userIds.Add(ub.Value);
            }
        }

        // Id, value converter ile Guid'a eşlendiği için filtre koşulunda
        // u.Id.Value yazılamaz (EF çeviremez); aynı CLR tipinde karşılaştırılır.
        List<IdentityId> userIdList = userIds.Select(id => new IdentityId(id)).ToList();

        var userFullNames = _masterContext.Set<User>()
            .Where(u => userIdList.Contains(u.Id))
            .ToDictionary(u => u.Id.Value, u => u.FullName);

        var dtos = new List<EntityWithAuditDto<TEntity>>(entityList.Count);
        foreach (var entity in entityList)
        {
            var createdFullName = userFullNames.GetValueOrDefault(entity.CreatedBy.Value, new FullName("(silinmis kullanici)"));
            var updatedFullName = entity.UpdatedBy is IdentityId ub2 && userFullNames.TryGetValue(ub2.Value, out var ufn)
                ? ufn
                : new FullName("(silinmis kullanici)");

            dtos.Add(new EntityWithAuditDto<TEntity>
            {
                Entity = entity,
                CreatedUser = new AuditUser { Id = entity.CreatedBy, FullName = createdFullName },
                UpdatedUser = entity.UpdatedBy is IdentityId ub3
                    ? new AuditUser { Id = ub3, FullName = userFullNames.GetValueOrDefault(ub3.Value, new FullName("(silinmis kullanici)")) }
                    : null
            });
        }

        return new MaterializedAsyncQueryable<EntityWithAuditDto<TEntity>>(dtos);
    }

    public Task<TEntity?> GetByIdIncludingDeletedAsync(IdentityId id, CancellationToken cancellationToken = default)
        => _yearContext.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    Task IAuditableRepository<TEntity>.SoftDeleteRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken)
        => SoftDeleteRangeAsync(ids, cancellationToken);

    void IAuditableRepository<TEntity>.UpdateRange(IEnumerable<TEntity> entities)
        => _yearContext.Set<TEntity>().UpdateRange(entities);

    public void SoftDelete(TEntity entity) => entity.Delete();

    public async Task SoftDeleteAsync(IdentityId id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await _yearContext.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} bulunamadi: {id.Value}");
        entity.Delete();
    }

    public void SoftDeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (TEntity entity in entities)
            entity.Delete();
    }

    public async Task SoftDeleteRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> idSet = ids.ToHashSet();
        List<TEntity> entities = await _yearContext.Set<TEntity>()
            .Where(e => idSet.Contains(e.Id))
            .ToListAsync(cancellationToken);
        foreach (TEntity entity in entities)
            entity.Delete();
    }

    public void Restore(TEntity entity) => entity.Restore();

    public async Task RestoreAsync(IdentityId id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await _yearContext.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} bulunamadi: {id.Value}");
        entity.Restore();
    }

    public void RestoreRange(IEnumerable<TEntity> entities)
    {
        foreach (TEntity entity in entities)
            entity.Restore();
    }

    public async Task RestoreRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> idSet = ids.ToHashSet();
        List<TEntity> entities = await _yearContext.Set<TEntity>()
            .IgnoreQueryFilters()
            .Where(e => idSet.Contains(e.Id))
            .ToListAsync(cancellationToken);
        foreach (TEntity entity in entities)
            entity.Restore();
    }

    public void HardDelete(TEntity entity) => _yearContext.Set<TEntity>().Remove(entity);

    public async Task HardDeleteAsync(IdentityId id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await _yearContext.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} bulunamadi: {id.Value}");
        _yearContext.Set<TEntity>().Remove(entity);
    }

    public void HardDeleteRange(IEnumerable<TEntity> entities) => _yearContext.Set<TEntity>().RemoveRange(entities);

    public async Task HardDeleteRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> idSet = ids.ToHashSet();
        List<TEntity> entities = await _yearContext.Set<TEntity>()
            .IgnoreQueryFilters()
            .Where(e => idSet.Contains(e.Id))
            .ToListAsync(cancellationToken);
        _yearContext.Set<TEntity>().RemoveRange(entities);
    }
}