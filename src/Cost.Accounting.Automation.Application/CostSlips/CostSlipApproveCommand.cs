using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:approve")]
public sealed record CostSlipApproveCommand(
    Guid Id,
    StockCostingMethod CostingMethod = StockCostingMethod.Fifo) : IRequest<Result<string>>;

internal sealed class CostSlipApproveCommandHandler(
    ICostSlipRepository costSlipRepository,
    IProductMovementRepository productMovementRepository,
    IProductRepository productRepository,
    IChartOfAccountLedgerPoster ledgerPoster) : IRequestHandler<CostSlipApproveCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CostSlipApproveCommand request, CancellationToken cancellationToken)
    {
        IdentityId id = new(request.Id);

        CostSlip? slip = await costSlipRepository
            .WhereWithTracking(s => s.Id == id)
            .Include(s => s.CostSlipItems)
            .FirstOrDefaultAsync(cancellationToken);

        if (slip is null)
        {
            return Result<string>.Failure("Maliyet pusulası bulunamadı.");
        }

        if (slip.Status == CostSlipStatus.Approved)
        {
            return Result<string>.Failure("Bu maliyet pusulası zaten onaylanmış durumda.");
        }

        bool hasMovements = await productMovementRepository
            .AnyAsync(m => m.ReferenceNo == slip.SlipNumber, cancellationToken);

        if (hasMovements)
        {
            return Result<string>.Failure("Bu maliyet pusulası için stok hareketleri zaten kayıtlı.");
        }

        Result<string> stockResult = await CostSlipStockHelper.ApplyStockEffectsAsync(
            slip,
            request.CostingMethod,
            productMovementRepository,
            ledgerPoster,
            productRepository,
            cancellationToken);

        if (!stockResult.IsSuccessful)
        {
            return stockResult;
        }

        slip.Approve();

        return Result<string>.Succeed("Maliyet pusulası onaylandı; stok hareketleri oluşturuldu.");
    }
}