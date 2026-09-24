using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Infrastructure.Abstractions;

internal class AuditableRepository<TEntity, TContext> : Repository<TEntity, TContext>, IAuditableRepository<TEntity>
    where TEntity : Entity
    where TContext : DbContext
{
private readonly TContext _context;

    protected TContext Context => _context;

    public AuditableRepository(TContext context) : base(context)
    {
        _context = context;
    }

public IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAudit()
        => BuildGetAllWithAudit(includeDeleted: false);

    public IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAuditIncludingDeleted()
        => BuildGetAllWithAudit(includeDeleted: true);

private IQueryable<EntityWithAuditDto<TEntity>> BuildGetAllWithAudit(bool includeDeleted)
    {
        var entities = includeDeleted
            ? _context.Set<TEntity>().IgnoreQueryFilters().AsNoTrackingWithIdentityResolution()
            : _context.Set<TEntity>().AsNoTrackingWithIdentityResolution();
        var users = _context.Set<User>().AsNoTracking();

        var res = entities
          .Join(users, m => m.CreatedBy, m => m.Id, (b, user) =>
                  new { entity = b, createdUser = user })
          .GroupJoin(users, m => m.entity.UpdatedBy, m => m.Id, (b, user) =>
                  new { b.entity, b.createdUser, updatedUser = user })
          .SelectMany(s => s.updatedUser.DefaultIfEmpty(),
              (x, updatedUser) => new EntityWithAuditDto<TEntity>
              {
                  Entity = x.entity,
                  CreatedUser = x.createdUser,
                  UpdatedUser = updatedUser
              });

        return res;
    }

    public Task<TEntity?> GetByIdIncludingDeletedAsync(IdentityId id, CancellationToken cancellationToken = default)
        => _context.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    Task IAuditableRepository<TEntity>.SoftDeleteRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken)
        => SoftDeleteRangeAsync(ids, cancellationToken);

    void IAuditableRepository<TEntity>.UpdateRange(IEnumerable<TEntity> entities)
        => _context.Set<TEntity>().UpdateRange(entities);

    // ---------------- Soft delete ----------------

    public void SoftDelete(TEntity entity)
        => entity.Delete();

    public async Task SoftDeleteAsync(IdentityId id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await _context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} bulunamadı: {id.Value}");

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

        List<TEntity> entities = await _context.Set<TEntity>()
            .Where(e => idSet.Contains(e.Id))
            .ToListAsync(cancellationToken);

        foreach (TEntity entity in entities)
            entity.Delete();
    }

    // ---------------- Restore ----------------

    public void Restore(TEntity entity)
        => entity.Restore();

    public async Task RestoreAsync(IdentityId id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await _context.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} bulunamadı: {id.Value}");

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

        List<TEntity> entities = await _context.Set<TEntity>()
            .IgnoreQueryFilters()
            .Where(e => idSet.Contains(e.Id))
            .ToListAsync(cancellationToken);

        foreach (TEntity entity in entities)
            entity.Restore();
    }

    // ---------------- Hard delete ----------------

    public void HardDelete(TEntity entity)
        => _context.Set<TEntity>().Remove(entity);

    public async Task HardDeleteAsync(IdentityId id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await _context.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} bulunamadı: {id.Value}");

        _context.Set<TEntity>().Remove(entity);
    }

    public void HardDeleteRange(IEnumerable<TEntity> entities)
        => _context.Set<TEntity>().RemoveRange(entities);

public async Task HardDeleteRangeAsync(IEnumerable<IdentityId> ids, CancellationToken cancellationToken = default)
    {
        HashSet<IdentityId> idSet = ids.ToHashSet();

        List<TEntity> entities = await _context.Set<TEntity>()
            .IgnoreQueryFilters()
            .Where(e => idSet.Contains(e.Id))
            .ToListAsync(cancellationToken);

        _context.Set<TEntity>().RemoveRange(entities);
    }
}