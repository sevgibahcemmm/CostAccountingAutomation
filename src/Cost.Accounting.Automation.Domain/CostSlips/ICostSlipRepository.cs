using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.CostSlips;

public interface ICostSlipRepository : IAuditableRepository<CostSlip>
{
    Task<CostSlip?> GetWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default);
}