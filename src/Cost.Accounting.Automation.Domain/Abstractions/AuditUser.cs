using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Users;

namespace Cost.Accounting.Automation.Domain.Abstractions;

/// <summary>
/// Lightweight audit user representation - only what's needed for display.
/// Replaces full <see cref="User"/> in <see cref="EntityWithAuditDto{TEntity}"/>.
/// </summary>
public sealed class AuditUser
{
    public required IdentityId Id { get; init; }
    public required FullName FullName { get; init; }

    public static AuditUser FromUser(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName
    };
}