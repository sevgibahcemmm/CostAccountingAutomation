using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Application.CurrentAccountMovements;

internal static class CurrentAccountMovementHelper
{
    public static string PrefixFor(CurrentAccountMovementType type)
        => type == CurrentAccountMovementType.Collection ? "TAH" : "ODM";

    public static async Task<string> GenerateNextAsync(
        ICurrentAccountMovementRepository repository,
        CurrentAccountMovementType type,
        CancellationToken cancellationToken)
    {
        int count = await repository
            .GetAllWithAuditIncludingDeleted()
            .CountAsync(m => m.Entity.MovementType == type, cancellationToken);

        return $"{PrefixFor(type)}-{DateTime.Now:yyyy}-{count + 1:00000}";
    }
}