using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class CurrentAccountMovementRepository : AuditableRepository<CurrentAccountMovement, ApplicationDbContext>, ICurrentAccountMovementRepository
{
    public CurrentAccountMovementRepository(ApplicationDbContext context, MasterDbContext masterContext) : base(context, masterContext)
    {
    }
}
