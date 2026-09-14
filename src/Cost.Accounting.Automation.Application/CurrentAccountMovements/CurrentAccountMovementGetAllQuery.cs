using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CurrentAccountMovements;

[Permission("current_account_movement:view")]
public sealed record CurrentAccountMovementGetAllQuery(
    CurrentAccountType? AccountType = null,
    Guid? CustomerId = null,
    Guid? SupplierId = null,
    bool OnlyDeleted = false,
    CurrentAccountMovementType[]? MovementTypes = null) : IRequest<IQueryable<CurrentAccountMovementDto>>
{
    public CurrentAccountMovementGetAllQuery() : this(null, null, null, false, null) { }
}

internal sealed class CurrentAccountMovementGetAllQueryHandler(
    ICurrentAccountMovementRepository currentAccountMovementRepository) : IRequestHandler<CurrentAccountMovementGetAllQuery, IQueryable<CurrentAccountMovementDto>>
{
    public Task<IQueryable<CurrentAccountMovementDto>> Handle(CurrentAccountMovementGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<CurrentAccountMovement>> source = request.OnlyDeleted
            ? currentAccountMovementRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : currentAccountMovementRepository.GetAllWithAudit();

        if (request.AccountType.HasValue)
        {
            source = source.Where(i => i.Entity.CurrentAccountType == request.AccountType.Value);
        }

        if (request.MovementTypes is { Length: > 0 })
        {
            source = source.Where(i => request.MovementTypes!.Contains(i.Entity.MovementType));
        }

        if (request.CustomerId.HasValue)
        {
            IdentityId custId = new(request.CustomerId.Value);
            source = source.Where(i => i.Entity.CustomerId == custId);
        }

        if (request.SupplierId.HasValue)
        {
            IdentityId supId = new(request.SupplierId.Value);
            source = source.Where(i => i.Entity.SupplierId == supId);
        }

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}
