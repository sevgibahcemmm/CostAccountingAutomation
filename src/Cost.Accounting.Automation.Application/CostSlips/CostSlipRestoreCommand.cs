using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:restore")]
public sealed record CostSlipRestoreCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class CostSlipRestoreCommandHandler(
    ICostSlipRepository costSlipRepository,
    IProductMovementRepository productMovementRepository) : IRequestHandler<CostSlipRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CostSlipRestoreCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        CostSlip? slip = await costSlipRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);

        if (slip is null)
        {
            return Result<string>.Failure("Maliyet pusulası bulunamadı.");
        }

        costSlipRepository.Restore(slip);

        var relatedMovements = await productMovementRepository
            .GetAllWithAuditIncludingDeleted()
            .Where(m => m.Entity.ReferenceNo == slip.SlipNumber && m.Entity.IsDeleted)
            .Select(m => m.Entity)
            .ToListAsync(cancellationToken);

        foreach (var movement in relatedMovements)
        {
            productMovementRepository.Restore(movement);
        }

        return Result<string>.Succeed("Maliyet pusulası ve ilişkili stok hareketleri başarıyla geri yüklendi.");
    }
}