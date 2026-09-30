using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Abstractions;

/// <summary>
/// Master DB repository - users are in the same DB, so audit join works directly.
/// </summary>
internal class MasterAuditableRepository<TEntity> : AuditableRepository<TEntity, MasterDbContext>, IAuditableRepository<TEntity>
    where TEntity : Entity
{
    public MasterAuditableRepository(MasterDbContext context) : base(context, context)
    {
    }

// Override to use the original same-context join (users are in master DB)
    public new IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAudit()
        => BuildGetAllWithAudit(includeDeleted: false);

    public new IQueryable<EntityWithAuditDto<TEntity>> GetAllWithAuditIncludingDeleted()
        => BuildGetAllWithAudit(includeDeleted: true);

    private IQueryable<EntityWithAuditDto<TEntity>> BuildGetAllWithAudit(bool includeDeleted)
    {
        var entities = this.Context.Set<TEntity>().AsNoTrackingWithIdentityResolution();

        if (!includeDeleted)
        {
            entities = entities.Where(e => !e.IsDeleted);
        }

        var users = this.Context.Set<User>().AsNoTracking();

        var res = entities
            .Join(users, e => e.CreatedBy, u => u.Id, (e, u) => new { entity = e, createdUser = u })
            .GroupJoin(this.Context.Set<User>().AsNoTracking(), x => x.entity.UpdatedBy, u => u.Id, (x, u) => new { x.entity, x.createdUser, updatedUser = u })
            .SelectMany(x => x.updatedUser.DefaultIfEmpty(),
                (x, updatedUser) => new EntityWithAuditDto<TEntity>
                {
                    Entity = x.entity,
                    CreatedUser = AuditUser.FromUser(x.createdUser),
                    UpdatedUser = updatedUser != null ? AuditUser.FromUser(updatedUser) : null
                });

        return res;
    }
}