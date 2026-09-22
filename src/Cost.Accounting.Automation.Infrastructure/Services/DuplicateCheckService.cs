using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Services;

internal sealed class DuplicateCheckService(ApplicationDbContext context) : IDuplicateCheckService
{
    public async Task<TEntity?> FindDuplicateAsync<TEntity>(
        string? duplicateKey,
        Guid? excludeId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
        where TEntity : Entity
    {
        if (string.IsNullOrWhiteSpace(duplicateKey))
        {
            return null;
        }

        IQueryable<TEntity> query = includeDeleted
            ? context.Set<TEntity>().IgnoreQueryFilters()
            : context.Set<TEntity>();

        if (excludeId is not null)
        {
            IdentityId id = new(excludeId.Value);
            query = query.Where(e => e.Id != id);
        }

        return await query.FirstOrDefaultAsync(e => e.DuplicateKey == duplicateKey, cancellationToken);
    }
}