using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
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
    IProductMovementRepository productMovementRepository,
    IChartOfAccountLedgerRepository ledgerRepository) : IRequestHandler<CostSlipDeleteCommand, Result<string>>
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

            List<ChartOfAccountLedger> ledgerEntries =
                await ledgerRepository.GetBySourceAsync("MaliyetTuketimi", movement.Id.Value, cancellationToken);

            if (ledgerEntries.Count > 0)
            {
                ledgerRepository.SoftDeleteRange(ledgerEntries);
            }
        }

        return Result<string>.Succeed("Maliyet pusulası ve ilişkili stok hareketleri başarıyla silindi.");
    }
}