using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:delete")]
public sealed record CostSlipDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class CostSlipDeleteCommandHandler(
    ICostSlipRepository costSlipRepository,
    IProductMovementRepository productMovementRepository) : IRequestHandler<CostSlipDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CostSlipDeleteCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        CostSlip? slip = await costSlipRepository.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (slip is null)
        {
            return Result<string>.Failure("Maliyet pusulası bulunamadı.");
        }

        costSlipRepository.SoftDelete(slip);

        var relatedMovements = await productMovementRepository
            .Where(m => m.ReferenceNo == slip.SlipNumber)
            .ToListAsync(cancellationToken);

        foreach (var movement in relatedMovements)
        {
            productMovementRepository.SoftDelete(movement);
        }

        return Result<string>.Succeed("Maliyet pusulası ve ilişkili stok hareketleri başarıyla silindi.");
    }
}