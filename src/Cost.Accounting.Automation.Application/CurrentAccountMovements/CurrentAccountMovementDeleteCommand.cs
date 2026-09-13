using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CurrentAccountMovements;

[Permission("current_account_movement:delete")]
public sealed record CurrentAccountMovementDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class CurrentAccountMovementDeleteCommandHandler(
    ICurrentAccountMovementRepository currentAccountMovementRepository) : IRequestHandler<CurrentAccountMovementDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CurrentAccountMovementDeleteCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);
        CurrentAccountMovement? movement = await currentAccountMovementRepository.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (movement is null)
        {
            return Result<string>.Failure("Cari hareket bulunamadı.");
        }

        currentAccountMovementRepository.SoftDelete(movement);
        return Result<string>.Succeed("Cari hareket başarıyla silindi.");
    }
}
