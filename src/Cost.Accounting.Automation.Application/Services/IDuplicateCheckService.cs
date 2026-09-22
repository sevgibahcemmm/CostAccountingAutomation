using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Application.Services;

public interface IDuplicateCheckService
{
    Task<TEntity?> FindDuplicateAsync<TEntity>(
        string? duplicateKey,
        Guid? excludeId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
        where TEntity : Entity;
}