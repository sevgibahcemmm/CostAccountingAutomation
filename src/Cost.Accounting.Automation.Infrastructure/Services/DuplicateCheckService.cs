using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Services;

internal sealed class DuplicateCheckService(
    ApplicationDbContext context,
    MasterDbContext masterContext) : IDuplicateCheckService
{
    public Task<TEntity?> FindDuplicateAsync<TEntity>(
        string? duplicateKey,
        Guid? excludeId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
        where TEntity : Entity
        => FindDuplicateInContextAsync<TEntity>(
            context,
            duplicateKey,
            excludeId,
            includeDeleted,
            cancellationToken);

    public Task<TEntity?> FindMasterDuplicateAsync<TEntity>(
        string? duplicateKey,
        Guid? excludeId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
        where TEntity : Entity
        => FindDuplicateInContextAsync<TEntity>(
            masterContext,
            duplicateKey,
            excludeId,
            includeDeleted,
            cancellationToken);

    private static async Task<TEntity?> FindDuplicateInContextAsync<TEntity>(
        DbContext targetContext,
        string? duplicateKey,
        Guid? excludeId,
        bool includeDeleted,
        CancellationToken cancellationToken)
        where TEntity : Entity
    {
        if (string.IsNullOrWhiteSpace(duplicateKey))
        {
            return null;
        }

        IQueryable<TEntity> query = includeDeleted
            ? targetContext.Set<TEntity>().IgnoreQueryFilters()
            : targetContext.Set<TEntity>();

        if (excludeId is not null)
        {
            IdentityId id = new(excludeId.Value);
            query = query.Where(e => e.Id != id);
        }

        return await query.FirstOrDefaultAsync(e => e.DuplicateKey == duplicateKey, cancellationToken);
    }
}