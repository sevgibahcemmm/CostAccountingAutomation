using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
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
    ICurrentAccountMovementRepository currentAccountMovementRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<CurrentAccountMovementGetAllQuery, IQueryable<CurrentAccountMovementDto>>
{
    public async Task<IQueryable<CurrentAccountMovementDto>> Handle(
        CurrentAccountMovementGetAllQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<CurrentAccountMovement> source = request.OnlyDeleted
            ? currentAccountMovementRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : currentAccountMovementRepository.GetAllActive();

        if (request.AccountType is CurrentAccountType accountType)
        {
            source = source.Where(i => i.CurrentAccountType == accountType);
        }

        if (request.MovementTypes is { Length: > 0 } movementTypes)
        {
            source = source.Where(i => movementTypes.Contains(i.MovementType));
        }

        if (request.CustomerId is Guid customerId)
        {
            IdentityId id = new(customerId);
            source = source.Where(i => i.CustomerId == id);
        }

        if (request.SupplierId is Guid supplierId)
        {
            IdentityId id = new(supplierId);
            source = source.Where(i => i.SupplierId == id);
        }

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
