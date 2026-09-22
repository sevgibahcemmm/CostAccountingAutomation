using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CurrentAccountMovements;

[Permission("current_account_movement:restore")]
public sealed record CurrentAccountMovementRestoreCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class CurrentAccountMovementRestoreCommandHandler(
    ICurrentAccountMovementRepository currentAccountMovementRepository) : IRequestHandler<CurrentAccountMovementRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CurrentAccountMovementRestoreCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);
        CurrentAccountMovement? movement = await currentAccountMovementRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);

        if (movement is null)
        {
            return Result<string>.Failure("Cari hareket bulunamadı.");
        }

        if (string.IsNullOrEmpty(movement.DocumentNo) && CurrentAccountMovement.IsManualPaymentType(movement.MovementType))
        {
            string documentNo = await CurrentAccountMovementHelper.GenerateNextAsync(currentAccountMovementRepository, movement.MovementType, cancellationToken);

            movement.Update(
                date: movement.Date,
                movementType: movement.MovementType,
                documentNo: documentNo,
                debit: movement.Debit,
                credit: movement.Credit,
                description: movement.Description);
        }

        currentAccountMovementRepository.Restore(movement);
        return Result<string>.Succeed("Cari hareket başarıyla geri yüklendi.");
    }
}
